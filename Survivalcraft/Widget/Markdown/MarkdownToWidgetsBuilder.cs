using Engine;
using Engine.Media;
using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using MdBlock = Markdig.Syntax.Block;

namespace Game {
    // 块级 AST → widget 树。双入口：Markdig 块（Build）/ HtmlAgilityPack DOM（BuildHtml）；
    // 行内内容一律走 MarkdownSpanParser。明确不动 PostDetailsContentBuilder（ScbbsPlus 基线避免扰动）。
    // 布局事实：CanvasWidget 是唯一会把有限 Size 约束进 child 测量输入的容器（并按 child 位置扣减可用宽）；
    // Rectangle/BevelledRectangle 的 MeasureOverride 直接返回 Size（-1 表示排列时撑满父 cell）；
    // ScrollPanelWidget 不设 DesiredSize，故其外层必须显式定高
    public class MarkdownToWidgetsBuilder {
        public BitmapFont Font;

        public float FontScale = 1f;

        public Color DefaultColor = Color.White;

        // <0 跟随父可用宽（MarkdownWidget.MeasureOverride 解析代码块宽度）
        public float ContentWidth = -1f;

        // 非 event：允许宿主整体赋值/解绑
        public Action<string> LinkClicked;

        // 代码块外层 Canvas，宽度需在 MarkdownWidget.MeasureOverride 按可用宽解析（构建时父宽未知）
        public readonly List<CanvasWidget> CodeBlocks = [];

        // Strikethrough 扩展：默认 pipeline 不解析 ~~x~~（会留在字面量）
        // PipeTables：默认 pipeline 不解析 GFM 管道表（| a | b |）
        static readonly MarkdownPipeline s_pipeline = new MarkdownPipelineBuilder()
            .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
            .UsePipeTables()
            .Build();

        // 标题字号/金色阶梯（PRD 6.3，与 PostDetailsContentBuilder 同源风格）
        static readonly float[] s_headingScales = [1.6f, 1.4f, 1.2f, 1.1f, 1f, 1f];

        static readonly Color HeadingColor = new(255, 230, 140);

        static readonly Color QuoteColor = new(175, 175, 175);

        static readonly Color QuoteLineColor = new(120, 120, 120);

        static readonly Color CodeTextColor = new(225, 225, 228);

        static readonly Color CodeCenterColor = new(52, 52, 56);

        static readonly Color CodeBevelColor = new(90, 90, 95);

        static readonly Color ThematicBreakColor = new(120, 120, 120);

        const int ListIndentPerDepth = 22;

        // 块级嵌套深度上限（引用/列表/HTML 互嵌递归）：防御不可信输入的深嵌套
        const int MaxDepth = 64;

        public StackPanelWidget Build(string markdown) {
            MarkdownDocument document = Markdown.Parse(markdown ?? string.Empty, s_pipeline);
            StackPanelWidget panel = CreatePanel();
            BuildBlocks(document, panel, 0, null, 0);
            return panel;
        }

        public void BuildBlocks(IEnumerable<MdBlock> blocks, StackPanelWidget panel, int listDepth, Color? colorOverride) {
            BuildBlocks(blocks, panel, listDepth, colorOverride, 0);
        }

        void BuildBlocks(IEnumerable<MdBlock> blocks, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
            if (depth > MaxDepth) {
                return;
            }
            foreach (MdBlock block in blocks) {
                BuildBlock(block, panel, listDepth, colorOverride, depth);
            }
        }

