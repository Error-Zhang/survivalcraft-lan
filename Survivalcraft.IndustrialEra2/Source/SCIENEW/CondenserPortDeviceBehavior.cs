using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class CondenserPortDeviceBehavior : DeviceBehavior
{
	public CondenserPortDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		CondenserPortUtils.RebindHostFromPort(Project, new Point3(x, y, z), value);
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		CondenserPortUtils.RebindHostFromPort(Project, new Point3(x, y, z), value);
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		int cellValueFast = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true).Terrain.GetCellValueFast(x, y, z);
		if (CondenserPortUtils.GetPortRole(cellValueFast).HasValue)
		{
			CondenserPortUtils.RebindHostFromPort(Project, new Point3(x, y, z), cellValueFast);
		}
	}
}
