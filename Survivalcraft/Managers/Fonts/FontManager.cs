using Engine;
using Engine.Graphics;
using Engine.Media;
using Game.IContentReader;
using SharpAstro.Fonts;

namespace Game {
    /// <summary>
    /// 系统字体发现结果：一个可用的系统字体面（ttc 容器内按 <see cref="FaceIndex"/> 定位）。<br/>
    /// 平台枚举见 <see cref="FontLocator"/>（Windows=注册表、Android=系统字体目录、其余平台空）。
    /// </summary>
    public class SystemFontFace {
        public string FilePath;

        public string Family;

        public VectorFontStyle Style;

        public int FaceIndex;

        /// <summary>true=等宽字体（进 Mono 链），false=正文字体（进 Default 链）。brief 四字段外的分类附加位，链条组装分流必需。</summary>
        public bool IsMono;
    }

    /// <summary>
    /// 平台系统字体发现（各平台恰好一个 Discover 实现）：Windows=FontLocator.Windows.cs（注册表）、
    /// Android=FontLocator.Android.cs（系统字体目录）、其余平台（IOS/BROWSER/LINUX/裸 API 构建）=
    /// FontLocator.None.cs 空实现（平台级位图回退，spec §1）。共享面探测助手亦置于本 partial（无平台条件）。
    /// </summary>
    static partial class FontLocator {
        /// <summary>
        /// OS/2 fsSelection 样式位映射（bit5=Bold、bit0=Italic）。与
        /// Game.IContentReader.VectorFontReader.BuildStyleFaces 同口径
        /// （该处为私有实现且不在本任务改动边界内，故此处按同规格另写；缺 OS/2 表或全 0 位归 Regular）。
        /// </summary>
        internal static VectorFontStyle MapFaceStyle(OpenTypeFont face) {
            ushort fsSelection = face.Os2?.FsSelection ?? 0;
            bool bold = (fsSelection & 0x20) != 0;
            bool italic = (fsSelection & 0x01) != 0;
            return bold && italic ? VectorFontStyle.BoldItalic
                : bold ? VectorFontStyle.Bold
                : italic ? VectorFontStyle.Italic
                : VectorFontStyle.Regular;
        }

