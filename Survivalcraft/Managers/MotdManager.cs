using System.Xml.Linq;
using Engine;
using XmlUtilities;

namespace Game {
    public static class MotdManager {
        public class FilterMod {
            public string Name = string.Empty;

            public string PackageName = string.Empty;

            public string Version = string.Empty;

            public string FilterAPIVersion = string.Empty;

            public string Explanation = string.Empty;
        }

        public static bool CanDownloadMotd = true;

        public static List<FilterMod> FilterModAll = [];

        private static string m_loadedFilterCache;

        public static void Update() {
#if DEBUG
            CanDownloadMotd = false;
#endif
            if (CanDownloadMotd && LanguageControl.jsonNode != null) {
                CanDownloadMotd = false;
                DownloadMotd();
            }

            string cache = SettingsManager.MotdLastDownloadedData;
            if (cache == m_loadedFilterCache || string.IsNullOrEmpty(cache)) {
                return;
            }
            m_loadedFilterCache = cache;
            try {
                LoadFilterMods(cache);
            }
            catch (Exception ex) {
                Log.Warning($"Failed reading mod compatibility notices. Reason: {ex.Message}");
            }
        }

        public static void DownloadMotd() {
            WebManager.Get(
                GetMotdUrl(),
                null,
                null,
                null,
                data => {
                    using StreamReader reader = new(new MemoryStream(data));
                    string filters = GetModFilterSection(reader.ReadToEnd());
                    if (filters != null) {
                        SettingsManager.MotdLastDownloadedData = filters;
                    }
                },
                error => Log.Warning($"Failed downloading mod compatibility notices. Reason: {error.Message}")
            );
        }

        private static string GetModFilterSection(string data) {
            const string close = "</Motd3>";
            int start = data.IndexOf("<Motd3", StringComparison.Ordinal);
            if (start < 0) {
                return null;
            }
            int end = data.IndexOf(close, start, StringComparison.Ordinal);
            return end >= 0 ? data[start..(end + close.Length)] : null;
        }

        public static void LoadFilterMods(string dataString) {
            string filters = GetModFilterSection(dataString);
            if (filters == null) {
                throw new InvalidOperationException("Missing mod compatibility data.");
            }
            XElement xElement = XmlUtils.LoadXmlFromString(filters, true);
            FilterModAll.Clear();
            foreach (XElement item in xElement.Elements()) {
                if (item.Name.LocalName == "FilterMod") {
                    XAttribute name = item.Attribute("Name");
                    if (name == null) {
                        continue;
                    }
                    XAttribute packageName = item.Attribute("PackageName");
                    if (packageName == null) {
                        continue;
                    }
                    XAttribute version = item.Attribute("Version");
                    if (version == null) {
                        continue;
                    }
                    XAttribute filterAPIVersion = item.Attribute("FilterAPIVersion");
                    if (filterAPIVersion == null) {
                        continue;
                    }
                    FilterMod filterMod = new() {
                        Name = name.Value,
                        PackageName = packageName.Value,
                        Version = version.Value,
                        FilterAPIVersion = filterAPIVersion.Value,
                        Explanation = item.Value
                    };
                    FilterModAll.Add(filterMod);
                }
            }
        }

        public static string GetMotdUrl() =>
            string.Format(SettingsManager.MotdUpdateUrl, VersionsManager.SerializationVersion, LanguageControl.CurrentLanguageName);
    }
}
