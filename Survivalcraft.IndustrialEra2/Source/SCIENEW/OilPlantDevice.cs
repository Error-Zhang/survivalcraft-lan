using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class OilPlantDevice : DefaultDevice, IEntityDevice<ComponentOilPlant>, IFluidTransferrerDevice
{
	public OilPlantDevice()
		: base("OilPlantDevice")
	{
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		int num = ((DevicesUtils.GetInnerData(value) >> 2 == 1) ? 217 : 216);
		int num2 = 234;
		int num3 = 232;
		return face switch
		{
			4 => num2,
			5 => num3,
			_ => num,
		};
	}

	public string GetEntityName(int value)
	{
		return "OilPlant";
	}

	public Widget GetWidget(IInventory inventory, ComponentOilPlant component)
	{
		return new OilPlantWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new OilPlantDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new OilPlantTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face != 4 && face != 5)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 16f);
		}
		return null;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is PetrochemicalEquationRecipe;
	}
}
