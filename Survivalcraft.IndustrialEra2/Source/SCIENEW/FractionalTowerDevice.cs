using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class FractionalTowerDevice : ModelDevice, IEntityDevice<ComponentFractionalTower>, IFluidTransferrerDevice
{
	public FractionalTowerDevice()
		: base("FractionalTowerDevice", "Models/FractionalTower", "FractionalTower", IndustrialModLoader.IETexture, Color.White, makeEmissive: false, flipWindingOrder: false, doubleSided: true)
	{
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Tank").ParentBone);
		m_blockMesh.AppendModelMeshPart(m_model.FindMesh("Tank").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, new Color(142, 41, 41));
		m_standaloneBlockMesh.AppendModelMeshPart(m_model.FindMesh("Tank").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, new Color(142, 41, 41));
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetAlphaTest);
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		if (face != 4)
		{
			return face != 5;
		}
		return false;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 205;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new FractionalTowerDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public string GetEntityName(int value)
	{
		return "FractionalTower";
	}

	public Widget GetWidget(IInventory inventory, ComponentFractionalTower component)
	{
		return new FractionalTowerWidget(inventory, component);
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new FractionalTowerTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face != 4)
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 16f);
		}
		return null;
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		Color color = new Color(142, 41, 41);
		return new BlockDebrisParticleSystem(subsystemTerrain, position, 1f, 1f, color, GetFaceTextureSlot(0, value), IndustrialModLoader.IETexture);
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is FractionatingEquationRecipe;
	}
}