        /// <summary>
        /// 候选文件探测：读盘 → SharpAstro LoadAll → 取第一个样式匹配的面（面序=ttc 容器序）。<br/>
        /// IO/解析失败或无样式匹配面均返回 null（候选缺席，调用方继续下一候选），失败记 Warning。
        /// </summary>
        internal static SystemFontFace ProbeFontFile(string filePath, string family, VectorFontStyle style, bool isMono) {
            try {
                byte[] data = File.ReadAllBytes(filePath);
                OpenTypeFont[] faces = OpenTypeFont.LoadAll(data);
                for (int i = 0; i < faces.Length; i++) {
                    if (MapFaceStyle(faces[i]) == style) {
                        return new SystemFontFace {
                            FilePath = filePath,
                            Family = family,
                            Style = style,
                            FaceIndex = i,
                            IsMono = isMono
                        };
                    }
                }
                return null;
            }
            catch (Exception ex) {
                Log.Warning($"[FontLocator] probe \"{filePath}\" failed: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>字体槽位种类（设置字符串 "system"|"ref:body"|"ref:mono"|"user:&lt;file&gt;" 的解析产物）。</summary>
    public enum FontSlotKind {
        /// <summary>"system"：跟随系统发现（正文章体/系统等宽），不产生独立条目。</summary>
        SystemDefault,

        /// <summary>"ref:body"：引用 body 主槽（FontBody）的解析结果。</summary>
        ReferenceBody,

        /// <summary>"ref:mono"：引用 mono 主槽（FontMono）的解析结果。</summary>
        ReferenceMono,

        /// <summary>"user:&lt;file&gt;"：用户导入字体（UserFontDir 下文件名）。</summary>
        UserFile
    }

    /// <summary>字体槽位设置值："system"|"ref:body"|"ref:mono"|"user:&lt;file&gt;" 与结构体的互转。</summary>
    public struct FontSlotSetting {
        public FontSlotKind Kind;

        /// <summary>仅 Kind==UserFile 时有效：UserFontDir 下的字体文件名。</summary>
        public string FileName;

        public static FontSlotSetting SystemDefault => new() { Kind = FontSlotKind.SystemDefault };

        /// <summary>设置字符串 → 槽位。null/空/未知值一律按 SystemDefault 处理（前向兼容）。</summary>
        public static FontSlotSetting Parse(string value) {
            if (string.IsNullOrEmpty(value)
                || value == "system") {
                return SystemDefault;
            }
            if (value == "ref:body") {
                return new FontSlotSetting { Kind = FontSlotKind.ReferenceBody };
            }
            if (value == "ref:mono") {
                return new FontSlotSetting { Kind = FontSlotKind.ReferenceMono };
            }
            if (value.StartsWith("user:", StringComparison.Ordinal)) {
                return new FontSlotSetting { Kind = FontSlotKind.UserFile, FileName = value.Substring(5) };
            }
            return SystemDefault;
        }

        /// <summary>槽位 → 设置字符串（SettingsManager 持久化格式）。</summary>
        public override string ToString() {
            return Kind switch {
                FontSlotKind.ReferenceBody => "ref:body",
                FontSlotKind.ReferenceMono => "ref:mono",
                FontSlotKind.UserFile => $"user:{FileName}",
                _ => "system"
            };
        }
    }

    /// <summary>
    /// 矢量字体链管理（spec §4）：默认链/等宽链的构建与热重建、位图回退字体、用户字体导入。<br/>
    /// 线程设计（控制器裁定 2026-10-07）：系统字体文件大（msyh.ttc ≈19MB），解析不得阻塞 UI——
    /// <see cref="Initialize"/> 只在 LoadingScreen 加载动作线程同步构建默认链（该阶段阻塞加载进度可接受，
    /// 且保证屏构建前链条必然就绪）；<see cref="RebuildChains"/> 在线程池构建、引用原子换入后，
    /// onCompleted 经 <see cref="Dispatcher.Dispatch"/> 编组回主线程（Window 帧循环 BeforeFrame 泵出）。
    /// </summary>
    public static class FontManager {
        /// <summary>用户导入字体目录（Storage 抽象路径）。</summary>
        public const string UserFontDir = "data:/Fonts";

        /// <summary>接受的字体文件后缀（导入过滤 + 用户字体列表过滤）。</summary>
        static readonly string[] FontFileSuffixes = [".ttf", ".otf", ".ttc"];

        static readonly object m_rebuildLock = new();

        /// <summary>正文链（Pericles 头 → body 槽位 → 系统正文字体）。null=构建失败/无可用字体，widget 走位图路径。</summary>
        public static VectorFont DefaultChain { get; private set; }

        /// <summary>等宽链（mono 槽位 → 系统等宽 → body 链条目尾部复用）。null 同上。</summary>
        public static VectorFont MonoChain { get; private set; }

        /// <summary>位图回退字体（原 LabelWidget.BitmapFont 的 ContentManager.Get 移入于此统一持有）。</summary>
        public static BitmapFont FallbackBitmapFont { get; private set; }

        /// <summary>
        /// 启动初始化：位图回退字体 + 默认链同步构建。只在 LoadingScreen 加载动作线程调用
        /// （加载动作于主线程 Update 内逐个执行，此处的阻塞即加载进度耗时，裁定允许）。
        /// 任何失败只降级（链=null → 位图路径），绝不上抛打断加载。
        /// </summary>
        public static void Initialize() {
            try {
                FallbackBitmapFont = ContentManager.Get<BitmapFont>("Fonts/Pericles");
            }
            catch (Exception ex) {
                FallbackBitmapFont = null;
                Log.Error($"[FontManager] fallback bitmap font load failed: {ex}");
            }
            try {
                lock (m_rebuildLock) {
                    RebuildChainsCore();
                }
            }
            catch (Exception ex) {
                Log.Error($"[FontManager] initial chain build failed: {ex}");
            }
        }

        /// <summary>
        /// 后台重建双链（发现+解析在线程池，不阻塞 UI）；完成后引用原子换入
        /// （引用赋值原子，绘制中的 widget 要么用旧链要么用新链），随后
        /// onCompleted 经 Dispatcher 编组在主线程执行（供 T10 设置屏热切换刷新）。
        /// </summary>
        public static void RebuildChains(Action onCompleted = null) {
            Task.Run(() => {
                try {
                    lock (m_rebuildLock) {
                        RebuildChainsCore();
                    }
                }
                catch (Exception ex) {
                    Log.Error($"[FontManager] chain rebuild failed: {ex}");
                }
                if (onCompleted != null) {
                    try {
                        Dispatcher.Dispatch(onCompleted);
                    }
                    catch (Exception ex) {
                        Log.Error($"[FontManager] onCompleted dispatch failed: {ex}");
                    }
                }
            });
        }

        /// <summary>双链构建本体（Discover/读盘/解析）。失败项逐级降级并记日志，正常不抛出。</summary>
        static void RebuildChainsCore() {
            List<SystemFontFace> systemFaces = FontLocator.Discover() ?? [];
            // 文件级条目缓存（一次重建内同一文件只读盘/解析一次；槽位引用与系统发现可能重复命中）
            Dictionary<string, ChainEntry> fileEntries = new(StringComparer.OrdinalIgnoreCase);
            List<ChainEntry> bodyEntries = BuildBodyEntries(systemFaces, fileEntries);
            List<ChainEntry> monoEntries = BuildMonoEntries(bodyEntries, systemFaces, fileEntries);
            // 旧链不 Dispose：绘制中的 widget 可能仍持旧引用，丢弃交 GC 优于使用期失效；
            // 跨线程重入由 m_rebuildLock 串行化
            DefaultChain = BuildChain(bodyEntries, "body");
            MonoChain = BuildChain(monoEntries, "mono");
        }

        // 条目序即链序：Pericles 头（原版拉丁开关）→ FontBody/FontBodyBold/FontBodyItalic/FontBodyBoldItalic 槽位 → 系统正文字体
        static List<ChainEntry> BuildBodyEntries(List<SystemFontFace> systemFaces, Dictionary<string, ChainEntry> fileEntries) {
            List<ChainEntry> entries = [];
            AddPericlesHead(entries);
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontBody, systemFaces, forBody: true, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontBodyBold, systemFaces, forBody: true, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontBodyItalic, systemFaces, forBody: true, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontBodyBoldItalic, systemFaces, forBody: true, depth: 0, fileEntries));
            AddSystemEntries(entries, systemFaces, mono: false, fileEntries);
            return entries;
        }

        // 条目序即链序：FontMono/FontMonoBold/FontMonoItalic/FontMonoBoldItalic 槽位 → 系统等宽 → body 条目尾部复用
        // （等宽字体普遍缺 CJK，尾部复用使 mono 段 CJK 沿链回落到正文字体渲染）
        static List<ChainEntry> BuildMonoEntries(List<ChainEntry> bodyEntries, List<SystemFontFace> systemFaces, Dictionary<string, ChainEntry> fileEntries) {
            List<ChainEntry> entries = [];
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontMono, systemFaces, forBody: false, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontMonoBold, systemFaces, forBody: false, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontMonoItalic, systemFaces, forBody: false, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontMonoBoldItalic, systemFaces, forBody: false, depth: 0, fileEntries));
            AddSystemEntries(entries, systemFaces, mono: true, fileEntries);
            foreach (ChainEntry entry in bodyEntries) {
                TryAddEntry(entries, entry);
            }
            return entries;
        }

