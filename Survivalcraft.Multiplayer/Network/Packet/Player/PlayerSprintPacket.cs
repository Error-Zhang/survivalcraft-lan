namespace Game.Network;

public sealed class PlayerSprintPacket : PlayerPacketBase {
    public override byte ID => (byte)PacketType.PlayerSprint;
    public override NetworkState MinNeedState => NetworkState.Playing;

    private bool m_sprinting;

    public PlayerSprintPacket() { }

    public PlayerSprintPacket(ComponentPlayer player, bool sprinting) {
        m_fromPlayerIndex = player.PlayerData.PlayerIndex;
        m_sprinting = sprinting;
    }

    public override void Serialize(PacketSerializer archive) {
        SerializePlayerId(archive, archive.IsWriting ? ResolvePlayer(m_fromPlayerIndex) : null);
        archive.Value(ref m_sprinting);
    }

    public override void Handle(bool isServer) {
        if (isServer && TryResolvePlayer(true, out PlayerData playerData)) {
            playerData.ComponentPlayer?.SetNetworkSprint(m_sprinting);
        }
    }
}
