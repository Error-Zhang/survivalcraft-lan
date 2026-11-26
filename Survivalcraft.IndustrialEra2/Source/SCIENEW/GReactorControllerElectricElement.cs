using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class GReactorControllerElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public Vector3 m_position;

	public float m_output;

	public ComponentGReactorController? m_controller;

	public GReactorControllerElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
		: base(subsystemElectricity, new List<CellFace>
		{
			new CellFace(point.X, point.Y, point.Z, 4),
			new CellFace(point.X, point.Y, point.Z, 5)
		})
	{
		m_subsystemBlockEntities = SubsystemElectricity.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
		m_position = new Vector3(point) + new Vector3(0.5f);
	}

	public override float GetOutputVoltage(int face)
	{
		return m_output;
	}

	public override bool Simulate()
	{
		if (m_controller == null)
		{
			m_controller = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z)?.Entity.FindComponent<ComponentGReactorController>();
		}
		if (CalculateHighInputsCount(4, out var inputValue) > 0)
		{
			if (m_isDispenseAllowed && (!m_lastDispenseTime.HasValue || SubsystemElectricity.SubsystemTime.GameTime - m_lastDispenseTime > 0.1))
			{
				m_isDispenseAllowed = false;
				m_lastDispenseTime = SubsystemElectricity.SubsystemTime.GameTime;
				if (m_controller != null)
				{
					int operation = (int)MathF.Round(inputValue * 15f);
					Operate(m_controller, operation);
				}
			}
		}
		else
		{
			m_isDispenseAllowed = true;
		}
		float output = m_output;
		if (m_controller != null)
		{
			m_output = MathF.Round(m_controller.m_coreHeatLevel / 3000f * 15f) / 15f;
		}
		SubsystemElectricity.QueueElectricElementForSimulation(this, SubsystemElectricity.CircuitStep + 1);
		return output != m_output;
	}

	public void Operate(ComponentGReactorController controller, int operation)
	{
		switch (operation)
		{
		case 8:
			controller.fr1 = true;
			break;
		case 9:
			controller.fr0 = true;
			break;
		case 10:
			controller.cr1 = true;
			break;
		case 11:
			controller.cr0 = true;
			break;
		case 12:
			controller.gr1 = true;
			break;
		case 13:
			controller.gr0 = true;
			break;
		case 14:
		case 15:
			controller.az5 = true;
			break;
		}
	}

	public int CalculateHighInputsCount(int face, out float inputValue)
	{
		int num = 0;
		inputValue = 0f;
		foreach (ElectricConnection connection in Connections)
		{
			float outputVoltage = connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
			if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != ElectricConnectorType.Input && connection.CellFace.Face == face && ElectricElement.IsSignalHigh(outputVoltage))
			{
				inputValue += outputVoltage;
				num++;
			}
		}
		return num;
	}
}
