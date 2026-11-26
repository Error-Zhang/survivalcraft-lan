using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MiningDrillDItem : ElectricToolDItem
{
	public BlockMesh m_headBlockMesh = new BlockMesh();

	public MiningDrillDItemBehavior.GetPointsFunc PointsGetter;

	public string m_suffix;

	public string m_baseID;

	public bool m_isDisplay;

	public MiningDrillDItem(string suffix, MiningDrillDItemBehavior.GetPointsFunc pointsGetter, bool display)
		: base("MiningDrill" + suffix, 2000)
	{
		Texture = ContentManager.Get<Texture2D>("Textures/MiningDrill");
		Color = Color.White;
		m_suffix = suffix;
		m_isDisplay = display;
		m_baseID = "MiningDrill";
		Model model = ContentManager.Get<Model>("Models/MiningDrill");
		Matrix matrix = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("MiningDrill").ParentBone) * Matrix.CreateTranslation(0f, 0f, 0f) * Matrix.CreateScale(1f) * Matrix.CreateRotationZ(0f);
		Matrix matrix2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Head").ParentBone) * Matrix.CreateTranslation(0f, 0f, 0f) * Matrix.CreateScale(1f) * Matrix.CreateRotationZ(0f);
		ModelMeshPart meshPart = model.FindMesh("MiningDrill").MeshParts[0];
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(meshPart, matrix, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		ModelMeshPart meshPart2 = model.FindMesh("Head").MeshParts[0];
		m_headBlockMesh.AppendModelMeshPart(meshPart2, matrix2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_headBlockMesh.AppendModelMeshPart(meshPart2, matrix2, makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, Color.White);
		PointsGetter = pointsGetter;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, Texture, Color * color, size, ref matrix, environmentData);
		Matrix matrix2 = Matrix.CreateRotationZ((DamageableItemManager.GetDamage(value) < DamageableItemManager.GetItem(value).GetDurability(value)) ? ((float)(Time.RealTime * 16.0 % (Math.PI * 2.0))) : 0f) * matrix;
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_headBlockMesh, Texture, Color * color, size, ref matrix2, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return LanguageControl.Get("DamageableBlocks", "Names", "MiningDrill");
	}

	public override string GetDescription(int value)
	{
		return LanguageControl.Get("DamageableBlocks", "Descriptions", "MiningDrill");
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
		return new Vector3(0.5f, -0.62f, -0.6f);
	}

	public override float GetFirstPersonScale(int value)
	{
		return 0.4f;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.85f, 0.22999999f, 0f);
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
			new MiningDrillDItemBehavior(project, PointsGetter),
			new ChargeByBatteryDItemBehavior(project)
		};
	}

	public override float GetQuarryPower(int value)
	{
		return 18f;
	}

	public override bool IsDisplay()
	{
		return m_isDisplay;
	}
}
