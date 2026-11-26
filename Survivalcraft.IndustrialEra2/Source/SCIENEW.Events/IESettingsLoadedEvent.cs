namespace SCIENEW.Events;

public readonly struct IESettingsLoadedEvent(int settingCount)
{
	public int SettingCount { get; } = settingCount;
}
