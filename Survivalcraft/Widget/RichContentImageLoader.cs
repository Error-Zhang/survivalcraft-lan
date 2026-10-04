using System.Globalization;
using System.Net;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    /// <summary>
    /// 富内容图片异步加载管线，HtmlWidget/HtmlToWidgetsBuilder（HTML）与 Markdown 系 widget 共用：
    /// 占位 frame（底框 + 隐藏 image + 状态字）→ 后台下载并解码（ImageSharp 纯 CPU，不碰 GL）→
    /// Dispatcher 回主线程做 GL 上传换 Subtexture。
    /// 另持有已加载图片纹理的生命周期（引擎 Dialog/Widget 树无 Dispose 级联）：
    /// 收集每张纹理统一释放，避免每次开详情累积一个不可回收句柄。
    /// </summary>
    sealed class RichContentImageLoader {
        static readonly CommunityHttpDownloader s_imageDownloader = new(HttpClientProvider.Community);

        // 占位 frame 高度（加载中/失败态）；成功后按图片适配
        const float PlaceholderHeight = 220f;

        // 相对 URL 解析基准；null = 仅接受绝对 URL
        public string BaseUrl;

        // Texture2D 持有 GL 纹理句柄（GraphicsResource:IDisposable）。
        // 收集每张加载的纹理，重建内容或对话框移除时统一释放。
        readonly List<Texture2D> m_loadedTextures = [];

        // URL → 纹理：内容重建（自动翻译等）时复用已下载图片，避免同一张图下载两次。
        // 纹理对象与 m_loadedTextures 同源（后者负责释放），ReleaseTextures 时一并清空。
        readonly Dictionary<string, Texture2D> m_imageTextureCache = new(StringComparer.OrdinalIgnoreCase);

        // 代际计数：ReleaseTextures 递增。图片下载/解码是异步的，回调捕获发起时的代际，
        // 回主线程后代际已变（宿主已释放/重建内容）即就地释放新纹理，既防 GL 句柄泄漏，
        // 也防旧内容的晚到回调动新内容的 frame；ReleaseTextures 后 loader 可继续复用（Markdown 重建场景）。
        int m_generation;

        // 已换上真实纹理的 frame：宿主（MarkdownWidget）测量期只回填未完成 frame 的宽度，
        // 已完成的 frame 宽=图宽，不能被覆盖（HorizontalAlignment=Center 居中）
        readonly HashSet<CanvasWidget> m_appliedFrames = [];

        // 纹理就绪但宿主宽未解析（frame.Size.X<=0，跟随父容器的宿主在测量期才回填）的 frame。
        // 挂起到宿主调 ResolvePendingWidths()（测量期回填之后）再适配上屏；
        // 不挂起就会按兜底宽适配，窄容器/列表项内溢出
        readonly List<(CanvasWidget Frame, RectangleWidget Background, RectangleWidget Image, LabelWidget Status, string Alt, string Url, float MaxHeight)> m_pendingFrames = [];

        // frame 是否已换上真实纹理（见 m_appliedFrames）
        public bool IsFrameApplied(CanvasWidget frame) => m_appliedFrames.Contains(frame);

        // 创建图片块 widget 并触发异步加载。占位宽 <=0（宽度跟随父容器）时由宿主在测量期回填 frame.Size.X。
        // 调用方负责把它挂进内容面板（图片块上方通常需要 spacer，由调用方按各自版式控制）。
        public CanvasWidget CreateImageBlock(string url, string alt, float placeholderWidth, float maxHeight) {
            url = ResolveUrl(url);
            alt = string.IsNullOrWhiteSpace(alt) ? LanguageControl.Get("PostDetailsContentBuilder", "Image") : alt.Trim();
            CanvasWidget frame = new() { Size = new Vector2(placeholderWidth, PlaceholderHeight), HorizontalAlignment = WidgetAlignment.Center };
            RectangleWidget background = new() {
                Size = new Vector2(placeholderWidth, PlaceholderHeight),
                FillColor = new Color(8, 8, 8, 220),
                OutlineColor = new Color(120, 120, 120, 180),
                OutlineThickness = 1f
            };
            frame.Children.Add(background);
            RectangleWidget image = new() {
                Size = new Vector2(placeholderWidth, PlaceholderHeight), FillColor = Color.White, OutlineThickness = 0f, TextureLinearFilter = true, IsVisible = false
            };
            frame.Children.Add(image);
            LabelWidget status = new() {
                Text = string.IsNullOrWhiteSpace(url)
                    ? FailedToLoadText(alt)
                    : LoadingText(alt),
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center,
                Color = new Color(170, 170, 170),
                FontScale = 0.78f,
                WordWrap = true
            };
            frame.Children.Add(status);
            if (string.IsNullOrWhiteSpace(url)) {
                return frame;
            }
            if (m_imageTextureCache.TryGetValue(url, out Texture2D cachedTexture)) {
                // 缓存命中（内容重建，如自动翻译）：复用已下载纹理，不再重复下载。
                ApplyImageTexture(cachedTexture, frame, background, image, status, alt, url, maxHeight);
                return frame;
            }
            _ = LoadImageAsync(url, alt, frame, background, image, status, maxHeight);
            return frame;
        }

        // 宿主销毁/重建前调用：释放纹理句柄并递增代际，晚到的旧代回调据此就地释放新纹理。
        public void ReleaseTextures() {
            m_generation++;
            if (m_loadedTextures.Count == 0) {
                return;
            }
            foreach (Texture2D texture in m_loadedTextures) {
                try {
                    texture?.Dispose();
                }
                catch (Exception ex) {
                    Log.Information(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            LanguageControl.Get("PostDetailsContentBuilder", "DisposePostDetailsTextureFailed"),
                            ex.Message
                        )
                    );
                }
            }
            m_loadedTextures.Clear();
            m_imageTextureCache.Clear();
            m_appliedFrames.Clear();
            m_pendingFrames.Clear();
        }

        // 宿主测量期回填 frame 宽度之后调用：宽已解析的挂起 frame 补适配上屏。
        // 每帧测量都会调，未解析的继续挂起
        public void ResolvePendingWidths() {
            if (m_pendingFrames.Count == 0) {
                return;
            }
            for (int i = m_pendingFrames.Count - 1; i >= 0; i--) {
                (CanvasWidget frame, RectangleWidget background, RectangleWidget image, LabelWidget status, string alt, string url, float maxHeight) = m_pendingFrames[i];
                if (frame.Size.X <= 0f) {
                    continue;
                }
                m_pendingFrames.RemoveAt(i);
                ApplyImageTexture(m_imageTextureCache.TryGetValue(url, out Texture2D texture) ? texture : null, frame, background, image, status, alt, url, maxHeight);
            }
        }

        async Task LoadImageAsync(string url,
            string alt,
            CanvasWidget frame,
            RectangleWidget background,
            RectangleWidget image,
            LabelWidget status,
            float maxHeight) {
            int generation = m_generation;
            byte[] data;
            try {
                using MemoryStream downloaded = await s_imageDownloader.DownloadToMemoryAsync(url, null, CancellationToken.None)
                    .ConfigureAwait(false);
                data = downloaded.ToArray();
                if (data.Length == 0) {
                    Dispatcher.Dispatch(delegate { status.Text = FailedToLoadText(alt); });
                    return;
                }
            }
            catch (Exception ex) {
                Dispatcher.Dispatch(delegate { status.Text = FailedToLoadText(alt); });
                Log.Information(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        LanguageControl.Get("PostDetailsContentBuilder", "RequestPostDetailsImageFailed"),
                        url,
                        ex.Message
                    )
                );
                return;
            }
            // 解码是纯 CPU（ImageSharp，大图可达几十毫秒）：留在后台线程，主线程只做 GL 上传
            Image decodedImage;
            try {
                using MemoryStream stream = new(data);
                decodedImage = Image.Load(stream);
            }
            catch (Exception ex) {
                Dispatcher.Dispatch(delegate { status.Text = FailedToLoadText(alt); });
                Log.Information(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        LanguageControl.Get("PostDetailsContentBuilder", "DecodePostDetailsImageFailed"),
                        url,
                        ex.Message
                    )
                );
                return;
            }
            Dispatcher.Dispatch(
                delegate {
                    try {
                        if (generation != m_generation) {
                            return;
                        }
                        Texture2D texture = Texture2D.Load(decodedImage);
                        if (generation != m_generation) {
                            // 宿主已释放/重建：纹理不会被任何 ReleaseTextures 回收，就地销毁。
                            try {
                                texture.Dispose();
                            }
                            catch (Exception disposeEx) {
                                Log.Information(
                                    string.Format(
                                        CultureInfo.InvariantCulture,
                                        LanguageControl.Get("PostDetailsContentBuilder", "DisposeLatePostDetailsTextureFailed"),
                                        disposeEx.Message
                                    )
                                );
                            }
                            return;
                        }
                        if (m_imageTextureCache.TryGetValue(url, out Texture2D existingTexture)) {
                            // 重建期间并发下载了同一 URL：后到者就地释放，复用先到者。
                            try {
                                texture.Dispose();
                            }
                            catch (Exception disposeEx) {
                                Log.Information(
                                    string.Format(
                                        CultureInfo.InvariantCulture,
                                        LanguageControl.Get("PostDetailsContentBuilder", "DisposeDuplicatePostDetailsTextureFailed"),
                                        disposeEx.Message
                                    )
                                );
                            }
                            texture = existingTexture;
                        }
                        else {
                            m_imageTextureCache[url] = texture;
                            m_loadedTextures.Add(texture);
                        }
                        ApplyImageTexture(texture, frame, background, image, status, alt, url, maxHeight);
                    }
                    catch (Exception ex) {
                        status.Text = FailedToLoadText(alt);
                        Log.Information(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                LanguageControl.Get("PostDetailsContentBuilder", "DecodePostDetailsImageFailed"),
                                url,
                                ex.Message
                            )
                        );
                    }
                }
            );
        }

        void ApplyImageTexture(Texture2D texture,
            CanvasWidget frame,
            RectangleWidget background,
            RectangleWidget image,
            LabelWidget status,
            string alt,
            string url,
            float maxHeight) {
            try {
                if (texture is not { Width: > 0, Height: > 0 }) {
                    status.Text = FailedToLoadText(alt);
                    return;
                }
                if (frame.Size.X <= 0f) {
                    // 宿主宽未解析（跟随父容器的宿主测量期才回填）：挂起，宽解析后由
                    // ResolvePendingWidths 补适配上屏。现在适配只能按兜底宽，窄容器/列表项内必溢出
                    m_pendingFrames.Add((frame, background, image, status, alt, url, maxHeight));
                    return;
                }
                Vector2 size = GetImageDisplaySize(texture, frame.Size.X, maxHeight);
                frame.Size = size;
                background.Size = size;
                image.Size = size;
                image.Subtexture = new Subtexture(texture, Vector2.Zero, Vector2.One);
                image.IsVisible = true;
                status.IsVisible = false;
                m_appliedFrames.Add(frame);
            }
            catch (Exception ex) {
                status.Text = FailedToLoadText(alt);
                Log.Warning(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        LanguageControl.Get("PostDetailsContentBuilder", "DecodePostDetailsImageFailed"),
                        url,
                        ex.Message
                    )
                );
            }
        }

        static string LoadingText(string alt) {
            return string.Format(CultureInfo.InvariantCulture, LanguageControl.Get("PostDetailsDialog", "Loading"), alt);
        }

        static string FailedToLoadText(string alt) {
            return string.Format(CultureInfo.InvariantCulture, LanguageControl.Get("PostDetailsDialog", "FailedToLoad"), alt);
        }

        // 图片显示尺寸：适配可用宽与高上限，不放大（scale ≤ 1）。
        // 可用宽=占位 frame 当前宽（宿主测量期回填过；宽未解析的 frame 在 ApplyImageTexture
        // 已挂起，不会走到这里）
        Vector2 GetImageDisplaySize(Texture2D texture, float availableWidth, float maxHeight) {
            float scale = MathUtils.Min(availableWidth / texture.Width, maxHeight / texture.Height);
            if (scale > 1f) {
                scale = 1f;
            }
            return new Vector2(texture.Width * scale, texture.Height * scale);
        }

        string ResolveUrl(string url) {
            if (string.IsNullOrWhiteSpace(url)) {
                return string.Empty;
            }
            url = WebUtility.HtmlDecode(url.Trim());
            if (url.StartsWith("//", StringComparison.Ordinal)) {
                return $"https:{url}";
            }
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri absoluteUri)) {
                return absoluteUri.ToString();
            }
            if (string.IsNullOrEmpty(BaseUrl)
                || !Uri.TryCreate(BaseUrl, UriKind.Absolute, out Uri baseUri)) {
                return url;
            }
            return url.StartsWith("/", StringComparison.Ordinal)
                ? new Uri(new Uri(baseUri.GetLeftPart(UriPartial.Authority)), url).ToString()
                : new Uri(baseUri, url).ToString();
        }
    }
}
