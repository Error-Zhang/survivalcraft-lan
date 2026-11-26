using Engine;

namespace SCIENEW.Events;

public sealed class FluidPushingContext
{
	public Point3 FromPosition { get; }

	public Point3 ToPosition { get; }

	public int FluidValue { get; }

	public float Volume { get; set; }

	public FluidPushingContext(Point3 fromPosition, Point3 toPosition, int fluidValue, float volume)
	{
		FromPosition = fromPosition;
		ToPosition = toPosition;
		FluidValue = fluidValue;
		Volume = volume;
	}
}
