using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SubsystemFireBoxes : Subsystem
{
	public Dictionary<CellFace, IFireBoxComponent> m_fireBoxes = new Dictionary<CellFace, IFireBoxComponent>();

	public void Add(Point3 point, IFireBoxComponent engine)
	{
		for (int i = 0; i < 6; i++)
		{
			if (!m_fireBoxes.TryAdd(new CellFace(point.X, point.Y, point.Z, i), engine))
			{
				m_fireBoxes[new CellFace(point.X, point.Y, point.Z, i)] = engine;
			}
		}
	}

	public void Remove(Point3 point)
	{
		for (int i = 0; i < 6; i++)
		{
			m_fireBoxes.Remove(new CellFace(point.X, point.Y, point.Z, i));
		}
	}

	public void Add(CellFace cellFace, IFireBoxComponent engine)
	{
		if (!m_fireBoxes.TryAdd(cellFace, engine))
		{
			m_fireBoxes[cellFace] = engine;
		}
	}

	public void Remove(CellFace cellFace)
	{
		m_fireBoxes.Remove(cellFace);
	}

	public bool IsPreheated(Point3 position, out IFireBoxComponent engineComponent)
	{
		engineComponent = null;
		for (int i = 0; i < 6; i++)
		{
			Point3 point = position + CellFace.FaceToPoint3(i);
			int face = CellFace.OppositeFace(i);
			CellFace key = new CellFace(point.X, point.Y, point.Z, face);
			if (m_fireBoxes.TryGetValue(key, out IFireBoxComponent value) && value.IsPreheating())
			{
				engineComponent = value;
				return true;
			}
		}
		return false;
	}

	public bool IsPreheated(CellFace cellFace, out IFireBoxComponent engineComponent)
	{
		engineComponent = null;
		Point3 point = cellFace.Point + CellFace.FaceToPoint3(cellFace.Face);
		int face = CellFace.OppositeFace(cellFace.Face);
		if (m_fireBoxes.TryGetValue(new CellFace(point.X, point.Y, point.Z, face), out IFireBoxComponent value) && value.IsPreheating())
		{
			engineComponent = value;
			return true;
		}
		return false;
	}
}
