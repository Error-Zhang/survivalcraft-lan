using Engine;
using Engine.Graphics;

namespace Game {
    // HtmlAgilityPack DOM → widget 树（块级遍历）。外部 HTML 片段入口（P2：HtmlWidget 壳、
    // CommunityPostDetailsDialog 正文）；MarkdownToWidgetsBuilder 的 HtmlBlock 分支也转发到此，
    // 产出的块注册表（CodeBlocks/ImageBlocks）由转发方合并回自己的集合供宿主测量期回填宽度。
    // 行内内容走 HtmlSpanParser
    public class HtmlToWidgetsBuilder : RichContentBuilder {
        // HTML 片段入口：解析为 DOM 后做块级遍历，块收进自建面板返回（HtmlWidget 壳用）
        public StackPanelWidget BuildHtml(string html, int listDepth, Color? colorOverride, int depth) {
            StackPanelWidget panel = CreatePanel();
            BuildHtmlInto(html, panel, listDepth, colorOverride, depth);
            return panel;
        }

        // 平铺入口：块直接追加进调用方面板（Markdown HtmlBlock 转发用，避免多包一层嵌套面板）
        public void BuildHtmlInto(string html, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(html ?? string.Empty);
            BuildHtmlNodes(doc.DocumentNode.ChildNodes, panel, listDepth, colorOverride, depth);
        }

