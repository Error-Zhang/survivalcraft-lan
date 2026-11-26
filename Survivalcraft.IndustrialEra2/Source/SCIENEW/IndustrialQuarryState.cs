using Engine;
using Game.Network;

namespace SCIENEW;

internal interface IIndustrialQuarry {
    bool IsPreparing { get; set; }
    void SelectArea();
    void SetDisposalMode(bool dispose);
    void SerializeLanState(PacketSerializer archive);
}

internal static class IndustrialQuarryState {
    public static void Serialize(
        PacketSerializer archive,
        ref SubsystemBeacons.RectanglePoints? area,
        ref bool preparing,
        ref int mobileId) {
        bool hasArea = area.HasValue;
        archive.Value(ref hasArea);
        archive.Value(ref preparing);
        archive.Value(ref mobileId);
        if (hasArea) {
            SubsystemBeacons.RectanglePoints bounds = area.GetValueOrDefault();
            archive.Value(ref bounds.Point1);
            archive.Value(ref bounds.Point2);
            archive.Value(ref bounds.Point3);
            archive.Value(ref bounds.Point4);
            area = bounds;
        }
        else {
            area = null;
        }
    }
}