        void BuildBlock(MdBlock block, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
            switch (block) {
                case HeadingBlock heading: {
                    RichTextWidget widget = CreateRichText(colorOverride);
                    widget.FontScale = FontScale * s_headingScales[MathUtils.Min(heading.Level, 6) - 1];
                    widget.Color = HeadingColor;
                    widget.Margin = new Vector2(0f, heading.Level <= 2 ? 8f : 4f);
                    if (heading.Inline != null) {
                        MarkdownSpanParser.Parse(heading.Inline, widget.Spans, default, widget.Color);
                    }
                    panel.Children.Add(widget);
                    break;
                }
                case ParagraphBlock paragraph: {
                    RichTextWidget widget = CreateRichText(colorOverride);
                    if (paragraph.Inline != null) {
                        MarkdownSpanParser.Parse(paragraph.Inline, widget.Spans, default, widget.Color);
                    }
                    panel.Children.Add(widget);
                    break;
                }
                case ListBlock list:
                    BuildList(list, panel, listDepth, colorOverride, depth);
                    break;
                case QuoteBlock quote:
                    panel.Children.Add(CreateQuote(quote, listDepth, colorOverride, depth));
                    break;
                case FencedCodeBlock fencedCode:
                    panel.Children.Add(CreateCodeBlock(GetBlockLines(fencedCode.Lines.Lines, fencedCode.Lines.Count)));
                    break;
                case CodeBlock indentedCode:
                    panel.Children.Add(CreateCodeBlock(GetBlockLines(indentedCode.Lines.Lines, indentedCode.Lines.Count)));
                    break;
                case ThematicBreakBlock:
                    panel.Children.Add(new RectangleWidget {
                        Size = new Vector2(1f, 1f),
                        HorizontalAlignment = WidgetAlignment.Stretch,
                        FillColor = ThematicBreakColor,
                        OutlineColor = Color.Transparent,
                        OutlineThickness = 0f,
                        Margin = new Vector2(0f, 6f)
                    });
                    break;
                case Table table:
                    panel.Children.Add(CreateTable(table, colorOverride));
                    break;
                case HtmlBlock htmlBlock:
                    BuildHtml(GetBlockText(htmlBlock), panel, listDepth, colorOverride, depth + 1);
                    break;
                default: {
                    // 未知块（自定义扩展等）：取源文本按段落兜底，避免内容静默丢失
                    string text = GetBlockText(block);
                    if (!string.IsNullOrWhiteSpace(text)) {
                        RichTextWidget widget = CreateRichText(colorOverride);
                        widget.Spans.Add(new TextSpan(text.TrimEnd()));
                        panel.Children.Add(widget);
                    }
                    break;
                }
            }
        }

        void BuildList(ListBlock list, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
            StackPanelWidget listPanel = CreatePanel();
            listPanel.Margin = new Vector2(0f, 2f);
            // 有序列表起始编号：Markdig 只给 OrderedStart 原始串（"3." 的 "3"），自己转 int（失败回退 1）
            int number = list.IsOrdered && int.TryParse(list.OrderedStart, out int orderedStart) ? orderedStart : 1;
            foreach (ListItemBlock item in list) {
                listPanel.Children.Add(CreateListItem(list.IsOrdered, number, listDepth, item, colorOverride, markdigItem: true, depth));
                number++;
            }
            panel.Children.Add(listPanel);
        }

        // 项 = CanvasWidget 行：marker 列 + 内容列。CanvasWidget 会按 child 位置自动扣减测量可用宽，
        // 内容列换行宽 = 行宽 - 缩进；行内内容与嵌套块共用一个竖排 StackPanel
        CanvasWidget CreateListItem(bool ordered, int number, int listDepth, IEnumerable<MdBlock> markdigBlocks, Color? colorOverride, bool markdigItem, int depth) {
            CanvasWidget row = new() {
                Size = new Vector2(ContentWidth > 0f ? ContentWidth : -1f, -1f)
            };
            LabelWidget markerLabel = new() {
                Text = HtmlTextUtils.GetListMarker(ordered, number, listDepth + 1),
                Font = Font,
                Color = colorOverride ?? DefaultColor,
                FontScale = FontScale * 0.8f
            };
            row.Children.Add(markerLabel);
            CanvasWidget.SetPosition(markerLabel, new Vector2(ListIndentPerDepth * listDepth + 4f, 3f));
            StackPanelWidget contentPanel = CreatePanel();
            row.Children.Add(contentPanel);
            CanvasWidget.SetPosition(contentPanel, new Vector2(ListIndentPerDepth * (listDepth + 1), 0f));
            bool hasBlocks = false;
            if (markdigItem) {
                foreach (MdBlock itemBlock in markdigBlocks) {
                    // 列表项内的段落与其标记同行
                    if (itemBlock is ParagraphBlock paragraphBlock) {
                        RichTextWidget widget = CreateRichText(colorOverride);
                        if (paragraphBlock.Inline != null) {
                            MarkdownSpanParser.Parse(paragraphBlock.Inline, widget.Spans, default, widget.Color);
                        }
                        contentPanel.Children.Add(widget);
                    }
                    else {
                        BuildBlock(itemBlock, contentPanel, listDepth + 1, colorOverride, depth + 1);
                    }
                    hasBlocks = true;
                }
            }
            if (!hasBlocks) {
                contentPanel.Children.Add(CreateRichText(colorOverride));
            }
            return row;
        }

