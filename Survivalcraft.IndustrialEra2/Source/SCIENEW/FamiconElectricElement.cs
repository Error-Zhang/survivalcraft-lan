using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class FamiconElectricElement : ElectricElement
{
	public SubsystemBlockEntities m_subsystemBlockEntities;

	public FamiconElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
		: base(subsystemElectricity, new List<CellFace>
		{
			new CellFace(point.X, point.Y, point.Z, 4),
			new CellFace(point.X, point.Y, point.Z, 5)
		})
	{
		m_subsystemBlockEntities = SubsystemElectricity.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
	}

	public override bool Simulate()
	{
		int num = 0;
		foreach (ElectricConnection connection in Connections)
		{
			if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != ElectricConnectorType.Input && connection.CellFace.Face == 5)
			{
				num |= (int)Math.Floor(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) * 15f);
			}
		}
		num &= 0xF;
		int num2 = 0;
		foreach (ElectricConnection connection2 in Connections)
		{
			if (connection2.ConnectorType != ElectricConnectorType.Output && connection2.NeighborConnectorType != ElectricConnectorType.Input && connection2.CellFace.Face == 4)
			{
				num2 |= (int)Math.Floor(connection2.NeighborElectricElement.GetOutputVoltage(connection2.NeighborConnectorFace) * 15f);
			}
		}
		num2 &= 0xF;
		byte input = (byte)((num << 4) | num2);
		(m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z)?.Entity.FindComponent<ComponentFamicon>())?.InputController.Input(input);
		return false;
	}
}
