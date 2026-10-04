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
        // 参数放宽 IEnumerable：li 内块级子节点以单节点数组重入（复用顶层分发口径）
        void BuildHtmlNodes(IEnumerable<HtmlAgilityPack.HtmlNode> nodes, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
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
                        BuildHtmlNodes(node.ChildNodes, panel, listDepth, colorOverride, depth + 1);
                        break;
                    case "ul":
                    case "ol":
                        Flush();
                        BuildHtmlList(node, node.Name.Equals("ol", StringComparison.Ordinal), panel, listDepth, colorOverride, depth);
                        break;
                    case "blockquote":
                        Flush();
                        panel.Children.Add(CreateHtmlQuote(node, listDepth, colorOverride, depth));
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
                        // 空表（无任何行）返回 null 不产部件——零尺寸 widget 仍会靠 Margin 贡献 8px 空白
                        TablePanelWidget table = CreateHtmlTable(node, colorOverride, depth);
                        if (table != null) {
                            panel.Children.Add(table);
                        }
                        break;
                    }
                    default:
                        // 未知标签：子树含块级标签（section/article 包裹 pre/列表等）时递归块级遍历，
                        // 否则透传子节点累积为段（b/i/span 等行内标签）
                        if (ContainsBlockChild(node)) {
                            Flush();
                            BuildHtmlNodes(node.ChildNodes, panel, listDepth, colorOverride, depth + 1);
                        }
                        else {
                            AppendInlineChildren(node, colorOverride);
                        }
                        break;
                }
            }
            Flush();
        }

        void BuildHtmlList(HtmlAgilityPack.HtmlNode listNode, bool ordered, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
            if (depth > MaxDepth) {
                return;
            }
            StackPanelWidget listPanel = CreatePanel();
            listPanel.Margin = new Vector2(0f, 2f);
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
                row.Children.Add(contentPanel);
                CanvasWidget.SetPosition(contentPanel, new Vector2(contentX, 0f));
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
                            widget.Spans.AddRange(pendingSpans);
                            contentPanel.Children.Add(widget);
                        }
                        pendingSpans = [];
                    }
                }
                foreach (HtmlAgilityPack.HtmlNode child in li.ChildNodes) {
                    bool isBlock = child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                        && child.Name.ToLowerInvariant() is "ul" or "ol" or "blockquote" or "pre" or "p" or "div"
                            or "section" or "article" or "table" or "footer"
                            or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "hr";
                    if (isBlock) {
                        FlushLi();
                        switch (child.Name.ToLowerInvariant()) {
                            case "ul":
                            case "ol":
                                BuildHtmlList(child, child.Name.Equals("ol", StringComparison.Ordinal), contentPanel, listDepth + 1, colorOverride, depth + 1);
                                break;
                            case "blockquote":
                                contentPanel.Children.Add(CreateHtmlQuote(child, listDepth + 1, colorOverride, depth + 1));
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
                                BuildHtmlNodes([child], contentPanel, listDepth + 1, colorOverride, depth + 1);
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

        CanvasWidget CreateHtmlQuote(HtmlAgilityPack.HtmlNode quoteNode, int listDepth, Color? colorOverride, int depth) {
            CanvasWidget outer = CreateQuoteFrame(out StackPanelWidget inner);
            // 格内注册 FlowShell（顶层不注册，行为不变）：quote 外壳宽归所在格 panel 按格净宽
            // 回填——构建期 NestedContentWidth 是顶层近似值。IndentX=本块挂载点缩进（inner 的
            // +10 是壳内内容缩进，不进外壳条目）
            CurrentPanel?.Shells.Add(new ShellEntry { Frame = outer, IndentX = CurrentIndentX, Kind = ShellKind.FlowShell });
            float prevIndent = CurrentIndentX;
            CurrentIndentX += 10f;
            BuildHtmlNodes(quoteNode.ChildNodes, inner, listDepth, QuoteColor, depth + 1);
            CurrentIndentX = prevIndent;
            return outer;
        }

        // HTML 表格 → TablePanelWidget 直产（spec §7.1）：布局/列协商/格线全由底座承担，本方法只做
        // DOM→行/格映射。行收集：table 直下 tr 与 thead/tbody/tfoot 内 tr 展平（caption/colgroup/col
        // 无对应视觉语义，跳过）；thead 行或含 th 的行为表头（粗体由 builder 侧染 span，同 Markdown
        // CreateTable 口径）。无滚动兜底（包装类超宽横向滚动不随迁）：Σmin 超宿主时底座定格列 min 溢出
        TablePanelWidget CreateHtmlTable(HtmlAgilityPack.HtmlNode tableNode, Color? colorOverride, int depth) {
            // 格内容相对格左缘定位（spec §5.3）：入口清零外层嵌套缩进——格 shell 的回填扣减基准
            // 是格净宽，li 内嵌套表格时外层缩进已由挂载位置体现，再计入会双扣；全部出口恢复
            //（prevIndent 纪律同 CreateHtmlQuote，含两处早退）
            float prevIndent = CurrentIndentX;
            CurrentIndentX = 0f;
            if (depth > MaxDepth) {
                CurrentIndentX = prevIndent;
                return null;
            }
            // 视觉对齐 MD 表现状：三开关全开+同 padding+同线色（InnerBorder 在场时 HeaderSeparator
            // 不双画，仅 InnerBorder=false 的自定义场景生效）
            TablePanelWidget widget = new() {
                CellPadding = new Vector2(6f, 3f),
                BorderColor = MarkdownTableWidget.GridLineColor,
                OuterBorder = true,
                InnerBorder = true,
                HeaderSeparator = true,
                Margin = new Vector2(0f, 4f)
            };
            List<(HtmlAgilityPack.HtmlNode Row, bool FromThead)> rows = [];
            foreach (HtmlAgilityPack.HtmlNode child in tableNode.ChildNodes) {
                if (child.NodeType != HtmlAgilityPack.HtmlNodeType.Element) {
                    continue;
                }
                string childTag = child.Name.ToLowerInvariant();
                if (childTag == "tr") {
                    rows.Add((child, false));
                }
                else if (childTag is "thead" or "tbody" or "tfoot") {
                    foreach (HtmlAgilityPack.HtmlNode tr in child.Elements("tr")) {
                        rows.Add((tr, childTag == "thead"));
                    }
                }
            }
            if (rows.Count == 0) {
                CurrentIndentX = prevIndent;
                return null;
            }
            // 剔除空行（无 td/th——本方法不产行、底座无对应布局行），使行序号=ExpandSlots 布局行号
            rows.RemoveAll(entry => !HasCellElement(entry.Row));
            // 列对齐：格级属性降列级——自上而下取每列第一个出现的对齐（td/th 与 tr 的 align 属性、
            // style text-align 都认），此后该列锚点固定（照包装类 SetColumnAlignments 语义）；
            // 列号照底座 ExpandSlots 口径模拟（M1）：cursor 跳过被前行 rowspan 登记的占格——旧口径
            // 仅按前置 colspan 累计，rowspan+列对齐组合下 anchor 染错列（视觉错对齐）。ExpandSlots
            // 的越界 colspan 二次钳（columns−Col）发生在全部 slot.Col 定值之后，不影响列号，故模拟
            // 对全部输入与布局期定列逐槽一致
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
                    Widget cell = CreateHtmlCell(cellNode, isHeader, colorOverride, depth);
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
                    // 列覆盖 [column, column+colSpan)，colspan 用未钳值与第一遍 cursor 推进同口径）
                    for (int rr = r + 1; rr < MathUtils.Min(r + rowSpan, rows.Count); rr++) {
                        for (int cc = column; cc < column + colSpan; cc++) {
                            occupied.Add((rr, cc));
                        }
                    }
                    column += colSpan;
                }
                if (cells.Count > 0) {
                    TableRowWidget row = new() { Header = isHeader };
                    foreach (Widget cell in cells) {
                        row.Children.Add(cell);
                    }
                    widget.Children.Add(row);
                }
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
        Widget CreateHtmlCell(HtmlAgilityPack.HtmlNode cellNode, bool isHeader, Color? colorOverride, int depth) {
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
                BuildHtmlNodes(cellNode.ChildNodes, panel, 0, colorOverride, depth + 1);
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
            if (LinkClicked != null) {
                widget.LinkClicked += href => LinkClicked(href);
            }
            // 按 float.MaxValue 自测拿自然宽（与旧 scratch Layout(float.MaxValue,∞) 同参对齐），
            // Size.X 锚定让底座 track=自然宽+2×CellPadding；Size.Y=-1 保持折行高自动
            widget.Measure(new Vector2(float.MaxValue, float.PositiveInfinity));
            widget.Size = new Vector2(widget.ParentDesiredSize.X + 2f * CellPaddingX, -1f);
            return widget;
        }

        // td 直下块级子标签判定（li 内块级同名单）：命中即整格走块级直装。footer 按用户裁定
        // 当块级元素（独立成段，无特殊视觉），走 default 单节点重入
        static bool IsBlockTag(string tag) {
            return tag.ToLowerInvariant() is "p" or "div" or "section" or "article" or "table"
                or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "hr" or "pre" or "blockquote"
                or "ul" or "ol" or "footer";
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
        // （CSS text-align 继承口径，嵌套表格一并）
        static void ApplyCellAnchor(Widget widget, TextAnchor anchor) {
            if (widget is RichTextWidget rich) {
                rich.TextAnchor = anchor;
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
                if (child.Name.ToLowerInvariant() is "pre" or "ul" or "ol" or "blockquote" or "p" or "div" or "hr" or "table"
                    or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "footer") {
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
