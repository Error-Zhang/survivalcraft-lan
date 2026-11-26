using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Implementation;

namespace SCIENEW;

public class GasFurnaceDevice : IlluminableDevice, IEntityDevice<ComponentGasFurnace>, IElectricElementDevice, IFluidTransferrerDevice
{
	public GasFurnaceDevice()
		: base("GasFurnaceDevice", 132, 133, 100, 100, 221, 221)
	{
	}

	public string GetEntityName(int value)
	{
		return "GasFurnace";
	}

	public Widget GetWidget(IInventory inventory, ComponentGasFurnace component)
	{
		return new GasFurnaceWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new GasFurnaceDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
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

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new GasFurnaceTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		int direction = GetDirection(value);
		DevicesUtils.GetSideFaceOfXZ(direction, out var face2, out var face3);
		int num = CellFace.OppositeFace(direction);
		if (face == num || face == face3 || face == face2 || face == 5)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 16f);
		}
		return null;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe.GetType().IsAssignableTo(typeof(OriginalSmeltingRecipe));
	}
}
