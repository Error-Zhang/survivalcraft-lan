using Engine;
using Game.Network;
using GameEntitySystem;

namespace Game;

public enum SiyahAkrepCompatNetOperation : ushort {
    ProjectileRequest = 1,
    MissileRequest = 2,
    ProjectileBroadcast = 3,
    StateSnapshot = 4,
    InventorySnapshot = 5
}

public sealed class SiyahAkrepProjectilePayload {
    public bool CustomProjectile;
    public int Value;
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 AngularVelocity;
    public Vector3 TrailOffset;
    public int OwnerEntityId;

    public void Serialize(PacketSerializer archive) {
        archive.Value(ref CustomProjectile);
        archive.Value(ref Value);
        archive.Value(ref Position);
        archive.Value(ref Velocity);
        archive.Value(ref AngularVelocity);
        archive.Value(ref TrailOffset);
        archive.Value(ref OwnerEntityId);
    }
}

public sealed class SiyahAkrepMissilePayload {
    public Vector3 Position;
    public Quaternion Rotation;
    public Project Project;
    public float RemainTime;
    public float DirectlyTime;
    public int GuideLevel;
    public bool AntiVehicle;
    public int TargetEntityId;
    public Vector3 Velocity;
    public bool HasStartEngineTime;
    public float StartEngineTime;

    public void Serialize(PacketSerializer archive) {
        archive.Value(ref Position);
        archive.Value(ref Rotation);
        archive.Value(ref RemainTime);
        archive.Value(ref DirectlyTime);
        archive.Value(ref GuideLevel);
        archive.Value(ref AntiVehicle);
        archive.Value(ref TargetEntityId);
        archive.Value(ref Velocity);
        archive.Value(ref HasStartEngineTime);
        if (HasStartEngineTime) {
            archive.Value(ref StartEngineTime);
        }
    }
}

public enum SiyahAkrepStateValueKind : byte {
    Boolean,
    Byte,
    Integer,
    Single,
    Double,
    Point3,
    Vector3,
    Quaternion
}

public sealed class SiyahAkrepStateValue {
    public string Name = string.Empty;
    public SiyahAkrepStateValueKind Kind;
    public bool BooleanValue;
    public byte ByteValue;
    public int IntegerValue;
    public float SingleValue;
    public double DoubleValue;
    public Point3 Point3Value;
    public Vector3 Vector3Value;
    public Quaternion QuaternionValue;

    public void Serialize(PacketSerializer archive) {
        archive.Value(ref Name);
        archive.Enum(ref Kind);
        switch (Kind) {
            case SiyahAkrepStateValueKind.Boolean:
                archive.Value(ref BooleanValue);
                break;
            case SiyahAkrepStateValueKind.Byte:
                archive.Value(ref ByteValue);
                break;
            case SiyahAkrepStateValueKind.Integer:
                archive.Value(ref IntegerValue);
                break;
            case SiyahAkrepStateValueKind.Single:
                archive.Value(ref SingleValue);
                break;
            case SiyahAkrepStateValueKind.Double:
                archive.Value(ref DoubleValue);
                break;
            case SiyahAkrepStateValueKind.Point3:
                archive.Value(ref Point3Value);
                break;
            case SiyahAkrepStateValueKind.Vector3:
                archive.Value(ref Vector3Value);
                break;
            case SiyahAkrepStateValueKind.Quaternion:
                archive.Value(ref QuaternionValue);
                break;
            default:
                throw new InvalidDataException($"Unknown SiyahAkrep state value kind: {Kind}");
        }
    }
}

public sealed class SiyahAkrepStateRecord {
    public int EntityId;
    public bool IsSubsystem;
    public string ComponentTypeName = string.Empty;
    public List<SiyahAkrepStateValue> Values = [];

    public void Serialize(PacketSerializer archive) {
        archive.Value(ref EntityId);
        archive.Value(ref IsSubsystem);
        archive.Value(ref ComponentTypeName);
        int count = archive.IsWriting ? Values.Count : 0;
        archive.Value(ref count);
        if (count < 0 || count > SiyahAkrepCompatNetPayloads.MaximumValuesPerRecord) {
            throw new InvalidDataException("Invalid SiyahAkrep state value count.");
        }

        if (archive.IsReading) {
            Values.Clear();
            for (int i = 0; i < count; i++) {
                SiyahAkrepStateValue value = new();
                value.Serialize(archive);
                Values.Add(value);
            }
        }
        else {
            foreach (SiyahAkrepStateValue value in Values) {
                value.Serialize(archive);
            }
        }
    }
}

public sealed class SiyahAkrepStateBatch {
    public int Sequence;
    public List<SiyahAkrepStateRecord> Records = [];

