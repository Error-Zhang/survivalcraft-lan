using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class AF_BoxBoobElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public SubsystemExplosions m_subsystemExplosions;

	public AF_BoxBoobElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
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
		m_subsystemExplosions = SubsystemElectricity.Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
	}

	public override bool Simulate()
	{
		if (CalculateHighInputsCount() > 0)
		{
			if (m_isDispenseAllowed && (!m_lastDispenseTime.HasValue || SubsystemElectricity.SubsystemTime.GameTime - m_lastDispenseTime > 0.1))
			{
				m_isDispenseAllowed = false;
				m_lastDispenseTime = SubsystemElectricity.SubsystemTime.GameTime;
				ComponentPlayer componentPlayer = m_subsystemBlockEntities.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true).FindNearestPlayer(new Vector3(CellFaces[0].Point));
				foreach (Entity entity in m_subsystemBlockEntities.Project.Entities)
				{
					if (entity.FindComponent<ComponentCreature>() != null)
					{
						entity.FindComponent<ComponentBody>().Position = componentPlayer.ComponentBody.Position;
					}
				}
			}
		}
		else
		{
			m_isDispenseAllowed = true;
		}
		return false;
	}
}
