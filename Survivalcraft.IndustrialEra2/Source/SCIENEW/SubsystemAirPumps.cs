using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class SubsystemAirPumps : Subsystem
{
	public Dictionary<Point3, HashSet<ComponentAirPump>> m_cells = new Dictionary<Point3, HashSet<ComponentAirPump>>();

	public void Add(Point3 point, ComponentAirPump componentAirPump)
	{
		for (int i = 0; i < 6; i++)
		{
			Point3 key = point + CellFace.FaceToPoint3(i);
			if (m_cells.TryGetValue(key, out HashSet<ComponentAirPump> value))
			{
				value.Add(componentAirPump);
				continue;
			}
			m_cells[key] = new HashSet<ComponentAirPump> { componentAirPump };
		}
	}

	public void Remove(Point3 point, ComponentAirPump componentAirPump)
	{
		for (int i = 0; i < 6; i++)
		{
			Point3 key = point + CellFace.FaceToPoint3(i);
			if (m_cells[key].Count == 1)
			{
				m_cells.Remove(key);
			}
			else
			{
				m_cells[key].Remove(componentAirPump);
			}
		}
	}

	public int GetPointCount(Point3 point)
	{
		if (!m_cells.TryGetValue(point, out HashSet<ComponentAirPump> value))
		{
			return 0;
		}
		return Enumerable.Count(value, (ComponentAirPump x) => x.GetWorkState() == WorkState.Active);
	}
}
