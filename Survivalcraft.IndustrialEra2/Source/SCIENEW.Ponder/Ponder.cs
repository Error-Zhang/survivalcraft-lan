using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game;
using GameEntitySystem;

namespace SCIENEW.Ponder;

public class Ponder : IDisposable
{
	protected List<IPonderSet> m_ponderSets;

	public List<IPonderSet> PonderSets => m_ponderSets;

	public StaticPonderBlockSet StaticPonderBlockSet { get; protected set; }

	public DynamicPonderBlockSet DynamicPonderBlockSet { get; protected set; }

	public PickablePonderBlockSet PickablePonderBlockSet { get; protected set; }

	public ProjectilePonderBlockSet ProjectilePonderBlockSet { get; protected set; }

	public Project Project { get; protected set; }

	public Ponder(Project project)
	{
		m_ponderSets = new List<IPonderSet>();
		Project = project;
		StaticPonderBlockSet = Add<StaticPonderBlockSet>();
		DynamicPonderBlockSet = Add<DynamicPonderBlockSet>();
		PickablePonderBlockSet = Add<PickablePonderBlockSet>();
		ProjectilePonderBlockSet = Add<ProjectilePonderBlockSet>();
	}

	public void Draw(Camera camera)
	{
		m_ponderSets.ForEach((IPonderSet set) =>
		{
			set.Draw(camera);
		});
	}

	public void Update()
	{
		m_ponderSets.ForEach((IPonderSet set) =>
		{
			set.Update();
		});
	}

	public T Get<T>(bool throwOnError = true) where T : class, IPonderSet
	{
		return Get(typeof(T), throwOnError) as T;
	}

	public IPonderSet Get(Type type, bool throwOnError = true)
	{
		foreach (IPonderSet ponderSet in m_ponderSets)
		{
			if (type.GetTypeInfo().IsAssignableFrom(ponderSet.GetType().GetTypeInfo()))
			{
				return ponderSet;
			}
		}
		if (!throwOnError)
		{
			return null;
		}
		throw new Exception($"Required ponder set {type.FullName} does not exist in ponder.");
	}

	public T Add<T>(bool throwOnError = true) where T : class, IPonderSet
	{
		return Add(typeof(T), throwOnError) as T;
	}

	public IPonderSet Add(Type type, bool throwOnError = true)
	{
		if (Get(type, throwOnError: false) != null)
		{
			if (!throwOnError)
			{
				return null;
			}
			throw new Exception("Type " + type.FullName + " already exists.");
		}
		try
		{
			IPonderSet ponderSet = Activator.CreateInstance(type, Project, this) as IPonderSet;
			m_ponderSets.Add(ponderSet);
			return ponderSet;
		}
		catch (Exception)
		{
			if (throwOnError)
			{
				throw;
			}
			return null;
		}
	}

	public void Dispose()
	{
		m_ponderSets.OfType<IDisposable>().ToList().ForEach((IDisposable x) =>
		{
			x.Dispose();
		});
	}
}
