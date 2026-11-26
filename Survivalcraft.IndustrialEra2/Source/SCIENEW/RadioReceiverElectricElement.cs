using System.Collections.Generic;
using Game;

namespace SCIENEW;

public class RadioReceiverElectricElement : ElectricElement
{
	public SubsystemRadioElectricElements m_subsystem;

	public int m_left;

	public int m_bottom;

	public int m_right;

	public int m_top;

	public int m_in;

	protected uint m_frequency = 1000u;

	public CellFace m_cellFace;

	protected bool m_enable;

	public uint Frequency
	{
		get
		{
			return m_frequency;
		}
		set
		{
			if (m_subsystem != null)
			{
				SubsystemElectricity.QueueElectricElementForSimulation(this, SubsystemElectricity.CircuitStep + 1);
				m_subsystem.ChangeFrequency(this, m_frequency, value);
			}
			m_frequency = value;
		}
	}

	public bool Enable
	{
		get
		{
			return m_enable;
		}
		set
		{
			if (value != m_enable)
			{
				SubsystemElectricity.QueueElectricElementForSimulation(this, SubsystemElectricity.CircuitStep + 1);
			}
			m_enable = value;
		}
	}

	public RadioReceiverElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
		: base(subsystemElectricity, cellFace)
	{
		m_subsystem = subsystemElectricity.Project.FindSubsystem<SubsystemRadioElectricElements>(throwOnError: true);
		m_cellFace = cellFace;
	}

	public override float GetOutputVoltage(int face)
	{
		return SubsystemElectricity.GetConnectorDirection(CellFaces[0].Face, 0, face) switch
		{
			ElectricConnectorDirection.Bottom => (float)m_bottom / 15f,
			ElectricConnectorDirection.Left => (float)m_left / 15f,
			ElectricConnectorDirection.Right => (float)m_right / 15f,
			ElectricConnectorDirection.In => (float)m_in / 15f,
			ElectricConnectorDirection.Top => (float)m_top / 15f,
			_ => (float)m_top / 15f,
		};
	}

	public override void OnAdded()
	{
		base.OnAdded();
		if (m_subsystem.FrequenciesInFile.TryGetValue(m_cellFace.Point, out var value))
		{
			m_frequency = value;
		}
		if (m_subsystem.EnablesInFile.TryGetValue(m_cellFace.Point, out var value2))
		{
			m_enable = value2;
		}
		m_subsystem.Add(m_frequency, this);
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		m_subsystem.Remove(m_frequency, this);
	}

	public override bool Simulate()
	{
		int top = m_top;
		int bottom = m_bottom;
		int left = m_left;
		int right = m_right;
		int num = m_in;
		m_top = (m_bottom = (m_right = (m_left = (m_in = 0))));
		if (m_enable && m_subsystem.GetTransmitter(m_frequency, out List<RadioTransmitterElectricElement> transmitters))
		{
			foreach (RadioTransmitterElectricElement item in transmitters)
			{
				m_top |= item.m_top;
				m_bottom |= item.m_bottom;
				m_left |= item.m_left;
				m_right |= item.m_right;
				m_in |= item.m_in;
			}
		}
		if (m_top == top && m_left == left && m_right == right && m_bottom == bottom)
		{
			return m_in != num;
		}
		return true;
	}
}
