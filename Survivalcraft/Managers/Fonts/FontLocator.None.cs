// 非 WINDOWS/ANDROID 平台（IOS/BROWSER/LINUX 及裸 net10.0 的 API 库构建）：平台级位图回退（spec §1）。
// 有意不用 brief 原稿的 "#if IOS || BROWSER || LINUX" 枚举：裸 net10.0（Survivalcraft.csproj）
// 不定义任何平台常量，枚举写法会使 FontLocator.Discover 缺失、FontManager 编译失败；
// 穷举反集保证任何 TFM 下恰好有一个 Discover 实现。
#if !(WINDOWS || ANDROID)
namespace Game {
    static partial class FontLocator {
        public static List<SystemFontFace> Discover() => [];
    }
}
#endif
