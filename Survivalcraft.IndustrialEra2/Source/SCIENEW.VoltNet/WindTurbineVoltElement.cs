using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class WindTurbineVoltElement : VoltElement
{
	public Lazy<ComponentWindTurbine?> m_component;

	public WindTurbineVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_component = subsystemTerrain.GetComponentAtPointLazy<ComponentWindTurbine>(position);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		if (m_component.Value == null)
		{
			return 0f;
		}
		return Math.Abs(m_component.Value.GetWindPower());
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return standardUL;
	}
}
