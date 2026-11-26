using Engine;
using Game;

namespace SCIENEW;

public class AF_SlotWidget : InventorySlotWidget
{
	public AF_SlotWidget()
	{
		Children.Remove(Children.Find<BevelledRectangleWidget>());
		RoundRectangleWidget widget = new RoundRectangleWidget
		{
			Radius = 4f,
			FillColor = new Color(178, 132, 210, 255),
			IsHitTestVisible = false
		};
		Children.Insert(0, widget);
	}
}
