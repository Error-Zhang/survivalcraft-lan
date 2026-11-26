using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class MachineToolElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public SubsystemPickables m_subsystemPickables;

	public Vector3 m_position;

	public MachineToolElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
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
				ComponentIECraftingTable componentIECraftingTable = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z)?.Entity.FindComponent<ComponentIECraftingTable>();
				if (componentIECraftingTable != null)
				{
					int slotValue = componentIECraftingTable.GetSlotValue(componentIECraftingTable.ResultSlotIndex);
					int num = componentIECraftingTable.RemoveSlotItems(componentIECraftingTable.ResultSlotIndex, componentIECraftingTable.GetSlotCount(componentIECraftingTable.ResultSlotIndex));
					if (num > 0)
					{
						m_subsystemPickables.AddPickable(slotValue, num, m_position + Vector3.UnitY * 0.7f, Vector3.UnitY * 4f, null);
					}
					int slotValue2 = componentIECraftingTable.GetSlotValue(componentIECraftingTable.RemainsSlotIndex);
					int num2 = componentIECraftingTable.RemoveSlotItems(componentIECraftingTable.RemainsSlotIndex, componentIECraftingTable.GetSlotCount(componentIECraftingTable.RemainsSlotIndex));
					if (num2 > 0)
					{
						m_subsystemPickables.AddPickable(slotValue2, num2, m_position + Vector3.UnitY * 0.7f, Vector3.UnitY * 4f, null);
					}
				}
			}
		}
		else
		{
			m_isDispenseAllowed = true;
		}
		float num3 = -1f;
		foreach (ElectricConnection connection in Connections)
		{
			if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != ElectricConnectorType.Input && connection.CellFace.Face == 4)
			{
				num3 = MathUtils.Max(connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), num3);
			}
		}
		ComponentIECraftingTable componentIECraftingTable2 = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z)?.Entity.FindComponent<ComponentIECraftingTable>();
		if (componentIECraftingTable2 != null)
		{
			if (num3 < 0f)
			{
				componentIECraftingTable2.RequireItemSlotIndex = null;
			}
			else
			{
				int value = (int)MathF.Round(num3 * 15f);
				componentIECraftingTable2.RequireItemSlotIndex = value;
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
