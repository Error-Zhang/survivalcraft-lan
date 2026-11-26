using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class CondenserAVoltElement : CondenserBodyVoltElementBase
{
	private Lazy<ComponentCondenserA?> m_componentCondenser;

	public float OutputVoltage = 1f;

	protected override ComponentCondenser? Component => m_componentCondenser.Value;

	public CondenserAVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentCondenser = subsystemTerrain.GetComponentAtPointLazy<ComponentCondenserA>(position);
	}

	protected override float GetDischargeVoltage(ComponentCondenser component)
	{
		return OutputVoltage;
	}

	protected override bool ShouldBodyDischarge(ComponentCondenser component)
	{
		if (component is ComponentCondenserA componentCondenserA)
		{
			return componentCondenserA.OutputPower > 0f;
		}
		return false;
	}
}
