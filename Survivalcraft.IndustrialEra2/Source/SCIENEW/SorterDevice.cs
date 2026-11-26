using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SorterDevice : DefaultDevice, IEntityDevice<ComponentChest>
{
	public int FrontSlot;

	public int LeftSlot;

	public int BackSlot;

	public int RightSlot;

	public int VerticalSlot;

	public SorterDevice()
		: base("SorterDevice")
	{
		FrontSlot = 167;
		LeftSlot = 200;
		BackSlot = 185;
		RightSlot = 201;
		VerticalSlot = 170;
	}

	public string GetEntityName(int value)
	{
		return "Sorter";
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXZ(componentMiner, isOpposite: true));
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public Widget GetWidget(IInventory inventory, ComponentChest component)
	{
		return new SorterWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new SorterDeviceBehavior(project)
		};
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + -2;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return face switch
		{
			0 => FrontSlot,
			1 => LeftSlot,
			2 => BackSlot,
			3 => RightSlot,
			_ => VerticalSlot,
		};
	}
}
