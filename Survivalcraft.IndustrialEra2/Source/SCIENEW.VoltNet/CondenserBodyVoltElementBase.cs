using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW.VoltNet;

public abstract class CondenserBodyVoltElementBase : VoltElement
{
	protected abstract ComponentCondenser? Component { get; }

	protected CondenserBodyVoltElementBase(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
	}

	protected abstract float GetDischargeVoltage(ComponentCondenser component);

	protected int CountBodyDischargeConnections()
	{
		ComponentCondenser component = Component;
		if (component == null)
		{
			return CalcConnection();
		}
		int num = 0;
		foreach (VoltConnection belongingConnection in m_belongingConnections)
		{
			foreach (KeyValuePair<CellFace, VoltElement> containsElement in belongingConnection.m_containsElements)
			{
				if (containsElement.Value == this && !(containsElement.Key.Point != Position) && !component.HasPortOnFace(containsElement.Key.Face))
				{
					num++;
					break;
				}
			}
		}
		return num;
	}

	public override void Update(float dt, float inputVoltage, float inputPower)
	{
		ComponentCondenser component = Component;
		if (component == null)
		{
			return;
		}
		if (component.CurrentMode == ComponentCondenser.Mode.Charge)
		{
			bool flag = inputPower != 0f && inputVoltage != 0f;
			if (flag)
			{
				WorkState currentWorkState = CurrentWorkState;
				bool flag2 = ((currentWorkState == WorkState.Broken || currentWorkState == WorkState.Overloaded) ? true : false);
				flag = !flag2;
			}
			if (flag)
			{
				component.InputEnergy(inputPower * dt);
			}
		}
		else if (component.CurrentMode == ComponentCondenser.Mode.Discharge && ShouldBodyDischarge(component))
		{
			int num = CountBodyDischargeConnections();
			if (num > 0)
			{
				component.OutputEnergy(component.OutputPower * dt * (float)num);
			}
		}
	}

	protected virtual bool ShouldBodyDischarge(ComponentCondenser component)
	{
		return component.HasEnergy;
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		ComponentCondenser component = Component;
		if (component == null)
		{
			return 0f;
		}
		if (component.HasPortOnFace(cellFace.Face))
		{
			return 0f;
		}
		if (component.CurrentMode == ComponentCondenser.Mode.Charge)
		{
			return 0f - standardPL;
		}
		if (component != null && component.CurrentMode == ComponentCondenser.Mode.Discharge && component.HasEnergy && ShouldBodyDischarge(component))
		{
			return component.OutputPower;
		}
		return 0f;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		ComponentCondenser component = Component;
		if (component == null)
		{
			return 0f;
		}
		if (component.HasPortOnFace(cellFace.Face))
		{
			return 0f;
		}
		if (component.CurrentMode == ComponentCondenser.Mode.Charge)
		{
			return 0f - standardUL;
		}
		if (component != null && component.CurrentMode == ComponentCondenser.Mode.Discharge && component.HasEnergy && ShouldBodyDischarge(component))
		{
			return GetDischargeVoltage(component);
		}
		return 0f;
	}
}
