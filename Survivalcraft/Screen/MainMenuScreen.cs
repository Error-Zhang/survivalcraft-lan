using System.Xml.Linq;
using Engine;
using Engine.Input;


namespace Game {
    public class MainMenuScreen : Screen {
        public ButtonWidget m_disableSafeModeButton;
        public ButtonWidget m_languageSwitchButton;
        public ButtonWidget m_fullscreenButton;
        public ButtonWidget m_dashboardButton;
        public StackPanelWidget m_leftBottomBar;
        public StackPanelWidget m_rightBottomBar;

        public const string fName = "MainMenuScreen";

        private ButtonWidget? _connectServerButton;

        public MainMenuScreen() {
            XElement node = ContentManager.Get<XElement>("Screens/MainMenuScreen");
            LoadContents(this, node);
            m_disableSafeModeButton = Children.Find<ButtonWidget>("DisableSafeModeButton");
            m_languageSwitchButton = Children.Find<ButtonWidget>("LanguageSwitchButton", false);
            m_leftBottomBar = Children.Find<StackPanelWidget>("LeftBottomBar", false);
            m_rightBottomBar = Children.Find<StackPanelWidget>("RightBottomBar", false);
            m_fullscreenButton = Children.Find<ButtonWidget>("FullscreenButton", false);
            m_dashboardButton = Children.Find<ButtonWidget>("DashboardButton", false);
#if BROWSER
            m_fullscreenButton.IsVisible = true;
            m_dashboardButton.IsVisible = true;
#endif
            ModsManager.HookAction(
                "OnMainMenuScreenCreated",
                loader => {
                    loader.OnMainMenuScreenCreated(this, m_leftBottomBar, m_rightBottomBar);
                    return false;
                }
            );

            if (_connectServerButton != null) {
                return;
            }

            StackPanelWidget? centerButtons = Children.Find<StackPanelWidget>("CenterButtons", false);
            if (centerButtons == null) {
                return;
            }

            BevelledButtonWidget? playButton = Children.Find<BevelledButtonWidget>("Play", false);

            BevelledButtonWidget button = new() {
                Size = new Vector2(310f, 60f),
                Name = "Multiplayer",
                Text = LanguageControl.GetContentWidgets("LanMultiplayer", "Multiplayer"),
                HorizontalAlignment = WidgetAlignment.Center,
                VerticalAlignment = WidgetAlignment.Center
            };

            if (playButton != null) {
                button.BevelSize = playButton.BevelSize;
                button.Color = playButton.Color;
                button.BevelColor = playButton.BevelColor;
                button.CenterColor = playButton.CenterColor;
                button.AmbientLight = playButton.AmbientLight;
                button.DirectionalLight = playButton.DirectionalLight;
                button.Font = playButton.Font;
                button.FontScale = playButton.FontScale;
                button.Margin = playButton.Margin;
            }

            _connectServerButton = button;

            Widget? buyButton = Children.Find<Widget>("Buy", false);
            int insertIndex = -1;
            if (buyButton != null) {
                insertIndex = centerButtons.Children.IndexOf(buyButton);
            }
            if (insertIndex >= 0) {
                centerButtons.Children.Insert(insertIndex, button);
            }
            else {
                centerButtons.Children.Add(button);
            }
        }

        public override void Enter(object[] parameters) {
            MusicManager.CurrentMix = MusicManager.Mix.Menu;
            if (SettingsManager.IsolatedStorageMigrationCounter < 3) {
                SettingsManager.IsolatedStorageMigrationCounter++;
                VersionConverter126To127.MigrateDataFromIsolatedStorageWithDialog();
            }
            if (SettingsManager.SafeMode) {
                m_disableSafeModeButton.IsVisible = true;
            }
            m_leftBottomBar.MarginLeft = SettingsManager.AdaptEdgeToEdgeDisplay ? Window.DisplayCutoutInsets.X * ScreensManager.FinalUiScale : 0f;
            m_rightBottomBar.MarginRight = SettingsManager.AdaptEdgeToEdgeDisplay ? Window.DisplayCutoutInsets.Z * ScreensManager.FinalUiScale : 0f;
        }

        public override void Leave() {
            Keyboard.BackButtonQuitsApp = false;
        }

        public override void Update() {
            Keyboard.BackButtonQuitsApp = !MarketplaceManager.IsTrialMode;
            Children.Find("Buy").IsVisible = MarketplaceManager.IsTrialMode;
            RectangleWidget rectangleWidget = Children.Find<RectangleWidget>("Logo");
            float num = 1f + 0.02f * MathF.Sin(1.5f * (float)MathUtils.Remainder(Time.FrameStartTime, 10000.0));
            rectangleWidget.RenderTransform =
                Matrix.CreateTranslation((0f - rectangleWidget.ActualSize.X) / 2f, (0f - rectangleWidget.ActualSize.Y) / 2f, 0f)
                * Matrix.CreateScale(num, num, 1f)
                * Matrix.CreateTranslation(rectangleWidget.ActualSize.X / 2f, rectangleWidget.ActualSize.Y / 2f, 0f);
            if (m_languageSwitchButton?.IsClicked ?? false) {
                LanguageControl.CreateLanguageSelectionDialog(null);
            }
#if BROWSER
            if (m_fullscreenButton.IsClicked) {
                Window.WindowMode = Window.WindowMode == WindowMode.Fullscreen ? WindowMode.Fixed : WindowMode.Fullscreen;
            }
            if (m_dashboardButton.IsClicked) {
                WebBrowserManager.LaunchBrowser("./dashboard.html");
            }
#endif
            if (Children.Find<ButtonWidget>("Play").IsClicked) {
                ScreensManager.SwitchScreen("Play");
            }
            if (Children.Find<ButtonWidget>("Help").IsClicked) {
                ScreensManager.SwitchScreen("Help");
            }
            if (Children.Find<ButtonWidget>("Content").IsClicked) {
                ScreensManager.SwitchScreen("Content");
            }
            if (Children.Find<ButtonWidget>("Settings").IsClicked) {
                ScreensManager.SwitchScreen("Settings");
            }
            if (Children.Find<ButtonWidget>("Buy").IsClicked) {
                MarketplaceManager.ShowMarketplace();
            }
            if (m_disableSafeModeButton.IsClicked) {
                SettingsManager.SafeMode = false;
                DialogsManager.ShowDialog(
                    null,
                    new MessageDialog(
                        LanguageControl.Warning,
                        LanguageControl.Get("SettingsCompatibilityScreen", "4"),
                        LanguageControl.Ok,
                        null,
                        null
                    )
                );
            }
            if ((Input.Back || Input.IsKeyDownOnce(Key.Escape)) && !Keyboard.BackButtonQuitsApp) {
                if (MarketplaceManager.IsTrialMode) {
                    ScreensManager.SwitchScreen("Nag");
                }
                else {
                    Window.Close();
                }
            }
            /*if (!string.IsNullOrEmpty(ExternalContentManager.openFilePath)) {
                ScreensManager.SwitchScreen("ExternalContent");
            }*/
            if (_connectServerButton != null
                && _connectServerButton.IsClicked) {
                ScreensManager.SwitchScreen("Multiplayer");
            }
        }
    }
}