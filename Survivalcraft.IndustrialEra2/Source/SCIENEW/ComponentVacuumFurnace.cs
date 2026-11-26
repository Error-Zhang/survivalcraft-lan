using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using SCIENEW.Events;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentVacuumFurnace : ComponentInventoryBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	private VacuumFurnaceRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemAirPumps m_subsystemAirPumps;

	private WorkState m_workState;

	public int RemainsSlotIndex => SlotsCount - 1;

	public int ResultSlotIndex => SlotsCount - 2;

	public int CircSlot1Index => SlotsCount - 3;

	public int CircSlot2Index => SlotsCount - 4;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float CraftingProgress => m_craftingProgress;

	public float m_speed => 0.1f;

	public bool IsWorking()
	{
		if (m_workState == WorkState.Active)
		{
			return m_subsystemAirPumps.GetPointCount(m_componentBlockEntity.Coordinates) >= 1;
		}
		return false;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemAirPumps = Project.FindSubsystem<SubsystemAirPumps>(throwOnError: true);
		m_craftingProgress = valuesDictionary.GetValue("CraftingProgress", 0f);
		m_updateCraftingRecipe = true;
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
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "VacuumFurnace"));
		}
		return num2;
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		m_workState = (Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true).GetVoltElement(coordinates) as VacuumFurnaceVoltElement)?.CurrentWorkState ?? WorkState.Broken;
		if (IsWorking())
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
				if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "VacuumFurnace", m_craftingRecipe)))
				{
					return;
				}
				m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
				if (m_craftingProgress >= 1f)
				{
					for (int i = 0; i < 4; i++)
					{
						if (m_slots[i].Count > 0)
						{
							m_slots[i].Count--;
						}
					}
					m_slots[ResultSlotIndex].Value = m_craftingRecipe.ResultID;
					m_slots[ResultSlotIndex].Count += m_craftingRecipe.ResultCount;
					if (m_craftingRecipe.RemainsCount > 0)
					{
						m_slots[RemainsSlotIndex].Value = m_craftingRecipe.RemainsID;
						m_slots[RemainsSlotIndex].Count += m_craftingRecipe.RemainsCount;
					}
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
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<VacuumFurnaceDevice>(IsWorking() ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public void FindRecipe()
	{
		m_craftingRecipe = null;
		VacuumFurnaceRecipe vacuumFurnaceRecipe = RecipaediaEXManager.FindMatchingRecipe<VacuumFurnaceRecipe>(new VacuumFurnaceRecipe
		{
			Ingredient1 = RecipesUtils.BlockToCratingID(GetSlotValue(0)),
			Ingredient2 = RecipesUtils.BlockToCratingID(GetSlotValue(1)),
			Ingredient3 = RecipesUtils.BlockToCratingID(GetSlotValue(2)),
			Ingredient4 = RecipesUtils.BlockToCratingID(GetSlotValue(3))
		});
		if (vacuumFurnaceRecipe == null)
		{
			return;
		}
		Slot slot = m_slots[ResultSlotIndex];
		int num = Terrain.ExtractContents(vacuumFurnaceRecipe.ResultID);
		if (slot.Count != 0 && (vacuumFurnaceRecipe.ResultID != slot.Value || vacuumFurnaceRecipe.ResultCount + slot.Count > BlocksManager.Blocks[num].GetMaxStacking(vacuumFurnaceRecipe.ResultID)))
		{
			return;
		}
		if (vacuumFurnaceRecipe.RemainsCount > 0)
		{
			Slot slot2 = m_slots[RemainsSlotIndex];
			int num2 = Terrain.ExtractContents(vacuumFurnaceRecipe.RemainsID);
			if (slot2.Count != 0 && (vacuumFurnaceRecipe.RemainsID != slot2.Value || vacuumFurnaceRecipe.RemainsCount + slot2.Count > BlocksManager.Blocks[num2].GetMaxStacking(vacuumFurnaceRecipe.RemainsID)))
			{
				return;
			}
		}
		m_craftingRecipe = vacuumFurnaceRecipe;
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => EnumerateInputSlots(),
			ProductionSlotKind.Output => new _003C_003Ez__ReadOnlyArray<int>(new int[2] { ResultSlotIndex, RemainsSlotIndex }),
			_ => m_emptySlotIndices,
		};
	}

	private IEnumerable<int> EnumerateInputSlots()
	{
		for (int i = 0; i <= CircSlot1Index; i++)
		{
			yield return i;
		}
	}
}