        /// <summary>
        /// 槽位值 → 链条目。SystemDefault 返回 null（系统发现在条目列表尾部统一处理）；<br/>
        /// 引用槽（ref:body/ref:mono）解析到目标槽自身的取值，目标槽再引用（A→B→A 环）或无系统字体时按 null 截断。
        /// </summary>
        static ChainEntry ResolveSlot(string slotValue, List<SystemFontFace> systemFaces, bool forBody, int depth, Dictionary<string, ChainEntry> fileEntries) {
            FontSlotSetting setting = FontSlotSetting.Parse(slotValue);
            switch (setting.Kind) {
                case FontSlotKind.UserFile:
                    return LoadUserFontEntry(setting.FileName);
                case FontSlotKind.ReferenceBody:
                    return depth > 0
                        ? null
                        : ResolveSlot(SettingsManager.FontBody, systemFaces, forBody: true, depth + 1, fileEntries)
                            ?? FirstSystemEntry(systemFaces, mono: false, fileEntries);
                case FontSlotKind.ReferenceMono:
                    return depth > 0
                        ? null
                        : ResolveSlot(SettingsManager.FontMono, systemFaces, forBody: false, depth + 1, fileEntries)
                            ?? FirstSystemEntry(systemFaces, mono: true, fileEntries);
                default:
                    return null;
            }
        }

