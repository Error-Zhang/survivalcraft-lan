using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class AluminumBucketItem : MeshItem
{
	public string m_category;

	public AluminumBucketItem(string id, Color contentColor, Matrix contentTc, string category = "Items")
		: base(id)
	{
		m_category = category;
		Model model = ContentManager.Get<Model>("Models/FullBucket");
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("Contents").MeshParts;
		m_standaloneBlockMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Contents").ParentBone) * Matrix.CreateRotationY(MathUtils.DegToRad(180f)) * Matrix.CreateTranslation(0f, -0.3f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, contentColor);
		m_standaloneBlockMesh.TransformTextureCoordinates(contentTc);
		meshParts = model.FindMesh("Bucket").MeshParts;
		BlockMesh blockMesh = new BlockMesh();
		blockMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Bucket").ParentBone) * Matrix.CreateRotationY(MathUtils.DegToRad(180f)) * Matrix.CreateTranslation(0f, -0.3f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Materials.Aluminum.GetColor());
		blockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(0.75f, -0.5625f, 0f));
		m_standaloneBlockMesh.AppendBlockMesh(blockMesh);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1f;
	}

	public override string GetCategory(int value)
	{
		return m_category;
	}

	public override int GetMaxStacking(int value)
	{
		return 1;
	}
}
