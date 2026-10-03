using Engine;
using Engine.Media;

namespace Game {
    // 富内容构建器基类：Markdown（MarkdownToWidgetsBuilder）与 HTML（HtmlToWidgetsBuilder）
    // 两个前端共用的配置字段、块级视觉常量与 widget 原语（富文本段/代码块/图片块/面板）。
    // 块注册表 CodeBlocks/ImageBlocks 供宿主 widget（MarkdownWidget 等）在测量期回填宽度——
    // 转发方（Markdown 遇 HtmlBlock）负责把子 builder 的注册表合并回自己
    public abstract class RichContentBuilder {
        public BitmapFont Font;

        public float FontScale = 1f;

        public Color DefaultColor = Color.White;

        // <0 跟随父可用宽（宿主 widget MeasureOverride 解析代码块宽度）
        public float ContentWidth = -1f;

        // 非 event：允许宿主整体赋值/解绑
        public Action<string> LinkClicked;

        // 代码块外层 Canvas，宽度需在宿主 MeasureOverride 按可用宽解析（构建时父宽未知）。
        // 元组带构建时的嵌套内容列缩进（CurrentIndentX），回填时从宿主宽中扣减——
        // 否则列表项/引用块内的代码块按全宽构建，右缘溢出面板
        public readonly List<(CanvasWidget Frame, float IndentX)> CodeBlocks = [];

        // 宽度未定时（ContentWidth<=0）需在测量期回填宽度的图片块 frame（同 CodeBlocks 机制；
        // 已加载完成的 frame 由 loader.IsFrameApplied 排除，宽=图宽不可覆盖）
        public readonly List<(CanvasWidget Frame, float IndentX)> ImageBlocks = [];

        // 图片异步加载管线；null = 图片保持占位文本（宿主不接图片管线的场景）。
        // 类型 internal：由宿主 widget 注入并持有纹理生命周期，直接使用本 builder 的外部代码保持占位行为
        internal RichContentImageLoader ImageLoader;

        // 当前嵌套内容列的横向缩进（列表项文字列、引用块竖线右移等）。
        // 进入嵌套上下文前保存、离开时恢复（栈式纪律归调用方）；代码块/图片块宽度据此扣减。
        // internal：Markdown 前端把 HtmlBlock 委托给 HtmlToWidgetsBuilder 时需要传递
        internal float CurrentIndentX;

        // 标题字号/金色阶梯（PRD 6.3，与 PostDetailsContentBuilder 同源风格）
        static readonly float[] s_headingScales = [1.6f, 1.4f, 1.2f, 1.1f, 1f, 1f];

        static readonly Color HeadingColor = new(255, 230, 140);

        protected static readonly Color QuoteColor = new(175, 175, 175);

        static readonly Color QuoteLineColor = new(120, 120, 120);

        static readonly Color CodeTextColor = new(225, 225, 228);

        static readonly Color CodeCenterColor = new(52, 52, 56);

        static readonly Color CodeBevelColor = new(90, 90, 95);

        protected static readonly Color ThematicBreakColor = new(120, 120, 120);

        protected const int ListIndentPerDepth = 22;

        // 列表行内坐标（相对挂载点）：marker 列偏移、marker→文字保底间隙、任务 checkbox 尺寸。
        // marker 字号=正文（用户裁定勿缩小）；y 偏移不做字形垂直补偿（字体可自定义，补偿会害正常字体偏下）
        protected const float MarkerOffsetX = 4f;

        protected const float MarkerOffsetY = 3f;

        protected const float MarkerTextGap = 8f;

        protected const float TaskCheckboxSize = 16f;

        // 图片块高上限（适配时缩小不放大，与 PDCB 同值）
        protected const float MaxImageHeight = 360f;

        // 块级嵌套深度上限（引用/列表/HTML 互嵌递归）：防御不可信输入的深嵌套
        protected const int MaxDepth = 64;

        protected RichTextWidget CreateRichText(Color? colorOverride) {
            RichTextWidget widget = new() {
                WordWrap = true,
                Font = Font,
                FontScale = FontScale,
                Color = colorOverride ?? DefaultColor,
                Margin = new Vector2(0f, 2f)
            };
            if (LinkClicked != null) {
                widget.LinkClicked += href => LinkClicked(href);
            }
            return widget;
        }

        protected static StackPanelWidget CreatePanel() => new() {
            Direction = LayoutDirection.Vertical
        };

