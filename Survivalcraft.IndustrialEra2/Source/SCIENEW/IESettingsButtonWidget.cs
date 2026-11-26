using Game;

namespace SCIENEW;

public class IESettingsButtonWidget : BevelledButtonWidget {
    public override void Update() {
        if (IsClicked) {
            ScreensManager.SwitchScreen("IndustrialEraHub", ScreensManager.CurrentScreen);
        }
    }
}
