using Engine;

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

        // 块级 HTML 节点遍历：行内节点累积成段，遇块级标签先冲刷当前段
        void BuildHtmlNodes(HtmlAgilityPack.HtmlNodeCollection nodes, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
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
                        && child.Name.ToLowerInvariant() is "ul" or "ol" or "blockquote" or "pre" or "p" or "div";
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
                            default: {
                                RichTextWidget widget = CreateRichText(colorOverride);
                                HtmlSpanParser.ParseHtmlNode(child, widget.Spans, default, widget.Color);
                                contentPanel.Children.Add(widget);
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
            float prevIndent = CurrentIndentX;
            CurrentIndentX += 10f;
            BuildHtmlNodes(quoteNode.ChildNodes, inner, listDepth, QuoteColor, depth + 1);
            CurrentIndentX = prevIndent;
            return outer;
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
                    or "h1" or "h2" or "h3" or "h4" or "h5" or "h6") {
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
