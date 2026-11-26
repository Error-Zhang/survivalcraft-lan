using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class AdvancedCrusherDevice : DefaultDevice, IEntityDevice<ComponentAdvancedCrusher>, IVoltDevice, IElectricElementDevice
{
	public Model m_model;

	public BlockMesh m_blockMesh = new BlockMesh();

	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public Texture2D m_texture;

	public BlockMesh m_wheel1Mesh = new BlockMesh();

	public BlockMesh m_wheel2Mesh = new BlockMesh();

	public AdvancedCrusherDevice()
		: base("AdvancedCrusherDevice")
	{
		m_model = ContentManager.Get<Model>("Models/AdvancedCrusher");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Body").ParentBone);
		m_blockMesh.AppendModelMeshPart(m_model.FindMesh("Body").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(1f, 0f, 1f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("StandaloneMesh").ParentBone);
		m_standaloneBlockMesh.AppendModelMeshPart(m_model.FindMesh("StandaloneMesh").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_texture = ContentManager.Get<Texture2D>("Textures/Blocks_SiyahAkrep");
		m_wheel1Mesh.AppendModelMeshPart(m_model.FindMesh("Crusher_Wheel0").MeshParts[0], Matrix.Identity, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_wheel2Mesh.AppendModelMeshPart(m_model.FindMesh("Crusher_Wheel1").MeshParts[0], Matrix.Identity, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetOpaque);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_texture, color, size, ref matrix, environmentData);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.6f;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 0.1f, 0f);
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
		return -99;
	}

	public override float GetObjectShadowStrength(int value)
	{
		return 0f;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 99;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		Point3 point = raycastResult.CellFace.Point + CellFace.FaceToPoint3(raycastResult.CellFace.Face);
		Point3[] array = new Point3[3]
		{
			new Point3(point.X + 1, point.Y, point.Z),
			new Point3(point.X, point.Y, point.Z + 1),
			new Point3(point.X + 1, point.Y, point.Z + 1)
		};
		Dictionary<Point3, bool> dictionary = new Dictionary<Point3, bool>();
		bool flag = true;
		for (int i = 0; i < array.Length; i++)
		{
			TerrainRaycastResult raycastResult2 = raycastResult;
			raycastResult2.CellFace.Point = array[i] - CellFace.FaceToPoint3(raycastResult.CellFace.Face);
			bool flag2 = subsystemTerrain.Terrain.GetCellContents(array[i].X, array[i].Y, array[i].Z) == AirBlock.Index && componentMiner.Place(raycastResult2, DevicesBlockManager.GetDeviceValueByType<EmptySpaceDevice>());
			dictionary[array[i]] = flag2;
			flag &= flag2;
		}
		if (flag && componentMiner.Place(raycastResult, DevicesBlockManager.GetDeviceValueByType<EmptySpaceDevice>()))
		{
			subsystemTerrain.DestroyCell(0, point.X, point.Y, point.Z, 0, noDrop: true, noParticleSystem: true);
			return base.GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
		}
		foreach (KeyValuePair<Point3, bool> item in dictionary)
		{
			if (item.Value)
			{
				subsystemTerrain.DestroyCell(0, item.Key.X, item.Key.Y, item.Key.Z, 0, noDrop: true, noParticleSystem: true);
			}
		}
		return default;
	}

	public string GetEntityName(int value)
	{
		return "AdvancedCrusher";
	}

	public Widget GetWidget(IInventory inventory, ComponentAdvancedCrusher component)
	{
		return new AdvancedCrusherWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new AdvancedCrusherDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 3 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new AdvancedCrusherVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 3f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new CrafterPopResultsElectricElement(subsystemElectricity, new Point3(x, y, z), 2);
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		if (recipe is One2OneRecipe one2OneRecipe)
		{
			return one2OneRecipe.ProcessType == "Kibbling";
		}
		return false;
	}
}
