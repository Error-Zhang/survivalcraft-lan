using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class KibblerDevice : FourDirectionalDevice, IEntityDevice<ComponentKibbler>, IElectricElementDevice
{
	public KibblerDevice()
		: base("KibblerDevice", 78, 107, 103, 208, 107, rotate: true)
	{
	}

	public string GetEntityName(int value)
	{
		return "Kibbler";
	}

	public Widget GetWidget(IInventory inventory, ComponentKibbler component)
	{
		return new KibblerWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new KibblerDeviceBehavior(project)
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
			return one2OneRecipe.ProcessType == "Kibbling";
		}
		return false;
	}
}
