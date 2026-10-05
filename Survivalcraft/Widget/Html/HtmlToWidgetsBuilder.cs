using Engine;
using Engine.Graphics;

namespace Game {
    // HtmlAgilityPack DOM → widget 树（块级遍历）。外部 HTML 片段入口（P2：HtmlWidget 壳、
    // CommunityPostDetailsDialog 正文）；MarkdownToWidgetsBuilder 的 HtmlBlock 分支也转发到此，
    // 产出的块注册表（CodeBlocks/ImageBlocks）由转发方合并回自己的集合供宿主测量期回填宽度。
    // 行内内容走 HtmlSpanParser
    public class HtmlToWidgetsBuilder : RichContentBuilder {
        // body bgcolor 根回退（cell>row>table>body 链最外级）：表格格/行/表均无 bgcolor 时回退到此。
        // 构建入口重置
        Color? m_bodyBackground;

        // body text=/link=：整页默认文字色/链接色（老帖源码常带）。vlink/alink 无访问状态追踪，
        // 静默忽略。构建入口重置
        Color? m_bodyTextColor;

        Color? m_bodyLinkColor;

        // HTML 片段入口：解析为 DOM 后做块级遍历，块收进自建面板返回（HtmlWidget 壳用）
        public StackPanelWidget BuildHtml(string html, int listDepth, Color? colorOverride, int depth) {
            StackPanelWidget panel = CreatePanel();
            BuildHtmlInto(html, panel, listDepth, colorOverride, depth);
            return panel;
        }

        // 平铺入口：块直接追加进调用方面板（Markdown HtmlBlock 转发用，避免多包一层嵌套面板）
        public void BuildHtmlInto(string html, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
            m_bodyBackground = null;
            m_bodyTextColor = null;
            m_bodyLinkColor = null;
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(html ?? string.Empty);
            BuildHtmlNodes(doc.DocumentNode.ChildNodes, panel, listDepth, colorOverride, depth);
        }

        // 默认文字色口径：显式 colorOverride > body text= > 调色板 DefaultColor
        Color EffectiveTextColor(Color? colorOverride) => colorOverride ?? m_bodyTextColor ?? DefaultColor;

        // base.CreateHeading 走虚分发回到这里：标题色由 HeadingColor 固定（产品配色，非 body
        // text= 语义——浏览器标题继承 body 色是另一口径，留档不跟）
        protected override RichTextWidget CreateRichText(Color? colorOverride) {
            RichTextWidget widget = base.CreateRichText(colorOverride);
            if (colorOverride == null && m_bodyTextColor.HasValue) {
                widget.Color = m_bodyTextColor.Value;
            }
            return widget;
        }

        // style 盒属性（padding/margin）壳方案：带声明时包一层透明 StackPanel 壳——壳 Margin=CSS
        // margin（壳外距），子件四边距=CSS padding（壳内缘到内容）。浏览器盒子模型同构：border 波
        // 边框画在壳矩形上时 padding 天然落框内。无相关声明返回 null（零开销直通）。
        // 单件版：叶子块（标题/引用/代码块/hr/表格/列表整体）
        Widget WrapInStyleBox(HtmlAgilityPack.HtmlNode node, Widget widget) {
            string style = node.GetAttributeValue("style", null);
            bool hasMargin = CssStyleParser.TryGetBox(style, "margin", out Vector4 margin);
            bool hasPadding = CssStyleParser.TryGetBox(style, "padding", out Vector4 padding);
            if (!hasMargin && !hasPadding) {
                return null;
            }
            ApplyBoxEdges(widget, padding, applyTop: true, applyBottom: true);
            StackPanelWidget shell = CreatePanel();
            if (hasMargin) {
                shell.Margin4 = margin;
            }
            // 对齐移交：壳顶替子件原外层对齐位，子件改 Stretch 填壳（Near/Center/Stretch 视觉不变）
            shell.HorizontalAlignment = widget.HorizontalAlignment;
            widget.HorizontalAlignment = WidgetAlignment.Stretch;
            shell.Children.Add(widget);
            return shell;
        }

        // 容器版开头：子树重入目标换成壳（返回 false=无声明直通）。padding.Left 同步入
        // CurrentIndentX——嵌套代码块/图片块构建宽与 shell 注册 IndentX 按左 pad 收窄（右 pad
        // 不进回填：IndentX 只有左语义，格内/引用内嵌套壳的右 pad 回填留档不做）
        bool TryBeginStyleBox(HtmlAgilityPack.HtmlNode node, out StackPanelWidget shell, out Vector4 padding) {
            string style = node.GetAttributeValue("style", null);
            bool hasMargin = CssStyleParser.TryGetBox(style, "margin", out Vector4 margin);
            bool hasPadding = CssStyleParser.TryGetBox(style, "padding", out padding);
            if (!hasMargin && !hasPadding) {
                shell = null;
                return false;
            }
            shell = CreatePanel();
            if (hasMargin) {
                shell.Margin4 = margin;
            }
            // 对齐移交：容器壳恒 Stretch 顶替子件原外层对齐位（原直挂 host panel 的子件在
            // center/align 容器里按宿主拉伸排满；Stretch 只影响 arrange 不改 measure，壳内
            // Near 子件视觉不变——审查 P1：Near 壳会把居中/撑满塌成内容宽左对齐）
            shell.HorizontalAlignment = WidgetAlignment.Stretch;
            CurrentIndentX += padding.X;
            return true;
        }

        // 容器壳收尾：padding 铺给直接子件（左右=每件、首件补上、末件补下——CSS 容器 padding
        // 口径，内容宽同步收窄）；空壳（纯空白子树）不产部件返回 false
        bool EndStyleBox(StackPanelWidget shell, Vector4 padding) {
            CurrentIndentX -= padding.X;
            if (shell.Children.Count == 0) {
                return false;
            }
            for (int i = 0; i < shell.Children.Count; i++) {
                ApplyBoxEdges(shell.Children[i], padding, applyTop: i == 0, applyBottom: i == shell.Children.Count - 1);
            }
            return true;
        }

        // 盒四边铺到子件四边距（+=：与子件已有装饰 margin 线性叠加，作者值不覆盖引擎值）
        static void ApplyBoxEdges(Widget widget, Vector4 edges, bool applyTop, bool applyBottom) {
            if (edges.X != 0f) {
                widget.MarginLeft += edges.X;
            }
            if (edges.Y != 0f && applyTop) {
                widget.MarginTop += edges.Y;
            }
            if (edges.Z != 0f) {
                widget.MarginRight += edges.Z;
            }
            if (edges.W != 0f && applyBottom) {
                widget.MarginBottom += edges.W;
            }
        }

