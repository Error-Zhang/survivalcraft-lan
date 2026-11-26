using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class Subsystem996Chunks : Subsystem
{
	public Dictionary<Point2, bool> m_terrainChunks996 = new Dictionary<Point2, bool>();

	public SubsystemTerrain m_subsystemTerrain;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		string value = valuesDictionary.GetValue("Chunks996", string.Empty);
		if (!string.IsNullOrEmpty(value))
		{
			string[] array = value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				Point2 point = new Point2(int.Parse(text.Split((ReadOnlySpan<char>)new char[1] { ',' })[0]), int.Parse(text.Split((ReadOnlySpan<char>)new char[1] { ',' })[1]));
				AddChunks996(point.X, point.Y, isPermanent: true);
			}
		}
	}

	public override void Save(ValuesDictionary valuesDictionary)
	{
		base.Save(valuesDictionary);
		string text = string.Empty;
		foreach (KeyValuePair<Point2, bool> item in m_terrainChunks996)
		{
			if (item.Value)
			{
				text = text + $"{item.Key.X},{item.Key.Y}" + ";";
			}
		}
		valuesDictionary.SetValue("Chunks996", text);
	}

	public bool AddChunks996(int x, int y, bool isPermanent)
	{
		Point2 key = new Point2(x, y);
		if (m_terrainChunks996.TryGetValue(key, out var value))
		{
			if (value)
			{
				return false;
			}
			if (isPermanent)
			{
				m_terrainChunks996[key] = true;
				return true;
			}
			return false;
		}
		m_terrainChunks996.Add(key, isPermanent);
		return true;
	}

	public bool RemoveChunks996(int x, int y)
	{
		Point2 key = new Point2(x, y);
		if (m_terrainChunks996.Remove(key))
		{
			return true;
		}
		return false;
	}

	public void Allocate996Chunk(Point2 coord, bool isPermaent)
	{
		bool value = AddChunks996(coord.X, coord.Y, isPermaent);
		Log.Warning($"chunk has been forced to allocate. coords:{coord}, success:{value}");
		m_subsystemTerrain.TerrainUpdater.AllocateAndFreeChunks(m_subsystemTerrain.TerrainUpdater.m_updateParameters.Locations.Values.ToArray());
	}

	public void Free996Chunk(Point2 coord)
	{
		bool value = RemoveChunks996(coord.X, coord.Y);
		m_subsystemTerrain.TerrainUpdater.AllocateAndFreeChunks(m_subsystemTerrain.TerrainUpdater.m_updateParameters.Locations.Values.ToArray());
		Log.Debug($"chunk has been forced to free. coords:{coord}, success:{value}");
	}
}
