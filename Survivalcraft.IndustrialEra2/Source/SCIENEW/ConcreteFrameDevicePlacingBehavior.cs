using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ConcreteFrameDevicePlacingBehavior : DeviceBehavior
{
	public ConcreteFrameDevicePlacingBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		return Place(raycastResult.CellFace.Point, componentMiner, 40);
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		int cellValueFast = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true).Terrain.GetCellValueFast(x, y - 1, z);
		bool flag = BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast)].IsTransparent_(cellValueFast);
		if (flag)
		{
			bool flag2 = BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast)] is BaseDeviceBlock;
			if (flag2)
			{
				DeviceBlock device = DevicesBlockManager.GetDevice(cellValueFast);
				bool flag3 = ((device is ConcreteFrameDevice || device is ConcreteFrameWithLevelDevice) ? true : false);
				flag2 = flag3;
			}
			flag = !flag2;
		}
		if (flag)
		{
			Project.FindSubsystem<SubsystemTerrain>(throwOnError: true).DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
		}
	}

	public bool Place(Point3 position, ComponentMiner componentMiner, int count)
	{
		if (count <= 0)
		{
			return false;
		}
		int activeBlockValue = componentMiner.ActiveBlockValue;
		if (Terrain.ExtractContents(activeBlockValue) == BaseDeviceBlock.Index && DevicesBlockManager.GetDevice(activeBlockValue) is ConcreteFrameDevice)
		{
			Point3 position2 = position + Point3.UnitY;
			int cellValue = Project.FindSubsystem<SubsystemTerrain>().Terrain.GetCellValue(position2.X, position2.Y, position2.Z);
			bool flag = Terrain.ExtractContents(cellValue) == BaseDeviceBlock.Index;
			if (flag)
			{
				DeviceBlock device = DevicesBlockManager.GetDevice(cellValue);
				bool flag2 = ((device is ConcreteFrameDevice || device is ConcreteFrameWithLevelDevice) ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				return Place(position2, componentMiner, --count);
			}
			if (Terrain.ExtractContents(cellValue) == 0)
			{
				Project.FindSubsystem<SubsystemTerrain>(throwOnError: true).ChangeCell(position2.X, position2.Y, position2.Z, DevicesBlockManager.GetDeviceValueByType<ConcreteFrameDevice>());
				componentMiner.Inventory.RemoveSlotItems(componentMiner.Inventory.ActiveSlotIndex, 1);
				Project.FindSubsystem<SubsystemSoundMaterials>().PlayImpactSound(PlanksBlock.Index, new Vector3(position), 1f);
				return true;
			}
		}
		return false;
	}
}
