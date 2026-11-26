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
using ZLinq;

namespace SCIENEW;

public class ComponentOilPlant : ComponentInventoryTankBase, IUpdateable, IInventoryProductionSlots, ITankProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[3] { 0, 1, 2 };

	private static readonly int[] m_outputSlotIndices = new int[3] { 3, 4, 5 };

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	private PetrochemicalEquationRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	protected float m_speed = 0.1f;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public SubsystemFireBoxes m_subsystemFireBoxes;

	private bool m_previousWorkState = true;

	private bool m_currentWorkState;

	public int RemainsSlotIndex => SlotsCount - 2;

	public int ResultSlotIndex => SlotsCount - 3;

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
		bool flag = slotIndex == SlotsCount - 1 || slotIndex == SlotsCount - 2 || slotIndex == SlotsCount - 3;
		int num = (flag ? GetSlotValue(slotIndex) : 0);
		int num2 = base.RemoveSlotItems(slotIndex, count);
		if (((num2 > 0) & flag) && num != 0)
		{
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "OilPlant"));
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
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		m_currentWorkState = m_subsystemFireBoxes.IsPreheated(coordinates, out IFireBoxComponent _);
		if (!m_currentWorkState)
		{
			m_craftingProgress = 0f;
			m_craftingRecipe = null;
		}
		if (m_previousWorkState != m_currentWorkState)
		{
			m_previousWorkState = m_currentWorkState;
			m_updateCraftingRecipe = true;
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
		else
		{
			if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "OilPlant", m_craftingRecipe)))
			{
				return;
			}
			m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
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
						if (ComponentInventoryTankBase.AcquireItems(this, value2, equationItem2.Count, 3..6) > 0)
						{
							break;
						}
					}
				}
				materialsFluids = m_craftingRecipe.ResultsFluids;
				for (int i = 0; i < materialsFluids.Length; i++)
				{
					EquationFluid equationFluid2 = materialsFluids[i];
					ComponentInventoryTankBase.AcquireFluid(this, equationFluid2.Value, equationFluid2.Volume, 3..6);
				}
				m_craftingRecipe = null;
				m_craftingProgress = 0f;
				m_updateCraftingRecipe = true;
			}
		}
		ReplaceFurnace(coordinates);
	}

	public void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = FourDirectionalDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<OilPlantDevice>((m_currentWorkState && m_craftingRecipe != null) ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public void FindEquation()
	{
		m_craftingRecipe = null;
		float requiredPlayerLevel = FindInteractingPlayer()?.PlayerData.Level ?? 1f;
		List<EquationItem> list = new List<EquationItem>();
		List<EquationFluid> list2 = new List<EquationFluid>();
		int[] inputSlotIndices = m_inputSlotIndices;
		foreach (int num in inputSlotIndices)
		{
			if (GetSlotValue(num) != 0)
			{
				list.Add(new EquationItem(new int[1] { GetSlotValue(num) }, GetSlotCount(num)));
			}
			if (GetTankValue(num) != 0)
			{
				list2.Add(new EquationFluid(GetTankValue(num), GetTankVolume(num)));
			}
		}
		PetrochemicalEquationRecipe petrochemicalEquationRecipe = RecipaediaEXManager.FindMatchingRecipe<PetrochemicalEquationRecipe>(new PetrochemicalEquationRecipe
		{
			MaterialsItems = list.ToArray(),
			MaterialsFluids = list2.ToArray(),
			RequiredPlayerLevel = requiredPlayerLevel
		});
		if (petrochemicalEquationRecipe == null)
		{
			return;
		}
		EquationItem[] resultsItems = petrochemicalEquationRecipe.ResultsItems;
		for (int i = 0; i < resultsItems.Length; i++)
		{
			inputSlotIndices = resultsItems[i].Values;
			foreach (int value in inputSlotIndices)
			{
				if (ComponentInventoryTankBase.FindAcquireSlotForItem(this, value, 3..6) == -1)
				{
					return;
				}
			}
		}
		EquationFluid[] resultsFluids = petrochemicalEquationRecipe.ResultsFluids;
		for (int i = 0; i < resultsFluids.Length; i++)
		{
			EquationFluid equationFluid = resultsFluids[i];
			if (ComponentInventoryTankBase.FindAcquireTankForFluid(this, equationFluid.Value, 3..6) == -1)
			{
				return;
			}
		}
		inputSlotIndices = m_outputSlotIndices;
		foreach (int slotIndex in inputSlotIndices)
		{
			int resultSlotValue = GetSlotValue(slotIndex);
			if (resultSlotValue != 0 && petrochemicalEquationRecipe.ResultsItems.AsValueEnumerable().All((EquationItem x) => x.Values.AsValueEnumerable().All((int y) => resultSlotValue != y)))
			{
				return;
			}
		}
		inputSlotIndices = m_outputSlotIndices;
		foreach (int tankIndex in inputSlotIndices)
		{
			int resultTankValue = GetTankValue(tankIndex);
			if (resultTankValue != 0 && petrochemicalEquationRecipe.ResultsFluids.AsValueEnumerable().All((EquationFluid x) => resultTankValue != x.Value))
			{
				return;
			}
		}
		m_craftingRecipe = petrochemicalEquationRecipe;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemFireBoxes = Project.FindSubsystem<SubsystemFireBoxes>(throwOnError: true);
		m_craftingProgress = valuesDictionary.GetValue("CraftingProgress", 0f);
		m_tanks[0].Capacity = (m_tanks[1].Capacity = (m_tanks[2].Capacity = (m_tanks[3].Capacity = (m_tanks[4].Capacity = (m_tanks[5].Capacity = 4f)))));
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
			ProductionSlotKind.Output => m_outputSlotIndices,
			_ => m_emptySlotIndices,
		};
	}
}
