using System;

namespace SCIENEW.Events;

public sealed class IndustrialHubEntry
{
	public string ButtonText { get; }

	public string Description { get; }

	public Action OnClick { get; }

	public IndustrialHubEntry(string buttonText, string description, Action onClick)
	{
		ButtonText = buttonText ?? string.Empty;
		Description = description ?? string.Empty;
		OnClick = onClick ?? throw new ArgumentNullException("onClick");
	}
}
