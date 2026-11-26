using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class WindTurbineDeviceBehavior : EntityDeviceBehavior<ComponentWindTurbine>
{
	public SubsystemTerrain m_subsystemTerrain;

	public WindTurbineDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override void OnPoll(int value, int x, int y, int z, int pollPass)
	{
		if (BlockEntityUtils.GetBlockEntity(position: new Point3(x, y, z), subsystemTerrain: m_subsystemTerrain, blockEntity: out ComponentBlockEntity blockEntity))
		{
			blockEntity.Entity.FindComponent<ComponentWindTurbine>(throwOnError: true).UpdateOnPoll();
		}
	}
}
