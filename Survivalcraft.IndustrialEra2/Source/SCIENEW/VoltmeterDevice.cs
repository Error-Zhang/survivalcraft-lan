using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class VoltmeterDevice : DefaultDevice, IRotatableDevice, IVoltDevice, IEntityDevice<ComponentVoltmeter>
{
	public const float MinRange = 1f;

	public const float MaxRange = 4f;

	public const float DefaultRange = 1f;

	public const float MinPointerAngle = 0f;

	public const float MaxPointerAngle = (float)Math.PI / 2f;

	public BlockMesh m_shellStandaloneBlockMesh = new BlockMesh();

	public BlockMesh m_pointerStandaloneBlockMesh = new BlockMesh();

	public BlockMesh[] m_shellBlockMeshesByData = new BlockMesh[4];

	public BlockMesh m_pointerMesh = new BlockMesh();

	public BoundingBox[][] m_collisionBoxesByFace = new BoundingBox[4][];

	public static readonly Vector3 PointerPivotLocal = new Vector3(-1f / 32f, 0f, 0f);

	private Matrix m_pointerBoneTransform;

	public VoltmeterDevice()
		: base("VoltmeterDevice")
	{
		Model model = ContentManager.Get<Model>("Models/Voltmeter");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Shell").ParentBone);
		m_pointerBoneTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Pointer").ParentBone);
		m_pointerMesh.AppendModelMeshPart(model.FindMesh("Pointer").MeshParts[0], Matrix.Identity, makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		for (int i = 0; i < 4; i++)
		{
			m_shellBlockMeshesByData[i] = new BlockMesh();
			Matrix identity = Matrix.Identity;
			identity *= Matrix.CreateRotationY((float)i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
			m_shellBlockMeshesByData[i].AppendModelMeshPart(model.FindMesh("Shell").MeshParts[0], boneAbsoluteTransform * identity, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_collisionBoxesByFace[i] = new BoundingBox[1] { m_shellBlockMeshesByData[i].CalculateBoundingBox() };
		}
		m_shellStandaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Shell").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_pointerStandaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Pointer").MeshParts[0], m_pointerBoneTransform * Matrix.CreateTranslation(0f, 0f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
	}

	public Matrix CalcPointerMatrix(Point3 position, int direction, float pointerAngle)
	{
		return Matrix.CreateTranslation(-PointerPivotLocal) * Matrix.CreateRotationZ(pointerAngle) * Matrix.CreateTranslation(PointerPivotLocal) * m_pointerBoneTransform * Matrix.CreateRotationY((float)direction * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f) * Matrix.CreateTranslation(position.X, position.Y, position.Z);
	}

	public static float CalcPointerAngle(float voltage, float range)
	{
		float f = MathUtils.Saturate(voltage / Math.Max(range, 1f));
		return MathUtils.Lerp(0f, (float)Math.PI / 2f, f);
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_shellBlockMeshesByData[GetDirection(value)], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 4;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_shellStandaloneBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_pointerStandaloneBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXZ(componentMiner));
		return result;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 105;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_collisionBoxesByFace[GetDirection(value)];
	}

	public int GetNextDirection(int value, bool reverse = false)
	{
		return DevicesBlockManager.GetDeviceValue(Id, (GetDirection_Static(value) + 1) % 4);
	}

	protected virtual int GetDirection(int value)
	{
		return GetDirection_Static(value);
	}

	public static int GetDirection_Static(int value)
	{
		return DevicesUtils.GetInnerData(value) & 3;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
	{
		return true;
	}

	public string GetEntityName(int value)
	{
		return "Voltmeter";
	}

	public Widget GetWidget(IInventory inventory, ComponentVoltmeter component)
	{
		return new VoltmeterWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new VoltmeterDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new VoltmeterVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 0f;
	}

	public float GetStandardUL(int value)
	{
		return 0f;
	}
}