        // 引用块：左竖线（-1 高度随内容拉伸）+ 灰字 + 左缩进
        CanvasWidget CreateQuote(ContainerBlock quote, int listDepth, Color? colorOverride, int depth) {
            CanvasWidget outer = new() {
                Size = new Vector2(ContentWidth > 0f ? ContentWidth : -1f, -1f),
                Margin = new Vector2(0f, 4f)
            };
            RectangleWidget line = new() {
                Size = new Vector2(2f, 1f),
                VerticalAlignment = WidgetAlignment.Stretch,
                FillColor = QuoteLineColor,
                OutlineColor = Color.Transparent,
                OutlineThickness = 0f
            };
            outer.Children.Add(line);
            StackPanelWidget inner = CreatePanel();
            inner.Margin = new Vector2(0f, 2f);
            BuildBlocks(quote, inner, listDepth, QuoteColor, depth + 1);
            outer.Children.Add(inner);
            CanvasWidget.SetPosition(inner, new Vector2(10f, 0f));
            return outer;
        }

        // 表格：Table→TableRow(IsHeader)→TableCell（1.4.0 无 Inline 属性，行内挂在 cell 内 ParagraphBlock.Inline 上）；
        // 列对齐在 Table.ColumnDefinitions[i].Alignment；表头行染粗体 span（跟随换行）
        MarkdownTableWidget CreateTable(Table table, Color? colorOverride) {
            MarkdownTableWidget widget = new() {
                Font = Font,
                FontScale = FontScale,
                DefaultColor = colorOverride ?? DefaultColor
            };
            if (LinkClicked != null) {
                widget.LinkClicked += href => LinkClicked(href);
            }
            var alignments = new List<MarkdownTableAlignment?>();
            foreach (TableColumnDefinition definition in table.ColumnDefinitions) {
                alignments.Add(definition.Alignment switch {
                    TableColumnAlign.Center => MarkdownTableAlignment.Center,
                    TableColumnAlign.Right => MarkdownTableAlignment.Right,
                    TableColumnAlign.Left => MarkdownTableAlignment.Left,
                    _ => null
                });
            }
            widget.SetColumnAlignments(alignments);
            foreach (MdBlock child in table) {
                if (child is not TableRow row) {
                    continue;
                }
                bool isHeader = row.IsHeader;
                List<List<TextSpan>> cells = [];
                foreach (MdBlock rowChild in row) {
                    if (rowChild is not TableCell cell) {
                        continue;
                    }
                    List<TextSpan> spans = [];
                    foreach (MdBlock cellChild in cell) {
                        if (cellChild is ParagraphBlock paragraph
                            && paragraph.Inline != null) {
                            MarkdownSpanParser.Parse(paragraph.Inline, spans, default, widget.DefaultColor);
                        }
                    }
                    if (isHeader) {
                        foreach (TextSpan span in spans) {
                            span.Style = span.Style switch {
                                RichTextStyle.Italic => RichTextStyle.BoldItalic,
                                RichTextStyle.Normal => RichTextStyle.Bold,
                                _ => span.Style
                            };
                        }
                    }
                    cells.Add(spans);
                }
                widget.AddRow(isHeader, cells);
            }
            return widget;
        }

        // HtmlAgilityPack DOM 入口（HtmlBlock / 外部 HTML 片段）
        public void BuildHtml(string html, StackPanelWidget panel, int listDepth, Color? colorOverride) {
            BuildHtml(html, panel, listDepth, colorOverride, 0);
        }

