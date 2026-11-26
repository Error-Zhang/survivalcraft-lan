using Game;
using GameEntitySystem;

namespace SCIENEW;

public class DrillerDevice : SixDirectionModelDevice, IEntityDevice<ComponentDriller>
{
	public enum Mode
	{
		Drill,
		Dispose
	}

	public DrillerDevice()
		: base("DrillerDevice", "Models/Dirller", "Dirller", faceTransparent: true)
	{
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, raycastResult.CellFace.Face);
		return result;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 100;
	}

	public string GetEntityName(int value)
	{
		return "Driller";
	}

	public Widget GetWidget(IInventory inventory, ComponentDriller component)
	{
		return new DrillerWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new DrillerDeviceBehavior(project)
		};
	}
}
