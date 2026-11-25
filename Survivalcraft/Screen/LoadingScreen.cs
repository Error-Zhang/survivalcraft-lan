using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using Engine;
using Engine.Graphics;
using Engine.Media;
using Game.Animation;
using NCalc;
#if ANDROID
using Android.App;
#elif WINDOWS
//using System.Windows.Forms;
#endif

namespace Game {
    public class LoadingScreen : Screen {
        public enum LogType {
            Info = 0,
            Warning = 1,
            Error = 2,
            Advice = 3
        }

        class LogItem(LogType type, string log) {
            public LogType LogType = type;
            public string Message = log;
        }

        List<Action> LoadingActoins = [];
        List<Action> ModLoadingActoins = [];
        CanvasWidget Canvas = new() { Size = new Vector2(float.PositiveInfinity) };

        RectangleWidget Background = new() {
            FillColor = SettingsManager.DisplayLog ? Color.Black : Color.White,
            OutlineThickness = 0f,
            DepthWriteEnabled = true
        };

        static ListPanelWidget LogList;
        public static bool m_isContentLoaded;
        public const string fName = "LoadingScreen";

        static LoadingScreen() {
            if (SettingsManager.DisplayLog) {
                LogList = new ListPanelWidget { Direction = LayoutDirection.Vertical, PlayClickSound = false };
                LogList.ItemWidgetFactory = obj => {
                    if (obj is LogItem logItem) {
                        CanvasWidget canvasWidget = new() {
                            Size = new Vector2(Display.Viewport.Width, 40),
                            Margin = new Vector2(0, 2),
                            HorizontalAlignment = WidgetAlignment.Near
                        };
                        FontTextWidget fontTextWidget = new() {
                            FontScale = 0.6f,
                            Text = logItem.Message,
                            Color = GetColor(logItem.LogType),
                            VerticalAlignment = WidgetAlignment.Center,
                            HorizontalAlignment = WidgetAlignment.Near
                        };
                        canvasWidget.Children.Add(fontTextWidget);
                        canvasWidget.IsVisible = true;
                        return canvasWidget;
                    }
                    return null;
                };
                LogList.ItemSize = 30;
            }
        }

        public static Color GetColor(LogType type) {
            return type switch {
                LogType.Advice => Color.Cyan,
                LogType.Error => Color.Red,
                LogType.Warning => Color.Yellow,
                _ => Color.White
            };
        }

        public LoadingScreen() {
            if (SettingsManager.DisplayLog) {
                Canvas.AddChildren(Background);
                Canvas.AddChildren(LogList);
                AddChildren(Canvas);
            }
            Info($"Initializing Mods CommonLib. Api Version: {ModsManager.APIVersionString}");
        }

        public void ContentLoaded() {
            if (SettingsManager.DisplayLog) {
                _ = ContentManager.Get<Image>("Fonts/Pericles", ".webp");
                m_isContentLoaded = true;
                return;
            }
            ClearChildren();
            RectangleWidget rectangle1 = new() {
                FillColor = Color.White,
                OutlineColor = Color.Transparent,
                Size = new Vector2(256f),
                VerticalAlignment = WidgetAlignment.Center,
                HorizontalAlignment = WidgetAlignment.Center
            };
            rectangle1.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/CandyRufusLogo");
            RectangleWidget rectangle2 = new() {
                FillColor = Color.White,
                OutlineColor = Color.Transparent,
                Size = new Vector2(80f, 50f),
                VerticalAlignment = WidgetAlignment.Far,
                HorizontalAlignment = WidgetAlignment.Far,
                Margin = new Vector2(10f)
            };
            rectangle2.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/EngineLogo");
            BusyBarWidget busyBar = new() {
                VerticalAlignment = WidgetAlignment.Far,
                HorizontalAlignment = WidgetAlignment.Center,
                Margin = new Vector2(0, 40)
            };
            Canvas.AddChildren(Background);
            Canvas.AddChildren(rectangle1);
            Canvas.AddChildren(rectangle2);
            Canvas.AddChildren(busyBar);
            AddChildren(Canvas);
            m_isContentLoaded = true;
            Task.Run(() => {
                _ = ContentManager.Get<Image>("Fonts/Pericles", ".webp");
            });
        }

