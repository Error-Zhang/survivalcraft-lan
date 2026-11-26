using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SubsystemAirBlowers : Subsystem
{
	public Dictionary<CellFace, IAirBlowerComponent> m_airBlowers = new Dictionary<CellFace, IAirBlowerComponent>();

	public void Add(Point3 point, IAirBlowerComponent blower)
	{
		for (int i = 0; i < 6; i++)
		{
			CellFace key = new CellFace(point.X, point.Y, point.Z, i);
			if (!m_airBlowers.TryAdd(key, blower))
			{
				m_airBlowers[key] = blower;
			}
		}
	}

	public void Remove(Point3 point)
	{
		for (int i = 0; i < 6; i++)
		{
			m_airBlowers.Remove(new CellFace(point.X, point.Y, point.Z, i));
		}
	}

	public bool IsBlowing(Point3 position, out IAirBlowerComponent blowerComponent)
	{
		blowerComponent = null;
		for (int i = 0; i < 6; i++)
		{
			Point3 point = position + CellFace.FaceToPoint3(i);
			int face = CellFace.OppositeFace(i);
			CellFace key = new CellFace(point.X, point.Y, point.Z, face);
			if (m_airBlowers.TryGetValue(key, out IAirBlowerComponent value) && value.IsWorking())
			{
				blowerComponent = value;
				return true;
			}
		}
		return false;
	}
}
