using RecipaediaEX.Events;

namespace SCIENEW.Benchmark;

public static class IEBenchmarkEvents
{
	public static void PublishSubsystemUpdate(string name, float elapsedSeconds, int contextCount = -1)
	{
		RecipaediaEventBus.GetPublisher<SubsystemUpdateSample>().Publish(new SubsystemUpdateSample
		{
			Name = name,
			ElapsedSeconds = elapsedSeconds,
			ContextCount = contextCount
		});
	}
}
