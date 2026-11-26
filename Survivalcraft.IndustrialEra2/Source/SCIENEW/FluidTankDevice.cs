using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class FluidTankDevice : ModelDevice, IEntityDevice<ComponentFluidTank>, IFluidTransferrerDevice
{
	private readonly BlockMesh m_upMesh = new BlockMesh();

	private readonly BlockMesh m_downMesh = new BlockMesh();

	private readonly BlockMesh m_straightMesh = new BlockMesh();

	public FluidTankDevice()
		: base("FluidTankDevice", "Models/FluidTank", "Tank_Normal", IndustrialModLoader.IETexture, new Color(142, 41, 41), makeEmissive: false, flipWindingOrder: false, doubleSided: true)
	{
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Tank_Up").ParentBone);
		m_upMesh.AppendModelMeshPart(m_model.FindMesh("Tank_Up").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, new Color(142, 41, 41));
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Tank_Down").ParentBone);
		m_downMesh.AppendModelMeshPart(m_model.FindMesh("Tank_Down").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, new Color(142, 41, 41));
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Tank_Straight").ParentBone);
		m_straightMesh.AppendModelMeshPart(m_model.FindMesh("Tank_Straight").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, new Color(142, 41, 41));
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
		int cellValueFast = generator.SubsystemTerrain.Terrain.GetCellValueFast(x, y + 1, z);
		int cellValueFast2 = generator.SubsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
		bool flag = DevicesBlockManager.GetDevice(cellValueFast) is FluidTankDevice;
		bool flag2 = DevicesBlockManager.GetDevice(cellValueFast2) is FluidTankDevice;
		BlockMesh blockMesh = m_blockMesh;
		if (flag && !flag2)
		{
			blockMesh = m_downMesh;
		}
		else if (!flag & flag2)
		{
			blockMesh = m_upMesh;
		}
		else if (flag & flag2)
		{
			blockMesh = m_straightMesh;
		}
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetAlphaTest);
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 205;
	}

	public string GetEntityName(int value)
	{
		return "FluidTank";
	}

	public Widget GetWidget(IInventory inventory, ComponentFluidTank component)
	{
		return new FluidTankWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new FluidTankDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new FluidTankTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.InputOutput, 16f);
	}

	public override bool IsFluidBlocker_(int value)
	{
		return false;
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		Color color = new Color(142, 41, 41);
		return new BlockDebrisParticleSystem(subsystemTerrain, position, 1f, 1f, color, GetFaceTextureSlot(0, value), IndustrialModLoader.IETexture);
	}
}
