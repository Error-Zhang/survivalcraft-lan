using Engine;

namespace Game {
    // CSS width/max-width 块级尺寸件（单子件容器，builder ApplyBlockSizing 包裹产，恒最外——
    // 盒壳在内层）：测量期把宿主可用宽按声明钳成目标宽传给子件（子件
    // HorizontalAlignment=Stretch 由 builder 设置，Arrange 铺满钳后宽），自身 DesiredSize.X
    // 报钳后宽——子件自然宽更大时保溢出按内容宽报（表格 min-content 溢出现状，不截断）。
    // 宽度口径：FixedWidth>=0 优先（∞ 宽宿主下恒生效，绝对值语义）；否则 WidthPercent 声明
    // 时×宿主可用宽；都无=宿主可用宽（max-width 仍可钳小）。% 的解析基准=测量期宿主可用宽
    //（构建期不可知），宿主给 ∞ 宽时 % 无解析基准，跳过 % 与 % 上限走子件自然宽/固定值。
    // margin:auto 居中不由本件承担（builder 检出后设 HorizontalAlignment=Center，宿主排列期
    // 居中）；margin/padding/border/background 归壳层 WrapInStyleBox，本件不管。
    // 已知限制（留档）：
    // - 回填击穿：子树含 pre/img 自适应 frame 时，宿主测量回填（HtmlWidget 壳与格 panel 按
    //   根容器净宽强制铺满 frame 宽）不知本件存在，壳内自适应块 width 失效（待回填感知 sizer）
    // - FixedWidth 超宿主可用宽：引擎 Arrange 把本件钳到宿主格宽裁切（故意保持绝对值语义，
    //   不随宿主缩放）
    // - % 分母含自身 margin 近似：水平 margin 迁到本件后，宿主测量扣 margin 再传入，% 实际
    //   分母=宿主净宽−自身 margin（CSS 应为不含自身 margin 的包含块 content 宽）
    public class HtmlBlockSizerWidget : StackPanelWidget {
        // 固定像素宽（width:300px → 300；builder 已做 content-box 补偿=声明值+水平 pad/border），
        // <0=未声明
        public float FixedWidth = -1f;

        // 百分比宽（width:50% → 50），null=未声明；解析基准=测量期宿主可用宽
        public float? WidthPercent;

        // 固定像素宽上限（max-width:200px → 200；补偿口径同 FixedWidth），<0=未声明
        public float MaxFixedWidth = -1f;

        // 百分比宽上限（max-width:50% → 50），null=未声明；解析基准=测量期宿主可用宽
        public float? MaxWidthPercent;

        public HtmlBlockSizerWidget() {
            Direction = LayoutDirection.Vertical;
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            float availX = parentAvailableSize.X;
            float width = ResolveTargetWidth(availX);
            float fixedHeight = 0f;
            float contentWidth = 0f;
            int fillCount = 0;
            foreach (Widget child in Children) {
                if (!child.IsVisible) {
                    continue;
                }
                // 引擎口径：可用宽先扣子件四边距（垂直栈高度不限）
                child.Measure(Vector2.Max(new Vector2(width, float.PositiveInfinity) - child.MarginHorizontalSumAndVerticalSum, Vector2.Zero));
                if (child.ParentDesiredSize.Y != float.PositiveInfinity) {
                    fixedHeight += child.ParentDesiredSize.Y + child.MarginVerticalSum;
                }
                else {
                    fillCount++;
                }
                contentWidth = MathUtils.Max(contentWidth, child.ParentDesiredSize.X + child.MarginHorizontalSum);
            }
            // m_fixedSize/m_fillCount 供 base ArrangeOverride 分配 ∞ 高子件（与 StackPanel 同语义）
            m_fixedSize = fixedHeight;
            m_fillCount = fillCount;
            // 目标宽解析为 ∞（无固定值且宿主 ∞ 宽/% 无基准）时按子件自然宽报，不产 ∞ 期望宽
            DesiredSize = new Vector2(
                float.IsPositiveInfinity(width) ? contentWidth : MathUtils.Max(width, contentWidth),
                fillCount == 0 ? fixedHeight : float.PositiveInfinity
            );
        }

        // 目标宽：width 声明优先（固定 > 百分比），缺省=宿主可用宽；max-width 声明再取小。
        // 固定值在 ∞ 宽宿主下恒生效（绝对值语义）；仅 % 在 ∞ 时无解析基准跳过
        float ResolveTargetWidth(float availX) {
            float width = FixedWidth >= 0f
                ? FixedWidth
                : float.IsPositiveInfinity(availX)
                    ? float.PositiveInfinity
                    : WidthPercent.HasValue ? availX * WidthPercent.Value / 100f : availX;
            float maxWidth = float.PositiveInfinity;
            if (MaxFixedWidth >= 0f) {
                maxWidth = MaxFixedWidth;
            }
            else if (MaxWidthPercent.HasValue
                && !float.IsPositiveInfinity(availX)) {
                maxWidth = availX * MaxWidthPercent.Value / 100f;
            }
            return MathUtils.Min(width, maxWidth);
        }
    }
}
