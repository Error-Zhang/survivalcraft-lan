using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SubsystemUID : Subsystem, IDrawable
{
	public int[] DrawOrders => new int[1] { 3000 };

	public void Draw(Camera camera, int drawOrder)
	{
	}
}
