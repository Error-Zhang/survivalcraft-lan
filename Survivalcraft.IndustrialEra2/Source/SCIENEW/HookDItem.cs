using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class HookDItem : ElectricToolDItem
{
	public BlockMesh m_headBlockMesh = new BlockMesh();

	public HookDItem()
		: base("Hook", 1000)
	{
		Texture = ContentManager.Get<Texture2D>("Textures/MiningDrill");
		Color = Color.White;
		Model model = ContentManager.Get<Model>("Models/Hook");
		Matrix matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Hook").ParentBone) * Matrix.CreateTranslation(0f, 0f, 0f) * Matrix.CreateScale(1f) * Matrix.CreateRotationZ(0f);
		Matrix matrix2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Head").ParentBone) * Matrix.CreateTranslation(0f, 0f, 0f) * Matrix.CreateScale(1f) * Matrix.CreateRotationZ(0f);
		ModelMeshPart meshPart = model.FindMesh("Hook").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		ModelMeshPart meshPart2 = model.FindMesh("Head").MeshParts[0];
		m_headBlockMesh.AppendModelMeshPart(meshPart2, matrix2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_headBlockMesh.AppendModelMeshPart(meshPart2, matrix2, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, Texture, Color * color, size, ref matrix, environmentData);
		DrawBlockMode drawBlockMode = environmentData.DrawBlockMode;
		if ((drawBlockMode == DrawBlockMode.World || drawBlockMode == DrawBlockMode.UI) ? true : false)
		{
			DrawHook(primitivesRenderer, color, size, ref matrix, environmentData);
		}
	}

	public void DrawHook(PrimitivesRenderer3D primitivesRenderer, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_headBlockMesh, Texture, Color * color, size, ref matrix, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.7f;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override Vector3 GetFirstPersonOffset(int value)
	{
		return new Vector3(0.5f, -0.42000002f, -0.6f);
	}

	public override float GetFirstPersonScale(int value)
	{
		return 0.4f;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.35f, 0.22999999f, 0f);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-1f, 0.5f, -1f);
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}

	public override DamageableItemBehavior[] GetBehaviors(Project project)
	{
		return new DamageableItemBehavior[2]
		{
			new HookDItemBehavior(project),
			new ChargeByBatteryDItemBehavior(project)
		};
	}
}
