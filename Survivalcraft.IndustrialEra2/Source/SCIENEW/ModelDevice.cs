using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public abstract class ModelDevice : DefaultDevice
{
	public Model m_model;

	public BlockMesh m_blockMesh = new BlockMesh();

	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public Texture2D m_texture;

	public BoundingBox m_boundingBox;

	public ModelDevice(string id, string modelPath, string meshName, Texture2D texture, Color color, bool makeEmissive = false, bool flipWindingOrder = false, bool doubleSided = false, bool flipNormals = false)
		: base(id)
	{
		m_model = ContentManager.Get<Model>(modelPath);
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh(meshName).ParentBone);
		m_blockMesh.AppendModelMeshPart(m_model.FindMesh(meshName).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive, flipWindingOrder, doubleSided, flipNormals, color);
		m_standaloneBlockMesh.AppendModelMeshPart(m_model.FindMesh(meshName).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive, flipWindingOrder, doubleSided, flipNormals, color);
		m_boundingBox = m_blockMesh.CalculateBoundingBox();
		m_texture = texture;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetOpaque);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_texture, color, size, ref matrix, environmentData);
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return new BoundingBox[1] { m_boundingBox };
	}
}
