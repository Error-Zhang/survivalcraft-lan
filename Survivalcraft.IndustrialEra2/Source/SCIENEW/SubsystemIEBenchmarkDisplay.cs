using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Benchmark;

namespace SCIENEW;

public class SubsystemIEBenchmarkDisplay : Subsystem, IUpdateable
{
	private static readonly string[] m_trackedNames = new string[2] { "SubsystemVoltNet", "SubsystemFluidTransfer" };

	public UpdateOrder UpdateOrder => UpdateOrder.BlockHighlight;

	public void Update(float dt)
	{
		if (!SettingsManager.DisplayFpsCounter || ScreensManager.CurrentScreen == null || ScreensManager.CurrentScreen.GetType() != typeof(GameScreen))
		{
			return;
		}
		string[] trackedNames = m_trackedNames;
		foreach (string text in trackedNames)
		{
			if (IEBenchmarkCollector.TryGet(text, out var lastMs, out var avgMs, out var contextCount))
			{
				string text2 = $"{text} tick {lastMs:F2}ms avg {avgMs:F2}ms";
				if (contextCount >= 0)
				{
					text2 += $" ({contextCount})";
				}
				PerformanceManager.AddExtraStat(text2);
			}
		}
		if (IESettingsManager.Get<bool>("DisplayPlayerCoords"))
		{
			SubsystemPlayers subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(throwOnError: false);
			if (subsystemPlayers != null && subsystemPlayers.ComponentPlayers.Count != 0)
			{
				Vector3 position = subsystemPlayers.ComponentPlayers[0].ComponentBody.Position;
				PerformanceManager.AddExtraStat($"Player ({position.X:F2},{position.Y:F2},{position.Z:F2})");
			}
		}
	}
}
