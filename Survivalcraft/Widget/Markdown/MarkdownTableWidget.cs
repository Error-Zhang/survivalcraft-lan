using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    public enum MarkdownTableAlignment {
        Left,
        Center,
        Right
    }

    // Markdown 表格部件（P3，设计＝temp/MarkdownSupportPRD.md §7 + temp/TableLayoutResearch.md）：
    // 两阶段列宽协商——先测每列自然宽与最小宽（min 列宽 = max(2 汉字宽, 最长拉丁词)，CJK 字间可断、词不拆），
    // 自然总宽 ≤ 父宽时保持自然宽左对齐不摊满；超出时 rich 式削峰收缩（并列最宽列压到次宽水平逐层下压，
    // 各列保底 min）；压到 min 仍超 → 整体包横向 ScrollPanel。每格一个 RichTextWidget 子件
    // （复用排版/链接命中/伪粗体/背景/删除线），全边框格线用 layer=-1 FlatBatch quad（不用字体字符）。
    // 无斑马条纹；表头粗体由 builder 侧染 span Style，跟随内容换行
    public class MarkdownTableWidget : ContainerWidget {
        // 格线色（淡，半透明灰）；case 目验调
        public static Color GridLineColor = new(160, 160, 160, 90);

        const float CellPaddingX = 6f;

        const float CellPaddingY = 3f;

        const float LineThickness = 1f;

        // 滚动模式下 ScrollPanel 高度 = 表格高 + 此余量：滚动条画在 ActualSize.Y-5..-1，
        // 不留余量会压在表格底框线上
        const float ScrollBarReserve = 8f;

        class CellData {
            public List<TextSpan> Spans;

            public RichTextWidget Widget;

            public float Height;
        }

        class RowData {
            public bool IsHeader;

            public List<CellData> Cells = [];

            public float Height;
        }

        // 格线与单元格排布容器；测量/排布数据由 owner 计算好存字段
        class TableGridWidget : ContainerWidget {
            readonly MarkdownTableWidget m_owner;

            public TableGridWidget(MarkdownTableWidget owner) {
                m_owner = owner;
            }

            public override void MeasureOverride(Vector2 parentAvailableSize) {
                IsDrawRequired = true;
                DesiredSize = new Vector2(m_owner.m_tableWidth, m_owner.m_tableHeight);
            }

            public override void ArrangeOverride() {
                float y = 0f;
                foreach (RowData row in m_owner.m_rows) {
                    float x = 0f;
                    for (int c = 0; c < row.Cells.Count; c++) {
                        float width = m_owner.m_columnWidths[c];
                        row.Cells[c].Widget.Arrange(
                            new Vector2(x + CellPaddingX, y + CellPaddingY),
                            new Vector2(MathUtils.Max(width - 2f * CellPaddingX, 0f), MathUtils.Max(row.Height - 2f * CellPaddingY, 0f))
                        );
                        x += width;
                    }
                    y += row.Height;
                }
            }

            public override void Draw(DrawContext dc) {
                // 全边框格线：外框 + 行分隔线（rows+1 条横线）+ 列分隔线（cols+1 条竖线）。
                // 四角已各自经 GlobalTransform 预变换入批（同 RichTextWidget 的 QueueTransformedQuad），
                // 不能再对 batch 做 TransformTriangles（共享 batch 的顶点区间在多 draw-item 间不可靠，且会二次变换）
                FlatBatch2D batch = dc.PrimitivesRenderer2D.FlatBatch(-1, DepthStencilState.None);
                Color color = GridLineColor * GlobalColorTransform;
                if (color.A > 0
                    && m_owner.m_tableWidth > 0f
                    && m_owner.m_tableHeight > 0f) {
                    float width = m_owner.m_tableWidth;
                    float height = m_owner.m_tableHeight;
                    // 横线：顶边、每行分界、底边（底边回缩一个线厚防出界）
                    QueueQuad(batch, 0f, 0f, width, LineThickness, color);
                    float y = 0f;
                    foreach (RowData row in m_owner.m_rows) {
                        y += row.Height;
                        QueueQuad(batch, 0f, MathUtils.Min(y, height - LineThickness), width, LineThickness, color);
                    }
                    // 竖线：左边、每列分界、右边
                    QueueQuad(batch, 0f, 0f, LineThickness, height, color);
                    float x = 0f;
                    for (int c = 0; c < m_owner.m_columnWidths.Length; c++) {
                        x += m_owner.m_columnWidths[c];
                        QueueQuad(batch, MathUtils.Min(x, width - LineThickness), 0f, LineThickness, height, color);
                    }
                }
            }

            // 局部坐标 quad 四角各自经 GlobalTransform 变换后入批（同 RichTextWidget，斜切/旋转变换下不塌陷）
            void QueueQuad(FlatBatch2D batch, float x, float y, float w, float h, Color color) {
                Matrix m = GlobalTransform;
                Vector2 p1 = Vector2.Transform(new Vector2(x, y), m);
                Vector2 p2 = Vector2.Transform(new Vector2(x + w, y), m);
                Vector2 p3 = Vector2.Transform(new Vector2(x + w, y + h), m);
                Vector2 p4 = Vector2.Transform(new Vector2(x, y + h), m);
                batch.QueueQuad(p1, p2, p3, p4, 0f, color);
            }
        }

        readonly List<RowData> m_rows = [];

        readonly List<MarkdownTableAlignment?> m_alignments = [];

        readonly TableGridWidget m_grid;

        ScrollPanelWidget m_scrollPanel;

        float[] m_columnWidths = [];

        float m_tableWidth;

        float m_tableHeight;

        // 视口宽（滚动模式下 ScrollPanel 的 ActualSize）
        float m_viewportWidth;

        float m_cachedAvailableWidth = float.NaN;

        bool m_scrollMode;

        // cells 在 AddRow 时快照字体属性，故 Font/FontScale/DefaultColor 须在加行前设置
        public BitmapFont Font { get; set; }

        public float FontScale { get; set; } = 1f;

        public Color DefaultColor { get; set; } = Color.White;

        public event Action<string> LinkClicked;

        public MarkdownTableWidget() {
            Margin = new Vector2(0f, 4f);
            m_grid = new TableGridWidget(this);
        }

        public void AddRow(bool isHeader, List<List<TextSpan>> cells) {
            RowData row = new() { IsHeader = isHeader };
            foreach (List<TextSpan> spans in cells) {
                RichTextWidget widget = new() {
                    Font = Font,
                    FontScale = FontScale,
                    Color = DefaultColor,
                    WordWrap = true
                };
                widget.Spans.AddRange(spans);
                if (LinkClicked != null) {
                    widget.LinkClicked += href => LinkClicked(href);
                }
                m_grid.Children.Add(widget);
                row.Cells.Add(new CellData { Spans = spans, Widget = widget });
            }
            m_rows.Add(row);
            m_cachedAvailableWidth = float.NaN;
        }

        // 与列一一对应（GFM 冒号对齐；缺省/超出部分左对齐）
        public void SetColumnAlignments(IReadOnlyList<MarkdownTableAlignment?> alignments) {
            m_alignments.Clear();
            m_alignments.AddRange(alignments);
            m_cachedAvailableWidth = float.NaN;
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsDrawRequired = true;
            float availX = MathUtils.Max(parentAvailableSize.X, 0f);
            if (availX != m_cachedAvailableWidth) {
                m_cachedAvailableWidth = availX;
                Relayout(availX);
            }
            UpdateChildrenStructure(availX);
            float height = m_scrollMode ? m_tableHeight + ScrollBarReserve : m_tableHeight;
            // 子件测量必须由本部件驱动：ScrollPanel 测 child 时 X 方向不约束（∞），m_grid 测量返回既定尺寸
            if (m_scrollMode
                && m_scrollPanel != null) {
                m_scrollPanel.Measure(new Vector2(m_viewportWidth, height));
            }
            else {
                m_grid.Measure(new Vector2(m_tableWidth, float.PositiveInfinity));
            }
            DesiredSize = new Vector2(m_scrollMode ? m_viewportWidth : m_tableWidth, height);
        }

        public override void ArrangeOverride() {
            if (m_scrollMode
                && m_scrollPanel != null) {
                m_scrollPanel.Arrange(Vector2.Zero, new Vector2(m_viewportWidth, m_tableHeight + ScrollBarReserve));
                return;
            }
            m_grid.Arrange(Vector2.Zero, new Vector2(m_tableWidth, m_tableHeight));
        }

        void UpdateChildrenStructure(float availX) {
            if (m_scrollMode) {
                if (m_scrollPanel == null) {
                    m_scrollPanel = new ScrollPanelWidget {
                        Direction = LayoutDirection.Horizontal,
                        ScrollPosition = 0f,
                        ScrollSpeed = 0f
                    };
                    m_scrollPanel.Children.Add(m_grid);
                }
                if (m_scrollPanel.ParentWidget != this) {
                    Children.Clear();
                    Children.Add(m_scrollPanel);
                }
            }
            else if (m_grid.ParentWidget != this) {
                Children.Clear();
                Children.Add(m_grid);
            }
        }

        // 两阶段布局：测量（自然宽/min 宽）→ 分配（自然左对齐 / 削峰收缩 / 滚动兜底）→ 按列宽重排格
        void Relayout(float availX) {
            int columns = 0;
            foreach (RowData row in m_rows) {
                columns = MathUtils.Max(columns, row.Cells.Count);
            }
            m_scrollMode = false;
            if (columns == 0
                || m_rows.Count == 0) {
                m_tableWidth = 0f;
                m_tableHeight = 0f;
                m_columnWidths = [];
                return;
            }
            BitmapFont font = Font ?? (LabelWidget.BitmapFont ?? BitmapFont.DebugFont);
            // min 列宽下限：2 汉字宽（基准字号）
            float floor = 2f * font.MeasureText("字", new Vector2(FontScale), Vector2.Zero).X;
            float[] natural = new float[columns];
            float[] mins = new float[columns];
            RichTextLayout scratch = new() {
                Font = font,
                WordWrap = true,
                DefaultFontScale = FontScale
            };
            foreach (RowData row in m_rows) {
                for (int c = 0; c < row.Cells.Count; c++) {
                    scratch.Spans = row.Cells[c].Spans;
                    scratch.Layout(float.MaxValue, float.PositiveInfinity);
                    natural[c] = MathUtils.Max(natural[c], scratch.MeasuredSize.X + 2f * CellPaddingX);
                    mins[c] = MathUtils.Max(mins[c], MeasureCellMinWidth(scratch.Spans, font) + 2f * CellPaddingX);
                }
            }
            for (int c = 0; c < columns; c++) {
                mins[c] = MathUtils.Max(mins[c], floor + 2f * CellPaddingX);
            }
            float totalNatural = 0f;
            foreach (float width in natural) {
                totalNatural += width;
            }
            float totalMin = 0f;
            foreach (float width in mins) {
                totalMin += width;
            }
            if (totalNatural <= availX) {
                // 自然宽左对齐，不摊满
                m_columnWidths = natural;
            }
            else if (totalMin <= availX) {
                m_columnWidths = CollapseWidths(natural, mins, availX);
            }
            else {
                // 各列压到 min 仍超宽：自然宽 + 横向滚动
                m_columnWidths = natural;
                m_scrollMode = true;
            }
            m_tableWidth = 0f;
            foreach (float width in m_columnWidths) {
                m_tableWidth += width;
            }
            m_viewportWidth = MathUtils.Min(m_tableWidth, availX);
            AssignAnchors();
            LayoutCells();
        }

        // rich _collapse_widths 削峰瀑布：每轮把（并列）最宽列压到次宽列水平，逐层下压；
        // 各列保底 min（rich 无 floor，此处收紧）；全部等宽后一并压向各自 min
        float[] CollapseWidths(float[] widths, float[] mins, float availX) {
            float[] result = (float[])widths.Clone();
            float excess = 0f;
            foreach (float width in result) {
                excess += width;
            }
            excess -= availX;
            int guard = 0;
            while (excess > 0.01f
                && guard++ < 1000) {
                float maxWidth = 0f;
                for (int i = 0; i < result.Length; i++) {
                    if (result[i] > mins[i]) {
                        maxWidth = MathUtils.Max(maxWidth, result[i]);
                    }
                }
                if (maxWidth <= 0f) {
                    break;
                }
                // 并列最宽且可缩的列一组；次宽 = 严格低于 maxWidth 的最大列宽
                int groupCount = 0;
                float totalCut = 0f;
                float second = 0f;
                for (int i = 0; i < result.Length; i++) {
                    if (result[i] < maxWidth - 0.01f) {
                        second = MathUtils.Max(second, result[i]);
                    }
                }
                float[] cuts = new float[result.Length];
                for (int i = 0; i < result.Length; i++) {
                    if (result[i] >= maxWidth - 0.01f
                        && result[i] > mins[i]) {
                        float target = MathUtils.Max(second, mins[i]);
                        cuts[i] = MathUtils.Max(result[i] - target, 0f);
                        totalCut += cuts[i];
                        groupCount++;
                    }
                }
                if (groupCount == 0
                    || totalCut <= 0.001f) {
                    break;
                }
                float scale = MathUtils.Min(1f, excess / totalCut);
                for (int i = 0; i < result.Length; i++) {
                    if (cuts[i] > 0f) {
                        float delta = cuts[i] * scale;
                        result[i] -= delta;
                        excess -= delta;
                    }
                }
            }
            return result;
        }

        void AssignAnchors() {
            for (int c = 0; c < m_columnWidths.Length; c++) {
                MarkdownTableAlignment? alignment = c < m_alignments.Count ? m_alignments[c] : null;
                TextAnchor anchor = alignment switch {
                    MarkdownTableAlignment.Center => TextAnchor.HorizontalCenter,
                    MarkdownTableAlignment.Right => TextAnchor.Right,
                    _ => TextAnchor.Default
                };
                foreach (RowData row in m_rows) {
                    if (c < row.Cells.Count) {
                        row.Cells[c].Widget.TextAnchor = anchor;
                    }
                }
            }
        }

        void LayoutCells() {
            foreach (RowData row in m_rows) {
                float rowHeight = 0f;
                for (int c = 0; c < row.Cells.Count; c++) {
                    CellData cell = row.Cells[c];
                    float inner = MathUtils.Max(m_columnWidths[c] - 2f * CellPaddingX, 0f);
                    cell.Widget.Measure(new Vector2(inner, float.PositiveInfinity));
                    cell.Height = cell.Widget.ParentDesiredSize.Y;
                    rowHeight = MathUtils.Max(rowHeight, cell.Height);
                }
                row.Height = rowHeight + 2f * CellPaddingY;
            }
            m_tableHeight = 0f;
            foreach (RowData row in m_rows) {
                m_tableHeight += row.Height;
            }
        }

        // 格 min 宽：最长不可断 token（拉丁词整词不拆；CJK 字间可断，单字即完整 token）。
        // 略过 kerning（min 轻微高估，安全方向）
        float MeasureCellMinWidth(List<TextSpan> spans, BitmapFont font) {
            float maxWord = 0f;
            float word = 0f;
            foreach (TextSpan span in spans) {
                float scale = span.FontScale > 0f ? span.FontScale : FontScale;
                string text = span.Text ?? string.Empty;
                foreach (char c in text) {
                    if (c == '\n'
                        || c == '\r'
                        || char.IsWhiteSpace(c)) {
                        maxWord = MathUtils.Max(maxWord, word);
                        word = 0f;
                        continue;
                    }
                    if (FontTextWidget.IsCjkChar(c)) {
                        maxWord = MathUtils.Max(maxWord, word);
                        word = 0f;
                        maxWord = MathUtils.Max(maxWord, CharacterAdvance(c, scale, font));
                        continue;
                    }
                    word += CharacterAdvance(c, scale, font);
                }
            }
            maxWord = MathUtils.Max(maxWord, word);
            return maxWord;
        }

        float CharacterAdvance(char c, float scale, BitmapFont font) => (font.GetGlyph(c).Width + font.Spacing.X) * scale * font.Scale;
    }
}
