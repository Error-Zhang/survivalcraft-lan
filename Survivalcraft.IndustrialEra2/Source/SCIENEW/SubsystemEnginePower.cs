using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SubsystemEnginePower : Subsystem
{
	public Dictionary<CellFace, IEngineComponent> m_engines = new Dictionary<CellFace, IEngineComponent>();

	public void Add(Point3 point, IEngineComponent engine)
	{
		for (int i = 0; i < 6; i++)
		{
			if (!m_engines.TryAdd(new CellFace(point.X, point.Y, point.Z, i), engine))
			{
				m_engines[new CellFace(point.X, point.Y, point.Z, i)] = engine;
			}
		}
	}

	public void Remove(Point3 point)
	{
		for (int i = 0; i < 6; i++)
		{
			m_engines.Remove(new CellFace(point.X, point.Y, point.Z, i));
		}
	}

	public void Add(CellFace cellFace, IEngineComponent engine)
	{
		if (!m_engines.TryAdd(cellFace, engine))
		{
			m_engines[cellFace] = engine;
		}
	}

	public void Remove(CellFace cellFace)
	{
		m_engines.Remove(cellFace);
	}

	public bool IsPowered(Point3 position, out IEngineComponent engineComponent)
	{
		engineComponent = null;
		for (int i = 0; i < 6; i++)
		{
			Point3 point = position + CellFace.FaceToPoint3(i);
			int face = CellFace.OppositeFace(i);
			CellFace key = new CellFace(point.X, point.Y, point.Z, face);
			if (m_engines.TryGetValue(key, out IEngineComponent value) && value.IsPowering())
			{
				engineComponent = value;
				return true;
			}
		}
		return false;
	}

	public bool IsPowered(CellFace cellFace, out IEngineComponent engineComponent)
	{
		engineComponent = null;
		Point3 point = cellFace.Point + CellFace.FaceToPoint3(cellFace.Face);
		int face = CellFace.OppositeFace(cellFace.Face);
		if (m_engines.TryGetValue(new CellFace(point.X, point.Y, point.Z, face), out IEngineComponent value) && value.IsPowering())
		{
			engineComponent = value;
			return true;
		}
		return false;
	}
}
