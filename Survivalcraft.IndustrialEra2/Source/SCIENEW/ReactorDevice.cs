using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class ReactorDevice : FourDirectionalDevice, IEntityDevice<ComponentReactor>, IFluidTransferrerDevice
{
	public ReactorDevice()
		: base("ReactorDevice", 192, 192, 192, 232, 232)
	{
	}

	public string GetEntityName(int value)
	{
		return "Reactor";
	}

	public Widget GetWidget(IInventory inventory, ComponentReactor component)
	{
		return new ReactorWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new ReactorDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new ReactorTransferrer(point, subsystemTerrain);
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

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is ReactionEquationRecipe;
	}
}
