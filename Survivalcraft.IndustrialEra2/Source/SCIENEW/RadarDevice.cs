using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class RadarDevice : DefaultDevice, IEntityDevice<ComponentRadar>, IVoltDevice, IElectricElementDevice
{
	public Model m_model;

	public BlockMesh m_bottomMesh = new BlockMesh();

	public BlockMesh m_rotorMesh = new BlockMesh();

	public BlockMesh m_displayMesh = new BlockMesh();

	public Texture2D m_texture;

	public RadarDevice()
		: base("RadarDevice")
	{
		m_model = ContentManager.Get<Model>("Models/Radar");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("RadarBottom").ParentBone);
		m_bottomMesh.AppendModelMeshPart(m_model.FindMesh("RadarBottom").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_displayMesh.AppendModelMeshPart(m_model.FindMesh("RadarBottom").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("RadarRotor").ParentBone);
		m_rotorMesh.AppendModelMeshPart(m_model.FindMesh("RadarRotor").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_displayMesh.AppendModelMeshPart(m_model.FindMesh("RadarRotor").MeshParts[0], boneAbsoluteTransform * Matrix.CreateRotationY((float)Math.PI / 4f) * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_texture = IndustrialModLoader.IETexture;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 99;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public string GetEntityName(int value)
	{
		return "Radar";
	}

	public Widget GetWidget(IInventory inventory, ComponentRadar component)
	{
		return new RadarWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new RadarDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public bool IsRotating(int value)
	{
		return DevicesUtils.GetInnerData(value) > 0;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_bottomMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetAlphaTest);
		generator.GenerateWireVertices(value, x, y, z, 4, 0.8f, Vector2.Zero, geometry.SubsetOpaque);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_displayMesh, m_texture, color, size, ref matrix, environmentData);
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new RadarVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 4f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new RadarElectricElement(subsystemElectricity, new CellFace(x, y, z, 4));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		if (face != 4)
		{
			return null;
		}
		return ElectricConnectorType.Output;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}
}
