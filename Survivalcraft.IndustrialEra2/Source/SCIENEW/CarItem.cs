using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CarItem : MeshItem
{
	public CarItem()
		: base("Car")
	{
		Model model = ContentManager.Get<Model>("Models/Car");
		Matrix matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Chasis").ParentBone) * Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(0f, -0.4f, 0f);
		ModelMeshPart meshPart = model.FindMesh("Chasis").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		Texture = ContentManager.Get<Texture2D>("Textures/Car");
		Color = Color.White;
		for (int i = 1; i <= 4; i++)
		{
			string name = $"Wheel{i}";
			matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(name).ParentBone) * Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(0f, -0.4f, 0f);
			meshPart = model.FindMesh(name).MeshParts[0];
			m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		}
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, -0.24f, 0f);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.7f, 0.8f, -1f);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.8f;
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new CarItemBehavior(project)
		};
	}
}
