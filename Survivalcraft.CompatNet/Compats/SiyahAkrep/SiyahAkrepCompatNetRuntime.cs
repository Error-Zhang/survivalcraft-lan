using System.Globalization;
using System.Reflection;
using Engine;
using Game.Network;
using GameEntitySystem;

namespace Game;

/// <summary>
/// Runtime bridges for the SiyahAkrep 2.8.3 package. The target assembly is
/// deliberately accessed through reflection so CompatNet can still load when
/// the optional mod is not installed.
/// </summary>
public static class SiyahAkrepCompatNetRuntime {
    public const string TargetPackageName = "ysf.siyahakrep";
    public const string TargetAssemblyName = "sc-siyahakrep.dll";

    private const double StatePublishInterval = 0.20;
    private const double InventoryPublishInterval = 0.35;
    private const double FullRefreshInterval = 5.0;
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly string[] s_allowedStateTokens = [
        "active", "ammo", "amount", "angle", "broken", "bullet", "count", "cooldown",
        "damage", "direction", "enabled", "energy", "explod", "fire", "firing", "fuel",
        "health", "index", "level", "loaded", "mode", "power", "progress", "recipe",
        "reload", "remain", "rotation", "running", "speed", "state", "target", "time",
        "velocity", "water", "fluid", "guide", "vehicle", "weapon", "value", "work"
    ];

    private static readonly string[] s_blockedStateTokens = [
        "array", "audio", "cache", "component", "dictionary", "event", "gui", "last",
        "list", "model", "particle", "project", "random", "subsystem", "task", "texture",
        "widget"
    ];

    private static Type s_saProjectileType;
    private static Type s_functionsType;
    private static int s_stateSequence;

    public static Projectile FireProjectileBridge(
        SubsystemProjectiles subsystemProjectiles,
        int value,
        Vector3 position,
        Vector3 velocity,
        Vector3 angularVelocity,
        ComponentCreature owner) {
        if (!NetworkManager.IsClientRunning) {
            return subsystemProjectiles.FireProjectile(
                value,
                position,
                velocity,
                angularVelocity,
                owner);
        }

        ComponentPlayer player = FindLocalPlayer(owner);
        if (player == null) {
            return null;
        }

        SiyahAkrepProjectilePayload payload = new() {
            CustomProjectile = false,
            Value = value,
            Position = position,
            Velocity = velocity,
            AngularVelocity = angularVelocity,
            OwnerEntityId = owner?.Entity?.Id ?? 0
        };
        CompatNetCore.QueueRequest(
            player,
            SiyahAkrepCompatNetAdapter.StableAdapterId,
            (ushort)SiyahAkrepCompatNetOperation.ProjectileRequest,
            SiyahAkrepCompatNetPayloads.Write(payload));
        return null;
    }

    /// <summary>
    /// Direct SAProjectile paths call FireProjectileFast and otherwise bypass
    /// the engine's OnNetworkProjectileFired hook.
    /// </summary>
    public static void FireProjectileFastBridge(
        SubsystemProjectiles subsystemProjectiles,
        Projectile projectile) {
        if (projectile == null) {
            return;
        }

        if (!NetworkManager.IsClientRunning) {
            subsystemProjectiles.FireProjectileFast(projectile);
            if (NetworkManager.IsServerRunning && IsTargetProjectile(projectile)) {
                QueueProjectileBroadcast(projectile);
            }
            return;
        }

        ComponentPlayer player = FindLocalPlayer(projectile.Owner);
        if (player == null) {
            // The server owns AI, vehicle and turret projectiles. Suppressing
            // the client-side copy prevents duplicate bullets and explosions.
            return;
        }

        SiyahAkrepProjectilePayload payload = CreateProjectilePayload(projectile);
        CompatNetCore.QueueRequest(
            player,
            SiyahAkrepCompatNetAdapter.StableAdapterId,
            (ushort)SiyahAkrepCompatNetOperation.ProjectileRequest,
            SiyahAkrepCompatNetPayloads.Write(payload));
    }

    /// <summary>
    /// A custom bullet's block behavior runs on clients as well as the server.
    /// Keep its terrain destruction authoritative so speculative client hits do
    /// not leave foliage removed only on the firing client.
    /// </summary>
    public static void DestroyProjectileCellBridge(
        SubsystemTerrain subsystemTerrain,
        int toolLevel,
        int x,
        int y,
        int z,
        int newValue,
        bool noDrop,
        bool noParticleSystem,
        MovingBlock movingBlock,
        WorldItem worldItem) {
        if (subsystemTerrain == null
            || NetworkManager.IsClientRunning
            && !NetworkManager.IsServerRunning
            && worldItem is Projectile projectile
            && IsTargetProjectile(projectile)) {
            return;
        }

        subsystemTerrain.DestroyCell(
            toolLevel,
            x,
            y,
            z,
            newValue,
            noDrop,
            noParticleSystem,
            movingBlock);
    }

