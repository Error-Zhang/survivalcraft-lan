using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ElectricToolDItem : MeshDamageableItem, IChargeableDItem
{
	public Vector3 IconBlockOffset;

	public Vector3 IconViewOffset;

	public float IconScale;

	public Type[] Behaviors;

	public float HackPower;

	public ElectricToolDItem(string id, int durability, Type[] behaviors, string modelName, string meshName, Texture2D texture, Color color, Matrix boneTransform, Matrix tcTransform, Vector3 iconBlockOffset, Vector3 iconViewOffset, float iconScale, float scale = 1f, float transform2 = 0f, float hackPower = 1f)
		: base(id, durability, modelName, meshName, texture, color, boneTransform, tcTransform, scale, transform2)
	{
		Behaviors = behaviors;
		IconScale = iconScale;
		IconViewOffset = iconViewOffset;
		IconBlockOffset = iconBlockOffset;
		HackPower = hackPower;
	}

	public ElectricToolDItem(string id, int durability)
		: base(id, durability)
	{
		Behaviors = Array.Empty<Type>();
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return IconBlockOffset;
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return IconViewOffset;
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return IconScale;
	}

	public override DamageableItemBehavior[] GetBehaviors(Project project)
	{
		List<DamageableItemBehavior> list = new List<DamageableItemBehavior>();
		Type[] behaviors = Behaviors;
		for (int i = 0; i < behaviors.Length; i++)
		{
			DamageableItemBehavior item = Activator.CreateInstance(behaviors[i], project) as DamageableItemBehavior;
			list.Add(item);
		}
		return list.ToArray();
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}

	public override int GetDamageDestructionValue(int value)
	{
		return DamageableItemManager.SetDamage(DamageableItemManager.GetItemID(GetCraftingId()), Durability);
	}

	public override float GetHackPower(int value)
	{
		return HackPower;
	}

	public static bool IsOutOfPower(int value)
	{
		return DamageableItemManager.GetDamage(value) >= DamageableItemManager.GetItem(value).GetDurability(value);
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

	public override bool IsEditable_(int value)
	{
		return true;
	}
}
