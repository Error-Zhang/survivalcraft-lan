using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class UThickenerDevice : FourDirectionalDevice, IEntityDevice<ComponentUThickener>, IVoltDevice, IFluidTransferrerDevice
{
	public UThickenerDevice()
		: base("UThickenerDevice", 136, 100, 100, 221, 221)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "UThickener";
	}

	public Widget GetWidget(IInventory inventory, ComponentUThickener component)
	{
		return new UThickenerWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[3]
		{
			new UThickenerDeviceBehavior(project),
			new VoltDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new UThickenerVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 3f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new UThickenerTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		int direction = GetDirection(value);
		DevicesUtils.GetSideFaceOfXZ(direction, out var face2, out var face3);
		int num = CellFace.OppositeFace(direction);
		if (face == num || face == face3 || face == face2)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 16f);
		}
		return null;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is ThickeningEquationRecipe;
	}
}
