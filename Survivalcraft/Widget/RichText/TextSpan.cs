using Engine;

namespace Game {
    public enum RichTextStyle {
        Normal,

        Bold,

        // 预留：Pericles 无斜体变体，当前按 Normal 渲染，后续视需要实现切变变换
        Italic
    }

    public class TextSpan {
        public string Text { get; set; }

        // null 时回退到 RichTextWidget.Color
        public Color? Color { get; set; }

        public float FontScale { get; set; }

        public RichTextStyle Style { get; set; }

        // 非 null 时该段为链接，点击触发 RichTextWidget.LinkClicked
        public string Href { get; set; }

        // 调用方自定义载荷（如 markdown 源节点），排版层不解释
        public object Tag { get; set; }

        public TextSpan() {
            Text = string.Empty;
            FontScale = 1f;
            Style = RichTextStyle.Normal;
        }

        public TextSpan(string text) : this() {
            Text = text ?? string.Empty;
        }

        public TextSpan(string text, Color? color) : this(text) {
            Color = color;
        }

        public TextSpan(string text, Color? color, RichTextStyle style) : this(text, color) {
            Style = style;
        }

        public TextSpan(string text, Color? color, RichTextStyle style, string href) : this(text, color, style) {
            Href = href;
        }

        public bool IsBold => Style == RichTextStyle.Bold;

        public bool IsLink => !string.IsNullOrEmpty(Href);
    }
}
