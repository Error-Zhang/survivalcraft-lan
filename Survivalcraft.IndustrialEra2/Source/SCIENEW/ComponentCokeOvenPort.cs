using System;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCokeOvenPort : ComponentTankBase
{
	public ComponentCokeOven? m_componentCokeOven;

	public int m_coalTarIndex;

	public float m_yieldUnit = 1f;

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (!(FluidsManager.GetFluid(value).GetCraftingID(value) == "C:CoalTar"))
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}

	public void AssignCokeOven(ComponentCokeOven componentCokeOven)
	{
		m_componentCokeOven = componentCokeOven;
		m_componentCokeOven.OnRecipeFinished = YieldTar;
	}

	public void RemoveCokeOven()
	{
		ComponentCokeOven? componentCokeOven = m_componentCokeOven;
		componentCokeOven.OnRecipeFinished = (Action)Delegate.Remove(componentCokeOven.OnRecipeFinished, new Action(YieldTar));
		m_componentCokeOven = null;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_tanks[0].Capacity = 64f;
		m_coalTarIndex = FluidsManager.GetFluidID("C:CoalTar", 0);
	}

	public void YieldTar()
	{
		m_tanks[0].Value = m_coalTarIndex;
		m_tanks[0].Volume = Math.Min(m_tanks[0].Volume + m_yieldUnit, m_tanks[0].Capacity);
	}
}
