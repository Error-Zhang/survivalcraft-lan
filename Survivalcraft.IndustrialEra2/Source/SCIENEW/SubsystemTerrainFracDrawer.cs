using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemTerrainFracDrawer : Subsystem, IUpdateable, IDrawable
{
	public SubsystemPlayers m_subsystemPlayers;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public Dictionary<Point2, float> m_temperaturePoints = new Dictionary<Point2, float>();

	public float m_minTemperature;

	public float m_maxTemperature;

	public bool m_displayTemperature;

	public Dictionary<Point2, float> m_windPoints = new Dictionary<Point2, float>();

	public float m_minWind;

	public float m_maxWind;

	public bool m_displayWind;

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public int m_basicY = 60;

	public int m_range = 8;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1] { 2000 };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
	}

	public void Update(float dt)
	{
		if (m_subsystemTime.PeriodicGameTimeEvent(1.0, 0.0))
		{
			UpdateTemperatures();
			UpdateWind();
		}
		if (Keyboard.IsKeyDownOnce(Key.F5))
		{
			m_displayTemperature = !m_displayTemperature;
		}
		if (Keyboard.IsKeyDownOnce(Key.F6))
		{
			m_displayWind = !m_displayWind;
		}
	}

	public void UpdateTemperatures()
	{
		m_temperaturePoints.Clear();
		m_minTemperature = float.MaxValue;
		m_maxTemperature = float.MinValue;
		foreach (PlayerData playersDatum in m_subsystemPlayers.PlayersData)
		{
			if (playersDatum.ComponentPlayer?.ComponentBody is not ComponentBody body)
			{
				continue;
			}
			Vector3 position = body.Position;
			TerrainChunk chunk = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(position.X), Terrain.ToCell(position.Z));
			if (chunk == null)
			{
				continue;
			}
			Point2 origin = chunk.Origin;
			float y = position.Y;
			for (int i = -m_range; i < m_range; i++)
			{
				for (int j = -m_range; j < m_range; j++)
				{
					Point3 position2 = new Point3(origin.X + i * 16, Terrain.ToCell(y), origin.Y + j * 16);
					Point2 sample = new Point2(position2.X, position2.Z);
					if (m_temperaturePoints.ContainsKey(sample))
					{
						continue;
					}
					float averageTemperatureAtChunk = TerrainUtils.GetAverageTemperatureAtChunk(m_subsystemTerrain, position2, out _);
					m_temperaturePoints.Add(sample, averageTemperatureAtChunk);
					m_minTemperature = Math.Min(m_minTemperature, averageTemperatureAtChunk);
					m_maxTemperature = Math.Max(m_maxTemperature, averageTemperatureAtChunk);
				}
			}
		}
	}

	public void UpdateWind()
	{
		m_windPoints.Clear();
		m_maxWind = float.MinValue;
		m_minWind = float.MaxValue;
		foreach (KeyValuePair<Point2, float> temperaturePoint in m_temperaturePoints)
		{
			Point2 key = temperaturePoint.Key;
			float value = temperaturePoint.Value;
			Point2[] array = new Point2[2]
			{
				new Point2(key.X + 16, key.Y),
				new Point2(key.X, key.Y + 16)
			};
			int num = 0;
			float num2 = 0f;
			Point2[] array2 = array;
			foreach (Point2 key2 in array2)
			{
				if (m_temperaturePoints.TryGetValue(key2, out var value2))
				{
					num++;
					float num3 = value2 - value;
					num2 += num3;
				}
			}
			if (num > 0)
			{
				float num4 = num2 / (float)num;
				m_maxWind = Math.Max(m_maxWind, num4);
				m_minWind = Math.Min(m_minWind, num4);
				m_windPoints.TryAdd(key, num4);
			}
			else
			{
				m_windPoints.TryAdd(key, m_minWind);
			}
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		FlatBatch3D flatBatch = m_primitivesRenderer3D.FlatBatch(0, DepthStencilState.None);
		if (m_displayTemperature)
		{
			DrawField(m_temperaturePoints, m_maxTemperature, m_minTemperature, flatBatch);
		}
		if (m_displayWind)
		{
			DrawField(m_windPoints, m_maxWind, m_minWind, flatBatch);
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}

	public static void QueueQuad(FlatBatch3D flatBatch, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color1, Color color2, Color color3, Color color4)
	{
		int count = flatBatch.TriangleVertices.Count;
		flatBatch.TriangleVertices.Add(new VertexPositionColor(p1, color1));
		flatBatch.TriangleVertices.Add(new VertexPositionColor(p2, color2));
		flatBatch.TriangleVertices.Add(new VertexPositionColor(p3, color3));
		flatBatch.TriangleVertices.Add(new VertexPositionColor(p4, color4));
		flatBatch.TriangleIndices.Add(count);
		flatBatch.TriangleIndices.Add(count + 1);
		flatBatch.TriangleIndices.Add(count + 2);
		flatBatch.TriangleIndices.Add(count + 2);
		flatBatch.TriangleIndices.Add(count);
		flatBatch.TriangleIndices.Add(count + 3);
	}

	public void DrawField(Dictionary<Point2, float> data, float max, float min, FlatBatch3D flatBatch)
	{
		foreach (var (point2, num2) in data)
		{
			Point2 key = point2 + new Point2(16, 0);
			Point2 key2 = point2 + new Point2(16, 16);
			Point2 key3 = point2 + new Point2(0, 16);
			Vector3 p = new Vector3(point2.X + 8, num2 + (float)m_basicY, point2.Y + 8);
			if (!data.TryGetValue(key, out var value))
			{
				continue;
			}
			Vector3 p2 = new Vector3(key.X + 8, value + (float)m_basicY, key.Y + 8);
			if (data.TryGetValue(key2, out var value2))
			{
				Vector3 p3 = new Vector3(key2.X + 8, value2 + (float)m_basicY, key2.Y + 8);
				if (data.TryGetValue(key3, out var value3))
				{
					Vector3 p4 = new Vector3(key3.X + 8, value3 + (float)m_basicY, key3.Y + 8);
					QueueQuad(flatBatch, p, p2, p3, p4, GetColor(min, max, num2), GetColor(min, max, value), GetColor(min, max, value2), GetColor(min, max, value3));
				}
			}
		}
	}

	public Color GetColor(float minTemperature, float maxTemperature, float temperature)
	{
		float num = 1f - (temperature - minTemperature) / (maxTemperature - minTemperature);
		Vector3 vector = Color.HsvToRgb(new Vector3(num * 240f, 1f, 0.5f));
		return new Color(vector.X, vector.Y, vector.Z, 0.5f);
	}
}
