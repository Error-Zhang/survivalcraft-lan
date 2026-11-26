using System;
using System.Collections.Generic;
using Game;
using RecipaediaEX.Events;

namespace SCIENEW.Benchmark;

public static class IEBenchmarkCollector
{
	private class Entry
	{
		public RunningAverage m_average = new RunningAverage(1f);

		public float m_lastSeconds;

		public int m_lastContextCount = -1;
	}

	private static readonly Dictionary<string, Entry> m_entries = new Dictionary<string, Entry>();

	private static IDisposable? m_subscription;

	public static void Subscribe()
	{
		m_subscription?.Dispose();
		m_subscription = RecipaediaEventBus.GetSubscriber<SubsystemUpdateSample>().Subscribe(OnSample);
	}

	public static void Unsubscribe()
	{
		m_subscription?.Dispose();
		m_subscription = null;
		m_entries.Clear();
	}

	private static void OnSample(SubsystemUpdateSample sample)
	{
		if (!m_entries.TryGetValue(sample.Name, out Entry value))
		{
			value = new Entry();
			m_entries[sample.Name] = value;
		}
		value.m_average.AddSample(sample.ElapsedSeconds);
		value.m_lastSeconds = sample.ElapsedSeconds;
		value.m_lastContextCount = sample.ContextCount;
	}

	public static bool TryGet(string name, out float lastMs, out float avgMs, out int contextCount)
	{
		if (!m_entries.TryGetValue(name, out Entry value))
		{
			lastMs = 0f;
			avgMs = 0f;
			contextCount = -1;
			return false;
		}
		lastMs = value.m_lastSeconds * 1000f;
		avgMs = value.m_average.Value * 1000f;
		contextCount = value.m_lastContextCount;
		return true;
	}
}
