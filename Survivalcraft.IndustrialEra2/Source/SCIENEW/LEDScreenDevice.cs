using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class LEDScreenDevice : DefaultDevice, IEntityDevice<ComponentLEDScreen>, IVoltDevice
{
	private readonly BlockMesh[] m_shellMeshesByFace = new BlockMesh[6];

	private readonly BlockMesh[] m_faceMeshesByFace = new BlockMesh[6];

	private readonly BlockMesh m_shellStandaloneBlockMesh = new BlockMesh();

	private readonly BlockMesh m_faceStandaloneBlockMesh = new BlockMesh();

	private readonly BoundingBox[][] m_collisionBoxesByFace = new BoundingBox[6][];

	private readonly Texture2D m_shellTexture;

	private readonly Texture2D m_faceDefaultTexture;

	public LEDScreenDevice()
		: base("LEDScreenDevice")
	{
		Model model = ContentManager.Get<Model>("Models/LEDScreen");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("ASMLed").ParentBone);
		Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Face").ParentBone);
		for (int i = 0; i < 6; i++)
		{
			Matrix matrix = CreateFaceMatrix(i);
			m_shellMeshesByFace[i] = new BlockMesh();
			m_shellMeshesByFace[i].AppendModelMeshPart(model.FindMesh("ASMLed").MeshParts[0], boneAbsoluteTransform * matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_faceMeshesByFace[i] = new BlockMesh();
			m_faceMeshesByFace[i].AppendModelMeshPart(model.FindMesh("Face").MeshParts[0], boneAbsoluteTransform2 * matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_collisionBoxesByFace[i] = new BoundingBox[1] { m_shellMeshesByFace[i].CalculateBoundingBox() };
		}
		Matrix matrix2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f) * Matrix.CreateRotationY((float)Math.PI / 2f);
		m_shellStandaloneBlockMesh.AppendModelMeshPart(model.FindMesh("ASMLed").MeshParts[0], boneAbsoluteTransform * matrix2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_faceStandaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Face").MeshParts[0], boneAbsoluteTransform * matrix2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_shellTexture = ContentManager.Get<Texture2D>("Textures/LEDScreenShell");
		m_faceDefaultTexture = ContentManager.Get<Texture2D>("Textures/LEDScreenFaceDefault");
	}

	private static Matrix CreateFaceMatrix(int face)
	{
		if (face >= 4)
		{
			if (face != 4)
			{
				return Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f);
			}
			return Matrix.CreateTranslation(0.5f, 0f, 0.5f);
		}
		return Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY((float)face * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
	}

	public static int GetDirection_Static(int value)
	{
		return DevicesUtils.GetInnerData(value);
	}

	protected virtual int GetDirection(int value)
	{
		return GetDirection_Static(value);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_shellStandaloneBlockMesh, m_shellTexture, color, 1.25f * size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_faceStandaloneBlockMesh, m_faceDefaultTexture, color, 1.25f * size, ref matrix, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		int direction = GetDirection(value);
		LEDScreenTextureManager.LEDFacialData facialData = GetFacialData(direction, new Point3(x, y, z), generator.SubsystemTerrain);
		Texture2D valueOrDefault = LEDScreenTextureManager.m_facialDataToSlot.GetValueOrDefault(facialData, m_faceDefaultTexture);
		generator.GenerateMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_shellMeshesByFace[direction], Color.White, null, geometry.GetGeometry(m_shellTexture).SubsetOpaque);
		generator.GenerateMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_faceMeshesByFace[direction], Color.White, null, geometry.GetGeometry(valueOrDefault).SubsetOpaque);
	}

	public LEDScreenTextureManager.LEDFacialData GetFacialData(int face, Point3 point, SubsystemTerrain terrain)
	{
		bool[] array = new bool[4] { true, true, true, true };
		bool[] array2 = new bool[4] { true, true, true, true };
		DevicesUtils.FaceToAxesAndConner(face, out Point3[] axes, out Point3[] conners);
		for (int i = 0; i < 4; i++)
		{
			Point3 neighborPosition = point + axes[i];
			Point3 neighborPosition2 = point + axes[(i + 3) % 4];
			array2[i] = CheckBlock(terrain, point, neighborPosition);
			Point3 neighborPosition3 = point + conners[i];
			array[i] = CheckBlock(terrain, point, neighborPosition3) || CheckBlock(terrain, point, neighborPosition) || CheckBlock(terrain, point, neighborPosition2);
		}
		return new LEDScreenTextureManager.LEDFacialData(array, array2);
	}

	private bool CheckBlock(SubsystemTerrain terrain, Point3 position, Point3 neighborPosition)
	{
		int cellValueFast = terrain.Terrain.GetCellValueFast(neighborPosition.X, neighborPosition.Y, neighborPosition.Z);
		if (Terrain.ExtractContents(cellValueFast) != BaseDeviceBlock.Index)
		{
			return true;
		}
		if (!(DevicesBlockManager.GetDevice(cellValueFast) is LEDScreenDevice))
		{
			return true;
		}
		int cellValueFast2 = terrain.Terrain.GetCellValueFast(position.X, position.Y, position.Z);
		return GetDirection(cellValueFast2) != GetDirection(cellValueFast);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, raycastResult.CellFace.Face);
		return result;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_collisionBoxesByFace[GetDirection(value)];
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "LEDScreenGroup";
	}

	public Widget GetWidget(IInventory inventory, ComponentLEDScreen component)
	{
		throw new NotImplementedException();
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new LEDScreenDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new LEDScreenVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 2f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { CellFace.OppositeFace(GetDirection(value)) };
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
		return 99;
	}
}
