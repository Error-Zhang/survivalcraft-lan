using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;

namespace SCIENEW;

public class SubsystemUpdrafts : Subsystem
{
	public HashSet<CellFace> m_updrafts = new HashSet<CellFace>();

	public void AddUpdraft(CellFace cellFace)
	{
		m_updrafts.Add(cellFace);
	}

	public void RemoveUpdraft(CellFace cellFace)
	{
		m_updrafts.Remove(cellFace);
	}

	public Vector3 GetForce(Vector3 position)
	{
		foreach (CellFace updraft in m_updrafts)
		{
			float num = (new Vector3(updraft.Point) + new Vector3(0.5f) - position).Length();
			if (!(num > 12f))
			{
				Point3 p = Terrain.ToCell(position) - updraft.Point;
				Point3 point = CellFace.FaceToPoint3(updraft.Face);
				int num2 = Math.Abs(p.X) + Math.Abs(p.Y) + Math.Abs(p.Z);
				int num3 = p.X * point.X + p.Y * point.Y + p.Z * point.Z;
				if (num2 == num3)
				{
					return 1f / num * new Vector3(p).Normalize();
				}
			}
		}
		return Vector3.Zero;
	}
}
