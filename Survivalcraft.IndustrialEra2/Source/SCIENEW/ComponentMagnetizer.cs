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
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentMagnetizer : ComponentInventoryBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	private One2OneRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	protected float m_speed = 0.1f;

	protected float m_fireTimeRemaining;

	public float m_heatLevel;

	public SubsystemExplosions m_subsystemExplosions;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemVoltNet m_subsystemVoltNet;

	public int ResultSlotIndex => SlotsCount - 1;

	public int FuelSlotIndex => SlotsCount - 2;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex == ResultSlotIndex)
		{
			return 0;
		}
		if (slotIndex == FuelSlotIndex)
		{
			if (!(BlocksManager.Blocks[Terrain.ExtractContents(value)].GetFuelHeatLevel(value) > 0f))
			{
				return 0;
			}
			return base.GetSlotCapacity(slotIndex, value);
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_fireTimeRemaining = valuesDictionary.GetValue("FireTimeRemaining", 0f);
		m_heatLevel = valuesDictionary.GetValue("HeatLevel", 0f);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
		valuesDictionary.SetValue("HeatLevel", m_heatLevel);
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
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "Magnetizer"));
		}
		return num2;
	}

	public void FindRecipe()
	{
		m_craftingRecipe = null;
		string ingredient = RecipesUtils.BlockToCratingID(GetSlotValue(0));
		One2OneRecipe actual = new One2OneRecipe
		{
			ProcessType = "Magnetizing",
			Ingredient = ingredient,
			IngredientCount = GetSlotCount(0)
		};
		m_craftingRecipe = RecipaediaEXManager.FindMatchingRecipe<One2OneRecipe>(actual);
		if (m_craftingRecipe != null)
		{
			Slot slot = m_slots[ResultSlotIndex];
			int num = Terrain.ExtractContents(m_craftingRecipe.ResultID);
			if (slot.Count != 0 && (m_craftingRecipe.ResultID != slot.Value || m_craftingRecipe.ResultCount + slot.Count > BlocksManager.Blocks[num].GetMaxStacking(m_craftingRecipe.ResultID)))
			{
				m_craftingRecipe = null;
			}
		}
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(coordinates);
		if (voltElement == null)
		{
			return;
		}
		if (voltElement.CurrentWorkState != WorkState.Active)
		{
			m_craftingProgress = 0f;
			m_fireTimeRemaining = 0f;
			m_heatLevel = 0f;
			return;
		}
		if (m_heatLevel > 0f)
		{
			m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt);
			if (m_fireTimeRemaining == 0f)
			{
				m_heatLevel = 0f;
			}
		}
		if (m_updateCraftingRecipe)
		{
			m_updateCraftingRecipe = false;
			FindRecipe();
		}
		if (m_craftingRecipe == null)
		{
			m_heatLevel = 0f;
			m_fireTimeRemaining = 0f;
		}
		if (m_craftingRecipe != null && m_fireTimeRemaining <= 0f)
		{
			Slot slot = m_slots[FuelSlotIndex];
			if (slot.Count > 0)
			{
				int num = Terrain.ExtractContents(slot.Value);
				Block block = BlocksManager.Blocks[num];
				if (block.GetExplosionPressure(slot.Value) > 0f)
				{
					slot.Count = 0;
					m_subsystemExplosions.TryExplodeBlock(coordinates.X, coordinates.Y, coordinates.Z, slot.Value);
				}
				else if (block.GetFuelHeatLevel(slot.Value) > 0f)
				{
					slot.Count--;
					m_fireTimeRemaining = block.GetFuelFireDuration(slot.Value);
					m_heatLevel = block.GetFuelHeatLevel(slot.Value);
				}
			}
		}
		if (m_fireTimeRemaining <= 0f)
		{
			m_craftingRecipe = null;
			m_craftingProgress = 0f;
		}
		if (m_craftingRecipe == null || (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "Magnetizer", m_craftingRecipe))))
		{
			return;
		}
		m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
		if (m_craftingProgress >= 1f)
		{
			int num2 = MathUtils.Max(1, m_craftingRecipe.IngredientCount);
			if (m_slots[0].Count < num2)
			{
				m_craftingRecipe = null;
				m_craftingProgress = 0f;
				m_updateCraftingRecipe = true;
				return;
			}
			m_slots[0].Count -= num2;
			m_slots[ResultSlotIndex].Value = m_craftingRecipe.ResultID;
			m_slots[ResultSlotIndex].Count += m_craftingRecipe.ResultCount;
			m_craftingRecipe = null;
			m_craftingProgress = 0f;
			m_updateCraftingRecipe = true;
		}
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		switch (kind)
		{
		case ProductionSlotKind.Input:
		{
			int num = Math.Max(0, SlotsCount - 2);
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = i;
			}
			return array;
		}
		case ProductionSlotKind.Output:
			return new _003C_003Ez__ReadOnlySingleElementList<int>(ResultSlotIndex);
		case ProductionSlotKind.Consume:
			return new _003C_003Ez__ReadOnlySingleElementList<int>(FuelSlotIndex);
		default:
			return m_emptySlotIndices;
		}
	}
}
