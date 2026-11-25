using Engine;
using Game;

namespace Survivalcraft.Multiplayer {
    public class NetworkStatusWidget : LabelWidget {
        const float RightWidgetGap = 8f;

        public override void Update() {
            base.Update();

            int latency = NetworkManager.ClientLatency;
            Text = NetworkManager.IsClientRunning ? (latency >= 0 ? $"延迟：{latency}ms" : "延迟：--ms") : string.Empty;
        }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            if (string.IsNullOrEmpty(Text)) {
                IsDrawRequired = false;
                DesiredSize = Vector2.Zero;
                return;
            }
            base.MeasureOverride(parentAvailableSize);
        }

        public override void Arrange(Vector2 position, Vector2 parentActualSize) {
            if (ParentWidget is ContainerWidget parent) {
                position.X = Math.Max(0f, position.X - GetRightHudInset(parent));
            }
            base.Arrange(position, parentActualSize);
        }

        internal static float GetRightHudInset(ContainerWidget parent) {
            ContainerWidget rightControlsContainerWidget = parent.Children.Find<ContainerWidget>("RightControlsContainer", false);
            if (rightControlsContainerWidget?.IsVisible != true) {
                return 0f;
            }

            float width = rightControlsContainerWidget.ActualSize.X;
            if (width <= 0f && !float.IsPositiveInfinity(rightControlsContainerWidget.ParentDesiredSize.X)) {
                width = rightControlsContainerWidget.ParentDesiredSize.X;
            }
            if (width <= 0f) {
                return 0f;
            }

            float hiddenWidth = MathUtils.Max(rightControlsContainerWidget.RenderTransform.M41, 0f);
            float visibleWidth = MathUtils.Max(width - hiddenWidth, 0f);
            return visibleWidth > 0f ? visibleWidth + RightWidgetGap : 0f;
        }

    }
}
