using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class ElectrolyzerDevice : FourDirectionalDevice, IEntityDevice<ComponentElectrolyzer>, IFluidTransferrerDevice, IVoltDevice
{
	public ElectrolyzerDevice()
		: base("ElectrolyzerDevice", 108, 108, 108, 224, 99, rotate: true)
	{
	}

	public string GetEntityName(int value)
	{
		return "Electrolyzer";
	}

	public Widget GetWidget(IInventory inventory, ComponentElectrolyzer component)
	{
		return new ElectrolyzerWidget(inventory, component);
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[3]
		{
			new ElectrolyzerDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new ElectrolyzerTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		int direction = GetDirection(value);
		DevicesUtils.GetSideFaceOfXZ(direction, out var face2, out var face3);
		int num = CellFace.OppositeFace(direction);
		if (face == num || face == face3 || face == face2 || face == direction)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 16f);
		}
		return null;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new ElectrolyzerVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 4f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is ElectrolyzingEquationRecipe;
	}
}
