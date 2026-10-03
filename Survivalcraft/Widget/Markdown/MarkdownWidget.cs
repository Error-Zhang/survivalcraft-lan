using Engine;
using Engine.Media;

namespace Game {
    // Markdown 部件：竖排块级布局（StackPanelWidget）。Markdown setter 仅标脏，首次 Measure 时重建
    // （字体/父宽在构建期不可得）；子 RichTextWidget 的 LinkClicked 聚合转发。
    // 图片管线：本部件持有 RichContentImageLoader（纹理句柄生命周期归部件）——
    // UpdateCeases（被移出更新树，ContainerWidget 级联）与 Rebuild（内容变更）时全量释放
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

        // 图片纹理生命周期归本部件（widget 树无 Dispose 级联，UpdateCeases 释放）
        readonly RichContentImageLoader m_imageLoader = new();

        public MarkdownWidget() {
            Direction = LayoutDirection.Vertical;
            DefaultColor = Color.White;
            // 块容器横向撑满宿主格：内部 Center 对齐（图片块等）的参考系恒为宿主内容宽，
            // 不随内容最宽子件塌缩（窄内容居中会整体左漂）
            HorizontalAlignment = WidgetAlignment.Stretch;
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsDrawRequired = true;
            if (m_needsBuild) {
                Rebuild();
            }
            // 代码块外层宽度：构建时父宽未知，测量期按可用宽解析（扣减构建时记录的嵌套缩进，
            // 列表项/引用块内不占全宽）
            float contentWidth = ContentWidth > 0f ? ContentWidth : parentAvailableSize.X;
            foreach ((CanvasWidget codeBlock, float indentX) in m_builder.CodeBlocks) {
                codeBlock.Size = new Vector2(MathUtils.Max(contentWidth - indentX, 50f), codeBlock.Size.Y);
            }
            // 图片块同机制；已加载完成的 frame 宽=图宽，不覆盖（HorizontalAlignment=Center 居中）
            foreach ((CanvasWidget imageBlock, float indentX) in m_builder.ImageBlocks) {
                if (!m_imageLoader.IsFrameApplied(imageBlock)) {
                    imageBlock.Size = new Vector2(MathUtils.Max(contentWidth - indentX, 50f), imageBlock.Size.Y);
                }
            }
            base.MeasureOverride(parentAvailableSize);
        }

        public override void UpdateCeases() {
            // 框架把本部件移出更新树时调用（ContainerWidget 级联）：释放图片纹理句柄并标记已释放，
            // 晚到的图片回调就地销毁新纹理，不再加入列表
            m_imageLoader.ReleaseTextures();
            base.UpdateCeases();
        }

        void Rebuild() {
            // 内容变更：旧内容的图片纹理立即全量释放（loader 代际递增使旧下载晚到回调就地销毁），
            // 缓存一并清空——内容变了没有复用价值；同类内 Markdown 相同值不会走到这（setter 判等）
            m_imageLoader.ReleaseTextures();
            // 缩进栈归零：构建中途异常留下的脏缩进不得传染下一次构建
            m_builder.CurrentIndentX = 0;
            Children.Clear();
            m_builder.CodeBlocks.Clear();
            m_builder.ImageBlocks.Clear();
            m_builder.Font = Font ?? LabelWidget.BitmapFont ?? BitmapFont.DebugFont;
            m_builder.FontScale = FontScale;
            m_builder.DefaultColor = DefaultColor;
            m_builder.ContentWidth = ContentWidth;
            m_builder.ImageLoader = m_imageLoader;
            m_builder.LinkClicked = null;
            if (LinkClicked != null) {
                m_builder.LinkClicked += href => LinkClicked(href);
            }
            try {
                StackPanelWidget content = m_builder.Build(m_markdown);
                // content 面板仅作构建容器，块级 widget 直接上提到本部件，减少一层嵌套
                foreach (Widget block in content.Children.ToArray()) {
                    content.Children.Remove(block);
                    Children.Add(block);
                }
            }
            catch (Exception e) {
                // 解析/构建失败不能留在半空状态：清残留 + 兜底错误段落。
                // 同一输入结果确定，清脏防每帧 Measure 重试
                m_builder.CodeBlocks.Clear();
                m_builder.ImageBlocks.Clear();
                Children.Add(new FontTextWidget {
                    Text = "(Markdown 渲染失败：" + e.Message + ")",
                    Font = m_builder.Font,
                    Color = DefaultColor,
                    WordWrap = true
                });
            }
            m_needsBuild = false;
        }
    }
}
