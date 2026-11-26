using System;
using Engine;
using Game;
using SCIENEW.Extensions;
using SCIENEW.Utils;

namespace SCIENEW;

public class AutoLaserElectricElement : ElectricElement
{
	public float m_left;

	public float m_bottom;

	public float m_right;

	public float m_top;

	public ComponentAutoLaser m_component;

	public AutoLaserElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
		: base(subsystemElectricity, cellFace)
	{
	}

	public override float GetOutputVoltage(int face)
	{
		return SubsystemElectricity.GetConnectorDirection(CellFaces[0].Face, 0, face) switch
		{
			ElectricConnectorDirection.Bottom => m_bottom,
			ElectricConnectorDirection.Left => m_left,
			ElectricConnectorDirection.Right => m_right,
			ElectricConnectorDirection.Top => m_top,
			_ => m_top,
		};
	}

	public override bool Simulate()
	{
		float top = m_top;
		float bottom = m_bottom;
		float left = m_left;
		float right = m_right;
		m_top = (m_bottom = (m_right = (m_left = 0f)));
		if (m_component == null)
		{
			if (BlockEntityUtils.GetBlockEntity(SubsystemElectricity.SubsystemTerrain, CellFaces[0].Point, out ComponentBlockEntity blockEntity))
			{
				m_component = blockEntity.Entity.FindComponent<ComponentAutoLaser>(throwOnError: true);
			}
		}
		else
		{
			Vector2 xZ = m_component.TargetDirection.XZ;
			m_top = xZ.Length() / 40f;
			if (xZ.Length() == 0f)
			{
				m_left = (m_right = (m_bottom = 0f));
			}
			else
			{
				float num = xZ.AngleDifference(Vector2.UnitY);
				int num2 = 360 - (int)MathF.Round(num * 180f / (float)Math.PI);
				m_left = (float)(num2 / 100) / 15f;
				m_bottom = (float)(num2 % 100 / 10) / 15f;
				m_right = (float)(num2 % 10) / 15f;
			}
		}
		SubsystemElectricity.QueueElectricElementForSimulation(this, SubsystemElectricity.CircuitStep + 1);
		if (m_top == top && m_left == left && m_right == right)
		{
			return m_bottom != bottom;
		}
		return true;
	}
}
