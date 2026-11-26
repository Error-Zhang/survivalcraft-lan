namespace SCIENEW.Benchmark;

public readonly struct SubsystemUpdateSample
{
	public string Name { get; init; }

	public float ElapsedSeconds { get; init; }

	public int ContextCount { get; init; }
}
