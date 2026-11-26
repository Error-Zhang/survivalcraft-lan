using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class AlloyFurnaceDevice : IlluminableDevice, IEntityDevice<ComponentAlloyFurnace>, IFluidTransferrerDevice
{
	public AlloyFurnaceDevice()
		: base("AlloyFurnaceDevice", 130, 129, 100, 100, 221, 221)
	{
	}

	public string GetEntityName(int value)
	{
		return "AlloyFurnace";
	}

	public Widget GetWidget(IInventory inventory, ComponentAlloyFurnace component)
	{
		return new AlloyFurnaceWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new AlloyFurnaceDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new AlloyFurnaceTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		int direction = GetDirection(value);
		DevicesUtils.GetSideFaceOfXZ(direction, out var face2, out var face3);
		int num = CellFace.OppositeFace(direction);
		if (face == num || face == face3 || face == face2)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 1f);
		}
		return null;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is AlloyingEquationRecipe;
	}
}
