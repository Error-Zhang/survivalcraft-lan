using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class VacuumFurnaceDevice : IlluminableModelDevice, IEntityDevice<ComponentVacuumFurnace>, IVoltDevice, IElectricElementDevice
{
	public VacuumFurnaceDevice()
		: base("VacuumFurnaceDevice", "Models/VacuumFurnace", "VacuumFurnace", "VacuumFurnace_Lit")
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		HashSet<int> hashSet = new HashSet<int>();
		hashSet.Add(0);
		hashSet.Add(1);
		hashSet.Add(2);
		hashSet.Add(3);
		hashSet.Add(4);
		hashSet.Add(5);
		hashSet.Remove(DevicesUtils.GetInnerData(value));
		return hashSet;
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new VacuumFurnaceVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 4f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public string GetEntityName(int value)
	{
		return "VacuumFurnace";
	}

	public Widget GetWidget(IInventory inventory, ComponentVacuumFurnace component)
	{
		return new VacuumFurnaceWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new VacuumFurnaceDeviceBehavior(project),
			new VoltDeviceBehavior(project)
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
		return recipe is VacuumFurnaceRecipe;
	}
}
