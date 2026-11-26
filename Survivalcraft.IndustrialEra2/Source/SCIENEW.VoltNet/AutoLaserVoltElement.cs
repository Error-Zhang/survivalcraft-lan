using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class AutoLaserVoltElement : VoltElement
{
	public Lazy<ComponentAutoLaser?> m_componentAutoLaser;

	public AutoLaserVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentAutoLaser = subsystemTerrain.GetComponentAtPointLazy<ComponentAutoLaser>(position);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		return 0f - standardPL;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return 0f - standardUL;
	}

	public override void Update(float dt, float inputVoltage, float inputPower)
	{
		base.Update(dt, inputVoltage, inputPower);
		if (m_componentAutoLaser.Value != null)
		{
			double energy = SubsystemLaser.ElectricPowerToEnergy(inputPower);
			m_componentAutoLaser.Value.m_energy = energy;
		}
	}
}
