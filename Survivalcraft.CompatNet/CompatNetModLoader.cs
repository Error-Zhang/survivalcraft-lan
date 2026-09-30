using Engine;
using Game.Network;
using GameEntitySystem;

namespace Game;

public sealed class CompatNetModLoader : ModLoader {
    private static readonly GhoulCompatNetAdapter s_ghoulAdapter = new();
    private static readonly object s_preloadSync = new();

    private static readonly ICompatNetAdapter[] s_builtinAdapters = CreateBuiltInAdapters();

    private static IReadOnlyList<ICompatNetAdapter> BuiltInAdapters => s_builtinAdapters;

    private static ICompatNetAdapter[] CreateBuiltInAdapters() {
        List<ICompatNetAdapter> adapters = [
            s_ghoulAdapter
        ];
        return adapters.ToArray();
    }

    public override void __ModInitialize() {
        ModsManager.RegisterHook("OnLoadingFinished", this);
        ModsManager.RegisterHook("OnIUpdateableUpdate", this);
        ModsManager.RegisterHook("SubsystemUpdate", this);
        ModsManager.RegisterHook("OnShowDialog", this);
        foreach (ICompatNetAdapter adapter in BuiltInAdapters) {
            CompatNetAdapterRegistry.Register(adapter);
        }
    }

    public override void OnShowDialog(ref ContainerWidget parentWidget, ref Dialog dialog) {
        NeorxnaCompatRuntime.FixRecipeDialogParent(ref parentWidget, dialog);
    }

    public override void OnPreloadAssemblies(IList<ModAssemblyImage> assemblies) {
        lock (s_preloadSync) {
            // All preload loaders have been instantiated by this point. Rebuild
            // the registry so adapters added by separate compatibility mods are
            // included, while still keeping the built-in adapters available to
            // direct/unit-test callers.
            CompatNetCore.Reset();
            CompatNetAdapterRegistry.Reset();
            foreach (ICompatNetAdapter adapter in BuiltInAdapters) {
                CompatNetAdapterRegistry.Register(adapter);
            }
            foreach (ICompatNetAdapter adapter in ModsManager.ModLoaders.OfType<ICompatNetAdapter>()) {
                CompatNetAdapterRegistry.Register(adapter);
            }

            foreach (ICompatNetAdapter adapter in CompatNetAdapterRegistry.Adapters) {
                adapter.OnPreloadAssemblies(assemblies);
            }

        }
    }

    public override void OnLoadingFinished(List<Action> actions) {
        try {
            CompatNetCore.InitializeNetwork();
            Log.Information("[Survivalcraft.CompatNet] Registered CompatNetPacket.");
        }
        catch (Exception e) {
            Log.Error($"[Survivalcraft.CompatNet] Failed to register network packet: {e}");
        }
    }

    public override void OnIUpdateableUpdate(
        SubsystemUpdate subsystemUpdate,
        IUpdateable updateable,
        float dt,
        out bool skip) {
        skip = false;
        if (NetworkManager.IsServerRunning) {
            foreach (ICompatNetAdapter adapter in CompatNetAdapterRegistry.Adapters) {
                if (adapter.ShouldSkipServerUpdate(updateable)) {
                    skip = true;
                    return;
                }
            }
            return;
        }
        if (!NetworkManager.IsClientRunning) {
            return;
        }

        foreach (ICompatNetAdapter adapter in CompatNetAdapterRegistry.Adapters) {
            if (adapter.ShouldSkipClientUpdate(updateable)) {
                skip = true;
                return;
            }
        }
    }

    public override void SubsystemUpdate(SubsystemUpdate subsystemUpdate, float dt) {
        if (!NetworkManager.IsServerRunning) {
            return;
        }

        foreach (ICompatNetAdapter adapter in CompatNetAdapterRegistry.Adapters) {
            try {
                adapter.OnServerUpdate(dt);
            }
            catch (Exception e) {
                Log.Error(
                    $"[Survivalcraft.CompatNet] Server state update failed for {adapter.AdapterName}: {e}");
            }
        }
    }
}
