using Engine;
using Game;

namespace SCIENEW.Ponder;

public class PickablePonderBlock : IPonderBlock
{
	public Pickable Pickable;

	public int Value => Pickable.Value;

	public Vector3 Position => Pickable.Position;

	public object Tag { get; set; }

	public bool Paused { get; set; }

	public PickablePonderBlock(Pickable pickable, object tag)
	{
		Pickable = pickable;
		Tag = tag;
	}
}
