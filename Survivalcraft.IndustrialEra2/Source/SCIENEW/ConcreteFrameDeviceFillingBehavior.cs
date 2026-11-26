using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ConcreteFrameDeviceFillingBehavior : DeviceBehavior
{
	public SubsystemTerrain m_subsystemTerrain;

	public ConcreteFrameDeviceFillingBehavior(Project project)
		: base(project)
	{
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		bool flag = Fill(raycastResult.CellFace.Point, componentMiner.ActiveBlockValue);
		if (flag)
		{
			componentMiner.Inventory.RemoveSlotItems(componentMiner.Inventory.ActiveSlotIndex, 1);
		}
		return flag;
	}

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		if (worldItem.ToRemove)
		{
			return;
		}
		while (!worldItem.ToRemove && Fill(cellFace.Point, worldItem.Value))
		{
			if (worldItem is Pickable pickable)
			{
				pickable.Count--;
				if (pickable.Count == 0)
				{
					pickable.ToRemove = true;
				}
			}
			else
			{
				worldItem.ToRemove = true;
			}
		}
	}

	public bool Fill(Point3 position, int blockIndex)
	{
		int cellValue = m_subsystemTerrain.Terrain.GetCellValue(position.X, position.Y - 1, position.Z);
		if (Terrain.ExtractContents(cellValue) == BaseDeviceBlock.Index)
		{
			if (DevicesBlockManager.GetDevice(cellValue) is ConcreteFrameDevice)
			{
				return Fill(position - Point3.UnitY, blockIndex);
			}
			if (DevicesBlockManager.GetDevice(cellValue) is ConcreteFrameWithLevelDevice && ConcreteFrameWithLevelDevice.GetLevel(cellValue) < 3)
			{
				return Fill(position - Point3.UnitY, blockIndex);
			}
		}
		if (Terrain.ExtractContents(blockIndex) == BaseItemBlock.Index)
		{
			int cellValue2 = m_subsystemTerrain.Terrain.GetCellValue(position.X, position.Y, position.Z);
			DeviceBlock device = DevicesBlockManager.GetDevice(cellValue2);
			if (ItemsBlockManager.GetItem(blockIndex).GetCraftingId() == "ConcretePowder")
			{
				string craftingId = device.GetCraftingId();
				if ((craftingId == "ConcreteFrameWithLevelDevice" || craftingId == "ConcreteFrameDevice") ? true : false)
				{
					int num = ((!(device is ConcreteFrameDevice)) ? (ConcreteFrameWithLevelDevice.GetLevel(cellValue2) + 1) : 0);
					if (num >= 4)
					{
						return false;
					}
					m_subsystemTerrain.ChangeCell(position.X, position.Y, position.Z, DevicesBlockManager.GetDeviceValue("ConcreteFrameWithLevelDevice", num));
					Project.FindSubsystem<SubsystemSoundMaterials>().PlayImpactSound(DirtBlock.Index, new Vector3(position), 1f);
					return true;
				}
			}
			else if (ItemsBlockManager.GetItem(blockIndex).GetCraftingId() == "HighStrengthConcretePowder")
			{
				string craftingId = device.GetCraftingId();
				if ((craftingId == "HighStrengthConcreteFrameWithLevelDevice" || craftingId == "ConcreteFrameDevice") ? true : false)
				{
					int num2 = ((!(device is ConcreteFrameDevice)) ? (ConcreteFrameWithLevelDevice.GetLevel(cellValue2) + 1) : 0);
					if (num2 >= 4)
					{
						return false;
					}
					m_subsystemTerrain.ChangeCell(position.X, position.Y, position.Z, DevicesBlockManager.GetDeviceValue("HighStrengthConcreteFrameWithLevelDevice", num2));
					Project.FindSubsystem<SubsystemSoundMaterials>().PlayImpactSound(DirtBlock.Index, new Vector3(position), 1f);
					return true;
				}
			}
		}
		return false;
	}
}
