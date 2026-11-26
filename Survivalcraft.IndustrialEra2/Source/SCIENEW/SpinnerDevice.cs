using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class SpinnerDevice : FourDirectionalDevice, IEntityDevice<ComponentSpinner>, IElectricElementDevice
{
	public SpinnerDevice()
		: base("SpinnerDevice", 186, 186, 186, 118, 186, rotate: true)
	{
	}

	public string GetEntityName(int value)
	{
		return "Spinner";
	}

	public Widget GetWidget(IInventory inventory, ComponentSpinner component)
	{
		return new SpinnerWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new SpinnerDeviceBehavior(project)
		};
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Wood";
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) - 1;
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
			return one2OneRecipe.ProcessType == "Spinning";
		}
		return false;
	}
}
