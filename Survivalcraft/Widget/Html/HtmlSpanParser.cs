using System.Net;
using System.Text.RegularExpressions;
using Engine;

namespace Game {
    // HTML DOM/片段 → List<TextSpan>。供 HtmlToWidgetsBuilder（块级 DOM 遍历）与
    // MarkdownSpanParser（Markdig 行内 HtmlInline 标签栈）共用。
    // 样式上下文类型 MarkdownSpanStyle 是双解析器契约，定义在 MarkdownSpanParser.cs
    public static class HtmlSpanParser {
        // 行内嵌套深度上限：防御不可信输入的深嵌套（StackOverflowException 不可捕获，直接崩进程）
        const int MaxDepth = 64;

        // HTML 片段 → span（块级 HTML 完整 DOM 场景）。行内上下文的零散标签走 ApplyInlineHtmlTag 标签栈
        public static void ParseFragment(string html, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, int depth) {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(html ?? string.Empty);
            foreach (HtmlAgilityPack.HtmlNode node in doc.DocumentNode.ChildNodes) {
                ParseHtmlNode(node, spans, style, defaultColor, depth + 1);
            }
        }

        public static void ParseHtmlNode(HtmlAgilityPack.HtmlNode node, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, int depth = 0) {
            switch (node.NodeType) {
                case HtmlAgilityPack.HtmlNodeType.Text:
                    // HTML 空白语义：元素间源码换行/缩进是排版噪声，折叠为单空格。
                    // 不 trim（行中 "a <b>b</b>" 的尾空格保衔接），段首空白由块级 Flush 清理
                    AddText(CollapseWhitespace(WebUtility.HtmlDecode(node.InnerText)), spans, style, defaultColor);
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
            if (tag == "img") {
                // 图片升级为图片块由块级遍历拦截（AppendInlineChildren 等）；span 透传路径
                //（表格 cell、未知标签内）无图片槽，产占位文本（色同 Markdown 行内图占位），不再静默丢
                spans.Add(new TextSpan("[图片:" + node.GetAttributeValue("alt", string.Empty) + "]", new Color(160, 160, 168)));
                return;
            }
            // script/style 体的源码不是可见内容（HasVisibleText 同口径），span 透传会把它渲染成段落
            if (tag is "script" or "style") {
                return;
            }
            if (tag == "q") {
                // <q>：内容前后补英文弯引号（HTML 默认 content 引号随 lang 变，这里固定 “”；
                // 嵌套 q 不换引号层级——用户裁定简单做）
                AddText("\u201C", spans, style, defaultColor);
                MarkdownSpanStyle qInner = ApplyOpenTag(node, style, out _, out _);
                ParseHtmlChildren(node, spans, qInner, defaultColor, depth);
                AddText("\u201D", spans, style, defaultColor);
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
                case "del":
                case "s":
                case "strike":
                    inner.Strikethrough = true;
                    break;
                case "ins":
                case "u":
                    // u 与 ins 视觉同效（CSS 默认 text-decoration: underline）
                    inner.Underline = true;
                    break;
                case "small":
                    // CSS smaller：相对父级累乘（small>small 连缩）
                    inner.Scale = (inner.Scale > 0f ? inner.Scale : 1f) * 0.8f;
                    break;
                case "sub":
                case "sup": {
                    // 字号同 CSS smaller 惯例 0.833；基线偏移取浏览器近似（super ≈ 升 0.33em、sub ≈ 降 0.25em），
                    // 嵌套累加；em 基准=父字号（绘制期 DefaultFontScale）
                    inner.Scale = (inner.Scale > 0f ? inner.Scale : 1f) * 0.833f;
                    inner.BaselineShift += node.Name.Equals("sub", StringComparison.OrdinalIgnoreCase) ? 0.25f : -0.33f;
                    break;
                }
                case "a":
                    inner.Href = node.GetAttributeValue("href", null);
                    inner.Color = RichContentPalette.LinkColor;
                    break;
                case "code":
                case "samp":
                    // samp（程序输出样本）与 code 视觉同效
                    forceColor = RichContentPalette.CodeColor;
                    background = RichContentPalette.CodeBackgroundColor;
                    // 标签栈路径（行内 HTML）没有 DOM 后刷机制，色与底色都直接挂进样式上下文
                    inner.Color = RichContentPalette.CodeColor;
                    inner.Background = RichContentPalette.CodeBackgroundColor;
                    break;
                case "kbd":
                    forceColor = RichContentPalette.KbdColor;
                    background = RichContentPalette.KbdBackgroundColor;
                    inner.Color = RichContentPalette.KbdColor;
                    inner.Background = RichContentPalette.KbdBackgroundColor;
                    break;
                case "mark":
                    forceColor = RichContentPalette.MarkColor;
                    background = RichContentPalette.MarkBackgroundColor;
                    inner.Color = RichContentPalette.MarkColor;
                    inner.Background = RichContentPalette.MarkBackgroundColor;
                    break;
                case "big":
                    // big/small 对：用户裁定 ×1.2（CSS 惯例 1.25 弃用），嵌套累乘同 small
                    inner.Scale = (inner.Scale > 0f ? inner.Scale : 1f) * 1.2f;
                    break;
                case "var":
                case "address":
                    // var（变量）与 address（联系地址）HTML 默认渲染同为斜体
                    inner.Italic = true;
                    break;
                case "font": {
                    // 旧版表现属性三件套：color（CSS 色值全口径）/size（1-7 与 ±N 相对）/face（全仓
                    // 唯一位图字体 Pericles，无字库可换，静默忽略）。color 直写样式上下文——套在 <a>
                    // 内时覆盖链接色，浏览器同款（表现属性提示胜过 UA 链接色）
                    if (CssColorParser.TryParse(node.GetAttributeValue("color", null), out Color fontColor)) {
                        inner.Color = fontColor;
                    }
                    if (TryParseFontSizeAttribute(node.GetAttributeValue("size", null), out float sizeRatio)) {
                        inner.Scale = (inner.Scale > 0f ? inner.Scale : 1f) * sizeRatio;
                    }
                    break;
                }
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
        public static MarkdownSpanStyle ApplyInlineHtmlTag(string tag, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, Stack<MarkdownSpanStyle> htmlStack, int depth) {
            if (string.IsNullOrEmpty(tag)) {
                return style;
            }
            if (tag.StartsWith("</", StringComparison.Ordinal)) {
                // 闭标签：出栈恢复进入前样式；无配对开标签时保持当前样式
                MarkdownSpanStyle restored = htmlStack.Count > 0 ? htmlStack.Pop() : style;
                Match closeName = s_htmlCloseTagNameRegex.Match(tag);
                if (closeName.Success
                    && closeName.Groups[1].Value.ToLowerInvariant() == "q") {
                    // <q> 闭标签：补尾引号（用弹出后的进入前 style，不吃 q 内容样式——与 DOM 路径对齐；
                    // 此处 style 已是 q 内部值，<q style="color:red"> 的闭引号会误染红）
                    AddText("\u201D", spans, restored, defaultColor);
                }
                return restored;
            }
            // void 元素与自闭合写法没有配对闭标签，按完整片段当场生效（<br>→换行等）
            if (IsVoidHtmlTag(tag)) {
                ParseFragment(tag, spans, style, defaultColor, depth + 1);
                return style;
            }
            // 开标签：q 先补头引号；入栈保存进入前样式，再借 HAP 解析属性效果（style/href；残缺片段无子节点）
            Match openName = s_htmlTagNameRegex.Match(tag);
            if (openName.Success
                && openName.Groups[1].Value.ToLowerInvariant() == "q") {
                AddText("\u201C", spans, style, defaultColor);
            }
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

        static readonly Regex s_htmlCloseTagNameRegex = new("^</\\s*([a-zA-Z][a-zA-Z0-9]*)", RegexOptions.Compiled);

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
                    // 前景色跳过链接（浏览器 UA 样式：kbd/code/mark 内的链接保持链接色）；底色照刷
                    if (forceColor != null
                        && spans[i].Href == null) {
                        spans[i].Color = forceColor;
                    }
                    spans[i].BackgroundColor = background;
                }
            }
        }

        // 样式上下文 → TextSpan。双解析器共用的 span 构造出口（粗斜体并存组合、颜色/链接/底色/删除线）
        public static void AddText(string text, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor) {
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            // 粗斜体并存时用 BoldItalic（原三目链 Bold 优先会丢斜体）
            RichTextStyle richStyle = style.Bold
                ? (style.Italic ? RichTextStyle.BoldItalic : RichTextStyle.Bold)
                : style.Italic ? RichTextStyle.Italic : RichTextStyle.Normal;
            spans.Add(new TextSpan(text, style.Color ?? defaultColor, richStyle, style.Href) {
                BackgroundColor = style.Background,
                Strikethrough = style.Strikethrough,
                FontScale = style.Scale,
                Underline = style.Underline,
                BaselineShift = style.BaselineShift
            });
        }

        static readonly Regex s_whitespaceRegex = new("[^\\S\u00A0]+", RegexOptions.Compiled);

        // DOM 文本节点空白规范化：连续空白（含换行/缩进）折叠为单空格。
        // 行首 \\n 不处理会让 RichTextLayout 整段 runs 清空（真实帖子 HTML 多行格式必踩）
        static string CollapseWhitespace(string text) {
            return string.IsNullOrEmpty(text) ? text : s_whitespaceRegex.Replace(text, " ");
        }

        // 段首空白清理：剥掉首个 span 的前导空白（&nbsp; 不算——排版意图，帖子常用它做首行缩进，
        // 被 TrimStart 整体吃掉会丢），纯空白 span 整个移除，直至遇到非空白开头。
        // 块级 Flush 前调用——折叠后的元素间空白落在段首时不应渲染（会缩进首行）
        public static void TrimLeadingWhitespace(List<TextSpan> spans) {
            while (spans.Count > 0) {
                TextSpan first = spans[0];
                string text = first.Text;
                int i = 0;
                while (i < text.Length
                    && char.IsWhiteSpace(text[i])
                    && text[i] != '\u00A0') {
                    i++;
                }
                if (i >= text.Length) {
                    spans.RemoveAt(0);
                    continue;
                }
                if (i > 0) {
                    first.Text = text[i..];
                }
                break;
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

        // font size 属性倍率表：Chrome 16px medium 基准的绝对字号映射（10/13/16/18/24/32/48px ÷ 16）
    static readonly float[] s_fontSizeRatios = [0.625f, 0.8125f, 1f, 1.125f, 1.5f, 2f, 3f];

    // font size 属性：绝对 1-7 或 ±N 相对（"+2"/"-1"，int.TryParse 吞掉前导符号得 2/-1）。
    // 相对值以基准 3 折算后钳 1-7（HTML 规范：不随嵌套累加，浏览器同款）；绝对越界（0/8+）
    // 规范规定无效果——返回 false 不缩放
    static bool TryParseFontSizeAttribute(string attribute, out float ratio) {
        ratio = 1f;
        if (string.IsNullOrWhiteSpace(attribute)) {
            return false;
        }
        string trimmed = attribute.Trim();
        if (trimmed[0] is '+' or '-') {
            if (!int.TryParse(trimmed, out int relative)) {
                return false;
            }
            ratio = s_fontSizeRatios[MathUtils.Clamp(3 + relative, 1, 7) - 1];
            return true;
        }
        if (!int.TryParse(trimmed, out int absolute)
            || absolute is < 1 or > 7) {
            return false;
        }
        ratio = s_fontSizeRatios[absolute - 1];
        return true;
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
