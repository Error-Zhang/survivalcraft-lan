using Engine;
using Engine.Graphics;
using Engine.Media;
using Game.IContentReader;
using SharpAstro.Fonts;
#if BROWSER
using Engine.Browser;
#endif

namespace Game {
    /// <summary>
    /// 系统字体发现结果：一个可用的系统字体面（ttc 容器内按 <see cref="FaceIndex"/> 定位）。<br/>
    /// 平台枚举见 <see cref="FontLocator"/>（Windows=注册表、Android=系统字体目录、
    /// Linux=fontconfig、BROWSER=Local Font Access API、其余平台空）。
    /// </summary>
    public class SystemFontFace {
        public string FilePath;

        public string Family;

        public VectorFontStyle Style;

        public int FaceIndex;

        /// <summary>true=等宽字体（进 Mono 链），false=正文字体（进 Default 链）。brief 四字段外的分类附加位，链条组装分流必需。</summary>
        public bool IsMono;

        /// <summary>浏览器 Local Font Access 路线的字体字节（内存持有不落盘——用户裁定系统字体
        /// 唯一落盘路径=手动导入字体到 FontsPath）；FilePath 为 "localfont://" 展示串不落盘。
        /// 其余平台恒 null 走 FilePath 读盘。</summary>
        public byte[] Data;
    }

    /// <summary>
    /// 平台系统字体发现（各平台恰好一个 Discover 实现）：Windows=FontLocator.Windows.cs（注册表）、
    /// Android=FontLocator.Android.cs（系统字体目录）、Linux=FontLocator.Linux.cs（fontconfig+
    /// 目录扫描兜底）、BROWSER=FontLocator.Browser.cs（Local Font Access API，手势扫描+内存字节）、
    /// 其余平台（IOS/裸 API 构建）=FontLocator.None.cs 空实现（平台级位图回退，spec §1）。
    /// 共享面探测助手亦置于本 partial（无平台条件）。
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

    /// <summary>字体槽位种类（设置字符串 "builtin"|"system"|"ref:body"|"ref:mono"|"user:&lt;file&gt;" 的解析产物）。</summary>
    public enum FontSlotKind {
        /// <summary>"builtin"：游戏内置字体（Content Fonts/PericlesPro 四样式面，全平台可用，不依赖系统发现）。</summary>
        BuiltIn,

        /// <summary>"system"：跟随系统发现（正文章体/系统等宽），不产生独立条目；仅 WINDOWS/ANDROID/LINUX 及 BROWSER（运行时探测）可用（见 FontManager.SystemFontsSupported）。</summary>
        SystemDefault,

        /// <summary>"ref:body"：引用 body 主槽（FontBody）的解析结果。</summary>
        ReferenceBody,

        /// <summary>"ref:mono"：引用 mono 主槽（FontMono）的解析结果。</summary>
        ReferenceMono,

        /// <summary>"user:&lt;file&gt;"：用户导入字体（ModsManager.FontsPath 下文件名）。</summary>
        UserFile
    }

    /// <summary>字体槽位设置值："builtin"|"system"|"ref:body"|"ref:mono"|"user:&lt;file&gt;" 与结构体的互转。</summary>
    public struct FontSlotSetting {
        public FontSlotKind Kind;

        /// <summary>仅 Kind==UserFile 时有效：ModsManager.FontsPath 下的字体文件名。</summary>
        public string FileName;

        public static FontSlotSetting SystemDefault => new() { Kind = FontSlotKind.SystemDefault };