        // 块级 HTML 节点遍历：行内节点累积成段，遇块级标签先冲刷当前段。
        // 参数放宽 IEnumerable：li 内块级子节点以单节点数组重入（复用顶层分发口径）。
        // centerContent：居中容器（center/figure/figcaption）内构建——段/标题拉满宽+文本行居中，
        // 全部重入点（div/dl/列表/引用/表格格/未知容器）透传（Chrome text-align 继承口径）
        void BuildHtmlNodes(IEnumerable<HtmlAgilityPack.HtmlNode> nodes, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth, bool centerContent = false) {
            if (depth > MaxDepth) {
                return;
            }
            List<TextSpan> pendingSpans = [];
            void Flush() {
                if (pendingSpans.Count > 0) {
                    HtmlSpanParser.TrimLeadingWhitespace(pendingSpans);
                    if (pendingSpans.Count == 0) {
                        return;
                    }
                    RichTextWidget widget = CreateRichText(colorOverride);
                    if (centerContent) {
                        widget.HorizontalAlignment = WidgetAlignment.Stretch;
                        widget.TextAnchor = TextAnchor.HorizontalCenter;
                    }
                    widget.Spans.AddRange(pendingSpans);
                    panel.Children.Add(widget);
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
                        AddImageBlock(panel, child.GetAttributeValue("src", null), child.GetAttributeValue("alt", null));
                        continue;
                    }
                    HtmlSpanParser.ParseHtmlNode(child, pendingSpans, default, inlineColorOverride ?? DefaultColor);
                }
            }
            foreach (HtmlAgilityPack.HtmlNode node in nodes) {
                if (node.NodeType != HtmlAgilityPack.HtmlNodeType.Element
                    && node.NodeType != HtmlAgilityPack.HtmlNodeType.Text) {
                    continue;
                }
                if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Text) {
                    HtmlSpanParser.ParseHtmlNode(node, pendingSpans, default, colorOverride ?? DefaultColor);
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
                        if (centerContent) {
                            widget.HorizontalAlignment = WidgetAlignment.Stretch;
                            widget.TextAnchor = TextAnchor.HorizontalCenter;
                        }
                        HtmlSpanParser.ParseHtmlNode(node, widget.Spans, default, widget.Color);
                        panel.Children.Add(widget);
                        break;
                    }
                    case "p":
                        Flush();
                        AppendInlineChildren(node, colorOverride);
                        Flush();
                        break;
                    case "div":
                        // 容器标签递归块级遍历（子级 p/pre/列表各归其位），不做 span 平铺——
                        // 否则 div 内 pre 被当普通元素折叠，代码缩进/换行全毁
                        Flush();
                        BuildHtmlNodes(node.ChildNodes, panel, listDepth, colorOverride, depth + 1, centerContent);
                        break;
                    case "ul":
                    case "ol":
                    case "menu":
                        // menu 与 ul 同渲染（HTML 弃用标签，Chrome 按无序列表渲染）
                        Flush();
                        BuildHtmlList(node, node.Name.Equals("ol", StringComparison.Ordinal), panel, listDepth, colorOverride, depth, centerContent);
                        break;
                    case "blockquote":
                        Flush();
                        panel.Children.Add(CreateHtmlQuote(node, listDepth, colorOverride, depth, centerContent));
                        break;
                    case "pre": {
                        Flush();
                        HtmlAgilityPack.HtmlNode code = node.Element("code") ?? node;
                        panel.Children.Add(CreateCodeBlock(ReplaceBrWithNewline(code).Split('\n')));
                        break;
                    }
                    case "hr":
                        Flush();
                        panel.Children.Add(CreateThematicBreak());
                        break;
                    case "img" when ImageLoader != null:
                        // 块级 <img>：与纯图段落同管线。行内 <img>（p 内）仍被丢弃（范围外）
                        Flush();
                        AddImageBlock(panel, node.GetAttributeValue("src", null), node.GetAttributeValue("alt", null));
                        break;
                    case "table": {
                        Flush();
                        // caption（表题）：表上方居中块渲染（CreateHtmlTable 行收集本就跳过 caption，
                        // 不双渲染）；用户裁定 caption=居中块，与 center/figure 同款
                        HtmlAgilityPack.HtmlNode captionNode = node.Element("caption");
                        if (captionNode != null) {
                            BuildHtmlNodes(captionNode.ChildNodes, panel, listDepth, colorOverride, depth + 1, true);
                        }
                        // 空表（无任何行）返回 null 不产部件——零尺寸 widget 仍会靠 Margin 贡献 8px 空白
                        TablePanelWidget table = CreateHtmlTable(node, colorOverride, depth, centerContent);
                        if (table != null) {
                            panel.Children.Add(table);
                        }
                        break;
                    }
                    case "center":
                    case "figure":
                    case "figcaption":
                    case "caption":
                        // 居中容器（center 老帖高频 / figure 图容器 / figcaption 题注 / caption 表题，
                        // 用户裁定同款居中块）：块边界+子节点重入，centerContent 贯穿段/标题行盒居中
                        // （Chrome text-align 语义）；table 直下 caption 已在 table case 预提取，此处
                        // 兜 table 外裸 caption
                        Flush();
                        BuildHtmlNodes(node.ChildNodes, panel, listDepth, colorOverride, depth + 1, true);
                        break;
                    case "dl":
                    case "details":
                        // dl 定义列表容器 / details 折叠容器（无折叠交互，按已展开直排——用户裁定）：
                        // 块边界+子节点重入，dt/dd 各归其位
                        Flush();
                        BuildHtmlNodes(node.ChildNodes, panel, listDepth, colorOverride, depth + 1, centerContent);
                        break;
                    case "dt":
                        // 术语段：独立成段不加粗（用户裁定），同 p 口径
                        Flush();
                        AppendInlineChildren(node, colorOverride);
                        Flush();
                        break;
                    case "dd": {
                        // 定义描述段：独立容器物理位移 ListIndentPerDepth（CurrentIndentX 只被代码/
                        // 图片块宽度扣减消费，文字段不读它——缩进须像引用块一样 SetPosition 落地）；
                        // CurrentIndentX 同步进档让 dd 内代码块/图片按缩进后净宽构建
                        Flush();
                        float prevIndentDd = CurrentIndentX;
                        CurrentIndentX += ListIndentPerDepth;
                        StackPanelWidget ddPanel = CreatePanel();
                        if (centerContent) {
                            // 居中容器内 dd：撑满外壳剩余宽（Margin 左侧缩进扣余），居中锚点生效
                            ddPanel.HorizontalAlignment = WidgetAlignment.Stretch;
                        }
                        BuildHtmlNodes(node.ChildNodes, ddPanel, listDepth, colorOverride, depth + 1, centerContent);
                        CurrentIndentX = prevIndentDd;
                        CanvasWidget ddOuter = new() { Size = new Vector2(NestedContentWidth(), -1f) };
                        if (centerContent) {
                            // 居中容器内 dd：外壳撑满（同引用/列表行框口径）
                            ddOuter.HorizontalAlignment = WidgetAlignment.Stretch;
                        }
                        ddOuter.Children.Add(ddPanel);
                        // 缩进走 MarginLeft 不走 SetPosition（同 BuildHtmlList contentPanel 口径）
                        ddPanel.MarginLeft = ListIndentPerDepth;
                        // 格内注册 FlowShell（顶层不注册，行为不变）：dd 外壳宽归所在格 panel 按格净宽
                        // 回填（构建期 NestedContentWidth 是顶层近似值）；SetPosition 的 ListIndentPerDepth
                        // 是壳内内容位移，不进外壳条目（口径同 CreateHtmlQuote 的 +10）
                        CurrentPanel?.Shells.Add(new ShellEntry { Frame = ddOuter, IndentX = CurrentIndentX, Kind = ShellKind.FlowShell });
                        panel.Children.Add(ddOuter);
                        break;
                    }
                    case "summary":
                        // 折叠标题：开头补 ▼ 代表已展开（用户裁定），余同 p 口径
                        Flush();
                        HtmlSpanParser.AddText("▼ ", pendingSpans, default, colorOverride ?? DefaultColor);
                        AppendInlineChildren(node, colorOverride);
                        Flush();
                        break;
                    case "footer":
                        // 页脚：块级=独立成段（用户裁定），无特殊视觉。前照会合并不了本段
                        Flush();
                        HtmlSpanParser.ParseHtmlNode(node, pendingSpans, default, colorOverride ?? DefaultColor);
                        Flush();
                        break;
                    case "address":
                        // 联系地址：块级=独立成段（HTML 块级语义），斜体由 ParseHtmlNode→ApplyOpenTag 自带
                        //（此前块级上下文只透传子节点，斜体静默丢失）
                        Flush();
                        HtmlSpanParser.ParseHtmlNode(node, pendingSpans, default, colorOverride ?? DefaultColor);
                        Flush();
                        break;
                    default:
                        // 未知标签：子树含块级标签（section/article 包裹 pre/列表等）时递归块级遍历，
                        // 否则整节点进 span 解析——ApplyOpenTag/引号/色底后刷全生效。此前只透传子节点，
                        // 行内容器标签（b/i/q/kbd/span、裸置块级层的 br/img）自带样式被静默跳过
                        if (ContainsBlockChild(node)) {
                            Flush();
                            BuildHtmlNodes(node.ChildNodes, panel, listDepth, colorOverride, depth + 1, centerContent);
                        }
                        else {
                            HtmlSpanParser.ParseHtmlNode(node, pendingSpans, default, colorOverride ?? DefaultColor);
                        }
                        break;
                }
            }
            Flush();
        }

        void BuildHtmlList(HtmlAgilityPack.HtmlNode listNode, bool ordered, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth, bool centerContent = false) {
            if (depth > MaxDepth) {
                return;
            }
            StackPanelWidget listPanel = CreatePanel();
            listPanel.Margin = new Vector2(0f, 2f);
            if (centerContent) {
                // 居中容器内列表：列表面板撑满宿主（否则 Near 收缩到内容宽，行框 Stretch 无意义）
                listPanel.HorizontalAlignment = WidgetAlignment.Stretch;
            }
            // <ol start="4">：解析失败回退 1
            int number = int.TryParse(listNode.GetAttributeValue("start", null), out int parsedStart) ? parsedStart : 1;
            // 文字列按全列表最宽 marker 对齐（"10." 比 "1." 宽），保底 marker→文字间隙；
            // 嵌套列表挂在本级文字列内，缩进节奏恒一档 ListIndentPerDepth
            List<HtmlAgilityPack.HtmlNode> items = [.. listNode.Elements("li")];
            float markerScale = FontScale;
            float maxMarkerWidth = 0f;
            int scanNumber = number;
            foreach (HtmlAgilityPack.HtmlNode li in items) {
                maxMarkerWidth = MathUtils.Max(
                    maxMarkerWidth,
                    Font.MeasureText(HtmlTextUtils.GetListMarker(ordered, scanNumber, listDepth + 1), new Vector2(markerScale), Vector2.Zero).X
                );
                if (ordered) {
                    scanNumber++;
                }
            }
            float contentX = MathUtils.Max(ListIndentPerDepth, MarkerOffsetX + maxMarkerWidth + MarkerTextGap);
            foreach (HtmlAgilityPack.HtmlNode li in items) {
                // 行框宽随嵌套收窄（同 MarkdownToWidgetsBuilder.CreateListItem）
                CanvasWidget row = new() {
                    Size = new Vector2(NestedContentWidth(), -1f)
                };
                if (centerContent) {
                    // 居中容器内列表：行框撑满列表宽（NestedContentWidth()=-1 时 desired 收缩），
                    // 文字列 Stretch+锚点才有居中空间
                    row.HorizontalAlignment = WidgetAlignment.Stretch;
                }
                // 格内注册 FlowShell（顶层不注册，行为不变）：li 行框宽归所在格 panel 按格净宽
                // 回填（IndentX=行框挂载点缩进；消除格内 li 定宽/natural 污染列协商的 D1 源）
                CurrentPanel?.Shells.Add(new ShellEntry { Frame = row, IndentX = CurrentIndentX, Kind = ShellKind.FlowShell });
                FontTextWidget markerLabel = new() {
                    Text = HtmlTextUtils.GetListMarker(ordered, number, listDepth + 1),
                    Font = Font,
                    Color = colorOverride ?? DefaultColor,
                    FontScale = markerScale
                };
                row.Children.Add(markerLabel);
                CanvasWidget.SetPosition(markerLabel, new Vector2(MarkerOffsetX, MarkerOffsetY));
                StackPanelWidget contentPanel = CreatePanel();
                // 文字列缩进走 MarginLeft 不走 SetPosition（Canvas 对 SetPosition 子件按 desired 排布，
                // alignment 失效）：居中容器内列表由 Stretch 撑满行框剩余宽，文字列 HorizontalCenter
                // 才有居中空间；Near 时 x=MarginLeft 视觉不变
                contentPanel.MarginLeft = contentX;
                if (centerContent) {
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
                            if (centerContent) {
                                widget.HorizontalAlignment = WidgetAlignment.Stretch;
                                widget.TextAnchor = TextAnchor.HorizontalCenter;
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
                            or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "hr";
                    if (isBlock) {
                        FlushLi();
                        switch (child.Name.ToLowerInvariant()) {
                            case "ul":
                            case "ol":
                            case "menu":
                                BuildHtmlList(child, child.Name.Equals("ol", StringComparison.Ordinal), contentPanel, listDepth + 1, colorOverride, depth + 1, centerContent);
                                break;
                            case "blockquote":
                                contentPanel.Children.Add(CreateHtmlQuote(child, listDepth + 1, colorOverride, depth + 1, centerContent));
                                break;
                            case "pre": {
                                HtmlAgilityPack.HtmlNode code = child.Element("code") ?? child;
                                contentPanel.Children.Add(CreateCodeBlock(ReplaceBrWithNewline(code).Split('\n')));
                                break;
                            }
                            case "h1":
                            case "h2":
                            case "h3":
                            case "h4":
                            case "h5":
                            case "h6": {
                                RichTextWidget widget = CreateHeading(child.Name[1] - '0', colorOverride);
                                if (centerContent) {
                                    widget.HorizontalAlignment = WidgetAlignment.Stretch;
                                    widget.TextAnchor = TextAnchor.HorizontalCenter;
                                }
                                HtmlSpanParser.ParseHtmlNode(child, widget.Spans, default, widget.Color);
                                contentPanel.Children.Add(widget);
                                break;
                            }
                            case "hr":
                                contentPanel.Children.Add(CreateThematicBreak());
                                break;
                            default: {
                                // p/div/section/article/table：单节点重入顶层分发（BuildHtmlNodes），
                                // 与顶层同口径——纯行内整块成段、嵌块递归、li 内 p>img 不再静默丢
                                BuildHtmlNodes([child], contentPanel, listDepth + 1, colorOverride, depth + 1, centerContent);
                                break;
                            }
                        }
                    }
                    else if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                        && child.Name.Equals("img", StringComparison.OrdinalIgnoreCase)
                        && ImageLoader != null) {
                        // li 内行内 <img> 升级为图片块（同 AppendInlineChildren 口径）
                        FlushLi();
                        AddImageBlock(contentPanel, child.GetAttributeValue("src", null), child.GetAttributeValue("alt", null));
                    }
                    else {
                        HtmlSpanParser.ParseHtmlNode(child, pendingSpans, default, colorOverride ?? DefaultColor);
                    }
                }
                FlushLi();
                CurrentIndentX = prevIndent;
                listPanel.Children.Add(row);
                number++;
            }
            panel.Children.Add(listPanel);
        }

        CanvasWidget CreateHtmlQuote(HtmlAgilityPack.HtmlNode quoteNode, int listDepth, Color? colorOverride, int depth, bool centerContent = false) {
            CanvasWidget outer = CreateQuoteFrame(out StackPanelWidget inner);
            if (centerContent) {
                // 居中容器内引用：外壳与 inner 双层 Stretch 撑满（HtmlWidget 壳路径 ContentWidth=-1，
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
            BuildHtmlNodes(quoteNode.ChildNodes, inner, listDepth, QuoteColor, depth + 1, centerContent);
            CurrentIndentX = prevIndent;
            return outer;
        }

        // HTML 表格 → TablePanelWidget 直产（spec §7.1）：布局/列协商/格线全由底座承担，本方法只做
        // DOM→行/格映射。行收集：table 直下 tr 与 thead/tbody 内 tr 展平、tfoot 行沉底（HTML 渲染
        // 语义，FooterRows 记行数）；caption/colgroup/col 无对应视觉语义，跳过（caption 已在表 case
        // 预提取渲染）。thead 行或含 th 的行为表头（粗体由 builder 侧染 span，同 Markdown CreateTable
        // 口径）。无滚动兜底（包装类超宽横向滚动不随迁）：Σmin 超宿主时底座定格列 min 溢出
        TablePanelWidget CreateHtmlTable(HtmlAgilityPack.HtmlNode tableNode, Color? colorOverride, int depth, bool centerContent = false) {
            // 格内容相对格左缘定位（spec §5.3）：入口清零外层嵌套缩进——格 shell 的回填扣减基准
            // 是格净宽，li 内嵌套表格时外层缩进已由挂载位置体现，再计入会双扣；全部出口恢复
            //（prevIndent 纪律同 CreateHtmlQuote，含两处早退）
            float prevIndent = CurrentIndentX;
            CurrentIndentX = 0f;
            if (depth > MaxDepth) {
                CurrentIndentX = prevIndent;
                return null;
            }
            // 视觉对齐 MD 表现状：四开关全开+同 padding+同线色（InnerBorder 在场时 Header/Footer
            // Separator 不双画，仅 InnerBorder=false 的自定义场景生效；FooterSeparator 对称补齐）
            TablePanelWidget widget = new() {
                CellPadding = new Vector2(6f, 3f),
                BorderColor = MarkdownTableWidget.GridLineColor,
                OuterBorder = true,
                InnerBorder = true,
                HeaderSeparator = true,
                FooterSeparator = true,
                Margin = new Vector2(0f, 4f)
            };
            List<(HtmlAgilityPack.HtmlNode Row, bool FromThead)> rows = [];
            // tfoot 行收集沉底（HTML 渲染语义：footer 行无论 DOM 位置恒在表尾），行数设
            // FooterRows 驱动底座表尾结构（FooterSeparator 语义对称 HeaderSeparator）
            List<HtmlAgilityPack.HtmlNode> footerRows = [];
            foreach (HtmlAgilityPack.HtmlNode child in tableNode.ChildNodes) {
                if (child.NodeType != HtmlAgilityPack.HtmlNodeType.Element) {
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
                    // colspan/rowspan 属性：空/非法/≤0 → 1（Set 后仅非平凡值登记 attached 状态）
                    int colSpan = ParseSpanAttribute(cellNode, "colspan");
                    int rowSpan = ParseSpanAttribute(cellNode, "rowspan");
                    Widget cell = CreateHtmlCell(cellNode, isHeader, colorOverride, depth, centerContent);
                    if (colSpan > 1) {
                        TablePanelWidget.SetColSpan(cell, colSpan);
                    }
                    if (rowSpan > 1) {
                        TablePanelWidget.SetRowSpan(cell, rowSpan);
                    }
                    // M1：跳过被前行 rowspan 占住的槽位定真实列号（照 ExpandSlots cursor 口径）
                    while (occupied.Contains((r, column))) {
                        column++;
                    }
                    while (alignments.Count <= column) {
                        alignments.Add(null);
                    }
                    alignments[column] ??= ParseCellAlignment(cellNode, tr);
                    ApplyCellAnchor(cell, ToTextAnchor(alignments[column]));
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
                    TableRowWidget row = new() { Header = isHeader };
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
        Widget CreateHtmlCell(HtmlAgilityPack.HtmlNode cellNode, bool isHeader, Color? colorOverride, int depth, bool centerContent = false) {
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
                BuildHtmlNodes(cellNode.ChildNodes, panel, 0, colorOverride, depth + 1, centerContent);
                CurrentPanel = prevPanel;
                if (isHeader) {
                    BoldWidgetSpans(panel);
                }
                return panel;
            }
            // 纯行内：span 解析（img 命中 HtmlSpanParser 占位分支，照旧保留）
            List<TextSpan> spans = [];
            HtmlSpanParser.ParseHtmlNode(cellNode, spans, default, colorOverride ?? DefaultColor);
            if (isHeader) {
                BoldCellSpans(spans);
            }
            RichTextWidget widget = new() {
                Font = Font,
                FontScale = FontScale,
                Color = colorOverride ?? DefaultColor,
                WordWrap = true,
                // 拉满格矩形（track−2×CellPadding）：TextAnchor 居中/右对齐按 ActualSize 定偏移，
                // Near 会塌缩到内容宽
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch
            };
            widget.Spans.AddRange(spans);
            if (centerContent) {
                // 居中容器内的表格格文本（Chrome text-align 继承进 td；列对齐缺省时不覆写，见 ApplyCellAnchor）
                widget.TextAnchor = TextAnchor.HorizontalCenter;
            }
            if (LinkClicked != null) {
                widget.LinkClicked += href => LinkClicked(href);
            }
            // 按 float.MaxValue 自测拿自然宽（与旧 scratch Layout(float.MaxValue,∞) 同参对齐），
            // Size.X 锚定让底座 track=自然宽+2×CellPadding；Size.Y=-1 保持折行高自动
            widget.Measure(new Vector2(float.MaxValue, float.PositiveInfinity));
            widget.Size = new Vector2(widget.ParentDesiredSize.X + 2f * CellPaddingX, -1f);
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
                or "caption";
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
        // （CSS text-align 继承口径，嵌套表格一并）。列对齐只补缺省锚点：centerContent 已设
        // HorizontalCenter 的段/标题不被列缺省锚点覆写回 Default（<td><center> 老帖高频组合，
        // 无条件赋值会击穿居中；元素自身设置优先于继承）
        static void ApplyCellAnchor(Widget widget, TextAnchor anchor) {
            if (widget is RichTextWidget rich) {
                if (anchor != TextAnchor.Default
                    && rich.TextAnchor == TextAnchor.Default) {
                    rich.TextAnchor = anchor;
                }
                rich.HorizontalAlignment = WidgetAlignment.Stretch;
            }
            else if (widget is ContainerWidget container) {
                foreach (Widget child in container.Children) {
                    ApplyCellAnchor(child, anchor);
                }
            }
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
                    or "caption") {
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
