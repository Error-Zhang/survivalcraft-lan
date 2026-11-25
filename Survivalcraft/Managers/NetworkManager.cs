using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Engine;

namespace Game;

public enum NetworkState {
    NotConnected, Connected, ProjectLoaded, LoadTerrain, Playing
}

public enum RelayHostCodeStatus {
    Inactive, Creating, Ready, Failed
}

public class RelayJoinInfo(IPEndPoint endPoint, string roomCode) {
    public IPEndPoint EndPoint { get; } = endPoint;
    public string RoomCode { get; } = roomCode;
}

public class ClientSession {
    public int PlayerIndex { get; set; }
    public Guid NetworkGuid { get; set; }
    public NetworkState NetworkState { get; set; }
}

public sealed record LanRoomInfo(Guid Id, IPEndPoint EndPoint, string WorldName, string HostName, int PlayerCount, int Capacity);

public static class NetworkManager {
    public static bool CanHostLan => StartServerAction != null;
    public static Action StartLanDiscoveryAction { get; set; }
    public static Action StopLanDiscoveryAction { get; set; }
    public static Func<IReadOnlyList<LanRoomInfo>> LanRoomsFunc { get; set; }
    public static Func<string> LanAddressFunc { get; set; }
    public static string LanAddress => LanAddressFunc?.Invoke();
    public static IReadOnlyList<LanRoomInfo> LanRooms => LanRoomsFunc?.Invoke() ?? Array.Empty<LanRoomInfo>();
    public static object NetworkClient { get; set; }
    public static object NetworkServer { get; set; }
    public static object RelayClient { get; set; }
    public static object RelayHost { get; set; }
    public static RelayHostCodeStatus RelayHostCodeStatus { get; set; } = RelayHostCodeStatus.Inactive;

    public static Func<bool> IsServerRunningFunc { get; set; }
    public static Func<bool> IsClientRunningFunc { get; set; }
    public static Func<int> ClientLatencyFunc { get; set; }
    public static Func<ClientSession> MySessionFunc { get; set; }
    public static Func<IEnumerable<ClientSession>> ServerSessionsFunc { get; set; }
    public static Func<string> RelayGlobalCodeFunc { get; set; }
    public static Func<string> RelayRoomCodeFunc { get; set; }
    public static Func<string, Task<RelayJoinInfo>> ResolveRelayJoinInfoAsyncFunc { get; set; }
    public static Func<TerrainSerializer23> CreateTerrainSerializerNetFunc { get; set; }

    public static Action InitializeAction { get; set; }
    public static Action UpdateAction { get; set; }
    public static Action StopAction { get; set; }
    public static Action StartServerAction { get; set; }
    public static Action<IPEndPoint> ConnectServerAction { get; set; }
    public static Func<Task> StartRelayHostAsyncFunc { get; set; }
    public static Action<IPEndPoint> StartRelayHostAction { get; set; }
    public static Action<IPEndPoint, string> ConnectRelayAction { get; set; }
    public static Action<object> QueueAction { get; set; }
    public static Action<Pickable> QueuePickableCreateAction { get; set; }
    public static Action<List<Pickable>> QueuePickableUpdateAction { get; set; }
    public static Action<ComponentPlayer, Guid> SavePlayerEntityAction { get; set; }

    public static bool IsServerRunning => IsServerRunningFunc != null ? IsServerRunningFunc() : false;
    public static bool IsClientRunning => IsClientRunningFunc != null ? IsClientRunningFunc() : false;
    public static int ClientLatency => ClientLatencyFunc != null ? ClientLatencyFunc() : -1;
    public static ClientSession MySession => MySessionFunc != null ? MySessionFunc() : null;
    public static int PlayerIndex => MySession?.PlayerIndex ?? -1;
    public static Guid NetworkGuid => MySession?.NetworkGuid ?? Guid.Empty;
    public static NetworkState MyState => MySession?.NetworkState ?? NetworkState.NotConnected;
    public static string RelayGlobalCode => RelayGlobalCodeFunc?.Invoke();
    public static string RelayRoomCode => RelayRoomCodeFunc?.Invoke();
    public static void SetPlayerIndex(int index) { if (MySession != null) MySession.PlayerIndex = index; }
    public static void SetState(NetworkState state) { if (MySession != null) MySession.NetworkState = state; }
    public static IEnumerable<ClientSession> ServerSessions => ServerSessionsFunc != null ? ServerSessionsFunc() : Array.Empty<ClientSession>();

