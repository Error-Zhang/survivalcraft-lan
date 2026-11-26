using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentSteamTurbine : ComponentEngine, IUpdateable, IEngineComponent
{
	protected float m_fireTimeRemaining;

	protected float m_heatLevel;

	public float m_speed = 1f;

	public float waterConsumingSpeed = 2f / 75f;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemExplosions m_subsystemExplosions;

	public ComponentIEModel? m_componentIEModel;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float HeatLevel => m_heatLevel;

	public float FireTimeRemaining => m_fireTimeRemaining;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (!(BlocksManager.Blocks[Terrain.ExtractContents(value)].GetFuelHeatLevel(value) > 0f))
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_tanks[0].Capacity = 16f;
		m_fireTimeRemaining = valuesDictionary.GetValue("FireTimeRemaining", 0f);
		m_heatLevel = valuesDictionary.GetValue("HeatLevel", 0f);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		if (m_type == EngineType.Block)
		{
			m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
			SteamTurbineDevice device = DevicesBlockManager.GetDevice<SteamTurbineDevice>();
			if (device != null)
			{
				m_componentIEModel.SetModel(device.m_model);
				m_componentIEModel.SetTexture(IndustrialModLoader.IETexture);
				m_componentIEModel.MainBoneName = device.m_model.RootBone.Name;
				m_componentIEModel.RenderingMode = ModelRenderingMode.AlphaThreshold;
			}
		}
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
		valuesDictionary.SetValue("HeatLevel", m_heatLevel);
	}

	public void Update(float dt)
	{
		if (m_fireTimeRemaining <= 0f)
		{
			if (m_heatLevel > 0f)
			{
				m_heatLevel = 0f;
			}
			if (GetSlotCount(0) > 0)
			{
				int slotValue = GetSlotValue(0);
				Block block = BlocksManager.Blocks[Terrain.ExtractContents(slotValue)];
				if (block.GetExplosionPressure(slotValue) > 0f && HasWater())
				{
					m_slots[0].Count--;
					if (m_type == EngineType.Block)
					{
						m_subsystemExplosions.TryExplodeBlock(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, slotValue);
					}
					else
					{
						m_subsystemExplosions.AddExplosion((int)m_componentBody.Position.X, (int)m_componentBody.Position.Y, (int)m_componentBody.Position.Z, block.GetExplosionPressure(slotValue), isIncendiary: false, noExplosionSound: false);
					}
				}
				if (block.GetFuelFireDuration(slotValue) > 0f && HasWater())
				{
					m_fireTimeRemaining = block.GetFuelFireDuration(slotValue) * 1.5f;
					m_heatLevel = block.GetFuelHeatLevel(slotValue);
					m_slots[0].Count--;
				}
			}
		}
		else if (HasWater())
		{
			m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt);
			m_tanks[0].Volume = MathUtils.Max(0f, m_tanks[0].Volume - dt * waterConsumingSpeed);
		}
		else
		{
			m_fireTimeRemaining = 0f;
		}
		if (m_type == EngineType.Block)
		{
			ReplaceFurnace(m_componentBlockEntity.Coordinates);
			UpdateDrawing();
		}
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<SteamTurbineDevice>((m_heatLevel > 0f) ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	private bool HasWater()
	{
		if (GetTankValue(0) == FluidsManager.GetFluidID("WaterFluid", 0))
		{
			return GetTankVolume(0) > 0f;
		}
		return false;
	}

	public new bool IsPowering()
	{
		return m_heatLevel > 0f;
	}

	public void UpdateDrawing()
	{
		if (m_componentIEModel != null)
		{
			if (m_type != EngineType.Block || !IsPowering())
			{
				m_componentIEModel.SetBoneTransform("Fan", null);
				return;
			}
			Matrix value = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 0f, 0.5f));
			Matrix value2 = Matrix.CreateRotationY((float)(Time.RealTime * 20.0 % 6.2831854820251465));
			m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, value);
			m_componentIEModel.SetBoneTransform("Fan", value2);
		}
	}
}
