using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class AF_BoxGeneratorVoltElement : VoltElement
{
	public Lazy<ComponentAF_BoxGenerator?> m_componentAF_BoxGenerator;

	public AF_BoxGeneratorVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentAF_BoxGenerator = subsystemTerrain.GetComponentAtPointLazy<ComponentAF_BoxGenerator>(position);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		return (m_componentAF_BoxGenerator.Value?.m_selectedResult?.PL).GetValueOrDefault();
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return (m_componentAF_BoxGenerator.Value?.m_selectedResult?.UL).GetValueOrDefault();
	}
}
