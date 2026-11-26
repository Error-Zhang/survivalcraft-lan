using System.Collections.Generic;

namespace SCIENEW.Events;

public sealed class IndustrialHubCreatingEvent
{
	public IndustrialEraHubScreen Screen { get; }

	public List<IndustrialHubEntry> ExtraEntries { get; }

	public IndustrialHubCreatingEvent(IndustrialEraHubScreen screen)
	{
		Screen = screen;
		ExtraEntries = new List<IndustrialHubEntry>();
	}
}
