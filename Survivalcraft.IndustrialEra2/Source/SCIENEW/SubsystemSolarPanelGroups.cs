using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Engine.Media;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.VoltNet;
using TemplatesDatabase;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW;

public class SubsystemSolarPanelGroups : Subsystem, IDrawable, IUpdateable
{
	public struct Cell : IEquatable<Cell>
	{
		public int X;

		public int Y;

		public int Z;

		public int Value;

		public bool Equals(Cell other)
		{
			if (X == other.X && Y == other.Y && Z == other.Z)
			{
				return Value == other.Value;
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is Cell other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y, Z, Value);
		}

		public static bool operator ==(Cell left, Cell right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Cell left, Cell right)
		{
			return !left.Equals(right);
		}

		public Cell(int x, int y, int z, int value)
		{
			X = x;
			Y = y;
			Z = z;
			Value = value;
		}
	}

	public class Base
	{
		public Dictionary<Cell, SolarPanelVoltElement> m_panelElements = new Dictionary<Cell, SolarPanelVoltElement>();

		public Project Project;

		private SubsystemTerrain m_subsystemTerrain;

		public int ID => GetHashCode();

		public int ControlledCount => m_panelElements.Count;

		public int ConnectionCount => m_panelElements.AsValueEnumerable().Count((KeyValuePair<Cell, SolarPanelVoltElement> x) => x.Value.Connected());

		public float AverageLight { get; set; }

		public Base(Dictionary<Cell, SolarPanelVoltElement> voltElements, Project project)
		{
			m_panelElements = voltElements;
			Project = project;
			m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		}

		public float GetCurrentPL(float standardPL)
		{
			return standardPL * AverageLight * (float)ControlledCount / (float)ConnectionCount;
		}

		public void UpdateAverageLight()
		{
			int num = 15 * ControlledCount;
			int num2 = 0;
			foreach (Cell key in m_panelElements.Keys)
			{
				if (m_subsystemTerrain.m_subsystemsky.SkyLightValue >= 10 && m_subsystemTerrain.Terrain.GetCellLight(key.X, key.Y + 1, key.Z) >= 10)
				{
					num2 += m_subsystemTerrain.Terrain.GetCellLight(key.X, key.Y + 1, key.Z);
				}
			}
			AverageLight = (float)num2 / (float)num;
		}
	}

	public Dictionary<Cell, Base> m_cellToBase = new Dictionary<Cell, Base>();

	public Dictionary<Point2, HashSet<Base>> m_chunkToBase = new Dictionary<Point2, HashSet<Base>>();

	private PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	private FlatBatch3D m_flatBath;

	private FontBatch3D m_textBatch;

	private bool m_debugCanDraw;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1] { 1000 };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_flatBath = m_primitivesRenderer3D.FlatBatch(0, DepthStencilState.None);
		m_textBatch = m_primitivesRenderer3D.FontBatch(BitmapFont.DebugFont, 0, DepthStencilState.None);
	}

	public bool IsThereABase(Cell cell)
	{
		return m_cellToBase.ContainsKey(cell);
	}

	public Base GetBase(Cell cell)
	{
		if (!m_cellToBase.ContainsKey(cell))
		{
			return null;
		}
		return m_cellToBase[cell];
	}

	public Cell[] GetCells(Base solarBase)
	{
		List<Cell> list = new List<Cell>();
		foreach (KeyValuePair<Cell, Base> item in m_cellToBase)
		{
			if (item.Value == solarBase)
			{
				list.Add(item.Key);
			}
		}
		return list.ToArray();
	}

	public Cell[] GetCellsByLinq(Base solarBase)
	{
		List<Cell> list = new List<Cell>();
		list.AddRange((from x in m_cellToBase.AsValueEnumerable()
			where x.Value == solarBase
			select x).Select<FromDictionary<Cell, Base>, KeyValuePair<Cell, Base>, Cell>((Func<KeyValuePair<Cell, Base>, Cell>)((KeyValuePair<Cell, Base> x) => x.Key)).ToArray<FromDictionary<Cell, Base>, KeyValuePair<Cell, Base>, Cell>());
		return list.ToArray();
	}

	public void AddBaseByPoints(HashSet<Cell> cells, Base solarBase)
	{
		HashSet<Point2> hashSet = new HashSet<Point2>();
		foreach (Cell cell in cells)
		{
			m_cellToBase[cell] = solarBase;
			Point2 item = Terrain.ToChunk(cell.X, cell.Z);
			hashSet.Add(item);
		}
		foreach (Point2 item2 in hashSet)
		{
			if (!m_chunkToBase.TryGetValue(item2, out HashSet<Base> value) || value == null)
			{
				m_chunkToBase[item2] = new HashSet<Base> { solarBase };
			}
			else
			{
				value.Add(solarBase);
			}
		}
		solarBase.UpdateAverageLight();
	}

	public void RemoveBase(Base solarBase)
	{
		Cell[] cells = GetCells(solarBase);
		for (int i = 0; i < cells.Length; i++)
		{
			Cell key = cells[i];
			m_cellToBase.Remove(key);
			Point2 key2 = Terrain.ToChunk(key.X, key.Z);
			if (m_chunkToBase.TryGetValue(key2, out HashSet<Base> value))
			{
				value.Remove(solarBase);
				if (value.Count == 0)
				{
					m_chunkToBase.Remove(key2);
				}
			}
		}
	}

	public int GetBasesCount()
	{
		HashSet<Base> hashSet = new HashSet<Base>();
		foreach (Base value in m_cellToBase.Values)
		{
			hashSet.Add(value);
		}
		return hashSet.Count;
	}

	public void UpdateLightInChunk(TerrainChunk chunk)
	{
		foreach (KeyValuePair<Point2, HashSet<Base>> item in m_chunkToBase)
		{
			if (item.Key != chunk.Coords)
			{
				continue;
			}
			foreach (Base item2 in item.Value)
			{
				item2.UpdateAverageLight();
			}
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (!m_debugCanDraw)
		{
			return;
		}
		foreach (KeyValuePair<Cell, Base> item in m_cellToBase)
		{
			m_flatBath.QueueBoundingBox(new BoundingBox(new Vector3(item.Key.X, item.Key.Y, item.Key.Z), new Vector3(item.Key.X, item.Key.Y, item.Key.Z) + Vector3.One), Color.DarkCyan);
			Vector3 vector = camera.ViewDirection.Cross(Vector3.UnitY).Normalize();
			Vector3 vector2 = -vector.Cross(camera.ViewDirection).Normalize();
			float num = 0.006f;
			m_textBatch.QueueText(item.Value.ID + "\r\n" + GetBasesCount(), new Vector3(item.Key.X, item.Key.Y, item.Key.Z) + new Vector3(0.5f), vector * num, vector2 * num, Color.White, TextAnchor.Center, Vector2.Zero);
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}

	public void Update(float dt)
	{
		if (Keyboard.IsKeyDownOnce(Key.F5))
		{
			m_debugCanDraw = !m_debugCanDraw;
		}
	}
}
