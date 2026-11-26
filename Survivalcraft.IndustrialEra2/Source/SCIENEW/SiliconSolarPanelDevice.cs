using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class SiliconSolarPanelDevice : DefaultDevice, IVoltDevice
{
	private BlockMesh m_panelMesh = new BlockMesh();

	private BlockMesh m_panelStandaloneBlockMesh = new BlockMesh();

	private BlockMesh m_faceMesh = new BlockMesh();

	private BlockMesh m_faceStandaloneBlockMesh = new BlockMesh();

	private Texture2D m_panelTexture;

	private Texture2D m_faceDefaultTexture;

	public BoundingBox m_boundingBox;

	public SiliconSolarPanelDevice()
		: base("SiliconSolarPanelDevice")
	{
		Model model = ContentManager.Get<Model>("Models/SolarPanel");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("SolarPanel").ParentBone);
		m_panelMesh.AppendModelMeshPart(model.FindMesh("SolarPanel").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_panelStandaloneBlockMesh.AppendModelMeshPart(model.FindMesh("SolarPanel").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Face").ParentBone);
		m_faceMesh.AppendModelMeshPart(model.FindMesh("Face").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_faceStandaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Face").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_boundingBox = m_panelMesh.CalculateBoundingBox();
		m_panelTexture = ContentManager.Get<Texture2D>("Textures/AdvancedShell");
		m_faceDefaultTexture = SolarPanelTextureManager.m_facialDataToSlot_S[new SolarPanelTextureManager.PanelFacialData(new bool[4] { true, true, true, true }, new bool[4] { true, true, true, true })];
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_panelStandaloneBlockMesh, m_panelTexture, color, size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_faceStandaloneBlockMesh, m_faceDefaultTexture, color, size, ref matrix, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		SolarPanelTextureManager.PanelFacialData facialData = GetFacialData(new Point3(x, y, z), generator.SubsystemTerrain);
		Texture2D valueOrDefault = SolarPanelTextureManager.m_facialDataToSlot_S.GetValueOrDefault(facialData, m_faceDefaultTexture);
		generator.GenerateMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_panelMesh, Color.White, null, geometry.GetGeometry(m_panelTexture).SubsetOpaque);
		generator.GenerateMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_faceMesh, Color.White, null, geometry.GetGeometry(valueOrDefault).SubsetOpaque);
	}

	public SolarPanelTextureManager.PanelFacialData GetFacialData(Point3 point, SubsystemTerrain terrain)
	{
		bool[] array = new bool[4] { true, true, true, true };
		bool[] array2 = new bool[4] { true, true, true, true };
		Point3[] array3 = new Point3[4]
		{
			-Point3.UnitZ,
			Point3.UnitX,
			Point3.UnitZ,
			-Point3.UnitX
		};
		Point3[] array4 = new Point3[4]
		{
			new Point3(-1, 0, -1),
			new Point3(1, 0, -1),
			new Point3(1, 0, 1),
			new Point3(-1, 0, 1)
		};
		for (int i = 0; i < 4; i++)
		{
			Point3 position = point + array3[i];
			Point3 position2 = point + array3[(i + 3) % 4];
			array2[i] = CheckBlock(terrain, position);
			Point3 position3 = point + array4[i];
			array[i] = CheckBlock(terrain, position3) || CheckBlock(terrain, position) || CheckBlock(terrain, position2);
		}
		return new SolarPanelTextureManager.PanelFacialData(array, array2);
	}

	private bool CheckBlock(SubsystemTerrain terrain, Point3 position)
	{
		int cellValueFast = terrain.Terrain.GetCellValueFast(position.X, position.Y, position.Z);
		if (Terrain.ExtractContents(cellValueFast) != BaseDeviceBlock.Index)
		{
			return true;
		}
		if (!(DevicesBlockManager.GetDevice(cellValueFast) is SiliconSolarPanelDevice))
		{
			return true;
		}
		return false;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return new BoundingBox[1] { m_boundingBox };
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override int GetShadowStrength(int value)
	{
		return 10;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 112;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 15f / 32f, 0f);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new SolarPanelDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new PolySiliconSolarPanelVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 0.8f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}
}
