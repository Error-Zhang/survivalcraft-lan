using System;
using System.Collections;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAF_BoxGenerator : ComponentInventoryBase, IUpdateable
{
	public class GachaResult
	{
		public int Level;

		public float UL;

		public float PL;

		public float Time;

		public float Probability;

		public required string Name;

		public Color Color;
	}

	public List<GachaResult> Pool = new List<GachaResult>();

	public GachaResult? m_selectedResult;

	public float m_time;

	public float m_gachaAnim;

	public CoroutineSchedulerOrder SchedulerOrder = new CoroutineSchedulerOrder();

	public SubsystemFireworksBlockBehavior m_subsystemFireworksBlockBehavior;

	public SubsystemTime m_subsystemTime;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		Pool.Add(new GachaResult
		{
			Level = 2,
			UL = 1f,
			PL = 24f,
			Time = 100f,
			Probability = 1f / 64f,
			Name = LanguageUtils.GetText(this, "FiveStar"),
			Color = new Color(255, 210, 0)
		});
		Pool.Add(new GachaResult
		{
			Level = 1,
			UL = 1f,
			PL = 6f,
			Time = 10f,
			Probability = 7f / 64f,
			Name = LanguageUtils.GetText(this, "FourStar"),
			Color = new Color(178, 132, 210, 255)
		});
		Pool.Add(new GachaResult
		{
			Level = 0,
			UL = 1f,
			PL = 1f,
			Time = 1f,
			Probability = 0.875f,
			Name = LanguageUtils.GetText(this, "ThreeStar"),
			Color = new Color(0, 174, 255)
		});
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemFireworksBlockBehavior = Project.FindSubsystem<SubsystemFireworksBlockBehavior>(throwOnError: true);
	}

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (value != ItemsBlockManager.GetItemID("AF_Post238") && value != ItemsBlockManager.GetItemID("AF_Post235"))
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public void Gacha()
	{
		if (m_selectedResult == null && !(m_gachaAnim > 0f) && GetSlotCount(0) != 0)
		{
			SchedulerOrder.AddCoroutine(GachaCoroutine());
		}
	}

	public IEnumerator GachaCoroutine()
	{
		while (m_gachaAnim < 1f)
		{
			m_gachaAnim += m_subsystemTime.GameTimeDelta;
			yield return default(WaitForFrameCondition);
		}
		m_gachaAnim = 0f;
		RemoveSlotItems(0, 1);
		float num = ((GetSlotValue(0) == ItemsBlockManager.GetItemID("AF_Post238")) ? new Game.Random().Float(0f, 1f) : new Game.Random().Float(0f, 0.25f));
		float num2 = 0f;
		foreach (GachaResult item in Pool)
		{
			num2 += item.Probability;
			if (num2 >= num)
			{
				m_selectedResult = item;
				break;
			}
		}
		FindInteractingPlayer().ComponentGui.DisplayLargeMessage(string.Format(LanguageUtils.GetText(this, "GachaResult"), m_selectedResult.Name), "", 10f, 0f);
		ComponentGui componentGui = FindInteractingPlayer().ComponentGui;
		string text = LanguageUtils.GetText(this, "GachaEffect");
		_003C_003Ey__InlineArray4<object> buffer = default;
		buffer[0] = m_selectedResult.Name;
		buffer[1] = m_selectedResult.Time;
		buffer[2] = m_selectedResult.PL;
		buffer[3] = m_selectedResult.UL;
		componentGui.DisplaySmallMessage(string.Format(text, (ReadOnlySpan<object?>)buffer), m_selectedResult.Color, blinking: true, playNotificationSound: true);
		SubsystemFireworksBlockBehavior subsystemFireworksBlockBehavior = m_subsystemFireworksBlockBehavior;
		subsystemFireworksBlockBehavior.m_newYearCelebrationTimeRemaining = m_selectedResult.Level switch
		{
			1 => 5f,
			2 => 20f,
			_ => m_subsystemFireworksBlockBehavior.m_newYearCelebrationTimeRemaining,
		};
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		SchedulerOrder.RemoveAllCoroutines();
	}

	public void Update(float dt)
	{
		SchedulerOrder.UpdateCoroutines();
		if (m_selectedResult != null)
		{
			m_time += dt;
			if (m_time >= m_selectedResult.Time)
			{
				m_selectedResult = null;
				m_time = 0f;
			}
		}
	}
}
