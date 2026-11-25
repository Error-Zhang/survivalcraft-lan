using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine;


namespace Game {
    public class ConnectServerDialog : Dialog {
        private TextBoxWidget m_ipTextBoxWidget;

        private ButtonWidget m_connectButtonWidget;

        private ButtonWidget m_cancelButtonWidget;
        private bool m_isResolvingAddress;

        public bool AutoHide {
            get;
            set;
        }

        public ConnectServerDialog() : this("127.0.0.1:28887") { }

        public ConnectServerDialog(string initialAddress) {
            XElement node = ContentManager.Get<XElement>("Dialogs/ConnectServerDialog");
            LoadContents(this, node);
            m_ipTextBoxWidget = Children.Find<TextBoxWidget>("ConnectServerDialog.IpTextBox");
            m_connectButtonWidget = Children.Find<ButtonWidget>("ConnectServerDialog.ConnectButton");
            m_cancelButtonWidget = Children.Find<ButtonWidget>("ConnectServerDialog.CancelButton");

            m_ipTextBoxWidget.Text = initialAddress;
            m_ipTextBoxWidget.HasFocus = true;
            m_ipTextBoxWidget.Enter += delegate {
                ConnectToServer();
            };

            AutoHide = true;
        }

        public override void Update() {
            if (Input.Cancel && !m_isResolvingAddress) {
                Dismiss();
            }
            else if (Input.Ok) {
                ConnectToServer();
            }
            else if (m_connectButtonWidget.IsClicked) {
                ConnectToServer();
            }
            else if (m_cancelButtonWidget.IsClicked && !m_isResolvingAddress) {
                Dismiss();
            }
        }

        public async void ConnectToServer() {
            if (m_isResolvingAddress) {
                return;
            }
            m_isResolvingAddress = true;
            m_connectButtonWidget.IsEnabled = false;
            try {
                object target = await ResolveAddressAsync(m_ipTextBoxWidget.Text);
                Dispatcher.Dispatch(() => {
                    m_isResolvingAddress = false;
                    m_connectButtonWidget.IsEnabled = true;
                    Dismiss();
                    ScreensManager.SwitchScreen("GameLoading", null, null, target, null);
                });
            }
            catch (Exception ex) {
                Dispatcher.Dispatch(() => {
                    m_isResolvingAddress = false;
                    m_connectButtonWidget.IsEnabled = true;
                    DialogsManager.Alert(LanguageControl.Error, ex.Message, ParentWidget);
                });
            }
        }

        internal static async Task<object> ResolveAddressAsync(string address) {
            string text = address?.Trim();
            if (string.IsNullOrEmpty(text)) {
                throw new FormatException(Text("AddressRequired"));
            }
            if (text.StartsWith("relay@", StringComparison.OrdinalIgnoreCase)) {
                string[] relay = text[6..].Split('#', 2);
                if (relay.Length != 2 || string.IsNullOrWhiteSpace(relay[1])
                    || !TryParseEndPointParts(relay[0], 28888, out EndPointParts relayParts)) {
                    throw new FormatException(Text("RelayFormat"));
                }
                return new RelayJoinInfo(await ResolveEndpointAsync(relayParts), relay[1].Trim());
            }
            if (LooksLikeGlobalCode(text)) {
                return await NetworkManager.ResolveRelayJoinInfoAsync(text)
                    ?? throw new InvalidOperationException(Text("CodeFailed"));
            }
            if (!TryParseEndPointParts(text, 28887, out EndPointParts parts)) {
                throw new FormatException(Text("AddressFormat"));
            }
            return await ResolveEndpointAsync(parts);
        }

        private static async Task<IPEndPoint> ResolveEndpointAsync(EndPointParts parts) {
            IPAddress address = await NetworkManager.TryGetIPAddress(parts.Host);
            if (address == null) {
                throw new InvalidOperationException(Text("AddressMissing"));
            }
            return new IPEndPoint(address, parts.Port);
        }

        private static string Text(string key) => LanguageControl.GetContentWidgets("LanMultiplayer", key);

        private static bool TryParseEndPointParts(string text, int defaultPort, out EndPointParts endPointParts) {
            endPointParts = default;
            text = text?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return false;

            string host;
            int port = defaultPort;

            if (text[0] == '[') {
                int closeBracketIndex = text.IndexOf(']');
                if (closeBracketIndex <= 1) return false;
                host = text[1..closeBracketIndex];
                string suffix = text[(closeBracketIndex + 1)..];
                if (suffix.Length > 0) {
                    if (suffix[0] != ':' || !TryParsePort(suffix[1..], out port)) return false;
                }
            }
            else {
                int firstColonIndex = text.IndexOf(':');
                int lastColonIndex = text.LastIndexOf(':');
                if (firstColonIndex >= 0 && firstColonIndex == lastColonIndex) {
                    host = text[..firstColonIndex];
                    if (!TryParsePort(text[(firstColonIndex + 1)..], out port)) return false;
                }
                else {
                    host = text;
                }
            }

            if (string.IsNullOrWhiteSpace(host)) return false;
            endPointParts = new EndPointParts(host.Trim(), port);
            return true;
        }

        private static bool TryParsePort(string text, out int port) {
            return int.TryParse(text, out port) && port > 0 && port <= 65535;
        }

        private static bool LooksLikeGlobalCode(string text) {
            if (string.IsNullOrWhiteSpace(text) || text.Contains(':') || text.Contains('@') || text.Contains('/') || text.Contains('.')) return false;
            int count = 0;
            foreach (char c in text) {
                if (c is '-' or ' ') continue;
                if (!char.IsDigit(c)) return false;
                count++;
            }
            return count == 4;
        }

        private readonly struct EndPointParts {
            public EndPointParts(string host, int port) {
                Host = host;
                Port = port;
            }

            public string Host { get; }

            public int Port { get; }
        }

        public void Dismiss() {
            if (AutoHide) {
                DialogsManager.HideDialog(this);
            }
        }
    }
}
