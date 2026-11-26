using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class GasolineBoatItem : MeshItem
{
	public GasolineBoatItem()
		: base("GasolineBoat")
	{
		Model model = ContentManager.Get<Model>("Models/GasolineBoat");
		Matrix matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Boat").ParentBone) * Matrix.CreateTranslation(0f, -0.4f, 0f);
		ModelMeshPart meshPart = model.FindMesh("Boat").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		Matrix matrix2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Propeller").ParentBone) * Matrix.CreateTranslation(0f, -0.4f, 0f);
		ModelMeshPart meshPart2 = model.FindMesh("Propeller").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart2, matrix2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart2, matrix2, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		Texture = ContentManager.Get<Texture2D>("Textures/SteamBoat");
		Color = Color.White;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 0f, -0.3f);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.7f, 0.8f, -1f);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.45f;
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new GasolineBoatItemBehavior(project)
		};
	}
}