    /// <summary>
    /// Machine interaction callbacks are replayed on the server. UI must only
    /// be opened for the locally controlled player, not for its remote replica.
    /// </summary>
    public static void ShowDialogForLocalOwner(
        ContainerWidget containerWidget,
        Dialog dialog,
        object owner) {
        if (IsLocalPlayerOwner(owner)) {
            DialogsManager.ShowDialog(containerWidget, dialog);
        }
    }

    /// <summary>
    /// The mod opens the document/entries screens from replicated player and
    /// item update callbacks. Only the locally controlled player owns UI.
    /// </summary>
    public static void SwitchScreenForLocalOwner(Screen screen, object[] parameters, object owner) {
        if (IsLocalPlayerOwner(owner)) {
            ScreensManager.SwitchScreen(screen, parameters);
        }
    }

    private static bool IsLocalPlayerOwner(object owner) {
        ComponentPlayer player = owner switch {
            ComponentPlayer componentPlayer => componentPlayer,
            ComponentMiner miner => miner.ComponentPlayer,
            Component component => component.Entity?.FindComponent<ComponentPlayer>(),
            Subsystem subsystem => subsystem.Project?
                .FindSubsystem<SubsystemPlayers>()?
                .ComponentPlayers.FirstOrDefault(candidate => candidate.PlayerData?.IsMainPlayer == true),
            _ => null
        };

        // On a dedicated server, PlayerData.IsMainPlayer can be true for
        // PlayerIndex 0 even though there is no local UI. Require an active
        // client before treating that flag as ownership of a screen.
        return NetworkManager.IsClientRunning && player?.PlayerData?.IsMainPlayer == true
            || !NetworkManager.IsClientRunning && !NetworkManager.IsServerRunning;
    }

    public static bool TryQueueMissile(
        Vector3 position,
        Quaternion rotation,
        Project project,
        float remainTime,
        float directlyTime,
        int guideLevel,
        bool antiVehicle,
        ComponentBody target,
        ComponentCreature owner,
        Vector3 velocity) {
        return TryQueueMissile(
            position,
            rotation,
            project,
            remainTime,
            directlyTime,
            guideLevel,
            antiVehicle,
            target,
            owner,
            velocity,
            null);
    }

    public static bool TryQueueMissile(
        Vector3 position,
        Quaternion rotation,
        Project project,
        float remainTime,
        float directlyTime,
        int guideLevel,
        bool antiVehicle,
        ComponentBody target,
        ComponentCreature owner,
        Vector3 velocity,
        float startEngineTime) {
        return TryQueueMissile(
            position,
            rotation,
            project,
            remainTime,
            directlyTime,
            guideLevel,
            antiVehicle,
            target,
            owner,
            velocity,
            (float?)startEngineTime);
    }

    public static bool TryQueueMissile(
        Vector3 position,
        Quaternion rotation,
        Project project,
        float remainTime,
        float directlyTime,
        int guideLevel,
        bool antiVehicle,
        ComponentBody target,
        ComponentCreature owner,
        Vector3 velocity,
        float? startEngineTime) {
        if (!NetworkManager.IsClientRunning) {
            return false;
        }

        ComponentPlayer player = FindLocalPlayer(owner);
        if (player == null) {
            // Remote AI and vehicle logic must wait for the server's entity.
            return true;
        }

        SiyahAkrepMissilePayload payload = new() {
            Position = position,
            Rotation = rotation,
            RemainTime = remainTime,
            DirectlyTime = directlyTime,
            GuideLevel = guideLevel,
            AntiVehicle = antiVehicle,
            TargetEntityId = target?.Entity?.Id ?? 0,
            Velocity = velocity,
            HasStartEngineTime = startEngineTime.HasValue,
            StartEngineTime = startEngineTime.GetValueOrDefault()
        };
        CompatNetCore.QueueRequest(
            player,
            SiyahAkrepCompatNetAdapter.StableAdapterId,
            (ushort)SiyahAkrepCompatNetOperation.MissileRequest,
            SiyahAkrepCompatNetPayloads.Write(payload));
        return true;
    }