        /// <summary>设置字符串 → 槽位。null/空/未知值一律按 SystemDefault 处理（前向兼容）。</summary>
        public static FontSlotSetting Parse(string value) {
            if (string.IsNullOrEmpty(value)
                || value == "system") {
                return SystemDefault;
            }
            if (value == "builtin") {
                return new FontSlotSetting { Kind = FontSlotKind.BuiltIn };
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
                FontSlotKind.BuiltIn => "builtin",
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
        /// <summary>
        /// 内置字体槽位值（FontSlotKind.BuiltIn 的持久化串）。
        /// </summary>
        public const string BuiltinSlotValue = "builtin";

        /// <summary>
        /// 平台系统字体发现是否可用：WINDOWS/ANDROID/LINUX 编译期恒 true；BROWSER 运行时探测
        /// （'queryLocalFonts' in window，Firefox/Safari 恒 false）；其余平台恒 false。<br/>
        /// false：设置屏隐藏"系统默认"选项、加载设置时 "system" 槽位迁移为内置字体
        /// （<see cref="MigrateSystemSlotsToBuiltin"/>），矢量链由内置头/槽位/模组/用户字体构成。
        /// </summary>
        public static bool SystemFontsSupported {
            get {
#if BROWSER
                if (!m_browserSystemFontsSupported.HasValue) {
                    m_browserSystemFontsSupported = BrowserInterop.IsLocalFontsSupported();
                }
                return m_browserSystemFontsSupported.Value;
#elif WINDOWS || ANDROID || LINUX
                return true;
#else
                return false;
#endif
            }
        }

#if BROWSER
        /// <summary>运行时探测缓存（同步 JS interop 便宜，仍缓存免重复跨界）。</summary>
        static bool? m_browserSystemFontsSupported;
#endif

        /// <summary>接受的字体文件后缀（导入过滤 + 用户字体列表过滤）。SharpAstro 亦支持 WOFF/WOFF2。</summary>
        static readonly string[] FontFileSuffixes = [".ttf", ".otf", ".ttc", ".woff", ".woff2"];

        static readonly object m_rebuildLock = new();

        /// <summary>样式面探测结果缓存：探测键（"system:body"|"system:mono"|"user:&lt;文件名&gt;"|"none"）→ 源包含的样式面集合。</summary>
        static readonly Dictionary<string, HashSet<VectorFontStyle>> m_sourceStylesCache = new(StringComparer.Ordinal);

        static readonly object m_probeLock = new();

        /// <summary>最近一次链构建的系统发现结果（"system:*" 探测键取同族发现样式并集用；构建前为 null）。</summary>
        static List<SystemFontFace> m_lastSystemFaces;

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
            MigrateSystemSlotsToBuiltin();
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
                // 首建=链从 null 换入，同样通知（Core 失败在下方 catch，不达派发）
                DispatchFontChainsRebuilt();
            }
            catch (Exception ex) {
                Log.Error($"[FontManager] initial chain build failed: {ex}");
            }
        }

        /// <summary>
        /// 同步重建双链（语言切换专用）：ChangeLanguage 本身同步重建全屏，新屏 ctor 逐个捕获
        /// 链引用，异步 rebuild 会让新屏捕到旧 locale 的链。阻塞量=系统字体解析（数百 ms 级），
        /// 语言切换低频可接受。旧链当场释放：切换动画窗内残留旧屏的绘制走缺链守卫（位图回退，
        /// 不崩，见 VectorFont.Dispose）
        /// </summary>
        public static void RebuildChainsSync() {
            lock (m_rebuildLock) {
                List<VectorFont> retired = RebuildChainsCore();
                if (retired != null) {
                    foreach (VectorFont chain in retired) {
                        try {
                            chain.Dispose();
                        }
                        catch (Exception ex) {
                            Log.Warning($"[FontManager] previous chain dispose failed: {ex.Message}");
                        }
                    }
                }
            }
            DispatchFontChainsRebuilt();
        }

        /// <summary>
        /// 浏览器系统字体扫描（Local Font Access）：须在用户手势上下文调用（设置屏基础槽选"系统默认"）——
        /// queryLocalFonts 首调触发权限弹窗（local-fonts 权限跨会话持久，重扫静默）。扫描全程内存
        /// 不落盘（用户裁定：唯一落盘=手动导入字体到 FontsPath），完成后主线程回调重建；拒绝/失败/
        /// 不支持照常回调（链回落 builtin 头+槽位，等同未扫描）。非浏览器平台无扫描语义，直调回调。
        /// </summary>
        public static void ScanSystemFontsAsync(Action onCompleted) {
#if BROWSER
            ScanSystemFontsCoreAsync(onCompleted);
#else
            onCompleted?.Invoke();
#endif
        }

#if BROWSER
        /// <summary>await 续体回主线程（同 Storage.ChooseFile 浏览器分支口径），不经 Task.Run——
        /// JS interop 须主线程派发，且手势上下文经 await 传递给 queryLocalFonts。</summary>
        static async Task ScanSystemFontsCoreAsync(Action onCompleted) {
            try {
                await FontLocator.ScanLocalFontsAsync();
            }
            catch (Exception ex) {
                Log.Warning($"[FontManager] local font scan failed: {ex.Message}");
            }
            onCompleted?.Invoke();
        }
#endif

