using System.Xml.Linq;
using Engine;

namespace Game {
    public sealed class MultiplayerScreen : Screen {
        private readonly ListPanelWidget m_lanRooms;
        private readonly LabelWidget m_lanEmpty;
        private readonly ButtonWidget m_joinLan;
        private readonly ButtonWidget m_connect;
        private readonly ButtonWidget m_back;
        private IReadOnlyList<LanRoomInfo> m_previousRooms = Array.Empty<LanRoomInfo>();
        private double m_searchStarted;
        private double m_nextRefresh;

        public MultiplayerScreen() {
            LoadContents(this, ContentManager.Get<XElement>("Screens/MultiplayerScreen"));
            m_lanRooms = Children.Find<ListPanelWidget>("LanRooms");
            m_lanEmpty = Children.Find<LabelWidget>("LanEmpty");
            m_joinLan = Children.Find<ButtonWidget>("JoinLanRoom");
            m_connect = Children.Find<ButtonWidget>("DirectConnect");
            m_back = Children.Find<ButtonWidget>("TopBar.Back");
            m_lanRooms.ItemWidgetFactory = item => CreateRoomRow((LanRoomInfo)item);
        }

        public override void Enter(object[] parameters) {
            m_searchStarted = Time.RealTime;
            m_nextRefresh = 0;
            m_lanRooms.ClearItems();
            m_previousRooms = Array.Empty<LanRoomInfo>();
            NetworkManager.StartLanDiscoveryAction?.Invoke();
        }

        public override void Leave() {
            NetworkManager.StopLanDiscoveryAction?.Invoke();
        }

        public override void Update() {
            if (DialogsManager.HasDialogs(this)) {
                return;
            }
            if (Input.Back || Input.Cancel || m_back.IsClicked) {
                ScreensManager.SwitchScreen("MainMenu");
                return;
            }
            if (Time.RealTime >= m_nextRefresh) {
                m_nextRefresh = Time.RealTime + 0.35;
                RefreshRooms();
            }
            m_joinLan.IsEnabled = m_lanRooms.SelectedItem is LanRoomInfo;
            if (m_connect.IsClicked) {
                DialogsManager.ShowDialog(this, new ConnectServerDialog(string.Empty));
            }
            else if (m_joinLan.IsEnabled && (m_joinLan.IsClicked || Input.Ok) && m_lanRooms.SelectedItem is LanRoomInfo room) {
                ScreensManager.SwitchScreen("GameLoading", null, null, room.EndPoint, null);
            }
        }

        private void RefreshRooms() {
            IReadOnlyList<LanRoomInfo> rooms = NetworkManager.LanRooms;
            m_lanEmpty.IsVisible = rooms.Count == 0;
            m_lanEmpty.Text = LanguageControl.GetContentWidgets("LanMultiplayer", Time.RealTime - m_searchStarted < 3 ? "Searching" : "NoRooms");
            if (rooms.SequenceEqual(m_previousRooms)) {
                return;
            }
            Guid selectedId = (m_lanRooms.SelectedItem as LanRoomInfo)?.Id ?? Guid.Empty;
            m_lanRooms.ClearItems();
            m_lanRooms.AddItems(rooms);
            m_lanRooms.SelectedItem = rooms.FirstOrDefault(room => room.Id == selectedId);
            m_previousRooms = rooms;
        }

        private static Widget CreateRoomRow(LanRoomInfo room) {
            StackPanelWidget row = new() {
                Direction = LayoutDirection.Horizontal,
                Margin = new Vector2(12, 0),
                VerticalAlignment = WidgetAlignment.Center
            };
            StackPanelWidget names = new() {
                Direction = LayoutDirection.Vertical,
                VerticalAlignment = WidgetAlignment.Center
            };
            names.Children.Add(new LabelWidget { Text = room.WorldName, Size = new Vector2(480, 26) });
            names.Children.Add(new LabelWidget { Text = room.HostName, Size = new Vector2(480, 22), FontScale = 0.75f, Color = new Color(185, 185, 185) });
            row.Children.Add(names);
            row.Children.Add(new CanvasWidget { Size = new Vector2(float.PositiveInfinity, 0) });
            row.Children.Add(new LabelWidget {
                Text = $"{room.PlayerCount}/{room.Capacity}",
                VerticalAlignment = WidgetAlignment.Center
            });
            return row;
        }

    }
}
