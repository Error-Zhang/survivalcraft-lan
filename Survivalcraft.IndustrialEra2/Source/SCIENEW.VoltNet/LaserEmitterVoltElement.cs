using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class LaserEmitterVoltElement : VoltElement
{
	public Lazy<ComponentLaserEmitter?> m_componentLaserEmitter;

	public LaserEmitterVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_componentLaserEmitter = subsystemTerrain.GetComponentAtPointLazy<ComponentLaserEmitter>(position);
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
		if (m_componentLaserEmitter.Value != null)
		{
			double energy = SubsystemLaser.ElectricPowerToEnergy(inputPower);
			m_componentLaserEmitter.Value.m_energy = energy;
		}
	}
}