        /// <summary>OnFontChainsRebuilt 钩子派发（主线程调用面）：仅链确实换入后调用
        /// （Core 失败路径 retired=null 不派发）。异常经 ModsManager.TryInvoke 捕获记日志。</summary>
        static void DispatchFontChainsRebuilt() {
            ModsManager.HookAction("OnFontChainsRebuilt", loader => {
                loader.OnFontChainsRebuilt();
                return false;
            });
        }

        /// <summary>
        /// 平台迁移（!SystemFontsSupported 平台，Initialize 早期执行）："system" 槽位在无系统发现
        /// 平台恒解析 null（链退化），加载设置时主动重置为内置字体并落盘一次；ref 槽随其目标
        /// 基础槽（已迁移）自动落到内置字体，无需改写。
        /// </summary>
        static void MigrateSystemSlotsToBuiltin() {
            if (SystemFontsSupported) {
                return;
            }
            bool changed = false;
            if (IsSystemSlot(SettingsManager.FontBody)) { SettingsManager.FontBody = BuiltinSlotValue; changed = true; }
            if (IsSystemSlot(SettingsManager.FontBodyBold)) { SettingsManager.FontBodyBold = BuiltinSlotValue; changed = true; }
            if (IsSystemSlot(SettingsManager.FontBodyItalic)) { SettingsManager.FontBodyItalic = BuiltinSlotValue; changed = true; }
            if (IsSystemSlot(SettingsManager.FontBodyBoldItalic)) { SettingsManager.FontBodyBoldItalic = BuiltinSlotValue; changed = true; }
            if (IsSystemSlot(SettingsManager.FontMono)) { SettingsManager.FontMono = BuiltinSlotValue; changed = true; }
            if (IsSystemSlot(SettingsManager.FontMonoBold)) { SettingsManager.FontMonoBold = BuiltinSlotValue; changed = true; }
            if (IsSystemSlot(SettingsManager.FontMonoItalic)) { SettingsManager.FontMonoItalic = BuiltinSlotValue; changed = true; }
            if (IsSystemSlot(SettingsManager.FontMonoBoldItalic)) { SettingsManager.FontMonoBoldItalic = BuiltinSlotValue; changed = true; }
            if (changed) {
                SettingsManager.SaveSettings();
            }
        }

        static bool IsSystemSlot(string slotValue) => FontSlotSetting.Parse(slotValue).Kind == FontSlotKind.SystemDefault;

        /// <summary>基础槽是否取内置字体（该族系统尾部不入链的判据）。</summary>
        static bool IsBuiltinSlot(string slotValue) => FontSlotSetting.Parse(slotValue).Kind == FontSlotKind.BuiltIn;

        /// <summary>
        /// 后台重建双链（发现+解析在线程池，不阻塞 UI）；完成后引用原子换入
        /// （引用赋值原子，绘制中的 widget 要么用旧链要么用新链），随后 onCompleted 经
        /// <see cref="Dispatcher.Dispatch"/> 编组在主线程执行（供 T10 设置屏热切换刷新），
        /// 回调跑完后 Dispose 上一代链（I4 热切换 GPU 泄漏修复：旧图集纹理被
        /// PrimitivesRenderer2D 批缓存强引用不会交 GC，须显式释放；陈旧批 IsEmpty 永不
        /// flush，纹理在其下释放安全）。
        /// </summary>
        public static void RebuildChains(Action onCompleted = null) {
            Task.Run(() => {
                List<VectorFont> retired = null;
                try {
                    lock (m_rebuildLock) {
                        retired = RebuildChainsCore();
                    }
                }
                catch (Exception ex) {
                    Log.Error($"[FontManager] chain rebuild failed: {ex}");
                }
                try {
                    Dispatcher.Dispatch(() => {
                        if (onCompleted != null) {
                            try {
                                onCompleted();
                            }
                            catch (Exception ex) {
                                Log.Error($"[FontManager] onCompleted failed: {ex}");
                                // 屏重建失败：旧链可能仍被屏持有，跳过释放（宁泄漏不崩溃）
                                return;
                            }
                        }
                        if (retired != null) {
                            foreach (VectorFont chain in retired) {
                                try {
                                    chain.Dispose();
                                }
                                catch (Exception ex) {
                                    Log.Warning($"[FontManager] previous chain dispose failed: {ex.Message}");
                                }
                            }
                            // Core 失败（retired=null）链未换入，不派发 mod 通知
                            DispatchFontChainsRebuilt();
                        }
                    });
                }
                catch (Exception ex) {
                    Log.Error($"[FontManager] onCompleted dispatch failed: {ex}");
                }
            });
        }

