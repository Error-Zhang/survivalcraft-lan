using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class CondenserVoltElement : CondenserBodyVoltElementBase
{
	private Lazy<ComponentCondenser?> m_componentCondenser;

	protected override ComponentCondenser? Component => m_componentCondenser.Value;

	public CondenserVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentCondenser = subsystemTerrain.GetComponentAtPointLazy<ComponentCondenser>(position);
	}

	protected override float GetDischargeVoltage(ComponentCondenser component)
	{
		return component.OutputVoltage;
	}
}
