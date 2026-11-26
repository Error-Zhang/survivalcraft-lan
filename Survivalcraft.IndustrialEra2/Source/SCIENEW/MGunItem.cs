using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MGunItem : MeshItem
{
	public MGunItem()
		: base("MGun")
	{
		Model model = ContentManager.Get<Model>("Models/MGun");
		Matrix matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Tripode").ParentBone) * Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(0f, -0.4f, 0f);
		ModelMeshPart meshPart = model.FindMesh("Tripode").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		Texture = ContentManager.Get<Texture2D>("Textures/MGun");
		Color = Color.White;
		matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Turret").ParentBone) * Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(0f, -0.4f, 0f);
		meshPart = model.FindMesh("Turret").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, -0.15f, 0.42f);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.7f, 0.8f, -1f);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1.25f;
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new MGunItemBehavior(project)
		};
	}

	public override string GetCategory(int value)
	{
		return "Weapons";
	}
}
