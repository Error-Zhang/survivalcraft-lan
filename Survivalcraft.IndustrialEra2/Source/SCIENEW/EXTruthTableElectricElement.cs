using System;
using Engine;
using Game;

namespace SCIENEW;

public class EXTruthTableElectricElement : RotateableElectricElement
{
	public SubsystemEXTruthTableBlockBehavior m_subsystemTruthTableCircuitBlockBehavior;

	public float m_voltage;

	public EXTruthTableElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
		: base(subsystemElectricity, cellFace)
	{
		m_subsystemTruthTableCircuitBlockBehavior = subsystemElectricity.Project.FindSubsystem<SubsystemEXTruthTableBlockBehavior>(throwOnError: true);
	}

	public override float GetOutputVoltage(int face)
	{
		return m_voltage;
	}

	public override bool Simulate()
	{
		float voltage = m_voltage;
		int rotation = Rotation;
		float i = 0f;
		float i2 = 0f;
		float i3 = 0f;
		float i4 = 0f;
		foreach (ElectricConnection connection in Connections)
		{
			if (connection.ConnectorType == ElectricConnectorType.Output || connection.NeighborConnectorType == ElectricConnectorType.Input)
			{
				continue;
			}
			ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
			if (connectorDirection.HasValue)
			{
				if (connectorDirection == ElectricConnectorDirection.Top)
				{
					i = connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f;
				}
				else if (connectorDirection == ElectricConnectorDirection.Right)
				{
					i2 = connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f;
				}
				else if (connectorDirection == ElectricConnectorDirection.Bottom)
				{
					i3 = connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f;
				}
				else if (connectorDirection == ElectricConnectorDirection.Left)
				{
					i4 = connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f;
				}
			}
		}
		EXTruthTableData blockData = m_subsystemTruthTableCircuitBlockBehavior.GetBlockData(CellFaces[0].Point);
		if (blockData != null)
		{
			if (!blockData.Calc(i, i2, i3, i4, out float result, out string calcState))
			{
				SubsystemElectricity.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true).FindNearestPlayer(new Vector3(CellFaces[0].Point)).ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get("EXTruthTableElectricElement", "Failed"), calcState), Color.White, blinking: true, playNotificationSound: false);
			}
			result = Math.Clamp(result, 0f, 15f);
			m_voltage = (float)Convert.ToInt32(result) / 15f;
		}
		return m_voltage != voltage;
	}
}
