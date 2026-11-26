using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class GalvanicCellVoltElement : VoltElement
{
	public float m_energy;

	public GalvanicCellVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_energy = 20f;
	}

	public override void Simulate(float dt)
	{
		base.Simulate(dt);
		m_energy = MathUtils.Max(m_energy - m_standardPower * dt * (float)CalcConnection(), 0f);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		if (!(m_energy > 0f))
		{
			return 0f;
		}
		return standardPL;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		if (!(m_energy > 0f))
		{
			return 0f;
		}
		return standardUL;
	}
}
