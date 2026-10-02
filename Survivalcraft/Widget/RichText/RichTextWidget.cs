using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    // 富文本部件：多 span（颜色/字号/粗体/链接）单 FontBatch 合批绘制
    // 排版走 RichTextLayout；命中测试按行内 run 的 x 区间反查（单行 LTR 无 bidi）
    public class RichTextWidget : Widget {
        public List<TextSpan> m_spans = [];

        public BitmapFont m_font;

        public Vector2 m_fontSpacing;

        public float m_fontScale;

        public int m_maxLines = int.MaxValue;

        public bool m_wordWrap;

        public bool m_ellipsis;

        public readonly RichTextLayout m_layout = new();

        public Vector2? m_layoutSize;

        public float? m_layoutAvailableWidth;

        public float? m_layoutAvailableHeight;

        public bool m_isDirty = true;

        public Vector2 Size { get; set; }

        public List<TextSpan> Spans {
            get => m_spans;
            set {
                if (value != m_spans) {
                    m_spans = value ?? [];
                    MarkLayoutDirty();
                }
            }
        }

        // 就地修改 Spans 列表内容后调用（列表引用不变时引擎无法感知变化）
        public void MarkLayoutDirty() {
            m_isDirty = true;
        }

        public TextAnchor TextAnchor { get; set; }

        public BitmapFont Font {
            get => m_font ?? (LabelWidget.BitmapFont ?? BitmapFont.DebugFont);
            set {
                if (value != m_font) {
                    m_font = value;
                    MarkLayoutDirty();
                }
            }
        }

        public float FontScale {
            get => m_fontScale;
            set {
                if (value != m_fontScale) {
                    m_fontScale = value;
                    MarkLayoutDirty();
                }
            }
        }

        public Vector2 FontSpacing {
            get => m_fontSpacing;
            set {
                if (value != m_fontSpacing) {
                    m_fontSpacing = value;
                    MarkLayoutDirty();
                }
            }
        }

        public bool WordWrap {
            get => m_wordWrap;
            set {
                if (value != m_wordWrap) {
                    m_wordWrap = value;
                    MarkLayoutDirty();
                }
            }
        }

        public bool Ellipsis {
            get => m_ellipsis;
            set {
                if (value != m_ellipsis) {
                    m_ellipsis = value;
                    MarkLayoutDirty();
                }
            }
        }

        public int MaxLines {
            get => m_maxLines;
            set {
                if (value != m_maxLines) {
                    m_maxLines = value;
                    MarkLayoutDirty();
                }
            }
        }

        // span 未指定颜色时的默认色
        public Color Color { get; set; }

        public bool DropShadow { get; set; }

        public bool TextureLinearFilter { get; set; }

        // 链接下划线；关闭后链接仅靠 span 颜色区分
        public bool ShowLinkUnderline { get; set; }

        public bool HasContent {
            get {
                foreach (TextSpan span in m_spans) {
                    if (!string.IsNullOrEmpty(span.Text)) {
                        return true;
                    }
                }
                return false;
            }
        }

        public event Action<string> LinkClicked;

        public RichTextWidget() {
            IsHitTestVisible = false;
            FontScale = 1f;
            Color = Color.White;
            TextureLinearFilter = true;
            ShowLinkUnderline = true;
            Size = new Vector2(-1f);
        }

        public override void Update() {
            if (LinkClicked != null
                && Input.Click.HasValue) {
                TextRun startRun = FindLinkAtPoint(ScreenToWidget(Input.Click.Value.Start));
                if (startRun != null
                    && FindLinkAtPoint(ScreenToWidget(Input.Click.Value.End)) == startRun) {
                    LinkClicked(startRun.Span.Href);
                }
            }
        }

        public override void Draw(DrawContext dc) {
            if (m_layout.Lines.Count == 0
                || Color.A == 0) {
                return;
            }
            // 背景/下划线用 layer=-1 的 FlatBatch：渲染器按 batch Layer 排序绘制，
            // -1 排在文字批次（默认 0）之前，背景才压在文字下
            bool needsFlatBatch = ShowLinkUnderline || HasBackground;
            FlatBatch2D flatBatch2D = needsFlatBatch ? dc.PrimitivesRenderer2D.FlatBatch(-1, DepthStencilState.None) : null;
            SamplerState samplerState = TextureLinearFilter ? SamplerState.LinearClamp : SamplerState.PointClamp;
            FontBatch2D fontBatch2D = dc.PrimitivesRenderer2D.FontBatch(Font, 1, DepthStencilState.None, null, null, samplerState);
            int batchStart = fontBatch2D.TriangleVertices.Count;
            float blockY = CalculateBlockY();
            foreach (TextLine line in m_layout.Lines) {
                float lineX = 0f;
                if ((TextAnchor & TextAnchor.HorizontalCenter) != 0) {
                    lineX = (ActualSize.X - line.Width) / 2f;
                }
                else if ((TextAnchor & TextAnchor.Right) != 0) {
                    lineX = ActualSize.X - line.Width;
                }
                foreach (TextRun run in line.Runs) {
                    Color color = (run.Span.Color ?? Color) * GlobalColorTransform;
                    if (color.A == 0
                        && !run.Span.IsLink
                        && run.Span.BackgroundColor == null) {
                        continue;
                    }
                    Vector2 scale = new(SpanScale(run.Span));
                    Vector2 position = new(lineX + run.X, blockY + line.Y);
                    if (flatBatch2D != null
                        && run.Span.BackgroundColor is Color background) {
                        // 背景＝run 自身字号的 em 高矩形（同下划线基准）
                        Color backgroundColor = background * GlobalColorTransform;
                        if (backgroundColor.A != 0) {
                            QueueTransformedQuad(
                                flatBatch2D,
                                position.X,
                                position.Y,
                                run.Width,
                                Font.GlyphHeight * scale.X * Font.Scale,
                                backgroundColor
                            );
                        }
                    }
                    if (DropShadow) {
                        fontBatch2D.QueueText(
                            run.Text,
                            position + 1f * new Vector2(FontScale),
                            0f,
                            new Color((byte)0, (byte)0, (byte)0, color.A),
                            TextAnchor.Default,
                            scale,
                            FontSpacing
                        );
                    }
                    if (run.Span.IsBold) {
                        // 伪粗体：同串横偏 1×FontScale 双绘
                        fontBatch2D.QueueText(
                            run.Text,
                            position + new Vector2(FontScale, 0f),
                            0f,
                            color,
                            TextAnchor.Default,
                            scale,
                            FontSpacing
                        );
                    }
                    fontBatch2D.QueueText(
                        run.Text,
                        position,
                        0f,
                        color,
                        TextAnchor.Default,
                        scale,
                        FontSpacing
                    );
                    if (run.Span.IsLink
                        && ShowLinkUnderline) {
                        // GlyphHeight 须乘 Font.Scale（与 CalculateLineHeight 一致），否则落在下一行
                        float underlineY = position.Y + Font.GlyphHeight * scale.X * Font.Scale + 1f;
                        float thickness = MathUtils.Max(FontScale * Font.Scale, 1f);
                        QueueTransformedQuad(
                            flatBatch2D,
                            position.X,
                            underlineY,
                            run.Width,
                            thickness,
                            color
                        );
                    }
                }
            }
            fontBatch2D.TransformTriangles(GlobalTransform, batchStart);
        }

        // 局部坐标 quad（x,y 为左上，w/h 尺寸）四角各自经 GlobalTransform 变换后入 FlatBatch（同 RectangleWidget 做法，
        // 只变换两对角点会在旋转/斜切变换下塌陷）
        void QueueTransformedQuad(FlatBatch2D batch, float x, float y, float width, float height, Color color) {
            Matrix m = GlobalTransform;
            Vector2 p1 = Vector2.Transform(new Vector2(x, y), m);
            Vector2 p2 = Vector2.Transform(new Vector2(x + width, y), m);
            Vector2 p3 = Vector2.Transform(new Vector2(x + width, y + height), m);
            Vector2 p4 = Vector2.Transform(new Vector2(x, y + height), m);
            batch.QueueQuad(p1, p2, p3, p4, 0f, color);
        }

        // 文字块顶部相对 widget 的 y 偏移（垂直锚点）；绘制与命中测试必须共用同一计算
        float CalculateBlockY() {
            float totalHeight = 0f;
            foreach (TextLine line in m_layout.Lines) {
                totalHeight += line.Height;
            }
            if ((TextAnchor & TextAnchor.VerticalCenter) != 0) {
                return (ActualSize.Y - totalHeight) / 2f;
            }
            if ((TextAnchor & TextAnchor.Bottom) != 0) {
                return ActualSize.Y - totalHeight;
            }
            return 0f;
        }

        bool HasBackground {
            get {
                foreach (TextSpan span in m_spans) {
                    if (span.BackgroundColor != null) {
                        return true;
                    }
                }
                return false;
            }
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsDrawRequired = HasContent && Color.A != 0;
            UpdateLayout(parentAvailableSize.X, parentAvailableSize.Y);
            if (!m_layoutSize.HasValue) {
                return;
            }
            DesiredSize = new Vector2(Size.X < 0f ? m_layoutSize.Value.X : Size.X, Size.Y < 0f ? m_layoutSize.Value.Y : Size.Y);
        }

        public float CalculateLineHeight() => (Font.GlyphHeight + Font.Spacing.Y + FontSpacing.Y) * FontScale * Font.Scale;

        void UpdateLayout(float availableWidth, float availableHeight) {
            availableWidth += 0.1f;
            if (!m_isDirty
                && m_layoutSize.HasValue
                && m_layoutAvailableWidth.HasValue
                && m_layoutAvailableWidth.Value == availableWidth
                && m_layoutAvailableHeight.HasValue
                && m_layoutAvailableHeight.Value == availableHeight) {
                return;
            }
            m_layout.Font = Font;
            m_layout.FontSpacing = FontSpacing;
            m_layout.DefaultFontScale = FontScale;
            m_layout.WordWrap = WordWrap;
            m_layout.MaxLines = MaxLines;
            m_layout.Ellipsis = Ellipsis;
            m_layout.Spans = m_spans;
            m_layout.Layout(availableWidth, availableHeight);
            m_layoutSize = m_layout.MeasuredSize;
            m_layoutAvailableWidth = availableWidth;
            m_layoutAvailableHeight = availableHeight;
            m_isDirty = false;
        }

        float SpanScale(TextSpan span) => span.FontScale > 0f ? span.FontScale : FontScale;

        // 命中测试：坐标为 widget 本地坐标；先按行 y 区间定位行（含垂直锚点偏移，与 Draw 一致），再按行内 run x 区间定位链接 run
        public TextRun FindLinkAtPoint(Vector2 point) {
            float blockY = CalculateBlockY();
            foreach (TextLine line in m_layout.Lines) {
                if (point.Y < blockY + line.Y
                    || point.Y >= blockY + line.Y + line.Height) {
                    continue;
                }
                float lineX = 0f;
                if ((TextAnchor & TextAnchor.HorizontalCenter) != 0) {
                    lineX = (ActualSize.X - line.Width) / 2f;
                }
                else if ((TextAnchor & TextAnchor.Right) != 0) {
                    lineX = ActualSize.X - line.Width;
                }
                foreach (TextRun run in line.Runs) {
                    if (run.IsLink
                        && point.X >= lineX + run.X
                        && point.X < lineX + run.X + run.Width) {
                        return run;
                    }
                }
            }
            return null;
        }
    }
}
