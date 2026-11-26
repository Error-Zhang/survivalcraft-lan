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

public class ComponentCentrifuge : ComponentInventoryBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[1];

	private static readonly int[] m_outputSlotIndices = new int[4] { 1, 2, 3, 4 };

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	protected bool m_updateCraftingRecipe;

	protected float m_speed = 0.1f;

	protected float m_craftingProgress;

	private SiftingRecipe? m_craftingRecipe;

	public SubsystemVoltNet m_subsystemVoltNet;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemPickables m_subsystemPickables;

	public SubsystemTerrain m_subsystemTerrain;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int MaterialSlot => 0;

	public float CraftingProgress => m_craftingProgress;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
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
		bool flag = slotIndex != MaterialSlot;
		int num = (flag ? GetSlotValue(slotIndex) : 0);
		int num2 = base.RemoveSlotItems(slotIndex, count);
		if (((num2 > 0) & flag) && num != 0)
		{
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "Centrifuge"));
		}
		return num2;
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		VoltElement? voltElement = m_subsystemVoltNet.GetVoltElement(coordinates);
		if (voltElement != null && voltElement.CurrentWorkState == WorkState.Active)
		{
			if (m_updateCraftingRecipe)
			{
				m_updateCraftingRecipe = false;
				m_craftingRecipe = FindRecipe(GetSlotValue(MaterialSlot), GetSlotCount(MaterialSlot));
			}
			if (m_craftingRecipe == null)
			{
				m_craftingProgress = 0f;
			}
			else
			{
				if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "Centrifuge", m_craftingRecipe)))
				{
					return;
				}
				m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
				if (!(m_craftingProgress >= 1f))
				{
					return;
				}
				if (m_slots[MaterialSlot].Count > 0)
				{
					m_slots[MaterialSlot].Count--;
				}
				if (m_craftingRecipe.Results.Length != 0)
				{
					float num = 0f;
					SiftingRecipe.SiftingResult[] results = m_craftingRecipe.Results;
					foreach (SiftingRecipe.SiftingResult siftingResult in results)
					{
						num = MathUtils.Max(num, siftingResult.Weight);
					}
					if (num > 0f)
					{
						float num2 = new Game.Random().Float(0f, 1f);
						results = m_craftingRecipe.Results;
						for (int i = 0; i < results.Length; i++)
						{
							SiftingRecipe.SiftingResult siftingResult2 = results[i];
							if (siftingResult2.Weight <= 0f || siftingResult2.Weight / num < num2)
							{
								continue;
							}
							int value = siftingResult2.Value;
							int num3 = MathUtils.Max(1, siftingResult2.Count);
							for (int j = 0; j < num3; j++)
							{
								if (ComponentInventoryTankBase.AcquireItems(this, value, 1, 1..5) > 0)
								{
									int direction_Static = FourDirectionalDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
									Vector3 position = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + CellFace.FaceToVector3(direction_Static) * 0.7f;
									m_subsystemPickables.AddPickable(value, 1, position, CellFace.FaceToVector3(direction_Static) * 4f, null);
								}
							}
						}
					}
				}
				m_craftingRecipe = null;
				m_craftingProgress = 0f;
				m_updateCraftingRecipe = true;
			}
		}
		else
		{
			m_craftingProgress = 0f;
			m_craftingRecipe = null;
			m_updateCraftingRecipe = true;
		}
	}

	public SiftingRecipe? FindRecipe(int mainMaterial, int materialCount)
	{
		if (mainMaterial == 0)
		{
			return null;
		}
		return RecipaediaEXManager.FindMatchingRecipe<SiftingRecipe>(new SiftingRecipe
		{
			Ingredient = RecipesUtils.BlockToCratingID(mainMaterial),
			IngredientCount = materialCount,
			ProcessType = "Centrifuging"
		});
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
