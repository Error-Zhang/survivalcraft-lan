using Game;

namespace SCIENEW;

public struct WidgetAlignmentData
{
	public WidgetAlignment Horizontal;

	public WidgetAlignment Vertical;

	public static int ToInt(WidgetAlignmentData widgetAlignmentData)
	{
		return (int)((int)widgetAlignmentData.Horizontal * 4 + widgetAlignmentData.Vertical);
	}

	public int ToInt()
	{
		return ToInt(this);
	}

	public static WidgetAlignmentData FromInt(int data)
	{
		return new WidgetAlignmentData
		{
			Horizontal = (WidgetAlignment)(data / 4),
			Vertical = (WidgetAlignment)(data % 4)
		};
	}
}
