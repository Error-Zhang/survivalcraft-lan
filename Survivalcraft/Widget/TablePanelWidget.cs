using System.Runtime.CompilerServices;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    /// <summary>
    /// 表格行容器：cell 的分组壳，承载 Header/BackgroundColor 行级属性。
    /// 布局权全在父 TablePanelWidget——Measure/Arrange 均空实现（ContainerWidget 默认实现会
    /// 自行量子件，必须 override 掉），cell 的测量与摆位由 TablePanelWidget 直接驱动。
    /// 详见 docs/superpowers/specs/2026-10-04-table-panel-widget-design.md。
    /// </summary>
    public class TableRowWidget : ContainerWidget {
        /// <summary>表头行：行 IsHeader = 行壳 Header || 行序号 &lt; TablePanelWidget.HeaderRows。</summary>
        public bool Header { get; set; }

        /// <summary>行底色（整行绘制在 cell 底色之下，Task 5 消费）。</summary>
        public Color? BackgroundColor { get; set; }

        // 布局权在 TablePanelWidget：不量子件、不摆子件（cell 由表容器直接 Measure/Arrange）
        public override void MeasureOverride(Vector2 parentAvailableSize) { }

        public override void ArrangeOverride() { }
    }

    /// <summary>
    /// CSS table 布局容器：子件成行（TableRowWidget 行壳或裸子件匿名行，spec §2.5），cell 支持
    /// ColSpan/RowSpan 与格底色（attached property，XML 写 TablePanelWidget.ColSpan="2"，本体
    /// LoadProperties 原生 Type.Prop 语法），按 ConditionalWeakTable 存储、WidgetRemoved 清表。
    /// 布局流水线：Measure 期 CollectRows 收集行结构（本文件）→ 列协商（Task 3）→ 高度轮与
    /// 摆位（Task 4）→ 底色/边框绘制（Task 5）。
    /// 详见 docs/superpowers/specs/2026-10-04-table-panel-widget-design.md。
    /// </summary>
    public class TablePanelWidget : ContainerWidget {
        // 格线厚（照 MarkdownTableWidget.LineThickness）
        const float LineThickness = 1f;

        sealed class CellAttached {
            public int ColSpan = 1;

            public int RowSpan = 1;

            public Color? Background;
        }

        static readonly ConditionalWeakTable<Widget, CellAttached> m_attached = new();

        /// <summary>cell 间距：X 水平、Y 垂直。</summary>
        public Vector2 Gap { get; set; }

        /// <summary>cell 对称内边距：X 左右各 X、Y 上下各 Y。</summary>
        public Vector2 CellPadding { get; set; }

        /// <summary>前 N 行视为表头（行 IsHeader = 行壳 Header || 行序号 &lt; HeaderRows）。</summary>
        public int HeaderRows { get; set; }

        /// <summary>整表底色（Task 5 消费）。</summary>
        public Color? BackgroundColor { get; set; }

        public bool OuterBorder { get; set; }

        public bool InnerBorder { get; set; }

        /// <summary>表头下分隔线（实际显示 = InnerBorder || HeaderSeparator）。</summary>
        public bool HeaderSeparator { get; set; }

        public Color BorderColor { get; set; } = new Color(160, 160, 160, 90);

        public static void SetColSpan(Widget cell, int value) => GetAttached(cell).ColSpan = MathUtils.Max(value, 1);

        public static int GetColSpan(Widget cell) => GetAttached(cell).ColSpan;

        public static void SetRowSpan(Widget cell, int value) => GetAttached(cell).RowSpan = MathUtils.Max(value, 1);

        public static int GetRowSpan(Widget cell) => GetAttached(cell).RowSpan;

        public static void SetCellBackground(Widget cell, Color? value) => GetAttached(cell).Background = value;

        public static Color? GetCellBackground(Widget cell) => GetAttached(cell).Background;

        static CellAttached GetAttached(Widget widget) {
            if (!m_attached.TryGetValue(widget, out CellAttached attached)) {
                attached = new CellAttached();
                m_attached.Add(widget, attached);
            }
            return attached;
        }

        public override void WidgetRemoved(Widget widget) {
            m_attached.Remove(widget);
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsDrawRequired = true;
            CollectRows();
            ExpandSlots();                       // 列数 + occupied 占位表 + 每行 cell 错位定列
            float availX = MathUtils.Max(parentAvailableSize.X, 0f);
            for (int c = 0; c < m_columnMax.Length; c++) {
                m_columnMax[c] = 0f;
                m_columnMin[c] = 0f;
                m_columnSaturated[c] = false;
            }
            // 第一遍：ColSpan==1 的 cell 直取 Max 定列并标饱和
            foreach (TableLayoutRow row in m_layoutRows) {
                foreach (TableCellSlot slot in row.Cells) {
                    if (slot.Cell == null
                        || slot.ColSpan != 1) {
                        continue;
                    }
                    (float max, float min) = MeasureCellExtents(slot.Cell);
                    m_columnMax[slot.Col] = MathUtils.Max(m_columnMax[slot.Col], max);
                    m_columnMin[slot.Col] = MathUtils.Max(m_columnMin[slot.Col], min);
                    m_columnSaturated[slot.Col] = true;
                }
            }
            // 第二遍：span cell 均摊到未饱和列（均未饱和则摊全部覆盖列）——spec §5.1
            foreach (TableLayoutRow row in m_layoutRows) {
                foreach (TableCellSlot slot in row.Cells) {
                    if (slot.Cell == null
                        || slot.ColSpan == 1) {
                        continue;
                    }
                    (float max, float min) = MeasureCellExtents(slot.Cell);
                    SpreadToColumns(slot, max, min);   // min/max 各自独立均摊
                }
            }
            // floor：每列 min = Max(min, 2 汉字宽)（照 MD 表口径）。TablePanelWidget 无字体属性，
            // 照 MD 表 Relayout 现状用 LabelWidget.BitmapFont 兜底；margin 已由量测侧并入
            // （MeasureCellExtents），CellPadding 由高度轮扣净宽/摆位内缩消费
            BitmapFont font = LabelWidget.BitmapFont ?? BitmapFont.DebugFont;
            float floorW = 2f * font.MeasureText("字", new Vector2(1f), Vector2.Zero).X;
            for (int c = 0; c < m_columnMin.Length; c++) {
                m_columnMin[c] = MathUtils.Max(m_columnMin[c], floorW);
                m_columnMax[c] = MathUtils.Max(m_columnMax[c], m_columnMin[c]);
            }
            NegotiateColumns(availX);
            MeasureRowHeights();                 // 高度轮：列宽定稿，cell 重测拿折行后高（spec §5.3）
            float sumW = 0f;
            foreach (float width in m_columnWidth) {
                sumW += width;
            }
            float sumH = 0f;
            foreach (float height in m_rowHeights) {
                sumH += height;
            }
            // X=Σ列宽+Gap.X×(列数−1)；Y=Σ行高+Gap.Y×(行数−1)
            DesiredSize = new Vector2(
                sumW + Gap.X * MathUtils.Max(m_columnWidth.Length - 1, 0),
                sumH + Gap.Y * MathUtils.Max(m_rowHeights.Length - 1, 0));
        }

        // 摆位（spec §5.4）：行壳 Arrange((0,rowY),(表宽,行高))，rowY=行高前缀和+Gap.Y 累计；cell
        // 父权摆孙件——合成矩形相对 cell 的父（行壳，裸 cell 父即本表），x=列前缀和+Gap.X、y=rowY−壳 y
        // （壳行归 0、裸行保持 rowY），rowspan 合成高=Σ覆盖行高+Gap.Y×(span−1)，内缩 CellPadding 后
        // ArrangeChildWidgetInCell（对齐语义引擎原生）；空槽（补位）跳过。列 x/宽用摊余后宽度——
        // Stretch 摊余在摆位前一次线性完成，不改 DesiredSize
        public override void ArrangeOverride() {
            ComputeArrangedWidths();   // Draw 消费（引擎帧序 Measure→Arrange→Draw 保证就绪）
            float tableWidth = TrackSpanWidth(m_arrangedWidths, 0, m_arrangedWidths.Length);
            float rowY = 0f;
            for (int r = 0; r < m_layoutRows.Count; r++) {
                TableLayoutRow row = m_layoutRows[r];
                if (row.RowWidget != null) {
                    row.RowWidget.Arrange(new Vector2(0f, rowY), new Vector2(tableWidth, m_rowHeights[r]));
                }
                float rectY = row.RowWidget != null ? 0f : rowY;   // cell 矩形 y 相对 cell 的父（壳行归 0）
                foreach (TableCellSlot slot in row.Cells) {
                    if (slot.Cell == null) {
                        continue;
                    }
                    float colX = 0f;
                    for (int c = 0; c < slot.Col; c++) {
                        colX += m_arrangedWidths[c] + Gap.X;
                    }
                    int spanRows = MathUtils.Max(MathUtils.Min(slot.RowSpan, m_layoutRows.Count - slot.Row), 1);
                    float spanH = Gap.Y * (spanRows - 1);
                    for (int rr = slot.Row; rr < slot.Row + spanRows; rr++) {
                        spanH += m_rowHeights[rr];
                    }
                    ArrangeChildWidgetInCell(
                        new Vector2(colX + CellPadding.X, rectY + CellPadding.Y),
                        new Vector2(colX + TrackSpanWidth(m_arrangedWidths, slot.Col, slot.ColSpan) - CellPadding.X, rectY + spanH - CellPadding.Y),
                        slot.Cell);
                }
                rowY += m_rowHeights[r] + Gap.Y;
            }
        }

        // 绘制（spec §3）：单 FlatBatch layer=-1 垫底，顺序 ①表底 ②行底 ③cell 底 ④格线。quad 四角
        // 各自 Vector2.Transform(局部点, GlobalTransform) 预变换入批（照 MarkdownTableWidget 的
        // TableGridWidget.Draw 先例——共享 batch 严禁 TransformTriangles 二次变换，顶点区间在多
        // draw-item 间不可靠）；颜色 ×GlobalColorTransform、A>0 才画。列几何用 Arrange 期摊余数组
        // m_arrangedWidths（Stretch 摊余只在 Arrange 期发生，引擎帧序 Measure→Arrange→Draw 保证
        // Draw 前已赋值）。cell 底=跨越 track 合成矩形（含 CellPadding 与跨内 Gap，cell 间 Gap 区
        // 归表底）；线位=track 边界 x/y 前缀和，底边/右边线回缩一个线厚防出界（照 MD 表）
        public override void Draw(DrawContext dc) {
            int columns = m_arrangedWidths.Length;
            int rows = m_layoutRows.Count;
            bool headerLine = InnerBorder || HeaderSeparator;
            bool anyRowBackground = false;
            bool anyCellBackground = false;
            foreach (TableLayoutRow row in m_layoutRows) {
                anyRowBackground |= row.Background.HasValue;
                foreach (TableCellSlot slot in row.Cells) {
                    if (slot.Cell != null
                        && GetCellBackground(slot.Cell).HasValue) {
                        anyCellBackground = true;
                        break;
                    }
                }
            }
            // 全开关关+无底色（或无结构）：零 batch 开销
            if (columns == 0
                || rows == 0
                || (!BackgroundColor.HasValue
                    && !anyRowBackground
                    && !anyCellBackground
                    && !OuterBorder
                    && !InnerBorder
                    && !headerLine)) {
                return;
            }
            FlatBatch2D batch = dc.PrimitivesRenderer2D.FlatBatch(-1, DepthStencilState.None);
            float tableWidth = TrackSpanWidth(m_arrangedWidths, 0, columns);
            float tableHeight = Gap.Y * MathUtils.Max(rows - 1, 0);
            foreach (float height in m_rowHeights) {
                tableHeight += height;
            }
            if (tableWidth <= 0f
                || tableHeight <= 0f) {
                return;
            }
            // 行 y 前缀和（rowspan cell 底与线位共用）——帧复用缓冲，每调用全量覆写
            if (m_rowTops.Length != rows) {
                m_rowTops = new float[rows];
            }
            float[] rowTops = m_rowTops;
            float top = 0f;
            for (int r = 0; r < rows; r++) {
                rowTops[r] = top;
                top += m_rowHeights[r] + Gap.Y;
            }
            // ① 表底
            if (BackgroundColor.HasValue) {
                Color color = BackgroundColor.Value * GlobalColorTransform;
                if (color.A > 0) {
                    QueueQuad(batch, 0f, 0f, tableWidth, tableHeight, color);
                }
            }
            // ② 行底（整行宽=表宽，画在 cell 底之下）
            for (int r = 0; r < rows; r++) {
                if (!m_layoutRows[r].Background.HasValue) {
                    continue;
                }
                Color color = m_layoutRows[r].Background.Value * GlobalColorTransform;
                if (color.A > 0) {
                    QueueQuad(batch, 0f, rowTops[r], tableWidth, m_rowHeights[r], color);
                }
            }
            // ③ cell 底：跨越 track 合成矩形（含 CellPadding；跨内 Gap 属合成矩形，cell 间 Gap 归表底）
            for (int r = 0; r < rows; r++) {
                foreach (TableCellSlot slot in m_layoutRows[r].Cells) {
                    if (slot.Cell == null) {
                        continue;
                    }
                    Color? background = GetCellBackground(slot.Cell);
                    if (!background.HasValue) {
                        continue;
                    }
                    Color color = background.Value * GlobalColorTransform;
                    if (color.A <= 0) {
                        continue;
                    }
                    float colX = 0f;
                    for (int c = 0; c < slot.Col; c++) {
                        colX += m_arrangedWidths[c] + Gap.X;
                    }
                    int spanRows = MathUtils.Max(MathUtils.Min(slot.RowSpan, rows - slot.Row), 1);
                    float spanH = Gap.Y * (spanRows - 1);
                    for (int rr = slot.Row; rr < slot.Row + spanRows; rr++) {
                        spanH += m_rowHeights[rr];
                    }
                    QueueQuad(batch, colX, rowTops[slot.Row], TrackSpanWidth(m_arrangedWidths, slot.Col, slot.ColSpan), spanH, color);
                }
            }
            // ④ 格线
            Color lineColor = BorderColor * GlobalColorTransform;
            if (lineColor.A <= 0) {
                return;
            }
            // 外框四条（底边/右边回缩一个线厚防出界）
            if (OuterBorder) {
                QueueQuad(batch, 0f, 0f, tableWidth, LineThickness, lineColor);
                QueueQuad(batch, 0f, tableHeight - LineThickness, tableWidth, LineThickness, lineColor);
                QueueQuad(batch, 0f, 0f, LineThickness, tableHeight, lineColor);
                QueueQuad(batch, tableWidth - LineThickness, 0f, LineThickness, tableHeight, lineColor);
            }
            // 内部行列分隔线（位=track 起始边界前缀和，不含外框两条）：被 span cell 跨越的段
            // 不画（colspan cell 中间无竖线、rowspan cell 中间无横线，CSS table 语义），连续
            // 无跨越段合并成一条 quad、跨内 Gap 随段连续（与 cell 底色合成矩形同口径）
            if (InnerBorder) {
                for (int r = 1; r < rows; r++) {
                    QueueHorizontalLineSegments(batch, MathUtils.Min(rowTops[r], tableHeight - LineThickness), r, lineColor);
                }
                for (int c = 1; c < columns; c++) {
                    float x = 0f;
                    for (int cc = 0; cc < c; cc++) {
                        x += m_arrangedWidths[cc] + Gap.X;
                    }
                    QueueVerticalLineSegments(batch, MathUtils.Min(x, tableWidth - LineThickness), c, rowTops, tableHeight, lineColor);
                }
            }
            // 表头下横线：最末 IsHeader 行下缘（多个表头行只画最末行下）；该分界已被 InnerBorder
            // 画出（表头非最末行）则不重复叠加；无表头行则 HeaderSeparator 无效果
            if (headerLine) {
                int lastHeader = -1;
                for (int r = 0; r < rows; r++) {
                    if (m_layoutRows[r].IsHeader) {
                        lastHeader = r;
                    }
                }
                if (lastHeader >= 0
                    && !(InnerBorder && lastHeader < rows - 1)) {
                    float y = MathUtils.Min(rowTops[lastHeader] + m_rowHeights[lastHeader], tableHeight - LineThickness);
                    QueueHorizontalLineSegments(batch, y, lastHeader + 1, lineColor);
                }
            }
        }

        // 局部坐标 quad 四角各自经 GlobalTransform 变换后入批（照 TableGridWidget.QueueQuad，斜切/旋转变换下不塌陷）
        void QueueQuad(FlatBatch2D batch, float x, float y, float w, float h, Color color) {
            Matrix m = GlobalTransform;
            Vector2 p1 = Vector2.Transform(new Vector2(x, y), m);
            Vector2 p2 = Vector2.Transform(new Vector2(x + w, y), m);
            Vector2 p3 = Vector2.Transform(new Vector2(x + w, y + h), m);
            Vector2 p4 = Vector2.Transform(new Vector2(x, y + h), m);
            batch.QueueQuad(p1, p2, p3, p4, 0f, color);
        }

        // 行分界 rowBoundary 上的横线：被 rowspan cell 跨越（slot.Row < rowBoundary <
        // slot.Row+spanRows）的列段跳过，其余连续列段合并成一条 quad（段宽含段内 Gap.X）。
        // 表头下横线同口径复用（y 由调用者定：行分界取 rowTops[r]、表头线取表头行下缘）
        void QueueHorizontalLineSegments(FlatBatch2D batch, float y, int rowBoundary, Color lineColor) {
            int columns = m_arrangedWidths.Length;
            if (m_hCrossed.Length != columns) {
                m_hCrossed = new bool[columns];
            }
            Array.Clear(m_hCrossed, 0, columns);   // 每调用清零重填（条件置位，复用不留陈旧跨越态）
            bool[] crossed = m_hCrossed;
            foreach (TableLayoutRow row in m_layoutRows) {
                foreach (TableCellSlot slot in row.Cells) {
                    if (slot.Cell == null) {
                        continue;
                    }
                    int spanRows = MathUtils.Max(MathUtils.Min(slot.RowSpan, m_layoutRows.Count - slot.Row), 1);
                    if (slot.Row < rowBoundary && slot.Row + spanRows > rowBoundary) {
                        for (int c = slot.Col; c < slot.Col + slot.ColSpan; c++) {
                            crossed[c] = true;
                        }
                    }
                }
            }
            int c0 = 0;
            while (c0 < columns) {
                if (crossed[c0]) {
                    c0++;
                    continue;
                }
                int c1 = c0 + 1;
                while (c1 < columns && !crossed[c1]) {
                    c1++;
                }
                float x = 0f;
                for (int c = 0; c < c0; c++) {
                    x += m_arrangedWidths[c] + Gap.X;
                }
                float w = Gap.X * (c1 - c0 - 1);
                for (int c = c0; c < c1; c++) {
                    w += m_arrangedWidths[c];
                }
                QueueQuad(batch, x, y, w, LineThickness, lineColor);
                c0 = c1;
            }
        }

        // 列分界 colBoundary 上的竖线：被 colspan cell 跨越（slot.Col < colBoundary <
        // slot.Col+ColSpan）的行段跳过，其余连续行段合并成一条 quad（段高含段内 Gap.Y，
        // 行 y 位用 Draw 期 rowTops 前缀和，已含 Gap）
        void QueueVerticalLineSegments(FlatBatch2D batch, float x, int colBoundary, float[] rowTops, float tableHeight, Color lineColor) {
            int rows = m_layoutRows.Count;
            if (m_vCrossed.Length != rows) {
                m_vCrossed = new bool[rows];
            }
            Array.Clear(m_vCrossed, 0, rows);   // 每调用清零重填（条件置位，复用不留陈旧跨越态）
            bool[] crossed = m_vCrossed;
            foreach (TableLayoutRow row in m_layoutRows) {
                foreach (TableCellSlot slot in row.Cells) {
                    if (slot.Cell == null
                        || slot.Col >= colBoundary
                        || slot.Col + slot.ColSpan <= colBoundary) {
                        continue;
                    }
                    int spanRows = MathUtils.Max(MathUtils.Min(slot.RowSpan, rows - slot.Row), 1);
                    for (int r = slot.Row; r < slot.Row + spanRows; r++) {
                        crossed[r] = true;
                    }
                }
            }
            int r0 = 0;
            while (r0 < rows) {
                if (crossed[r0]) {
                    r0++;
                    continue;
                }
                int r1 = r0 + 1;
                while (r1 < rows && !crossed[r1]) {
                    r1++;
                }
                float h = Gap.Y * (r1 - r0 - 1);
                for (int r = r0; r < r1; r++) {
                    h += m_rowHeights[r];
                }
                QueueQuad(batch, x, MathUtils.Min(rowTops[r0], tableHeight - LineThickness), LineThickness, h, lineColor);
                r0 = r1;
            }
        }

        readonly List<TableLayoutRow> m_layoutRows = [];

        // 帧路径复用缓冲（每帧 Measure/Draw 反复执行，避免逐帧分配，照 FlexPanelWidget
        // m_rowPool 先例——pop→清字段→复用，空则 new）
        readonly List<TableLayoutRow> m_layoutRowPool = [];

        readonly List<TableCellSlot> m_slotPool = [];

        readonly List<int> m_spreadTargets = [];   // SpreadToColumns 均摊目标列，每次调用 Clear 重填

        float[] m_columnMin = [];

        float[] m_columnMax = [];

        float[] m_columnWidth = [];

        bool[] m_columnSaturated = [];   // span 均摊用（列已有 ColSpan==1 cell 直测定格），帧内重置

        float[] m_rowHeights = [];       // 高度轮产物：每行总高（含 2×CellPadding.Y），帧内重算（累积自 0，复用先清零）

        float[] m_arrangedWidths = [];   // Arrange 期摊余列宽（Stretch 摊余产物，原地覆写），Draw 绘制口径用

        float[] m_rowTops = [];          // Draw 期行 y 前缀和（rowspan cell 底与线位共用），每调用全量覆写

        bool[] m_hCrossed = [];          // 横线分段 scratch：rowspan 跨越的列（长度=列数），每调用清零重填

        bool[] m_vCrossed = [];          // 竖线分段 scratch：colspan 跨越的行（长度=行数），每调用清零重填

        // 收集可见子件成行结构：行壳行的可见 cell 进槽、裸子件=单 cell 单行。IsHeader =
        // 行壳 Header || 行序号 < HeaderRows。每帧 Measure 重算；行/槽对象归还池复用（单线程
        // UI，帧序 Measure→Arrange→Draw 内 m_layoutRows 仍有效），Acquire* 全字段重置、
        // 复用槽逐字段覆写，不留陈旧态
        void CollectRows() {
            foreach (TableLayoutRow row in m_layoutRows) {
                m_slotPool.AddRange(row.Cells);
                m_layoutRowPool.Add(row);
            }
            m_layoutRows.Clear();
            int rowIndex = 0;
            foreach (Widget child in Children) {
                if (!child.IsVisible) { continue; }
                if (child is TableRowWidget rowWidget) {
                    TableLayoutRow row = AcquireRow();
                    row.RowWidget = rowWidget;
                    row.IsHeader = rowWidget.Header || rowIndex < HeaderRows;
                    row.Background = rowWidget.BackgroundColor;
                    foreach (Widget cell in rowWidget.Children) {
                        if (cell.IsVisible) {
                            TableCellSlot slot = AcquireSlot();
                            slot.Cell = cell;
                            slot.ColSpan = MathUtils.Max(GetColSpan(cell), 1);
                            slot.RowSpan = MathUtils.Max(GetRowSpan(cell), 1);
                            row.Cells.Add(slot);
                        }
                    }
                    m_layoutRows.Add(row);
                    rowIndex++;
                }
                else {
                    // 裸子件=单 cell 单行（CSS 匿名行简化，spec §2.5）
                    TableLayoutRow row = AcquireRow();
                    row.RowWidget = null;
                    row.IsHeader = rowIndex < HeaderRows;
                    row.Background = null;
                    TableCellSlot slot = AcquireSlot();
                    slot.Cell = child;
                    slot.ColSpan = MathUtils.Max(GetColSpan(child), 1);
                    slot.RowSpan = MathUtils.Max(GetRowSpan(child), 1);
                    row.Cells.Add(slot);
                    m_layoutRows.Add(row);
                    rowIndex++;
                }
            }
        }

        TableLayoutRow AcquireRow() {
            int last = m_layoutRowPool.Count - 1;
            if (last >= 0) {
                TableLayoutRow pooled = m_layoutRowPool[last];
                m_layoutRowPool.RemoveAt(last);
                pooled.Cells.Clear();   // 槽已归 m_slotPool，此处仅弃陈旧引用
                return pooled;
            }
            return new TableLayoutRow();
        }

        // 槽全字段重置：Cell 由 CollectRows 覆写、Row/Col 由 ExpandSlots 每帧覆写，Height 仅
        // RowSpan==1 槽在高度轮覆写——池化必须在此归零防 rowspan 槽读到陈旧高
        TableCellSlot AcquireSlot() {
            int last = m_slotPool.Count - 1;
            if (last >= 0) {
                TableCellSlot pooled = m_slotPool[last];
                m_slotPool.RemoveAt(last);
                pooled.Cell = null;
                pooled.Row = 0;
                pooled.Col = 0;
                pooled.ColSpan = 1;
                pooled.RowSpan = 1;
                pooled.Height = 0f;
                return pooled;
            }
            return new TableCellSlot();
        }

        // 槽位展开：列数 = 各行占格（含 span 展开后）最大值。每行 cell 用 cursor 推进定 Col，cursor
        // 遇被前 rowspan 占住的格跳过（occupied 登记 rowspan 覆盖格）；越界 span 防御钳到
        // columns − Col（至少 1）。产出协商/摆位数组尺寸与每 slot 的 Row/Col/ColSpan（帧内重算）
        void ExpandSlots() {
            int columns = 0;
            HashSet<(int Row, int Col)> occupied = [];
            for (int r = 0; r < m_layoutRows.Count; r++) {
                TableLayoutRow row = m_layoutRows[r];
                int cursor = 0;
                foreach (TableCellSlot slot in row.Cells) {
                    slot.Row = r;
                    while (occupied.Contains((r, cursor))) {
                        cursor++;
                    }
                    slot.Col = cursor;
                    cursor += slot.ColSpan;
                    for (int rr = r + 1; rr < MathUtils.Min(r + slot.RowSpan, m_layoutRows.Count); rr++) {
                        for (int cc = slot.Col; cc < cursor; cc++) {
                            occupied.Add((rr, cc));
                        }
                    }
                    columns = MathUtils.Max(columns, cursor);
                }
            }
            foreach (TableLayoutRow row in m_layoutRows) {
                foreach (TableCellSlot slot in row.Cells) {
                    slot.ColSpan = MathUtils.Max(MathUtils.Min(slot.ColSpan, columns - slot.Col), 1);
                }
            }
            if (m_columnMax.Length != columns) {
                m_columnMax = new float[columns];
                m_columnMin = new float[columns];
                m_columnSaturated = new bool[columns];
            }
        }

        // max+min 一次取（Task 1 探针结论的修正协议：严禁对 cell 直调 Measure((0,∞))——LabelWidget
        // 在可用宽 0 下 FontTextWidget.UpdateLines 零推进死循环 OOM 崩游戏；RichTextWidget (0,∞)
        // X=单字宽≠最长 token 语义也不成立）。margin 在唯一出口各并入一次（fix round 2：core 值
        // 全程不含 margin，返回处 maxCore+marginX / minCore+marginX——原实现在非文字分路 min=max
        // 处用了已含 margin 的 max，返回处再加 → min 双计 margin）
        (float max, float min) MeasureCellExtents(Widget cell) {
            float marginX = cell.MarginHorizontalSum;
            cell.Measure(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
            float maxCore = cell.ParentDesiredSize.X;
            if (!IsFinite(maxCore)) {
                maxCore = 0f;          // ∞（未显式 Size 的 ClickableWidget 系）→ 无自然宽，floor 兜底
            }
            maxCore = MathUtils.Max(maxCore, 0f);
            float minCore;
            if (cell is RichTextWidget rich) {
                // token 扫描（照 MD 表 MeasureCellMinWidth）+2×CellPadding.X：min 是 track 口径
                //（高度轮 innerW 扣 2×pad+margin 的逆操作），缺 pad 时列宽落 [token, token+2×pad)
                // 窗口净宽<词宽 → BreakWord 紧急强拆长词（F2）
                minCore = MeasureSpansMinWidth(rich.Spans, rich.Font, rich.FontScale) + 2f * CellPadding.X;
            }
            else if (cell is LabelWidget label) {
                // label 单 span 扫描（v1 忽略 label.FontScale，量级影响可接受），pad 口径同上
                minCore = MeasureSpansMinWidth([ new TextSpan(label.Text) ], label.Font, 1f) + 2f * CellPadding.X;
            }
            else {
                // 非文字类不收缩（按钮/图片 CSS 语义本就 min=max）。注意按钮系（ClickableWidget 无
                // MeasureOverride）未显式 Size 时 desired=∞，此处按 0+floor 兜底、摆位被 cell 钳满——
                // 按钮作 cell 请显式 Size（引擎惯例本就如此），不走自然尺寸协商。
                // 不补 2×CellPadding.X：显式 Size 本就是外框口径（含 pad），补了反而双计
                minCore = maxCore;
            }
            return (maxCore + marginX, MathUtils.Max(minCore, 0f) + marginX);
        }

        // cell min 宽：最长不可断 token（拉丁词整词不拆；CJK 字间可断，单字即完整 token）。
        // 照 MarkdownTableWidget.MeasureCellMinWidth 提取的共用实现（略过 kerning，min 轻微高估，安全方向）。
        // v1 口径：表格协商按默认 WordBreak=BreakWord 假设（CJK 可断）；cell 显式 WordBreak=KeepAll
        // 时渲染按词级断行、此处 min 低估——收缩态该格可能溢出，属进阶用法自担
        static float MeasureSpansMinWidth(List<TextSpan> spans, BitmapFont font, float defaultFontScale) {
            float maxWord = 0f;
            float word = 0f;
            foreach (TextSpan span in spans) {
                float scale = span.FontScale > 0f ? span.FontScale : defaultFontScale;
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

        static float CharacterAdvance(char c, float scale, BitmapFont font) => (font.GetGlyph(c).Width + font.Spacing.X) * scale * font.Scale;

        // span cell 均摊：per = value / 参与列数，逐列 Max 并入；参与列 = 覆盖列中的未饱和列
        // （均未饱和则摊全部覆盖列）——spec §5.1
        void SpreadToColumns(TableCellSlot slot, float max, float min) {
            int span = MathUtils.Max(MathUtils.Min(slot.ColSpan, m_columnMax.Length - slot.Col), 1);
            List<int> targets = m_spreadTargets;   // 帧复用缓冲，每次调用 Clear 重填
            targets.Clear();
            for (int c = slot.Col; c < slot.Col + span; c++) {
                if (!m_columnSaturated[c]) {
                    targets.Add(c);
                }
            }
            if (targets.Count == 0) {
                for (int c = slot.Col; c < slot.Col + span; c++) {
                    targets.Add(c);
                }
            }
            float perMax = max / targets.Count;
            float perMin = min / targets.Count;
            foreach (int c in targets) {
                m_columnMax[c] = MathUtils.Max(m_columnMax[c], perMax);
                m_columnMin[c] = MathUtils.Max(m_columnMin[c], perMin);
            }
        }

        // CSS auto 列协商（spec §5.2；线性比例削峰——MD 表瀑布削峰的通用化，case7 收缩态等价性
        // 由 Task 7 裁定）：Σmax ≤ 可用宽 shrink-to-fit 原样保持；Σmin > 可用宽各列定格 min 交
        // 外层；其余 width = min + (max − min) × (avail − Σmin) / (Σmax − Σmin)。
        // gap 先扣（DesiredSize 会把 Gap.X×(列数−1) 加回）：三态总宽 desired.X 才不超宿主可用宽
        void NegotiateColumns(float availX) {
            int n = m_columnMax.Length;
            availX = MathUtils.Max(availX - Gap.X * MathUtils.Max(n - 1, 0), 0f);
            if (m_columnWidth.Length != n) {
                m_columnWidth = new float[n];   // 帧复用：同长度直接覆写（三路分支均全量写入）
            }
            float sumMax = 0f;
            float sumMin = 0f;
            for (int c = 0; c < n; c++) {
                m_columnMax[c] = MathUtils.Max(m_columnMax[c], m_columnMin[c]);
                sumMax += m_columnMax[c];
                sumMin += m_columnMin[c];
            }
            if (sumMax <= availX
                || n == 0) {
                Array.Copy(m_columnMax, m_columnWidth, n);   // shrink-to-fit
                return;
            }
            if (sumMin > availX) {
                Array.Copy(m_columnMin, m_columnWidth, n);   // 定格 Σmin 交外层
                return;
            }
            float span = sumMax - sumMin;
            float room = availX - sumMin;
            for (int c = 0; c < n; c++) {
                m_columnWidth[c] = m_columnMin[c] + (m_columnMax[c] - m_columnMin[c]) * room / span;
            }
        }

        // 高度轮（spec §5.3）：列宽定稿后每 cell 以净宽重测拿折行后真实高度。净宽 = track 跨宽
        // （测量轮 m_columnWidth 口径，gap 属 track 间距不进 cell）− 2×CellPadding.X − cell 水平
        // margin（margin 已并入列需求，此处扣出的是内容测量净宽）。第一遍单格 cell 定行高：行高 =
        // max(cell 内容高) + 2×CellPadding.Y；第二遍 rowspan cell 需求 = 内容高 + 2×CellPadding.Y，
        // Σ覆盖行高（含行间 Gap.Y）不足 → 差值按覆盖行当前高比例摊（全 0 行均摊）——一轮近似，
        // 多 rowspan cell 按序叠加不迭代。cell 垂直 margin 不计入行高（v1 简化，与摆位内缩一致）。
        // 每帧 Measure 全量重算
        void MeasureRowHeights() {
            if (m_rowHeights.Length != m_layoutRows.Count) {
                m_rowHeights = new float[m_layoutRows.Count];
            }
            Array.Clear(m_rowHeights, 0, m_rowHeights.Length);   // 行高累积自 0，复用先清零
            foreach (TableLayoutRow row in m_layoutRows) {
                foreach (TableCellSlot slot in row.Cells) {
                    if (slot.Cell == null
                        || slot.RowSpan != 1) {
                        continue;
                    }
                    float innerW = TrackSpanWidth(m_columnWidth, slot.Col, 1) - 2f * CellPadding.X - slot.Cell.MarginHorizontalSum;
                    slot.Cell.Measure(new Vector2(MathUtils.Max(innerW, 0f), float.PositiveInfinity));
                    slot.Height = IsFinite(slot.Cell.ParentDesiredSize.Y) ? MathUtils.Max(slot.Cell.ParentDesiredSize.Y, 0f) : 0f;
                    m_rowHeights[slot.Row] = MathUtils.Max(m_rowHeights[slot.Row], slot.Height);
                }
            }
            for (int r = 0; r < m_rowHeights.Length; r++) {
                m_rowHeights[r] += 2f * CellPadding.Y;
            }
            foreach (TableLayoutRow row in m_layoutRows) {
                foreach (TableCellSlot slot in row.Cells) {
                    if (slot.Cell == null
                        || slot.RowSpan == 1) {
                        continue;
                    }
                    float innerW = TrackSpanWidth(m_columnWidth, slot.Col, slot.ColSpan) - 2f * CellPadding.X - slot.Cell.MarginHorizontalSum;
                    slot.Cell.Measure(new Vector2(MathUtils.Max(innerW, 0f), float.PositiveInfinity));
                    float contentY = IsFinite(slot.Cell.ParentDesiredSize.Y) ? MathUtils.Max(slot.Cell.ParentDesiredSize.Y, 0f) : 0f;
                    int spanRows = MathUtils.Max(MathUtils.Min(slot.RowSpan, m_rowHeights.Length - slot.Row), 1);
                    float covered = 0f;
                    for (int r = slot.Row; r < slot.Row + spanRows; r++) {
                        covered += m_rowHeights[r];
                    }
                    float deficit = contentY + 2f * CellPadding.Y - covered - Gap.Y * (spanRows - 1);
                    if (deficit <= 0f) {
                        continue;
                    }
                    if (covered > 0f) {
                        for (int r = slot.Row; r < slot.Row + spanRows; r++) {
                            m_rowHeights[r] += deficit * m_rowHeights[r] / covered;
                        }
                        continue;
                    }
                    float per = deficit / spanRows;   // 全 0 行均摊
                    for (int r = slot.Row; r < slot.Row + spanRows; r++) {
                        m_rowHeights[r] += per;
                    }
                }
            }
        }

        // track 跨宽：Σ宽[col..col+span) + Gap.X×(span−1)（gap 属 track 间距，不进 cell 测量宽）——
        // 测量轮传 m_columnWidth、摆位轮传摊余数组
        float TrackSpanWidth(float[] widths, int col, int span) {
            float sum = Gap.X * MathUtils.Max(span - 1, 0);
            for (int c = col; c < col + span; c++) {
                sum += widths[c];
            }
            return sum;
        }

        // Stretch 摊余（spec §5.4）：diff = ActualSize.X − (Σ列宽 + Gap.X×(列数−1))。正差（宿主
        // 实给宽于 shrink-to-fit，部件 HorizontalAlignment=Stretch）按列 max 比例摊满（列可超 max，
        // Stretch 语义）；负差（外部强压 ActualSize<Desired）按列 max 比例缩、每列下限 m_columnMin
        // （clamp 后 Σ 仍超 ActualSize 就随它去——宿主强压至 Σmin 以下不崩即可）。一次线性原地写
        // m_arrangedWidths（帧复用缓冲：长度不变直接覆写；Arrange 与 Draw 共用同一数组，引擎帧序
        // 保证 Draw 读到本帧产物），不改 m_columnWidth（协商产物保持测量语义）；全 0 列无比例基准退化均摊
        void ComputeArrangedWidths() {
            int n = m_columnWidth.Length;
            if (m_arrangedWidths.Length != n) {
                m_arrangedWidths = new float[n];
            }
            float[] arranged = m_arrangedWidths;
            if (n == 0) {
                return;
            }
            Array.Copy(m_columnWidth, arranged, n);
            float sumW = 0f;
            foreach (float width in arranged) {
                sumW += width;
            }
            float diff = ActualSize.X - (sumW + Gap.X * (n - 1));
            if (diff == 0f) {
                return;
            }
            float sumMax = 0f;
            foreach (float max in m_columnMax) {
                sumMax += max;
            }
            if (diff > 0f) {
                float per = diff / n;
                for (int c = 0; c < n; c++) {
                    arranged[c] += sumMax > 0f ? diff * m_columnMax[c] / sumMax : per;
                }
                return;
            }
            float shrinkPer = diff / n;
            for (int c = 0; c < n; c++) {
                float shrink = sumMax > 0f ? diff * m_columnMax[c] / sumMax : shrinkPer;
                arranged[c] = MathUtils.Max(arranged[c] + shrink, m_columnMin[c]);
            }
        }

        // 照 FlexPanelWidget.IsFinite：Infinity/NaN 视为无界贡献，防污染协商
        static bool IsFinite(float value) => !float.IsInfinity(value) && !float.IsNaN(value);
    }

    /// <summary>行内 cell 槽位（Task 3 协商 / Task 4 摆位 / Task 5 绘制消费）。</summary>
    internal class TableCellSlot {
        public Widget Cell;        // null = 补空槽

        public int Row;            // 所属行（ExpandSlots 产物，Task 4 高度轮按行分组用）

        public int Col;

        public int ColSpan;

        public int RowSpan;

        public float Height;       // 高度轮产物（Task 4）
    }

    /// <summary>布局行结构（CollectRows 产物）。</summary>
    internal class TableLayoutRow {
        public Widget RowWidget;   // TableRowWidget 或 null（裸 cell 单行）

        public bool IsHeader;      // 行壳 Header || 行序号 < HeaderRows

        public Color? Background;  // 行壳 BackgroundColor

        public List<TableCellSlot> Cells = [];
    }
}