    public void Serialize(PacketSerializer archive) {
        archive.Value(ref Sequence);
        int count = archive.IsWriting ? Records.Count : 0;
        archive.Value(ref count);
        if (count < 0 || count > SiyahAkrepCompatNetPayloads.MaximumRecordsPerBatch) {
            throw new InvalidDataException("Invalid SiyahAkrep state record count.");
        }

        if (archive.IsReading) {
            Records.Clear();
            for (int i = 0; i < count; i++) {
                SiyahAkrepStateRecord record = new();
                record.Serialize(archive);
                Records.Add(record);
            }
        }
        else {
            foreach (SiyahAkrepStateRecord record in Records) {
                record.Serialize(archive);
            }
        }
    }
}

public sealed class SiyahAkrepInventorySlot {
    public int Value;
    public int Count;

    public void Serialize(PacketSerializer archive) {
        archive.Value(ref Value);
        archive.Value(ref Count);
        if (Value < 0 || Count < 0) {
            throw new InvalidDataException("Invalid SiyahAkrep inventory slot.");
        }
    }
}

public sealed class SiyahAkrepInventoryRecord {
    public int EntityId;
    public bool IsSubsystem;
    public string OwnerTypeName = string.Empty;
    public int ActiveSlotIndex = -1;
    public List<SiyahAkrepInventorySlot> Slots = [];

    public void Serialize(PacketSerializer archive) {
        archive.Value(ref EntityId);
        archive.Value(ref IsSubsystem);
        archive.Value(ref OwnerTypeName);
        archive.Value(ref ActiveSlotIndex);
        int count = archive.IsWriting ? Slots.Count : 0;
        archive.Value(ref count);
        if (count < 0 || count > SiyahAkrepCompatNetPayloads.MaximumSlotsPerInventory) {
            throw new InvalidDataException("Invalid SiyahAkrep inventory slot count.");
        }

        if (archive.IsReading) {
            Slots.Clear();
            for (int i = 0; i < count; i++) {
                SiyahAkrepInventorySlot slot = new();
                slot.Serialize(archive);
                Slots.Add(slot);
            }
        }
        else {
            foreach (SiyahAkrepInventorySlot slot in Slots) {
                slot.Serialize(archive);
            }
        }
    }
}

public sealed class SiyahAkrepInventoryBatch {
    public int Sequence;
    public bool FullRefresh;
    public List<SiyahAkrepInventoryRecord> Records = [];

    public void Serialize(PacketSerializer archive) {
        archive.Value(ref Sequence);
        archive.Value(ref FullRefresh);
        int count = archive.IsWriting ? Records.Count : 0;
        archive.Value(ref count);
        if (count < 0 || count > SiyahAkrepCompatNetPayloads.MaximumInventoryRecordsPerBatch) {
            throw new InvalidDataException("Invalid SiyahAkrep inventory record count.");
        }

        if (archive.IsReading) {
            Records.Clear();
            for (int i = 0; i < count; i++) {
                SiyahAkrepInventoryRecord record = new();
                record.Serialize(archive);
                Records.Add(record);
            }
        }
        else {
            foreach (SiyahAkrepInventoryRecord record in Records) {
                record.Serialize(archive);
            }
        }
    }
}

public static class SiyahAkrepCompatNetPayloads {
    public const int MaximumValuesPerRecord = 24;
    public const int MaximumRecordsPerBatch = 96;
    public const int MaximumInventoryRecordsPerBatch = 24;
    public const int MaximumSlotsPerInventory = 256;

    public static byte[] Write(SiyahAkrepProjectilePayload payload) =>
        CompatNetPayload.Write(payload.Serialize);

    public static SiyahAkrepProjectilePayload ReadProjectile(byte[] payload) {
        SiyahAkrepProjectilePayload result = new();
        CompatNetPayload.Read(payload, result.Serialize);
        return result;
    }

    public static byte[] Write(SiyahAkrepMissilePayload payload) =>
        CompatNetPayload.Write(payload.Serialize);

    public static SiyahAkrepMissilePayload ReadMissile(byte[] payload) {
        SiyahAkrepMissilePayload result = new();
        CompatNetPayload.Read(payload, result.Serialize);
        return result;
    }

    public static byte[] Write(SiyahAkrepStateBatch batch) =>
        CompatNetPayload.Write(batch.Serialize);

    public static SiyahAkrepStateBatch ReadState(byte[] payload) {
        SiyahAkrepStateBatch result = new();
        CompatNetPayload.Read(payload, result.Serialize);
        return result;
    }

    public static byte[] Write(SiyahAkrepInventoryBatch batch) =>
        CompatNetPayload.Write(batch.Serialize);

    public static SiyahAkrepInventoryBatch ReadInventory(byte[] payload) {
        SiyahAkrepInventoryBatch result = new();
        CompatNetPayload.Read(payload, result.Serialize);
        return result;
    }
}
