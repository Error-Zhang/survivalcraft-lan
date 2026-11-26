using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class UnloaderDevice : SixDirectionalDevice, IEntityDevice<ComponentUnloader>, IElectricElementDevice
{
	public UnloaderDevice()
		: base("UnloaderDevice", 184, 170, 170)
	{
	}

	public string GetEntityName(int value)
	{
		return "Unloader";
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXYZ(componentMiner, isOpposite: true));
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public Widget GetWidget(IInventory inventory, ComponentUnloader component)
	{
		return new ChestNameWidget(inventory, component, "UnloaderWidget");
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new UnloaderDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new UnloaderElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + -2;
	}
}
