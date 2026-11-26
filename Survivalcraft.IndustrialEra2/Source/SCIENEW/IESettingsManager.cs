using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;
using RecipaediaEX.Events;
using SCIENEW.Descriptors;
using SCIENEW.Events;
using SCIENEW.Modules.Settings;
using TemplatesDatabase;
using XmlUtilities;

namespace SCIENEW;

public static class IESettingsManager
{
	public static string SettingsPath = ExtPath + "/IESettings.xml";

	public static ValuesDictionary Settings = new ValuesDictionary();

	public const string MachinesVolumeKey = "MachinesVolume";

	public const string CraftingMessageKey = "CraftingMessage";

	public const string SniperModeKey = "SniperMode";

	public const string GasolineBoatMountOffsetKey = "GasolineBoatMountOffset";

	public const string LowQualityKey = "LowQuality";

	public const string RadarAlignmentKey = "RadarAlignment";

	public const string MiningDrillDisplayPatternKey = "MiningDrillDisplayPattern";

	public const string TechTreeStaticBackgroundKey = "TechTreeStaticBackground";

	public const string IndustrialHubStaticBackgroundKey = "IndustrialHubStaticBackground";

	public const string DisplayPlayerCoordsKey = "DisplayPlayerCoords";

	public static KeyValuePair<string, object>[] SettingsItems = new KeyValuePair<string, object>[10]
	{
		new KeyValuePair<string, object>("MachinesVolume", 0.3f),
		new KeyValuePair<string, object>("CraftingMessage", false),
		new KeyValuePair<string, object>("SniperMode", true),
		new KeyValuePair<string, object>("GasolineBoatMountOffset", true),
		new KeyValuePair<string, object>("LowQuality", false),
		new KeyValuePair<string, object>("RadarAlignment", WidgetAlignmentData.FromInt(9)),
		new KeyValuePair<string, object>("MiningDrillDisplayPattern", true),
		new KeyValuePair<string, object>("TechTreeStaticBackground", false),
		new KeyValuePair<string, object>("IndustrialHubStaticBackground", false),
		new KeyValuePair<string, object>("DisplayPlayerCoords", false)
	};

	public static readonly Dictionary<string, Type> Descriptors = new Dictionary<string, Type>();

	private static readonly HashSet<Assembly> m_scannedAssemblies = new HashSet<Assembly>();

	public static string ExtPath => ModsManager.ExternalPath;

	public static void Init()
	{
		if (Storage.FileExists(SettingsPath))
		{
			using Stream stream = Storage.OpenFile(SettingsPath, OpenFileMode.CreateOrOpen);
			try
			{
				XElement overridesNode = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
				Settings.ApplyOverrides(overridesNode);
				KeyValuePair<string, object>[] settingsItems = SettingsItems;
				for (int i = 0; i < settingsItems.Length; i++)
				{
					KeyValuePair<string, object> keyValuePair = settingsItems[i];
					if (Settings.ContainsKey(keyValuePair.Key))
					{
						if (Settings[keyValuePair.Key].GetType().FullName != keyValuePair.Value.GetType().FullName)
						{
							Settings[keyValuePair.Key] = keyValuePair.Value;
							Log.Warning(string.Format(LanguageControl.Get("IESettingsManager", "SettingError"), keyValuePair.Key));
						}
					}
					else
					{
						Settings.Add(keyValuePair.Key, keyValuePair.Value);
						Log.Warning(string.Format(LanguageControl.Get("IESettingsManager", "SettingAbsent"), keyValuePair.Key));
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Format(LanguageControl.Get("IESettingsManager", "SettingsFileError"), ex.Message));
				CreateDefaultSettings();
			}
		}
		else
		{
			CreateDefaultSettings();
		}
		foreach (Assembly item in TypeCache.LoadedAssemblies.Where((Assembly a) => !TypeCache.IsKnownSystemAssembly(a)))
		{
			if (m_scannedAssemblies.Contains(item))
			{
				continue;
			}
			foreach (Type item2 in from t in item.GetTypes()
				where typeof(IDescriptor).IsAssignableFrom(t) && typeof(Widget).IsAssignableFrom(t) && (object)t != null && t.IsClass && !t.IsAbstract
				select t)
			{
				DescriptorAttribute customAttribute = item2.GetCustomAttribute<DescriptorAttribute>();
				if (customAttribute != null && !string.IsNullOrEmpty(customAttribute.TargetType.FullName))
				{
					Descriptors.Add(customAttribute.TargetType.FullName, item2);
				}
			}
			m_scannedAssemblies.Add(item);
		}
		RecipaediaEventBus.GetPublisher<IESettingsLoadedEvent>().Publish(new IESettingsLoadedEvent(Settings.Count));
	}

	public static void LateInit()
	{
		ScreensManager.AddScreen("IESettings", new IESettingsScreen());
		ScreensManager.m_screens["IndustrialEraHub"] = new IndustrialEraHubScreen();
		ScreensManager.m_screens["IndustrialTechTreeMaker"] = new TechTreeMakerScreen();
		ScreensManager.m_screens["IndustrialIconMaker"] = new IconMakerScreen();
		ScreensManager.m_screens["IECredits"] = new IECreditsScreen();
	}

	private static void CreateDefaultSettings()
	{
		Settings = new ValuesDictionary();
		KeyValuePair<string, object>[] settingsItems = SettingsItems;
		for (int i = 0; i < settingsItems.Length; i++)
		{
			KeyValuePair<string, object> keyValuePair = settingsItems[i];
			Settings.Add(keyValuePair.Key, keyValuePair.Value);
		}
	}

	public static void Save()
	{
		Task.Run(() =>
		{
			using (Stream stream = Storage.OpenFile(SettingsPath, OpenFileMode.Create))
			{
				XElement node = new XElement("IESettings");
				Settings.Save(node);
				XmlUtils.SaveXmlToStream(node, stream, null, throwOnError: true);
			}
			Log.Information(LanguageControl.Get("IESettingsManager", "SettingsSaving"));
		});
	}

	private static object Get(string key)
	{
		return Settings[key];
	}

	public static T Get<T>(string key)
	{
		return Settings.GetValue<T>(key);
	}

	public static void Set(string key, object value)
	{
		IESettingsChangingContext iESettingsChangingContext = new IESettingsChangingContext(key, value);
		if (RecipaediaInterceptBus.TryProceed(iESettingsChangingContext))
		{
			Settings[key] = iESettingsChangingContext.Value;
			RecipaediaEventBus.GetPublisher<OnSettingsChangedEvent>().Publish(new OnSettingsChangedEvent(key, iESettingsChangingContext.Value));
		}
	}

	public static Widget? GetDescriptor(string settingName, Type settingType)
	{
		if (string.IsNullOrEmpty(settingType.FullName))
		{
			return null;
		}
		if (!Descriptors.TryGetValue(settingType.FullName, out Type value))
		{
			return null;
		}
		if (!(Activator.CreateInstance(value) is IDescriptor descriptor))
		{
			return null;
		}
		descriptor.PropertyName = settingName;
		return descriptor.GetWidget();
	}
}
