using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class MercuryPresserDevice : FourDirectionalDevice, IEntityDevice<ComponentMercuryPresser>, IElectricElementDevice
{
	public MercuryPresserDevice()
		: base("MercuryPresserDevice", 74, 75, 75, 75, 75)
	{
	}

	public string GetEntityName(int value)
	{
		return "MercuryPresser";
	}

	public Widget GetWidget(IInventory inventory, ComponentMercuryPresser component)
	{
		return new MercuryPresserWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new MercuryPresserDeviceBehavior(project)
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
			switch (one2OneRecipe.ProcessType)
			{
			case "MercuryPressingMedium":
			case "MercuryPressingSlow":
			case "Pressing":
				return true;
			}
		}
		return false;
	}
}
