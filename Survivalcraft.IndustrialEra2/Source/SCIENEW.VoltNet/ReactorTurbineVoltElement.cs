using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class ReactorTurbineVoltElement : VoltElement
{
	public Lazy<ComponentReactorTurbine?> m_component;

	public ReactorTurbineVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_component = subsystemTerrain.GetComponentAtPointLazy<ComponentReactorTurbine>(position);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		return (m_component.Value?.OutputPower ?? 0f) / (float)MathUtils.Max(CalcConsumerConnectionCount(), 1);
	}

	public override float GetConsumerDemand(float standardPL, CellFace cellFace)
	{
		return 0f;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return m_component.Value?.OutputVoltage ?? 0f;
	}
}