        /// <summary>
        /// 探测槽位源包含的样式面集合（T10 设置屏基础槽联动禁用的依据）。<br/>
        /// user 源读盘/解析（可能数十 MB）在线程池执行，system 源零 IO（取链构建捕获的发现
        /// 样式并集，Windows 逐样式分文件）；结果写入缓存后经 <see cref="Dispatcher.Dispatch"/>
        /// 编组回调主线程；缓存命中时在调用线程同步回调（屏 Enter 恢复禁用态不闪帧）。
        /// onProbed 为 null 时仅预热缓存不回调。失败/空源返回空集合（不锁定任何样式槽）。
        /// </summary>
        public static void ProbeSourceStyles(bool mono, string slotValue, Action<HashSet<VectorFontStyle>> onProbed) {
            if (TryGetSourceStyles(mono, slotValue, out HashSet<VectorFontStyle> cached)) {
                onProbed?.Invoke(cached);
                return;
            }
            string probeKey = ResolveProbeKey(mono, slotValue, depth: 0);
            Task.Run(() => {
                HashSet<VectorFontStyle> styles = ProbeSourceStylesCore(probeKey);
                if (onProbed != null) {
                    try {
                        Dispatcher.Dispatch(() => onProbed(styles));
                    }
                    catch (Exception ex) {
                        Log.Error($"[FontManager] style probe callback dispatch failed: {ex}");
                    }
                }
            });
        }

        /// <summary>
        /// 同步取槽位源样式集（仅缓存命中，屏 Update 恢复联动禁用态用，不触发读盘/解析）；未命中返回 false。
        /// </summary>
        public static bool TryGetSourceStyles(bool mono, string slotValue, out HashSet<VectorFontStyle> styles) {
            string probeKey = ResolveProbeKey(mono, slotValue, depth: 0);
            lock (m_probeLock) {
                return m_sourceStylesCache.TryGetValue(probeKey, out styles);
            }
        }

        /// <summary>槽位值 → 探测键：user 文件直接成键；引用槽递归到目标基础槽取值；builtin/系统按槽位族成键。</summary>
        static string ResolveProbeKey(bool mono, string slotValue, int depth) {
            if (depth > 4) {
                return "none";
            }
            FontSlotSetting setting = FontSlotSetting.Parse(slotValue);
            return setting.Kind switch {
                FontSlotKind.UserFile => $"user:{setting.FileName}",
                FontSlotKind.ReferenceBody => ResolveProbeKey(mono: false, SettingsManager.FontBody, depth + 1),
                FontSlotKind.ReferenceMono => ResolveProbeKey(mono: true, SettingsManager.FontMono, depth + 1),
                FontSlotKind.BuiltIn => "builtin",
                _ => mono ? "system:mono" : "system:body"
            };
        }

