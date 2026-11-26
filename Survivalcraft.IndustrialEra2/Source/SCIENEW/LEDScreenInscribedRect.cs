using Engine;

namespace SCIENEW;

public struct LEDScreenInscribedRect
{
	public int Area;

	public Point2 TopLeft;

	public int Width;

	public int Height;

	public bool IsValid => Area > 0;
}