        void BuildHtml(string html, StackPanelWidget panel, int listDepth, Color? colorOverride, int depth) {
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
                    RichTextWidget widget = CreateRichText(colorOverride);
                    widget.Spans.AddRange(pendingSpans);
                    panel.Children.Add(widget);
                    pendingSpans.Clear();
                }
            }
            foreach (HtmlAgilityPack.HtmlNode node in nodes) {
                if (node.NodeType != HtmlAgilityPack.HtmlNodeType.Element
                    && node.NodeType != HtmlAgilityPack.HtmlNodeType.Text) {
                    continue;
                }
                if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Text) {
                    MarkdownSpanParser.ParseHtmlNode(node, pendingSpans, default, colorOverride ?? DefaultColor);
                    continue;
                }
                switch (node.Name.ToLowerInvariant()) {
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6": {
                        Flush();
                        int level = node.Name[1] - '0';
                        RichTextWidget widget = CreateRichText(colorOverride);
                        widget.FontScale = FontScale * s_headingScales[MathUtils.Min(level, 6) - 1];
                        widget.Color = HeadingColor;
                        widget.Margin = new Vector2(0f, level <= 2 ? 8f : 4f);
                        MarkdownSpanParser.ParseHtmlNode(node, widget.Spans, default, widget.Color);
                        panel.Children.Add(widget);
                        break;
                    }
                    case "p":
                    case "div":
                        Flush();
                        MarkdownSpanParser.ParseHtmlNode(node, pendingSpans, default, colorOverride ?? DefaultColor);
                        Flush();
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
                        panel.Children.Add(new RectangleWidget {
                            Size = new Vector2(1f, 1f),
                            HorizontalAlignment = WidgetAlignment.Stretch,
                            FillColor = ThematicBreakColor,
                            OutlineColor = Color.Transparent,
                            OutlineThickness = 0f,
                            Margin = new Vector2(0f, 6f)
                        });
                        break;
                    default:
                        // 未知块级标签：透传子节点累积为段
                        MarkdownSpanParser.ParseHtmlNode(node, pendingSpans, default, colorOverride ?? DefaultColor);
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
            foreach (HtmlAgilityPack.HtmlNode li in listNode.Elements("li")) {
                CanvasWidget row = new() {
                    Size = new Vector2(ContentWidth > 0f ? ContentWidth : -1f, -1f)
                };
                LabelWidget markerLabel = new() {
                    Text = HtmlTextUtils.GetListMarker(ordered, number, listDepth + 1),
                    Font = Font,
                    Color = colorOverride ?? DefaultColor,
                    FontScale = FontScale * 0.8f
                };
                row.Children.Add(markerLabel);
                CanvasWidget.SetPosition(markerLabel, new Vector2(ListIndentPerDepth * listDepth + 4f, 3f));
                StackPanelWidget contentPanel = CreatePanel();
                row.Children.Add(contentPanel);
                CanvasWidget.SetPosition(contentPanel, new Vector2(ListIndentPerDepth * (listDepth + 1), 0f));
                // li 内行内内容累积成段；内嵌块级标签递归处理
                List<TextSpan> pendingSpans = [];
                foreach (HtmlAgilityPack.HtmlNode child in li.ChildNodes) {
                    bool isBlock = child.NodeType == HtmlAgilityPack.HtmlNodeType.Element
                        && child.Name.ToLowerInvariant() is "ul" or "ol" or "blockquote" or "pre" or "p" or "div";
                    if (isBlock) {
                        if (pendingSpans.Count > 0) {
                            RichTextWidget widget = CreateRichText(colorOverride);
                            widget.Spans.AddRange(pendingSpans);
                            contentPanel.Children.Add(widget);
                            pendingSpans = [];
                        }
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
                                MarkdownSpanParser.ParseHtmlNode(child, widget.Spans, default, widget.Color);
                                contentPanel.Children.Add(widget);
                                break;
                            }
                        }
                    }
                    else {
                        MarkdownSpanParser.ParseHtmlNode(child, pendingSpans, default, colorOverride ?? DefaultColor);
                    }
                }
                if (pendingSpans.Count > 0) {
                    RichTextWidget widget = CreateRichText(colorOverride);
                    widget.Spans.AddRange(pendingSpans);
                    contentPanel.Children.Add(widget);
                }
                listPanel.Children.Add(row);
                number++;
            }
            panel.Children.Add(listPanel);
        }

        CanvasWidget CreateHtmlQuote(HtmlAgilityPack.HtmlNode quoteNode, int listDepth, Color? colorOverride, int depth) {
            CanvasWidget outer = new() {
                Size = new Vector2(ContentWidth > 0f ? ContentWidth : -1f, -1f),
                Margin = new Vector2(0f, 4f)
            };
            RectangleWidget line = new() {
                Size = new Vector2(2f, 1f),
                VerticalAlignment = WidgetAlignment.Stretch,
                FillColor = QuoteLineColor,
                OutlineColor = Color.Transparent,
                OutlineThickness = 0f
            };
            outer.Children.Add(line);
            StackPanelWidget inner = CreatePanel();
            inner.Margin = new Vector2(0f, 2f);
            BuildHtmlNodes(quoteNode.ChildNodes, inner, listDepth, QuoteColor, depth + 1);
            outer.Children.Add(inner);
            CanvasWidget.SetPosition(inner, new Vector2(10f, 0f));
            return outer;
        }

        // 代码块：BevelledRectangle 框 + 横向 ScrollPanel + 逐行 Label（不换行，保留原始空白）。
        // ScrollPanel 不产出 DesiredSize：不定位的子节点由外层 Canvas 排满整个 cell，
        // 外层高度显式定（行高累计 + 滚动条/内边距余量），文字内边距走 linesPanel Margin
        CanvasWidget CreateCodeBlock(string[] lines) {
            if (lines.Length == 0
                || (lines.Length == 1 && lines[0].Length == 0)) {
                lines = [" "];
            }
            float codeScale = 0.9f;
            float lineHeight = ((Font?.GlyphHeight ?? 51f) + (Font?.Spacing.Y ?? 0f)) * codeScale * (Font?.Scale ?? 1f) + 1f;
            float contentHeight = lines.Length * lineHeight;
            CanvasWidget outer = new() {
                Size = new Vector2(ContentWidth > 0f ? ContentWidth : -1f, contentHeight + 20f),
                Margin = new Vector2(0f, 4f)
            };
            BevelledRectangleWidget frame = new() {
                Size = new Vector2(1f, 1f),
                HorizontalAlignment = WidgetAlignment.Stretch,
                VerticalAlignment = WidgetAlignment.Stretch,
                CenterColor = CodeCenterColor,
                BevelColor = CodeBevelColor
            };
            outer.Children.Add(frame);
            ScrollPanelWidget scrollPanel = new() {
                Direction = LayoutDirection.Horizontal,
                ScrollPosition = 0f
            };
            StackPanelWidget linesPanel = new() {
                Direction = LayoutDirection.Vertical,
                Margin = new Vector2(8f, 6f)
            };
            foreach (string line in lines) {
                linesPanel.Children.Add(new LabelWidget {
                    Text = line.Length == 0 ? " " : line,
                    Font = Font,
                    Color = CodeTextColor,
                    FontScale = codeScale,
                    WordWrap = false
                });
            }
            scrollPanel.Children.Add(linesPanel);
            outer.Children.Add(scrollPanel);
            CodeBlocks.Add(outer);
            return outer;
        }

        RichTextWidget CreateRichText(Color? colorOverride) {
            RichTextWidget widget = new() {
                WordWrap = true,
                Font = Font,
                FontScale = FontScale,
                Color = colorOverride ?? DefaultColor,
                Margin = new Vector2(0f, 2f)
            };
            if (LinkClicked != null) {
                widget.LinkClicked += href => LinkClicked(href);
            }
            return widget;
        }

        static StackPanelWidget CreatePanel() => new() {
            Direction = LayoutDirection.Vertical
        };

        static string[] GetBlockLines(Markdig.Helpers.StringLine[] lines, int count) {
            string[] result = new string[count];
            for (int i = 0; i < count; i++) {
                result[i] = lines[i].ToString();
            }
            return result;
        }

        static string GetBlockText(MdBlock block) {
            return GetBlockText(block, 0);
        }

        static string GetBlockText(MdBlock block, int depth) {
            if (depth > MaxDepth) {
                return string.Empty;
            }
            if (block is LeafBlock leafBlock) {
                return string.Join("\n", GetBlockLines(leafBlock.Lines.Lines, leafBlock.Lines.Count));
            }
            var builder = new System.Text.StringBuilder();
            if (block is ContainerBlock container) {
                foreach (MdBlock child in container) {
                    builder.AppendLine(GetBlockText(child, depth + 1));
                }
            }
            return builder.ToString();
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
