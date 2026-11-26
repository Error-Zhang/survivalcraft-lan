using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class GReactorControllerDevice : FourDirectionModelDevice, IEntityDevice<ComponentGReactorController>, IElectricElementDevice
{
	public GReactorControllerDevice()
		: base("GReactorControllerDevice", "Models/GReactorController", "GReactorController")
	{
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 2;
	}

	public string GetEntityName(int value)
	{
		return "GReactorController";
	}

	public Widget GetWidget(IInventory inventory, ComponentGReactorController component)
	{
		return new GReactorControllerWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new GReactorControllerDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new GReactorControllerElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return face switch
		{
			5 => (ElectricConnectorType?)ElectricConnectorType.Output,
			4 => ElectricConnectorType.Input,
			_ => null,
		};
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}
}
