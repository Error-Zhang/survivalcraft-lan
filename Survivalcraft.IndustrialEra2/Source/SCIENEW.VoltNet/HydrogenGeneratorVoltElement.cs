using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class HydrogenGeneratorVoltElement : VoltElement
{
	public Lazy<ComponentHydrogenGenerator?> m_componentHydrogenGenerator;

	public HydrogenGeneratorVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentHydrogenGenerator = subsystemTerrain.GetComponentAtPointLazy<ComponentHydrogenGenerator>(position);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		ComponentHydrogenGenerator value = m_componentHydrogenGenerator.Value;
		if (value == null || !value.IsWorking)
		{
			return 0f;
		}
		return standardPL / (float)MathUtils.Max(CalcConsumerConnectionCount(), 1);
	}

	public override float GetConsumerDemand(float standardPL, CellFace cellFace)
	{
		return 0f;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		ComponentHydrogenGenerator value = m_componentHydrogenGenerator.Value;
		if (value == null || !value.IsWorking)
		{
			return 0f;
		}
		return standardUL;
	}
}
