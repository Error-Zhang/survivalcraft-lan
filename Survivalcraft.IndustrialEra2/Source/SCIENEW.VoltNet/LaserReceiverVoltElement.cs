using System;
using Engine;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.VoltNet;

public class LaserReceiverVoltElement : VoltElement
{
	public readonly Lazy<ComponentLaserReceiver?> m_component;

	public LaserReceiverVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_component = subsystemTerrain.GetComponentAtPointLazy<ComponentLaserReceiver>(position);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		if (m_component.Value == null)
		{
			return 0f;
		}
		return SubsystemLaser.EnergyToElectricPower(m_component.Value.GetEnergy() / 1.2000000476837158);
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return standardUL;
	}
}
