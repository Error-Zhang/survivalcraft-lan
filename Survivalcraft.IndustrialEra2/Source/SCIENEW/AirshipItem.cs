using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class AirshipItem : MeshItem
{
	public AirshipItem()
		: base("Airship")
	{
		Model model = ContentManager.Get<Model>("Models/Airship");
		Matrix matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Airship").ParentBone) * Matrix.CreateScale(0.25f) * Matrix.CreateTranslation(0f, -0.4f, 0f);
		ModelMeshPart meshPart = model.FindMesh("Airship").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		Texture = ContentManager.Get<Texture2D>("Textures/AirShip");
		Color = Color.White;
		for (int i = 1; i <= 2; i++)
		{
			string name = $"Rotor{i}";
			matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(name).ParentBone) * Matrix.CreateScale(0.25f) * Matrix.CreateTranslation(0f, -0.4f, 0f);
			meshPart = model.FindMesh(name).MeshParts[0];
			m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		}
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0.1f, -0.48f, 0f);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.7f, 0.8f, -1f);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.6f;
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new AirShipItemBehavior(project)
		};
	}
}
