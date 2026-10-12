using Engine.Media;
using FontStashSharp;

namespace Engine.Graphics {
    /// <summary>
    /// 矢量文本 3D sink：IFontBatchSink 适配，令 VectorFont.DrawText（FSS 驱动）可绘入世界空间。
    /// 字形 quad 位于文本像素空间（原点=Origin、X 沿 Right、Y 沿 Down；|Right|/|Down| = 每 text px 的世界尺寸，
    /// 可非正交任意朝向），经当前基向量即时变换落批；
    /// 按图集纹理经 PrimitivesRenderer3D.VectorFontBatch 分批（FSS 图集可能跨页，防整批 Flush 纹理错配），
    /// 结构与 FontTextWidget.VectorGlyphSink（2D 版）同构。
    /// VectorFont.DrawText 的位图回退段与斜体切变（ShearedFontBatchSink 在文本空间平移 X）均经本 sink 变换自动成立。
    /// 宜按场景/帧生命周期使用：批字典与渲染器侧批不随字体热切换清理（旧图集批空置后 Flush 经 IsEmpty 跳过，
    /// 无绘制风险，仅累积）——2D 版按次绘制新建天然限寿，本类长命持有须自行重建
    /// </summary>
    public class VectorFontSink3D : IFontBatchSink {
        readonly PrimitivesRenderer3D _renderer;
        readonly SamplerState _samplerState;
        readonly int _layer;
        readonly Dictionary<Texture2D, VectorFontBatch3D> _batches = [];

        /// <summary>当前绘制的文本空间→世界基向量：原点（字形行左上锚点）与 X/Y 方向基向量</summary>
        public Vector3 Origin, Right = Vector3.UnitX, Down = Vector3.UnitY;

        /// <param name="layer">矢量批的 Layer（m_allBatches 排序层，相对其他 3D 批的绘制序）</param>
        public VectorFontSink3D(PrimitivesRenderer3D renderer, SamplerState samplerState = null, int layer = 0) {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _layer = layer;
        }

        /// <summary>设定本次绘制的世界基向量（每次 QueueText 前调用；quad 排入时即时变换，串中不重设即可）</summary>
        public void Begin(Vector3 origin, Vector3 right, Vector3 down) {
            Origin = origin;
            Right = right;
            Down = down;
        }

        public void QueueGlyphQuad(Texture2D texture, Vector2 destination, Vector2 size, Vector2 uv1, Vector2 uv2, float depth, Color color) {
            // depth 参数 3D 无意义（世界位置自带深度），文本像素空间 → 世界按当前基向量映射
            VectorFontBatch3D batch = GetBatch(texture);
            batch.QueueGlyphQuad(texture,
                Origin + Right * destination.X + Down * destination.Y,
                Right, Down, size, uv1, uv2, color);
        }

        VectorFontBatch3D GetBatch(Texture2D texture) {
            if (!_batches.TryGetValue(texture, out VectorFontBatch3D batch)) {
                batch = _renderer.VectorFontBatch(texture, _layer, DepthStencilState.Default, null, null, _samplerState);
                _batches.Add(texture, batch);
            }
            return batch;
        }