        /// <summary>
        /// 探测键 → 样式面集合（双重检查缓存；结果恒非 null）。仅在线程池调用。<br/>
        /// user 源须经 Storage 读取（"data:/" 虚拟路径 File API 不解析，同 LoadUserFontEntry 口径）；
        /// system 源零 IO——取链构建捕获的系统发现样式并集（Windows 逐样式分文件，
        /// 探单文件会漏样式位）。读盘/解析异常不落缓存（下次探测重试，不产生永久空集）；
        /// 确定性空源（用户文件缺失/无系统字体/引用环截断）正常缓存。
        /// </summary>
        static HashSet<VectorFontStyle> ProbeSourceStylesCore(string probeKey) {
            lock (m_probeLock) {
                if (m_sourceStylesCache.TryGetValue(probeKey, out HashSet<VectorFontStyle> cached)) {
                    return cached;
                }
            }
            HashSet<VectorFontStyle> styles = [];
            bool cacheable = false;
            try {
                if (probeKey.StartsWith("user:", StringComparison.Ordinal)) {
                    string path = Storage.CombinePaths(ModsManager.FontsPath, probeKey.Substring(5));
                    if (Storage.FileExists(path)) {
                        CollectFaceStyles(Storage.ReadAllBytes(path), styles);
                    }
                    cacheable = true; // 文件缺失=确定态（同名覆盖导入会显式失效本缓存）
                }
                else if (probeKey.StartsWith("system:", StringComparison.Ordinal)) {
                    bool mono = probeKey.EndsWith("mono", StringComparison.Ordinal);
                    List<SystemFontFace> faces = m_lastSystemFaces;
                    if (faces != null) {
                        foreach (SystemFontFace face in faces) {
                            if (face.IsMono == mono) {
                                styles.Add(face.Style);
                            }
                        }
                    }
                    cacheable = true; // 无系统字体=确定态
                }
                else if (probeKey == "builtin") {
                    byte[] data = LoadPericlesContentData();
                    if (data != null) {
                        CollectFaceStyles(data, styles);
                    }
                    cacheable = true; // 内置内容缺失=确定态
                }
                else {
                    cacheable = true; // "none"（引用环/超深截断）
                }
            }
            catch (Exception ex) {
                Log.Warning($"[FontManager] style probe \"{probeKey}\" failed: {ex.Message}");
            }
            if (cacheable) {
                lock (m_probeLock) {
                    m_sourceStylesCache[probeKey] = styles;
                }
            }
            return styles;
        }

        static void CollectFaceStyles(byte[] data, HashSet<VectorFontStyle> styles) {
            foreach (OpenTypeFont face in OpenTypeFont.LoadAll(data)) {
                styles.Add(FontLocator.MapFaceStyle(face));
            }
        }

        /// <summary>双链构建本体（Discover/读盘/解析）。失败项逐级降级并记日志，正常不抛出。<br/>
        /// 返回值=被换下的上一代链（暂存，不经此 Dispose）：RebuildChains 在 onCompleted
        /// （宿主屏重建回调）跑完后统一释放，修复热切换旧图集纹理泄漏。<br/>
        /// 平台说明：链构建全平台执行——系统发现 WINDOWS/ANDROID/LINUX 编译期实现、BROWSER 运行时
        /// 探测（设置屏手势扫描后 Discover 有结果，未扫描恒空=链回落 builtin 头+槽位），其余平台
        /// （IOS/裸 API 构建，同 FontLocator.None.cs 反集口径）Discover
        /// 恒空：系统尾部无条目、"system" 槽位解析 null（该平台加载期已迁移为内置字体，见
        /// MigrateSystemSlotsToBuiltin），链由内置头/槽位/模组/用户字体构成，全空时才退位图路径。</summary>
        static List<VectorFont> RebuildChainsCore() {
            List<SystemFontFace> systemFaces = FontLocator.Discover() ?? [];
            m_lastSystemFaces = systemFaces;
            // 文件级条目缓存（一次重建内同一文件只读盘/解析一次；槽位引用与系统发现可能重复命中）
            Dictionary<string, ChainEntry> fileEntries = new(StringComparer.OrdinalIgnoreCase);
            List<ChainEntry> bodyEntries = BuildBodyEntries(systemFaces, fileEntries);
            List<ChainEntry> monoEntries = BuildMonoEntries(bodyEntries, systemFaces, fileEntries);
            // 所有可抛点已过：先捕获旧链引用再原子换入新链（暂存链在 RebuildChains 主线程
            // onCompleted 之后释放）。MonoChain 回落 DefaultChain 时两属性同引用，去重防双 Dispose
            List<VectorFont> retired = [];
            if (DefaultChain != null) {
                retired.Add(DefaultChain);
            }
            if (MonoChain != null
                && MonoChain != DefaultChain) {
                retired.Add(MonoChain);
            }
            // mono 链构建失败/无 Regular 面时回落默认链（引用复用，非独立构建）——
            // 代码块等宽排版无专用等宽源时仍可用正文链渲染，不退位图路径
            DefaultChain = BuildChain(bodyEntries, "body");
            MonoChain = BuildChain(monoEntries, "mono") ?? DefaultChain;
            return retired;
        }

