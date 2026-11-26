using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class WorkshopDevice : DefaultDevice, IEntityDevice<ComponentIECraftingTable>, IElectricElementDevice, IVoltDevice
{
	public WorkshopDevice()
		: base("WorkshopDevice")
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		switch (face)
		{
		case 4:
			return 68;
		case 1:
		case 3:
			return 63;
		case 5:
			return 99;
		default:
			return 62;
		}
	}

	public string GetEntityName(int value)
	{
		return "Workshop";
	}

	public Widget GetWidget(IInventory inventory, ComponentIECraftingTable component)
	{
		return new WorkshopWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new WorkshopDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new WorkshopElectricElement(subsystemElectricity, new Point3(x, y, z));
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

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new WorkshopVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 3f;
	}

	public float GetStandardUL(int value)
	{
		return 2f;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe.GetType().IsAssignableTo(typeof(OriginalCraftingRecipe));
	}
}
