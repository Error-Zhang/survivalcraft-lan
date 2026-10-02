using Engine;
using Engine.Media;

namespace Game {
    // Markdown 部件：竖排块级布局（StackPanelWidget）。Markdown setter 仅标脏，首次 Measure 时重建
    // （字体/父宽在构建期不可得）；子 RichTextWidget 的 LinkClicked 聚合转发
    public class MarkdownWidget : StackPanelWidget {
        string m_markdown;

        bool m_needsBuild = true;

        public string Markdown {
            get => m_markdown;
            set {
                if (value != m_markdown) {
                    m_markdown = value;
                    m_needsBuild = true;
                }
            }
        }

        // 字体；null 回退 LabelWidget 静态字体 / DebugFont
        public BitmapFont Font { get; set; }

        // 基础字号（标题阶梯与行内 FontScale 均以此为基准）
        public float FontScale { get; set; } = 1f;

        // span 未指定颜色时的默认色
        public Color DefaultColor { get; set; }

        // <0 跟随父可用宽
        public float ContentWidth { get; set; } = -1f;

        public event Action<string> LinkClicked;

        readonly MarkdownToWidgetsBuilder m_builder = new();

        public MarkdownWidget() {
            Direction = LayoutDirection.Vertical;
            DefaultColor = Color.White;
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsDrawRequired = true;
            if (m_needsBuild) {
                Rebuild();
            }
            // 代码块外层宽度：构建时父宽未知，测量期按可用宽解析
            float contentWidth = ContentWidth > 0f ? ContentWidth : parentAvailableSize.X;
            foreach (CanvasWidget codeBlock in m_builder.CodeBlocks) {
                codeBlock.Size = new Vector2(contentWidth, codeBlock.Size.Y);
            }
            base.MeasureOverride(parentAvailableSize);
        }

        void Rebuild() {
            m_needsBuild = false;
            Children.Clear();
            m_builder.CodeBlocks.Clear();
            m_builder.Font = Font ?? LabelWidget.BitmapFont ?? BitmapFont.DebugFont;
            m_builder.FontScale = FontScale;
            m_builder.DefaultColor = DefaultColor;
            m_builder.ContentWidth = ContentWidth;
            m_builder.LinkClicked = null;
            if (LinkClicked != null) {
                m_builder.LinkClicked += href => LinkClicked(href);
            }
            StackPanelWidget content = m_builder.Build(m_markdown);
            // content 面板仅作构建容器，块级 widget 直接上提到本部件，减少一层嵌套
            foreach (Widget block in content.Children.ToArray()) {
                content.Children.Remove(block);
                Children.Add(block);
            }
        }
    }
}