        /// <summary>Pericles 链头：原版拉丁字形（FontVanilaEnglish 开关控制）。原始字节经 ContentManager.GetStream
        /// （ContentInfo 注册期共享流，只读不释放，ToArray 与 VectorFontReader.Get 同口径）。</summary>
        static void AddPericlesHead(List<ChainEntry> entries) {
            if (!SettingsManager.FontVanilaEnglish) {
                return;
            }
            try {
                byte[] data = null;
                foreach (string suffix in new VectorFontReader().DefaultSuffix) {
                    Stream stream = ContentManager.GetStream($"Fonts/PericlesPro.{suffix}");
                    if (stream == null) {
                        continue;
                    }
                    data = stream is MemoryStream memoryStream ? memoryStream.ToArray() : CopyToBytes(stream);
                    break;
                }
                if (data == null) {
                    Log.Information("[FontManager] \"Fonts/PericlesPro\" not found in content, vanilla head entry skipped.");
                    return;
                }
                TryAddEntry(entries, VectorFontReader.BuildEntries(data, "Pericles")[0]);
            }
            catch (Exception ex) {
                Log.Error($"[FontManager] Pericles head entry build failed: {ex}");
            }
        }

        /// <summary>系统发现 → 条目（同文件去重；逐文件失败降级继续）。</summary>
        static void AddSystemEntries(List<ChainEntry> entries, List<SystemFontFace> systemFaces, bool mono, Dictionary<string, ChainEntry> fileEntries) {
            HashSet<string> added = new(StringComparer.OrdinalIgnoreCase);
            foreach (SystemFontFace face in systemFaces) {
                if (face.IsMono != mono
                    || !added.Add(face.FilePath)) {
                    continue;
                }
                TryAddEntry(entries, GetFileEntry(face.FilePath, fileEntries));
            }
        }

        /// <summary>引用槽落空时的兜底：第一个系统发现面（无则 null）。</summary>
        static ChainEntry FirstSystemEntry(List<SystemFontFace> systemFaces, bool mono, Dictionary<string, ChainEntry> fileEntries) {
            foreach (SystemFontFace face in systemFaces) {
                if (face.IsMono == mono) {
                    return GetFileEntry(face.FilePath, fileEntries);
                }
            }
            return null;
        }

        /// <summary>系统字体文件 → 链条目（缓存：一次重建内同一文件只读盘/解析一次，失败不重试）。</summary>
        static ChainEntry GetFileEntry(string filePath, Dictionary<string, ChainEntry> fileEntries) {
            if (fileEntries.TryGetValue(filePath, out ChainEntry cached)) {
                return cached;
            }
            ChainEntry entry = null;
            try {
                byte[] data = File.ReadAllBytes(filePath);
                entry = VectorFontReader.BuildEntries(data, $"system:{Storage.GetFileName(filePath)}")[0];
            }
            catch (Exception ex) {
                Log.Warning($"[FontManager] font file \"{filePath}\" load failed: {ex.Message}");
            }
            fileEntries.Add(filePath, entry);
            return entry;
        }

