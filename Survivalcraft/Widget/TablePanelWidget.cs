using System.Runtime.CompilerServices;
using Engine;

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
            // Task 2 占位：仅收集行结构，期望恒零（列协商 Task 3 填入）
            CollectRows();
            DesiredSize = Vector2.Zero;
        }

        readonly List<TableLayoutRow> m_layoutRows = [];

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
            // 列数 = 各行占格（含 span 展开后）最大值；槽位展开用 cursor+occupied（Task 3 实装）
        }
    }

    /// <summary>行内 cell 槽位（Task 3 协商 / Task 4 摆位 / Task 5 绘制消费）。</summary>
    internal class TableCellSlot {
        public Widget Cell;        // null = 补空槽

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