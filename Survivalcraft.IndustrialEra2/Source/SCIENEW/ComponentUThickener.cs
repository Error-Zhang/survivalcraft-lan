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
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentUThickener : ComponentInventoryTankBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[1];

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	protected ThickeningEquationRecipe? m_craftingRecipe;

	protected float m_speed = 0.1f;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemVoltNet m_subsystemVoltNet;

	public float CraftingProgress => m_craftingProgress;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int ResultSlotIndex => SlotsCount - 1;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_updateCraftingRecipe = true;
		m_tanks[0].Capacity = 16f;
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_craftingProgress = valuesDictionary.GetValue("CraftingProgress", 0f);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("CraftingProgress", m_craftingProgress);
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		m_updateCraftingRecipe = true;
		base.AddSlotItems(slotIndex, value, count);
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		m_updateCraftingRecipe = true;
		int num = ((slotIndex == ResultSlotIndex) ? GetSlotValue(ResultSlotIndex) : 0);
		int num2 = base.RemoveSlotItems(slotIndex, count);
		if (num2 > 0 && slotIndex == ResultSlotIndex && num != 0)
		{
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "UThickener"));
		}
		return num2;
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
		if (m_subsystemVoltNet.GetVoltElement(m_componentBlockEntity.Coordinates) is UThickenerVoltElement { CurrentWorkState: WorkState.Active })
		{
			if (m_updateCraftingRecipe)
			{
				m_updateCraftingRecipe = false;
				FindEquation();
			}
			if (m_craftingRecipe == null)
			{
				m_craftingProgress = 0f;
			}
			else if (!(m_craftingProgress <= 0f) || RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "UThickener", m_craftingRecipe)))
			{
				m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
				if (m_craftingProgress >= 1f)
				{
					ComponentInventoryTankBase.FindRemoveSlotForItem(this, m_craftingRecipe.IngredientValue, m_craftingRecipe.IngredientCount);
					ComponentInventoryTankBase.FindRemoveTankFluid(this, m_craftingRecipe.IngredientFluidValue, m_craftingRecipe.IngredientFluidVolume);
					ComponentInventoryTankBase.AcquireItems(this, m_craftingRecipe.ResultValue, m_craftingRecipe.ResultCount, ResultSlotIndex..(ResultSlotIndex + 1));
					m_craftingRecipe = null;
					m_craftingProgress = 0f;
					m_updateCraftingRecipe = true;
				}
			}
		}
		else
		{
			m_craftingProgress = 0f;
		}
	}

	public void FindEquation()
	{
		m_craftingRecipe = null;
		if (GetSlotValue(0) != 0 && GetTankValue(0) != 0)
		{
			float requiredPlayerLevel = FindInteractingPlayer()?.PlayerData.Level ?? 1f;
			ThickeningEquationRecipe thickeningEquationRecipe = RecipaediaEXManager.FindMatchingRecipe<ThickeningEquationRecipe>(new ThickeningEquationRecipe
			{
				IngredientValue = GetSlotValue(0),
				IngredientCount = GetSlotCount(0),
				IngredientFluidValue = GetTankValue(0),
				IngredientFluidVolume = GetTankVolume(0),
				RequiredPlayerLevel = requiredPlayerLevel
			});
			if (thickeningEquationRecipe != null && ComponentInventoryTankBase.FindAcquireSlotForItem(this, thickeningEquationRecipe.ResultValue, ResultSlotIndex..(ResultSlotIndex + 1)) != -1)
			{
				m_craftingRecipe = thickeningEquationRecipe;
			}
		}
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => (IEnumerable<int>)m_inputSlotIndices,
			ProductionSlotKind.Output => new int[1] { ResultSlotIndex },
			_ => m_emptySlotIndices,
		};
	}
}
