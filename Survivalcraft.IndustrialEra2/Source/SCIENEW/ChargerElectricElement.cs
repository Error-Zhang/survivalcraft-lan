using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class ChargerElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public ChargerElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
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
		ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z);
		if (blockEntity != null)
		{
			ComponentCharger componentCharger = blockEntity.Entity.FindComponent<ComponentCharger>();
			if (componentCharger.ElectricalControl)
			{
				componentCharger.Mode = ((CalculateHighInputsCount() > 0) ? ComponentCondenser.Mode.Discharge : ComponentCondenser.Mode.Charge);
			}
		}
		return false;
	}
}
