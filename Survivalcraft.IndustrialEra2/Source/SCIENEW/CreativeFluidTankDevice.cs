using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CreativeFluidTankDevice : ModelDevice, IEntityDevice<ComponentCreativeFluidTank>, IFluidTransferrerDevice
{
	public CreativeFluidTankDevice()
		: base("CreativeFluidTankDevice", "Models/FluidTank", "Tank_Normal", IndustrialModLoader.IETexture, new Color(69, 174, 255), makeEmissive: false, flipWindingOrder: false, doubleSided: true)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) - 3;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override int GetShadowStrength(int value)
	{
		return -99;
	}

	public override float GetObjectShadowStrength(int value)
	{
		return 0f;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetAlphaTest);
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 205;
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		Color color = new Color(69, 174, 255);
		return new BlockDebrisParticleSystem(subsystemTerrain, position, 1f, 1f, color, GetFaceTextureSlot(0, value), IndustrialModLoader.IETexture);
	}

	public string GetEntityName(int value)
	{
		return "CreativeFluidTank";
	}

	public Widget GetWidget(IInventory inventory, ComponentCreativeFluidTank component)
	{
		return new CreativeFluidTankWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CreativeFluidTankDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new CreativeFluidTankTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 16f);
	}
}
