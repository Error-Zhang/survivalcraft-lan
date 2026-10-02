using System.Net;
using System.Text.RegularExpressions;
using Engine;
using Markdig.Syntax.Inlines;

namespace Game {
    // 行内样式上下文：粗斜体/颜色/链接随嵌套深入累积
    public struct MarkdownSpanStyle {
        public bool Bold;

        public bool Italic;

        public Color? Color;

        public string Href;
    }

    // Markdig inline 序列 → List<TextSpan>；HtmlInline 片段喂 HtmlAgilityPack 递归映射
    public static class MarkdownSpanParser {
        // 行内代码配色（与 PRD 6.4 一致：橙黄字 + 深底）
        public static readonly Color CodeColor = new(255, 180, 96);

        public static readonly Color CodeBackgroundColor = new(60, 60, 70);

        public static readonly Color LinkColor = new(96, 168, 255);

        // 超链接蓝
        public static void Parse(ContainerInline container, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor) {
            foreach (Inline inline in container) {
                ParseInline(inline, spans, style, defaultColor);
            }
        }

        static void ParseInline(Inline inline, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor) {
            switch (inline) {
                case LiteralInline literal:
                    AddText(literal.Content.ToString(), spans, style, defaultColor);
                    break;
                case EmphasisInline emphasis: {
                    // DelimiterCount：1=斜体、2=粗体、3=粗斜体
                    MarkdownSpanStyle inner = style;
                    if (emphasis.DelimiterCount >= 2) {
                        inner.Bold = true;
                    }
                    if (emphasis.DelimiterCount == 1 || emphasis.DelimiterCount >= 3) {
                        inner.Italic = true;
                    }
                    if (inline is ContainerInline emphasisContainer) {
                        Parse(emphasisContainer, spans, inner, defaultColor);
                    }
                    break;
                }
                case CodeInline code:
                    spans.Add(new TextSpan(code.Content, CodeColor) {
                        BackgroundColor = CodeBackgroundColor,
                        Style = style.Bold ? RichTextStyle.Bold : RichTextStyle.Normal
                    });
                    break;
                case LinkInline link: {
                    if (link.IsImage) {
                        // 图片占位：v1 不真渲染（P3）
                        spans.Add(new TextSpan($"[图片:{GetInlineText(link)}]", new Color(160, 160, 168)));
                        break;
                    }
                    MarkdownSpanStyle inner = style;
                    inner.Href = link.Url;
                    inner.Color = LinkColor;
                    if (inline is ContainerInline linkContainer) {
                        Parse(linkContainer, spans, inner, defaultColor);
                    }
                    break;
                }
                case AutolinkInline autolink:
                    spans.Add(new TextSpan(autolink.Url, LinkColor) { Href = autolink.Url });
                    break;
                case LineBreakInline:
                    // 软换行 v1 一律当硬换行
                    AddText("\n", spans, style, defaultColor);
                    break;
                case HtmlEntityInline entity:
                    AddText(WebUtility.HtmlDecode(entity.Original.ToString()), spans, style, defaultColor);
                    break;
                case HtmlInline html:
                    ParseHtmlFragment(html.ToString(), spans, style, defaultColor);
                    break;
                default:
                    if (inline is ContainerInline fallbackContainer) {
                        Parse(fallbackContainer, spans, style, defaultColor);
                    }
                    else if (inline is LeafInline) {
                        // 未知叶子 inline：取其源文本兜底，避免内容静默丢失
                        string text = inline.ToString();
                        if (!string.IsNullOrEmpty(text)) {
                            AddText(text, spans, style, defaultColor);
                        }
                    }
                    break;
            }
        }

        static void AddText(string text, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor) {
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            RichTextStyle richStyle = style.Bold ? RichTextStyle.Bold : style.Italic ? RichTextStyle.Italic : RichTextStyle.Normal;
            spans.Add(new TextSpan(text, style.Color ?? defaultColor, richStyle, style.Href));
        }

