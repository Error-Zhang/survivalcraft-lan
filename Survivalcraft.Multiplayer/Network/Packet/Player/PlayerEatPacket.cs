using Engine;

namespace Game.Network;

public sealed class PlayerEatPacket : PlayerPacketBase {
    public override byte ID => (byte)PacketType.PlayerEat;
    public override NetworkState MinNeedState => NetworkState.Playing;
    private int m_slot;
    private int m_food;
    private Vector3 m_direction;

    public PlayerEatPacket() { }

    public PlayerEatPacket(ComponentPlayer player, int slot, int food, Vector3 direction) {
        m_fromPlayerIndex = player.PlayerData.PlayerIndex;
        m_slot = slot;
        m_food = food;
        m_direction = direction;
    }

    public override void Serialize(PacketSerializer archive) {
        SerializePlayerId(archive, archive.IsWriting ? ResolvePlayer(m_fromPlayerIndex) : null);
        archive.Value(ref m_slot);
        archive.Value(ref m_food);
        archive.Value(ref m_direction);
    }

    public override void Handle(bool isServer) {
        if (!TryResolvePlayer(isServer, out PlayerData data) || data.ComponentPlayer is not ComponentPlayer player) {
            return;
        }
        if (isServer) {
            if (!player.TryEatNetworkFood(m_slot, m_food, m_direction)) {
                return;
            }
            InventoryPacketHelpers.QueueInventorySync(player.ComponentMiner.Inventory);
            QueuePlayerPacket(this, true);
        }
        else if (!data.IsMainPlayer) {
            player.ShowNetworkEating(m_food);
        }
    }
}