        /// <summary>
        /// 高层入口：整段文本（支持 \n 多行）矢量绘制入世界空间，对应 FontBatch3D.QueueText。
        /// position = 首行左上锚点（经 anchor 对齐）；right/down 模长 = 每 text px 的世界尺寸；
        /// anchor 按墨迹宽（最长行，字距按 n−1 缝口径补入）与总行盒高（行数 × lineAdvance）对齐；
        /// 样式走 VectorFont.DrawText 降级解析（缺真面→Stroked/切变，整链缺→位图回退）。
        /// 返回锚定用总行盒高（text px）
        /// </summary>
        public float QueueText(VectorFont font, string text, Vector3 position, Vector3 right, Vector3 down, Color color, float size,
            TextAnchor anchor = TextAnchor.Default, bool bold = false, bool italic = false, float characterSpacing = 0f) {
            ArgumentNullException.ThrowIfNull(font);
            ArgumentNullException.ThrowIfNull(text);
            string[] lines = text.Split('\n');
            float lineAdvance = GetLineAdvance(font, size, bold, italic);
            Vector2 box = MeasureLines(font, lines, size, bold, italic, characterSpacing, lineAdvance);
            Vector2 offset = Vector2.Zero;
            if ((anchor & TextAnchor.HorizontalCenter) != 0) {
                offset.X = -box.X / 2f;
            }
            else if ((anchor & TextAnchor.Right) != 0) {
                offset.X = -box.X;
            }
            if ((anchor & TextAnchor.VerticalCenter) != 0) {
                offset.Y = -box.Y / 2f;
            }
            else if ((anchor & TextAnchor.Bottom) != 0) {
                offset.Y = -box.Y;
            }
            for (int i = 0; i < lines.Length; i++) {
                // offset/lineAdvance 为 text px 量纲，与字形推进同用原始基向量（模长=世界/px）。
                // 不可归一化：FontBatch3D.QueueText 用 Normalize 是其 offset 已按 scale 预乘成世界量纲，口径不同
                Begin(position + offset.X * right + (offset.Y + i * lineAdvance) * down, right, down);
                font.DrawText(this, lines[i], Vector2.Zero, color, size, bold, italic, characterSpacing);
            }
            return box.Y;
        }

        /// <summary>量测整段文本锚定盒（宽=最长行墨宽、高=行数×lineAdvance；text px，与 QueueText 锚定同口径）</summary>
        public Vector2 MeasureText(VectorFont font, string text, float size, bool bold = false, bool italic = false, float characterSpacing = 0f) {
            ArgumentNullException.ThrowIfNull(font);
            ArgumentNullException.ThrowIfNull(text);
            string[] lines = text.Split('\n');
            float lineAdvance = GetLineAdvance(font, size, bold, italic);
            return MeasureLines(font, lines, size, bold, italic, characterSpacing, lineAdvance);
        }

        Vector2 MeasureLines(VectorFont font, string[] lines, float size, bool bold, bool italic, float characterSpacing, float lineAdvance) {
            var spriteFont = GetSpriteFont(font, size, bold, italic);
            float width = 0f;
            foreach (string line in lines) {
                float w;
                if (spriteFont != null) {
                    // VectorFont.MeasureString 不含调用方字距；墨宽按字符间 n−1 缝补入（与位图 CalculateTextOffset 扣字距同口径）
                    w = font.MeasureString(line, size, bold, italic).X;
                }
                else {
                    // 整链缺失：DrawText 全行落位图回退，宽度按位图度量（此时 VectorFont.MeasureString 返 Zero）
                    var fallback = font.FallbackBitmapFont;
                    w = fallback != null
                        ? fallback.MeasureText(line, new Vector2(size / VectorFont.DefaultSizeValue), Vector2.Zero).X
                        : 0f;
                }
                w += Math.Max(0, line.Length - 1) * characterSpacing;
                if (w > width) {
                    width = w;
                }
            }
            return new Vector2(width, lines.Length * lineAdvance);
        }

        // 行推进与 DrawText 行盒同口径：矢量链存在取 FSS LineHeight（按样式降级解析，与实际绘制链一致）；
        // 链存在但含缺字回退段时行盒取两源最大（DrawText 单行返回=各段最大，防回退字形与下行相侵）；
        // 整链缺失全行为位图回退段，行盒=位图回退行高；两者皆无降级为 size
        float GetLineAdvance(VectorFont font, float size, bool bold, bool italic) {
            float lineAdvance = size;
            var spriteFont = GetSpriteFont(font, size, bold, italic);
            if (spriteFont != null) {
                lineAdvance = spriteFont.LineHeight;
            }
            var fallback = font.FallbackBitmapFont;
            if (fallback != null) {
                lineAdvance = Math.Max(lineAdvance, fallback.LineHeight * (size / VectorFont.DefaultSizeValue * fallback.Scale));
            }
            return lineAdvance;
        }

        SpriteFontBase GetSpriteFont(VectorFont font, float size, bool bold, bool italic)
            => font.GetStyleFont(font.ResolveStyle(bold, italic, degrade: true), size)
                ?? font.GetStyleFont(VectorFontStyle.Regular, size);
    }
}
