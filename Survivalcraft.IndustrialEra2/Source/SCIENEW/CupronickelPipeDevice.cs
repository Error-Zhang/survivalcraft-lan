using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CupronickelPipeDevice : DefaultDevice, IFluidTransferrerPipeDevice, IPaintableBlock
{
	public BlockMesh m_centerMesh = new BlockMesh();

	public BlockMesh[] m_aroundMeshes = new BlockMesh[6];

	public BlockMesh[] m_edgeMeshes = new BlockMesh[6];

	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public string m_pureID;

	public int? m_colorIndex;

	public CupronickelPipeDevice(int? colorIndex = null)
		: base(GetColoredCraftingID("CupronickelPipeDevice", colorIndex))
	{
		Model model = ContentManager.Get<Model>("Models/Pipe");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Pipe_Center").ParentBone);
		m_centerMesh.AppendModelMeshPart(model.FindMesh("Pipe_Center").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		for (int i = 0; i < 6; i++)
		{
			m_aroundMeshes[i] = new BlockMesh();
			boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Pipe_Normal_" + i).ParentBone);
			m_aroundMeshes[i].AppendModelMeshPart(model.FindMesh("Pipe_Normal_" + i).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_edgeMeshes[i] = new BlockMesh();
			boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Pipe_Edge_" + i).ParentBone);
			m_edgeMeshes[i].AppendModelMeshPart(model.FindMesh("Pipe_Edge_" + i).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
		m_standaloneBlockMesh.AppendBlockMesh(m_centerMesh);
		m_standaloneBlockMesh.AppendBlockMesh(m_edgeMeshes[1]);
		m_standaloneBlockMesh.AppendBlockMesh(m_edgeMeshes[3]);
		m_standaloneBlockMesh.TransformPositions(Matrix.CreateTranslation(-0.5f, -0.5f, -0.5f));
		m_colorIndex = colorIndex;
		m_pureID = "CupronickelPipeDevice";
	}

	public override string GetCategory(int value)
	{
		if (m_colorIndex.HasValue)
		{
			return "Painted";
		}
		return base.GetCategory(value);
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return SubsystemPalette.GetName(subsystemTerrain, m_colorIndex, LanguageControl.Get("Devices", "Names", m_pureID));
	}

	public override string GetDescription(int value)
	{
		return LanguageControl.Get("Devices", "Descriptions", m_pureID);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		Color color = (m_colorIndex.HasValue ? (Color.White * SubsystemPalette.GetColor(generator.SubsystemTerrain, m_colorIndex)) : Color.White);
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_centerMesh, color, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		List<int> list = new List<int>();
		for (int i = 0; i < 6; i++)
		{
			if (CanConnect(value, new CellFace(x, y, z, i), generator.SubsystemTerrain.Terrain))
			{
				Point3 point = CellFace.FaceToPoint3(i);
				int num = x + point.X;
				int num2 = y + point.Y;
				int num3 = z + point.Z;
				int cellValueFast = generator.Terrain.GetCellValueFast(num, num2, num3);
				int connectorFace = CellFace.OppositeFace(i);
				if (BlockInterfaceResolver.ResolveFromValue<CupronickelPipeDevice>(cellValueFast) != null)
				{
					list.Add(i);
					generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_aroundMeshes[i], color, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
				}
				else if (BlockInterfaceResolver.HasFluidConnectorOnFace(generator.Terrain, cellValueFast, connectorFace, num, num2, num3))
				{
					list.Add(i);
					generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_edgeMeshes[i], color, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
				}
			}
		}
		if (list.Count == 0)
		{
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_edgeMeshes[1], color, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_edgeMeshes[3], color, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		}
		else if (list.Count == 1)
		{
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_edgeMeshes[CellFace.OppositeFace(list[0])], color, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		}
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		color = (m_colorIndex.HasValue ? (Color.White * SubsystemPalette.GetColor(environmentData.SubsystemTerrain, m_colorIndex)) : Color.White);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 3;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override float GetObjectShadowStrength(int value)
	{
		return 0f;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 107;
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new FluidPipeDeviceBehavior(project)
		};
	}

	public static string GetColoredCraftingID(string id, int? color)
	{
		return id + (color.HasValue ? ("." + color.Value) : "");
	}

	public Type GetTransferringFluidType()
	{
		return typeof(IPipeableFluid);
	}

	public bool CanConnect(int value, CellFace neighbor, Terrain terrain)
	{
		Point3 point = neighbor.Point + CellFace.FaceToPoint3(neighbor.Face);
		int cellValue = terrain.GetCellValue(point.X, point.Y, point.Z);
		if (Terrain.ExtractContents(cellValue) == BaseDeviceBlock.Index && DevicesBlockManager.GetDevice(cellValue) is CupronickelPipeDevice cupronickelPipeDevice)
		{
			if (!m_colorIndex.HasValue)
			{
				return true;
			}
			if (m_colorIndex.HasValue && !cupronickelPipeDevice.m_colorIndex.HasValue)
			{
				return true;
			}
			if (m_colorIndex.HasValue && cupronickelPipeDevice.m_colorIndex.HasValue && m_colorIndex.Value == cupronickelPipeDevice.m_colorIndex.Value)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public PipeFluidConnection GetConnection(HashSet<FluidTransferrer> fromTransferrers, HashSet<FluidTransferrer> toTransferrers, HashSet<FluidConnector> fromConnectors, HashSet<FluidConnector> toConnectors, float rate, Type transferringType, SubsystemFluidTransfer subsystemFluidTransfer)
	{
		return new PipeFluidConnection(fromTransferrers, toTransferrers, fromConnectors, toConnectors, rate, transferringType, subsystemFluidTransfer);
	}

	public int? GetPaintColor()
	{
		return m_colorIndex;
	}

	public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color)
	{
		return DevicesBlockManager.GetDeviceValue(GetColoredCraftingID(m_pureID, color), 0);
	}
}
