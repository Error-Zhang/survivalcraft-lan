using System.Reflection;
using Engine;
using Game.Network;
using GameEntitySystem;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Game;

/// <summary>
/// Multiplayer compatibility for [API1.9.4] SiyahAkrep 2.8.3.
/// </summary>
public sealed class SiyahAkrepCompatNetAdapter : ICompatNetAdapter {
    // 12 is occupied by Ghoul. 21 is the next stable ID reserved for this
    // adapter; it must stay unchanged once released to clients and servers.
    public const ushort StableAdapterId = 21;

    private bool m_targetPackageLoaded;
    private double m_nextStatePublishTime;
    private double m_nextStateFullRefreshTime;
    private int m_lastAppliedStateSequence;
    private double m_nextInventoryPublishTime;
    private double m_nextInventoryFullRefreshTime;
    private int m_inventorySequence;
    private int m_lastAppliedInventorySequence;
    private readonly Dictionary<string, byte[]> m_lastStatePayloads = [];
    private readonly Dictionary<string, byte[]> m_lastInventoryPayloads = [];

    public ushort AdapterId => StableAdapterId;

    public string AdapterName => SiyahAkrepCompatNetRuntime.TargetPackageName;

    public void OnPreloadAssemblies(IList<ModAssemblyImage> assemblies) {
        m_targetPackageLoaded = false;
        m_nextStatePublishTime = 0;
        m_nextStateFullRefreshTime = 0;
        m_lastAppliedStateSequence = 0;
        m_nextInventoryPublishTime = 0;
        m_nextInventoryFullRefreshTime = 0;
        m_inventorySequence = 0;
        m_lastAppliedInventorySequence = 0;
        m_lastStatePayloads.Clear();
        m_lastInventoryPayloads.Clear();

        foreach (ModAssemblyImage image in assemblies ?? Array.Empty<ModAssemblyImage>()) {
            if (!IsTargetImage(image)) {
                continue;
            }

            m_targetPackageLoaded = true;
            try {
                if (SiyahAkrepCompatNetPatcher.Patch(image)) {
                    Log.Information(
                        $"[Survivalcraft.CompatNet] Patched SiyahAkrep paths in {image.FileName}.");
                }
            }
            catch (Exception e) {
                Log.Error(
                    $"[Survivalcraft.CompatNet] Failed to patch SiyahAkrep assembly {image.FileName}: {e}");
            }
        }
    }

    public void HandleRequest(CompatNetPacket packet, PlayerData playerData) {
        if (packet == null || playerData?.ComponentPlayer == null) {
            return;
        }

        switch ((SiyahAkrepCompatNetOperation)packet.Operation) {
            case SiyahAkrepCompatNetOperation.ProjectileRequest:
                SiyahAkrepCompatNetRuntime.HandleServerProjectile(
                    SiyahAkrepCompatNetPayloads.ReadProjectile(packet.Payload),
                    playerData);
                break;
            case SiyahAkrepCompatNetOperation.MissileRequest:
                SiyahAkrepCompatNetRuntime.HandleServerMissile(
                    SiyahAkrepCompatNetPayloads.ReadMissile(packet.Payload),
                    playerData);
                break;
        }
    }

    public void HandleBroadcast(CompatNetPacket packet) {
        if (packet == null || !NetworkManager.IsClientRunning || NetworkManager.IsServerRunning) {
            return;
        }

        switch ((SiyahAkrepCompatNetOperation)packet.Operation) {
            case SiyahAkrepCompatNetOperation.ProjectileBroadcast:
                SiyahAkrepCompatNetRuntime.ApplyProjectileBroadcast(
                    SiyahAkrepCompatNetPayloads.ReadProjectile(packet.Payload));
                break;
            case SiyahAkrepCompatNetOperation.StateSnapshot:
                SiyahAkrepCompatNetRuntime.ApplyState(
                    packet.Payload,
                    ref m_lastAppliedStateSequence);
                break;
            case SiyahAkrepCompatNetOperation.InventorySnapshot:
                SiyahAkrepCompatNetRuntime.ApplyInventories(
                    packet.Payload,
                    ref m_lastAppliedInventorySequence);
                break;
        }
    }

    public void OnServerUpdate(float dt) {
        SiyahAkrepCompatNetRuntime.PublishState(
            m_targetPackageLoaded,
            ref m_nextStatePublishTime,
            ref m_nextStateFullRefreshTime,
            m_lastStatePayloads);
        SiyahAkrepCompatNetRuntime.PublishInventories(
            m_targetPackageLoaded,
            ref m_nextInventoryPublishTime,
            ref m_nextInventoryFullRefreshTime,
            ref m_inventorySequence,
            m_lastInventoryPayloads);
    }

    public bool ShouldSkipClientUpdate(IUpdateable updateable) {
        return SiyahAkrepCompatNetRuntime.ShouldSkipClientUpdate(
            updateable,
            m_targetPackageLoaded);
    }

