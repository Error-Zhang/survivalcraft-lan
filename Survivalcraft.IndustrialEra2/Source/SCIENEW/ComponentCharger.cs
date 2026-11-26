using System;
using System.Collections.Generic;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCharger : ComponentInventoryBase
{
	public ComponentCondenser.Mode Mode;

	public bool ElectricalControl;

	public float m_cacheEnergy;

	public void InputPower(float energy)
	{
		Dictionary<int, IChargeableDItem> dictionary = new Dictionary<int, IChargeableDItem>();
		for (int i = 0; i < 8; i++)
		{
			int slotValue = GetSlotValue(i);
			if (Terrain.ExtractContents(slotValue) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(slotValue) is IChargeableDItem chargeableDItem && !chargeableDItem.IsFull(slotValue))
			{
				dictionary.Add(i, chargeableDItem);
			}
		}
		if (m_cacheEnergy < 1f)
		{
			m_cacheEnergy += energy;
			return;
		}
		int count = (int)(m_cacheEnergy * 10f / (float)dictionary.Count);
		foreach (KeyValuePair<int, IChargeableDItem> item in dictionary)
		{
			int value = item.Value.Charge(GetSlotValue(item.Key), count);
			m_slots[item.Key].Value = value;
		}
		m_cacheEnergy = 0f;
	}

	public float OutputEnergy(float power, int k, float dt)
	{
		float num = power * dt * (float)k;
		if (m_cacheEnergy > num)
		{
			m_cacheEnergy -= num;
			return power;
		}
		Dictionary<int, IChargeableDItem> dictionary = new Dictionary<int, IChargeableDItem>();
		for (int i = 0; i < 8; i++)
		{
			int slotValue = GetSlotValue(i);
			if (Terrain.ExtractContents(slotValue) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(slotValue) is IChargeableDItem chargeableDItem && !chargeableDItem.IsEmpty(slotValue))
			{
				dictionary.Add(i, chargeableDItem);
			}
		}
		int num2 = 0;
		foreach (KeyValuePair<int, IChargeableDItem> item in dictionary)
		{
			int num3 = Math.Max(((DamageableItemBlock)item.Value).GetDamage(GetSlotValue(item.Key)) - ((DamageableItemBlock)item.Value).GetDurability(GetSlotValue(item.Key)), 10);
			num2 += num3;
			m_slots[item.Key].Value = item.Value.Discharge(GetSlotValue(item.Key), num3);
		}
		m_cacheEnergy += (float)num2 * 0.1f;
		m_cacheEnergy -= num;
		if (!(m_cacheEnergy > 0f))
		{
			return 0f;
		}
		return power;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		Mode = valuesDictionary.GetValue("Mode", ComponentCondenser.Mode.Charge);
		ElectricalControl = valuesDictionary.GetValue("ElectricalControl", defaultValue: false);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Mode", Mode);
		valuesDictionary.SetValue("ElectricalControl", ElectricalControl);
	}
}
