using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class PresserDevice : FourDirectionalDevice, IEntityDevice<ComponentPresser>, IElectricElementDevice
{
	public PresserDevice()
		: base("PresserDevice", 76, 107, 103, 207, 107, rotate: true)
	{
	}

	public string GetEntityName(int value)
	{
		return "Presser";
	}

	public Widget GetWidget(IInventory inventory, ComponentPresser component)
	{
		return new PresserWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new PresserDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new CrafterPopResultsElectricElement(subsystemElectricity, new Point3(x, y, z), GetDirection(value));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		if (recipe is One2OneRecipe one2OneRecipe)
		{
			return one2OneRecipe.ProcessType == "Pressing";
		}
		return false;
	}
}
