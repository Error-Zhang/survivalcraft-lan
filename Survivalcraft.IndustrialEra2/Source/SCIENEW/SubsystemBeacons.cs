using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.Graphics;

namespace SCIENEW;

public class SubsystemBeacons : Subsystem, IDrawable
{
	public struct RectanglePoints(Point3 point1, Point3 point2, Point3 point3, Point3 point4)
	{
		public Point3 Point1 = point1;

		public Point3 Point2 = point2;

		public Point3 Point3 = point3;

		public Point3 Point4 = point4;

		public Point3 MinPoint => new Point3(Math.Min(Point1.X, Math.Min(Point2.X, Math.Min(Point3.X, Point4.X))), Math.Min(Point1.Y, Math.Min(Point2.Y, Math.Min(Point3.Y, Point4.Y))), Math.Min(Point1.Z, Math.Min(Point2.Z, Math.Min(Point3.Z, Point4.Z))));

		public Point3 MaxPoint => new Point3(Math.Max(Point1.X, Math.Max(Point2.X, Math.Max(Point3.X, Point4.X))), Math.Max(Point1.Y, Math.Max(Point2.Y, Math.Max(Point3.Y, Point4.Y))), Math.Max(Point1.Z, Math.Max(Point2.Z, Math.Max(Point3.Z, Point4.Z))));

		public override int GetHashCode()
		{
			return HashCode.Combine(Point1, Point2, Point3, Point4);
		}

		public BoundingBox GetBoundingBox()
		{
			return new BoundingBox(new _003C_003Ez__ReadOnlyArray<Vector3>(new Vector3[8]
			{
				new Vector3(Point1) + Vector3.One * 0.5f - Vector3.UnitY * 0.5f,
				new Vector3(Point2) + Vector3.One * 0.5f - Vector3.UnitY * 0.5f,
				new Vector3(Point3) + Vector3.One * 0.5f - Vector3.UnitY * 0.5f,
				new Vector3(Point4) + Vector3.One * 0.5f - Vector3.UnitY * 0.5f,
				new Vector3(Point1) + Vector3.One * 0.5f + Vector3.UnitY * 0.5f,
				new Vector3(Point2) + Vector3.One * 0.5f + Vector3.UnitY * 0.5f,
				new Vector3(Point3) + Vector3.One * 0.5f + Vector3.UnitY * 0.5f,
				new Vector3(Point4) + Vector3.One * 0.5f + Vector3.UnitY * 0.5f
			}));
		}

		public BoundingBox GetOutRangeBoundingBox()
		{
			return new BoundingBox(new Vector3(MinPoint), new Vector3(MaxPoint) + Vector3.One);
		}

		public override string ToString()
		{
			return $"({Point1}) ({Point2})\n({Point3}) ({Point4})";
		}
	}

	public HashSet<Point3> m_beacons = new HashSet<Point3>();

	public List<RectanglePoints> m_areas = new List<RectanglePoints>();

	public Dictionary<Point3, RectanglePoints> m_pointsToAreas = new Dictionary<Point3, RectanglePoints>();

	public PrimitivesRenderer3D m_p3d = new PrimitivesRenderer3D();

	public int[] DrawOrders => new int[1] { 1500 };

	public void AddPoint(Point3 point)
	{
		m_beacons.Add(point);
		RefreshAreas();
	}

	public void RemovePoint(Point3 point)
	{
		m_beacons.Remove(point);
		RefreshAreas();
	}

	public void GetRectangle(Point3 point, out RectanglePoints? rectangle)
	{
		for (int i = 0; i < 4; i++)
		{
			Point3 key = point + CellFace.FaceToPoint3(i);
			if (m_pointsToAreas.TryGetValue(key, out var value))
			{
				rectangle = value;
				return;
			}
		}
		rectangle = null;
	}

	public void RefreshAreas()
	{
		m_areas.Clear();
		m_pointsToAreas.Clear();
		HashSet<Point3> hashSet = new HashSet<Point3>();
		foreach (Point3 beacon in m_beacons)
		{
			if (hashSet.Contains(beacon))
			{
				continue;
			}
			foreach (Point3 beacon2 in m_beacons)
			{
				if (!hashSet.Contains(beacon2) && beacon.X != beacon2.X && beacon.Z != beacon2.Z && Math.Abs(beacon.X - beacon2.X) <= 64 && Math.Abs(beacon.Z - beacon2.Z) <= 64 && beacon.Y == beacon2.Y)
				{
					Point3 point = new Point3(beacon.X, beacon.Y, beacon2.Z);
					Point3 point2 = new Point3(beacon2.X, beacon2.Y, beacon.Z);
					if (m_beacons.Contains(point) && m_beacons.Contains(point2) && beacon.Y == point.Y && beacon.Y == point2.Y && !hashSet.Contains(point) && !hashSet.Contains(point2))
					{
						RectanglePoints rectanglePoints = new RectanglePoints(beacon, point, point2, beacon2);
						m_areas.Add(rectanglePoints);
						m_pointsToAreas[beacon] = rectanglePoints;
						m_pointsToAreas[beacon2] = rectanglePoints;
						m_pointsToAreas[point] = rectanglePoints;
						m_pointsToAreas[point2] = rectanglePoints;
						hashSet.Add(beacon);
						hashSet.Add(beacon2);
						hashSet.Add(point);
						hashSet.Add(point2);
					}
				}
			}
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		foreach (RectanglePoints area in m_areas)
		{
			m_p3d.FlatBatch(0, DepthStencilState.None).QueueBoundingBoxFace(area.GetBoundingBox(), Color.Yellow);
		}
		m_p3d.Flush(camera.ViewProjectionMatrix);
	}
}
