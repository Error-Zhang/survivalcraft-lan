using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class InductionFurnaceDevice : IlluminableModelDevice, IEntityDevice<ComponentInductionFurnace>, IVoltDevice, IFluidTransferrerDevice
{
	public InductionFurnaceDevice()
		: base("InductionFurnaceDevice", "Models/InductionFurnace", "InductionFurnace", "InductionFurnace_Lit")
	{
	}

	public string GetEntityName(int value)
	{
		return "InductionFurnace";
	}

	public Widget GetWidget(IInventory inventory, ComponentInductionFurnace component)
	{
		return new InductionFurnaceWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[3]
		{
			new InductionFurnaceDeviceBehavior(project),
			new VoltDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 99;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new InductionFurnaceVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 4f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new InductionFurnaceTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		int direction = GetDirection(value);
		DevicesUtils.GetSideFaceOfXZ(direction, out var face2, out var face3);
		int num = CellFace.OppositeFace(direction);
		if (face == num || face == face3 || face == face2 || face == direction)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 1f);
		}
		return null;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is MoltingEquationRecipe;
	}
}
