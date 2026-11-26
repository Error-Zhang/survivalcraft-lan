using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class FluidInserterElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public FluidInserterElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
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
				ComponentFluidInserter componentFluidInserter = blockEntity.Entity.FindComponent<ComponentFluidInserter>();
				if (componentFluidInserter != null)
				{
					flag = componentFluidInserter.Insert();
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