        // 日志已经附带状态，不需要添加状态字符串
        public static void Error(string mesg) {
            Add(LogType.Error, mesg);
        }

        public static void Info(string mesg) {
            Add(LogType.Info, mesg);
        }

        public static void Warning(string mesg) {
            Add(LogType.Warning, mesg);
        }

        public static void Advice(string mesg) {
            Add(LogType.Advice, $"[Advice]{mesg}");
        }

        public static void Add(LogType type, string mesg) {
            Dispatcher.Dispatch(
                delegate {
                    switch (type) {
                        case LogType.Info:
                        case LogType.Advice: Log.Information(mesg); break;
                        case LogType.Error: Log.Error(mesg); break;
                        case LogType.Warning: Log.Warning(mesg); break;
                    }
                    if (SettingsManager.DisplayLog && LogList != null) {
                        LogItem item = new(type, mesg);
                        LogList.AddItem(item);
                        LogList.ScrollToItem(item);
                    }
                }
            );
        }

        void InitActions() {
            bool isLoadSucceed = true;
            Exception exception = null;
            AddLoadAction(
                delegate { // 将所有的有效的scmod读取为ModEntity，并自动添加SurvivalCraftModEntity
                    ContentManager.Initialize();
                    ModsManager.Initialize();
                }
            );
            AddLoadAction(delegate {
                // 中文社区组合根接线：传输反馈的登录态回退延迟求值（仅真正上报时解析），接线前匿名上报。
                TransferManager.ConfigureFeedbackProviders(
                    () => AuthSessionManager.Instance.CurrentToken,
                    () => AuthSessionManager.Instance.ApiBaseUrl);
                // 按当前服务器激活会话（先于任何设置写入）：ApiBaseUrl 派生自
                // CommunityServerManager.CurrentChineseInfo，后续切服由 ChangeChineseInfo 直接通知。
                AuthSessionManager.Instance.OnServerSwitched();
                TransactionalFileReplacement.RecoverLeftoverBackups(ModsManager.ModsPath, ModsManager.DocPath);
                // 先于一切业务读取：修复上次进程死于 MoveFile 两步之间时 CommunityStoragePaths.RootDirectory 下的状态丢失。
                JsonFileStore.RecoverLeftoverTempFiles(CommunityStoragePaths.RootDirectory);
                // ModPacks 主文件/meta 与 JsonFileStore 同款 .tmp 命名，同因写入：Move 阶段死亡时包文件唯一副本成孤儿 .tmp。
                JsonFileStore.RecoverLeftoverTempFiles(ModpackRegistry.Shared.DirectoryPath);
                PendingLocalModActions.Process("startup-early", includeSettingActions: false);
            });
            AddLoadAction(
                delegate {
#if ANDROID
                    // On Android VR headsets, force enable VR when available
                    if (VrManager.IsVrAvailable && !SettingsManager.UseVr) {
                        SettingsManager.UseVr = true;
                    }
#endif
                    if (SettingsManager.UseVr) {
                        // StartVr waits for the background init (Windows) to finish
                        // and returns false if VR ended up unavailable. Don't gate on
                        // IsVrAvailable here — that flag is still resolving async at
                        // this early load step and would skip VR auto-start.
                        try {
                            VrManager.StartVr();
                            if (!VrManager.IsVrStarted) {
                                SettingsManager.UseVr = false;
                            }
                        }
                        catch (Exception ex) {
                            SettingsManager.UseVr = false;
                            Log.Error($"VR start error: {ex}");
                        }
                    }
                }
            );
            AddLoadAction(
                delegate { //检查所有Mod依赖项
                    //根据加载顺序排序后的结果
                    ModsManager.ModList.Clear();
                    foreach (ModEntity item in ModsManager.ModListAll) {
                        // 待重启实体未生效，不参与依赖检查也不进 ModList（后续内容加载只遍历 ModList，自然不可达）
                        if (item.IsDependencyChecked || item.IsDisabled || item.IsPendingRestart) {
                            continue;
                        }
                        item.CheckDependencies(ModsManager.ModList);
                    }
                    if (ModsManager.ModListAll.Count <= 2) {
                        return;
                    }
                    StringBuilder sb = new("Mod List:");
                    foreach (ModEntity entity in ModsManager.ModListAll) {
                        if (string.IsNullOrEmpty(entity.ModFilePath)) {
                            continue;
                        }
                        sb.AppendLine();
                        ModInfo info = entity.modInfo;
                        if (info == null) {
                            sb.Append($"[Invalid] FileName: {Storage.GetFileName(entity.ModFilePath)}");
                        }
                        else {
                            string status = entity.IsDisabled ? entity.DisableReason.ToString() : "Enabled";
                            sb.Append(
                                $"[{status}] Name: {info.Name}  Version: {info.Version}  PackageName: {info.PackageName}  FileName: {Storage.GetFileName(entity.ModFilePath)}"
                            );
                        }
                    }
                    Log.Information(sb.ToString());
                }
            );
            AddLoadAction(() => {
                foreach (ModEntity modEntity in ModsManager.ModList) {
                    modEntity.CombineContent();
                }
                ModsManager.DisposeNotEnabledModsResources();
            });
            AddLoadAction(ContentLoaded);
            AddLoadAction(() => {
                ModsManager.ModHooks.Clear();
                ModsManager.m_tempModHooks.Clear();

                Dictionary<ModEntity, List<ModAssemblyImage>> assemblyImages =
                    new(ReferenceEqualityComparer.Instance);
                List<ModAssemblyImage> ordinaryImages = [];

                void RegisterAssembly(Assembly assembly) {
                    AssemblyName assemblyName = assembly.GetName();
                    string fullName = assemblyName.FullName;
                    if (ModsManager.Dlls.TryGetValue(fullName, out Assembly existingAssembly)) {
                        Version existingVersion = existingAssembly.GetName().Version;
                        if (existingVersion == null || assemblyName.Version > existingVersion) {
                            ModsManager.Dlls[fullName] = assembly;
                        }
                    }
                    else {
                        ModsManager.Dlls.Add(fullName, assembly);
                    }
                }

                void FailAssembly(Exception e, ModEntity owner, ModAssemblyImage image, Assembly assembly) {
                    exception = e;
                    string separator = new('-', 10);
                    Log.Error($"{separator}Handle assembly failed{separator}");
                    Log.Error(
                        $"Loaded assembly:\n{string.Join("\n", AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName ?? x.GetName().FullName))}"
                    );
                    Log.Error(separator);
                    Log.Error($"Error mod: {owner.modInfo?.Name}");
                    Log.Error($"Error assembly: {image?.FileName ?? assembly?.FullName}");
                    if (assembly != null) {
#pragma warning disable IL2026
                        Log.Error($"Dependencies:\n{string.Join("\n", assembly.GetReferencedAssemblies().Select(x => x.FullName))}");
#pragma warning restore IL2026
                    }
                    Log.Error(separator);
                    Log.Error(e);
                    isLoadSucceed = false;
                }

                void LoadImage(ModEntity owner, ModAssemblyImage image) {
                    if (!isLoadSucceed || image.Skip) {
                        return;
                    }
                    try {
#pragma warning disable IL2026
                        image.LoadedAssembly = Assembly.Load(image.Bytes);
#pragma warning restore IL2026
                        RegisterAssembly(image.LoadedAssembly);
                    }
                    catch (Exception e) {
                        FailAssembly(e, owner, image, null);
                    }
                }

                void HandleImage(ModEntity owner, ModAssemblyImage image) {
                    if (!isLoadSucceed || image.Skip || image.LoadedAssembly == null) {
                        return;
                    }
                    Log.Information($"[{owner.modInfo.Name}] Handling assembly [{image.LoadedAssembly.FullName}]");
                    try {
                        owner.HandleAssembly(image.LoadedAssembly);
                    }
                    catch (Exception e) {
                        FailAssembly(e, owner, image, image.LoadedAssembly);
                    }
                }

                void ShowLoadingFailure() {
                    ModsManager.ModList.RemoveAll(entity => entity is not SurvivalCraftModEntity && entity is not FastDebugModEntity);
                    LoadingActoins.RemoveRange(1, LoadingActoins.Count - 1);
                    ModLoadingActoins.Clear();
                    ScreensManager.SwitchScreen(
                        new LoadingFailedScreen(
                            "Loading failed 加载失败",
                            ["Exceptions: 异常信息：", .. exception?.ToString().Split('\n') ?? []],
                            [
                                $"Check the API version required by mod is equal to the current API version ({ModsManager.APIVersionString}). Check and add missing mods. If not solved, please contact the developer of the mods or API with Game.log in the path below.",
                                $"检查模组是否缺失，并添加所缺失的模组。查看模组所需插件版版本与当前插件版版本（{ModsManager.APIVersionString}）是否一致。若以上方式都无法解决，请联系模组、插件版开发者，并发送下面路径中的 Game.log",
                                Storage.GetSystemPath(ModsManager.LogPath),
                                "And you can enable Safe Mode to stop loading any mod.",
                                "你还可以启用安全模式，停止加载任何模组。"
                            ]
                        )
                    );
                }

                // 系统程序集不是来自模组包，但仍需保持原有的系统 ModLoader 初始化流程。
                ModsManager.ModListAllDo(modEntity => {
                    if (isLoadSucceed && modEntity is SurvivalCraftModEntity) {
                        Assembly systemAssembly = typeof(BlocksManager).Assembly;
                        RegisterAssembly(systemAssembly);
                        try {
                            modEntity.HandleAssembly(systemAssembly);
                        }
                        catch (Exception e) {
                            FailAssembly(e, modEntity, null, systemAssembly);
                        }
                    }
                });

                // 预加载模组先加载自身；此阶段不触碰普通模组程序集。
                List<ModEntity> preloadMods = ModsManager.ModList
                    .Where(mod => mod.modInfo?.Preload == true)
                    .ToList();
                foreach (ModEntity modEntity in preloadMods) {
                    if (!isLoadSucceed) {
                        break;
                    }
                    List<ModAssemblyImage> images = modEntity.GetAssemblyImages().ToList();
                    assemblyImages[modEntity] = images;
                    foreach (ModAssemblyImage image in images) {
                        LoadImage(modEntity, image);
                    }
                }
                foreach (ModEntity modEntity in preloadMods) {
                    if (!isLoadSucceed || !assemblyImages.TryGetValue(modEntity, out List<ModAssemblyImage> images)) {
                        break;
                    }
                    foreach (ModAssemblyImage image in images) {
                        HandleImage(modEntity, image);
                    }
                }

                if (isLoadSucceed) {
                    try {
                        ModsManager.InvokePreloadStart();
                        List<string> patchedAssetPaths = ModsManager.ModList
                            .SelectMany(modEntity => modEntity.GetOverriddenAssetPaths())
                            .Distinct(StringComparer.Ordinal)
                            .ToList();
                        if (patchedAssetPaths.Count > 0) {
                            // Rebuild in load order to apply replacements and remove deleted resources.
                            ContentManager.Resources.Clear();
                            foreach (ModEntity modEntity in ModsManager.ModList) {
                                modEntity.CombineContent();
                            }
                            foreach (string assetPath in patchedAssetPaths) {
                                ContentManager.InvalidateResourceCache(assetPath);
                            }
                        }
                    }
                    catch (Exception e) {
                        FailAssembly(e, preloadMods.FirstOrDefault(), null, null);
                    }
                }

                // 普通模组现在只进入内存镜像，不执行 Assembly.Load。
                if (isLoadSucceed) {
                    ModsManager.ModListAllDo(modEntity => {
                        if (!isLoadSucceed
                            || modEntity is SurvivalCraftModEntity
                            || modEntity.modInfo?.Preload == true) {
                            return;
                        }
                        List<ModAssemblyImage> images = modEntity.GetAssemblyImages().ToList();
                        assemblyImages[modEntity] = images;
                        ordinaryImages.AddRange(images);
                    });
                }

                // 所有普通程序集仍未进入 CLR，此时是兼容层修改它们的唯一窗口。
                if (isLoadSucceed) {
                    try {
                        ModsManager.InvokePreloadAssemblies(ordinaryImages);
                    }
                    catch (Exception e) {
                        FailAssembly(e, preloadMods.FirstOrDefault(), null, null);
                    }
                }

                // 改写完成后才统一加载并处理普通程序集。
                foreach (ModAssemblyImage image in ordinaryImages) {
                    if (!isLoadSucceed) {
                        break;
                    }
                    LoadImage(image.Owner, image);
                }
                ModsManager.ModListAllDo(modEntity => {
                    if (!isLoadSucceed
                        || modEntity is SurvivalCraftModEntity
                        || modEntity.modInfo?.Preload == true
                        || !assemblyImages.TryGetValue(modEntity, out List<ModAssemblyImage> images)) {
                        return;
                    }
                    foreach (ModAssemblyImage image in images) {
                        HandleImage(modEntity, image);
                    }
                });

                if (!isLoadSucceed) {
                    ShowLoadingFailure();
                }
            }
            );
            AddLoadAction(
                delegate { // 初始化所有ModEntity的语言包
                    LanguageControl.LanguageTypes.Clear();
                    foreach (string language in new[] { "en-US", "zh-CN" }) {
#if BROWSER
                        LanguageControl.LanguageTypes.Add(language);
#else
                        LanguageControl.LanguageTypes.Add(language, new CultureInfo(language, false));
#endif
                    }
                    if (!ModsManager.Configs.TryGetValue("Language", out string savedLanguage)
                        || savedLanguage is not ("en-US" or "zh-CN")) {
                        string systemLanguage = Program.SystemLanguage?.Split('-', '_')[0];
                        savedLanguage = string.Equals(systemLanguage, "zh", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : "en-US";
                    }
                    LanguageControl.Initialize(savedLanguage);
                    ModsManager.ModListAllDo(modEntity => { modEntity.LoadLauguage(); });
                    LanguageControl.SetUsual();
#if !MOBILE
                    string title =
                        $"{(SettingsManager.SafeMode ? $"[{LanguageControl.Get("Usual", "safeMode")}]" : "")}{LanguageControl.Get("Usual", "gameName")} {ModsManager.ShortGameVersion} - {LanguageControl.Get("Usual", "api")} {ModsManager.APIVersionString}";
#if DEBUG
                    title = $"[{LanguageControl.Get("Usual", "debug")}]{title}";
#endif
                    Window.TitlePrefix = title;
#endif
                }
            );
            AddLoadAction(FontManager.Initialize); //默认矢量链同步构建：必须在语言初始化之后（Android AFontMatcher 按 CurrentLanguageName 选 CJK 面）且先于全部屏构建
            AddLoadAction(ModsManager.ParseAllModSettings); //语言已加载（日志可国际化）+ 模组 dll 已加载（类型可解析）
#if !IOS && !BROWSER
            AddLoadAction(
                delegate { // 读取所有的ModEntity的JavaScript
                    JsInterface.Initiate();
                    ModsManager.ModListAllDo(modEntity => { modEntity.LoadJs(); });
                    JsInterface.RegisterEvent();
                }
            );
#endif
            AddLoadAction(ModsManager.DealWithTempModHooks);
            AddLoadAction(
                delegate {
                    Info(LanguageControl.Get(fName, "1"));
                    List<Action> actions = [];
                    ModsManager.HookAction(
                        "OnLoadingStart",
                        loader => {
                            loader.OnLoadingStart(actions);
                            return false;
                        }
                    );
                    foreach (Action ac in actions) {
                        ModLoadingActoins.Add(ac);
                    }
                }
            );
            AddLoadAction(ClothingSlot.Initialize);
            AddLoadAction(
                delegate { // 初始化TextureAtlas
                    Info(LanguageControl.Get(fName, "2"));
                    TextureAtlasManager.Initialize();
                }
            );
            AddLoadAction(
                delegate { // 初始化Database
                    try {
                        ModsManager.InitImportantDatabaseClasses();
                        DatabaseManager.Initialize();
                        ModsManager.ModListAllDo(modEntity => {
                            modEntity.LoadXdb(ref DatabaseManager.DatabaseNode);
                        });
                        ModsManager.DealWithClassSubstitutes();
                    }
                    catch (Exception e) {
                        Warning(e.ToString());
                    }
                }
            );
            AddLoadAction(
                delegate {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    try {
                        DatabaseManager.LoadDataBaseFromXml(DatabaseManager.DatabaseNode);
                    }
                    catch (Exception e) {
                        Error(e.ToString());
                    }
                    stopwatch.Stop();
                    Info($"{LanguageControl.Get(fName, "3")}({stopwatch.ElapsedMilliseconds}ms)");
                }
            );
            AddLoadAction(
                delegate { // 初始化方块管理器
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    BlocksManager.Initialize();
                    stopwatch.Stop();
                    Info($"{LanguageControl.Get(fName, "4")}({stopwatch.ElapsedMilliseconds}ms)");
                }
            );
            AddLoadAction(CraftingRecipesManager.Initialize); // 初始化合成谱
            AddLoadAction(
                delegate {
                    Info(LanguageControl.Get(fName, "7"));
                    BlocksTexturesManager.Initialize();
                    CharacterSkinsManager.Initialize();
                    CommunityContentManager.Initialize();
                    OriginalCommunityContentManager.Initialize();
                    FurniturePacksManager.Initialize();
                    LightingManager.Initialize();
                    WorldsManager.Initialize();
                }
            );
            AddLoadAction(
                delegate {
                    Info(LanguageControl.Get(fName, "5"));
                    ModSettingsManager.LoadModSettings();
                }
            );
            InitScreens();
            AddLoadAction(FileAssociationManager.Initialize);
            AddLoadAction(() => {
                    //NCalc Warmup
                    Expression expression = new NCalc.Expression("[B] && [N] * 1.0 > 0.5");
                    _ = expression.GetParameterNames();
                    expression.Parameters.Add("B", true);
                    expression.Parameters.Add("N", 0.5f);
                    object expressionResult = expression.Evaluate();
                    _ = Convert.ToBoolean(expressionResult);
                }
            );
            AddLoadAction(() => {
                    AnimationDriverRegistration.Register();
                    AnimationTemplateRegistration.Register();
                }
            );
            AddLoadAction(
                delegate {
                    ModsManager.ModListAllDo(modEntity => {
                        if (modEntity.Loaders.Count > 0) {//modEntity.Loader不为空表示至少设置过一次，即Loaders长度不为0
                            Info($"[{modEntity.modInfo?.Name}] {LanguageControl.Get(fName, "6")}");
                            modEntity.Loaders.ForEach(item => item.OnLoadingFinished(ModLoadingActoins));
                        }
                    });
                }
            );
            AddLoadAction(KeyCompatibleGroupsManager.Initialize); // 初始化按键兼容组
            AddLoadAction(delegate {
                PendingLocalModActions.Process("startup");
                // 开机补报：重试上次会话遗留的待上报游玩时长（队列见 PlayedTimeTracker）。
                PlayedTimeTracker.RetryPendingReports();
                // 全局帧泵：下载收尾/自动更新/认证轮询改由引擎 Window.Frame 驱动，不再依赖 mod 屏 Update。
                CommunityFramePump.Initialize();
            });
            AddLoadAction(
                delegate {
                    string restartReturnScreen = ScreensManager.ConsumeRestartReturnScreen();
                    if (Program.IsDebugMultiplayerHost) {
                        if (!Program.StartupParameters.TryGetValue("debug-world", out string worldDirectory)) {
                            throw new InvalidOperationException("Debug multiplayer host did not receive a world directory.");
                        }
                        WorldInfo worldInfo = WorldsManager.GetWorldInfo(worldDirectory);
                        if (worldInfo == null) {
                            throw new InvalidOperationException($"Debug multiplayer world was not found: {worldDirectory}");
                        }
                        ScreensManager.SwitchScreen("GameLoading", worldInfo, null, false);
                    }
                    else if (Program.IsDebugMultiplayerClient) {
                        WorldInfo worldInfo = WorldsManager.CreateWorld(
                            new WorldSettings {
                                Name = "Debug Multiplayer",
                                OriginalSerializationVersion = VersionsManager.SerializationVersion,
                                GameMode = GameMode.Creative
                            }
                        );
                        Log.Information($"Created debug multiplayer world at {worldInfo.DirectoryName}.");
                        Program.StartDebugMultiplayerHost(
                            worldInfo.DirectoryName,
                            () => ScreensManager.SwitchScreen(
                                "GameLoading",
                                null,
                                null,
                                new IPEndPoint(IPAddress.Loopback, SettingsManager.ServerPort),
                                null
                            )
                        );
                    }
                    else if (Program.StartupParameters.TryGetValue("play", out string worldDirectory)) {
                        ScreensManager.SwitchScreen("Play", worldDirectory);
                    }
                    else if (Program.StartupParameters.TryGetValue("join", out string serverEndpoint)) {
                        ScreensManager.SwitchScreen("GameLoading", null, null, IPEndPoint.Parse(serverEndpoint), null);
                    }
                    else if (restartReturnScreen != null) {
                        ScreensManager.SwitchScreen(restartReturnScreen);
                    }
                    else {
                        ScreensManager.SwitchScreen("MainMenu");
                    }
                }
            );
        }

        void InitScreens() {
            AddLoadAction(delegate { AddScreen("Nag", new NagScreen()); });
            AddLoadAction(delegate { AddScreen("MainMenu", new MainMenuScreen()); });
            AddLoadAction(delegate { AddScreen("Recipaedia", new RecipaediaScreen()); });
            AddLoadAction(delegate { AddScreen("Bestiary", new BestiaryScreen()); });
            AddLoadAction(delegate { AddScreen("BestiaryDescription", new BestiaryDescriptionScreen()); });
            AddLoadAction(delegate { AddScreen("Help", new HelpScreen()); });
            AddLoadAction(delegate { AddScreen("HelpTopic", new HelpTopicScreen()); });
            AddLoadAction(delegate { AddScreen("Settings", new SettingsScreen()); });
            AddLoadAction(delegate { AddScreen("SettingsPerformance", new SettingsPerformanceScreen()); });
            AddLoadAction(delegate { AddScreen("SettingsGraphics", new SettingsGraphicsScreen()); });
            AddLoadAction(delegate { AddScreen("SettingsUi", new SettingsUiScreen()); });
            AddLoadAction(delegate { AddScreen("SettingsMultiplayer", new SettingsMultiplayerScreen()); });
            AddLoadAction(delegate { AddScreen("SettingsCompatibility", new SettingsCompatibilityScreen()); });
            AddLoadAction(delegate { AddScreen("SettingsAudio", new SettingsAudioScreen()); });
            AddLoadAction(delegate { AddScreen("SettingsFont", new SettingsFontScreen()); });
            AddLoadAction(delegate { AddScreen("SettingsControls", new SettingsControlsScreen()); });
            AddLoadAction(delegate { AddScreen("Play", new PlayScreen()); });
            AddLoadAction(delegate { AddScreen("NewWorld", new NewWorldScreen()); });
            AddLoadAction(delegate { AddScreen("ModifyWorld", new ModifyWorldScreen()); });
            AddLoadAction(delegate { AddScreen("WorldOptions", new WorldOptionsScreen()); });
            AddLoadAction(delegate { AddScreen("Multiplayer", new MultiplayerScreen()); });
            AddLoadAction(delegate { AddScreen("GameLoading", new GameLoadingScreen()); });
            AddLoadAction(delegate { AddScreen("Game", new GameScreen()); });
            AddLoadAction(delegate { AddScreen("TrialEnded", new TrialEndedScreen()); });
            AddLoadAction(delegate { AddScreen("ExternalContent", new ExternalContentScreen()); });
            AddLoadAction(delegate { AddScreen("OriginalCommunityContent", new OriginalCommunityContentScreen()); });
            AddLoadAction(delegate { AddScreen("CommunityContent", new CommunityScreen()); });
            AddLoadAction(delegate { AddScreen("CommunitySettings", new CommunitySettingsScreen()); });
            AddLoadAction(delegate { AddScreen("CommunityMyPublished", new CommunityMyPublishedScreen()); });
            AddLoadAction(delegate { AddScreen("Content", new ContentScreen()); });
            AddLoadAction(delegate { AddScreen("ManageContent", new ManageContentScreen()); });
            AddLoadAction(delegate { AddScreen("ModsManageContent", new ModsManageContentScreen()); });
            AddLoadAction(delegate { AddScreen("ModpacksManageContent", new ModpacksManageContentScreen()); });
            AddLoadAction(delegate { AddScreen("Players", new PlayersScreen()); });
            AddLoadAction(delegate { AddScreen("Player", new PlayerScreen()); });
            AddLoadAction(delegate { AddScreen("KeyboardMapping", new KeyboardMappingScreen()); });
            AddLoadAction(delegate { AddScreen("GamepadMapping", new GamepadMappingScreen()); });
            AddLoadAction(delegate { AddScreen("VrControllerMapping", new VrControllerMappingScreen()); });
            AddLoadAction(delegate { AddScreen("CameraManage", new CameraManageScreen()); });
            AddLoadAction(delegate { AddScreen("ManageClassSubstitutes", new ManageClassSubstitutesScreen()); });
            AddLoadAction(delegate { AddScreen("ModSettings", new ModSettingsScreen()); });
        }

        public void AddScreen(string name, Screen screen) {
            ScreensManager.AddScreen(name, screen);
        }

        void AddLoadAction(Action action) {
            LoadingActoins.Add(action);
        }

        public override void Leave() {
            LogList?.ClearItems();
#if ANDROID
            // 当前Android端SDL不支持半垂直同步
            if (SettingsManager.PresentationInterval > 1) {
                SettingsManager.PresentationInterval = 1;
            }
#endif
            Window.PresentationInterval = SettingsManager.PresentationInterval;
            ContentManager.Dispose("Textures/Gui/CandyRufusLogo");
            ContentManager.Dispose("Textures/Gui/EngineLogo");
        }

        public override void Enter(object[] parameters) {
            Window.PresentationInterval = 1;
            List<string> remove = new();
            foreach (KeyValuePair<string, Screen> screen in ScreensManager.m_screens) {
                if (screen.Value == this) {
                    continue;
                }
                remove.Add(screen.Key);
            }
            foreach (string screen in remove) {
                ScreensManager.m_screens.Remove(screen);
            }
            InitActions();
            base.Enter(parameters);
        }

        public override void Update() {
            if (Input.Back
                || Input.Cancel) {
                ConfirmQuit();
            }
            if (!ModsManager.GetAllowContinue()) {
                return;
            }
            Stopwatch sw = Stopwatch.StartNew();
            while (!m_isContentLoaded
                || sw.ElapsedMilliseconds < 100) {
                if (ModLoadingActoins.Count > 0) {
                    try {
                        ModLoadingActoins[0].Invoke();
                    }
                    catch (Exception e) {
                        Error(e.ToString());
                        break;
                    }
                    finally {
                        ModLoadingActoins.RemoveAt(0);
                    }
                }
                else if (LoadingActoins.Count > 0) {
                    try {
                        LoadingActoins[0].Invoke();
                    }
                    catch (Exception e) {
                        Error(e.ToString());
                        break;
                    }
                    finally {
                        LoadingActoins.RemoveAt(0);
                    }
                }
                else {
                    break;
                }
            }
            sw.Stop();
        }

        public void ConfirmQuit() {
#if ANDROID
#pragma warning disable CA1416
            if (Android.OS.Build.VERSION.SdkInt < (Android.OS.BuildVersionCodes)21) {
                Window.Close();
                return;
            }
            Window.Activity.RunOnUiThread(() => {
                    new AlertDialog.Builder(Window.Activity).SetMessage("Exit 退出?")
                        ?.SetPositiveButton("Yes 是", (_, _) => { Window.Close(); })
                        ?.SetNegativeButton("No 否", (_, _) => { })
                        ?.Show();
                }
            );
#pragma warning restore CA1416
#elif WINDOWS
            /*
            Task.Run(() =>
            {
                if(MessageBox.Show("Exit 退出?","", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Window.Close();
                }
            });*/
#endif
        }
    }
}




