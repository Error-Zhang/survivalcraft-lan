using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Implementation;

namespace SCIENEW;

public class MachineToolDevice : FourDirectionalDevice, IEntityDevice<ComponentIECraftingTable>, IElectricElementDevice
{
	public MachineToolDevice()
		: base("MachineToolDevice", 105, 105, 105, 146)
	{
	}

	public string GetEntityName(int value)
	{
		return "IECraftingTable";
	}

	public Widget GetWidget(IInventory inventory, ComponentIECraftingTable component)
	{
		return new MachineToolWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new MachineToolDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new MachineToolElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		if ((uint)(face - 4) > 1u)
		{
			return null;
		}
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe.GetType().IsAssignableTo(typeof(OriginalCraftingRecipe));
	}
}
