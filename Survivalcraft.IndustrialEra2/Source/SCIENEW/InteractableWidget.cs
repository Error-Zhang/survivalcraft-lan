using Engine;
using Game;

namespace SCIENEW;

public class InteractableWidget : ClickableWidget
{
	public bool IsMouseHover { get; set; }

	public override void Update()
	{
		base.Update();
		if (HitTestGlobal(Input.MousePosition ?? new Vector2(0f, 0f)) == this || HitTestGlobal(Input.PadCursorPosition) == this)
		{
			IsMouseHover = true;
		}
		else
		{
			IsMouseHover = false;
		}
	}
}
