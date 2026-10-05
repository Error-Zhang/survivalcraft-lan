using Engine;
using Engine.Graphics;

namespace Game {
    // style 盒属性壳：StackPanel 布局全继承，Draw 补画整幅背景（BackgroundColor）与四边边框
    // （外缘贴 ActualSize、向内 BorderWidths）。子件四边距已由 builder 并入 border+padding
    // （CSS border-box），边框区不压内容。照 TablePanelWidget 先例：FlatBatch2D layer=-1、
    // 局部坐标四角过 GlobalTransform、颜色×GlobalColorTransform、A<=0 跳过；横条全宽+竖条全高
    // 在四角重叠（同表格外框口径）
    public class HtmlStyleBoxWidget : StackPanelWidget {
        public Vector4 BorderWidths;

        // null=无背景（A<=0 同义，入批前统一跳过）
        public Color? BackgroundColor;

        // null=currentColor（CurrentColor 由 builder 按继承文字色解析）
        public Color? BorderLeftColor;

        public Color? BorderTopColor;

        public Color? BorderRightColor;

        public Color? BorderBottomColor;

        public Color CurrentColor = Color.White;

        // Widget.Draw 只在 IsDrawRequired 时调度（Widget.cs DrawWidgetsHierarchy 过滤），边框壳
        // 每帧都画（照 TablePanelWidget.MeasureOverride 先例）
        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsDrawRequired = true;
            base.MeasureOverride(parentAvailableSize);
        }

        public override void Draw(DrawContext dc) {
            Vector4 w = BorderWidths;
            float left = MathF.Max(w.X, 0f);
            float top = MathF.Max(w.Y, 0f);
            float right = MathF.Max(w.Z, 0f);
            float bottom = MathF.Max(w.W, 0f);
            bool hasBorder = left + top + right + bottom > 0f;
            // 背景与边框独立成立（有背景无边框也要画）；ActualSize 无效（未布局/零尺寸）两者都不画
            if ((!hasBorder && !HasBackground)
                || ActualSize.X <= 0f
                || ActualSize.Y <= 0f) {
                return;
            }
            FlatBatch2D batch = dc.PrimitivesRenderer2D.FlatBatch(-1, DepthStencilState.None);
            float width = ActualSize.X;
            float height = ActualSize.Y;
            // 背景整幅先入批（FlatBatch 同层按入批序：背景垫在同壳边框之下）
            if (HasBackground) {
                QueueQuad(batch, 0f, 0f, width, height, BackgroundColor.Value * GlobalColorTransform);
            }
            if (top > 0f) {
                QueueQuad(batch, 0f, 0f, width, top, EdgeColor(BorderTopColor));
            }
            if (bottom > 0f) {
                QueueQuad(batch, 0f, height - bottom, width, bottom, EdgeColor(BorderBottomColor));
            }
            if (left > 0f) {
                QueueQuad(batch, 0f, 0f, left, height, EdgeColor(BorderLeftColor));
            }
            if (right > 0f) {
                QueueQuad(batch, width - right, 0f, right, height, EdgeColor(BorderRightColor));
            }
        }

        Color EdgeColor(Color? color) => (color ?? CurrentColor) * GlobalColorTransform;

        // 背景成立判定：有值且非全透明
        bool HasBackground => BackgroundColor.HasValue && BackgroundColor.Value.A > 0;

        // 局部坐标 quad 四角各自经 GlobalTransform 变换后入批（斜切/旋转变换下不塌陷）
        void QueueQuad(FlatBatch2D batch, float x, float y, float w, float h, Color color) {
            if (color.A <= 0) {
                return;
            }
            Matrix m = GlobalTransform;
            Vector2 p1 = Vector2.Transform(new Vector2(x, y), m);
            Vector2 p2 = Vector2.Transform(new Vector2(x + w, y), m);
            Vector2 p3 = Vector2.Transform(new Vector2(x + w, y + h), m);
            Vector2 p4 = Vector2.Transform(new Vector2(x, y + h), m);
            batch.QueueQuad(p1, p2, p3, p4, 0f, color);
        }
    }
}
