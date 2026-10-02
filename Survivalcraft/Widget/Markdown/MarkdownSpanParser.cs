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

        // 行内底色（标签栈路径由 AddText 直带；DOM 递归路径另有后刷机制，两处同值冗余无害）
        public Color? Background;

        public string Href;
    }

    // Markdig inline 序列 → List<TextSpan>；HtmlInline 片段喂 HtmlAgilityPack 递归映射
    public static class MarkdownSpanParser {
        // 行内代码配色（与 PRD 6.4 一致：橙黄字 + 深底）
        public static readonly Color CodeColor = new(255, 180, 96);

        public static readonly Color CodeBackgroundColor = new(60, 60, 70);

        public static readonly Color LinkColor = new(96, 168, 255);

        // 超链接蓝
        // 行内嵌套深度上限：防御不可信输入的深嵌套（StackOverflowException 不可捕获，直接崩进程）
        const int MaxDepth = 64;

        public static void Parse(ContainerInline container, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor) {
            Parse(container, spans, style, defaultColor, 0);
        }

        static void Parse(ContainerInline container, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, int depth) {
            if (depth > MaxDepth) {
                return;
            }
            // 行内 HTML 标签栈：Markdig 1.4.0 对每个标签各发一个 HtmlInline（"<b>x</b>"＝开/文/闭 3 项），
            // 开标签入栈、闭标签出栈，字面量按当前样式染色
            Stack<MarkdownSpanStyle> htmlStack = [];
            foreach (Inline inline in container) {
                // 行内 HTML 开/闭标签会更新样式上下文，需传回循环继续用
                style = ParseInline(inline, spans, style, defaultColor, htmlStack, depth);
            }
        }

        static MarkdownSpanStyle ParseInline(Inline inline, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, Stack<MarkdownSpanStyle> htmlStack, int depth) {
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
                        Parse(emphasisContainer, spans, inner, defaultColor, depth + 1);
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
                        Parse(linkContainer, spans, inner, defaultColor, depth + 1);
                    }
                    break;
                }
                case AutolinkInline autolink: {
                    // Markdig 邮箱 autolink 的 Url 不带 scheme，补 mailto: 才能被系统识别打开
                    string href = autolink.IsEmail ? "mailto:" + autolink.Url : autolink.Url;
                    spans.Add(new TextSpan(autolink.Url, LinkColor) { Href = href });
                    break;
                }
                case LineBreakInline:
                    // 软换行 v1 一律当硬换行
                    AddText("\n", spans, style, defaultColor);
                    break;
                case HtmlEntityInline entity:
                    AddText(WebUtility.HtmlDecode(entity.Original.ToString()), spans, style, defaultColor);
                    break;
                case HtmlInline html:
                    style = ApplyInlineHtmlTag(html.Tag, spans, style, defaultColor, htmlStack, depth);
                    break;
                default:
                    if (inline is ContainerInline fallbackContainer) {
                        Parse(fallbackContainer, spans, style, defaultColor, depth + 1);
                    }
                    // 未知叶子 inline 不兜底转文本：LeafInline.ToString() 返回 CLR 类型名
                    // （仅 LiteralInline/AutolinkInline 重写过，均已在前接住），渲染出类名是垃圾
                    break;
            }
            return style;
        }

        static void AddText(string text, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor) {
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            RichTextStyle richStyle = style.Bold ? RichTextStyle.Bold : style.Italic ? RichTextStyle.Italic : RichTextStyle.Normal;
            spans.Add(new TextSpan(text, style.Color ?? defaultColor, richStyle, style.Href) {
                BackgroundColor = style.Background
            });
        }

        // 拼接 inline 子树的纯文本（图片 alt 等场景）
        public static string GetInlineText(ContainerInline container) {
            return GetInlineText(container, 0);
        }

        static string GetInlineText(ContainerInline container, int depth) {
            if (depth > MaxDepth) {
                return string.Empty;
            }
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
                            builder.Append(GetInlineText(inner, depth + 1));
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        // HTML 片段 → span（块级 HTML 完整 DOM 场景）。行内上下文的零散标签走 ApplyInlineHtmlTag 标签栈
        static void ParseHtmlFragment(string html, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, int depth) {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(html ?? string.Empty);
            foreach (HtmlAgilityPack.HtmlNode node in doc.DocumentNode.ChildNodes) {
                ParseHtmlNode(node, spans, style, defaultColor, depth + 1);
            }
        }

        public static void ParseHtmlNode(HtmlAgilityPack.HtmlNode node, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, int depth = 0) {
            switch (node.NodeType) {
                case HtmlAgilityPack.HtmlNodeType.Text:
                    AddText(WebUtility.HtmlDecode(node.InnerText), spans, style, defaultColor);
                    return;
                case HtmlAgilityPack.HtmlNodeType.Element:
                    break;
                default:
                    return;
            }
            if (depth > MaxDepth) {
                return;
            }
            string tag = node.Name.ToLowerInvariant();
            if (tag == "br") {
                AddText("\n", spans, style, defaultColor);
                return;
            }
            MarkdownSpanStyle inner = ApplyOpenTag(node, style, out Color? background, out Color? forceColor);
            ParseHtmlChildren(node, spans, inner, defaultColor, depth, forceColor, background);
        }

        // 单个开标签对样式上下文的累积效果；background/forceColor 需后刷到生成的 span（code/span 场景）
        static MarkdownSpanStyle ApplyOpenTag(HtmlAgilityPack.HtmlNode node, MarkdownSpanStyle style, out Color? background, out Color? forceColor) {
            background = null;
            forceColor = null;
            MarkdownSpanStyle inner = style;
            switch (node.Name.ToLowerInvariant()) {
                case "b":
                case "strong":
                    inner.Bold = true;
                    break;
                case "i":
                case "em":
                    inner.Italic = true;
                    break;
                case "a":
                    inner.Href = node.GetAttributeValue("href", null);
                    inner.Color = LinkColor;
                    break;
                case "code":
                    forceColor = CodeColor;
                    background = CodeBackgroundColor;
                    // 标签栈路径（行内 HTML）没有 DOM 后刷机制，色与底色都直接挂进样式上下文
                    inner.Color = CodeColor;
                    inner.Background = CodeBackgroundColor;
                    break;
                case "span": {
                    string styleAttr = node.GetAttributeValue("style", null);
                    if (TryParseStyleColor(styleAttr, "color", out Color color)) {
                        inner.Color = color;
                    }
                    if (TryParseStyleColor(styleAttr, "background-color", out Color spanBackground)) {
                        background = spanBackground;
                        inner.Background = spanBackground;
                    }
                    break;
                }
                default:
                    // 未知标签：透传子节点；节点自带 style color 时染其内容
                    if (TryParseStyleColor(node.GetAttributeValue("style", null), "color", out Color inheritedColor)) {
                        inner.Color = inheritedColor;
                    }
                    break;
            }
            return inner;
        }

        // 行内上下文的零散 HtmlInline：Markdig 逐标签分发、无配对信息，逐项喂 HAP 会被自动闭合成空节点
        // （"<b>" 无子节点、"</b>" 被丢弃），样式全静默失效——用标签栈把开/闭标签串起来
        static MarkdownSpanStyle ApplyInlineHtmlTag(string tag, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, Stack<MarkdownSpanStyle> htmlStack, int depth) {
            if (string.IsNullOrEmpty(tag)) {
                return style;
            }
            if (tag.StartsWith("</", StringComparison.Ordinal)) {
                // 闭标签：出栈恢复进入前样式；无配对开标签时忽略
                return htmlStack.Count > 0 ? htmlStack.Pop() : style;
            }
            // void 元素与自闭合写法没有配对闭标签，按完整片段当场生效（<br>→换行等）
            if (IsVoidHtmlTag(tag)) {
                ParseHtmlFragment(tag, spans, style, defaultColor, depth + 1);
                return style;
            }
            // 开标签：入栈保存进入前样式，再借 HAP 解析属性效果（style/href；残缺片段无子节点）
            htmlStack.Push(style);
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(tag);
            foreach (HtmlAgilityPack.HtmlNode node in doc.DocumentNode.ChildNodes) {
                if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Element) {
                    style = ApplyOpenTag(node, style, out _, out _);
                }
            }
            return style;
        }

        static readonly Regex s_htmlTagNameRegex = new("^<\\s*([a-zA-Z][a-zA-Z0-9]*)", RegexOptions.Compiled);

        static bool IsVoidHtmlTag(string tag) {
            if (tag.EndsWith("/>", StringComparison.Ordinal)) {
                return true;
            }
            Match match = s_htmlTagNameRegex.Match(tag);
            return match.Success && match.Groups[1].Value.ToLowerInvariant() is "br" or "hr" or "img" or "wbr";
        }

        static void ParseHtmlChildren(HtmlAgilityPack.HtmlNode node, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, int depth, Color? forceColor = null, Color? background = null) {
            int firstNew = spans.Count;
            foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes) {
                ParseHtmlNode(child, spans, style, defaultColor, depth + 1);
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

        // 左边界 lookbehind：防止 border-color/link-color 等属性内的 color 子串被误当文字色
        static readonly Regex s_styleColorRegex = new("(?<![\\w-])(?<name>color|background-color)\\s*:\\s*(?<value>[^;]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            if (value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase)) {
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
                    && !TryParseAlpha(parts[3].Trim(), out aa)) {
                    return false;
                }
                color = new Color(rr, gg, bb, aa);
                return true;
            }
            return false;
        }

        // CSS alpha 两种写法：整数 0-255 与小数 0.0-1.0（rgba(1,2,3,0.5) 最常见）
        static bool TryParseAlpha(string text, out int alpha) {
            if (int.TryParse(text, out alpha)) {
                return true;
            }
            if (float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fraction)) {
                alpha = (int)MathF.Round(MathUtils.Clamp(fraction, 0f, 1f) * 255f);
                return true;
            }
            alpha = 255;
            return false;
        }
    }
}
