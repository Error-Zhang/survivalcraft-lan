using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class WorkshopElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public SubsystemPickables m_subsystemPickables;

	public Vector3 m_position;

	public WorkshopElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
		: base(subsystemElectricity, new List<CellFace>
		{
			new CellFace(point.X, point.Y, point.Z, 4),
			new CellFace(point.X, point.Y, point.Z, 5)
		})
	{
		m_subsystemBlockEntities = SubsystemElectricity.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
		m_subsystemPickables = SubsystemElectricity.Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_position = new Vector3(point) + new Vector3(0.5f);
	}

	public override bool Simulate()
	{
		if (CalculateHighInputsCount(5) > 0)
		{
			if (m_isDispenseAllowed && (!m_lastDispenseTime.HasValue || SubsystemElectricity.SubsystemTime.GameTime - m_lastDispenseTime > 0.1))
			{
				m_isDispenseAllowed = false;
				m_lastDispenseTime = SubsystemElectricity.SubsystemTime.GameTime;
				(m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z)?.Entity.FindComponent<ComponentWorkshop>())?.PopResults();
			}
		}
		else
		{
			m_isDispenseAllowed = true;
		}
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		foreach (ElectricConnection connection in Connections)
		{
			if (connection.ConnectorType == ElectricConnectorType.Output || connection.NeighborConnectorType == ElectricConnectorType.Input || connection.CellFace.Face != 4)
			{
				continue;
			}
			ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(CellFaces[0].Face, 0, connection.ConnectorFace);
			if (connectorDirection.HasValue)
			{
				switch (connectorDirection)
				{
				case ElectricConnectorDirection.Bottom:
					num4 = (int)MathF.Floor(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
					break;
				case ElectricConnectorDirection.Top:
					num3 = (int)MathF.Floor(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
					break;
				case ElectricConnectorDirection.Left:
					num = (int)MathF.Floor(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
					break;
				case ElectricConnectorDirection.Right:
					num2 = (int)MathF.Floor(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
					break;
				}
			}
		}
		int num5 = num + num2;
		int num6 = num3 + num4;
		ComponentWorkshop componentWorkshop = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z)?.Entity.FindComponent<ComponentWorkshop>();
		if (componentWorkshop != null)
		{
			if (num5 < 0 && num6 < 0)
			{
				componentWorkshop.RequireItemSlotIndex = null;
			}
			else
			{
				if (num5 < 0)
				{
					num5 = 0;
				}
				if (num6 < 0)
				{
					num6 = 0;
				}
				num5 = Math.Clamp(num5, 0, 5);
				num6 = Math.Clamp(num6, 0, 5);
				componentWorkshop.RequireItemSlotIndex = num5 * 5 + num6;
			}
		}
		return false;
	}

	public int CalculateHighInputsCount(int face)
	{
		int num = 0;
		foreach (ElectricConnection connection in Connections)
		{
			if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != ElectricConnectorType.Input && connection.CellFace.Face == face && ElectricElement.IsSignalHigh(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace)))
			{
				num++;
			}
		}
		return num;
	}
}
