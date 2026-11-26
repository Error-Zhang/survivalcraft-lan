using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class CokeOvenDevice : IlluminableDevice, IEntityDevice<ComponentCokeOven>, IElectricElementDevice
{
	public CokeOvenDevice()
		: base("CokeOvenDevice", 173, 242, 174, 4, 4)
	{
		DownSlot = 4;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Stone";
	}

	public string GetEntityName(int value)
	{
		return "CokeOven";
	}

	public Widget GetWidget(IInventory inventory, ComponentCokeOven component)
	{
		return new CokeOvenWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new CokeOvenDeviceBehavior(project)
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
		return recipe is CokingRecipe;
	}
}
