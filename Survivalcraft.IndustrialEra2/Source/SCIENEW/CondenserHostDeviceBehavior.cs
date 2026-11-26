using Engine;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class CondenserHostDeviceBehavior<T> : EntityDeviceBehavior<T> where T : ComponentCondenser
{
	public CondenserHostDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		base.OnBlockAdded(value, oldValue, x, y, z);
		CondenserPortUtils.TryRebindHostAt(Project, new Point3(x, y, z));
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		CondenserPortUtils.MaybeRebindCondenserHost(Project, x, y, z);
		base.OnBlockRemoved(value, newValue, x, y, z);
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		CondenserPortUtils.MaybeRebindCondenserHost(Project, x, y, z);
	}
}
