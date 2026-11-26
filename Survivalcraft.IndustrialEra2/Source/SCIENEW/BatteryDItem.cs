using System;
using Engine;
using Engine.Graphics;

namespace SCIENEW;

public class BatteryDItem : MeshDamageableItem, IChargeableDItem
{
	public BatteryDItem(string id, int durability, string modelName, string meshName, Texture2D texture, Color color, Matrix boneTransform, Matrix tcTransform, float scale = 1f, float transform2 = 0f)
		: base(id, durability, modelName, meshName, texture, color, boneTransform, tcTransform, scale, transform2)
	{
	}

	public int Charge(int value, int count)
	{
		return DamageableItemManager.SetDamage(value, Math.Max(DamageableItemManager.GetDamage(value) - count, 0));
	}

	public int Discharge(int value, int count)
	{
		return DamageableItemManager.SetDamage(value, Math.Min(DamageableItemManager.GetDamage(value) + count, Durability));
	}

	public bool IsFull(int value)
	{
		return DamageableItemManager.GetDamage(value) == 0;
	}

	public bool IsEmpty(int value)
	{
		return DamageableItemManager.GetDamage(value) == Durability;
	}
}
