using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using Engine;

namespace Game.Network;

internal sealed class LanDiscovery {
    private const int DiscoveryPort = 28886;
    private const int MaxMessageSize = 1024;
    private const int MaxRooms = 64;
    private const double ProbeInterval = 1.5;
    private const double RoomTimeout = 5;
    private static readonly byte[] s_probe = "SCAPI-LAN-1.9.5.1?"u8.ToArray();

    private readonly Dictionary<Guid, (LanRoomInfo Room, double LastSeen)> m_rooms = new();
    private UdpClient m_host;
    private UdpClient m_browser;
    private Guid m_roomId;
    private double m_nextProbe;

    public IReadOnlyList<LanRoomInfo> Rooms => m_rooms.Values.Select(entry => entry.Room)
        .OrderBy(room => room.WorldName, StringComparer.CurrentCulture)
        .ThenBy(room => room.Id)
        .ToArray();

    public void StartHosting() {
        StopHosting();
        try {
            m_host = new UdpClient(new IPEndPoint(IPAddress.Any, DiscoveryPort));
            m_roomId = Guid.NewGuid();
        }
        catch (SocketException ex) {
            Log.Warning($"LAN discovery listener could not start: {ex.Message}");
        }
    }

    public void StopHosting() {
        m_host?.Dispose();
        m_host = null;
    }

    public void StartBrowsing() {
        StopBrowsing();
        try {
            m_browser = new UdpClient(new IPEndPoint(IPAddress.Any, 0)) { EnableBroadcast = true };
            m_nextProbe = 0;
        }
        catch (SocketException ex) {
            Log.Warning($"LAN discovery could not start: {ex.Message}");
        }
    }

    public void StopBrowsing() {
        m_browser?.Dispose();
        m_browser = null;
        m_rooms.Clear();
    }

    public void Update() {
        if (m_host != null && NetworkManager.NetworkServer is NetworkServer { IsRunning: true } server) {
            ReplyToProbes(server);
        }
        if (m_browser != null) {
            UpdateBrowser();
        }
    }

    private void ReplyToProbes(NetworkServer server) {
        SubsystemGameInfo gameInfo = GameManager.Project?.FindSubsystem<SubsystemGameInfo>();
        SubsystemPlayers players = GameManager.Project?.FindSubsystem<SubsystemPlayers>();
        PlayerData host = players?.PlayersData.FirstOrDefault(player => player.IsMainPlayer && player.ComponentPlayer != null);
        if (gameInfo == null || host == null) {
            return;
        }

        byte[] response = null;
        try {
            for (int i = 0; i < 16 && m_host.Available > 0; i++) {
                IPEndPoint sender = new(IPAddress.Any, 0);
                byte[] request = m_host.Receive(ref sender);
                if (request.AsSpan().SequenceEqual(s_probe)) {
                    response ??= JsonSerializer.SerializeToUtf8Bytes(new Advertisement(
                        m_roomId, server.Port, Limit(gameInfo.WorldSettings.Name), Limit(host.Name),
                        1 + NetworkManager.ServerSessions.Count(session => session.NetworkState == NetworkState.Playing),
                        gameInfo.WorldSettings.MaxOnlinePlayerCount));
                    m_host.Send(response, sender);
                }
            }
        }
        catch (SocketException ex) {
            Log.Warning($"LAN discovery reply failed: {ex.Message}");
        }
    }

    private void UpdateBrowser() {
        double now = Time.RealTime;
        if (now >= m_nextProbe) {
            m_nextProbe = now + ProbeInterval;
            SendProbe(IPAddress.Loopback);
            SendProbe(IPAddress.Broadcast);
        }
        try {
            for (int i = 0; i < 16 && m_browser.Available > 0; i++) {
                IPEndPoint sender = new(IPAddress.Any, 0);
                byte[] response = m_browser.Receive(ref sender);
                if (sender.Port != DiscoveryPort || response.Length > MaxMessageSize) {
                    continue;
                }
                AddRoom(response, sender.Address, now);
            }
        }
        catch (SocketException) {
            // A host can close between probes.
        }
        foreach (Guid id in m_rooms.Where(entry => now - entry.Value.LastSeen > RoomTimeout).Select(entry => entry.Key).ToArray()) {
            m_rooms.Remove(id);
        }
    }

    private void SendProbe(IPAddress address) {
        try {
            m_browser.Send(s_probe, new IPEndPoint(address, DiscoveryPort));
        }
        catch (SocketException) {
            // Some interfaces do not support broadcast. Direct connect remains available.
        }
    }

    private void AddRoom(byte[] response, IPAddress address, double now) {
        Advertisement info;
        try {
            info = JsonSerializer.Deserialize<Advertisement>(response);
        }
        catch (JsonException) {
            return;
        }
        if (info == null || info.Id == Guid.Empty || info.Port is < 1 or > 65535
            || info.WorldName == null || info.HostName == null || info.PlayerCount < 0 || info.Capacity < 0
            || m_rooms.Count >= MaxRooms && !m_rooms.ContainsKey(info.Id)) {
            return;
        }
        IPEndPoint endPoint = new(address, info.Port);
        if (m_rooms.TryGetValue(info.Id, out var previous)
            && IPAddress.IsLoopback(previous.Room.EndPoint.Address) && !IPAddress.IsLoopback(address)
            && now - previous.LastSeen < RoomTimeout) {
            endPoint = previous.Room.EndPoint;
        }
        m_rooms[info.Id] = (new LanRoomInfo(info.Id, endPoint, Limit(info.WorldName), Limit(info.HostName), info.PlayerCount, info.Capacity), now);
    }

    private static string Limit(string text) {
        text = string.Concat((text ?? string.Empty).Where(c => !char.IsControl(c)));
        return text.Length > 48 ? text[..48] : text;
    }

    public static string GetLocalAddress(int port) {
        IPAddress first = null;
        try {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces()) {
                if (adapter.OperationalStatus != OperationalStatus.Up || adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback) {
                    continue;
                }
                IPInterfaceProperties properties = adapter.GetIPProperties();
                foreach (UnicastIPAddressInformation entry in properties.UnicastAddresses) {
                    IPAddress address = entry.Address;
                    if (address.AddressFamily != AddressFamily.InterNetwork || address.GetAddressBytes() is [169, 254, ..]) {
                        continue;
                    }
                    first ??= address;
                    if (properties.GatewayAddresses.Any(gateway => gateway.Address.AddressFamily == AddressFamily.InterNetwork)) {
                        return new IPEndPoint(address, port).ToString();
                    }
                }
            }
        }
        catch (NetworkInformationException ex) {
            Log.Warning($"Could not enumerate local addresses: {ex.Message}");
        }
        return new IPEndPoint(first ?? IPAddress.Loopback, port).ToString();
    }

    private sealed record Advertisement(Guid Id, int Port, string WorldName, string HostName, int PlayerCount, int Capacity);
}
