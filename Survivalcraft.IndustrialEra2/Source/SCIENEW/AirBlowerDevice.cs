using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class AirBlowerDevice : DefaultDevice, IEntityDevice<ComponentAirBlower>
{
	public BlockMesh m_blockMesh = new BlockMesh();

	public BlockMesh m_blockMesh_Working = new BlockMesh();

	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public AirBlowerDevice()
		: base("AirBlowerDevice")
	{
		Model model = ContentManager.Get<Model>("Models/AirBlower");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("AirBlower_Working").ParentBone);
		Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("AirBlower_Working").ParentBone);
		m_blockMesh.AppendModelMeshPart(model.FindMesh("AirBlower").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_blockMesh_Working.AppendModelMeshPart(model.FindMesh("AirBlower_Working").MeshParts[0], boneAbsoluteTransform2 * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("AirBlower_Working").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, IsWorking(value) ? m_blockMesh_Working : m_blockMesh, Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetOpaque);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
	}

	public bool IsWorking(int value)
	{
		return DevicesUtils.GetInnerData(value) >> 2 == 1;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 107;
	}

	public string GetEntityName(int value)
	{
		return "AirBlower";
	}

	public Widget GetWidget(IInventory inventory, ComponentAirBlower component)
	{
		return null;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new AirBlowerDeviceBehavior(project)
		};
	}
}
