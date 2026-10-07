#if ANDROID
using Engine;
using Engine.Media;

namespace Game {
    static partial class FontLocator {
        // /system/fonts 世界可读（API 23→36 核验）；/system/product/fonts、/vendor/fonts 部分设备存在（存在性探测）
        static readonly string[] FontDirectories = ["/system/fonts", "/system/product/fonts", "/vendor/fonts"];

        // 候选名覆盖 AOSP+主流 OEM（MiSans=小米、HarmonyOS Sans=华为）；组内按序取首个可用文件
        static readonly (string[] Names, string Family, VectorFontStyle Style)[] BodyCandidates = [
            (["NotoSansCJK-Regular.ttc", "NotoSansCJKsc-Regular.otf"], "Noto Sans CJK SC", VectorFontStyle.Regular),
            (["MiSans-Regular.ttf", "MIUI_Normal.ttf", "HarmonyOS_Sans_SC_Medium.ttf"], "OEM Sans", VectorFontStyle.Regular),
        ];

        static readonly (string[] Names, string Family, VectorFontStyle Style)[] BoldCandidates = [
            (["MiSans-Bold.ttf", "MiSans-Demibold.ttf", "HarmonyOS_Sans_SC_Bold.ttf"], "OEM Sans", VectorFontStyle.Bold),
        ];

        static readonly (string[] Names, string Family, VectorFontStyle Style)[] MonoCandidates = [
            (["DroidSansMono.ttf", "RobotoMono-Regular.ttf", "CutiveMono.ttf"], "Mono", VectorFontStyle.Regular),
        ];

        public static List<SystemFontFace> Discover() {
            List<SystemFontFace> result = [];
            try {
                Dictionary<string, string> files = EnumerateFontFiles();
                MatchAndAdd(result, files, BodyCandidates, mono: false);
                MatchAndAdd(result, files, BoldCandidates, mono: false);
                MatchAndAdd(result, files, MonoCandidates, mono: true);
            }
            catch (Exception ex) {
                Log.Warning($"[FontLocator] Android discover failed: {ex.Message}");
            }
            return result;
        }

        static Dictionary<string, string> EnumerateFontFiles() {
            Dictionary<string, string> files = new(StringComparer.OrdinalIgnoreCase);
            foreach (string directory in FontDirectories) {
                if (!Directory.Exists(directory)) {
                    continue;
                }
                try {
                    foreach (string file in Directory.GetFiles(directory)) {
                        files.TryAdd(Path.GetFileName(file), file);
                    }
                }
                catch (Exception ex) {
                    Log.Warning($"[FontLocator] enumerate \"{directory}\" failed: {ex.Message}");
                }
            }
            return files;
        }

        static void MatchAndAdd(List<SystemFontFace> result, Dictionary<string, string> files, (string[] Names, string Family, VectorFontStyle Style)[] candidates, bool mono) {
            foreach ((string[] names, string family, VectorFontStyle style) in candidates) {
                foreach (string name in names) {
                    if (!files.TryGetValue(name, out string fullPath)) {
                        continue;
                    }
                    SystemFontFace face = ProbeFontFile(fullPath, family, style, mono);
                    if (face != null) {
                        result.Add(face);
                        break; // 组内首个可用候选即止（候选组=同角色的降级序列）
                    }
                }
            }
        }
    }
}
#endif