        /// <summary>用户字体文件 → 链条目（文件缺失/解析失败降级 null，槽位跳过）。</summary>
        static ChainEntry LoadUserFontEntry(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return null;
            }
            string path = Storage.CombinePaths(UserFontDir, fileName);
            try {
                if (!Storage.FileExists(path)) {
                    Log.Warning($"[FontManager] user font \"{fileName}\" missing, slot skipped.");
                    return null;
                }
                byte[] data = Storage.ReadAllBytes(path);
                return VectorFontReader.BuildEntries(data, $"user:{fileName}")[0];
            }
            catch (Exception ex) {
                Log.Warning($"[FontManager] user font \"{fileName}\" load failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>条目入链（null 与重复引用跳过；条目序即链序=回退优先序）。</summary>
        static void TryAddEntry(List<ChainEntry> entries, ChainEntry entry) {
            if (entry != null
                && !entries.Contains(entry)) {
                entries.Add(entry);
            }
        }

        /// <summary>链条目 → VectorFont。空链/无 Regular 面/构造异常均返回 null（widget 退位图路径），错误记日志。</summary>
        static VectorFont BuildChain(List<ChainEntry> entries, string name) {
            if (entries.Count == 0) {
                Log.Information($"[FontManager] {name} chain empty (bitmap fallback).");
                return null;
            }
            // Regular 整链缺失则全 widget 退位图度量/绘制，视为构建失败
            // （RegularSources 为 Engine.Media internal，此程序集不可见，按条目样式表等价判定）
            if (!entries.Any(entry => entry.StyleFaces.ContainsKey(VectorFontStyle.Regular))) {
                Log.Warning($"[FontManager] {name} chain has no Regular face (bitmap fallback).");
                return null;
            }
            try {
                return new VectorFont(entries, FallbackBitmapFont);
            }
            catch (Exception ex) {
                Log.Error($"[FontManager] {name} chain build failed: {ex}");
                return null;
            }
        }

        /// <summary>
        /// 校验并导入用户字体：SharpAstro 试解析（0 面即拒）+ Latin 必备字符（A-Z a-z 0-9）覆盖探测，
        /// 通过后写入 <see cref="UserFontDir"/>（同名覆盖）。失败时 error 为原因文本并返回 false，不抛出。
        /// </summary>
        public static bool ValidateAndImportFont(Stream stream, string fileName, out string error) {
            error = null;
            try {
                if (stream == null
                    || !stream.CanRead) {
                    error = "Font stream is empty or unreadable.";
                    return false;
                }
                string suffix = Storage.GetExtensionBCL(fileName ?? string.Empty).ToLowerInvariant();
                if (!FontFileSuffixes.Contains(suffix)) {
                    error = $"Unsupported font type \"{suffix}\" (expect ttf/otf/ttc).";
                    return false;
                }
                byte[] data = CopyToBytes(stream);
                if (data.Length == 0) {
                    error = "Font file is empty.";
                    return false;
                }
                OpenTypeFont[] faces;
                try {
                    faces = OpenTypeFont.LoadAll(data);
                }
                catch (Exception ex) {
                    error = $"Not a valid OpenType/TrueType font: {ex.Message}";
                    return false;
                }
                if (faces.Length == 0) {
                    error = "Font file contains no faces.";
                    return false;
                }
                // Latin 覆盖探测（面 0）：必备字符缺失即拒，防止纯 CJK/纯符号字体把拉丁段拉回位图
                List<char> missing = [];
                foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") {
                    if (faces[0].GetGlyphId(c) == 0) {
                        missing.Add(c);
                    }
                }
                if (missing.Count > 0) {
                    string sample = new string(missing.Take(16).ToArray());
                    error = $"Font lacks required Latin characters ({missing.Count} missing, e.g. \"{sample}\").";
                    return false;
                }
                string safeName = Storage.SanitizeFileNameSegment(Storage.GetFileName(fileName));
                if (!Storage.DirectoryExists(UserFontDir)) {
                    Storage.CreateDirectory(UserFontDir);
                }
                Storage.WriteAllBytes(Storage.CombinePaths(UserFontDir, safeName), data);
                Log.Information($"[FontManager] user font imported: {safeName}");
                return true;
            }
            catch (Exception ex) {
                error = ex.Message;
                Log.Error($"[FontManager] font import failed: {ex}");
                return false;
            }
        }

        /// <summary>列出已导入的用户字体文件名（按后缀过滤；目录不存在/枚举失败返回空列表）。</summary>
        public static List<string> ListUserFonts() {
            try {
                if (!Storage.DirectoryExists(UserFontDir)) {
                    return [];
                }
                return Storage.ListFileNames(UserFontDir)
                    .Where(fileName => FontFileSuffixes.Contains(Storage.GetExtensionBCL(fileName).ToLowerInvariant()))
                    .ToList();
            }
            catch (Exception ex) {
                Log.Warning($"[FontManager] list user fonts failed: {ex.Message}");
                return [];
            }
        }

        static byte[] CopyToBytes(Stream stream) {
            using MemoryStream buffer = new();
            stream.CopyTo(buffer);
            return buffer.ToArray();
        }
    }
}
