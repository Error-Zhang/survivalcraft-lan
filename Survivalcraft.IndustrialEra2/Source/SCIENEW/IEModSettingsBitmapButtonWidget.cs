using Game;

namespace SCIENEW;

public class IEModSettingsBitmapButtonWidget : BitmapButtonWidget
{
	public override void Update()
	{
		base.Update();
		if (IsClicked)
		{
			ScreensManager.SwitchScreen("IndustrialEraHub", ScreensManager.CurrentScreen);
		}
	}
}
