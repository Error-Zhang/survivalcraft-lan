using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemQuarryDrillerMobiles : Subsystem
{
	protected Dictionary<int, ComponentQuarryDriller> m_mobiles = new Dictionary<int, ComponentQuarryDriller>();

	protected int m_maxID;

	protected Stack<int> m_removedID = new Stack<int>();

	public override void OnEntityAdded(Entity entity)
	{
		ComponentQuarryDriller componentQuarryDriller = entity.FindComponent<ComponentQuarryDriller>();
		if (componentQuarryDriller != null)
		{
			m_mobiles[componentQuarryDriller.ID] = componentQuarryDriller;
		}
	}

	public override void OnEntityRemoved(Entity entity)
	{
		ComponentQuarryDriller componentQuarryDriller = entity.FindComponent<ComponentQuarryDriller>();
		if (componentQuarryDriller != null)
		{
			m_mobiles.Remove(componentQuarryDriller.ID);
		}
	}

	internal IEnumerable<ComponentQuarryDriller> Mobiles => m_mobiles.Values;

	public int CreateMobile(Action<Entity> initialize = null)
	{
		Entity entity = DatabaseManager.CreateEntity(Project, "QuarryDrillerMobile", throwIfNotFound: true);
		entity.FindComponent<ComponentSpawn>(throwOnError: true).SpawnDuration = 0f;
		int num = ((m_removedID.Count > 0) ? m_removedID.Pop() : m_maxID++);
		entity.FindComponent<ComponentQuarryDriller>(throwOnError: true).ID = num;
		initialize?.Invoke(entity);
		Project.AddEntity(entity);
		return num;
	}

	public void RemoveMobile(int id)
	{
		if (m_mobiles.TryGetValue(id, out ComponentQuarryDriller value))
		{
			Project.RemoveEntity(value.Entity, disposeEntity: true);
			m_removedID.Push(id);
		}
	}

	public override void Save(ValuesDictionary valuesDictionary)
	{
		base.Save(valuesDictionary);
		valuesDictionary.SetValue("MaxID", m_maxID);
		valuesDictionary.SetValue("RemovedID", string.Join(";", m_removedID.Reverse()));
	}

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_maxID = valuesDictionary.GetValue("MaxID", 0);
		string[] array = valuesDictionary.GetValue("RemovedID", string.Empty).Split((ReadOnlySpan<char>)new char[1] { ';' });
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], out var result))
			{
				m_removedID.Push(result);
			}
		}
	}

	public ComponentQuarryDriller? GetMobile(int id)
	{
		if (!m_mobiles.ContainsKey(id))
		{
			return null;
		}
		return m_mobiles[id];
	}
}
