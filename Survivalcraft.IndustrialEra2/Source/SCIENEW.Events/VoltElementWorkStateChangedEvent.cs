using Engine;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW.Events;

public readonly struct VoltElementWorkStateChangedEvent(Project project, Point3 position, int blockValue, WorkState previous, WorkState current)
{
	public Project Project { get; } = project;

	public Point3 Position { get; } = position;

	public int BlockValue { get; } = blockValue;

	public WorkState Previous { get; } = previous;

	public WorkState Current { get; } = current;
}
