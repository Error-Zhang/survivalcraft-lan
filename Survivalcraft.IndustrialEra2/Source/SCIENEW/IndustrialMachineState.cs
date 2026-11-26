using System;
using System.Collections.Generic;
using System.Reflection;
using Engine;
using Game.Network;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

internal static class IndustrialMachineState {
    private const BindingFlags InstanceFields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private static readonly string[] s_stateNames = [
        "m_craftingProgress", "m_smeltingProgress", "m_isWorking", "m_workState",
        "start", "m_start", "m_isPowering", "<Ignited>k__BackingField",
        "m_voltage", "m_power", "m_inNum", "m_outNum", "m_inIndex", "m_outIndex",
        "Mode", "<Mode>k__BackingField", "m_mode", "m_radius", "ElectricalControl",
        "m_normal", "<Yaw>k__BackingField", "<Pitch>k__BackingField",
        "m_coreUFuelRodCount", "m_coreCarbonRodCount", "m_coreControlRodCount",
        "m_sorterUFuelRodCount", "m_sorterCarbonRodCount", "m_sorterControlRodCount",
        "m_craftingTime", "m_isCrafting", "m_heatLevel", "m_fireTimeRemaining", "m_cacheEnergy"
    ];
    private static readonly Dictionary<Type, FieldInfo[]> s_fields = new();

    private static FieldInfo[] GetFields(Component component) {
        if (component == null) {
            return [];
        }
        Type type = component.GetType();
        if (type.Assembly != typeof(IndustrialModLoader).Assembly) {
            return [];
        }
        if (s_fields.TryGetValue(type, out FieldInfo[] existing)) {
            return existing;
        }

        List<FieldInfo> fields = new();
        foreach (string name in s_stateNames) {
            for (Type current = type; current != null; current = current.BaseType) {
                FieldInfo field = current.GetField(name, InstanceFields | BindingFlags.DeclaredOnly);
                if (field != null) {
                    if (!field.IsInitOnly && (field.FieldType == typeof(float) || field.FieldType == typeof(bool)
                        || field.FieldType == typeof(Vector3) || field.FieldType.IsEnum || field.FieldType == typeof(int))) {
                        fields.Add(field);
                    }
                    break;
                }
            }
        }
        return s_fields[type] = fields.ToArray();
    }

    public static bool HasState(Component component) => component is IIndustrialQuarry or ComponentTurretTargeting || GetFields(component).Length > 0;

    public static void Write(PacketSerializer archive, Component component) {
        FieldInfo[] fields = GetFields(component);
        byte count = (byte)fields.Length;
        archive.Value(ref count);
        foreach (FieldInfo field in fields) {
            if (field.FieldType == typeof(Vector3)) {
                Vector3 vector = (Vector3)field.GetValue(component);
                archive.Value(ref vector);
            }
            else {
                float value = Convert.ToSingle(field.GetValue(component));
                archive.Value(ref value);
            }
        }
        SerializeSpecialState(archive, component);
    }

    public static void Read(PacketSerializer archive, Component component) {
        FieldInfo[] fields = GetFields(component);
        byte count = 0;
        archive.Value(ref count);
        if (fields.Length != count) {
            throw new System.IO.InvalidDataException("Industrial device snapshot does not match its component schema.");
        }
        for (int i = 0; i < count; i++) {
            FieldInfo field = fields[i];
            if (field.FieldType == typeof(Vector3)) {
                Vector3 vector = default;
                archive.Value(ref vector);
                field.SetValue(component, vector);
                continue;
            }
            float value = 0;
            archive.Value(ref value);
            if (!float.IsFinite(value)) {
                continue;
            }
            Type kind = field.FieldType;
            if (kind == typeof(float)) {
                field.SetValue(component, value);
            }
            else if (kind == typeof(bool)) {
                field.SetValue(component, value != 0);
            }
            else if (kind.IsEnum) {
                field.SetValue(component, Enum.ToObject(kind, (int)value));
            }
            else {
                field.SetValue(component, (int)value);
            }
        }
        SerializeSpecialState(archive, component);
    }

    private static void SerializeSpecialState(PacketSerializer archive, Component component) {
        Action<PacketSerializer> serialize = component switch {
            IIndustrialQuarry quarry => quarry.SerializeLanState,
            ComponentTurretTargeting targeting => targeting.SerializeSettings,
            _ => null
        };
        byte[] state = archive.IsWriting && serialize != null ? Game.CompatNetPayload.Write(serialize) : [];
        archive.Buffer(ref state, 8 * 1024);
        if (archive.IsReading && state.Length > 0 && serialize != null) {
            Game.CompatNetPayload.Read(state, serialize);
        }
    }
}
