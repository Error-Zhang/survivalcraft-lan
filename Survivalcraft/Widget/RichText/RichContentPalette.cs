using Engine;

namespace Game {
    // 富文本行内配色：Markdown（MarkdownSpanParser）与 HTML（HtmlSpanParser）双解析器共用。
    // 行内代码橙黄字 + 深底（PRD 6.4）、超链接蓝
    public static class RichContentPalette {
        public static readonly Color CodeColor = new(255, 180, 96);

        public static readonly Color CodeBackgroundColor = new(60, 60, 70);

        public static readonly Color LinkColor = new(96, 168, 255);
    }
}
