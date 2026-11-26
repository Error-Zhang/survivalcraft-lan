using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class AmmeterDevice : DefaultDevice, IRotatableDevice, IVoltWireDevice, IEntityDevice<ComponentAmmeter>
{
	public const float MinRange = 4f;

	public const float MaxRange = 64f;

	public const float DefaultRange = 16f;

	public const float MinPointerAngle = 0f;

	public const float MaxPointerAngle = (float)Math.PI / 2f;

	public BlockMesh m_shellStandaloneBlockMesh = new BlockMesh();

	public BlockMesh m_pointerStandaloneBlockMesh = new BlockMesh();

	public BlockMesh[] m_shellBlockMeshesByData = new BlockMesh[4];

	public BlockMesh m_pointerMesh = new BlockMesh();

	public BoundingBox[][] m_collisionBoxesByFace = new BoundingBox[4][];

	public static readonly Vector3 PointerPivotLocal = new Vector3(-1f / 32f, 0f, 0f);

	private Matrix m_pointerBoneTransform;

	public AmmeterDevice()
		: base("AmmeterDevice")
	{
		Model model = ContentManager.Get<Model>("Models/Ammeter");
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

	public static float CalcPointerAngle(float current, float range)
	{
		float f = MathUtils.Saturate(current / Math.Max(range, 4f));
		return MathUtils.Lerp(0f, (float)Math.PI / 2f, f);
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 4;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_shellBlockMeshesByData[GetDirection(value)], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
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
		return "Ammeter";
	}

	public Widget GetWidget(IInventory inventory, ComponentAmmeter component)
	{
		return new AmmeterWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new AmmeterDeviceBehavior(project),
			new VoltWireDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		DevicesUtils.GetSideFaceOfXZ(GetDirection(value), out var face, out var face2);
		return new HashSet<int> { face, face2 };
	}

	public float GetPL(int value)
	{
		return float.MaxValue;
	}

	public float GetUL(int value)
	{
		return float.MaxValue;
	}

	public Materials GetMaterial()
	{
		return Materials.None;
	}
}
