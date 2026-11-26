using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentWindTurbine : ComponentInventoryBase, IUpdateable
{
	public float m_deltaTemperature;

	public ComponentBlockEntity m_componentBlockEntity;

	public ComponentIEModel m_componentIEModel;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemWeather m_subsystemWeather;

	public bool m_chunksNormal;

	public bool m_spaceEnough;

	public int m_direction;

	public float m_rotation;

	public float m_workingTime;

	public IRotorDItem? m_rotorItem;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float GetWindPower()
	{
		if (!m_chunksNormal)
		{
			return 0f;
		}
		if (!m_spaceEnough)
		{
			return 0f;
		}
		if (GetSlotCount(0) <= 0)
		{
			return 0f;
		}
		float num = (float)(m_componentBlockEntity.Coordinates.Y - 60) / 195f;
		PrecipitationShaftInfo precipitationShaftInfo = m_subsystemWeather.GetPrecipitationShaftInfo(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Z);
		if (m_componentBlockEntity.Coordinates.Y >= precipitationShaftInfo.YLimit - 1 && precipitationShaftInfo.Type == PrecipitationType.Rain)
		{
			num += precipitationShaftInfo.Intensity * 0.4f;
		}
		return Math.Max(MathUtils.Lerp(m_deltaTemperature, m_deltaTemperature * 2f, num), 0f);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_deltaTemperature = valuesDictionary.GetValue("DeltaTemperature", 0f);
		m_workingTime = valuesDictionary.GetValue("WorkingTime", 0f);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemWeather = Project.FindSubsystem<SubsystemWeather>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("DeltaTemperature", m_deltaTemperature);
		valuesDictionary.SetValue("WorkingTime", m_workingTime);
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		base.AddSlotItems(slotIndex, value, count);
		UpdateOnPoll();
		UpdateRotor();
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		int result = base.RemoveSlotItems(slotIndex, count);
		UpdateOnPoll();
		UpdateRotor();
		return result;
	}

	public void UpdateRotor()
	{
		if (GetSlotCount(0) <= 0)
		{
			m_rotorItem = null;
			return;
		}
		int slotValue = GetSlotValue(0);
		m_rotorItem = DamageableItemManager.GetItem(slotValue) as IRotorDItem;
		m_componentIEModel.SetModel(m_rotorItem?.GetModel());
		m_componentIEModel.SetTexture(IndustrialModLoader.IETexture);
	}

	public void UpdateOnPoll()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z);
		m_direction = FourDirectionModelDevice.GetDirection_Static(cellValueFast);
		Point3 position = coordinates + 16 * CellFace.FaceToPoint3(m_direction);
		float averageTemperatureAtChunk = TerrainUtils.GetAverageTemperatureAtChunk(m_subsystemTerrain, coordinates, out var chunkValid);
		float averageTemperatureAtChunk2 = TerrainUtils.GetAverageTemperatureAtChunk(m_subsystemTerrain, position, out var chunkValid2);
		m_deltaTemperature = averageTemperatureAtChunk - averageTemperatureAtChunk2;
		m_chunksNormal = chunkValid & chunkValid2;
		CheckSpace();
		UpdateRotor();
	}

	public void CheckSpace()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		Vector3 vector = -CellFace.FaceToVector3(FourDirectionModelDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z)));
		int x = coordinates.X;
		int y = coordinates.Y;
		int z = coordinates.Z;
		int[] array = new int[25]
		{
			0, 0, 1, 1, 1, 0, -1, -1, -1, -1,
			0, 1, 2, 2, 2, 2, 2, 1, 0, -1,
			-2, -2, -2, -2, -2
		};
		int[] array2 = new int[25]
		{
			0, -1, -1, 0, 1, 1, 1, 0, -1, -2,
			-2, -2, -2, -1, 0, 1, 2, 2, 2, 2,
			2, 1, 0, -1, -2
		};
		for (int i = 1; i < 5; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				x = coordinates.X - (int)vector.X * i;
				y = coordinates.Y - (int)vector.Y * i;
				z = coordinates.Z - (int)vector.Z * i;
				if (vector.X != 0f)
				{
					y = coordinates.Y + array[j];
					z = coordinates.Z + array2[j];
				}
				if (vector.Y != 0f)
				{
					x = coordinates.X + array[j];
					z = coordinates.Z + array2[j];
				}
				if (vector.Z != 0f)
				{
					x = coordinates.X + array[j];
					y = coordinates.Y + array2[j];
				}
				if (Terrain.ExtractContents(Terrain.ReplaceLight(m_subsystemTerrain.Terrain.GetCellValue(x, y, z), 0)) != AirBlock.Index)
				{
					m_spaceEnough = false;
					return;
				}
			}
		}
		m_spaceEnough = true;
	}

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (Terrain.ExtractContents(value) != BaseDamageableItemBlock.Index || !(DamageableItemManager.GetItem(value) is IRotorDItem))
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public void Update(float dt)
	{
		if (!m_chunksNormal)
		{
			UpdateOnPoll();
		}
		UpdateDrawing();
		if (m_rotorItem == null)
		{
			return;
		}
		float windPower = GetWindPower();
		m_rotation += dt * 2f * windPower;
		m_rotation %= (float)Math.PI * 2f;
		m_workingTime += windPower * dt;
		if (m_workingTime > 100f)
		{
			m_workingTime = 0f;
			int damage = DamageableItemManager.GetDamage(GetSlotValue(0));
			m_slots[0].Value = DamageableItemManager.SetDamage(GetSlotValue(0), damage + 1);
			if (BlocksManager.Blocks[BaseDamageableItemBlock.Index].GetBlockHealth(GetSlotValue(0)) < 0f)
			{
				m_slots[0].Count--;
				UpdateRotor();
				UpdateOnPoll();
			}
		}
	}

	public void UpdateDrawing()
	{
		if (m_rotorItem == null)
		{
			if (m_componentIEModel.Model != null)
			{
				m_componentIEModel.SetBoneTransform(0, null);
			}
		}
		else
		{
			Vector3 position = new Vector3(m_componentBlockEntity.Coordinates) + Vector3.One * 0.5f + CellFace.FaceToVector3(m_direction) * 0.7f;
			Matrix matrix = Matrix.CreateFromAxisAngle(CellFace.FaceToVector3(m_direction), m_rotation) * Matrix.CreateScale(1.5f) * Matrix.CreateTranslation(position);
			matrix = m_rotorItem.GetRotorTransformByFace(m_direction) * matrix;
			m_componentIEModel.SetBoneTransform(0, matrix);
		}
	}
}
