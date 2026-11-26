using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class FermenterDevice : FourDirectionalDevice, IEntityDevice<ComponentFermenter>, IFluidTransferrerDevice
{
	public FermenterDevice()
		: base("FermenterDevice", 233, 233, 233, 234, 232)
	{
	}

	public string GetEntityName(int value)
	{
		return "Fermenter";
	}

	public Widget GetWidget(IInventory inventory, ComponentFermenter component)
	{
		return new FermenterWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new FermenterDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new FermenterTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face == 4)
		{
			return null;
		}
		return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 16f);
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is FermentingEquationRecipe;
	}
}
