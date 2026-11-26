using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class PillItem : MeshItem
{
	public string m_category;

	public BlockMesh m_iconMesh = new BlockMesh();

	public Texture2D m_icon;

	public PillItem(string id, Color color, string category = "Food")
		: base(id)
	{
		m_category = category;
		m_icon = ContentManager.Get<Texture2D>("Textures/Iron");
		Texture = ContentManager.Get<Texture2D>("Textures/Iron");
		Model model = ContentManager.Get<Model>("Models/UCake");
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("UCake").MeshParts;
		m_standaloneBlockMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("UCake").ParentBone) * Matrix.CreateRotationY(MathUtils.DegToRad(180f)) * Matrix.CreateTranslation(0f, -0.3f, 0f) * Matrix.CreateScale(0.3f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
	}

	public override bool Eat(ComponentVitalStats vitalStats, int value)
	{
		if (ItemsBlockManager.GetItemCraftingID(value) == "SleepingPill")
		{
			vitalStats.m_sleep = 0f;
			vitalStats.m_subsystemAudio.PlayRandomSound("Audio/Creatures/HumanEat", 1f, vitalStats.m_random.Float(-0.2f, 0.2f), vitalStats.m_componentPlayer.ComponentBody.Position, 2f, 0f);
			vitalStats.m_componentPlayer.PlayerStats.FoodItemsEaten++;
		}
		if (ItemsBlockManager.GetItemCraftingID(value) == "IbuprofenPill")
		{
			vitalStats.m_componentPlayer.ComponentFlu.m_fluDuration = 0f;
			vitalStats.m_componentPlayer.ComponentFlu.m_fluOnset = 0f;
			vitalStats.m_subsystemAudio.PlayRandomSound("Audio/Creatures/HumanEat", 1f, vitalStats.m_random.Float(-0.2f, 0.2f), vitalStats.m_componentPlayer.ComponentBody.Position, 2f, 0f);
			vitalStats.m_componentPlayer.PlayerStats.FoodItemsEaten++;
		}
		if (ItemsBlockManager.GetItemCraftingID(value) == "PenicillinPill")
		{
			vitalStats.m_componentPlayer.ComponentSickness.m_sicknessDuration = 0f;
			vitalStats.m_subsystemAudio.PlayRandomSound("Audio/Creatures/HumanEat", 1f, vitalStats.m_random.Float(-0.2f, 0.2f), vitalStats.m_componentPlayer.ComponentBody.Position, 2f, 0f);
			vitalStats.m_componentPlayer.PlayerStats.FoodItemsEaten++;
		}
		return false;
	}

	public override float GetNutritionalValue(int value)
	{
		return 0.0001f;
	}

	public override FoodType GetFoodType(int value)
	{
		return FoodType.None;
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
