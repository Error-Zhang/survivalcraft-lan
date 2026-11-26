using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ConcreteFrameWithLevelDevice : DefaultDevice
{
	public BlockMesh[] m_blockMeshesByLevel = new BlockMesh[4];

	public BlockMesh m_frameBlockMesh = new BlockMesh();

	public Model m_model;

	public Texture2D m_texture;

	public string m_dropID;

	public Color m_contentColor = Color.White;

	public ConcreteFrameWithLevelDevice(string subID, string dropId, Color contentColor)
		: base(subID + "ConcreteFrameWithLevelDevice")
	{
		m_dropID = dropId;
		m_contentColor = contentColor;
		m_model = ContentManager.Get<Model>("Models/ConcreteFrame");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("ConcreteFrame").ParentBone);
		m_frameBlockMesh.AppendModelMeshPart(m_model.FindMesh("ConcreteFrame").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		for (int i = 0; i < 4; i++)
		{
			Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh($"ConcreteFrame_L{i + 1}").ParentBone);
			m_blockMeshesByLevel[i] = new BlockMesh();
			m_blockMeshesByLevel[i].AppendModelMeshPart(m_model.FindMesh($"ConcreteFrame_L{i + 1}").MeshParts[0], boneAbsoluteTransform2 * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
		m_texture = IndustrialModLoader.IETexture;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_frameBlockMesh, GetIsWet(value) ? Color.LightGray : Color.White, null, null, geometry.GetGeometry(m_texture).SubsetOpaque);
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMeshesByLevel[GetLevel(value)], m_contentColor, null, null, geometry.GetGeometry(m_texture).SubsetOpaque);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_frameBlockMesh, m_texture, GetIsWet(value) ? (Color.LightGray * color) : (Color.White * color), size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_blockMeshesByLevel[GetLevel(value)], m_texture, m_contentColor * color, size, ref matrix, environmentData);
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Wood";
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 186;
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) - 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[3]
		{
			new ConcreteFrameDevicePlacingBehavior(project),
			new ConcreteFrameDeviceFillingBehavior(project),
			new ConcreteFrameDeviceBindingBehavior(project)
		};
	}

	public static int GetLevel(int value)
	{
		return DevicesUtils.GetInnerData(value) & 3;
	}

	public static bool GetIsWet(int value)
	{
		return (DevicesUtils.GetInnerData(value) & 4) >> 2 == 1;
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		dropValues.Add(new BlockDropValue
		{
			Value = DevicesBlockManager.GetDeviceValue("ConcreteFrameDevice", 0),
			Count = 1
		});
		for (int i = 0; i <= GetLevel(oldValue); i++)
		{
			dropValues.Add(new BlockDropValue
			{
				Value = ItemsBlockManager.GetItemID(m_dropID),
				Count = 1
			});
		}
	}

	public override bool IsDisplay()
	{
		return false;
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}
}
