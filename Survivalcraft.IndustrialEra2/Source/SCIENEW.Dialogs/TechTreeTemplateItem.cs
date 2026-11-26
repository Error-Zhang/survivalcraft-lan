using SCIENEW.Tech;

namespace SCIENEW.Dialogs;

public sealed class TechTreeTemplateItem
{
	public TechTreeTemplateKind Kind { get; }

	public string Label { get; }

	public TechTreeModFile? ModEntry { get; }

	public string? ExternalFileName { get; }

	public TechTreeTemplateItem(TechTreeTemplateKind kind, string label, TechTreeModFile? modEntry = null, string? externalFileName = null)
	{
		Kind = kind;
		Label = label;
		ModEntry = modEntry;
		ExternalFileName = externalFileName;
	}
}
