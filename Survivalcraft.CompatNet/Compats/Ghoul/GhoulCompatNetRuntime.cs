namespace Game;

/// <summary>
/// Runtime bridges used by the Ghoul startup compatibility patcher.
/// Ghoul state synchronization is intentionally not implemented here.
/// </summary>
public static class GhoulCompatNetRuntime {
    public const string TargetPackageName = "saghoulsmod";
    public const string TargetAssemblyName = "sc-saghoulmod.dll";

    /// <summary>
    /// Resolves the current gameplay GUI root for legacy Ghoul UI code.
    /// </summary>
    public static ContainerWidget GetGuiWidget(GameWidget gameWidget) => gameWidget?.GuiWidget;

    /// <summary>
    /// Supplies a valid world path when Ghoul loads a network project. The
    /// legacy mod assumes GameLoadingScreen is still registered, but the
    /// network project is constructed after that screen has been replaced.
    /// </summary>
    public static string GetWorldPathDefault() {
        string path = ScreensManager.FindScreen<GameLoadingScreen>("GameLoading")?.m_worldInfo?.DirectoryName;
        if (!string.IsNullOrEmpty(path)) {
            return path;
        }

        path = GameManager.WorldInfo?.DirectoryName;
        return !string.IsNullOrEmpty(path) ? path : ModsManager.WorldsDirectoryName;
    }
}
