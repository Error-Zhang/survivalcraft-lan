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
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAlloyFurnace : ComponentInventoryTankBase, IUpdateable, IInventoryProductionSlots, ITankProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[2] { 0, 1 };

	private static readonly int[] m_inputTankIndices = new int[3] { 0, 1, 2 };

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	protected AlloyingEquationRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	public bool m_updateCraftingRecipe;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemTerrain m_subsystemTerrain;

	public int ResultSlotIndex => SlotsCount - 1;

	protected virtual float Speed => 0.1f;

	public float CraftingProgress => m_craftingProgress;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

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
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "AlloyFurnace"));
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
		if (m_updateCraftingRecipe)
		{
			m_updateCraftingRecipe = false;
			FindEquation();
		}
		if (m_craftingRecipe == null)
		{
			m_craftingProgress = 0f;
		}
		else
		{
			if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "AlloyFurnace", m_craftingRecipe)))
			{
				return;
			}
			m_craftingProgress = MathUtils.Min(m_craftingProgress + Speed * dt, 1f);
			if (m_craftingProgress >= 1f)
			{
				EquationItem[] materialsItems = m_craftingRecipe.MaterialsItems;
				for (int i = 0; i < materialsItems.Length; i++)
				{
					EquationItem equationItem = materialsItems[i];
					int[] values = equationItem.Values;
					foreach (int value in values)
					{
						if (ComponentInventoryTankBase.FindRemoveSlotForItem(this, value, equationItem.Count) >= 0)
						{
							break;
						}
					}
				}
				EquationFluid[] materialsFluids = m_craftingRecipe.MaterialsFluids;
				for (int i = 0; i < materialsFluids.Length; i++)
				{
					EquationFluid equationFluid = materialsFluids[i];
					ComponentInventoryTankBase.FindRemoveTankFluid(this, equationFluid.Value, equationFluid.Volume);
				}
				materialsItems = m_craftingRecipe.ResultsItems;
				for (int i = 0; i < materialsItems.Length; i++)
				{
					EquationItem equationItem2 = materialsItems[i];
					int[] values = equationItem2.Values;
					foreach (int value2 in values)
					{
						if (ComponentInventoryTankBase.AcquireItems(this, value2, equationItem2.Count, ResultSlotIndex..(ResultSlotIndex + 1)) > 0)
						{
							break;
						}
					}
				}
				m_craftingRecipe = null;
				m_craftingProgress = 0f;
				m_updateCraftingRecipe = true;
			}
		}
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<AlloyFurnaceDevice>((m_craftingProgress > 0f) ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public virtual void FindEquation()
	{
		m_craftingRecipe = null;
		float requiredPlayerLevel = FindInteractingPlayer()?.PlayerData.Level ?? 1f;
		List<EquationItem> list = new List<EquationItem>();
		List<EquationFluid> list2 = new List<EquationFluid>();
		for (int i = 0; i < 3; i++)
		{
			if (GetTankValue(i) != 0)
			{
				list2.Add(new EquationFluid(GetTankValue(i), GetTankVolume(i)));
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (GetSlotValue(j) != 0)
			{
				list.Add(new EquationItem(new int[1] { GetSlotValue(j) }, GetSlotCount(j)));
			}
		}
		AlloyingEquationRecipe alloyingEquationRecipe = RecipaediaEXManager.FindMatchingRecipe<AlloyingEquationRecipe>(new AlloyingEquationRecipe
		{
			MaterialsItems = list.ToArray(),
			MaterialsFluids = list2.ToArray(),
			RequiredPlayerLevel = requiredPlayerLevel
		});
		if (alloyingEquationRecipe == null)
		{
			return;
		}
		EquationItem[] resultsItems = alloyingEquationRecipe.ResultsItems;
		for (int k = 0; k < resultsItems.Length; k++)
		{
			int[] values = resultsItems[k].Values;
			foreach (int value in values)
			{
				if (ComponentInventoryTankBase.FindAcquireSlotForItem(this, value, ResultSlotIndex..(ResultSlotIndex + 1)) == -1)
				{
					return;
				}
			}
		}
		m_craftingRecipe = alloyingEquationRecipe;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_craftingProgress = valuesDictionary.GetValue("CraftingProgress", 0f);
		foreach (Tank tank in m_tanks)
		{
			tank.Capacity = 12f;
		}
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

	IEnumerable<int> ITankProductionSlots.GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Input)
		{
			return m_inputTankIndices;
		}
		return m_emptySlotIndices;
	}
}
