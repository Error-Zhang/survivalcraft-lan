using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class FuelRodDItem : MeshDamageableItem
{
	public enum RodType
	{
		UFuelRod,
		PFuelRod,
		ControlRod,
		CarbonRod
	}

	public RodType m_type;

	public FuelRodDItem(string id, int duability, RodType rodType, Color color)
		: base(id, duability)
	{
		m_type = rodType;
		Model model = ContentManager.Get<Model>("Models/FuelRod");
		Matrix matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Shell").ParentBone) * Matrix.CreateTranslation(0f, 0f, 0f) * Matrix.CreateScale(1f);
		ModelMeshPart meshPart = model.FindMesh("Shell").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		Texture = ContentManager.Get<Texture2D>("Textures/Iron");
		Color = Color.White;
		matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Rod").ParentBone) * Matrix.CreateTranslation(0f, 0f, 0f) * Matrix.CreateScale(1f);
		meshPart = model.FindMesh("Rod").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, color);
	}
}
