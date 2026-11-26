using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SteamTurbineDevice : IlluminableModelDevice, IEntityDevice<ComponentSteamTurbine>, IFluidTransferrerDevice
{
	public BlockMesh m_fanMesh = new BlockMesh();

	public BlockMesh m_displayFanMesh = new BlockMesh();

	public Model m_model;

	public SteamTurbineDevice()
		: base("SteamTurbineDevice", "Models/SteamTurbine", "SteamTurbine", "SteamTurbine_Working")
	{
		m_model = ContentManager.Get<Model>("Models/SteamTurbine");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Fan").ParentBone);
		m_fanMesh.AppendModelMeshPart(m_model.FindMesh("Fan").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_displayFanMesh.AppendModelMeshPart(m_model.FindMesh("Fan").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_displayFanMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, IlluminableModelDevice.IsLight(value) ? m_litBlockMeshesByData[GetDirection(value)] : m_blockMeshesByData[GetDirection(value)], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		if (!IlluminableModelDevice.IsLight(value))
		{
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_fanMesh, Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		}
	}

	public string GetEntityName(int value)
	{
		return "SteamTurbine";
	}

	public Widget GetWidget(IInventory inventory, ComponentSteamTurbine component)
	{
		return new SteamTurbineWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new SteamTurbineDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new SteamTurbineTransferrer(point, subsystemTerrain);
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
}
