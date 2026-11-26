using System;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentGasFurnace : ComponentIEFurnace, IUpdateable
{
	private ITank m_tank;

	private int m_cachedFuelValue;

	private float m_cachedFuelVolume;

	public override float SmeltSpeed => 0.3f;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex < 0 || slotIndex >= m_slots.Count)
		{
			return 0;
		}
		return BlocksManager.Blocks[Terrain.ExtractContents(value)].GetMaxStacking(value);
	}

	protected override void ReplaceFurnaceBlock(int cellValue)
	{
		int direction_Static = IlluminableDevice.GetDirection_Static(cellValue);
		int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<GasFurnaceDevice>((m_heatLevel > 0f && m_smeltingRecipe != null) ? (4 | direction_Static) : direction_Static);
		m_componentBlockEntity.BlockValue = deviceValueByType;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_furnaceSize = SlotsCount - 2;
		m_tank = Entity.FindComponent<ITank>();
	}

	public static int FindAcquireTankForFluid(ITank tank, int value)
	{
		return ComponentTankBase.FindAcquireTankForFluid(tank, value);
	}

	public static float AcquireFluid(ITank tank, int value, float volume)
	{
		return ComponentTankBase.AcquireFluid(tank, value, volume);
	}

	public static void EmitTankFluid(ITank tank, int tankIndex, Vector3 position, Vector3 velocity)
	{
		ComponentTankBase.EmitTankFluid(tank, tankIndex, position, velocity);
	}

	protected override void OnBeginUpdate(float dt)
	{
		ITank tank = GetTank();
		if (tank != null)
		{
			int tankValue = tank.GetTankValue(0);
			float tankVolume = tank.GetTankVolume(0);
			if (tankValue != m_cachedFuelValue || Math.Abs(tankVolume - m_cachedFuelVolume) > 1E-06f)
			{
				m_cachedFuelValue = tankValue;
				m_cachedFuelVolume = tankVolume;
				m_updateSmeltingRecipe = true;
			}
		}
	}

	public void SetFuelStateDirty()
	{
		m_cachedFuelValue = int.MinValue;
		m_cachedFuelVolume = -1f;
		m_updateSmeltingRecipe = true;
	}

	protected override void UpdateFireState(float dt)
	{
		if (m_heatLevel <= 0f)
		{
			m_fireTimeRemaining = 0f;
			return;
		}
		ITank tank = GetTank();
		if (tank == null)
		{
			StopSmelting(resetProgress: false);
			return;
		}
		int tankValue = tank.GetTankValue(0);
		if (!TryGetFuelFluid(tankValue, out IFuelFluid fuelFluid))
		{
			StopSmelting(resetProgress: false);
			return;
		}
		float fuelSpeed = fuelFluid.GetFuelSpeed(tankValue);
		if (fuelSpeed <= 0f)
		{
			StopSmelting(resetProgress: false);
			return;
		}
		tank.RemoveTankFluid(0, dt * fuelSpeed);
		float tankVolume = tank.GetTankVolume(0);
		m_fireTimeRemaining = tankVolume / fuelSpeed;
		if (tankVolume <= 0f)
		{
			StopSmelting(resetProgress: false);
		}
	}

	protected override bool TryAcquireFuel()
	{
		ITank tank = GetTank();
		if (tank == null)
		{
			return false;
		}
		int tankValue = tank.GetTankValue(0);
		if (!TryGetFuelFluid(tankValue, out IFuelFluid fuelFluid))
		{
			return false;
		}
		float tankVolume = tank.GetTankVolume(0);
		float fuelSpeed = fuelFluid.GetFuelSpeed(tankValue);
		if (tankVolume <= 0f || fuelSpeed <= 0f)
		{
			return false;
		}
		m_heatLevel = fuelFluid.GetFuelLevel(tankValue);
		m_fireTimeRemaining = tankVolume / fuelSpeed;
		return m_heatLevel > 0f;
	}

	protected override float GetHeatLevelForRecipeSearch()
	{
		if (m_heatLevel > 0f)
		{
			return m_heatLevel;
		}
		ITank tank = GetTank();
		if (tank == null)
		{
			return 0f;
		}
		int tankValue = tank.GetTankValue(0);
		if (!TryGetFuelFluid(tankValue, out IFuelFluid fuelFluid))
		{
			return 0f;
		}
		return fuelFluid.GetFuelLevel(tankValue);
	}

	private ITank GetTank()
	{
		if (m_tank == null)
		{
			m_tank = Entity.FindComponent<ITank>();
		}
		return m_tank;
	}

	private static bool TryGetFuelFluid(int fluidValue, out IFuelFluid fuelFluid)
	{
		fuelFluid = FluidsManager.GetFluid(fluidValue) as IFuelFluid;
		return fuelFluid != null;
	}
}
