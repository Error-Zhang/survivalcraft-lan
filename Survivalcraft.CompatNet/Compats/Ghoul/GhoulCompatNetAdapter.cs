using System.Reflection;
using Engine;
using Game.Network;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Game;

/// <summary>
/// CompatNet adapter for the optional Ghoul mod.
/// </summary>
public sealed class GhoulCompatNetAdapter : CompatNetAdapterLoader {
    public const ushort StableAdapterId = 12;

    public override ushort AdapterId => StableAdapterId;
    public override string AdapterName => GhoulCompatNetRuntime.TargetPackageName;

    public override void OnPreloadAssemblies(IList<ModAssemblyImage> assemblies) {
        foreach (ModAssemblyImage image in assemblies ?? Array.Empty<ModAssemblyImage>()) {
            string packageName = image?.Owner?.modInfo?.PackageName;
            string fileName = image?.FileName;
            if (string.Equals(packageName, GhoulCompatNetRuntime.TargetPackageName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(fileName, GhoulCompatNetRuntime.TargetAssemblyName, StringComparison.OrdinalIgnoreCase)) {
                try {
                    if (GhoulCompatNetPatcher.Patch(image)) {
                        Log.Information(
                            $"[Survivalcraft.CompatNet] Patched Ghoul compatibility paths in {image.FileName}.");
                    }
                }
                catch (Exception e) {
                    Log.Error(
                        $"[Survivalcraft.CompatNet] Failed to patch Ghoul assembly {image.FileName}: {e}");
                }
            }
        }
    }

    public override void HandleRequest(CompatNetPacket packet, PlayerData playerData) {
        // Ghoul has no request-side synchronization adapter. This loader only
        // applies startup compatibility patches to the optional assembly.
    }

    private static class GhoulCompatNetPatcher {
        private const string GameWidgetTypeName = "GameWidget";

        public static bool Patch(ModAssemblyImage image) {
            using MemoryStream input = new(image.Bytes, writable: false);
            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                input,
                new ReaderParameters { InMemory = true, ReadSymbols = false });

            ModuleDefinition module = assembly.MainModule;
            MethodInfo guiWidgetBridge = typeof(GhoulCompatNetRuntime).GetMethod(
                nameof(GhoulCompatNetRuntime.GetGuiWidget),
                BindingFlags.Static | BindingFlags.Public);
            MethodInfo worldPathBridge = typeof(GhoulCompatNetRuntime).GetMethod(
                nameof(GhoulCompatNetRuntime.GetWorldPathDefault),
                BindingFlags.Static | BindingFlags.Public);
            if (guiWidgetBridge == null || worldPathBridge == null) {
                return false;
            }

            MethodReference guiWidgetBridgeReference = module.ImportReference(guiWidgetBridge);
            MethodReference worldPathBridgeReference = module.ImportReference(worldPathBridge);
            bool changed = false;
            foreach (TypeDefinition type in module.Types.SelectMany(FlattenTypes)) {
                foreach (MethodDefinition method in type.Methods) {
                    if (!method.HasBody) {
                        continue;
                    }

                    for (int i = 1; i < method.Body.Instructions.Count; i++) {
                        Instruction instruction = method.Body.Instructions[i];
                        if (instruction.OpCode != OpCodes.Ldfld
                            || instruction.Operand is not FieldReference field
                            || field.Name != nameof(ContainerWidget.Children)
                            || field.DeclaringType.FullName != typeof(ContainerWidget).FullName) {
                            continue;
                        }

                        Instruction previous = method.Body.Instructions[i - 1];
                        if ((previous.OpCode != OpCodes.Call
                                && previous.OpCode != OpCodes.Callvirt)
                            || previous.Operand is not MethodReference getter
                            || (!string.Equals(
                                    getter.ReturnType.FullName,
                                    GameWidgetTypeName,
                                    StringComparison.Ordinal)
                                && !string.Equals(
                                    getter.ReturnType.Name,
                                    GameWidgetTypeName,
                                    StringComparison.Ordinal))) {
                            continue;
                        }

                        // Older Ghoul builds assume GameWidget.Children is the
                        // UI root.  The API now keeps the gameplay UI below
                        // GameWidget.GuiWidget, so route those legacy lookups
                        // through the current root before reading Children.
                        Instruction getGuiWidget = Instruction.Create(
                            OpCodes.Call,
                            guiWidgetBridgeReference);
                        method.Body.GetILProcessor().InsertBefore(instruction, getGuiWidget);
                        i++;
                        changed = true;
                    }
                }
            }

            int patchedWorldPathCount = PatchWorldPathLookups(
                module,
                worldPathBridgeReference);
            changed |= patchedWorldPathCount > 0;

            if (!changed) {
                return false;
            }

            using MemoryStream output = new();
            assembly.Write(output, new WriterParameters { WriteSymbols = false });
            image.Bytes = output.ToArray();
            return true;
        }

        private static int PatchWorldPathLookups(
            ModuleDefinition module,
            MethodReference worldPathBridge) {
            TypeDefinition type = module.Types
                .SelectMany(FlattenTypes)
                .FirstOrDefault(candidate => candidate.FullName == "SAGhoul.Tartareosity.SubsystemWorld");
            MethodDefinition method = type?.Methods.FirstOrDefault(candidate => candidate.Name == "Load");
            if (method == null || !method.HasBody) {
                return 0;
            }

            int patchedCount = 0;
            ILProcessor processor = method.Body.GetILProcessor();
            for (int i = 2; i < method.Body.Instructions.Count; i++) {
                Instruction directoryName = method.Body.Instructions[i];
                Instruction worldInfo = method.Body.Instructions[i - 1];
                Instruction loadingScreen = method.Body.Instructions[i - 2];
                if (directoryName.OpCode != OpCodes.Ldfld
                    || directoryName.Operand is not FieldReference directoryField
                    || directoryField.Name != nameof(WorldInfo.DirectoryName)
                    || directoryField.DeclaringType.FullName != typeof(WorldInfo).FullName
                    || worldInfo.OpCode != OpCodes.Ldfld
                    || worldInfo.Operand is not FieldReference worldInfoField
                    || worldInfoField.Name != "m_worldInfo"
                    || worldInfoField.DeclaringType.FullName != typeof(GameLoadingScreen).FullName
                    || !IsLoadLocal(loadingScreen)) {
                    continue;
                }

                processor.Remove(directoryName);
                processor.Remove(worldInfo);
                processor.Remove(loadingScreen);
                processor.InsertBefore(
                    method.Body.Instructions[i - 2],
                    Instruction.Create(OpCodes.Call, worldPathBridge));
                patchedCount++;
                i--;
            }

            return patchedCount;
        }

        private static bool IsLoadLocal(Instruction instruction) {
            return instruction.OpCode.Code is Code.Ldloc
                or Code.Ldloc_0
                or Code.Ldloc_1
                or Code.Ldloc_2
                or Code.Ldloc_3
                or Code.Ldloc_S;
        }

        private static IEnumerable<TypeDefinition> FlattenTypes(TypeDefinition type) {
            yield return type;
            foreach (TypeDefinition nested in type.NestedTypes.SelectMany(FlattenTypes)) {
                yield return nested;
            }
        }
    }
}
