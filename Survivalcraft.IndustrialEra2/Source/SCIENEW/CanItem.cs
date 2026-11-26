using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class CanItem : MeshItem
{
	public string m_category;

	public BlockMesh m_iconMesh = new BlockMesh();

	public Texture2D m_icon;

	public CanItem(string id, Texture2D icon, string category = "Items")
		: base(id)
	{
		m_category = category;
		m_icon = icon;
		Texture = IndustrialModLoader.IETexture;
		Model model = ContentManager.Get<Model>("Models/Can");
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("Can").MeshParts;
		m_standaloneBlockMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Can").ParentBone) * Matrix.CreateRotationY(MathUtils.DegToRad(180f)) * Matrix.CreateTranslation(0f, -0.3f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, new Color(220, 220, 220));
		meshParts = model.FindMesh("Icon").MeshParts;
		m_iconMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Icon").ParentBone) * Matrix.CreateRotationY(MathUtils.DegToRad(180f)) * Matrix.CreateTranslation(0f, -0.3f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, Texture, color, 2f * size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_iconMesh, m_icon, color, 2f * size, ref matrix, environmentData);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1f;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 0.05f, 0f);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(1f, 0.75f, 1f);
	}

	public override string GetCategory(int value)
	{
		return m_category;
	}
}
