using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.VoltNet;

namespace SCIENEW;

public static class WireDrawingManager
{
	public static Dictionary<Materials, (BlockMesh CenterMesh, BlockMesh[] AroundMeshes)> m_meshes = new Dictionary<Materials, (BlockMesh, BlockMesh[])>();

	public static void Init()
	{
		Model model = ContentManager.Get<Model>("Models/IEWire");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("CopperWire_Center").ParentBone);
		BlockMesh blockMesh = new BlockMesh();
		BlockMesh[] array = new BlockMesh[6];
		blockMesh.AppendModelMeshPart(model.FindMesh("CopperWire_Center").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		for (int i = 0; i < 6; i++)
		{
			array[i] = new BlockMesh();
			boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("CopperWire_" + i).ParentBone);
			array[i].AppendModelMeshPart(model.FindMesh("CopperWire_" + i).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
		m_meshes.Add(Materials.Copper, (blockMesh, array));
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("SilverWire_Center").ParentBone);
		BlockMesh blockMesh2 = new BlockMesh();
		BlockMesh[] array2 = new BlockMesh[6];
		blockMesh2.AppendModelMeshPart(model.FindMesh("SilverWire_Center").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		for (int j = 0; j < 6; j++)
		{
			array2[j] = new BlockMesh();
			boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("SilverWire_" + j).ParentBone);
			array2[j].AppendModelMeshPart(model.FindMesh("SilverWire_" + j).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
		m_meshes.Add(Materials.Silver, (blockMesh2, array2));
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("GoldWire_Center").ParentBone);
		BlockMesh blockMesh3 = new BlockMesh();
		BlockMesh[] array3 = new BlockMesh[6];
		blockMesh3.AppendModelMeshPart(model.FindMesh("GoldWire_Center").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		for (int k = 0; k < 6; k++)
		{
			array3[k] = new BlockMesh();
			boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("GoldWire_" + k).ParentBone);
			array3[k].AppendModelMeshPart(model.FindMesh("GoldWire_" + k).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
		m_meshes.Add(Materials.Gold, (blockMesh3, array3));
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("AluminumWire_Center").ParentBone);
		BlockMesh blockMesh4 = new BlockMesh();
		BlockMesh[] array4 = new BlockMesh[6];
		blockMesh4.AppendModelMeshPart(model.FindMesh("AluminumWire_Center").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		for (int l = 0; l < 6; l++)
		{
			array4[l] = new BlockMesh();
			boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("AluminumWire_" + l).ParentBone);
			array4[l].AppendModelMeshPart(model.FindMesh("AluminumWire_" + l).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
		m_meshes.Add(Materials.Aluminum, (blockMesh4, array4));
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("SteelWire_Center").ParentBone);
		BlockMesh blockMesh5 = new BlockMesh();
		BlockMesh[] array5 = new BlockMesh[6];
		blockMesh5.AppendModelMeshPart(model.FindMesh("SteelWire_Center").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		for (int m = 0; m < 6; m++)
		{
			array5[m] = new BlockMesh();
			boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("SteelWire_" + m).ParentBone);
			array5[m].AppendModelMeshPart(model.FindMesh("SteelWire_" + m).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
		m_meshes.Add(Materials.Steel, (blockMesh5, array5));
	}

	public static BlockMesh GetStandaloneBlockMesh(Materials material)
	{
		BlockMesh blockMesh = new BlockMesh();
		blockMesh.AppendBlockMesh(m_meshes[material].CenterMesh);
		blockMesh.AppendBlockMesh(m_meshes[material].AroundMeshes[1]);
		blockMesh.AppendBlockMesh(m_meshes[material].AroundMeshes[3]);
		blockMesh.TransformPositions(Matrix.CreateTranslation(-0.5f, -0.5f, -0.5f));
		return blockMesh;
	}

	public static void GenerateWireVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z, Materials targetMaterial)
	{
		if (m_meshes.TryGetValue(targetMaterial, out (BlockMesh, BlockMesh[]) value2))
		{
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, value2.Item1, Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
		}
		HashSet<int> voltConnectionFaceMask = BlockInterfaceResolver.GetVoltConnectionFaceMask(value);
		for (int i = 0; i < 6; i++)
		{
			Point3 point = CellFace.FaceToPoint3(i);
			int item = CellFace.OppositeFace(i);
			int cellValueFast = generator.Terrain.GetCellValueFast(x + point.X, y + point.Y, z + point.Z);
			object interfaceProviderByValue = BlockInterfaceResolver.GetInterfaceProviderByValue(cellValueFast);
			HashSet<int> voltConnectionFaceMask2 = BlockInterfaceResolver.GetVoltConnectionFaceMask(cellValueFast);
			if (!voltConnectionFaceMask.Contains(i) || !voltConnectionFaceMask2.Contains(item))
			{
				continue;
			}
			if (interfaceProviderByValue is IVoltDevice)
			{
				generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_meshes[m_meshes.ContainsKey(targetMaterial) ? targetMaterial : Materials.Copper].AroundMeshes[i], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
			}
			if (!(interfaceProviderByValue is IVoltWireDevice voltWireDevice))
			{
				continue;
			}
			Materials material = voltWireDevice.GetMaterial();
			(BlockMesh, BlockMesh[]) value3;
			if (targetMaterial != Materials.None)
			{
				if (material == targetMaterial || material == Materials.None)
				{
					generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_meshes[targetMaterial].AroundMeshes[i], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
				}
			}
			else if (m_meshes.TryGetValue(material, out value3))
			{
				generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, value3.Item2[i], Color.White, null, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetAlphaTest);
			}
		}
	}
}
