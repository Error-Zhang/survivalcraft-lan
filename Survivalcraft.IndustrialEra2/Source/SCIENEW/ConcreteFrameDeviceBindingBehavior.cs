using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ConcreteFrameDeviceBindingBehavior : DeviceBehavior
{
	public SubsystemTerrain m_subsystemTerrain;

	public ConcreteFrameDeviceBindingBehavior(Project project)
		: base(project)
	{
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		if (m_subsystemTerrain.Terrain.GetCellContents(neighborX, neighborY, neighborZ) != WaterBlock.Index)
		{
			return;
		}
		int cellValue = m_subsystemTerrain.Terrain.GetCellValue(x, y, z);
		DeviceBlock device = DevicesBlockManager.GetDevice(cellValue);
		if (device is ConcreteFrameWithLevelDevice && ConcreteFrameWithLevelDevice.GetLevel(cellValue) >= 3)
		{
			if (device.GetCraftingId() == "ConcreteFrameWithLevelDevice")
			{
				m_subsystemTerrain.DestroyCell(0, x, y, z, NormalBlockManager.GetNormalBlockID("ConcreteWall"), noDrop: true, noParticleSystem: false);
			}
			else if (device.GetCraftingId() == "HighStrengthConcreteFrameWithLevelDevice")
			{
				m_subsystemTerrain.DestroyCell(0, x, y, z, NormalBlockManager.GetNormalBlockID("HighStrengthConcreteWall"), noDrop: true, noParticleSystem: false);
			}
			Project.FindSubsystem<SubsystemPickables>(throwOnError: true).AddPickable(DevicesBlockManager.GetDeviceValueByType<ConcreteFrameDevice>(), 1, new Vector3(x, y, z) + new Vector3(0.5f), null, null);
		}
	}
}
