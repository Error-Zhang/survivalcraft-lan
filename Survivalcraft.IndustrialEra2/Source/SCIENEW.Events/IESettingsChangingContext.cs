namespace SCIENEW.Events;

public sealed class IESettingsChangingContext
{
	public string Key { get; }

	public object Value { get; set; }

	public IESettingsChangingContext(string key, object value)
	{
		Key = key ?? string.Empty;
		Value = value;
	}
}