        // 拼接 inline 子树的纯文本（图片 alt 等场景）
        public static string GetInlineText(ContainerInline container) {
            var builder = new System.Text.StringBuilder();
            foreach (Inline inline in container) {
                switch (inline) {
                    case LiteralInline literal:
                        builder.Append(literal.Content.ToString());
                        break;
                    case CodeInline code:
                        builder.Append(code.Content);
                        break;
                    case HtmlEntityInline entity:
                        builder.Append(WebUtility.HtmlDecode(entity.Original.ToString()));
                        break;
                    default:
                        if (inline is ContainerInline inner) {
                            builder.Append(GetInlineText(inner));
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        // HtmlInline 片段 → span。块级标签在行内上下文按"透传子节点"处理（块级断段由 builder 的 HtmlBlock 入口负责）
        static void ParseHtmlFragment(string html, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor) {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(html ?? string.Empty);
            foreach (HtmlAgilityPack.HtmlNode node in doc.DocumentNode.ChildNodes) {
                ParseHtmlNode(node, spans, style, defaultColor);
            }
        }

        public static void ParseHtmlNode(HtmlAgilityPack.HtmlNode node, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor) {
            switch (node.NodeType) {
                case HtmlAgilityPack.HtmlNodeType.Text:
                    AddText(WebUtility.HtmlDecode(node.InnerText), spans, style, defaultColor);
                    return;
                case HtmlAgilityPack.HtmlNodeType.Element:
                    break;
                default:
                    return;
            }
            string tag = node.Name.ToLowerInvariant();
            switch (tag) {
                case "b":
                case "strong": {
                    MarkdownSpanStyle inner = style;
                    inner.Bold = true;
                    ParseHtmlChildren(node, spans, inner, defaultColor);
                    break;
                }
                case "i":
                case "em": {
                    MarkdownSpanStyle inner = style;
                    inner.Italic = true;
                    ParseHtmlChildren(node, spans, inner, defaultColor);
                    break;
                }
                case "a": {
                    MarkdownSpanStyle inner = style;
                    inner.Href = node.GetAttributeValue("href", null);
                    inner.Color = LinkColor;
                    ParseHtmlChildren(node, spans, inner, defaultColor);
                    break;
                }
                case "code":
                    ParseHtmlChildren(node, spans, style, defaultColor, CodeColor, CodeBackgroundColor);
                    break;
                case "br":
                    AddText("\n", spans, style, defaultColor);
                    break;
                case "span": {
                    MarkdownSpanStyle inner = style;
                    string styleAttr = node.GetAttributeValue("style", null);
                    if (TryParseStyleColor(styleAttr, "color", out Color color)) {
                        inner.Color = color;
                    }
                    // 背景色随文字 span 一起走（同 MarkdownSpanParser 单 span 能力）
                    ParseHtmlChildren(node, spans, inner, defaultColor, null, TryParseStyleColor(styleAttr, "background-color", out Color background) ? background : null);
                    break;
                }
                default: {
                    // 未知标签：透传子节点；节点自带 style color 时染其内容
                    MarkdownSpanStyle inner = style;
                    if (TryParseStyleColor(node.GetAttributeValue("style", null), "color", out Color inheritedColor)) {
                        inner.Color = inheritedColor;
                    }
                    ParseHtmlChildren(node, spans, inner, defaultColor);
                    break;
                }
            }
        }

        static void ParseHtmlChildren(HtmlAgilityPack.HtmlNode node, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, Color? forceColor = null, Color? background = null) {
            int firstNew = spans.Count;
            foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes) {
                ParseHtmlNode(child, spans, style, defaultColor);
            }
            if (forceColor != null || background != null) {
                for (int i = firstNew; i < spans.Count; i++) {
                    if (forceColor != null) {
                        spans[i].Color = forceColor;
                    }
                    spans[i].BackgroundColor = background;
                }
            }
        }

        static readonly Regex s_styleColorRegex = new("(?<name>color|background-color)\\s*:\\s*(?<value>[^;]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // 仅支持 #rgb/#rrggbb 与 rgb()/rgba()；解析失败返回 false（不染色）
        static bool TryParseStyleColor(string styleAttribute, string name, out Color color) {
            color = default;
            if (string.IsNullOrEmpty(styleAttribute)) {
                return false;
            }
            foreach (Match match in s_styleColorRegex.Matches(styleAttribute)) {
                if (!string.Equals(match.Groups["name"].Value, name, StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                string value = match.Groups["value"].Value.Trim();
                if (TryParseColorValue(value, out color)) {
                    return true;
                }
            }
            return false;
        }

        static bool TryParseColorValue(string value, out Color color) {
            color = default;
            if (value.StartsWith("#")) {
                string hex = value[1..];
                if (hex.Length == 6
                    && int.TryParse(hex[..2], System.Globalization.NumberStyles.HexNumber, null, out int r)
                    && int.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out int g)
                    && int.TryParse(hex[4..], System.Globalization.NumberStyles.HexNumber, null, out int b)) {
                    color = new Color(r, g, b);
                    return true;
                }
                if (hex.Length == 3
                    && int.TryParse(new string(hex[0], 2), System.Globalization.NumberStyles.HexNumber, null, out int r3)
                    && int.TryParse(new string(hex[1], 2), System.Globalization.NumberStyles.HexNumber, null, out int g3)
                    && int.TryParse(new string(hex[2], 2), System.Globalization.NumberStyles.HexNumber, null, out int b3)) {
                    color = new Color(r3, g3, b3);
                    return true;
                }
                return false;
            }
            // rgb(r,g,b) / rgba(r,g,b,a)
            if (value.StartsWith("rgb")) {
                int open = value.IndexOf('(');
                int close = value.LastIndexOf(')');
                if (open < 0 || close <= open) {
                    return false;
                }
                string[] parts = value[(open + 1)..close].Split(',');
                if (parts.Length is not (3 or 4)) {
                    return false;
                }
                if (!int.TryParse(parts[0].Trim(), out int rr)
                    || !int.TryParse(parts[1].Trim(), out int gg)
                    || !int.TryParse(parts[2].Trim(), out int bb)) {
                    return false;
                }
                int aa = 255;
                if (parts.Length == 4
                    && !int.TryParse(parts[3].Trim(), out aa)) {
                    return false;
                }
                color = new Color(rr, gg, bb, aa);
                return true;
            }
            return false;
        }
    }
}
