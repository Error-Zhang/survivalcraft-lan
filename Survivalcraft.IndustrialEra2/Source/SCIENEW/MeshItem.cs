using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.Graphics;

namespace SCIENEW;

public class MeshItem : DefaultItem
{
	public int DefaultTextureSlot;

	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public Color Color;

	public Texture2D Texture;

	public MeshItem(string id)
		: base(id)
	{
		Id = id;
		Texture = IndustrialModLoader.BaseTexture;
	}

	public MeshItem(string id, string modelName, string meshName, Texture2D texture, Color color, Matrix boneTransform, Matrix tcTransform, float scale = 1f, float transform2 = 0f)
		: this(id)
	{
		Id = id;
		Model model = ContentManager.Get<Model>(modelName);
		Matrix matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(meshName).ParentBone) * boneTransform * Matrix.CreateTranslation(0f, -0.4f, 0f) * Matrix.CreateScale(scale) * Matrix.CreateRotationZ(transform2);
		ModelMeshPart meshPart = model.FindMesh(meshName).MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		Texture = texture;
		Color = color;
		m_standaloneBlockMesh.TransformTextureCoordinates(tcTransform);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, Texture, Color * color, size, ref matrix, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateMeshVertices(BlocksManager.Blocks[Terrain.ExtractContents(value)], x, y, z, m_standaloneBlockMesh, Color.White, null, geometry.GetGeometry(Texture).SubsetOpaque);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.85f;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public MeshItem AppendMesh(string modelName, string meshName, Matrix boneTransform, Matrix tcTransform, Color color)
	{
		m_standaloneBlockMesh.AppendMesh(modelName, meshName, boneTransform, tcTransform, color);
		return this;
	}

	public override Vector3 GetFirstPersonOffset(int value)
	{
		return new Vector3(0.5f, -0.5f, -0.6f);
	}

	public override float GetFirstPersonScale(int value)
	{
		return 0.4f;
	}
}
