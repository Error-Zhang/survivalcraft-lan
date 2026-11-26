using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class ThermalGeneratorVoltElement : VoltElement
{
	public Lazy<ComponentThermalGenerator?> m_componentThermalGenerator;

	public ThermalGeneratorVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentThermalGenerator = subsystemTerrain.GetComponentAtPointLazy<ComponentThermalGenerator>(position);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		ComponentThermalGenerator value = m_componentThermalGenerator.Value;
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
		ComponentThermalGenerator value = m_componentThermalGenerator.Value;
		if (value == null || !value.IsWorking)
		{
			return 0f;
		}
		return standardUL;
	}
}