    private static bool IsTargetImage(ModAssemblyImage image) {
        return image != null
            && (string.Equals(
                    image.Owner?.modInfo?.PackageName,
                    SiyahAkrepCompatNetRuntime.TargetPackageName,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    image.FileName,
                    SiyahAkrepCompatNetRuntime.TargetAssemblyName,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static class SiyahAkrepCompatNetPatcher {
        public static bool Patch(ModAssemblyImage image) {
            using MemoryStream input = new(image.Bytes, writable: false);
            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                input,
                new ReaderParameters { InMemory = true, ReadSymbols = false });

            ModuleDefinition module = assembly.MainModule;
            MethodReference projectileBridge = ResolveBridge(
                module,
                nameof(SiyahAkrepCompatNetRuntime.FireProjectileBridge),
                parameterCount: 6);
            MethodReference projectileFastBridge = ResolveBridge(
                module,
                nameof(SiyahAkrepCompatNetRuntime.FireProjectileFastBridge),
                parameterCount: 2);
            MethodReference destroyProjectileCellBridge = ResolveBridge(
                module,
                nameof(SiyahAkrepCompatNetRuntime.DestroyProjectileCellBridge),
                parameterCount: 10);
            MethodReference showDialogBridge = ResolveBridge(
                module,
                nameof(SiyahAkrepCompatNetRuntime.ShowDialogForLocalOwner),
                parameterCount: 3);
            MethodReference switchScreenBridge = ResolveBridge(
                module,
                nameof(SiyahAkrepCompatNetRuntime.SwitchScreenForLocalOwner),
                parameterCount: 3);
            bool changed = false;

            foreach (TypeDefinition type in module.Types.SelectMany(FlattenTypes)) {
                foreach (MethodDefinition method in type.Methods) {
                    if (!method.HasBody) {
                        continue;
                    }

                    ILProcessor il = method.Body.GetILProcessor();
                    for (int i = 0; i < method.Body.Instructions.Count; i++) {
                        Instruction instruction = method.Body.Instructions[i];
                        if (instruction.Operand is not MethodReference reference) {
                            continue;
                        }

                        if (reference.DeclaringType.FullName == typeof(ScreensManager).FullName
                            && reference.Name == nameof(ScreensManager.SwitchScreen)
                            && reference.Parameters.Count == 2
                            && TryCreateScreenOwnerLoad(type, method, out Instruction screenOwnerLoad)) {
                            il.InsertBefore(instruction, screenOwnerLoad);
                            instruction.OpCode = OpCodes.Call;
                            instruction.Operand = switchScreenBridge;
                            changed = true;
                        }
                        else if (reference.DeclaringType.FullName == typeof(DialogsManager).FullName
                            && reference.Name == nameof(DialogsManager.ShowDialog)
                            && reference.Parameters.Count == 2
                            && TryCreateDialogOwnerLoad(type, method, out Instruction ownerLoad)) {
                            il.InsertBefore(instruction, ownerLoad);
                            instruction.OpCode = OpCodes.Call;
                            instruction.Operand = showDialogBridge;
                            changed = true;
                        }
                        else if (type.FullName == "SiyahAkrep.Subsystems.SubsystemGunsBulletBlockBehavior"
                            && method.Name == "OnHitAsProjectile"
                            && reference.DeclaringType.FullName == typeof(SubsystemTerrain).FullName
                            && reference.Name == nameof(SubsystemTerrain.DestroyCell)
                            && reference.Parameters.Count == 8) {
                            // The original call stack already contains the
                            // terrain and DestroyCell arguments. Append the
                            // projectile so the bridge can reject client-only
                            // predictions for this target mod's bullets.
                            il.InsertBefore(instruction, Instruction.Create(OpCodes.Ldarg_3));
                            instruction.OpCode = OpCodes.Call;
                            instruction.Operand = destroyProjectileCellBridge;
                            changed = true;
                        }
                        else if (reference.DeclaringType.FullName != typeof(SubsystemProjectiles).FullName) {
                            continue;
                        }
                        else if (reference.Name == "FireProjectile"
                            && reference.Parameters.Count == 5
                            && reference is not GenericInstanceMethod) {
                            instruction.OpCode = OpCodes.Call;
                            instruction.Operand = projectileBridge;
                            changed = true;
                        }
                        else if (reference.Name == "FireProjectileFast"
                            && reference.Parameters.Count == 1) {
                            instruction.OpCode = OpCodes.Call;
                            instruction.Operand = projectileFastBridge;
                            changed = true;
                        }
                    }

                    if (type.FullName == "SiyahAkrep.Managers.Functions"
                        && method.Name == "AddMissile"
                        && method.IsStatic
                        && !method.ReturnType.IsValueType
                        && method.Parameters.Count is 10 or 11) {
                        MethodReference missileBridge = ResolveMissileBridge(module, method.Parameters.Count);
                        changed |= InjectMissileRequest(method, missileBridge);
                    }
                }
            }

            if (!changed) {
                return false;
            }

            using MemoryStream output = new();
            assembly.Write(output, new WriterParameters { WriteSymbols = false });
            image.Bytes = output.ToArray();
            return true;
        }

        private static bool TryCreateDialogOwnerLoad(
            TypeDefinition type,
            MethodDefinition method,
            out Instruction instruction) {
            instruction = null;

            if (type.FullName == "SiyahAkrep.Components.ComponentGunsUser"
                && method.HasThis
                && method.Name is "Update" or "set_LUPrestige") {
                instruction = Instruction.Create(OpCodes.Ldarg_0);
                return true;
            }

            bool eligibleMethod = type.FullName switch {
                "SiyahAkrep.Machine.SubsystemSAMachineBlockBehavior" =>
                    method.Name is "OnInteract" or "OnEditBlock" or "OnEditInventoryItem",
                "SiyahAkrep.Subsystems.SubsystemGMItemsBlockBehavior" => method.Name == "OnUse",
                _ => false
            };
            if (!eligibleMethod) {
                return false;
            }

            ParameterDefinition ownerParameter = method.Parameters.LastOrDefault(parameter =>
                parameter.ParameterType.FullName is "Game.ComponentPlayer" or "Game.ComponentMiner");
            if (ownerParameter == null) {
                return false;
            }

            instruction = Instruction.Create(OpCodes.Ldarg, ownerParameter);
            return true;
        }

        private static bool TryCreateScreenOwnerLoad(
            TypeDefinition type,
            MethodDefinition method,
            out Instruction instruction) {
            instruction = null;

            if (type.FullName == "SiyahAkrep.Components.ComponentGunsUser"
                && method.HasThis
                && method.Name == "UpdateButton") {
                instruction = Instruction.Create(OpCodes.Ldarg_0);
                return true;
            }

            if (type.FullName == "SiyahAkrep.Machine.SubsystemSAMachineBlockBehavior"
                && method.HasThis
                && method.Name == "\u00A0"
                && method.Parameters.Count == 0) {
                instruction = Instruction.Create(OpCodes.Ldarg_0);
                return true;
            }

            if (type.FullName != "SiyahAkrep.Subsystems.SubsystemGMItemsBlockBehavior"
                || method.Name != "OnUse") {
                return false;
            }

            ParameterDefinition ownerParameter = method.Parameters.LastOrDefault(parameter =>
                parameter.ParameterType.FullName is "Game.ComponentPlayer" or "Game.ComponentMiner");
            if (ownerParameter == null) {
                return false;
            }

            instruction = Instruction.Create(OpCodes.Ldarg, ownerParameter);
            return true;
        }

        private static MethodReference ResolveBridge(
            ModuleDefinition module,
            string name,
            int parameterCount) {
            MethodInfo method = typeof(SiyahAkrepCompatNetRuntime).GetMethods(
                    BindingFlags.Public | BindingFlags.Static)
                .First(candidate => candidate.Name == name
                    && candidate.GetParameters().Length == parameterCount);
            return module.ImportReference(method);
        }

        private static MethodReference ResolveMissileBridge(
            ModuleDefinition module,
            int parameterCount) {
            MethodInfo method = typeof(SiyahAkrepCompatNetRuntime).GetMethods(
                    BindingFlags.Public | BindingFlags.Static)
                .First(candidate => candidate.Name == nameof(SiyahAkrepCompatNetRuntime.TryQueueMissile)
                    && candidate.GetParameters().Length == parameterCount);
            return module.ImportReference(method);
        }

        private static bool InjectMissileRequest(
            MethodDefinition method,
            MethodReference bridge) {
            if (method.Body.Instructions.Any(instruction =>
                    instruction.Operand is MethodReference reference
                    && reference.Name == nameof(SiyahAkrepCompatNetRuntime.TryQueueMissile))) {
                return false;
            }

            Instruction originalStart = method.Body.Instructions[0];
            Instruction continueInstruction = Instruction.Create(OpCodes.Brfalse, originalStart);
            Instruction returnInstruction = Instruction.Create(OpCodes.Ldnull);
            Instruction retInstruction = Instruction.Create(OpCodes.Ret);
            List<Instruction> prefix = [];
            foreach (ParameterDefinition parameter in method.Parameters) {
                prefix.Add(Instruction.Create(OpCodes.Ldarg, parameter));
            }
            prefix.Add(Instruction.Create(OpCodes.Call, bridge));
            prefix.Add(continueInstruction);
            prefix.Add(returnInstruction);
            prefix.Add(retInstruction);

            ILProcessor il = method.Body.GetILProcessor();
            foreach (Instruction instruction in prefix) {
                il.InsertBefore(originalStart, instruction);
            }
            return true;
        }

        private static IEnumerable<TypeDefinition> FlattenTypes(TypeDefinition type) {
            yield return type;
            foreach (TypeDefinition nested in type.NestedTypes.SelectMany(FlattenTypes)) {
                yield return nested;
            }
        }
    }
}
