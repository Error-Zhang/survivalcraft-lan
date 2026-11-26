using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class ChargerVoltElement : VoltElement
{
	public Lazy<ComponentCharger?> m_componentCharger;

	public float OutputPower;

	public float OutputVoltage;

	public ChargerVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentCharger = subsystemTerrain.GetComponentAtPointLazy<ComponentCharger>(position);
	}

	public override void Update(float dt, float inputVoltage, float inputPower)
	{
		base.Update(dt, inputVoltage, inputPower);
		if (m_componentCharger.Value == null)
		{
			return;
		}
		bool flag = m_componentCharger.Value.Mode == ComponentCondenser.Mode.Charge;
		if (flag)
		{
			WorkState currentWorkState = CurrentWorkState;
			bool flag2 = ((currentWorkState == WorkState.Broken || currentWorkState == WorkState.Overloaded) ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			if (inputPower != 0f && inputVoltage != 0f)
			{
				m_componentCharger.Value.InputPower(inputPower * dt);
			}
		}
		else if (m_componentCharger.Value.Mode == ComponentCondenser.Mode.Discharge)
		{
			int k = CalcConnection();
			OutputPower = m_componentCharger.Value.OutputEnergy(m_standardPower, k, dt);
			OutputVoltage = ((OutputPower > 0f) ? m_standardVoltage : 0f);
		}
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		if (m_componentCharger.Value == null)
		{
			return 0f;
		}
		if (m_componentCharger.Value.Mode == ComponentCondenser.Mode.Charge)
		{
			return 0f - standardPL;
		}
		if (m_componentCharger.Value.Mode == ComponentCondenser.Mode.Discharge)
		{
			return OutputPower;
		}
		return 0f;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		if (m_componentCharger.Value == null)
		{
			return 0f;
		}
		if (m_componentCharger.Value.Mode == ComponentCondenser.Mode.Charge)
		{
			return 0f - standardUL;
		}
		if (m_componentCharger.Value.Mode == ComponentCondenser.Mode.Discharge)
		{
			return OutputVoltage;
		}
		return 0f;
	}
}
