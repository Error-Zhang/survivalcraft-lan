namespace SCIENEW.Modules.Settings;

public struct OnSettingsChangedEvent(string key, object value)
{
	public string m_key = key;

	public object m_value = value;
}