    /// <summary>
    /// Determines whether an ordinary creature is simulated exclusively by the
    /// server while this process is running as a client. Player entities are
    /// intentionally excluded because their input/presentation path is handled
    /// separately, as are mounts controlled by the local player.
    /// </summary>
    public static bool IsServerAuthoritativeCreatureBody(ComponentBody body) {
        if (!IsClientRunning
            || body?.Entity == null
            || body.Entity.FindComponent<ComponentCreature>() == null
            || body.Entity.FindComponent<ComponentPlayer>() != null) {
            return false;
        }

        bool hasChildPlayer = false;
        bool hasNonMainChildPlayer = false;
        for (int i = 0; i < body.ChildBodies.Count; i++) {
            ComponentPlayer childPlayer = body.ChildBodies[i].Entity.FindComponent<ComponentPlayer>();
            if (childPlayer == null) {
                continue;
            }
            hasChildPlayer = true;
            if (childPlayer.PlayerData?.IsMainPlayer == false) {
                hasNonMainChildPlayer = true;
            }
        }

        return !hasChildPlayer || hasNonMainChildPlayer;
    }

    public static void Initialize() {
        InitializeAction?.Invoke();
    }

    public static void Update() {
        UpdateAction?.Invoke();
    }

    public static void Stop() {
        StopAction?.Invoke();
        RelayHostCodeStatus = RelayHostCodeStatus.Inactive;
    }

    public static void StartServer() {
        StartServerAction?.Invoke();
    }

    public static bool TryStartLanHost(out string error) {
        error = null;
        if (!CanHostLan) {
            error = LanguageControl.GetContentWidgets("LanMultiplayer", "Unavailable");
            return false;
        }
        try {
            StartServer();
            return true;
        }
        catch (Exception ex) {
            Log.Warning(ex);
            error = string.Format(LanguageControl.GetContentWidgets("LanMultiplayer", "OpenFailed"), ex.Message);
            return false;
        }
    }

    public static void ConnectServer(IPEndPoint iPEndPoint) {
        ConnectServerAction?.Invoke(iPEndPoint);
    }

    public static async Task StartRelayHostAsync() {
        if (StartRelayHostAsyncFunc != null) {
            await StartRelayHostAsyncFunc();
        }
    }

    public static void StartRelayHost(IPEndPoint relayEndPoint) {
        StartRelayHostAction?.Invoke(relayEndPoint);
    }

    public static void ConnectRelay(IPEndPoint relayEndPoint, string roomCode) {
        ConnectRelayAction?.Invoke(relayEndPoint, roomCode);
    }

    public static void Queue(object packet) {
        QueueAction?.Invoke(packet);
    }

    public static void QueuePickableCreate(Pickable pickable) {
        QueuePickableCreateAction?.Invoke(pickable);
    }

    public static void QueuePickableUpdate(List<Pickable> pickables) {
        QueuePickableUpdateAction?.Invoke(pickables);
    }

    public static void SavePlayerEntity(ComponentPlayer player, Guid networkGuid) {
        SavePlayerEntityAction?.Invoke(player, networkGuid);
    }

    public static TerrainSerializer23 CreateTerrainSerializerNet() {
        return CreateTerrainSerializerNetFunc?.Invoke();
    }

    public static async Task<RelayJoinInfo> ResolveRelayJoinInfoAsync(string globalCode) {
        if (ResolveRelayJoinInfoAsyncFunc != null) {
            return await ResolveRelayJoinInfoAsyncFunc(globalCode);
        }
        return null;
    }

    public static async Task<IPAddress> TryGetIPAddress(string host) {
        if (!IPAddress.TryParse(host, out IPAddress address)) {
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);
            address = addresses.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?? addresses.FirstOrDefault();
        }
        return address;
    }
}
