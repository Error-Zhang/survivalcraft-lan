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
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentWireComposer : ComponentInventoryBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[2] { 0, 1 };

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	private WireComposingRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	protected float m_speed = 0.25f;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemTerrain m_subsystemTerrain;

	public int ResultSlotIndex => SlotsCount - 1;

	public int IngredientSlotIndex => 1;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float CraftingProgress => m_craftingProgress;

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
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "WireComposer"));
		}
		return num2;
	}

	public void Update(float dt)
	{
		if (m_subsystemEnginePower.IsPowered(m_componentBlockEntity.Coordinates, out IEngineComponent _))
		{
			if (m_updateCraftingRecipe)
			{
				m_updateCraftingRecipe = false;
				FindRecipe();
			}
			if (m_craftingRecipe == null)
			{
				m_craftingProgress = 0f;
			}
			else
			{
				if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "WireComposer", m_craftingRecipe)))
				{
					return;
				}
				m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
				if (m_craftingProgress >= 1f)
				{
					int num = MathUtils.Max(1, m_craftingRecipe.IngredientCount);
					int num2 = MathUtils.Max(1, m_craftingRecipe.WireIngredientCount);
					if (m_slots[0].Count >= num)
					{
						m_slots[0].Count -= num;
					}
					if (m_slots[IngredientSlotIndex].Count >= num2)
					{
						m_slots[IngredientSlotIndex].Count -= num2;
					}
					m_slots[ResultSlotIndex].Value = m_craftingRecipe.ResultID;
					m_slots[ResultSlotIndex].Count += m_craftingRecipe.ResultCount;
					m_craftingRecipe = null;
					m_craftingProgress = 0f;
					m_updateCraftingRecipe = true;
				}
			}
		}
		else
		{
			m_craftingProgress = 0f;
			m_craftingRecipe = null;
			m_updateCraftingRecipe = true;
		}
	}

	public void FindRecipe()
	{
		m_craftingRecipe = null;
		WireComposingRecipe wireComposingRecipe = RecipaediaEXManager.FindMatchingRecipe<WireComposingRecipe>(new WireComposingRecipe
		{
			Ingredient = RecipesUtils.BlockToCratingID(GetSlotValue(0)),
			IngredientCount = GetSlotCount(0),
			WireIngredient = RecipesUtils.BlockToCratingID(GetSlotValue(IngredientSlotIndex)),
			WireIngredientCount = GetSlotCount(IngredientSlotIndex)
		});
		if (wireComposingRecipe != null)
		{
			Slot slot = m_slots[ResultSlotIndex];
			int num = Terrain.ExtractContents(wireComposingRecipe.ResultID);
			if (slot.Count == 0 || (wireComposingRecipe.ResultID == slot.Value && wireComposingRecipe.ResultCount + slot.Count <= BlocksManager.Blocks[num].GetMaxStacking(wireComposingRecipe.ResultID)))
			{
				m_craftingRecipe = wireComposingRecipe;
			}
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_craftingProgress = valuesDictionary.GetValue("CraftingProgress", 0f);
		m_updateCraftingRecipe = true;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("CraftingProgress", m_craftingProgress);
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