        // 块级 HTML 节点遍历：行内节点累积成段，遇块级标签先冲刷当前段。
        // 参数放宽 IEnumerable：li 内块级子节点以单节点数组重入（复用顶层分发口径）。
        // blockAlign：容器块级文本对齐（center/figure/figcaption = HorizontalCenter，div/p/h 的
        // align 属性可再覆写），TextAnchor.Default=继承外层——全部重入点（div/dl/列表/引用/表格格/
        // 未知容器）透传（Chrome text-align 继承口径；显式 left 属性映射 Default=Left 同值，天然覆盖）
        void BuildHtmlNodes(IEnumerable<HtmlAgilityPack.HtmlNode> nodes, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth, TextAnchor blockAlign = TextAnchor.Default) {
            if (depth > MaxDepth) {
                return;
            }
            List<TextSpan> pendingSpans = [];
            // 行内段冲刷目标：带 style 盒属性的段落类标签（p/dt/summary/footer 行内形态）重定向
            // 进壳，其余场景恒等于 panel（壳方案 WrapInStyleBox 只覆盖单件块，段落壳在此换目标）
            StackPanelWidget flowPanel = panel;
            void Flush(TextAnchor? alignOverride = null) {
                TextAnchor effectiveAlign = alignOverride ?? blockAlign;
                if (pendingSpans.Count > 0) {
                    HtmlSpanParser.TrimLeadingWhitespace(pendingSpans);
                    if (pendingSpans.Count == 0) {
                        return;
                    }
                    RichTextWidget widget = CreateRichText(colorOverride);
                    if (effectiveAlign != TextAnchor.Default) {
                        widget.HorizontalAlignment = WidgetAlignment.Stretch;
                        widget.TextAnchor = effectiveAlign;
                    }
                    widget.Spans.AddRange(pendingSpans);
                    flowPanel.Children.Add(widget);
                    pendingSpans.Clear();
                }
            }
            // 行内上下文透传：子节点里的 <img> 升级为图片块（富文本编辑器帖图最常见 <p><img> 包裹，
            // img 无文本子节点会被 span 解析静默丢弃），其余照常 span 累积
            void AppendInlineChildren(HtmlAgilityPack.HtmlNode node, Color? inlineColorOverride) {
                foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes) {
                    if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                        && child.Name.Equals("img", StringComparison.OrdinalIgnoreCase)
                        && ImageLoader != null) {
                        Flush();
                        flowPanel.Children.Add(AddImageBlockWithAttributes(flowPanel, child));
                        continue;
                    }
                    HtmlSpanParser.ParseHtmlNode(child, pendingSpans, default, EffectiveTextColor(inlineColorOverride), linkColor: m_bodyLinkColor);
                }
            }
            foreach (HtmlAgilityPack.HtmlNode node in nodes) {
                if (node.NodeType != HtmlAgilityPack.HtmlNodeType.Element
                    && node.NodeType != HtmlAgilityPack.HtmlNodeType.Text) {
                    continue;
                }
                if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Text) {
                    HtmlSpanParser.ParseHtmlNode(node, pendingSpans, default, EffectiveTextColor(colorOverride));
                    continue;
                }
                // 全局 hidden 属性：任意值（含空串）都隐藏，块级元素连子树跳过（行内路径在
                // HtmlSpanParser.ParseHtmlNode 拦）
                if (node.GetAttributeValue("hidden", null) != null) {
                    continue;
                }
                switch (node.Name.ToLowerInvariant()) {
                    case "script":
                    case "style":
                        // 体源码非可见内容（HasVisibleText 同口径），块级直挂场景兜底跳过
                        Flush();
                        break;
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6": {
                        Flush();
                        RichTextWidget widget = CreateHeading(node.Name[1] - '0', colorOverride);
                        TextAnchor headingAlign = ParseBlockAlignment(node) ?? blockAlign;
                        if (headingAlign != TextAnchor.Default) {
                            widget.HorizontalAlignment = WidgetAlignment.Stretch;
                            widget.TextAnchor = headingAlign;
                        }
                        HtmlSpanParser.ParseHtmlNode(node, widget.Spans, default, widget.Color, linkColor: m_bodyLinkColor);
                        panel.Children.Add(WrapInStyleBox(node, widget) ?? widget);
                        break;
                    }
                    case "p": {
                        // p align 属性：本段对齐覆写继承（CSS text-align 元素级口径）；
                        // style 盒属性：段落壳（行内内容+升级 img 块整体入壳）
                        Flush();
                        bool styledPara = TryBeginStyleBox(node, out StackPanelWidget paraShell, out Vector4 paraPad);
                        if (styledPara) {
                            flowPanel = paraShell;
                        }
                        AppendInlineChildren(node, colorOverride);
                        Flush(ParseBlockAlignment(node));
                        if (styledPara) {
                            flowPanel = panel;
                            if (EndStyleBox(paraShell, paraPad)) {
                                panel.Children.Add(paraShell);
                            }
                        }
                        break;
                    }
                    case "div": {
                        // 容器标签递归块级遍历（子级 p/pre/列表各归其位），不做 span 平铺——
                        // 否则 div 内 pre 被当普通元素折叠，代码缩进/换行全毁；align 属性覆写继承。
                        // style 盒属性：子树入壳（容器壳，padding 铺直接子件）
                        Flush();
                        bool styledDiv = TryBeginStyleBox(node, out StackPanelWidget divShell, out Vector4 divPad);
                        BuildHtmlNodes(node.ChildNodes, styledDiv ? divShell : panel, listDepth, colorOverride, depth + 1, ParseBlockAlignment(node) ?? blockAlign);
                        if (styledDiv && EndStyleBox(divShell, divPad)) {
                            panel.Children.Add(divShell);
                        }
                        break;
                    }
                    case "html":
                    case "body": {
                        // 整页包装标签当 div（老帖源码常带 html/body 包裹，浏览器剥离外壳直排内容）；
                        // body bgcolor 作表格 bgcolor 回退链根，text=/link= 作默认文字/链接色
                        Flush();
                        if (node.Name.Equals("body", StringComparison.OrdinalIgnoreCase)) {
                            if (CssColorParser.TryParse(node.GetAttributeValue("bgcolor", null), out Color bodyBackground)) {
                                m_bodyBackground = bodyBackground;
                            }
                            if (CssColorParser.TryParse(node.GetAttributeValue("text", null), out Color bodyText)) {
                                m_bodyTextColor = bodyText;
                            }
                            if (CssColorParser.TryParse(node.GetAttributeValue("link", null), out Color bodyLink)) {
                                m_bodyLinkColor = bodyLink;
                            }
                        }
                        BuildHtmlNodes(node.ChildNodes, panel, listDepth, colorOverride, depth + 1, ParseBlockAlignment(node) ?? blockAlign);
                        break;
                    }
                    case "ul":
                    case "ol":
                    case "menu":
                        // menu 与 ul 同渲染（HTML 弃用标签，Chrome 按无序列表渲染）
                        Flush();
                        BuildHtmlList(node, node.Name.Equals("ol", StringComparison.Ordinal), panel, listDepth, colorOverride, depth, blockAlign);
                        break;
                    case "blockquote": {
                        Flush();
                        Widget quote = CreateHtmlQuote(node, listDepth, colorOverride, depth, blockAlign);
                        panel.Children.Add(WrapInStyleBox(node, quote) ?? quote);
                        break;
                    }
                    case "pre": {
                        Flush();
                        HtmlAgilityPack.HtmlNode code = node.Element("code") ?? node;
                        Widget codeBlock = CreateCodeBlock(ReplaceBrWithNewline(code).Split('\n'));
                        panel.Children.Add(WrapInStyleBox(node, codeBlock) ?? codeBlock);
                        break;
                    }
                    case "hr": {
                        Flush();
                        Widget thematicBreak = CreateHtmlThematicBreak(node);
                        panel.Children.Add(WrapInStyleBox(node, thematicBreak) ?? thematicBreak);
                        break;
                    }
                    case "img" when ImageLoader != null:
                        // 块级 <img>：与纯图段落同管线。行内 <img>（p 内）仍被丢弃（范围外）
                        Flush();
                        panel.Children.Add(AddImageBlockWithAttributes(panel, node));
                        break;
                    case "table": {
                        Flush();
                        // caption（表题）：表上方居中块渲染（CreateHtmlTable 行收集本就跳过 caption，
                        // 不双渲染）；用户裁定 caption=居中块，与 center/figure 同款
                        HtmlAgilityPack.HtmlNode captionNode = node.Element("caption");
                        if (captionNode != null) {
                            BuildHtmlNodes(captionNode.ChildNodes, panel, listDepth, colorOverride, depth + 1, TextAnchor.HorizontalCenter);
                        }
                        // 空表（无任何行）返回 null 不产部件——零尺寸 widget 仍会靠 Margin 贡献 8px 空白
                        TablePanelWidget table = CreateHtmlTable(node, colorOverride, depth, blockAlign);
                        if (table != null) {
                            panel.Children.Add(WrapInStyleBox(node, table) ?? table);
                        }
                        break;
                    }
                    case "center":
                    case "figure":
                    case "figcaption":
                    case "caption":
                        // 居中容器（center 老帖高频 / figure 图容器 / figcaption 题注 / caption 表题，
                        // 用户裁定同款居中块）：块边界+子节点重入，HorizontalCenter 贯穿段/标题行盒居中
                        // （Chrome text-align 语义，覆写外层继承）；table 直下 caption 已在 table case
                        // 预提取，此处兜 table 外裸 caption。style 盒属性：子树入壳
                        Flush();
                        bool styledCenter = TryBeginStyleBox(node, out StackPanelWidget centerShell, out Vector4 centerPad);
                        BuildHtmlNodes(node.ChildNodes, styledCenter ? centerShell : panel, listDepth, colorOverride, depth + 1, TextAnchor.HorizontalCenter);
                        if (styledCenter && EndStyleBox(centerShell, centerPad)) {
                            panel.Children.Add(centerShell);
                        }
                        break;
                    case "dl":
                    case "details":
                        // dl 定义列表容器 / details 折叠容器（无折叠交互，按已展开直排——用户裁定）：
                        // 块边界+子节点重入，dt/dd 各归其位。style 盒属性：子树入壳
                        Flush();
                        bool styledDl = TryBeginStyleBox(node, out StackPanelWidget dlShell, out Vector4 dlPad);
                        BuildHtmlNodes(node.ChildNodes, styledDl ? dlShell : panel, listDepth, colorOverride, depth + 1, blockAlign);
                        if (styledDl && EndStyleBox(dlShell, dlPad)) {
                            panel.Children.Add(dlShell);
                        }
                        break;
                    case "dt": {
                        // 术语段：独立成段不加粗（用户裁定），同 p 口径（含段落壳）
                        Flush();
                        bool styledDt = TryBeginStyleBox(node, out StackPanelWidget dtShell, out Vector4 dtPad);
                        if (styledDt) {
                            flowPanel = dtShell;
                        }
                        AppendInlineChildren(node, colorOverride);
                        Flush();
                        if (styledDt) {
                            flowPanel = panel;
                            if (EndStyleBox(dtShell, dtPad)) {
                                panel.Children.Add(dtShell);
                            }
                        }
                        break;
                    }
                    case "dd": {
                        // 定义描述段：独立容器物理位移 ListIndentPerDepth（CurrentIndentX 只被代码/
                        // 图片块宽度扣减消费，文字段不读它——缩进须像引用块一样 SetPosition 落地）；
                        // CurrentIndentX 同步进档让 dd 内代码块/图片按缩进后净宽构建
                        Flush();
                        float prevIndentDd = CurrentIndentX;
                        CurrentIndentX += ListIndentPerDepth;
                        StackPanelWidget ddPanel = CreatePanel();
                        if (blockAlign != TextAnchor.Default) {
                            // 对齐容器内 dd：撑满外壳剩余宽（Margin 左侧缩进扣余），锚点生效
                            ddPanel.HorizontalAlignment = WidgetAlignment.Stretch;
                        }
                        BuildHtmlNodes(node.ChildNodes, ddPanel, listDepth, colorOverride, depth + 1, blockAlign);
                        CurrentIndentX = prevIndentDd;
                        CanvasWidget ddOuter = new() { Size = new Vector2(NestedContentWidth(), -1f) };
                        if (blockAlign != TextAnchor.Default) {
                            // 对齐容器内 dd：外壳撑满（同引用/列表行框口径）
                            ddOuter.HorizontalAlignment = WidgetAlignment.Stretch;
                        }
                        ddOuter.Children.Add(ddPanel);
                        // 缩进走 MarginLeft 不走 SetPosition（同 BuildHtmlList contentPanel 口径）
                        ddPanel.MarginLeft = ListIndentPerDepth;
                        // 格内注册 FlowShell（顶层不注册，行为不变）：dd 外壳宽归所在格 panel 按格净宽
                        // 回填（构建期 NestedContentWidth 是顶层近似值）；SetPosition 的 ListIndentPerDepth
                        // 是壳内内容位移，不进外壳条目（口径同 CreateHtmlQuote 的 +10）
                        CurrentPanel?.Shells.Add(new ShellEntry { Frame = ddOuter, IndentX = CurrentIndentX, Kind = ShellKind.FlowShell });
                        // style 盒属性：ddOuter 入壳（已知留档：dd 内代码块/图片构建宽不吃 padding，
                        // CurrentIndentX 补偿口径 dd 专属缩进语义纠缠，不做）
                        panel.Children.Add(WrapInStyleBox(node, ddOuter) ?? ddOuter);
                        break;
                    }
                    case "summary": {
                        // 折叠标题：开头补 ▼ 代表已展开（用户裁定），余同 p 口径（含段落壳）
                        Flush();
                        bool styledSummary = TryBeginStyleBox(node, out StackPanelWidget summaryShell, out Vector4 summaryPad);
                        if (styledSummary) {
                            flowPanel = summaryShell;
                        }
                        HtmlSpanParser.AddText("▼ ", pendingSpans, default, EffectiveTextColor(colorOverride));
                        AppendInlineChildren(node, colorOverride);
                        Flush();
                        if (styledSummary) {
                            flowPanel = panel;
                            if (EndStyleBox(summaryShell, summaryPad)) {
                                panel.Children.Add(summaryShell);
                            }
                        }
                        break;
                    }
                    case "footer":
                    case "address": {
                        // 页脚/联系地址：独立成段（footer 用户裁定无特殊视觉；address 斜体由
                        // ParseHtmlNode→ApplyOpenTag 自带）。含块级子标签时块边界优先走子树递归
                        //（否则 <footer><p>a</p><p>b</p> 两段被平铺合并）。style 盒属性：
                        // 块级子标签=容器壳，行内形态=段落壳
                        Flush();
                        if (ContainsBlockChild(node)) {
                            bool styledFooter = TryBeginStyleBox(node, out StackPanelWidget footerShell, out Vector4 footerPad);
                            BuildHtmlNodes(node.ChildNodes, styledFooter ? footerShell : panel, listDepth, colorOverride, depth + 1, blockAlign);
                            if (styledFooter && EndStyleBox(footerShell, footerPad)) {
                                panel.Children.Add(footerShell);
                            }
                        }
                        else {
                            bool styledFooterInline = TryBeginStyleBox(node, out StackPanelWidget footerShell, out Vector4 footerInlinePad);
                            if (styledFooterInline) {
                                flowPanel = footerShell;
                            }
                            HtmlSpanParser.ParseHtmlNode(node, pendingSpans, default, EffectiveTextColor(colorOverride), linkColor: m_bodyLinkColor);
                            Flush();
                            if (styledFooterInline) {
                                flowPanel = panel;
                                if (EndStyleBox(footerShell, footerInlinePad)) {
                                    panel.Children.Add(footerShell);
                                }
                            }
                        }
                        Flush();
                        break;
                    }
                    default:
                        // 未知标签：子树含块级标签（section/article 包裹 pre/列表等）时递归块级遍历；
                        // 含直接 img 子级时走逐子拦截（img 升级图片块——<a><img> 帖图最高频包装，
                        // 整节点 span 解析会把图片退化成占位文本；容器行内样式对图片块无意义）；
                        // 其余整节点进 span 解析——ApplyOpenTag/引号/色底后刷全生效（此前只透传
                        // 子节点，行内容器标签自带样式被静默跳过）。
                        // style 盒属性：块级子标签=容器壳，img 段形态=段落壳；纯行内形态不做
                        //（行内盒 padding/margin 依赖 advance 计费，与行内 border 同波）
                        if (ContainsBlockChild(node)) {
                            Flush();
                            bool styledUnknown = TryBeginStyleBox(node, out StackPanelWidget unknownShell, out Vector4 unknownPad);
                            BuildHtmlNodes(node.ChildNodes, styledUnknown ? unknownShell : panel, listDepth, colorOverride, depth + 1, blockAlign);
                            if (styledUnknown && EndStyleBox(unknownShell, unknownPad)) {
                                panel.Children.Add(unknownShell);
                            }
                        }
                        else if (ImageLoader != null
                            && HasElementChild(node, "img")) {
                            bool styledImgPara = TryBeginStyleBox(node, out StackPanelWidget imgParaShell, out Vector4 imgParaPad);
                            if (styledImgPara) {
                                flowPanel = imgParaShell;
                            }
                            AppendInlineChildren(node, colorOverride);
                            if (styledImgPara) {
                                // 尾冲刷：img 之后的尾随文本仍滞 pendingSpans，不冲就逃出壳落外层
                                //（审查 P0，同 p/dt/summary/footer 行内分支口径）
                                Flush();
                                flowPanel = panel;
                                if (EndStyleBox(imgParaShell, imgParaPad)) {
                                    panel.Children.Add(imgParaShell);
                                }
                            }
                        }
                        else {
                            HtmlSpanParser.ParseHtmlNode(node, pendingSpans, default, EffectiveTextColor(colorOverride), linkColor: m_bodyLinkColor);
                        }
                        break;
                }
            }
            Flush();
        }

        void BuildHtmlList(HtmlAgilityPack.HtmlNode listNode, bool ordered, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth, TextAnchor blockAlign = TextAnchor.Default) {
            if (depth > MaxDepth) {
                return;
            }
            StackPanelWidget listPanel = CreatePanel();
            listPanel.Margin = new Vector2(0f, 2f);
            if (blockAlign != TextAnchor.Default) {
                // 对齐容器内列表：列表面板撑满宿主（否则 Near 收缩到内容宽，行框 Stretch 无意义）
                listPanel.HorizontalAlignment = WidgetAlignment.Stretch;
            }
            // <ol start="4">：解析失败回退 1
            int number = int.TryParse(listNode.GetAttributeValue("start", null), out int parsedStart) ? parsedStart : 1;
            // 列表级 type 属性（li 内同名属性逐项覆写）：ol 取 1/a/A/i/I、ul 取 disc/circle/square
            char listType = ParseListType(listNode, ordered) ?? '\0';
            // 文字列按全列表最宽 marker 对齐（"10." 比 "1." 宽），保底 marker→文字间隙；
            // 嵌套列表挂在本级文字列内，缩进节奏恒一档 ListIndentPerDepth。
            // marker 文本两遍共用一遍算出（li value 重编号/type 覆写逐项生效，宽度扫描吃同一序列）
            List<HtmlAgilityPack.HtmlNode> items = [.. listNode.Elements("li")];
            float markerScale = FontScale;
            float maxMarkerWidth = 0f;
            string[] markers = new string[items.Count];
            int scanNumber = number;
            for (int i = 0; i < items.Count; i++) {
                markers[i] = MarkerText(items[i], ordered, listType, scanNumber, listDepth + 1, out int nextNumber);
                maxMarkerWidth = MathUtils.Max(
                    maxMarkerWidth,
                    Font.MeasureText(markers[i], new Vector2(markerScale), Vector2.Zero).X
                );
                scanNumber = nextNumber;
            }
            float contentX = MathUtils.Max(ListIndentPerDepth, MarkerOffsetX + maxMarkerWidth + MarkerTextGap);
            for (int itemIndex = 0; itemIndex < items.Count; itemIndex++) {
                HtmlAgilityPack.HtmlNode li = items[itemIndex];
                // 行框宽随嵌套收窄（同 MarkdownToWidgetsBuilder.CreateListItem）
                CanvasWidget row = new() {
                    Size = new Vector2(NestedContentWidth(), -1f)
                };
                if (blockAlign != TextAnchor.Default) {
                    // 对齐容器内列表：行框撑满列表宽（NestedContentWidth()=-1 时 desired 收缩），
                    // 文字列 Stretch+锚点才有居中空间
                    row.HorizontalAlignment = WidgetAlignment.Stretch;
                }
                // 格内注册 FlowShell（顶层不注册，行为不变）：li 行框宽归所在格 panel 按格净宽
                // 回填（IndentX=行框挂载点缩进；消除格内 li 定宽/natural 污染列协商的 D1 源）
                CurrentPanel?.Shells.Add(new ShellEntry { Frame = row, IndentX = CurrentIndentX, Kind = ShellKind.FlowShell });
                FontTextWidget markerLabel = new() {
                    Text = markers[itemIndex],
                    Font = Font,
                    Color = EffectiveTextColor(colorOverride),
                    FontScale = markerScale
                };
                row.Children.Add(markerLabel);
                CanvasWidget.SetPosition(markerLabel, new Vector2(MarkerOffsetX, MarkerOffsetY));
                StackPanelWidget contentPanel = CreatePanel();
                // 文字列缩进走 MarginLeft 不走 SetPosition（Canvas 对 SetPosition 子件按 desired 排布，
                // alignment 失效）：对齐容器内列表由 Stretch 撑满行框剩余宽，锚点才有居中/靠右空间；
                // Near 时 x=MarginLeft 视觉不变
                contentPanel.MarginLeft = contentX;
                if (blockAlign != TextAnchor.Default) {
                    contentPanel.HorizontalAlignment = WidgetAlignment.Stretch;
                }
                row.Children.Add(contentPanel);
                // 嵌套上下文：li 内代码块/图片块宽度按文字列收窄。contentX 相对本级行挂载点，
                // 绝对缩进=父累计+本级（引用内列表、li 套 li 靠累加保持口径）
                float prevIndent = CurrentIndentX;
                CurrentIndentX += contentX;
                // li 内行内内容累积成段；内嵌块级标签递归处理
                List<TextSpan> pendingSpans = [];
                void FlushLi() {
                    if (pendingSpans.Count > 0) {
                        HtmlSpanParser.TrimLeadingWhitespace(pendingSpans);
                        if (pendingSpans.Count > 0) {
                            RichTextWidget widget = CreateRichText(colorOverride);
                            if (blockAlign != TextAnchor.Default) {
                                widget.HorizontalAlignment = WidgetAlignment.Stretch;
                                widget.TextAnchor = blockAlign;
                            }
                            widget.Spans.AddRange(pendingSpans);
                            contentPanel.Children.Add(widget);
                        }
                        pendingSpans = [];
                    }
                }
                foreach (HtmlAgilityPack.HtmlNode child in li.ChildNodes) {
                    bool isBlock = child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                        && child.Name.ToLowerInvariant() is "ul" or "ol" or "menu" or "blockquote" or "pre" or "p" or "div"
                            or "section" or "article" or "table" or "footer" or "address"
                            or "center" or "dl" or "dt" or "dd" or "figure" or "figcaption" or "details" or "summary"
                            or "caption"
                            or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "hr" or "html" or "body";
                    if (isBlock) {
                        // li 直发块不经 BuildHtmlNodes 分发，hidden 在此补拦（default case 重入
                        // 顶层分发自带拦截，blockquote/pre/hr/h1-6/列表直发只认此处）
                        if (child.GetAttributeValue("hidden", null) != null) {
                            continue;
                        }
                        FlushLi();
                        switch (child.Name.ToLowerInvariant()) {
                            case "ul":
                            case "ol":
                            case "menu":
                                BuildHtmlList(child, child.Name.Equals("ol", StringComparison.Ordinal), contentPanel, listDepth + 1, colorOverride, depth + 1, blockAlign);
                                break;
                            case "blockquote": {
                                Widget quote = CreateHtmlQuote(child, listDepth + 1, colorOverride, depth + 1, blockAlign);
                                contentPanel.Children.Add(WrapInStyleBox(child, quote) ?? quote);
                                break;
                            }
                            case "pre": {
                                HtmlAgilityPack.HtmlNode code = child.Element("code") ?? child;
                                Widget codeBlock = CreateCodeBlock(ReplaceBrWithNewline(code).Split('\n'));
                                contentPanel.Children.Add(WrapInStyleBox(child, codeBlock) ?? codeBlock);
                                break;
                            }
                            case "h1":
                            case "h2":
                            case "h3":
                            case "h4":
                            case "h5":
                            case "h6": {
                                RichTextWidget widget = CreateHeading(child.Name[1] - '0', colorOverride);
                                TextAnchor headingAlign = ParseBlockAlignment(child) ?? blockAlign;
                                if (headingAlign != TextAnchor.Default) {
                                    widget.HorizontalAlignment = WidgetAlignment.Stretch;
                                    widget.TextAnchor = headingAlign;
                                }
                                HtmlSpanParser.ParseHtmlNode(child, widget.Spans, default, widget.Color, linkColor: m_bodyLinkColor);
                                contentPanel.Children.Add(WrapInStyleBox(child, widget) ?? widget);
                                break;
                            }
                            case "hr": {
                                Widget thematicBreak = CreateHtmlThematicBreak(child);
                                contentPanel.Children.Add(WrapInStyleBox(child, thematicBreak) ?? thematicBreak);
                                break;
                            }
                            default: {
                                // p/div/section/article/table：单节点重入顶层分发（BuildHtmlNodes），
                                // 与顶层同口径——纯行内整块成段、嵌块递归、li 内 p>img 不再静默丢
                                BuildHtmlNodes([child], contentPanel, listDepth + 1, colorOverride, depth + 1, blockAlign);
                                break;
                            }
                        }
                    }
                    else if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                        && child.Name.Equals("img", StringComparison.OrdinalIgnoreCase)
                        && ImageLoader != null) {
                        // li 内行内 <img> 升级为图片块（同 AppendInlineChildren 口径）
                        FlushLi();
                        contentPanel.Children.Add(AddImageBlockWithAttributes(contentPanel, child));
                    }
                    else {
                        HtmlSpanParser.ParseHtmlNode(child, pendingSpans, default, EffectiveTextColor(colorOverride), linkColor: m_bodyLinkColor);
                    }
                }
                FlushLi();
                CurrentIndentX = prevIndent;
                listPanel.Children.Add(row);
                MarkerText(li, ordered, listType, number, listDepth + 1, out number);
            }
            // style 盒属性：列表整体入壳（listPanel 自带 margin(0,2) 与 padding 线性叠加）
            panel.Children.Add(WrapInStyleBox(listNode, listPanel) ?? listPanel);
        }

        // 列表项 marker 文本与下一项编号（两遍共用口径）：li value 先行重编号（HTML 渲染语义，
        // 后续项顺延）、li type 覆写列表级 type。next 输出编号已 +1（ordered 与否调用方皆直用）
        static string MarkerText(HtmlAgilityPack.HtmlNode li, bool ordered, char listType, int number, int depth, out int next) {
            if (ordered
                && int.TryParse(li.GetAttributeValue("value", null), out int value)) {
                number = value;
            }
            char type = ParseListType(li, ordered) ?? listType;
            next = number + 1;
            return HtmlTextUtils.GetListMarker(ordered, type, number, depth);
        }

        // list/li type 属性解析：ol 认 1/a/A/i/I（原始大小写敏感——小写化会并掉 A/I 两档），
        // ul/menu 认 disc/circle/square（映射 marker 字形）。非法/缺省返回 null（回退默认口径）
        static char? ParseListType(HtmlAgilityPack.HtmlNode node, bool ordered) {
            string type = node.GetAttributeValue("type", null)?.Trim();
            if (ordered) {
                return type switch {
                    "1" => '1',
                    "a" => 'a',
                    "A" => 'A',
                    "i" => 'i',
                    "I" => 'I',
                    _ => null
                };
            }
            return type?.ToLowerInvariant() switch {
                "disc" => '•',
                "circle" => '◦',
                "square" => '▪',
                _ => null
            };
        }

        CanvasWidget CreateHtmlQuote(HtmlAgilityPack.HtmlNode quoteNode, int listDepth, Color? colorOverride, int depth, TextAnchor blockAlign = TextAnchor.Default) {
            CanvasWidget outer = CreateQuoteFrame(out StackPanelWidget inner);
            if (blockAlign != TextAnchor.Default) {
                // 对齐容器内引用：外壳与 inner 双层 Stretch 撑满（HtmlWidget 壳路径 ContentWidth=-1，
                // NestedContentWidth()=-1 时外壳 desired 收缩到内容宽，仅 inner Stretch 无居中空间）
                outer.HorizontalAlignment = WidgetAlignment.Stretch;
                inner.HorizontalAlignment = WidgetAlignment.Stretch;
            }
            // 格内注册 FlowShell（顶层不注册，行为不变）：quote 外壳宽归所在格 panel 按格净宽
            // 回填——构建期 NestedContentWidth 是顶层近似值。IndentX=本块挂载点缩进（inner 的
            // +10 是壳内内容缩进，不进外壳条目）
            CurrentPanel?.Shells.Add(new ShellEntry { Frame = outer, IndentX = CurrentIndentX, Kind = ShellKind.FlowShell });
            float prevIndent = CurrentIndentX;
            CurrentIndentX += 10f;
            BuildHtmlNodes(quoteNode.ChildNodes, inner, listDepth, QuoteColor, depth + 1, blockAlign);
            CurrentIndentX = prevIndent;
            return outer;
        }

        // HTML 表格 → TablePanelWidget 直产（spec §7.1）：布局/列协商/格线全由底座承担，本方法只做
        // DOM→行/格映射。行收集：table 直下 tr 与 thead/tbody 内 tr 展平、tfoot 行沉底（HTML 渲染
        // 语义，FooterRows 记行数）；caption/colgroup/col 无对应视觉语义，跳过（caption 已在表 case
        // 预提取渲染）。thead 行或含 th 的行为表头（粗体由 builder 侧染 span，同 Markdown CreateTable
        // 口径）。无滚动兜底（包装类超宽横向滚动不随迁）：Σmin 超宿主时底座定格列 min 溢出
        TablePanelWidget CreateHtmlTable(HtmlAgilityPack.HtmlNode tableNode, Color? colorOverride, int depth, TextAnchor blockAlign = TextAnchor.Default) {
            // 格内容相对格左缘定位（spec §5.3）：入口清零外层嵌套缩进——格 shell 的回填扣减基准
            // 是格净宽，li 内嵌套表格时外层缩进已由挂载位置体现，再计入会双扣；全部出口恢复
            //（prevIndent 纪律同 CreateHtmlQuote，含两处早退）
            float prevIndent = CurrentIndentX;
            CurrentIndentX = 0f;
            if (depth > MaxDepth) {
                CurrentIndentX = prevIndent;
                return null;
            }
            // table cellpadding 属性（px）：CellPadding 横向直挂，纵向保持默认；纯行内格 Size.X
            // 锚定同参（cellPaddingX 透传 CreateHtmlCell）
            float cellPaddingX = CellPaddingX;
            if (int.TryParse(tableNode.GetAttributeValue("cellpadding", null), out int cellPadding)
                && cellPadding >= 0) {
                cellPaddingX = cellPadding;
            }
            // 视觉对齐 MD 表现状：四开关全开+同线色（InnerBorder 在场时 Header/Footer
            // Separator 不双画，仅 InnerBorder=false 的自定义场景生效；FooterSeparator 对称补齐）
            TablePanelWidget widget = new() {
                CellPadding = new Vector2(cellPaddingX, 3f),
                BorderColor = MarkdownTableWidget.GridLineColor,
                OuterBorder = true,
                InnerBorder = true,
                HeaderSeparator = true,
                FooterSeparator = true,
                Margin = new Vector2(0f, 4f)
            };
            // table align=center：表格块整体居中（desired=Σtracks 非拉伸）；left/right 浮动语义
            // 不做（文字环绕无对应排版模型）
            if (ParseBlockAlignment(tableNode) == TextAnchor.HorizontalCenter) {
                widget.HorizontalAlignment = WidgetAlignment.Center;
            }
            // bgcolor 回退链根：table 属性 → body 属性（整页包装场景）
            Color? tableBackground = CssColorParser.TryParse(tableNode.GetAttributeValue("bgcolor", null), out Color tableBg)
                ? tableBg
                : m_bodyBackground;
            List<(HtmlAgilityPack.HtmlNode Row, bool FromThead)> rows = [];
            // tfoot 行收集沉底（HTML 渲染语义：footer 行无论 DOM 位置恒在表尾），行数设
            // FooterRows 驱动底座表尾结构（FooterSeparator 语义对称 HeaderSeparator）
            List<HtmlAgilityPack.HtmlNode> footerRows = [];
            foreach (HtmlAgilityPack.HtmlNode child in tableNode.ChildNodes) {
                if (child.NodeType != HtmlAgilityPack.HtmlNodeType.Element) {
                    continue;
                }
                // 行收集路径不经 BuildHtmlNodes 分发，hidden 在此补拦（thead/tbody/tfoot 容器同口径）
                if (child.GetAttributeValue("hidden", null) != null) {
                    continue;
                }
                string childTag = child.Name.ToLowerInvariant();
                if (childTag == "tr") {
                    rows.Add((child, false));
                }
                else if (childTag is "thead" or "tbody" or "tfoot") {
                    if (childTag == "tfoot") {
                        footerRows.AddRange(child.Elements("tr"));
                    }
                    else {
                        foreach (HtmlAgilityPack.HtmlNode tr in child.Elements("tr")) {
                            rows.Add((tr, childTag == "thead"));
                        }
                    }
                }
            }
            foreach (HtmlAgilityPack.HtmlNode tr in footerRows) {
                rows.Add((tr, false));
            }
            if (rows.Count == 0) {
                CurrentIndentX = prevIndent;
                return null;
            }
            // 剔除空行（无 td/th——本方法不产行、底座无对应布局行），使行序号=ExpandSlots 布局行号；
            // FooterRows 计剔除后的真实 footer 行数
            int footerRowCount = footerRows.Count(HasCellElement);
            rows.RemoveAll(entry => !HasCellElement(entry.Row));
            // 自然列数上限（各行 td/th 格数最大值）——ExpandSlots 同名口径：span 展开不越过它，
            // 下列对齐模拟的 cursor 推进须与底座逐格一致（M1），单改一侧即错列
            int naturalColumns = 0;
            foreach ((HtmlAgilityPack.HtmlNode Row, bool _) in rows) {
                int cellCount = 0;
                foreach (HtmlAgilityPack.HtmlNode child in Row.ChildNodes) {
                    if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                        && child.Name.ToLowerInvariant() is "td" or "th") {
                        cellCount++;
                    }
                }
                naturalColumns = MathUtils.Max(naturalColumns, cellCount);
            }
            // 列对齐：格级属性降列级——自上而下取每列第一个出现的对齐（td/th 与 tr 的 align 属性、
            // style text-align 都认），此后该列锚点固定（照包装类 SetColumnAlignments 语义）；
            // 列号照底座 ExpandSlots 口径模拟（M1）：cursor 跳过被前行 rowspan 登记的占格，span
            // 展开同钳自然列数（naturalColumns）——两侧任一单改即错列。越界 colspan 二次钳
            //（columns−Col）发生在全部 slot.Col 定值之后，不影响列号，故模拟对全部输入与布局期
            // 定列逐槽一致
            List<MarkdownTableAlignment?> alignments = [];
            HashSet<(int Row, int Col)> occupied = [];
            for (int r = 0; r < rows.Count; r++) {
                (HtmlAgilityPack.HtmlNode tr, bool fromThead) = rows[r];
                // 表头行判定行级统一（F3）：thead 行或行内含 th，与 th 的格序无关——原实现随格
                // 遍历置位，th 前的 td 格以 false 建格漏染粗；整行格统一染粗（同 Markdown CreateTable 口径）
                bool isHeader = fromThead || HasHeaderCell(tr);
                // bgcolor 链第二级：tr 属性 → 表/body 根。挂行壳整体铺（覆盖 rowspan 空隙，
                // 浏览器同视觉），格级属性由 SetCellBackground 覆盖其上
                Color? rowBackground = CssColorParser.TryParse(tr.GetAttributeValue("bgcolor", null), out Color rowBg)
                    ? rowBg
                    : tableBackground;
                TextAnchor? rowVerticalAlignment = ParseVerticalAlignment(tr, tr);
                List<Widget> cells = [];
                int column = 0;
                foreach (HtmlAgilityPack.HtmlNode cellNode in tr.ChildNodes) {
                    if (cellNode.NodeType != HtmlAgilityPack.HtmlNodeType.Element) {
                        continue;
                    }
                    string cellTag = cellNode.Name.ToLowerInvariant();
                    if (cellTag is not ("td" or "th")) {
                        continue;
                    }
                    // td/th hidden：格收集同走不到 BuildHtmlNodes，补拦（行收集已拦 tr/行组容器）
                    if (cellNode.GetAttributeValue("hidden", null) != null) {
                        continue;
                    }
                    // colspan/rowspan 属性：空/非法/≤0 → 1（Set 后仅非平凡值登记 attached 状态）
                    int colSpan = ParseSpanAttribute(cellNode, "colspan");
                    int rowSpan = ParseSpanAttribute(cellNode, "rowspan");
                    Widget cell = CreateHtmlCell(cellNode, isHeader, colorOverride, depth, blockAlign, cellPaddingX);
                    if (colSpan > 1) {
                        TablePanelWidget.SetColSpan(cell, colSpan);
                    }
                    if (rowSpan > 1) {
                        TablePanelWidget.SetRowSpan(cell, rowSpan);
                    }
                    // 格级 bgcolor（链最内级）：仅在有显式属性时挂，行壳底色兜其余
                    if (CssColorParser.TryParse(cellNode.GetAttributeValue("bgcolor", null), out Color cellBg)) {
                        TablePanelWidget.SetCellBackground(cell, cellBg);
                    }
                    // M1：跳过被前行 rowspan 占住的槽位定真实列号（照 ExpandSlots cursor 口径）
                    while (occupied.Contains((r, column))) {
                        column++;
                    }
                    while (alignments.Count <= column) {
                        alignments.Add(null);
                    }
                    alignments[column] ??= ParseCellAlignment(cellNode, tr);
                    // valign：格属性 → 行属性（tr 与格同查；行级传自身节点即行属性直取）
                    ApplyCellAnchor(cell, ToTextAnchor(alignments[column]), ParseVerticalAlignment(cellNode, tr) ?? rowVerticalAlignment);
                    cells.Add(cell);
                    // rowspan 向后续行登记占格（照 ExpandSlots：行覆盖至 min(r+rowSpan, 行数) 前，
                    // 列覆盖 [column, column+advance)，advance=span 展开钳自然列数与底座同口径）
                    int advance = MathUtils.Min(MathUtils.Max(colSpan, 1), MathUtils.Max(naturalColumns - column, 1));
                    for (int rr = r + 1; rr < MathUtils.Min(r + rowSpan, rows.Count); rr++) {
                        for (int cc = column; cc < column + advance; cc++) {
                            occupied.Add((rr, cc));
                        }
                    }
                    column += advance;
                }
                if (cells.Count > 0) {
                    TableRowWidget row = new() { Header = isHeader, BackgroundColor = rowBackground };
                    foreach (Widget cell in cells) {
                        row.Children.Add(cell);
                    }
                    widget.Children.Add(row);
                }
            }
            if (footerRowCount > 0) {
                widget.FooterRows = footerRowCount;
            }
            CurrentIndentX = prevIndent;
            return widget;
        }

        // 列宽协商语义（照包装类 AddRow 口径）：自然宽+2×CellPadding
        const float CellPaddingX = 6f;

        // 单格构建（spec §7.1 cell 内容升级）：纯行内 → RichTextWidget（WordWrap + Stretch 拉满格矩形，
        // Size.X 锚定 自然宽+2×CellPadding——底座 track 即旧列宽，高度轮按 track−2×CellPadding 重测）；
        // 含块级子标签（p/div/section/article/table/h1-h6/hr/pre/blockquote/ul/ol），或有图片管线时
        // img 为唯一元素内容 → RichContentPanel 直装块：BuildHtmlNodes 整格重入（p/li 内
        // 块级同口径），img 升级图片块、嵌套 table 递归本方法。纯行内 cell 的 img 照旧 span 占位
        // （HtmlSpanParser 分支保留）。格内块 shell 宽由所在格 panel 按格净宽回传（本方法 push
        // CurrentPanel，见 RichContentPanel MeasureOverride）
        Widget CreateHtmlCell(HtmlAgilityPack.HtmlNode cellNode, bool isHeader, Color? colorOverride, int depth, TextAnchor blockAlign = TextAnchor.Default, float cellPaddingX = 6f) {
            bool hasBlockChild = false;
            bool imgOnly = ImageLoader != null;
            foreach (HtmlAgilityPack.HtmlNode child in cellNode.ChildNodes) {
                if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element) {
                    if (IsBlockTag(child.Name)) {
                        hasBlockChild = true;
                        break;
                    }
                    if (imgOnly
                        && !child.Name.Equals("img", StringComparison.OrdinalIgnoreCase)) {
                        imgOnly = false;
                    }
                }
                else if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Text
                    && imgOnly
                    && !string.IsNullOrWhiteSpace(child.InnerText)) {
                    imgOnly = false;
                }
            }
            if (hasBlockChild
                || imgOnly) {
                // 格容器升级 RichContentPanel：本格块级 shell（代码块/图片/引用外壳/列表行框）
                // 注册进本格 panel，由其 MeasureOverride 在底座高度轮按格净宽回填宽度——构建期
                // ContentWidth 是顶层近似值，对格内不成立（task-8-report 局限的机制化消解）
                RichContentPanel panel = new() { ImageLoader = ImageLoader };
                // 横向拉满格矩形：格内锚点/换行参考系=整格净宽，不随最宽子件塌缩
                panel.HorizontalAlignment = WidgetAlignment.Stretch;
                // push/pop：构建本格内容期间 shell 注册路由进本格 panel；逐格 push/pop 使嵌套
                // 表格（内表格 CreateHtmlCell 再 push）与本格结束后恢复外层上下文正确嵌套
                RichContentPanel prevPanel = CurrentPanel;
                CurrentPanel = panel;
                BuildHtmlNodes(cellNode.ChildNodes, panel, 0, colorOverride, depth + 1, blockAlign);
                CurrentPanel = prevPanel;
                if (isHeader) {
                    BoldWidgetSpans(panel);
                }
                return panel;
            }
            // 纯行内：span 解析（img 命中 HtmlSpanParser 占位分支，照旧保留）
            List<TextSpan> spans = [];
            HtmlSpanParser.ParseHtmlNode(cellNode, spans, default, EffectiveTextColor(colorOverride), linkColor: m_bodyLinkColor);
            if (isHeader) {
                BoldCellSpans(spans);
            }
            RichTextWidget widget = new() {
                Font = Font,
                FontScale = FontScale,
                Color = EffectiveTextColor(colorOverride),
                WordWrap = true,
                // td nowrap：老版防换行属性 → KeepAll（CJK/单词整体不破，浏览器 white-space:nowrap
                // 对纯文本格的近似口径）；含块级子件的格不适用（块级各自折行语义），留档不处理
                WordBreak = cellNode.GetAttributeValue("nowrap", null) != null
                    ? TextWordBreak.KeepAll
                    : TextWordBreak.BreakWord,
                // 拉满格矩形（track−2×CellPadding）：TextAnchor 居中/右对齐按 ActualSize 定偏移，
                // Near 会塌缩到内容宽
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch
            };
            widget.Spans.AddRange(spans);
            if (blockAlign != TextAnchor.Default) {
                // 对齐容器内的表格格文本（Chrome text-align 继承进 td；列对齐缺省时不覆写，见 ApplyCellAnchor）
                widget.TextAnchor = blockAlign;
            }
            if (LinkClicked != null) {
                widget.LinkClicked += href => LinkClicked(href);
            }
            // 按 float.MaxValue 自测拿自然宽（与旧 scratch Layout(float.MaxValue,∞) 同参对齐），
            // Size.X 锚定让底座 track=自然宽+2×CellPadding（cellpadding 属性时同参）；Size.Y=-1
            // 保持折行高自动
            widget.Measure(new Vector2(float.MaxValue, float.PositiveInfinity));
            widget.Size = new Vector2(widget.ParentDesiredSize.X + 2f * cellPaddingX, -1f);
            return widget;
        }

        // td 直下块级子标签判定（li 内块级同名单）：命中即整格走块级直装。footer/address 独立成段
        //（footer 用户裁定；address HTML 块级语义），switch 有显式 case；center/dl/dt/dd/figure/
        // figcaption/details/summary 同批块级化（article 本就在名单）
        static bool IsBlockTag(string tag) {
            return tag.ToLowerInvariant() is "p" or "div" or "section" or "article" or "table"
                or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "hr" or "pre" or "blockquote"
                or "ul" or "ol" or "menu" or "footer" or "address"
                or "center" or "dl" or "dt" or "dd" or "figure" or "figcaption" or "details" or "summary"
                or "caption" or "html" or "body";
        }

        // colspan/rowspan 属性解析：空/非法/≤0 → 1，上限钳 1000（HTML 标准同值）——不可信源
        // `<td colspan="2000000000">` 不钳会经 alignments 扩容 + 底座 new float[columns] 双重内存爆炸；
        // SetColSpan/SetRowSpan 内部还有 max(·,1) 钳制兜底，越界 span 由底座 ExpandSlots 钳到实际列数
        static int ParseSpanAttribute(HtmlAgilityPack.HtmlNode cell, string name) {
            return int.TryParse(cell.GetAttributeValue(name, null), out int value) && value > 0 ? Math.Min(value, 1000) : 1;
        }

        // 行直下是否含 th 格（行级表头判定辅助，F3；thead 判定由调用侧 fromThead 承担）
        static bool HasHeaderCell(HtmlAgilityPack.HtmlNode row) {
            foreach (HtmlAgilityPack.HtmlNode child in row.ChildNodes) {
                if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                    && child.Name.ToLowerInvariant() == "th") {
                    return true;
                }
            }
            return false;
        }

        // 行直下是否含 td/th 格（M1 空行剔除辅助：空行不产行、底座无对应布局行）
        static bool HasCellElement(HtmlAgilityPack.HtmlNode row) {
            foreach (HtmlAgilityPack.HtmlNode child in row.ChildNodes) {
                if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                    && child.Name.ToLowerInvariant() is "td" or "th") {
                    return true;
                }
            }
            return false;
        }

        // 块格表头染粗：递归格内全部 RichTextWidget（嵌套表格一并——CSS th 粗体继承口径）
        static void BoldWidgetSpans(Widget widget) {
            if (widget is RichTextWidget rich) {
                BoldCellSpans(rich.Spans);
            }
            else if (widget is ContainerWidget container) {
                foreach (Widget child in container.Children) {
                    BoldWidgetSpans(child);
                }
            }
        }

        // 格锚点下写：RichTextWidget 直写 TextAnchor 并横向拉满容器（留锚点偏移余量）；容器格递归
        // （CSS text-align 继承口径，嵌套表格一并）。列对齐只补缺省锚点：容器内已显式居中的段/标题
        //（blockAlign）优先于列对齐属性——<td><center> 老帖高频组合，无条件赋值会击穿居中；
        // 浏览器同款取舍（center 元素自身 text-align 胜过外层 td 属性），left 属性映射 Default 不写。
        // valign 走垂直位 OR 合成（HorizontalCenter/Right 与 Top/Bottom 位互不冲突，flags 枚举），
        // 并确保纵向 Stretch 留锚点偏移余量；块级格（RichContentPanel）内容纵排无法整体居中，
        // 递归只达内部文本件（单行文本格即全效，多行块近似），留档局限
        static void ApplyCellAnchor(Widget widget, TextAnchor anchor, TextAnchor? verticalAnchor = null) {
            if (widget is RichTextWidget rich) {
                if (anchor != TextAnchor.Default
                    && rich.TextAnchor == TextAnchor.Default) {
                    rich.TextAnchor = anchor;
                }
                if (verticalAnchor != null) {
                    rich.TextAnchor |= verticalAnchor.Value;
                    rich.VerticalAlignment = WidgetAlignment.Stretch;
                }
                rich.HorizontalAlignment = WidgetAlignment.Stretch;
            }
            else if (widget is ContainerWidget container) {
                foreach (Widget child in container.Children) {
                    ApplyCellAnchor(child, anchor, verticalAnchor);
                }
            }
        }

        // valign 属性 → 垂直锚点位：top/middle/bottom/baseline（baseline 近似 top，基线感知排版
        // 未建模同 sub/sup 留档 P4 TTF）。TextAnchor.Top==0==Default，故返回 nullable——null=无
        // 属性（不写），Top=显式置顶（OR 0 无副作用但保 Stretch 赋值）。缺省查所在行（tr valign
        // 行级回退），皆无返回 null
        static TextAnchor? ParseVerticalAlignment(HtmlAgilityPack.HtmlNode cell, HtmlAgilityPack.HtmlNode row) {
            string token = cell.GetAttributeValue("valign", null)?.Trim().ToLowerInvariant()
                ?? row.GetAttributeValue("valign", null)?.Trim().ToLowerInvariant();
            return token switch {
                "top" or "baseline" => TextAnchor.Top,
                "middle" => TextAnchor.VerticalCenter,
                "bottom" => TextAnchor.Bottom,
                _ => null
            };
        }

        // 块级 align 属性 → 文本锚点（CSS text-align 口径）：center/right/left 三值生效，
        // justify/char 不识别返回 null（继承外层）。left 显式映射 Default=Left 同值，
        // nullable 装箱保住"显式 left 覆盖继承的 right"语义（否则 ?? 链会回退到继承值）
        static TextAnchor? ParseBlockAlignment(HtmlAgilityPack.HtmlNode node) {
            return node.GetAttributeValue("align", null)?.Trim().ToLowerInvariant() switch {
                "center" => TextAnchor.HorizontalCenter,
                "right" => TextAnchor.Right,
                "left" => TextAnchor.Default,
                _ => null
            };
        }

        // hr 旧属性四件套：width（px / % 相对容器，容器宽未知退全宽）/size（厚度 px）/color/
        // align（仅定宽时有偏移空间；全宽时 Stretch 已占满无意义）。noshade 忽略（本就纯色平线）
        RectangleWidget CreateHtmlThematicBreak(HtmlAgilityPack.HtmlNode node) {
            RectangleWidget line = CreateThematicBreak();
            if (CssColorParser.TryParse(node.GetAttributeValue("color", null), out Color hrColor)) {
                line.FillColor = hrColor;
            }
            float width = -1f;
            string widthAttr = node.GetAttributeValue("width", null)?.Trim();
            if (!string.IsNullOrEmpty(widthAttr)) {
                if (widthAttr.EndsWith("%", StringComparison.Ordinal)
                    && float.TryParse(widthAttr[..^1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float percent)
                    && NestedContentWidth() > 0f) {
                    width = NestedContentWidth() * MathUtils.Clamp(percent / 100f, 0f, 1f);
                }
                else if (float.TryParse(widthAttr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pixels)) {
                    width = MathUtils.Max(pixels, 0f);
                }
            }
            float thickness = -1f;
            if (int.TryParse(node.GetAttributeValue("size", null), out int sizeAttribute)
                && sizeAttribute > 0) {
                thickness = MathUtils.Min(sizeAttribute, 100f);
            }
            if (width >= 0f
                || thickness > 0f) {
                // 显式尺寸：脱离 Stretch 定宽定厚（Rectangle 无内容 -1 缩线惯例，厚度必显式给定）
                line.HorizontalAlignment = ParseBlockAlignment(node) switch {
                    TextAnchor.HorizontalCenter => WidgetAlignment.Center,
                    TextAnchor.Right => WidgetAlignment.Far,
                    _ => WidgetAlignment.Near
                };
                line.Size = new Vector2(width >= 0f ? width : -1f, thickness > 0f ? thickness : 1f);
            }
            return line;
        }

        // <img> 属性口径统一入口（块级/img 直下级/li 行内级三调用点）：width/height 显式尺寸 +
        // hspace/vspace 外边距（各侧独立计，横向相加得双边距）。其余 align 属性留档不做
        //（left/right 浮动环绕无对应排版模型）
        // 返回待挂树 widget（调用方统一 Children.Add）：无 style 盒属性=frame 本体，
        // 有=透明壳（frame 包入壳，壳 Margin=CSS margin、frame 四边距=CSS padding；
        // 图片块升级为块级，块级元素 style 盒属性同权适用——审查 P2 补接）
        Widget AddImageBlockWithAttributes(StackPanelWidget target, HtmlAgilityPack.HtmlNode img) {
            Vector2? explicitSize = ParseImageSize(img);
            Vector2 margin = new(0f, 8f);
            if (float.TryParse(img.GetAttributeValue("hspace", null), out float hspace) && hspace > 0f) {
                margin.X = MathUtils.Min(hspace * 2f, 100f);
            }
            if (float.TryParse(img.GetAttributeValue("vspace", null), out float vspace) && vspace > 0f) {
                margin.Y = MathUtils.Min(vspace * 2f, 100f);
            }
            CanvasWidget frame = AddImageBlock(target, img.GetAttributeValue("src", null), img.GetAttributeValue("alt", null), explicitSize, margin);
            // AddImageBlock 已挂树；摘回统一由调用方挂（摘下重挂同位追加，顺序不变）
            target.Children.Remove(frame);
            return WrapInStyleBox(img, frame) ?? frame;
        }

        // img width/height：px 直取；width 的 % 相对容器（容器宽未知跳过该维）；height 的 %
        // 不做（无解析基准）。两维均未给返回 null（走自适应管线）。显式值钳 1-4096 防御
        Vector2? ParseImageSize(HtmlAgilityPack.HtmlNode img) {
            float width = ParseImageDimension(img.GetAttributeValue("width", null), allowPercent: true);
            float height = ParseImageDimension(img.GetAttributeValue("height", null), allowPercent: false);
            if (width <= 0f
                && height <= 0f) {
                return null;
            }
            return new Vector2(width, height);
        }

        float ParseImageDimension(string attribute, bool allowPercent) {
            if (string.IsNullOrWhiteSpace(attribute)) {
                return -1f;
            }
            string trimmed = attribute.Trim();
            if (trimmed.EndsWith("%", StringComparison.Ordinal)) {
                if (!allowPercent
                    || !float.TryParse(trimmed[..^1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float percent)
                    || NestedContentWidth() <= 0f) {
                    return -1f;
                }
                return MathUtils.Clamp(NestedContentWidth() * percent / 100f, 1f, 4096f);
            }
            return float.TryParse(trimmed, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float pixels)
                ? MathUtils.Clamp(pixels, 1f, 4096f)
                : -1f;
        }

        // MarkdownTableAlignment → TextAnchor（照包装类 AssignAnchors 映射；缺省=左对齐 Default）
        static TextAnchor ToTextAnchor(MarkdownTableAlignment? alignment) {
            return alignment switch {
                MarkdownTableAlignment.Center => TextAnchor.HorizontalCenter,
                MarkdownTableAlignment.Right => TextAnchor.Right,
                _ => TextAnchor.Default
            };
        }

        // 对齐取值顺序：cell 自身 align → 所在行 align → cell style text-align → 行 style text-align；
        // 皆缺省返回 null（MarkdownTableWidget 左对齐）。justify/inherit 等不识别即左对齐
        static MarkdownTableAlignment? ParseCellAlignment(HtmlAgilityPack.HtmlNode cell, HtmlAgilityPack.HtmlNode row) {
            MarkdownTableAlignment? alignment = ParseAlignmentToken(cell.GetAttributeValue("align", null))
                ?? ParseAlignmentToken(row.GetAttributeValue("align", null));
            if (alignment != null) {
                return alignment;
            }
            System.Text.RegularExpressions.Match cellMatch = s_textAlignRegex.Match(cell.GetAttributeValue("style", null) ?? string.Empty);
            if (cellMatch.Success) {
                return ParseAlignmentToken(cellMatch.Groups[1].Value);
            }
            System.Text.RegularExpressions.Match rowMatch = s_textAlignRegex.Match(row.GetAttributeValue("style", null) ?? string.Empty);
            return rowMatch.Success ? ParseAlignmentToken(rowMatch.Groups[1].Value) : null;
        }

        // 左边界 lookbehind：防自定义属性（如 --my-text-align）内的 text-align 子串误配（同 HtmlSpanParser 惯例）
        static readonly System.Text.RegularExpressions.Regex s_textAlignRegex =
            new("(?<![\\w-])text-align\\s*:\\s*([a-zA-Z]+)", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        static MarkdownTableAlignment? ParseAlignmentToken(string token) {
            return token?.ToLowerInvariant() switch {
                "center" => MarkdownTableAlignment.Center,
                "right" => MarkdownTableAlignment.Right,
                "left" => MarkdownTableAlignment.Left,
                _ => null
            };
        }

        // 直接子级是否含指定元素（浅查一层）：default 行内容器分支的 img 升级判据
        static bool HasElementChild(HtmlAgilityPack.HtmlNode node, string tag) {
            foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes) {
                if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                    && child.Name.Equals(tag, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        // 子树（浅查 4 层够）是否含块级标签：决定未知容器标签走块级递归还是 span 透传
        static bool ContainsBlockChild(HtmlAgilityPack.HtmlNode node, int depth = 0) {
            if (depth > 4) {
                return false;
            }
            foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes) {
                if (child.NodeType != HtmlAgilityPack.HtmlNodeType.Element) {
                    continue;
                }
                if (child.Name.ToLowerInvariant() is "pre" or "ul" or "ol" or "menu" or "blockquote" or "p" or "div" or "hr" or "table"
                    or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "footer" or "address"
                    or "center" or "dl" or "dt" or "dd" or "figure" or "figcaption" or "details" or "summary"
                    or "caption" or "html" or "body") {
                    return true;
                }
                if (ContainsBlockChild(child, depth + 1)) {
                    return true;
                }
            }
            return false;
        }

        // pre/code 内的 <br> 当换行；其余取原始文本
        static string ReplaceBrWithNewline(HtmlAgilityPack.HtmlNode node) {
            return ReplaceBrWithNewline(node, 0);
        }

        static string ReplaceBrWithNewline(HtmlAgilityPack.HtmlNode node, int depth) {
            if (depth > MaxDepth) {
                return string.Empty;
            }
            var builder = new System.Text.StringBuilder();
            foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes) {
                if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Text) {
                    builder.Append(child.InnerText);
                }
                else if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                    && child.Name.Equals("br", StringComparison.Ordinal)) {
                    builder.Append('\n');
                }
                else {
                    builder.Append(ReplaceBrWithNewline(child, depth + 1));
                }
            }
            return builder.ToString();
        }
    }
}
