using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Extensions.Graphics;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentDriller : ComponentInventoryBase, IUpdateable, IDrawable
{
	public DrillerDevice.Mode Mode;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemPickables m_subsystemPickables;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemTime m_subsystemTime;

	public SubsystemAudio m_subsystemAudio;

	private float m_rotation;

	private int m_direction;

	private BlockMesh[,] m_drillRodMeshesByFace = new BlockMesh[6, 2];

	private BlockMesh[] m_drillHeadMeshesByFace = new BlockMesh[6];

	private PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

	private DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();

	private float m_depth;

	private Dictionary<int, float> m_rodsToSpeed;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1];

	public int RodSlotIndex => 1;

	public int DrillerIndex => 0;

	public int RocksIndex => 2;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex == RodSlotIndex)
		{
			if (m_rodsToSpeed.ContainsKey(value))
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			return 0;
		}
		if (slotIndex == DrillerIndex)
		{
			if (Terrain.ExtractContents(value) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(value) is DrillDItem)
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		Mode = valuesDictionary.GetValue("Mode", DrillerDevice.Mode.Drill);
		m_depth = valuesDictionary.GetValue("Depth", 0f);
		m_rodsToSpeed = new Dictionary<int, float>();
		m_rodsToSpeed.Add(ItemsBlockManager.GetItemIdByMaterial<RodItem>(Materials.Iron), 1f);
		m_rodsToSpeed.Add(ItemsBlockManager.GetItemIdByMaterial<RodItem>(Materials.Steel), 2f);
		Model model = ContentManager.Get<Model>("Models/DrillRod");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("DrillRod").ParentBone);
		Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Driller").ParentBone);
		for (int i = 0; i < 6; i++)
		{
			Matrix matrix;
			if (i >= 4)
			{
				matrix = ((i != 4) ? (Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f)) : Matrix.CreateTranslation(0.5f, 0f, 0.5f));
			}
			else
			{
				matrix = Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY((float)i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
			}
			m_drillRodMeshesByFace[i, 0] = new BlockMesh();
			m_drillRodMeshesByFace[i, 0].AppendModelMeshPart(model.FindMesh("DrillRod").MeshParts[0], boneAbsoluteTransform * matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_drillRodMeshesByFace[i, 1] = new BlockMesh();
			m_drillRodMeshesByFace[i, 1].AppendModelMeshPart(model.FindMesh("DrillRodShort").MeshParts[0], boneAbsoluteTransform * matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			m_drillHeadMeshesByFace[i] = new BlockMesh();
			m_drillHeadMeshesByFace[i].AppendModelMeshPart(model.FindMesh("Driller").MeshParts[0], boneAbsoluteTransform2 * matrix, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Mode", Mode);
		valuesDictionary.SetValue("Depth", m_depth);
	}

	public void Update(float dt)
	{
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z);
		m_direction = SixDirectionModelDevice.GetDirection_Static(cellValueFast);
		int slotCount = GetSlotCount(RodSlotIndex);
		float num = ((GetSlotValue(RodSlotIndex) == 0) ? 0f : m_rodsToSpeed[GetSlotValue(RodSlotIndex)]);
		bool flag = m_subsystemEnginePower.IsPowered(m_componentBlockEntity.Coordinates, out IEngineComponent _);
		bool flag2 = GetSlotCount(DrillerIndex) > 0;
		if (flag)
		{
			m_rotation += dt * 2f * num;
			m_rotation %= (float)Math.PI * 2f;
		}
		int i;
		for (i = 0; i < 40; i++)
		{
			Point3 point = m_componentBlockEntity.Coordinates - CellFace.FaceToPoint3(m_direction) * i;
			if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValueFast(point.X, point.Y, point.Z)) == BedrockBlock.Index)
			{
				break;
			}
		}
		int num2 = Math.Min(slotCount, i);
		if (flag & flag2)
		{
			if (MathF.Abs(m_depth - (float)num2) > 0.1f)
			{
				m_depth = ((m_depth > (float)num2) ? (m_depth - dt * num) : (m_depth + dt * num));
				if (num2 == 0)
				{
					m_depth = 0f;
				}
			}
			else
			{
				m_depth = num2;
			}
		}
		else
		{
			m_depth = 0f;
		}
		Vector3 vector = CellFace.FaceToVector3(m_direction);
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int x = coordinates.X;
		int y = coordinates.Y;
		int z = coordinates.Z;
		int[] array = new int[9] { 0, 0, 1, 1, 1, 0, -1, -1, -1 };
		int[] array2 = new int[9] { 0, -1, -1, 0, 1, 1, 1, 0, -1 };
		_ = Vector3.Zero;
		if (flag & flag2)
		{
			for (int j = 1; (float)j <= m_depth + 1f; j++)
			{
				for (int k = 0; k < 9; k++)
				{
					x = coordinates.X - (int)vector.X * j;
					y = coordinates.Y - (int)vector.Y * j;
					z = coordinates.Z - (int)vector.Z * j;
					if (vector.X != 0f)
					{
						y = coordinates.Y + array[k];
						z = coordinates.Z + array2[k];
					}
					if (vector.Y != 0f)
					{
						x = coordinates.X + array[k];
						z = coordinates.Z + array2[k];
					}
					if (vector.Z != 0f)
					{
						x = coordinates.X + array[k];
						y = coordinates.Y + array2[k];
					}
					DrillBlock(new Point3(x, y, z));
				}
			}
		}
		if (!m_subsystemTime.PeriodicGameTimeEvent(0.25, 0.0) || Mode != DrillerDevice.Mode.Dispose)
		{
			return;
		}
		int num3 = 0;
		foreach (Slot slot in m_slots)
		{
			if (num3++ > RodSlotIndex && slot != null && slot.Count > 0)
			{
				if (m_subsystemProjectiles.FireProjectile(slot.Value, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + CellFace.FaceToVector3(m_direction) * 0.6f, CellFace.FaceToVector3(m_direction) * 40f, Vector3.Zero, null) != null)
				{
					m_subsystemAudio.PlaySound("Audio/DispenserShoot", 1f, 0f, new Vector3(m_componentBlockEntity.Coordinates), 4f, autoDelay: true);
				}
				else
				{
					m_subsystemPickables.AddPickable(slot.Value, 1, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + CellFace.FaceToVector3(m_direction) * 0.5f, CellFace.FaceToVector3(m_direction) * 5f, null);
				}
				slot.Count--;
				break;
			}
		}
	}

	private void DrillBlock(Point3 position)
	{
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(position.X, position.Y, position.Z);
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast)];
		if ((!(block is AirBlock) && !(block is FluidBlock) && !(block is BedrockBlock) && !(block is FocalorsFluidBlock)) || 1 == 0)
		{
			m_subsystemTerrain.DestroyCell(0, position.X, position.Y, position.Z, 0, noDrop: true, noParticleSystem: false);
			int damage = DamageableItemManager.GetDamage(GetSlotValue(DrillerIndex));
			m_slots[DrillerIndex].Value = DamageableItemManager.SetDamage(GetSlotValue(DrillerIndex), damage + 1);
			if (BlocksManager.Blocks[BaseDamageableItemBlock.Index].GetBlockHealth(GetSlotValue(DrillerIndex)) < 0f)
			{
				m_slots[DrillerIndex].Count--;
			}
			cellValueFast = Terrain.ReplaceLight(cellValueFast, 0);
			if (ComponentInventoryBase.AcquireItems(this, cellValueFast, 1) > 0)
			{
				m_subsystemPickables.AddPickable(cellValueFast, 1, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + CellFace.FaceToVector3(m_direction) * 0.5f, CellFace.FaceToVector3(m_direction) * 5f, null);
			}
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, coordinates);
		Matrix matrix = Matrix.CreateTranslation(-DevicesUtils.NormalToVector3(m_direction) * 0.5f) * Matrix.CreateFromAxisAngle(CellFace.FaceToVector3(m_direction), m_rotation) * Matrix.CreateTranslation(DevicesUtils.NormalToVector3(m_direction) * 0.5f);
		Matrix matrix2 = matrix * Matrix.CreateTranslation(new Vector3(coordinates));
		if (GetSlotCount(RodSlotIndex) > 0 && m_subsystemTerrain.IsVisible(camera, new Vector3(coordinates)))
		{
			BlocksManager.DrawMeshBlock(m_primitivesRenderer, m_drillRodMeshesByFace[m_direction, 1], IndustrialModLoader.IETexture, Color.White, 1f, ref matrix2, m_drawBlockEnvironmentData);
		}
		for (int i = 0; (float)i < ((MathF.Ceiling(m_depth) == m_depth) ? (m_depth + 1f) : MathF.Ceiling(m_depth)); i++)
		{
			if (i == 0)
			{
				float num = m_depth % 1f;
				int direction = m_direction;
				bool flag = (((uint)direction <= 1u || direction == 4) ? true : false);
				Vector3 vector = (flag ? (new Vector3(coordinates) - CellFace.FaceToVector3(m_direction) * num) : (new Vector3(coordinates) - CellFace.FaceToVector3(m_direction)));
				if (m_subsystemTerrain.IsVisible(camera, vector))
				{
					m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, vector);
					Matrix matrix3 = Matrix.CreateScale(Vector3.One - DevicesUtils.FaceToVector3Abs(m_direction) * (1f - num)) * matrix * Matrix.CreateTranslation(vector);
					BlocksManager.DrawMeshBlock(m_primitivesRenderer, m_drillRodMeshesByFace[m_direction, 0], IndustrialModLoader.IETexture, Color.White, 1f, ref matrix3, m_drawBlockEnvironmentData);
				}
			}
			else
			{
				Vector3 vector2 = new Vector3(coordinates) - CellFace.FaceToVector3(m_direction) * (m_depth % 1f + (float)i);
				if (m_subsystemTerrain.IsVisible(camera, vector2))
				{
					Matrix matrix4 = matrix * Matrix.CreateTranslation(vector2);
					m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, vector2);
					BlocksManager.DrawMeshBlock(m_primitivesRenderer, m_drillRodMeshesByFace[m_direction, 0], IndustrialModLoader.IETexture, Color.White, 1f, ref matrix4, m_drawBlockEnvironmentData);
				}
			}
		}
		if (GetSlotCount(DrillerIndex) > 0)
		{
			Vector3 vector3 = new Vector3(coordinates) - CellFace.FaceToVector3(m_direction) * m_depth;
			Matrix matrix5 = matrix * Matrix.CreateTranslation(vector3);
			m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, vector3);
			if (m_subsystemTerrain.IsVisible(camera, vector3))
			{
				BlocksManager.DrawMeshBlock(m_primitivesRenderer, m_drillHeadMeshesByFace[m_direction], IndustrialModLoader.IETexture, Color.White, 1f, ref matrix5, m_drawBlockEnvironmentData);
			}
		}
		m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
	}
}
