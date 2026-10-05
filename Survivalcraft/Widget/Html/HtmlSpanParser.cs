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

        public static void ParseHtmlNode(HtmlAgilityPack.HtmlNode node, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, int depth = 0, Color? linkColor = null) {
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
            // 全局 hidden 属性：任意值（含空串）都隐藏，整元素连子树跳过（HTML 标准）
            if (node.GetAttributeValue("hidden", null) != null) {
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
                MarkdownSpanStyle qInner = ApplyOpenTag(node, style, out _, out _, linkColor);
                ParseHtmlChildren(node, spans, qInner, defaultColor, depth, linkColor: linkColor);
                AddText("\u201D", spans, style, defaultColor);
                return;
            }
            MarkdownSpanStyle inner = ApplyOpenTag(node, style, out Color? background, out Color? forceColor, linkColor);
            ParseHtmlChildren(node, spans, inner, defaultColor, depth, forceColor, background, linkColor);
        }

        // 单个开标签对样式上下文的累积效果；background/forceColor 需后刷到生成的 span（code/span 场景）
        // linkColor：body link= 属性的链接色覆写（null=用调色板默认）；Markdown 行内路径不传，
        // 保持调色板口径
        static MarkdownSpanStyle ApplyOpenTag(HtmlAgilityPack.HtmlNode node, MarkdownSpanStyle style, out Color? background, out Color? forceColor, Color? linkColor = null) {
            background = null;
            forceColor = null;
            MarkdownSpanStyle inner = style;
            // 进入时字号累计（UA 标签 small/big/sub/sup/font 置位前的父级口径）：同元素上作者
            // font-size 声明按 CSS 级联整体替换标签置位，% / em 以父级为基准重算
            float inheritedScale = inner.Scale;
            // border-* style 对所有行内标签生效（b/i/a/code 与 span 同享；未知标签透传分支同口径）。
            // 嵌套带边框标签：内层替换外层（同片段只画一框），与嵌套 color 覆盖同语义
            if (CssStyleParser.TryGetBorder(node.GetAttributeValue("style", null), out CssStyleParser.CssBorderSpec borderSpec)) {
                inner.Border = new SpanBorder {
                    Widths = borderSpec.Widths,
                    Left = borderSpec.Left,
                    Top = borderSpec.Top,
                    Right = borderSpec.Right,
                    Bottom = borderSpec.Bottom
                };
            }
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
                    inner.Color = linkColor ?? RichContentPalette.LinkColor;
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
            // 行内 CSS 字体/装饰族：对所有行内标签生效。置于标签 switch 后——显式 normal 值
            // 撤销 b/u 的标签置位（作者样式>UA 样式，CSS 级联口径）
            ApplyInlineStyle(node.GetAttributeValue("style", null), ref inner, inheritedScale);
            return inner;
        }

        // 行内 style 声明 → 字体/装饰族样式累积（ref：显式 normal/baseline 0 需覆盖继承值）。
        // inheritedScale=进入本元素时的父级字号累计。各属性独立消费：非法值仅丢该条
        //（不污染其他属性，也不反噬——无效 font-weight/text-transform 保留继承值，CSS 口径）
        static void ApplyInlineStyle(string styleAttribute, ref MarkdownSpanStyle style, float inheritedScale) {
            if (string.IsNullOrEmpty(styleAttribute)) {
                return;
            }
            // font-size：px/pt（÷16 基准，同 font size 属性 16px medium 口径）、%与 em（父字号
            // 倍率）、rem（根字号=px 口径）。基准=inheritedScale：同元素上作者声明整体替换
            // UA 标签置位（<small style="font-size:24px">=1.5 非 0.8×1.5，CSS 级联）；子元素
            // 继承的累计值经 inheritedScale 传导，跨标签嵌套仍累乘。负值/0/未知单位无效
            if (CssStyleParser.TryGetDeclaration(styleAttribute, "font-size", out string fontSize)
                && TryParseCssFontSize(fontSize, out float sizeRatio)) {
                style.Scale = (inheritedScale > 0f ? inheritedScale : 1f) * sizeRatio;
            }
            // font-weight：bold/bolder/数值≥600 加粗（双权字库只有"有/无粗体"两档，400 档即
            // normal）；normal/lighter/数值<600 撤销继承粗体；无效值不动（不反噬继承）
            if (CssStyleParser.TryGetDeclaration(styleAttribute, "font-weight", out string fontWeight)
                && ParseFontWeightBold(fontWeight) is bool weightBold) {
                style.Bold = weightBold;
            }
            // font-style：italic/oblique（合成切变）；normal 撤销继承斜体
            if (CssStyleParser.TryGetDeclaration(styleAttribute, "font-style", out string fontStyle)) {
                string f = fontStyle.ToLowerInvariant();
                if (f is "italic" or "oblique") {
                    style.Italic = true;
                }
                else if (f == "normal") {
                    style.Italic = false;
                }
            }
            // text-decoration：多值空格分隔，underline/line-through 叠加；none 双清（可撤销
            // 继承的 u/del）；overline 无对应绘制支持，吞掉忽略
            if (CssStyleParser.TryGetDeclaration(styleAttribute, "text-decoration", out string textDecoration)) {
                bool sawNone = false;
                foreach (string token in textDecoration.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
                    switch (token.ToLowerInvariant()) {
                        case "underline":
                            style.Underline = true;
                            break;
                        case "line-through":
                            style.Strikethrough = true;
                            break;
                        case "none":
                            sawNone = true;
                            break;
                    }
                }
                if (sawNone) {
                    style.Underline = false;
                    style.Strikethrough = false;
                    style.LinkUnderlineSuppressed = true;
                }
            }
            // vertical-align：sub/super/middle 三关键字，比率同 sub/sup 标签（嵌套累加语义一致；
            // em 基准=父字号，绘制期换算）。长度值不支持，忽略
            if (CssStyleParser.TryGetDeclaration(styleAttribute, "vertical-align", out string verticalAlign)) {
                style.BaselineShift += verticalAlign.ToLowerInvariant() switch {
                    "sub" => 0.25f,
                    "super" => -0.33f,
                    "middle" => -0.25f,
                    _ => 0f
                };
            }
            // text-transform：枚举直写（struct 复制天然随嵌套继承，显式 none 可撤销继承变换）；
            // 无效值不动（保留继承变换）
            if (CssStyleParser.TryGetDeclaration(styleAttribute, "text-transform", out string textTransform)) {
                TextTransform? transform = textTransform.ToLowerInvariant() switch {
                    "uppercase" => TextTransform.Uppercase,
                    "lowercase" => TextTransform.Lowercase,
                    "capitalize" => TextTransform.Capitalize,
                    "none" => TextTransform.None,
                    _ => null
                };
                if (transform.HasValue) {
                    style.TextTransform = transform.Value;
                }
            }
            // opacity：number/% 值域（TryParseCssOpacity，CSS 口径负值/>1 钳 [0,1]、垃圾值整条忽略）。
            // 置位=replace 语义：v==1 也置（显式 1 撤销继承的 <1，作者声明胜继承的 CSS 级联口径）；
            // 嵌套再声明不叠乘——叠乘只发生在块级 ctx（HtmlToWidgetsBuilder opacityCtx）
            if (CssStyleParser.TryGetDeclaration(styleAttribute, "opacity", out string opacityDecl)
                && TryParseCssOpacity(opacityDecl, out float opacityValue)) {
                style.HasOpacity = true;
                style.Opacity = opacityValue;
            }
        }

        // opacity 值解析（行内 ApplyInlineStyle 与块级 builder 共用）：number 直取、%（÷100）；
        // 可解析即钳 [0,1]（CSS computed-value 口径：opacity:-0.5→0、2→1、150%→1）；解析失败=
        // 垃圾值整条丢弃（返回 false，调用方不动继承值）
        public static bool TryParseCssOpacity(string value, out float opacity) {
            string v = value.Trim();
            bool percent = v.EndsWith("%", StringComparison.Ordinal);
            if (percent) {
                v = v[..^1];
            }
            if (!float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float raw)
                || !float.IsFinite(raw)) {
                // NaN/Infinity 随 TryParse 穿透（NumberStyles.Float 不拒），Clamp 对 NaN 直通——
                // 显式拦（CSS 无效声明丢弃口径）
                opacity = 1f;
                return false;
            }
            opacity = MathUtils.Clamp(percent ? raw / 100f : raw, 0f, 1f);
            return true;
        }

        // font-size 值：px（÷16 基准）、pt（1pt=4/3px）、%（÷100）、em（原值）、rem（=px 口径，
        // 根字号基准 16px）。输出=相对默认字号倍率，钳 [0.25, 8] 防御敌意帖子（超大倍率逐字
        // 巨 quad 拖垮排版）。负值/0/未知单位无效返回 false
        static bool TryParseCssFontSize(string value, out float ratio) {
            ratio = 1f;
            string v = value.Trim().ToLowerInvariant();
            float scale;
            if (v.EndsWith("px", StringComparison.Ordinal)) {
                if (!float.TryParse(v.AsSpan(0, v.Length - 2), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out scale)
                    || scale <= 0f) {
                    return false;
                }
                scale /= 16f;
            }
            else if (v.EndsWith("pt", StringComparison.Ordinal)) {
                if (!float.TryParse(v.AsSpan(0, v.Length - 2), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out scale)
                    || scale <= 0f) {
                    return false;
                }
                scale /= 12f;
            }
            else if (v.EndsWith("%", StringComparison.Ordinal)) {
                if (!float.TryParse(v.AsSpan(0, v.Length - 1), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out scale)
                    || scale <= 0f) {
                    return false;
                }
                scale /= 100f;
            }
            else if (v.EndsWith("rem", StringComparison.Ordinal)) {
                if (!float.TryParse(v.AsSpan(0, v.Length - 3), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out scale)
                    || scale <= 0f) {
                    return false;
                }
            }
            else if (v.EndsWith("em", StringComparison.Ordinal)) {
                if (!float.TryParse(v.AsSpan(0, v.Length - 2), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out scale)
                    || scale <= 0f) {
                    return false;
                }
            }
            else {
                // 无单位数值非法（CSS font-size 不收 number）
                return false;
            }
            ratio = MathUtils.Clamp(scale, 0.25f, 8f);
            return true;
        }

        // font-weight 值 → 有/无粗体：bold/bolder 与数值 ≥600 加粗；normal/lighter 与数值 <600
        // 撤销；无效值返回 null（调用方不动继承值——不反噬 <b> 的标签置位，CSS 口径）。
        // 粗体是合成双绘，无多字重字库，600 阈值即浏览器 semibold 起粗的近似
        static bool? ParseFontWeightBold(string value) {
            string v = value.Trim().ToLowerInvariant();
            if (v is "bold" or "bolder") {
                return true;
            }
            if (v is "normal" or "lighter") {
                return false;
            }
            if (float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float weight)) {
                return weight >= 600f;
            }
            return null;
        }

        // 行内上下文的零散 HtmlInline：Markdig 逐标签分发、无配对信息，逐项喂 HAP 会被自动闭合成空节点
        // （"<b>" 无子节点、"</b>" 被丢弃），样式全静默失效——用标签栈把开/闭标签串起来。
        // 局限留档：此路径不识别全局 hidden（无 DOM 子树、需跨开/闭标签携带抑制态，成本高），
        // MD 行内 <span hidden> 照常渲染；hidden 拦截只在 DOM 路径（ParseHtmlNode/BuildHtmlNodes）生效
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

        static void ParseHtmlChildren(HtmlAgilityPack.HtmlNode node, List<TextSpan> spans, MarkdownSpanStyle style, Color defaultColor, int depth, Color? forceColor = null, Color? background = null, Color? linkColor = null) {
            int firstNew = spans.Count;
            foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes) {
                ParseHtmlNode(child, spans, style, defaultColor, depth + 1, linkColor);
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
            // text-transform 出口施加：变换后的文本进排版（测宽/换行按变换后字形算，浏览器同款）
            text = ApplyTextTransform(text, style.TextTransform);
            // 粗斜体并存时用 BoldItalic（原三目链 Bold 优先会丢斜体）
            RichTextStyle richStyle = style.Bold
                ? (style.Italic ? RichTextStyle.BoldItalic : RichTextStyle.Bold)
                : style.Italic ? RichTextStyle.Italic : RichTextStyle.Normal;
            spans.Add(new TextSpan(text, style.Color ?? defaultColor, richStyle, style.Href) {
                BackgroundColor = style.Background,
                Strikethrough = style.Strikethrough,
                FontScale = style.Scale,
                Underline = style.Underline,
                BaselineShift = style.BaselineShift,
                Border = style.Border,
                LinkUnderlineSuppressed = style.LinkUnderlineSuppressed,
                // 行内 opacity 透传 span（未声明=1f）：Draw 期与宿主 ColorTransform 叠乘淡出
                Opacity = style.HasOpacity ? style.Opacity : 1f
            });
        }

        // text-transform 施加：Capitalize=每词首字母大写、其余原样（CSS 口径，不做
        // ToTitleCase 的"其余转小写"）；词边界=前一字符非字母
        static string ApplyTextTransform(string text, TextTransform transform) {
            if (transform == TextTransform.None) {
                return text;
            }
            if (transform == TextTransform.Uppercase) {
                return text.ToUpperInvariant();
            }
            if (transform == TextTransform.Lowercase) {
                return text.ToLowerInvariant();
            }
            char[] chars = text.ToCharArray();
            bool atWordStart = true;
            for (int i = 0; i < chars.Length; i++) {
                if (char.IsLetter(chars[i])) {
                    if (atWordStart) {
                        chars[i] = char.ToUpperInvariant(chars[i]);
                        atWordStart = false;
                    }
                }
                else {
                    atWordStart = true;
                }
            }
            return new string(chars);
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
