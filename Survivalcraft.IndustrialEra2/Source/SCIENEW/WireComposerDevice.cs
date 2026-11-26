using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class WireComposerDevice : FourDirectionalDevice, IEntityDevice<ComponentWireComposer>, IElectricElementDevice
{
	public WireComposerDevice()
		: base("WireComposerDevice", 65, 107, 103, 55, 107, rotate: true)
	{
	}

	public string GetEntityName(int value)
	{
		return "WireComposer";
	}

	public Widget GetWidget(IInventory inventory, ComponentWireComposer component)
	{
		return new WireComposerWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new WireComposerDeviceBehavior(project)
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
		return recipe is WireComposingRecipe;
	}
}
