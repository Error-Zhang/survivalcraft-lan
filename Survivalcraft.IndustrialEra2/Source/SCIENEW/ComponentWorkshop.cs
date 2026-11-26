using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.ProductionIO;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW;

public class ComponentWorkshop : ComponentIECraftingTable, IUpdateable
{
	public SubsystemPickables m_subsystemPickables;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public SubsystemTime m_subsystemTime;

	public SubsystemVoltNet m_subsystemVoltNet;

	public Vector3 m_position;

	public IInventory? m_inventory;

	public double m_lastPopTime;

	public WorkState m_workState;

	public new UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemBlockEntities = Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_position = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f);
	}

	public new void Update(float dt)
	{
		base.Update(dt);
		int x = m_componentBlockEntity.Coordinates.X;
		int y = m_componentBlockEntity.Coordinates.Y;
		int z = m_componentBlockEntity.Coordinates.Z;
		m_inventory = m_subsystemBlockEntities.GetBlockEntity(x, y - 1, z)?.Entity.FindComponent<IInventory>();
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(coordinates);
		if (voltElement == null)
		{
			m_workState = WorkState.Broken;
		}
		else
		{
			m_workState = voltElement.CurrentWorkState;
		}
	}

	public void PopResults()
	{
		if (m_workState == WorkState.Active)
		{
			ElectricPopResult();
		}
		else
		{
			NormalPopResult();
		}
	}

	protected void ElectricPopResult()
	{
		if (m_subsystemTime.GameTime - m_lastPopTime < 1.0 || m_inventory == null || m_matchedRecipe == null)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		bool flag = true;
		foreach (string item in m_matchedRecipe.Ingredients.Where((string x) => !string.IsNullOrEmpty(x)))
		{
			if (!flag)
			{
				return;
			}
			for (int num = 0; num < m_inventory.SlotsCount; num++)
			{
				if (dictionary.ContainsKey(num) && dictionary[num] >= m_inventory.GetSlotCount(num))
				{
					continue;
				}
				string actualIngredient = RecipesUtils.BlockToCratingID(m_inventory.GetSlotValue(num));
				if (RecipesUtils.CompareIngredients(item, actualIngredient))
				{
					if (!dictionary.TryAdd(num, 1))
					{
						dictionary[num]++;
					}
					break;
				}
				if (num == m_inventory.SlotsCount - 1)
				{
					flag = false;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			m_inventory.RemoveSlotItems(item2.Key, item2.Value);
		}
		int resultValue = m_matchedRecipe.ResultValue;
		int resultCount = m_matchedRecipe.ResultCount;
		if (resultCount > 0)
		{
			m_subsystemPickables.AddPickable(resultValue, resultCount, m_position + Vector3.UnitY * 0.7f, Vector3.UnitY * 4f, null);
		}
		int remainsValue = m_matchedRecipe.RemainsValue;
		int remainsCount = m_matchedRecipe.RemainsCount;
		if (remainsCount > 0)
		{
			m_subsystemPickables.AddPickable(remainsValue, remainsCount, m_position + Vector3.UnitY * 0.7f, Vector3.UnitY * 4f, null);
		}
		m_lastPopTime = m_subsystemTime.GameTime;
	}

	protected void NormalPopResult()
	{
		Vector3 position = m_position + Vector3.UnitY * 0.7f;
		Vector3 value = Vector3.UnitY * 4f;
		ValueEnumerator<FromEnumerable<int>, int> enumerator = GetSlotIndices(ProductionSlotKind.Output).AsValueEnumerable().GetEnumerator<FromEnumerable<int>, int>();
		try
		{
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				if (current >= 0 && current < SlotsCount)
				{
					int slotValue = GetSlotValue(current);
					int num = RemoveSlotItems(current, GetSlotCount(current));
					if (num > 0)
					{
						m_subsystemPickables.AddPickable(slotValue, num, position, value, null);
					}
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
	}
}
