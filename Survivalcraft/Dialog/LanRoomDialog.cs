using System.Xml.Linq;
using Engine;

namespace Game;

public sealed class LanRoomDialog : Dialog {
    private readonly LabelWidget m_status;
    private readonly LabelWidget m_publicStatus;
    private readonly ButtonWidget m_toggle;
    private readonly ButtonWidget m_copy;
    private readonly ButtonWidget m_publicCode;
    private readonly ButtonWidget m_close;
    private string m_address;

    public LanRoomDialog() {
        LoadContents(this, ContentManager.Get<XElement>("Dialogs/LanRoomDialog"));
        m_status = Children.Find<LabelWidget>("RoomStatus");
        m_publicStatus = Children.Find<LabelWidget>("PublicStatus");
        m_toggle = Children.Find<ButtonWidget>("ToggleLan");
        m_copy = Children.Find<ButtonWidget>("CopyAddress");
        m_publicCode = Children.Find<ButtonWidget>("ShareCode");
        m_close = Children.Find<ButtonWidget>("Close");
        m_address = NetworkManager.LanAddress;
    }

    public override void Update() {
        if (Input.Back || Input.Cancel || m_close.IsClicked) {
            DialogsManager.HideDialog(this);
            return;
        }
        if (m_toggle.IsClicked) {
            if (NetworkManager.IsServerRunning) {
                NetworkManager.Stop();
                GameManager.SaveProject(false, true);
                m_address = null;
            }
            else if (NetworkManager.TryStartLanHost(out string error)) {
                m_address = NetworkManager.LanAddress;
            }
            else {
                DialogsManager.Alert(Text("Room"), error);
            }
        }
        if (m_copy.IsClicked && m_address != null) {
            ClipboardManager.ClipboardString = m_address;
        }

        bool open = NetworkManager.IsServerRunning;
        m_toggle.Text = Text(open ? "CloseRoom" : "OpenToLan");
        m_copy.IsEnabled = open && m_address != null;
        m_status.Text = open
            ? string.Format(Text("RoomOpen"), m_address, NetworkManager.ServerSessions.Count(s => s.NetworkState == NetworkState.Playing))
            : Text("RoomClosed");
        m_publicCode.IsEnabled = open && NetworkManager.RelayHostCodeStatus is RelayHostCodeStatus.Inactive or RelayHostCodeStatus.Failed;
        m_publicStatus.Text = NetworkManager.RelayHostCodeStatus switch {
            RelayHostCodeStatus.Creating => Text("CreatingCode"),
            RelayHostCodeStatus.Ready => string.Format(Text("PublicCode"), NetworkManager.RelayGlobalCode),
            RelayHostCodeStatus.Failed => Text("CodeFailed"),
            _ => string.Empty
        };
        if (m_publicCode.IsClicked && m_publicCode.IsEnabled) {
            _ = NetworkManager.StartRelayHostAsync();
        }
    }

    private static string Text(string key) => LanguageControl.GetContentWidgets("LanMultiplayer", key);
}