        // 图片块：走共用 RichContentImageLoader；宽度未定时 frame 记入 ImageBlocks 由测量期回填。
        // 占位宽扣减嵌套缩进（列表项/引用块内图片不占全宽）
        protected void AddImageBlock(StackPanelWidget panel, string url, string alt) {
            float width = NestedContentWidth();
            CanvasWidget frame = ImageLoader.CreateImageBlock(url, alt, width, MaxImageHeight);
            frame.Margin = new Vector2(0f, 4f);
            panel.Children.Add(frame);
            if (ContentWidth <= 0f) {
                ImageBlocks.Add((frame, CurrentIndentX));
            }
        }

        // 嵌套感知的可用内容宽：显式 ContentWidth 扣减当前缩进（保底防深嵌套负值）；
        // <0 时返回 -1（跟随父格/测量期回填）
        protected float NestedContentWidth() {
            return ContentWidth > 0f ? MathUtils.Max(ContentWidth - CurrentIndentX, 50f) : -1f;
        }

        // 标题行 widget：金色 + 字号阶梯（level 1-6）
        protected RichTextWidget CreateHeading(int level, Color? colorOverride) {
            RichTextWidget widget = CreateRichText(colorOverride);
            widget.FontScale = FontScale * s_headingScales[MathUtils.Min(level, 6) - 1];
            widget.Color = HeadingColor;
            widget.Margin = new Vector2(0f, level <= 2 ? 8f : 4f);
            return widget;
        }

        // 代码块：BevelledRectangle 框 + 横向 ScrollPanel + 逐行 Label（不换行，保留原始空白）。
        // ScrollPanel 不产出 DesiredSize：不定位的子节点由外层 Canvas 排满整个 cell，
        // 外层高度显式定（行高累计 + 滚动条/内边距余量），文字内边距走 linesPanel Margin
        protected CanvasWidget CreateCodeBlock(string[] lines) {
            if (lines.Length == 0
                || (lines.Length == 1 && lines[0].Length == 0)) {
                lines = [" "];
            }
            float codeScale = 0.9f;
            float lineHeight = ((Font?.GlyphHeight ?? 51f) + (Font?.Spacing.Y ?? 0f)) * codeScale * (Font?.Scale ?? 1f) + 1f;
            float contentHeight = lines.Length * lineHeight;
            CanvasWidget outer = new() {
                Size = new Vector2(NestedContentWidth(), contentHeight + 20f),
                Margin = new Vector2(0f, 4f)
            };
            BevelledRectangleWidget frame = new() {
                Size = new Vector2(1f, 1f),
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                CenterColor = CodeCenterColor,
                BevelColor = CodeBevelColor,
                DirectionalLight = 0f,
                AmbientLight = 1f
            };
            outer.Children.Add(frame);
            ScrollPanelWidget scrollPanel = new() {
                Direction = LayoutDirection.Horizontal,
                ScrollPosition = 0f,
                Margin = new Vector2(3f, 3f),
            };
            StackPanelWidget linesPanel = new() {
                Direction = LayoutDirection.Vertical,
                Margin = new Vector2(5f, 6f)
            };
            foreach (string line in lines) {
                linesPanel.Children.Add(new FontTextWidget {
                    Text = line.Length == 0 ? " " : line,
                    Font = Font,
                    Color = CodeTextColor,
                    FontScale = codeScale,
                    WordWrap = false
                });
            }
            scrollPanel.Children.Add(linesPanel);
            outer.Children.Add(scrollPanel);
            CodeBlocks.Add((outer, CurrentIndentX));
            return outer;
        }

        // 引用块外壳：左竖线（-1 高度随内容拉伸）+ 灰字缩进容器，inner 面板交调用方填内容
        protected CanvasWidget CreateQuoteFrame(out StackPanelWidget inner) {
            CanvasWidget outer = new() {
                Size = new Vector2(ContentWidth > 0f ? ContentWidth : -1f, -1f),
                Margin = new Vector2(0f, 4f)
            };
            RectangleWidget line = new() {
                Size = new Vector2(2f, 1f),
                VerticalAlignment = WidgetAlignment.Stretch,
                FillColor = QuoteLineColor,
                OutlineColor = Color.Transparent,
                OutlineThickness = 0f
            };
            outer.Children.Add(line);
            inner = CreatePanel();
            inner.Margin = new Vector2(0f, 2f);
            outer.Children.Add(inner);
            CanvasWidget.SetPosition(inner, new Vector2(10f, 0f));
            return outer;
        }

        // 水平分隔线（hr / Markdown ---）
        protected static RectangleWidget CreateThematicBreak() {
            return new RectangleWidget {
                Size = new Vector2(1f, 1f),
                HorizontalAlignment = WidgetAlignment.Stretch,
                FillColor = ThematicBreakColor,
                OutlineColor = Color.Transparent,
                OutlineThickness = 0f,
                Margin = new Vector2(0f, 6f)
            };
        }
    }
}
