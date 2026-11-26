using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class DiodeTubeItem : DefaultItem
{
	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public Texture2D Texture;

	public DiodeTubeItem()
		: base("DiodeTube")
	{
		Model model = ContentManager.Get<Model>("Models/DiodeTube");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Foot").ParentBone);
		ModelMeshPart meshPart = model.FindMesh("Foot").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Shell").ParentBone);
		meshPart = model.FindMesh("Shell").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, new Color(140, 192, 140, 92));
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Silicon").ParentBone);
		meshPart = model.FindMesh("Silicon").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, new Color(100, 150, 100) * Color.Gray);
		Texture = ContentManager.Get<Texture2D>("Textures/Iron");
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, Texture, color, size, ref matrix, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateMeshVertices(BlocksManager.Blocks[Terrain.ExtractContents(value)], x, y, z, m_standaloneBlockMesh, Color.White, null, geometry.GetGeometry(Texture).SubsetOpaque);
	}
}