    public static void HandleServerProjectile(
        SiyahAkrepProjectilePayload payload,
        PlayerData playerData) {
        SubsystemProjectiles subsystem = GameManager.Project?.FindSubsystem<SubsystemProjectiles>();
        ComponentCreature owner = playerData?.ComponentPlayer;
        if (subsystem == null || owner == null || payload == null) {
            return;
        }

        if (payload.CustomProjectile) {
            Projectile projectile = CreateCustomProjectile(subsystem, payload, owner);
            if (projectile == null) {
                return;
            }

            subsystem.FireProjectileFast(projectile);
            QueueProjectileBroadcast(projectile);
            return;
        }

        // Ordinary paths retain the engine's existing ProjectilePacket and
        // block-behavior handling.
        subsystem.FireProjectile(
            payload.Value,
            payload.Position,
            payload.Velocity,
            payload.AngularVelocity,
            owner);
    }

    public static void HandleServerMissile(
        SiyahAkrepMissilePayload payload,
        PlayerData playerData) {
        if (payload == null || GameManager.Project == null) {
            return;
        }

        Type functionsType = ResolveFunctionsType();
        if (functionsType == null) {
            Log.Error("[Survivalcraft.CompatNet] SiyahAkrep Functions type was not found.");
            return;
        }

        ComponentBody target = GameManager.Project.FindEntity(payload.TargetEntityId)
            ?.FindComponent<ComponentBody>();
        ComponentCreature owner = playerData?.ComponentPlayer;
        int parameterCount = payload.HasStartEngineTime ? 11 : 10;
        MethodInfo method = functionsType.GetMethods(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(candidate =>
                candidate.Name == "AddMissile"
                && candidate.GetParameters().Length == parameterCount);
        if (method == null) {
            Log.Error(
                $"[Survivalcraft.CompatNet] SiyahAkrep AddMissile({parameterCount}) was not found.");
            return;
        }

        List<object> arguments = [
            payload.Position,
            payload.Rotation,
            GameManager.Project,
            payload.RemainTime,
            payload.DirectlyTime,
            payload.GuideLevel,
            payload.AntiVehicle,
            target,
            owner,
            payload.Velocity
        ];
        if (payload.HasStartEngineTime) {
            arguments.Add(payload.StartEngineTime);
        }

        try {
            method.Invoke(null, [.. arguments]);
        }
        catch (Exception e) {
            Log.Error($"[Survivalcraft.CompatNet] Failed to create SiyahAkrep missile: {e}");
        }
    }

    public static Projectile ApplyProjectileBroadcast(
        SiyahAkrepProjectilePayload payload) {
        if (payload == null || GameManager.Project == null) {
            return null;
        }

        SubsystemProjectiles subsystem = GameManager.Project.FindSubsystem<SubsystemProjectiles>();
        if (subsystem == null) {
            return null;
        }

        ComponentCreature owner = GameManager.Project.FindEntity(payload.OwnerEntityId)
            ?.FindComponent<ComponentCreature>();
        if (!payload.CustomProjectile) {
            return subsystem.FireProjectile(
                payload.Value,
                payload.Position,
                payload.Velocity,
                payload.AngularVelocity,
                owner,
                true);
        }

        Projectile projectile = CreateCustomProjectile(subsystem, payload, owner);
        if (projectile == null) {
            return null;
        }

        subsystem.FireProjectileFast(projectile);
        return projectile;
    }

    public static void PublishState(
        bool targetLoaded,
        ref double nextPublishTime,
        ref double nextFullRefreshTime,
        Dictionary<string, byte[]> lastPayloads) {
        if (!targetLoaded || !NetworkManager.IsServerRunning || GameManager.Project == null) {
            return;
        }

        double now = Time.RealTime;
        if (now < nextPublishTime) {
            return;
        }
        nextPublishTime = now + StatePublishInterval;
        bool fullRefresh = now >= nextFullRefreshTime;
        if (fullRefresh) {
            nextFullRefreshTime = now + FullRefreshInterval;
        }

        List<SiyahAkrepStateRecord> changed = [];
        foreach (Entity entity in GameManager.Project.Entities) {
            if (entity == null) {
                continue;
            }

            foreach (Component component in entity.Components) {
                if (!IsTargetComponent(component)
                    || !TryCaptureState(component, out SiyahAkrepStateRecord record)) {
                    continue;
                }

                string key = $"entity:{record.EntityId}:{record.ComponentTypeName}";
                byte[] current = CompatNetPayload.Write(record.Serialize);
                if (fullRefresh
                    || !lastPayloads.TryGetValue(key, out byte[] previous)
                    || !previous.AsSpan().SequenceEqual(current)) {
                    lastPayloads[key] = current;
                    changed.Add(record);
                }
            }
        }

        foreach (Subsystem subsystem in GameManager.Project.Subsystems) {
            if (!IsTargetAssembly(subsystem?.GetType().Assembly)
                || !TryCaptureState(subsystem, out SiyahAkrepStateRecord record)) {
                continue;
            }

            string key = $"subsystem:{record.ComponentTypeName}";
            byte[] current = CompatNetPayload.Write(record.Serialize);
            if (fullRefresh
                || !lastPayloads.TryGetValue(key, out byte[] previous)
                || !previous.AsSpan().SequenceEqual(current)) {
                lastPayloads[key] = current;
                changed.Add(record);
            }
        }

        QueueStateBatches(changed);
    }

    public static void ApplyState(byte[] payload, ref int lastSequence) {
        SiyahAkrepStateBatch batch;
        try {
            batch = SiyahAkrepCompatNetPayloads.ReadState(payload);
        }
        catch {
            return;
        }

        if (batch.Sequence <= 0
            || (lastSequence != 0 && !IsSequenceAtLeast(batch.Sequence, lastSequence))) {
            return;
        }
        if (IsSequenceNewer(batch.Sequence, lastSequence)) {
            lastSequence = batch.Sequence;
        }

        foreach (SiyahAkrepStateRecord record in batch.Records) {
            if (record.IsSubsystem) {
                Subsystem subsystem = GameManager.Project?.Subsystems?.FirstOrDefault(candidate =>
                    candidate != null
                    && IsTargetAssembly(candidate.GetType().Assembly)
                    && candidate.GetType().FullName == record.ComponentTypeName);
                ApplyStateValues(subsystem, record.Values);
                continue;
            }

            Entity entity = GameManager.Project?.FindEntity(record.EntityId);
            if (entity == null || IsLocalPlayerEntity(entity)) {
                continue;
            }

            Component component = entity.Components.FirstOrDefault(candidate =>
                IsTargetComponent(candidate)
                && candidate.GetType().FullName == record.ComponentTypeName);
            ApplyStateValues(component, record.Values);
        }
    }

    public static void PublishInventories(
        bool targetLoaded,
        ref double nextPublishTime,
        ref double nextFullRefreshTime,
        ref int sequence,
        Dictionary<string, byte[]> lastPayloads) {
        if (!targetLoaded || !NetworkManager.IsServerRunning || GameManager.Project == null) {
            return;
        }

        double now = Time.RealTime;
        if (now < nextPublishTime) {
            return;
        }
        nextPublishTime = now + InventoryPublishInterval;
        bool fullRefresh = now >= nextFullRefreshTime;
        if (fullRefresh) {
            nextFullRefreshTime = now + FullRefreshInterval;
        }

        List<SiyahAkrepInventoryRecord> changed = [];
        foreach (Entity entity in GameManager.Project.Entities) {
            if (entity == null) {
                continue;
            }
            foreach (Component component in entity.Components) {
                if (!IsTargetComponent(component)
                    || component is not IInventory inventory
                    || !TryCaptureInventory(component, inventory, out SiyahAkrepInventoryRecord record)) {
                    continue;
                }
                AddChangedInventory(record, $"entity:{record.EntityId}:{record.OwnerTypeName}", fullRefresh, lastPayloads, changed);
            }
        }

        foreach (Subsystem subsystem in GameManager.Project.Subsystems) {
            if (!IsTargetAssembly(subsystem?.GetType().Assembly)
                || subsystem is not IInventory inventory
                || !TryCaptureInventory(subsystem, inventory, out SiyahAkrepInventoryRecord record)) {
                continue;
            }
            AddChangedInventory(record, $"subsystem:{record.OwnerTypeName}", fullRefresh, lastPayloads, changed);
        }

        if (changed.Count == 0) {
            return;
        }

        sequence = NextSequence(sequence);
        QueueInventoryBatches(changed, sequence, fullRefresh);
    }

    public static void ApplyInventories(byte[] payload, ref int lastSequence) {
        SiyahAkrepInventoryBatch batch;
        try {
            batch = SiyahAkrepCompatNetPayloads.ReadInventory(payload);
        }
        catch {
            return;
        }

        if (batch.Sequence <= 0
            || (lastSequence != 0 && !IsSequenceAtLeast(batch.Sequence, lastSequence))) {
            return;
        }
        if (IsSequenceNewer(batch.Sequence, lastSequence)) {
            lastSequence = batch.Sequence;
        }

        foreach (SiyahAkrepInventoryRecord record in batch.Records) {
            object owner;
            if (record.IsSubsystem) {
                owner = GameManager.Project?.Subsystems?.FirstOrDefault(candidate =>
                    candidate is IInventory
                    && IsTargetAssembly(candidate.GetType().Assembly)
                    && candidate.GetType().FullName == record.OwnerTypeName);
            }
            else {
                Entity entity = GameManager.Project?.FindEntity(record.EntityId);
                if (entity == null || IsLocalPlayerEntity(entity)) {
                    continue;
                }
                owner = entity.Components.FirstOrDefault(candidate =>
                    candidate is IInventory
                    && IsTargetComponent(candidate)
                    && candidate.GetType().FullName == record.OwnerTypeName);
            }

            if (owner is IInventory inventory) {
                ApplyInventory(owner, inventory, record);
            }
        }
    }

    public static bool ShouldSkipClientUpdate(IUpdateable updateable, bool targetLoaded) {
        if (!targetLoaded
            || !NetworkManager.IsClientRunning
            || NetworkManager.IsServerRunning
            || updateable is not Component component
            || !IsTargetAssembly(component.GetType().Assembly)
            || component.Entity == null
            || IsLocalPlayerEntity(component.Entity)) {
            return false;
        }

        string name = component.GetType().Name;
        if (name.Contains("Model", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Widget", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Visual", StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        return name.Contains("Guns", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Vehicle", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Missile", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Turret", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Human", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Guard", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Pathfinder", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Machine", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Merchant", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Grenade", StringComparison.OrdinalIgnoreCase);
    }

    private static ComponentPlayer FindLocalPlayer(ComponentCreature creature) {
        ComponentPlayer player = creature?.Entity?.FindComponent<ComponentPlayer>();
        return player?.PlayerData?.IsMainPlayer == true ? player : null;
    }

    private static SiyahAkrepProjectilePayload CreateProjectilePayload(Projectile projectile) {
        return new SiyahAkrepProjectilePayload {
            CustomProjectile = IsTargetProjectile(projectile),
            Value = projectile.Value,
            Position = projectile.Position,
            Velocity = projectile.Velocity,
            AngularVelocity = projectile.AngularVelocity,
            TrailOffset = projectile.TrailOffset,
            OwnerEntityId = projectile.Owner?.Entity?.Id ?? 0
        };
    }

    private static void QueueProjectileBroadcast(Projectile projectile) {
        CompatNetCore.QueueBroadcast(
            SiyahAkrepCompatNetAdapter.StableAdapterId,
            (ushort)SiyahAkrepCompatNetOperation.ProjectileBroadcast,
            SiyahAkrepCompatNetPayloads.Write(CreateProjectilePayload(projectile)));
    }

    private static Projectile CreateCustomProjectile(
        SubsystemProjectiles subsystem,
        SiyahAkrepProjectilePayload payload,
        ComponentCreature owner) {
        Type projectileType = ResolveSaProjectileType();
        if (projectileType == null || !typeof(Projectile).IsAssignableFrom(projectileType)) {
            return null;
        }

        MethodInfo method = typeof(SubsystemProjectiles).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(candidate =>
                candidate.Name == "CreateProjectile"
                && candidate.IsGenericMethodDefinition
                && candidate.GetGenericArguments().Length == 1
                && candidate.GetParameters().Length == 5);
        if (method == null) {
            return null;
        }

        try {
            MethodInfo constructed = method.MakeGenericMethod(projectileType);
            Projectile projectile = constructed.Invoke(subsystem, [
                payload.Value,
                payload.Position,
                payload.Velocity,
                payload.AngularVelocity,
                owner
            ]) as Projectile;
            if (projectile != null) {
                projectile.TrailOffset = payload.TrailOffset;
            }
            return projectile;
        }
        catch (Exception e) {
            Log.Error($"[Survivalcraft.CompatNet] Failed to create SAProjectile: {e}");
            return null;
        }
    }

    private static Type ResolveSaProjectileType() {
        return s_saProjectileType ??= ResolveType("SiyahAkrep.Common.SAProjectile");
    }

    private static Type ResolveFunctionsType() {
        return s_functionsType ??= ResolveType("SiyahAkrep.Managers.Functions");
    }

    private static Type ResolveType(string fullName) {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            Type type = assembly.GetType(fullName, throwOnError: false);
            if (type != null && IsTargetAssembly(type.Assembly)) {
                return type;
            }
        }
        return null;
    }

    private static bool IsTargetProjectile(Projectile projectile) {
        return projectile != null
            && IsTargetAssembly(projectile.GetType().Assembly)
            && projectile.GetType().FullName == "SiyahAkrep.Common.SAProjectile";
    }

    private static bool IsTargetComponent(Component component) {
        return component != null && IsTargetAssembly(component.GetType().Assembly);
    }

    private static bool IsLocalPlayerEntity(Entity entity) {
        return entity?.FindComponent<ComponentPlayer>()?.PlayerData?.IsMainPlayer == true;
    }

    private static bool TryCaptureState(object instance, out SiyahAkrepStateRecord record) {
        record = new() {
            EntityId = instance is Component component ? component.Entity.Id : 0,
            IsSubsystem = instance is Subsystem,
            ComponentTypeName = instance?.GetType().FullName ?? string.Empty
        };
        if (instance == null || string.IsNullOrEmpty(record.ComponentTypeName)) {
            return false;
        }
        if (!record.IsSubsystem && record.EntityId <= 0) {
            return false;
        }

        HashSet<string> names = [];
        for (Type current = instance.GetType();
             current != null && current != typeof(object);
             current = current.BaseType) {
            foreach (FieldInfo field in current.GetFields(
                         InstanceFlags | BindingFlags.DeclaredOnly)) {
                if (record.Values.Count >= SiyahAkrepCompatNetPayloads.MaximumValuesPerRecord
                    || field.IsStatic
                    || !IsTargetAssembly(field.DeclaringType?.Assembly)
                    || names.Contains(field.Name)
                    || !TryReadStateValue(field.Name, field.FieldType, () => field.GetValue(instance), out SiyahAkrepStateValue value)) {
                    continue;
                }
                record.Values.Add(value);
                names.Add(field.Name);
            }

            foreach (PropertyInfo property in current.GetProperties(
                         InstanceFlags | BindingFlags.DeclaredOnly)) {
                if (record.Values.Count >= SiyahAkrepCompatNetPayloads.MaximumValuesPerRecord
                    || names.Contains(property.Name)
                    || property.GetIndexParameters().Length != 0
                    || !IsTargetAssembly(property.DeclaringType?.Assembly)
                    || property.GetMethod == null
                    || !TryReadStateValue(property.Name, property.PropertyType, () => property.GetValue(instance), out SiyahAkrepStateValue value)) {
                    continue;
                }
                record.Values.Add(value);
                names.Add(property.Name);
            }
        }

        return record.Values.Count > 0;
    }

    private static bool TryReadStateValue(
        string name,
        Type type,
        Func<object> getter,
        out SiyahAkrepStateValue value) {
        value = null;
        if (!IsAllowedStateName(name) || type == null) {
            return false;
        }

        try {
            object raw = getter();
            if (raw == null) {
                return false;
            }
            value = new SiyahAkrepStateValue { Name = name };
            if (type == typeof(bool)) {
                value.Kind = SiyahAkrepStateValueKind.Boolean;
                value.BooleanValue = (bool)raw;
                return true;
            }
            if (type == typeof(byte)) {
                value.Kind = SiyahAkrepStateValueKind.Byte;
                value.ByteValue = (byte)raw;
                return true;
            }
            if (type.IsEnum || type == typeof(sbyte) || type == typeof(short)
                || type == typeof(ushort) || type == typeof(int) || type == typeof(uint)
                || type == typeof(long) || type == typeof(ulong)) {
                value.Kind = SiyahAkrepStateValueKind.Integer;
                value.IntegerValue = Convert.ToInt32(raw, CultureInfo.InvariantCulture);
                return true;
            }
            if (type == typeof(float)) {
                value.Kind = SiyahAkrepStateValueKind.Single;
                value.SingleValue = (float)raw;
                return true;
            }
            if (type == typeof(double)) {
                value.Kind = SiyahAkrepStateValueKind.Double;
                value.DoubleValue = (double)raw;
                return true;
            }
            if (type == typeof(Point3)) {
                value.Kind = SiyahAkrepStateValueKind.Point3;
                value.Point3Value = (Point3)raw;
                return true;
            }
            if (type == typeof(Vector3)) {
                value.Kind = SiyahAkrepStateValueKind.Vector3;
                value.Vector3Value = (Vector3)raw;
                return true;
            }
            if (type == typeof(Quaternion)) {
                value.Kind = SiyahAkrepStateValueKind.Quaternion;
                value.QuaternionValue = (Quaternion)raw;
                return true;
            }
        }
        catch {
            // A version-skewed or throwing property is skipped individually.
        }
        return false;
    }

    private static void ApplyStateValues(object instance, List<SiyahAkrepStateValue> values) {
        if (instance == null || values == null || instance is Component component && IsLocalPlayerEntity(component.Entity)) {
            return;
        }

        foreach (SiyahAkrepStateValue value in values) {
            if (value == null || !IsAllowedStateName(value.Name)) {
                continue;
            }
            try {
                for (Type current = instance.GetType(); current != null && current != typeof(object); current = current.BaseType) {
                    FieldInfo field = current.GetField(value.Name, InstanceFlags | BindingFlags.DeclaredOnly);
                    if (field != null && !field.IsStatic && IsTargetAssembly(field.DeclaringType?.Assembly)) {
                        if (TryConvertStateValue(value, field.FieldType, out object converted)) {
                            field.SetValue(instance, converted);
                        }
                        break;
                    }

                    PropertyInfo property = current.GetProperty(value.Name, InstanceFlags | BindingFlags.DeclaredOnly);
                    if (property?.SetMethod != null && IsTargetAssembly(property.DeclaringType?.Assembly)
                        && TryConvertStateValue(value, property.PropertyType, out object propertyValue)) {
                        property.SetValue(instance, propertyValue);
                        break;
                    }
                }
            }
            catch {
                // One incompatible field must not prevent the remaining state
                // from reaching the client.
            }
        }
    }

    private static bool TryConvertStateValue(
        SiyahAkrepStateValue value,
        Type targetType,
        out object converted) {
        converted = null;
        object raw = value.Kind switch {
            SiyahAkrepStateValueKind.Boolean => value.BooleanValue,
            SiyahAkrepStateValueKind.Byte => value.ByteValue,
            SiyahAkrepStateValueKind.Integer => value.IntegerValue,
            SiyahAkrepStateValueKind.Single => value.SingleValue,
            SiyahAkrepStateValueKind.Double => value.DoubleValue,
            SiyahAkrepStateValueKind.Point3 => value.Point3Value,
            SiyahAkrepStateValueKind.Vector3 => value.Vector3Value,
            SiyahAkrepStateValueKind.Quaternion => value.QuaternionValue,
            _ => null
        };
        if (raw == null || targetType == null) {
            return false;
        }
        try {
            if (targetType.IsEnum && raw is int integer) {
                converted = Enum.ToObject(targetType, integer);
                return true;
            }
            if (targetType == raw.GetType()) {
                converted = raw;
                return true;
            }
            Type nullable = Nullable.GetUnderlyingType(targetType);
            if (nullable != null) {
                targetType = nullable;
            }
            if (targetType.IsPrimitive || targetType == typeof(decimal)) {
                converted = Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
                return true;
            }
        }
        catch {
        }
        return false;
    }

    private static bool IsAllowedStateName(string name) {
        if (string.IsNullOrEmpty(name)) {
            return false;
        }
        string lowered = name.ToLowerInvariant();
        return !s_blockedStateTokens.Any(lowered.Contains)
            && s_allowedStateTokens.Any(lowered.Contains);
    }

    private static bool TryCaptureInventory(
        object owner,
        IInventory inventory,
        out SiyahAkrepInventoryRecord record) {
        record = new() {
            EntityId = owner is Component component ? component.Entity.Id : 0,
            IsSubsystem = owner is Subsystem,
            OwnerTypeName = owner?.GetType().FullName ?? string.Empty,
            ActiveSlotIndex = inventory?.ActiveSlotIndex ?? -1
        };
        if ((!record.IsSubsystem && record.EntityId <= 0)
            || (record.IsSubsystem && record.EntityId != 0)
            || inventory == null
            || inventory.SlotsCount < 0
            || inventory.SlotsCount > SiyahAkrepCompatNetPayloads.MaximumSlotsPerInventory) {
            return false;
        }

        try {
            for (int i = 0; i < inventory.SlotsCount; i++) {
                int value = inventory.GetSlotValue(i);
                int count = inventory.GetSlotCount(i);
                if (value < 0 || count < 0) {
                    return false;
                }
                record.Slots.Add(new SiyahAkrepInventorySlot { Value = value, Count = count });
            }
            return true;
        }
        catch {
            return false;
        }
    }

    private static void ApplyInventory(
        object owner,
        IInventory inventory,
        SiyahAkrepInventoryRecord record) {
        try {
            if (record.Slots.Count != inventory.SlotsCount) {
                return;
            }
            if (record.ActiveSlotIndex >= 0) {
                inventory.ActiveSlotIndex = record.ActiveSlotIndex;
            }

            if (owner is ComponentInventoryBase baseInventory) {
                for (int i = 0; i < record.Slots.Count; i++) {
                    SiyahAkrepInventorySlot desired = record.Slots[i];
                    ComponentInventoryBase.Slot slot = baseInventory.m_slots[i];
                    slot.Value = desired.Value;
                    slot.Count = desired.Count;
                }
                return;
            }

            for (int i = 0; i < record.Slots.Count; i++) {
                SiyahAkrepInventorySlot desired = record.Slots[i];
                int currentCount = inventory.GetSlotCount(i);
                if (currentCount > 0) {
                    inventory.RemoveSlotItems(i, currentCount);
                }
                if (desired.Count > 0 && desired.Value > 0) {
                    inventory.AddSlotItems(i, desired.Value, desired.Count);
                }
            }
        }
        catch {
        }
    }

    private static void AddChangedInventory(
        SiyahAkrepInventoryRecord record,
        string key,
        bool fullRefresh,
        Dictionary<string, byte[]> lastPayloads,
        List<SiyahAkrepInventoryRecord> changed) {
        byte[] current = CompatNetPayload.Write(record.Serialize);
        if (fullRefresh
            || !lastPayloads.TryGetValue(key, out byte[] previous)
            || !previous.AsSpan().SequenceEqual(current)) {
            lastPayloads[key] = current;
            changed.Add(record);
        }
    }

    private static void QueueStateBatches(List<SiyahAkrepStateRecord> records) {
        if (records.Count == 0) {
            return;
        }
        int sequence = NextSequence(s_stateSequence);
        s_stateSequence = sequence;
        List<SiyahAkrepStateRecord> current = [];
        foreach (SiyahAkrepStateRecord record in records) {
            current.Add(record);
            byte[] payload = SiyahAkrepCompatNetPayloads.Write(new SiyahAkrepStateBatch {
                Sequence = sequence,
                Records = current
            });
            if (payload.Length <= CompatNetPacket.MaximumPayloadBytes) {
                continue;
            }
            current.RemoveAt(current.Count - 1);
            QueueStateBatch(current, sequence);
            current = [record];
        }
        QueueStateBatch(current, sequence);
    }

    private static void QueueStateBatch(List<SiyahAkrepStateRecord> records, int sequence) {
        if (records.Count == 0) {
            return;
        }
        byte[] payload = SiyahAkrepCompatNetPayloads.Write(new SiyahAkrepStateBatch {
            Sequence = sequence,
            Records = records
        });
        if (payload.Length <= CompatNetPacket.MaximumPayloadBytes) {
            CompatNetCore.QueueBroadcast(
                SiyahAkrepCompatNetAdapter.StableAdapterId,
                (ushort)SiyahAkrepCompatNetOperation.StateSnapshot,
                payload);
        }
    }

    private static void QueueInventoryBatches(
        List<SiyahAkrepInventoryRecord> records,
        int sequence,
        bool fullRefresh) {
        List<SiyahAkrepInventoryRecord> current = [];
        foreach (SiyahAkrepInventoryRecord record in records) {
            current.Add(record);
            byte[] payload = SiyahAkrepCompatNetPayloads.Write(new SiyahAkrepInventoryBatch {
                Sequence = sequence,
                FullRefresh = fullRefresh,
                Records = current
            });
            if (payload.Length <= CompatNetPacket.MaximumPayloadBytes) {
                continue;
            }
            current.RemoveAt(current.Count - 1);
            QueueInventoryBatch(current, sequence, fullRefresh);
            current = [record];
        }
        QueueInventoryBatch(current, sequence, fullRefresh);
    }

    private static void QueueInventoryBatch(
        List<SiyahAkrepInventoryRecord> records,
        int sequence,
        bool fullRefresh) {
        if (records.Count == 0) {
            return;
        }
        byte[] payload = SiyahAkrepCompatNetPayloads.Write(new SiyahAkrepInventoryBatch {
            Sequence = sequence,
            FullRefresh = fullRefresh,
            Records = records
        });
        if (payload.Length <= CompatNetPacket.MaximumPayloadBytes) {
            CompatNetCore.QueueBroadcast(
                SiyahAkrepCompatNetAdapter.StableAdapterId,
                (ushort)SiyahAkrepCompatNetOperation.InventorySnapshot,
                payload);
        }
    }

    private static int NextSequence(int sequence) => sequence == int.MaxValue ? 1 : sequence + 1;

    private static bool IsSequenceNewer(int sequence, int previous) {
        if (previous == 0) {
            return sequence > 0;
        }
        if (sequence == previous) {
            return false;
        }
        if (previous == int.MaxValue) {
            return sequence == 1;
        }
        if (previous < int.MaxValue / 2 && sequence > int.MaxValue / 2) {
            return false;
        }
        return sequence > previous;
    }

    private static bool IsSequenceAtLeast(int sequence, int previous) =>
        sequence == previous || IsSequenceNewer(sequence, previous);

    public static bool IsTargetAssembly(Assembly assembly) {
        if (assembly == null) {
            return false;
        }
        return NamesEqual(assembly.GetName().Name, TargetAssemblyName)
            || NamesEqual(assembly.ManifestModule?.Name, TargetAssemblyName);
    }

    private static bool NamesEqual(string left, string right) =>
        string.Equals(NormalizeName(left), NormalizeName(right), StringComparison.OrdinalIgnoreCase);

    private static string NormalizeName(string value) {
        if (string.IsNullOrEmpty(value)) {
            return string.Empty;
        }
        string result = value.Replace('\\', '/');
        int separator = result.LastIndexOf('/');
        if (separator >= 0) {
            result = result[(separator + 1)..];
        }
        return result.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? result[..^4]
            : result;
    }
}
