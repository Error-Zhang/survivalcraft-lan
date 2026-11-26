using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class CreativeGeneratorVoltElement : VoltElement
{
	public Lazy<ComponentCreativeGenerator?> m_component;

	public CreativeGeneratorVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_component = subsystemTerrain.GetComponentAtPointLazy<ComponentCreativeGenerator>(position);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		return m_component.Value?.m_power ?? 0f;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return m_component.Value?.m_voltage ?? 0f;
	}
}
