#if WINDOWS
// 注册表 API 的平台兼容分析在 net10.0-windows 下仍误报 CA1416（同 FontTextWidget.cs/Storage.cs 先例，整文件仅 WINDOWS 编译）
#pragma warning disable CA1416
using Engine;
using Engine.Media;
using Microsoft.Win32;

namespace Game {
    static partial class FontLocator {
        // 雅黑 family 三文件分离：msyh.ttc(Regular)/msyhbd.ttc(Bold)/msyhl.ttc(Light)；无 Italic。
        // Pattern=注册表值数据中的文件名去扩展名（大小写不敏感精确匹配，避免 "cour" 误吞 "courbd"）
        static readonly (string Family, string Pattern, VectorFontStyle Style)[] BodyCandidates = [
            ("Microsoft YaHei", "msyh", VectorFontStyle.Regular),
            ("Microsoft YaHei", "msyhbd", VectorFontStyle.Bold),
        ];

        static readonly (string Family, string Pattern, VectorFontStyle Style)[] MonoCandidates = [
            ("Consolas", "consola", VectorFontStyle.Regular),
            ("Consolas", "consolab", VectorFontStyle.Bold),
            ("Cascadia Mono", "CascadiaMono", VectorFontStyle.Regular),
            ("Cascadia Code", "CascadiaCode", VectorFontStyle.Regular),
            ("Courier New", "cour", VectorFontStyle.Regular),
            ("Courier New", "courbd", VectorFontStyle.Bold),
        ];

        public static List<SystemFontFace> Discover() {
            List<SystemFontFace> result = [];
            try {
                Dictionary<string, string> files = EnumerateFontFiles();
                MatchAndAdd(result, files, BodyCandidates, mono: false);
                MatchAndAdd(result, files, MonoCandidates, mono: true);
            }
            catch (Exception ex) {
                Log.Warning($"[FontLocator] Windows discover failed: {ex.Message}");
            }
            return result;
        }

        // HKLM+HKCU ...\Microsoft\Windows NT\CurrentVersion\Fonts：值名（如
        // "Microsoft YaHei & Microsoft YaHei UI (TrueType)"）→ 值数据（文件名或 per-user 绝对路径）。
        // 文件解析：绝对路径直接用；相对名先探测 %WINDIR%\Fonts 再退 C:\Windows\Fonts（双路径 File.Exists）。
        // HKLM 先入为主、HKCU 补充（TryAdd 去重），键=文件名去扩展名（大小写不敏感）。
        static Dictionary<string, string> EnumerateFontFiles() {
            Dictionary<string, string> files = new(StringComparer.OrdinalIgnoreCase);
            string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            RegistryKey[] registryRoots = [Registry.LocalMachine, Registry.CurrentUser];
            foreach (RegistryKey root in registryRoots) {
                using RegistryKey key = root.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts", writable: false);
                if (key == null) {
                    continue;
                }
                foreach (string valueName in key.GetValueNames()) {
                    try {
                        if (key.GetValue(valueName) is not string fileName
                            || fileName.Length == 0) {
                            continue;
                        }
                        bool rooted = Path.IsPathRooted(fileName);
                        string fullPath = rooted
                            ? fileName
                            : Path.Combine(windowsDirectory, "Fonts", fileName);
                        if (!rooted
                            && !File.Exists(fullPath)) {
                            fullPath = Path.Combine(@"C:\Windows\Fonts", fileName);
                        }
                        if (File.Exists(fullPath)) {
                            files.TryAdd(Path.GetFileNameWithoutExtension(fileName), fullPath);
                        }
                    }
                    catch (Exception ex) {
                        Log.Warning($"[FontLocator] registry font value \"{valueName}\" skipped: {ex.Message}");
                    }
                }
            }
            return files;
        }

        static void MatchAndAdd(List<SystemFontFace> result, Dictionary<string, string> files, (string Family, string Pattern, VectorFontStyle Style)[] candidates, bool mono) {
            foreach ((string family, string pattern, VectorFontStyle style) in candidates) {
                if (!files.TryGetValue(pattern, out string fullPath)) {
                    continue;
                }
                SystemFontFace face = ProbeFontFile(fullPath, family, style, mono);
                if (face != null) {
                    result.Add(face);
                }
            }
        }
    }
}
#endif
