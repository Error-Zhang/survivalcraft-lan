using System;
using Game;

namespace SCIENEW;

public class RadioTransmitterElectricElement : ElectricElement
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
				m_subsystem.QueueReceiversSimulate(value);
				m_subsystem.QueueReceiversSimulate(m_frequency);
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

	public RadioTransmitterElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
		: base(subsystemElectricity, cellFace)
	{
		m_subsystem = subsystemElectricity.Project.FindSubsystem<SubsystemRadioElectricElements>(throwOnError: true);
		m_cellFace = cellFace;
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
		m_subsystem.QueueReceiversSimulate(m_frequency);
		m_subsystem.Remove(m_frequency, this);
	}

	public override bool Simulate()
	{
		int top = m_top;
		int bottom = m_bottom;
		int left = m_left;
		int right = m_right;
		int num = m_in;
		if (!m_enable)
		{
			m_in = (m_bottom = (m_right = (m_left = (m_top = 0))));
		}
		else
		{
			foreach (ElectricConnection connection in Connections)
			{
				if (connection.ConnectorType == ElectricConnectorType.Output || connection.NeighborConnectorType == ElectricConnectorType.Input)
				{
					continue;
				}
				ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(CellFaces[0].Face, 0, connection.ConnectorFace);
				if (connectorDirection.HasValue)
				{
					switch (connectorDirection)
					{
					case ElectricConnectorDirection.Bottom:
						m_bottom = (int)MathF.Round(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
						break;
					case ElectricConnectorDirection.In:
						m_in = (int)MathF.Round(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
						break;
					case ElectricConnectorDirection.Left:
						m_left = (int)MathF.Round(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
						break;
					case ElectricConnectorDirection.Right:
						m_right = (int)MathF.Round(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
						break;
					case ElectricConnectorDirection.Top:
						m_top = (int)MathF.Round(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
						break;
					}
				}
			}
		}
		if (m_top != top || m_left != left || m_right != right || m_bottom != bottom || m_in != num)
		{
			m_subsystem.QueueReceiversSimulate(m_frequency);
		}
		return false;
	}
}
