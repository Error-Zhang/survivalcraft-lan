using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class InserterElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public InserterElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
		: base(subsystemElectricity, new List<CellFace>
		{
			new CellFace(point.X, point.Y, point.Z, 0),
			new CellFace(point.X, point.Y, point.Z, 1),
			new CellFace(point.X, point.Y, point.Z, 2),
			new CellFace(point.X, point.Y, point.Z, 3),
			new CellFace(point.X, point.Y, point.Z, 4),
			new CellFace(point.X, point.Y, point.Z, 5)
		})
	{
		m_subsystemBlockEntities = SubsystemElectricity.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
	}

	public override bool Simulate()
	{
		if (CalculateHighInputsCount() > 0 && (!m_lastDispenseTime.HasValue || SubsystemElectricity.SubsystemTime.GameTime - m_lastDispenseTime > 0.1))
		{
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z);
			bool flag = false;
			if (blockEntity != null)
			{
				ComponentInserter componentInserter = blockEntity.Entity.FindComponent<ComponentInserter>();
				if (componentInserter != null)
				{
					flag = componentInserter.Place();
				}
			}
			if (flag)
			{
				m_lastDispenseTime = SubsystemElectricity.SubsystemTime.GameTime;
			}
		}
		return false;
	}
}
