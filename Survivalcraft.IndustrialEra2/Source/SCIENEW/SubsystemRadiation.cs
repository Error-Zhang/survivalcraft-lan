using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemRadiation : Subsystem, IUpdateable, IDrawable
{
	private const float MaxCherenkovRadiations = 16384f;

	private readonly Dictionary<Point3, float> m_radiations = new Dictionary<Point3, float>();

	private readonly Dictionary<Point3, float> m_cherenkovRadiations = new Dictionary<Point3, float>();

	private readonly HashSet<Point3> m_cherenkovRadiationsToRemove = new HashSet<Point3>();

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1] { 2000 };

	public Dictionary<Point3, float> Radiations => m_radiations;

	public void Add(Point3 coord, float radiation)
	{
		if (!m_radiations.TryAdd(coord, radiation))
		{
			m_radiations[coord] = MathUtils.Max(m_radiations[coord], radiation);
		}
	}

	public void Remove(Point3 coord)
	{
		m_radiations.Remove(coord);
	}

	public void AddCherenkov(Point3 coord)
	{
		if (!m_radiations.TryAdd(coord, 16384f))
		{
			m_cherenkovRadiations[coord] = MathUtils.Max(m_radiations[coord], 16384f);
		}
	}

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		int value = valuesDictionary.GetValue("Count", 0);
		if (value != 0)
		{
			ValuesDictionary value2 = valuesDictionary.GetValue<ValuesDictionary>("Keys");
			ValuesDictionary value3 = valuesDictionary.GetValue<ValuesDictionary>("Values");
			for (int i = 0; i < value; i++)
			{
				m_radiations.Add(value2.GetValue<Point3>(i.ToString()), value3.GetValue<float>(i.ToString()));
			}
		}
	}

	public void Update(float dt)
	{
		foreach (Point3 item in m_cherenkovRadiationsToRemove)
		{
			m_cherenkovRadiations.Remove(item);
		}
		m_cherenkovRadiationsToRemove.Clear();
		foreach (KeyValuePair<Point3, float> radiation in m_radiations)
		{
			double num = 50000.0;
			m_radiations[radiation.Key] = radiation.Value * (float)Math.Exp((0.0 - (double)dt) / num);
		}
		foreach (KeyValuePair<Point3, float> cherenkovRadiation in m_cherenkovRadiations)
		{
			double num2 = 4.0;
			m_cherenkovRadiations[cherenkovRadiation.Key] = cherenkovRadiation.Value * (float)Math.Exp((0.0 - (double)dt) / num2);
			if (cherenkovRadiation.Value <= 1f)
			{
				m_cherenkovRadiationsToRemove.Add(cherenkovRadiation.Key);
			}
		}
	}

	public float GetRadiation(Vector3 position)
	{
		if (m_radiations.Count > 0)
		{
			float num = 0f;
			{
				foreach (KeyValuePair<Point3, float> radiation in m_radiations)
				{
					float x = radiation.Value / position.DistanceSquared(new Vector3(radiation.Key) + new Vector3(0.5f));
					num = MathUtils.Max(num, x);
				}
				return num;
			}
		}
		return 0f;
	}

	public override void Save(ValuesDictionary valuesDictionary)
	{
		base.Save(valuesDictionary);
		valuesDictionary.SetValue("Count", m_radiations.Count);
		ValuesDictionary valuesDictionary2 = new ValuesDictionary();
		ValuesDictionary valuesDictionary3 = new ValuesDictionary();
		int num = 0;
		foreach (KeyValuePair<Point3, float> radiation in m_radiations)
		{
			valuesDictionary2.SetValue(num.ToString(), radiation.Key);
			valuesDictionary3.SetValue(num.ToString(), radiation.Value);
			num++;
		}
		valuesDictionary.SetValue("Keys", valuesDictionary2);
		valuesDictionary.SetValue("Values", valuesDictionary3);
	}

	public float Get(Point3 position)
	{
		return m_radiations[position];
	}

	public bool TryGet(Point3 position, out float radiation)
	{
		return m_radiations.TryGetValue(position, out radiation);
	}

	public void Draw(Camera camera, int drawOrder)
	{
		foreach (KeyValuePair<Point3, float> cherenkovRadiation in m_cherenkovRadiations)
		{
			DrawingUtils.DrawCherenkovRadiation(camera, new Vector2((float)cherenkovRadiation.Key.X + 0.5f, (float)cherenkovRadiation.Key.Z + 0.5f), 1.5f, new Color(223, 223, 255, (int)(255f * cherenkovRadiation.Value / 16384f)), new Color(137, 196, 255, (int)(100f * cherenkovRadiation.Value / 16384f)), new Color(137, 196, 255, 0));
		}
	}
}
