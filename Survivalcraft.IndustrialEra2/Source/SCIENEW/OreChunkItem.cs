using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class OreChunkItem : MeshItem, IMaterialBlock
{
	protected new readonly BlockMesh m_standaloneBlockMesh = new BlockMesh();

	protected Materials material;

	public float GetMoltedVolume()
	{
		return 0f;
	}

	public Materials GetMaterial()
	{
		return material;
	}

	public OreChunkItem(string id, Matrix transform, Matrix tcTransform, Color color, bool smooth)
		: base(id)
	{
		material = Materials.None;
		Color = color;
		Model model = ContentManager.Get<Model>(smooth ? "Models/ChunkSmooth" : "Models/Chunk");
		m_standaloneBlockMesh.AppendModelMeshPart(model.Meshes[0].MeshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.Meshes[0].ParentBone) * transform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
		m_standaloneBlockMesh.TransformTextureCoordinates(tcTransform);
	}

	public OreChunkItem(Matrix transform, Matrix tcTransform, Color color, bool smooth, Materials type)
		: base(type.ToString() + "OreChunk")
	{
		type.ToDisplayName();
		material = type;
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "OreChunkItem"), material.ToDisplayName());
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "OreChunkItem"), material.ToDisplayName());
		Color = color;
		Model model = ContentManager.Get<Model>(smooth ? "Models/ChunkSmooth" : "Models/Chunk");
		m_standaloneBlockMesh.AppendModelMeshPart(model.Meshes[0].MeshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.Meshes[0].ParentBone) * transform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
		m_standaloneBlockMesh.TransformTextureCoordinates(tcTransform);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, Color * color, 2f * size, ref matrix, environmentData);
	}

	public override string GetCraftingId()
	{
		return Id;
	}

	public override string GetCategory(int value)
	{
		return "Minerals";
	}

	public override float GetProjectilePower(int value)
	{
		return 2f;
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1f;
	}
}
