using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class ElectricAirBlowerDevice : DefaultDevice, IEntityDevice<ComponentElectricAirBlower>, IVoltDevice
{
	public Model m_model;

	public BlockMesh m_blockMesh = new BlockMesh();

	public BlockMesh m_blockMesh_Working = new BlockMesh();

	public BlockMesh m_fanMesh = new BlockMesh();

	public Texture2D m_texture;

	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public ElectricAirBlowerDevice()
		: base("ElectricAirBlowerDevice")
	{
		m_model = ContentManager.Get<Model>("Models/ElectricAirBlower");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("AirBlower").ParentBone);
		Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("AirBlower_Working").ParentBone);
		Matrix boneAbsoluteTransform3 = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Fan").ParentBone);
		m_blockMesh.AppendModelMeshPart(m_model.FindMesh("AirBlower").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_fanMesh.AppendModelMeshPart(m_model.FindMesh("Fan").MeshParts[0], boneAbsoluteTransform3 * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_blockMesh_Working.AppendModelMeshPart(m_model.FindMesh("AirBlower_Working").MeshParts[0], boneAbsoluteTransform2 * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(m_model.FindMesh("AirBlower_Working").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(m_model.FindMesh("Fan").MeshParts[0], boneAbsoluteTransform3 * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_texture = IndustrialModLoader.IETexture;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, IsWorking(value) ? m_blockMesh_Working : m_blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetOpaque);
		if (!IsWorking(value))
		{
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_fanMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetAlphaTest);
		}
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_texture, color, size, ref matrix, environmentData);
	}

	public bool IsWorking(int value)
	{
		return DevicesUtils.GetInnerData(value) == 1;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 107;
	}

	public string GetEntityName(int value)
	{
		return "ElectricAirBlower";
	}

	public Widget GetWidget(IInventory inventory, ComponentElectricAirBlower component)
	{
		return null;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new ElectricAirBlowerDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new ElectricAirBlowerVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 3f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}
}