        // 条目序即链序：Pericles 头（原版拉丁开关）→ FontBody/FontBodyBold/FontBodyItalic/FontBodyBoldItalic 槽位 → 模组条目（OnBuildingBodyFontChainEntries）→ 系统正文字体
        //（基础槽取 builtin 时系统尾部不入链——内置字体语义=仅用内置字形，CJK 缺字走位图兜底而非系统字体）
        static List<ChainEntry> BuildBodyEntries(List<SystemFontFace> systemFaces, Dictionary<string, ChainEntry> fileEntries) {
            List<ChainEntry> entries = [];
            AddPericlesHead(entries, fileEntries);
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontBody, systemFaces, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontBodyBold, systemFaces, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontBodyItalic, systemFaces, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontBodyBoldItalic, systemFaces, depth: 0, fileEntries));
            AddModEntries(entries, "OnBuildingBodyFontChainEntries", static (loader, list) => loader.OnBuildingBodyFontChainEntries(list));
            if (!IsBuiltinSlot(SettingsManager.FontBody)) {
                AddSystemEntries(entries, systemFaces, mono: false, fileEntries);
            }
            return entries;
        }

        // 条目序即链序：FontMono/FontMonoBold/FontMonoItalic/FontMonoBoldItalic 槽位 → 模组条目（OnBuildingMonoFontChainEntries）→ 系统等宽 → body 条目尾部复用
        // （等宽字体普遍缺 CJK，尾部复用使 mono 段 CJK 沿链回落到正文字体渲染）
        static List<ChainEntry> BuildMonoEntries(List<ChainEntry> bodyEntries, List<SystemFontFace> systemFaces, Dictionary<string, ChainEntry> fileEntries) {
            List<ChainEntry> entries = [];
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontMono, systemFaces, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontMonoBold, systemFaces, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontMonoItalic, systemFaces, depth: 0, fileEntries));
            TryAddEntry(entries, ResolveSlot(SettingsManager.FontMonoBoldItalic, systemFaces, depth: 0, fileEntries));
            AddModEntries(entries, "OnBuildingMonoFontChainEntries", static (loader, list) => loader.OnBuildingMonoFontChainEntries(list));
            if (!IsBuiltinSlot(SettingsManager.FontMono)) {
                AddSystemEntries(entries, systemFaces, mono: true, fileEntries);
            }
            foreach (ChainEntry entry in bodyEntries) {
                TryAddEntry(entries, entry);
            }
            return entries;
        }

        /// <summary>
        /// 模组字体条目并入（钩子回调逐 loader 收集后统一去重入链）：回调可能在线程池执行，
        /// 异常经 ModsManager.TryInvoke 捕获记日志不中断重建。同 Origin 先到先得——mod 回调
        /// 每次新建 ChainEntry 实例，TryAddEntry 的引用去重失效，须按 Origin 判。
        /// </summary>
        static void AddModEntries(List<ChainEntry> entries, string hookName, Action<ModLoader, List<ChainEntry>> hook) {
            List<ChainEntry> modEntries = [];
            ModsManager.HookAction(hookName, loader => {
                hook(loader, modEntries);
                return false;
            });
            foreach (ChainEntry entry in modEntries) {
                TryAddEntryByOrigin(entries, entry);
            }
        }

        static void TryAddEntryByOrigin(List<ChainEntry> entries, ChainEntry entry) {
            if (entry == null
                || entries.Any(existing => existing.Origin == entry.Origin)) {
                return;
            }
            entries.Add(entry);
        }

        /// <summary>
        /// 槽位值 → 链条目。SystemDefault 返回 null（系统发现在条目列表尾部统一处理）；<br/>
        /// BuiltIn 返回内置字体条目（PericlesPro 四样式面，全平台可用）；<br/>
        /// 引用槽（ref:body/ref:mono）解析到目标槽自身的取值，目标槽为系统默认/用户文件缺失时返回 null——
        /// 引用系统默认≡系统默认（同走尾部系统发现，不锚独立条目，模组条目保持其链序优先级）；<br/>
        /// A→B→A 引用环按深度截断。
        /// </summary>
        static ChainEntry ResolveSlot(string slotValue, List<SystemFontFace> systemFaces, int depth, Dictionary<string, ChainEntry> fileEntries) {
            FontSlotSetting setting = FontSlotSetting.Parse(slotValue);
            switch (setting.Kind) {
                case FontSlotKind.BuiltIn:
                    return GetPericlesEntry(fileEntries);
                case FontSlotKind.UserFile:
                    return LoadUserFontEntry(setting.FileName);
                case FontSlotKind.ReferenceBody:
                    return depth > 0
                        ? null
                        : ResolveSlot(SettingsManager.FontBody, systemFaces, depth + 1, fileEntries);
                case FontSlotKind.ReferenceMono:
                    return depth > 0
                        ? null
                        : ResolveSlot(SettingsManager.FontMono, systemFaces, depth + 1, fileEntries);
                default:
                    return null;
            }
        }

        /// <summary>Pericles 链头：原版拉丁字形（FontVanilaEnglish 开关控制）。与内置字体槽位
        /// （FontSlotKind.BuiltIn）共享条目缓存——开关开启且槽位取 builtin 时同引用去重不重复入链。</summary>
        static void AddPericlesHead(List<ChainEntry> entries, Dictionary<string, ChainEntry> fileEntries) {
            if (!SettingsManager.FontVanilaEnglish) {
                return;
            }
            TryAddEntry(entries, GetPericlesEntry(fileEntries));
        }

        /// <summary>内置字体（Content Fonts/PericlesPro 四样式面）→ 链条目，一次重建内共享缓存
        /// （键 "Pericles"，VanilaEnglish 头与 builtin 槽位同源同引用）。内容缺失/解析失败降级 null（槽位跳过）。</summary>
        static ChainEntry GetPericlesEntry(Dictionary<string, ChainEntry> fileEntries) {
            if (fileEntries.TryGetValue("Pericles", out ChainEntry cached)) {
                return cached;
            }
            ChainEntry entry = null;
            try {
                byte[] data = LoadPericlesContentData();
                if (data != null) {
                    entry = VectorFontReader.BuildEntries(data, "Pericles")[0];
                }
                else {
                    Log.Information("[FontManager] \"Fonts/PericlesPro\" not found in content, built-in font entry skipped.");
                }
            }
            catch (Exception ex) {
                Log.Error($"[FontManager] built-in font entry build failed: {ex}");
            }
            fileEntries.Add("Pericles", entry);
            return entry;
        }

        /// <summary>内置字体内容字节（PericlesPro 按后缀探测；ContentInfo 注册期共享流只读不释放，ToArray 与 VectorFontReader.Get 同口径）。</summary>
        static byte[] LoadPericlesContentData() {
            foreach (string suffix in new VectorFontReader().DefaultSuffix) {
                Stream stream = ContentManager.GetStream($"Fonts/PericlesPro.{suffix}");
                if (stream == null) {
                    continue;
                }
                return stream is MemoryStream memoryStream ? memoryStream.ToArray() : CopyToBytes(stream);
            }
            return null;
        }

        /// <summary>系统发现 → 条目（同文件去重；逐文件失败降级继续）。</summary>
        static void AddSystemEntries(List<ChainEntry> entries, List<SystemFontFace> systemFaces, bool mono, Dictionary<string, ChainEntry> fileEntries) {
            HashSet<string> added = new(StringComparer.OrdinalIgnoreCase);
            foreach (SystemFontFace face in systemFaces) {
                if (face.IsMono != mono
                    || !added.Add($"{face.FilePath}#{face.FaceIndex}")) {
                    continue;
                }
                TryAddEntry(entries, GetFileEntry(face, fileEntries));
            }
        }

        /// <summary>
        /// 系统字体面 → 链条目。只入发现面指定的那一个 face（缓存键 path#face）——
        /// ttc 容器序首面未必是目标地区变体（NotoSansCJK/SECCJK 面 0=JP），全面入链会让
        /// 渲染回落撞到 JP 面字形（"置"显示日文写法）；face 由系统 AFontMatcher 按语言
        /// 挑定（Windows 打表路线由 ProbeFontFile 按样式位挑定）。失败不重试。
        /// </summary>
        static ChainEntry GetFileEntry(SystemFontFace face, Dictionary<string, ChainEntry> fileEntries) {
            string cacheKey = $"{face.FilePath}#{face.FaceIndex}";
            if (fileEntries.TryGetValue(cacheKey, out ChainEntry cached)) {
                return cached;
            }
            ChainEntry entry = null;
            try {
                // 浏览器 Local Font Access 路线：字节内存持有（Data），FilePath 是展示串非盘上路径
                byte[] data = face.Data ?? File.ReadAllBytes(face.FilePath);
                entry = VectorFontReader.BuildEntry(data, $"system:{Storage.GetFileName(face.FilePath)}", face.FaceIndex, face.Style);
            }
            catch (Exception ex) {
                Log.Warning($"[FontManager] font file \"{face.FilePath}\" face {face.FaceIndex} load failed: {ex.Message}");
            }
            fileEntries.Add(cacheKey, entry);
            return entry;
        }

        /// <summary>用户字体文件 → 链条目（文件缺失/解析失败降级 null，槽位跳过）。</summary>
        static ChainEntry LoadUserFontEntry(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return null;
            }
            string path = Storage.CombinePaths(ModsManager.FontsPath, fileName);
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
        /// 一步构建模组自用字体链（不做全局注册，仅供模组自己的 FontTextWidget 使用）：
        /// 样式面按 OS/2 fsSelection 位自动映射；Regular 面缺失或解析失败返回 null（记日志）。
        /// <paramref name="fallback"/> 缺省=引擎位图回退字体（缺字兜底）。
        /// </summary>
        public static VectorFont CreateFont(byte[] data, string origin, BitmapFont fallback = null) {
            try {
                List<ChainEntry> entries = VectorFontReader.BuildEntries(data, origin);
                if (!entries[0].StyleFaces.ContainsKey(VectorFontStyle.Regular)) {
                    Log.Warning($"[FontManager] CreateFont \"{origin}\" has no Regular face, returning null.");
                    return null;
                }
                return new VectorFont(entries, fallback ?? FallbackBitmapFont);
            }
            catch (Exception ex) {
                Log.Error($"[FontManager] CreateFont \"{origin}\" failed: {ex}");
                return null;
            }
        }

        /// <summary>
        /// 字体文件包含的样式面集合（ttc 逐面按 OS/2 fsSelection 位判定；解析失败返回空集）。<br/>
        /// 模组在决定 <see cref="VectorFontReader.BuildEntry"/> 的 faceIndex/样式前的查询入口。
        /// </summary>
        public static HashSet<VectorFontStyle> GetFontFileStyles(byte[] data) {
            HashSet<VectorFontStyle> styles = [];
            try {
                CollectFaceStyles(data, styles);
            }
            catch (Exception ex) {
                Log.Warning($"[FontManager] font style probe failed: {ex.Message}");
            }
            return styles;
        }

        /// <summary>
        /// 校验并导入用户字体：SharpAstro 试解析（0 面即拒）+ Latin 必备字符（A-Z a-z 0-9）覆盖探测，
        /// 通过后写入 <see cref="ModsManager.FontsPath"/>（同名覆盖）。失败时 error 为原因文本并返回 false，不抛出。
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
                    error = $"Unsupported font type \"{suffix}\" (expect ttf/otf/ttc/woff/woff2).";
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
                if (!Storage.DirectoryExists(ModsManager.FontsPath)) {
                    Storage.CreateDirectory(ModsManager.FontsPath);
                }
                Storage.WriteAllBytes(Storage.CombinePaths(ModsManager.FontsPath, safeName), data);
                // 同名覆盖导入使旧探测缓存失效（源内容已变，缓存键仍为 "user:<文件名>"）
                lock (m_probeLock) {
                    _ = m_sourceStylesCache.Remove($"user:{safeName}");
                }
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
                if (!Storage.DirectoryExists(ModsManager.FontsPath)) {
                    return [];
                }
                return Storage.ListFileNames(ModsManager.FontsPath)
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
