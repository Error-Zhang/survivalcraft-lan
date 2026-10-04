using System.Runtime.CompilerServices;
using Engine;
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
            // （MeasureCellExtents），CellPadding 由摆位轮（Task 4）消费
            BitmapFont font = LabelWidget.BitmapFont ?? BitmapFont.DebugFont;
            float floorW = 2f * font.MeasureText("字", new Vector2(1f), Vector2.Zero).X;
            for (int c = 0; c < m_columnMin.Length; c++) {
                m_columnMin[c] = MathUtils.Max(m_columnMin[c], floorW);
                m_columnMax[c] = MathUtils.Max(m_columnMax[c], m_columnMin[c]);
            }
            NegotiateColumns(availX);
            float sumW = 0f;
            foreach (float width in m_columnWidth) {
                sumW += width;
            }
            // X=Σ列宽+Gap.X×(列数−1)；Y=高度轮产物（Task 4 落，暂 0）
            DesiredSize = new Vector2(sumW + Gap.X * MathUtils.Max(m_columnWidth.Length - 1, 0), 0f);
        }

        readonly List<TableLayoutRow> m_layoutRows = [];

        float[] m_columnMin = [];

        float[] m_columnMax = [];

        float[] m_columnWidth = [];

        bool[] m_columnSaturated = [];   // span 均摊用（列已有 ColSpan==1 cell 直测定格），帧内重置

        // 收集可见子件成行结构：行壳行的可见 cell 进槽、裸子件=单 cell 单行。IsHeader =
        // 行壳 Header || 行序号 < HeaderRows。每帧 Measure 重算（行数少，直清直建）。
        void CollectRows() {
            m_layoutRows.Clear();
            int rowIndex = 0;
            foreach (Widget child in Children) {
                if (!child.IsVisible) { continue; }
                if (child is TableRowWidget rowWidget) {
                    TableLayoutRow row = new() {
                        RowWidget = rowWidget,
                        IsHeader = rowWidget.Header || rowIndex < HeaderRows,
                        Background = rowWidget.BackgroundColor
                    };
                    foreach (Widget cell in rowWidget.Children) {
                        if (cell.IsVisible) {
                            row.Cells.Add(new TableCellSlot {
                                Cell = cell,
                                ColSpan = MathUtils.Max(GetColSpan(cell), 1),
                                RowSpan = MathUtils.Max(GetRowSpan(cell), 1)
                            });
                        }
                    }
                    m_layoutRows.Add(row);
                    rowIndex++;
                }
                else {
                    // 裸子件=单 cell 单行（CSS 匿名行简化，spec §2.5）
                    m_layoutRows.Add(new TableLayoutRow {
                        RowWidget = null,
                        IsHeader = rowIndex < HeaderRows,
                        Cells = [ new TableCellSlot { Cell = child, ColSpan = MathUtils.Max(GetColSpan(child), 1), RowSpan = MathUtils.Max(GetRowSpan(child), 1) } ]
                    });
                    rowIndex++;
                }
            }
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
        // X=单字宽≠最长 token 语义也不成立）。margin 并入返回值（max/min 都 +marginX）
        (float max, float min) MeasureCellExtents(Widget cell) {
            float marginX = cell.MarginHorizontalSum;
            cell.Measure(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
            float max = cell.ParentDesiredSize.X;
            if (!IsFinite(max)) {
                max = 0f;              // ∞（未显式 Size 的 ClickableWidget 系）→ 无自然宽，floor 兜底
            }
            max = MathUtils.Max(max, 0f) + marginX;
            float min;
            if (cell is RichTextWidget rich) {
                min = MeasureSpansMinWidth(rich.Spans, rich.Font, rich.FontScale);   // token 扫描（照 MD 表 MeasureCellMinWidth）
            }
            else if (cell is LabelWidget label) {
                // label 单 span 扫描（v1 忽略 label.FontScale，量级影响可接受）
                min = MeasureSpansMinWidth([ new TextSpan(label.Text) ], label.Font, 1f);
            }
            else {
                min = max;             // 非文字类不收缩（按钮/图片 CSS 语义本就 min=max）
            }
            return (max, MathUtils.Max(min, 0f) + marginX);
        }

        // cell min 宽：最长不可断 token（拉丁词整词不拆；CJK 字间可断，单字即完整 token）。
        // 照 MarkdownTableWidget.MeasureCellMinWidth 提取的共用实现（略过 kerning，min 轻微高估，安全方向）
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
            List<int> targets = [];
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
        // 外层；其余 width = min + (max − min) × (avail − Σmin) / (Σmax − Σmin)
        void NegotiateColumns(float availX) {
            int n = m_columnMax.Length;
            m_columnWidth = new float[n];
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