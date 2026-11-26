using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class BucketItem : MeshItem
{
	public string m_category;

	public BucketItem(string id, Color contentColor, string category = "Items")
		: base(id)
	{
		m_category = category;
		Model model = ContentManager.Get<Model>("Models/FullBucket");
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("Contents").MeshParts;
		m_standaloneBlockMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Contents").ParentBone) * Matrix.CreateRotationY(MathUtils.DegToRad(180f)) * Matrix.CreateTranslation(0f, -0.3f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, contentColor);
		m_standaloneBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(0.8125f, 0.6875f, 0f));
		meshParts = model.FindMesh("Bucket").MeshParts;
		m_standaloneBlockMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Bucket").ParentBone) * Matrix.CreateRotationY(MathUtils.DegToRad(180f)) * Matrix.CreateTranslation(0f, -0.3f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
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
}
