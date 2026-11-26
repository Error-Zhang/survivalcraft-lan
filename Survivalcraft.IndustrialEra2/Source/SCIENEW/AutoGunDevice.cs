using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class AutoGunDevice : SixDirectionModelDevice, IEntityDevice<ComponentAutoGun>, IVoltDevice, IElectricElementDevice
{
	public BlockMesh m_cameraMesh = new BlockMesh();

	public AutoGunDevice()
		: base("AutoGunDevice", "Models/AutoGun", "Bottom", faceTransparent: true)
	{
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("AutoGun").ParentBone);
		m_cameraMesh.AppendModelMeshPart(m_model.FindMesh("AutoGun").MeshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, raycastResult.CellFace.Face);
		return result;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
		generator.GenerateWireVertices(value, x, y, z, GetDirection(value), 0.8f, Vector2.Zero, geometry.SubsetOpaque);
		WireDrawingManager.GenerateWireVertices(generator, geometry, value, x, y, z, Materials.None);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		base.DrawBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
		Matrix matrix2 = ((environmentData.DrawBlockMode == DrawBlockMode.UI) ? (Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationY((float)Math.PI) * matrix) : (Matrix.CreateTranslation(0f, -0.15f, 0f) * Matrix.CreateRotationY((float)Math.PI) * matrix));
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_cameraMesh, IndustrialModLoader.IETexture, color, size, ref matrix2, environmentData);
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return Block.m_defaultCollisionBoxes;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 99;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new AutoGunDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new AutoGunElectricElement(subsystemElectricity, new CellFace(x, y, z, GetDirection(value)));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		if (GetDirection(value) == face)
		{
			return ElectricConnectorType.Output;
		}
		return null;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new AutoGunVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 4f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public string GetEntityName(int value)
	{
		return "AutoGun";
	}

	public Widget GetWidget(IInventory inventory, ComponentAutoGun component)
	{
		return new AutoGunWidget(inventory, component);
	}
}
