using System.Xml.Linq;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GameScreen : Screen {
        public double m_lastAutosaveTime;

        public GameScreen() {
            XElement node = ContentManager.Get<XElement>("Screens/GameScreen");
            LoadContents(this, node);
            IsDrawRequired = true;
            Window.Deactivated += delegate { GameManager.SaveProject(true, false); };
            Window.Closed += delegate { GameManager.DisposeProject(); };
        }

        public override void Enter(object[] parameters) {
            if (GameManager.Project != null) {
                GameManager.Project.FindSubsystem<SubsystemAudio>(true).Unmute();
                EnsureDebugMultiplayerPlayer();
            }
            MusicManager.StopMusic();
            MusicManager.CurrentMix = MusicManager.Mix.InGame;
        }

        private static void EnsureDebugMultiplayerPlayer() {
            if (GameManager.Project == null) {
                return;
            }

            SubsystemPlayers subsystemPlayers = GameManager.Project.FindSubsystem<SubsystemPlayers>();
            if (subsystemPlayers == null || subsystemPlayers.PlayersData.Any(playerData => playerData.IsMainPlayer)) {
                return;
            }

            if (Program.IsDebugMultiplayerClient
                && NetworkManager.IsClientRunning
                && Program.BeginDebugMultiplayerPlayerCreation()) {
                // The server assigns the final PlayerIndex and broadcasts the
                // resulting PlayerData back to this client.
                PlayerData playerData = new PlayerData(GameManager.Project);
                ModsManager.HookAction("OnNetworkPlayerCreate", loader => loader.OnNetworkPlayerCreate(playerData));
            }
        }

        public override void Leave() {
            if (GameManager.Project != null) {
                GameManager.Project.FindSubsystem<SubsystemAudio>(true).Mute();
                GameManager.SaveProject(true, true);
            }
            ShowHideCursors(true);
            MusicManager.CurrentMix = MusicManager.Mix.Menu;
        }

        public override void Update() {
            if (GameManager.Project != null) {
                double realTime = Time.RealTime;
                if (realTime - m_lastAutosaveTime > 300.0) {
                    m_lastAutosaveTime = realTime;
                    GameManager.SaveProject(false, true);
                    SettingsManager.SaveSettings();
                }
                if (MarketplaceManager.IsTrialMode
                    && GameManager.Project.FindSubsystem<SubsystemGameInfo>(true).TotalElapsedGameTime > 1140.0) {
                    GameManager.SaveProject(true, false);
                    GameManager.DisposeProject();
                    ScreensManager.SwitchScreen("TrialEnded");
                }
                GameManager.UpdateProject();
                if (Program.IsDebugMultiplayerHost) {
                    SubsystemPlayers subsystemPlayers = GameManager.Project.FindSubsystem<SubsystemPlayers>();
                    if (subsystemPlayers?.MainPlayerData?.IsReadyForPlaying == true) {
                        // The host is ready only after its local player has
                        // spawned and entered the Playing state.
                        Program.SignalDebugMultiplayerHostReady();
                    }
                }
            }
            ShowHideCursors(
                GameManager.Project == null
                || DialogsManager.Dialogs.Any(dialog => dialog.IsVisibleGlobal && dialog.IsChildWidgetOf(this))
                || DialogsManager.HasDialogs(RootWidget)
                || ScreensManager.CurrentScreen != this
            );
        }

        public override void Draw(DrawContext dc) {
            if (!ScreensManager.IsAnimating
                && SettingsManager.ResolutionMode == ResolutionMode.High) {
                Display.Clear(Color.Black, 1f, 0);
            }
        }

        public void ShowHideCursors(bool show) {
            Input.IsMouseCursorVisible = show;
            Input.IsPadCursorVisible = show;
        }
    }
}
