using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class FluidInserterDevice : SixDirectionModelDevice, IEntityDevice<ComponentFluidInserter>, IElectricElementDevice
{
	public FluidInserterDevice()
		: base("FluidInserterDevice", "Models/FluidInserter", "Inserter", faceTransparent: true, useStandaloneMesh: false)
	{
	}

	public string GetEntityName(int value)
	{
		return "FluidInserter";
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXYZ(componentMiner, isOpposite: true));
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public Widget GetWidget(IInventory inventory, ComponentFluidInserter component)
	{
		return new FluidInserterWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new FluidInserterDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new FluidInserterElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + -2;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 107;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
		for (int i = 0; i < 5; i++)
		{
			generator.GenerateWireVertices(value, x, y, z, i, 0f, Vector2.Zero, geometry.SubsetOpaque);
		}
	}
}
