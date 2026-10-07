using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    /// <summary>文本断行模式（CSS word-break 子集），默认 BreakWord。</summary>
    public enum TextWordBreak {
        /// <summary>词边界回退（拉丁回退空白/标点、CJK 字间可断+禁则），无断点时紧急强拆 fitCount-1——CSS word-break:break-word（deprecated）别名，默认历史行为。</summary>
        BreakWord,
        /// <summary>词边界回退同 BreakWord，无断点时纯溢出（整段保持单行，不强拆）——CSS normal 字面。</summary>
        Normal,
        /// <summary>行尾即拆（不回退断点，拉丁词在行尾处直接截断）——CSS word-break:break-all。</summary>
        BreakAll,
        /// <summary>词边界回退但禁用 CJK 字间断点（仅空白/标点可退），无断点时纯溢出——CSS word-break:keep-all 字面。</summary>
        KeepAll
    }

    public class FontTextWidget : Widget {
        public string m_text = string.Empty;

        public TextOrientation m_textOrientation;

        public BitmapFont m_font;

        public Vector2 m_fontSpacing;

        public float m_fontScale;

        public VectorFont m_vectorFont;

        public int m_maxLines = int.MaxValue;

        public bool m_wordWrap;

        public TextWordBreak m_wordBreak = TextWordBreak.BreakWord;

        public bool m_ellipsis;

        public List<string> m_lines = [];

        public Vector2? m_linesSize;

        public float? m_linesAvailableWidth;

        public float? m_linesAvailableHeight;

        public Vector2 Size { get; set; }

        public virtual string Text {
            get => m_text;
            set {
                if (m_text != value
                    && value != null) {
                    m_text = value;
                    m_linesSize = null;
                }
            }
        }

        public TextAnchor TextAnchor { get; set; }

        public TextOrientation TextOrientation {
            get => m_textOrientation;
            set {
                if (value != m_textOrientation) {
                    m_textOrientation = value;
                    m_linesSize = null;
                }
            }
        }

        public BitmapFont Font {
            get => m_font ?? (LabelWidget.BitmapFont ?? BitmapFont.DebugFont);
            set {
                if (value != m_font) {
                    m_font = value;
                    m_linesSize = null;
                }
            }
        }

        // 缩放单一存储（P4 I1 修复）：位图 FontScale 与矢量 VectorFontScale 同读写字段 m_fontScale，
        // 历史位图调用点（LoadingScreen 0.6、对话框 0.7-0.9 等）在矢量路径取得同尺字号，
        // 消除两属性各自存储导致的矢量路径忽视 FontScale 的尺寸回归
        public float FontScale {
            get => m_fontScale;
            set {
                if (value != m_fontScale) {
                    m_fontScale = value;
                    m_linesSize = null;
                }
            }
        }

        /// <summary>矢量字号缩放：位图 FontScale 的矢量侧别名（同一 backing field，见上）</summary>
        public float VectorFontScale {
            get => m_fontScale;
            set {
                if (value != m_fontScale) {
                    m_fontScale = value;
                    m_linesSize = null;
                }
            }
        }

        /// <summary>矢量字体链（P4）：null=位图路径（零改动）；非 null 时 Draw/Measure 全部走 VectorFont</summary>
        public VectorFont VectorFont {
            get => m_vectorFont;
            set {
                if (value != m_vectorFont) {
                    m_vectorFont = value;
                    m_linesSize = null;
                }
            }
        }

        public Vector2 FontSpacing {
            get => m_fontSpacing;
            set {
                if (value != m_fontSpacing) {
                    m_fontSpacing = value;
                    m_linesSize = null;
                }
            }
        }

        public bool WordWrap {
            get => m_wordWrap;
            set {
                if (value != m_wordWrap) {
                    m_wordWrap = value;
                    m_linesSize = null;
                }
            }
        }

        /// <summary>断行模式：默认 BreakWord=词边界回退+无断点强拆（引擎历史行为）；Normal/KeepAll 无断点纯溢出；BreakAll=行尾即拆。</summary>
        public TextWordBreak WordBreak {
            get => m_wordBreak;
            set {
                if (value != m_wordBreak) {
                    m_wordBreak = value;
                    m_linesSize = null;
                }
            }
        }

        public bool Ellipsis {
            get => m_ellipsis;
            set {
                if (value != m_ellipsis) {
                    m_ellipsis = value;
                    m_linesSize = null;
                }
            }
        }

        public int MaxLines {
            get => m_maxLines;
            set {
                if (value != m_maxLines) {
                    m_maxLines = value;
                    m_linesSize = null;
                }
            }
        }

        public Color Color { get; set; }

        public bool DropShadow { get; set; }

        public bool TextureLinearFilter { get; set; }
        public bool m_usedebug;

        public FontTextWidget() {
            IsHitTestVisible = false;
            FontScale = 1f;
            Color = Color.White;
            TextureLinearFilter = true;
            Size = new Vector2(-1f);
            // P4：默认启用矢量链（回滚点=本行）；链未构建/构建失败时为 null → 位图路径零改动
            VectorFont = FontManager.DefaultChain;
        }

        public override void Draw(DrawContext dc) {
            if (!string.IsNullOrEmpty(Text)
                && Color.A != 0) {
                if (VectorFont != null) {
                    DrawVectorText(dc);
                    return;
                }
                SamplerState samplerState = TextureLinearFilter ? SamplerState.LinearClamp : SamplerState.PointClamp;
                FontBatch2D fontBatch2D = dc.PrimitivesRenderer2D.FontBatch(Font, 1, DepthStencilState.None, null, null, samplerState);
                int count = fontBatch2D.TriangleVertices.Count;
                float num = 0f;
                if ((TextAnchor & TextAnchor.VerticalCenter) != 0) {
                    float num2 = Font.GlyphHeight * FontScale * Font.Scale
                        + (m_lines.Count - 1) * ((Font.GlyphHeight + Font.Spacing.Y) * FontScale * Font.Scale + FontSpacing.Y);
                    num = (ActualSize.Y - num2) / 2f;
                }
                else if ((TextAnchor & TextAnchor.Bottom) != 0) {
                    float num3 = Font.GlyphHeight * FontScale * Font.Scale
                        + (m_lines.Count - 1) * ((Font.GlyphHeight + Font.Spacing.Y) * FontScale * Font.Scale + FontSpacing.Y);
                    num = ActualSize.Y - num3;
                }
                TextAnchor anchor = TextAnchor & ~(TextAnchor.VerticalCenter | TextAnchor.Bottom);
                Color color = Color * GlobalColorTransform;
                float num4 = CalculateLineHeight();
                foreach (string line in m_lines) {
                    float x = 0f;
                    if ((TextAnchor & TextAnchor.HorizontalCenter) != 0) {
                        x = ActualSize.X / 2f;
                    }
                    else if ((TextAnchor & TextAnchor.Right) != 0) {
                        x = ActualSize.X;
                    }
                    //bool flag = true;
                    Vector2 vector = Vector2.Zero;
                    float angle = 0f;
                    if (TextOrientation == TextOrientation.Horizontal) {
                        vector = new Vector2(x, num);
                        angle = 0f;
                        _ = Display.ScissorRectangle;
                        //flag = true;
                    }
                    else if (TextOrientation == TextOrientation.VerticalLeft) {
                        vector = new Vector2(x, ActualSize.Y + num);
                        angle = MathUtils.DegToRad(-90f);
                        //flag = true;
                    }
                    //if (flag)
                    //{
                    if (DropShadow) {
                        fontBatch2D.QueueText(
                            line,
                            vector + 1f * new Vector2(FontScale),
                            0f,
                            new Color((byte)0, (byte)0, (byte)0, color.A),
                            anchor,
                            new Vector2(FontScale),
                            FontSpacing,
                            angle
                        );
                    }
                    fontBatch2D.QueueText(
                        line,
                        vector,
                        0f,
                        color,
                        anchor,
                        new Vector2(FontScale),
                        FontSpacing,
                        angle
                    );
                    //}
                    num += num4;
                }
                fontBatch2D.TransformTriangles(GlobalTransform, count);
            }
        }

        // 矢量绘制路径（P4）：m_lines 行列表与位图路径同源（UpdateLines 已在 Measure 时填充）。
        // 颜色（Color*GlobalColorTransform、阴影黑底取主色 A）、垂直锚点（num 行高累计）、
        // GlobalTransform（每批记录本 widget 入队前顶点数，收尾 TransformTriangles 同 :225）语义镜像位图路径；
        // 水平锚点：位图经 QueueText→CalculateTextOffset（Center −w/2、Right −w）沿行进轴偏移——
        // 水平排=沿 widget X（pivot.X 扣行宽）；竖排=行进轴为 (0,-1)（FontBatch2D.cs:74 Offset.X*vector），
        // 锚点 x 落在行高带轴（widget X，不扣行宽），行宽偏移折入 pivot.Y（Center/Right 使 run 以其为中心/终点）；
        // VerticalLeft 以 sink 内 -90° 刚体变换实现（QueueGlyphQuad 后 quad 仍轴对齐，与位图 QueueText 的 angle 路径同布局；
        // 差异：字形不逐点吸附像素，矢量图集 LinearClamp 采样按亚像素定位，v1 接受）
        void DrawVectorText(DrawContext dc) {
            SamplerState samplerState = TextureLinearFilter ? SamplerState.LinearClamp : SamplerState.PointClamp;
            VectorGlyphSink sink = new(dc.PrimitivesRenderer2D, samplerState);
            float lineHeight = VectorFont.LineHeight * VectorFontScale;
            float num = 0f;
            if ((TextAnchor & TextAnchor.VerticalCenter) != 0) {
                num = (ActualSize.Y - m_lines.Count * lineHeight) / 2f;
            }
            else if ((TextAnchor & TextAnchor.Bottom) != 0) {
                num = ActualSize.Y - m_lines.Count * lineHeight;
            }
            TextAnchor anchor = TextAnchor & ~(TextAnchor.VerticalCenter | TextAnchor.Bottom);
            Color color = Color * GlobalColorTransform;
            // UI 全局缩放（ScreensManager FinalUiScale 等经 GlobalTransform 施加）：字形按屏上实际
            // 像素尺寸栅格化（清晰），sink 把 FSS quad 除回 widget 空间——布局与度量仍按 widget 口径
            float gtScale = MathF.Abs(GlobalTransform.M11) is float s11 and > 0.0001f ? s11 : 1f;
            float size = VectorFont.DefaultSize * VectorFontScale * gtScale;
            sink.Scale = 1f / gtScale;
            bool vertical = TextOrientation == TextOrientation.VerticalLeft;
            int lineIndex = 0;
            foreach (string line in m_lines) {
                // 行宽必须按 widget 空间字号量（不含 gtScale）：锚点 runOffset 作用在 widget 空间 pivot 上，
                // 若混入栅格空间行宽（×gtScale），UI 全局缩放≠1 时 Center/Right 锚点偏移被放大 gtScale 倍
                //（ModSettingsScreen Description 不居中根因）；探针 gt=1 测不到，F9 矩阵以 RenderTransform ×1.6 复现
                float lineWidth = VectorFont.MeasureString(line, VectorFont.DefaultSize * VectorFontScale).X;
                float x = 0f;
                float runOffset = 0f; // CalculateTextOffset.X 口径：Center −w/2、Right −w（沿行进轴的行宽偏移）
                if ((anchor & TextAnchor.HorizontalCenter) != 0) {
                    x = ActualSize.X / 2f;
                    runOffset = -lineWidth / 2f;
                }
                else if ((anchor & TextAnchor.Right) != 0) {
                    x = ActualSize.X;
                    runOffset = -lineWidth;
                }
                Vector2 pivot;
                if (vertical) {
                    // 竖排（位图 QueueText angle 路径口径）：行 k 沿行高带轴 +X 堆叠（QueueText 行距 num·vector4，
                    // vector4=+X——行次序自左向右），锚点 x 不扣行宽；行宽偏移沿行进轴 (0,-1) 折入 pivot.Y
                    //（Center 以 pivot.Y 为 run 中心、Right 为 run 终点），各行共享同一基点 Y
                    pivot = new Vector2(x + lineIndex * lineHeight, ActualSize.Y + num - runOffset);
                    lineIndex++;
                }
                else {
                    pivot = new Vector2(x + runOffset, num); // 水平：锚点偏移沿行进轴（widget X）
                    num += lineHeight; // 行距只在水平分支累加（竖排走 lineIndex，num 恒=锚点基值）——切勿再留 foreach 尾部累加，否则水平双倍行距
                }
                sink.Rotated = vertical;
                if (DropShadow) {
                    // 阴影位移在 widget 空间恒 (+1,+1)*缩放：两种朝向下都等价于 pivot 平移（旋转分支输出两分量同加）
                    sink.Pivot = pivot + new Vector2(VectorFontScale);
                    VectorFont.DrawText(sink, line, Vector2.Zero, new Color((byte)0, (byte)0, (byte)0, color.A), size);
                }
                sink.Pivot = pivot;
                VectorFont.DrawText(sink, line, Vector2.Zero, color, size);
            }
            sink.TransformTriangles(GlobalTransform);
        }

        // widget 级字形 quad 接收口：按纹理路由/缓存 VectorFontBatch2D（layer/渲染状态与位图路径 FontBatch 同粒度），
        // 记录各批本 widget 入队前顶点数，供 Draw 收尾统一施加 GlobalTransform（镜像位图路径 TransformTriangles(GlobalTransform, count)）；
        // Rotated 时对 quad 做 -90° 刚体变换（位置转置 + UV 角点随转，见 QueueGlyphQuad 注），水平时仅 Pivot 平移
        // internal 共享（P4 Task 7）：RichTextWidget 矢量 run 分流复用同一 sink（Pivot/Rotated 为可选能力，不用即零开销）
        internal sealed class VectorGlyphSink : IFontBatchSink {
            readonly PrimitivesRenderer2D _renderer;
            readonly SamplerState _samplerState;
            readonly Dictionary<Texture2D, VectorFontBatch2D> _batches = [];
            readonly Dictionary<Texture2D, int> _batchStarts = [];

            internal bool Rotated;

            internal Vector2 Pivot;

            // FSS quad → widget 空间的收缩系数（GT 全局缩放的栅格补偿，见 DrawVectorText）
            internal float Scale = 1f;

            internal VectorGlyphSink(PrimitivesRenderer2D renderer, SamplerState samplerState) {
                _renderer = renderer;
                _samplerState = samplerState;
            }

            public void QueueGlyphQuad(Texture2D texture, Vector2 destination, Vector2 size, Vector2 uv1, Vector2 uv2, float depth, Color color) {
                if (!_batches.TryGetValue(texture, out VectorFontBatch2D batch)) {
                    batch = _renderer.VectorFontBatch(texture, 1, DepthStencilState.None, null, null, _samplerState);
                    _batches.Add(texture, batch);
                    _batchStarts.Add(texture, batch.TriangleVertices.Count);
                }
                if (Scale != 1f) {
                    destination *= Scale;
                    size *= Scale;
                }
                if (Rotated) {
                    // 真旋转=-90°（与位图 angle 路径同向）：字形角位随转、uv 原样。QueueGlyphQuad(dest,size,uv)
                    // 的 uv 角可分离（u 只随屏 X），表达不了 90° 旋转——只转位形会把字形图正排进竖列格
                    //（=顶朝上 bug），必须走四角位口。90°CCW：字形 TL→格左下、TR→格左上、BR→格右上、BL→格右下
                    float x0 = Pivot.X + destination.Y;
                    float y0 = Pivot.Y - destination.X;
                    batch.QueueGlyphQuadCorners(
                        texture,
                        new Vector2(x0, y0),
                        new Vector2(x0, y0 - size.X),
                        new Vector2(x0 + size.Y, y0 - size.X),
                        new Vector2(x0 + size.Y, y0),
                        uv1, uv2, depth, color);
                }
                else {
                    batch.QueueGlyphQuad(texture, destination + Pivot, size, uv1, uv2, depth, color);
                }
            }

            internal void TransformTriangles(Matrix matrix) {
                foreach (KeyValuePair<Texture2D, VectorFontBatch2D> pair in _batches) {
                    pair.Value.TransformTriangles(matrix, _batchStarts[pair.Key]);
                }
            }
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsDrawRequired = !string.IsNullOrEmpty(Text) && Color.A != 0;
            if (TextOrientation == TextOrientation.Horizontal) {
                UpdateLines(parentAvailableSize.X, parentAvailableSize.Y);
                if (!m_linesSize.HasValue) {
                    return;
                }
                DesiredSize = new Vector2(Size.X < 0f ? m_linesSize.Value.X : Size.X, Size.Y < 0f ? m_linesSize.Value.Y : Size.Y);
            }
            else if (TextOrientation == TextOrientation.VerticalLeft) {
                UpdateLines(parentAvailableSize.Y, parentAvailableSize.X);
                if (!m_linesSize.HasValue) {
                    return;
                }
                DesiredSize = new Vector2(Size.X < 0f ? m_linesSize.Value.Y : Size.X, Size.Y < 0f ? m_linesSize.Value.X : Size.Y);
            }
        }

        public float CalculateLineHeight() => VectorFont != null
            ? VectorFont.LineHeight * VectorFontScale
            : (Font.GlyphHeight + Font.Spacing.Y + FontSpacing.Y) * FontScale * Font.Scale;

        // 矢量版 FitText（VectorFont!=null 时替代 Font.FitText 的宽度查询）：逐字符 MeasureString 累计宽度，
        // 累计超 width 时返回已容字符数（位图 FitText 同语义；FSS 无 kerning 表，逐字符度量之和≈整串度量）；
        // \r/\u200b 零宽跳过与位图一致。首版 O(n) 每行（FSS 内部缓存 glyph 度量），优化后置
        int FitVectorText(float width, string text, int start, int length) {
            float size = VectorFont.DefaultSize * VectorFontScale;
            float num = 0f;
            for (int i = start; i < start + length; i++) {
                char c = text[i];
                if (c == '\r'
                    || c == '\u200b') {
                    continue;
                }
                num += VectorFont.MeasureString(c.ToString(), size).X;
                if (num > width) {
                    return i - start;
                }
            }
            return length;
        }

        // 在 text 的前 fitCount 个字符超出可用宽度时，寻找合适的换行位置（返回值为换行点，即下一行首字符下标）。
        // 词边界模式（BreakWord/Normal/KeepAll）回退到最近的空白或标点处，CJK 字间断点按模式启用/禁用并守行首/行尾禁则；
        // 无断点时 BreakWord 紧急强拆 fitCount-1（引擎历史行为），Normal/KeepAll 纯溢出（返回 text.Length，整段保持单行）
        public static int FindLineBreakPosition(string text, int fitCount, TextWordBreak wordBreak = TextWordBreak.BreakWord) {
            switch (wordBreak) {
                case TextWordBreak.BreakAll:
                    // BreakAll 豁免禁则与词边界回退，行尾即拆；fitCount 由调用侧钳正
                    return MathUtils.Max(fitCount, 1);
                case TextWordBreak.Normal:
                    // Normal 纯 CSS 字面：词边界回退同 BreakWord，无断点不强拆
                    return RetreatToBreakPosition(text, fitCount, true) ?? text.Length;
                case TextWordBreak.KeepAll:
                    // KeepAll 纯 CSS 字面：禁用 CJK 字间断点（仅空白/标点可退），无断点纯溢出
                    return RetreatToBreakPosition(text, fitCount, false) ?? text.Length;
                default:
                    // BreakWord（引擎历史行为，未知值兜底同此）：词边界回退，无断点紧急强拆 fitCount-1
                    return RetreatToBreakPosition(text, fitCount, true) ?? fitCount - 1;
            }
        }

        // 词边界回退：自 fitCount-2 向左扫到最近的空白/标点处（allowCjkBreak 时还认 CJK 字间断点，且守行首/行尾禁则），
        // 返回换行点（下一行首字符下标）；无断点返回 null（由调用方决定纯溢出或紧急强拆）
        static int? RetreatToBreakPosition(string text, int fitCount, bool allowCjkBreak) {
            if (allowCjkBreak
                && CanBreakBeforeCjk(text, fitCount)) {
                return fitCount;
            }
            int index = fitCount - 2;
            while (index >= 0
                && !char.IsWhiteSpace(text[index])
                && !char.IsPunctuation(text[index])
                && !(allowCjkBreak
                    && CanBreakBeforeCjk(text, index + 1))) {
                index--;
            }
            return index < 0 ? null : index + 1;
        }

        // 判断是否可以在 text 的 position 处（下一行首字符下标）断行：下一行首字符须为 CJK 字符，
        // 且不是行首禁则标点（如 ，。」…），其前一字符也不是行尾禁则标点（如 （「“）
        public static bool CanBreakBeforeCjk(string text, int position) {
            if (position <= 0 || position >= text.Length) {
                return false;
            }
            char next = text[position];
            char previous = text[position - 1];
            return IsCjkChar(next)
                && !IsCjkForbiddenLineStart(next)
                && !IsCjkForbiddenLineEnd(previous);
        }

        public static bool IsCjkChar(char c) => c is >= '\u2e80' and <= '\u9fff' // 部首、假名、注音、CJK 统一表意文字
            or >= '\uac00' and <= '\ud7af' // 谚文
            or >= '\uf900' and <= '\ufaff'; // CJK 兼容表意文字

        public static bool IsCjkForbiddenLineStart(char c) => "、。，．：；？！〉》」』】〕）｝ー・»”’‥…,:;!?)]}".Contains(c);

        public static bool IsCjkForbiddenLineEnd(char c) => "（《「『〈【〔｛“‘([{".Contains(c);

        public virtual void UpdateLines(float availableWidth, float availableHeight) {
            if (m_linesAvailableHeight.HasValue
                && m_linesAvailableHeight == availableHeight
                && m_linesAvailableWidth.HasValue
                && m_linesSize.HasValue) {
                float num = MathUtils.Min(m_linesSize.Value.X, m_linesAvailableWidth.Value) - 0.1f;
                float num2 = MathUtils.Max(m_linesSize.Value.X, m_linesAvailableWidth.Value) + 0.1f;
                if (availableWidth >= num
                    && availableWidth <= num2) {
                    return;
                }
            }
            availableWidth += 0.1f;
            m_lines.Clear();
            string[] array = (Text ?? string.Empty).Split(["\n"], StringSplitOptions.None);
            string text = "...";
            float x = VectorFont != null
                ? VectorFont.MeasureString(text, VectorFont.DefaultSize * VectorFontScale).X
                : Font.MeasureText(text, new Vector2(FontScale), FontSpacing).X;
            if (WordWrap) {
                int num3 = (int)MathUtils.Min(MathF.Floor(availableHeight / CalculateLineHeight()), MaxLines);
                for (int i = 0; i < array.Length; i++) {
                    string text2 = array[i].TrimEnd();
                    if (text2.Length == 0) {
                        m_lines.Add(string.Empty);
                        continue;
                    }
                    while (text2.Length > 0) {
                        bool flag;
                        int num4;
                        if (Ellipsis && m_lines.Count + 1 >= num3) {
                            num4 = VectorFont != null
                                ? FitVectorText(MathUtils.Max(availableWidth - x, 0f), text2, 0, text2.Length)
                                : Font.FitText(MathUtils.Max(availableWidth - x, 0f), text2, 0, text2.Length, FontScale, FontSpacing.X);
                            flag = true;
                        }
                        else {
                            num4 = VectorFont != null
                                ? FitVectorText(availableWidth, text2, 0, text2.Length)
                                : Font.FitText(availableWidth, text2, 0, text2.Length, FontScale, FontSpacing.X);
                            num4 = MathUtils.Max(num4, 1);
                            flag = false;
                            if (num4 < text2.Length) {
                                num4 = FindLineBreakPosition(text2, num4, WordBreak);
                                // 可用宽≈0 时断点可为 0（拉丁串回退 fitCount-1），钳 1 防零推进死循环——实证 label OOM 根因（与 RichTextLayout breakPosition<=0 防护对齐）
                                num4 = MathUtils.Max(num4, 1);
                            }
                        }
                        string text3;
                        if (num4 == text2.Length) {
                            text3 = text2;
                            text2 = string.Empty;
                        }
                        else {
                            text3 = text2.Substring(0, num4).TrimEnd();
                            if (flag) {
                                text3 += text;
                            }
                            text2 = text2.Substring(num4, text2.Length - num4).TrimStart();
                        }
                        m_lines.Add(text3);
                        if (flag) {
                            break;
                        }
                    }
                }
            }
            else if (Ellipsis) {
                for (int j = 0; j < array.Length; j++) {
                    string text4 = array[j].TrimEnd();
                    int num7 = VectorFont != null
                        ? FitVectorText(MathUtils.Max(availableWidth - x, 0f), text4, 0, text4.Length)
                        : Font.FitText(MathUtils.Max(availableWidth - x, 0f), text4, 0, text4.Length, FontScale, FontSpacing.X);
                    if (num7 < text4.Length) {
                        m_lines.Add(text4.Substring(0, num7).TrimEnd() + text);
                    }
                    else {
                        m_lines.Add(text4);
                    }
                }
            }
            else {
                m_lines.AddRange(array);
            }
            if (m_lines.Count > MaxLines) {
                m_lines = m_lines.Take(MaxLines).ToList();
            }
            Vector2 zero = Vector2.Zero;
            for (int k = 0; k < m_lines.Count; k++) {
                Vector2 vector = VectorFont != null
                    ? new Vector2(VectorFont.MeasureString(m_lines[k], VectorFont.DefaultSize * VectorFontScale).X, 0f)
                    : Font.MeasureText(m_lines[k], new Vector2(FontScale), FontSpacing);
                zero.X = MathUtils.Max(zero.X, vector.X);
                if (k < m_lines.Count - 1) {
                    zero.Y += CalculateLineHeight();
                }
                else {
                    zero.Y += VectorFont != null
                        ? VectorFont.LineHeight * VectorFontScale
                        : Font.GlyphHeight * FontScale * Font.Scale;
                }
            }
            m_linesSize = zero;
            m_linesAvailableWidth = availableWidth;
            m_linesAvailableHeight = availableHeight;
        }
    }
}