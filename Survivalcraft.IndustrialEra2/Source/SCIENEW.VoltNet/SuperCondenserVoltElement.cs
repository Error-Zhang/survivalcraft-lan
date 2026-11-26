using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class SuperCondenserVoltElement : CondenserBodyVoltElementBase
{
	private Lazy<ComponentSuperCondenser?> m_componentCondenser;

	public float OutputVoltage = 1f;

	protected override ComponentCondenser? Component => m_componentCondenser.Value;

	public SuperCondenserVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentCondenser = subsystemTerrain.GetComponentAtPointLazy<ComponentSuperCondenser>(position);
	}

	protected override float GetDischargeVoltage(ComponentCondenser component)
	{
		return OutputVoltage;
	}

	protected override bool ShouldBodyDischarge(ComponentCondenser component)
	{
		if (component is ComponentSuperCondenser componentSuperCondenser)
		{
			return componentSuperCondenser.OutputPower > 0f;
		}
		return false;
	}
}
