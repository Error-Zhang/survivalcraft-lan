using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class CoatingBucketDItem : MeshDamageableItem
{
	public int? m_colorIndex;

	public BlockMesh m_contentBlockMesh = new BlockMesh();

	public CoatingBucketDItem(int? colorIndex)
		: base(GetColoredID("CoatingBucket", colorIndex), 90)
	{
		m_colorIndex = colorIndex;
		Model model = ContentManager.Get<Model>("Models/CoatingBucket");
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("Contents").MeshParts;
		m_contentBlockMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Contents").ParentBone) * Matrix.CreateTranslation(0f, -0.25f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_contentBlockMesh.TransformTextureCoordinates((!colorIndex.HasValue) ? Matrix.CreateTranslation(0.8125f, 0.6875f, 0f) : Matrix.CreateTranslation(0.9375f, 0f, 0f));
		meshParts = model.FindMesh("Bucket").MeshParts;
		m_standaloneBlockMesh.AppendModelMeshPart(meshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Bucket").ParentBone) * Matrix.CreateTranslation(0f, -0.25f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Materials.Aluminum.GetColor());
		m_standaloneBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(0.75f, -0.5625f, 0f));
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_contentBlockMesh, m_colorIndex.HasValue ? (color * SubsystemPalette.GetColor(environmentData, m_colorIndex.Value)) : color, 2f * size, ref matrix, environmentData);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.9f;
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return SubsystemPalette.GetName(subsystemTerrain, m_colorIndex, LanguageControl.Get("DamageableBlocks", "Names", "CoatingBucket"));
	}

	public override string GetDescription(int value)
	{
		return LanguageControl.Get("DamageableBlocks", "Descriptions", "CoatingBucket");
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}

	public override int GetMaxStacking(int value)
	{
		return 1;
	}

	public int? GetColorIndex()
	{
		return m_colorIndex;
	}

	public static string GetColoredID(string id, int? colorIndex)
	{
		if (!colorIndex.HasValue)
		{
			return id;
		}
		return id + "." + colorIndex.Value;
	}

	public override DamageableItemBehavior[] GetBehaviors(Project project)
	{
		return new DamageableItemBehavior[1]
		{
			new CoatingBucketDItemBehavior(project)
		};
	}

	public override int GetDamageDestructionValue(int value)
	{
		return ItemsBlockManager.GetItemID("AluminumBucket");
	}
}
