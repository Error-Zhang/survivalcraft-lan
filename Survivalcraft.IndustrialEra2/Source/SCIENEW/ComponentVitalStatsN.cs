using System;
using Engine;
using Game;

namespace SCIENEW;

public class ComponentVitalStatsN : ComponentVitalStats
{
	public override void Update(float dt)
	{
		if (!IndustrialEraLanAdapter.IsRemote(Project))
		{
			base.Update(dt);
			return;
		}

		if (m_componentPlayer.PlayerData.IsMainPlayer)
		{
			m_componentPlayer.ComponentGui.FoodBarWidget.Value = Food;
			UpdateTemperatureBar(Temperature);
		}
	}

	public override bool Eat(int value)
	{
		int num = Terrain.ExtractContents(value);
		Block obj = BlocksManager.Blocks[num];
		float num2 = obj.GetNutritionalValue(value);
		float sicknessProbability = obj.GetSicknessProbability(value);
		if (ItemsBlockManager.GetItemID("SleepingPill") == value)
		{
			return true;
		}
		if (ItemsBlockManager.GetItemID("IbuprofenPill") == value)
		{
			return true;
		}
		if (ItemsBlockManager.GetItemID("PenicillinPill") == value)
		{
			return true;
		}
		if (num2 > 0f)
		{
			if (m_componentPlayer.ComponentSickness.IsSick && sicknessProbability > 0f)
			{
				m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(ComponentVitalStats.fName, 1), Color.White, blinking: true, playNotificationSound: true);
				return false;
			}
			if (Food >= 0.98f)
			{
				m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(ComponentVitalStats.fName, 2), Color.White, blinking: true, playNotificationSound: true);
				return false;
			}
			m_subsystemAudio.PlayRandomSound("Audio/Creatures/HumanEat", 1f, m_random.Float(-0.2f, 0.2f), m_componentPlayer.ComponentBody.Position, 2f, 0f);
			if (m_componentPlayer.ComponentSickness.IsSick)
			{
				num2 *= 0.75f;
			}
			Food += num2;
			m_satiation.TryGetValue(num, out var value2);
			value2 += MathF.Max(num2, 0.5f);
			m_satiation[num] = value2;
			if (m_componentPlayer.ComponentSickness.IsSick)
			{
				m_componentPlayer.ComponentSickness.NauseaEffect();
			}
			else if (sicknessProbability >= 0.5f)
			{
				m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(ComponentVitalStats.fName, 3), Color.White, blinking: true, playNotificationSound: true);
			}
			else if (sicknessProbability > 0f)
			{
				m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(ComponentVitalStats.fName, 4), Color.White, blinking: true, playNotificationSound: true);
			}
			else if (value2 > 2.5f)
			{
				m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(ComponentVitalStats.fName, 5), Color.White, blinking: true, playNotificationSound: true);
			}
			else if (value2 > 2f)
			{
				m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(ComponentVitalStats.fName, 6), Color.White, blinking: true, playNotificationSound: true);
			}
			else if (Food > 0.85f)
			{
				m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(ComponentVitalStats.fName, 7), Color.White, blinking: true, playNotificationSound: true);
			}
			else
			{
				m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(ComponentVitalStats.fName, 8), Color.White, blinking: true, playNotificationSound: false);
			}
			if (m_random.Bool(sicknessProbability) || value2 > 3.5f)
			{
				m_componentPlayer.ComponentSickness.StartSickness();
			}
			m_componentPlayer.PlayerStats.FoodItemsEaten++;
			return true;
		}
		return false;
	}
}
