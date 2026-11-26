using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using SCIENEW.Events;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentFermenter : ComponentInventoryTankBase, IUpdateable, IInventoryProductionSlots, ITankProductionSlots
{
	private static readonly int[] m_itemInputSlotIndices = new int[3] { 0, 1, 2 };

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	private static readonly int[] m_outputTankIndices = new int[1];

	private FermentingEquationRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	protected float m_speed = 0.05f;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public SubsystemMetersBlockBehavior m_subsystemMetersBlockBehavior;

	public float m_temperature;

	public int RemainsSlotIndex => SlotsCount - 2;

	public int ResultSlotIndex => SlotsCount - 1;

	public float CraftingProgress => m_craftingProgress;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex >= 3)
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		m_updateCraftingRecipe = true;
		base.AddSlotItems(slotIndex, value, count);
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		m_updateCraftingRecipe = true;
		return base.RemoveSlotItems(slotIndex, count);
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		m_updateCraftingRecipe = true;
		base.AddTankFluid(tankIndex, value, volume, out overflow);
	}

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		m_updateCraftingRecipe = true;
		return base.RemoveTankFluid(tankIndex, volume);
	}

	public void Update(float dt)
	{
		if (m_subsystemTime.PeriodicGameTimeEvent(2.0, 2.0 * (double)GetHashCode() % 1000.0 / 1000.0))
		{
			int x = m_componentBlockEntity.Coordinates.X;
			int y = m_componentBlockEntity.Coordinates.Y;
			int z = m_componentBlockEntity.Coordinates.Z;
			m_subsystemMetersBlockBehavior.CalculateTemperature(x, y, z, 0f, 0f, out m_temperature, out var _, out var _);
			m_temperature /= 15f;
		}
		if (m_updateCraftingRecipe)
		{
			m_updateCraftingRecipe = false;
			FindEquation();
		}
		if (m_craftingRecipe == null)
		{
			m_craftingProgress = 0f;
		}
		else if (!(m_craftingProgress <= 0f) || RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "Fermenter", m_craftingRecipe)))
		{
			m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt * m_temperature, 1f);
			if (m_craftingProgress >= 1f)
			{
				ComponentInventoryTankBase.FindRemoveSlotForItem(this, m_craftingRecipe.IngredientValue, m_craftingRecipe.IngredientCount);
				ComponentInventoryTankBase.AcquireFluid(this, m_craftingRecipe.ResultFluidValue, m_craftingRecipe.ResultFluidVolume);
				m_craftingRecipe = null;
				m_craftingProgress = 0f;
				m_updateCraftingRecipe = true;
			}
		}
	}

	public void FindEquation()
	{
		m_craftingRecipe = null;
		if (GetSlotValue(0) != 0)
		{
			float requiredPlayerLevel = FindInteractingPlayer()?.PlayerData.Level ?? 1f;
			FermentingEquationRecipe fermentingEquationRecipe = RecipaediaEXManager.FindMatchingRecipe<FermentingEquationRecipe>(new FermentingEquationRecipe
			{
				IngredientValue = GetSlotValue(0),
				IngredientCount = GetSlotCount(0),
				RequiredPlayerLevel = requiredPlayerLevel
			});
			if (fermentingEquationRecipe != null && ComponentInventoryTankBase.FindAcquireTankForFluid(this, fermentingEquationRecipe.ResultFluidValue) != -1)
			{
				m_craftingRecipe = fermentingEquationRecipe;
			}
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemMetersBlockBehavior = Project.FindSubsystem<SubsystemMetersBlockBehavior>(throwOnError: true);
		m_craftingProgress = valuesDictionary.GetValue("CraftingProgress", 0f);
		m_temperature = valuesDictionary.GetValue("Temperature", 0f);
		m_tanks[0].Capacity = 4f;
		m_updateCraftingRecipe = true;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("CraftingProgress", m_craftingProgress);
		valuesDictionary.SetValue("Temperature", m_temperature);
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => (IEnumerable<int>)m_itemInputSlotIndices,
			ProductionSlotKind.Output => new _003C_003Ez__ReadOnlyArray<int>(new int[2] { RemainsSlotIndex, ResultSlotIndex }),
			_ => m_emptySlotIndices,
		};
	}

	IEnumerable<int> ITankProductionSlots.GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Output)
		{
			return m_outputTankIndices;
		}
		return m_emptySlotIndices;
	}
}
