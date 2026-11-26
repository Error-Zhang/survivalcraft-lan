using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine;
using Game;
using Game.Network;
using GameEntitySystem;
using SCIENEW.VoltNet;
using RecipaediaEX;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Overlay;

namespace SCIENEW;

internal sealed class IndustrialEraLanAdapter : ICompatNetAdapter {
    private const ushort NetworkId = 11;
    private const ushort StateSnapshot = 4;
    private const ushort TankTransfer = 2;
    private const ushort MachineControl = 3;
    private const ushort TechTreeState = 5;
    private const ushort QuarryPose = 6;
    private const ushort RecipePlacement = 7;
    private const ushort RecipeResult = 8;
    private const ushort ProjectileState = 9;
    private const ushort ScreenLinks = 10;
    private const ushort VehicleInput = 11;
    private const ushort TargetingSettings = 12;
    private const ushort ToolCharge = 13;
    private const ushort RadioChannel = 14;
    private const int MaximumRecords = 128;
    private const int MaximumRecordSize = 16 * 1024;
    private const int MaximumTanks = 64;
    private readonly Dictionary<Point3, byte[]> m_deviceStates = new();
    private readonly Dictionary<Point3, (int Block, int Frequency, bool Active)> m_radioStates = new();
    private readonly Dictionary<int, byte[]> m_mobileStates = new();
    private readonly List<byte[]> m_pendingRecords = new();
    private readonly Dictionary<IInventory, (int Value, int Count)[]> m_inventoryStates = new(ReferenceEqualityComparer.Instance);
    private Project m_project;
    private Project m_remoteProject;
    private IndustrialTechTreeReplication m_hostTechTree = new();
    private IndustrialTechTreeReplication m_guestTechTree = new();
    private int m_pendingSize;
    private double m_nextUpdate;
    private double m_nextRefresh;
    private double m_nextVehicleInput;
    private byte[] m_screenLinks;
    private readonly Dictionary<ComponentPlayer, (ComponentBoat Boat, Vector2 Axes, double Expires)> m_vehicleInput = new(ReferenceEqualityComparer.Instance);

    internal IndustrialProjectileReplication Projectiles { get; } = new();

    internal void PublishProjectile(Projectile projectile, bool fired) =>
        Projectiles.PublishSpawn(projectile, fired, payload => CompatNetCore.QueueBroadcast(AdapterId, ProjectileState, payload));

    public ushort AdapterId => NetworkId;
    public string AdapterName => IndustrialModLoader.ModPackageName;

    public static bool IsRemote(Project project) {
        return NetworkManager.IsClientRunning && !NetworkManager.IsServerRunning && ReferenceEquals(GameManager.Project, project);
    }

    public void OnPreloadAssemblies(IList<ModAssemblyImage> assemblies) {
    }

    public bool ShouldSkipClientUpdate(IUpdateable updateable) {
        return updateable.GetType().Assembly == typeof(IndustrialModLoader).Assembly
            && updateable is not (SubsystemIEBenchmarkDisplay or SubsystemTerrainFracDrawer or SubsystemTechTree or ComponentFluidTank
                or ComponentCamera or ComponentLens or ComponentMirror or ComponentRadar or ComponentSubsystemScreensOperator
                or ComponentEntityOpener or ComponentClimbIEBlocks or ComponentCRTMonitor
                or ComponentRadarBackpack or ComponentVitalStatsN);
    }

    public void OnServerUpdate(float dt) {
        Project project = GameManager.Project;
        if (project != null) {
            Projectiles.Publish(project, payload => CompatNetCore.QueueBroadcast(AdapterId, ProjectileState, payload));
        }
        if (project == null || Time.RealTime < m_nextUpdate && ReferenceEquals(project, m_project)) {
            return;
        }

        bool refresh = !ReferenceEquals(project, m_project) || Time.RealTime >= m_nextRefresh;
        if (!ReferenceEquals(project, m_project)) {
            m_hostTechTree = new();
            m_screenLinks = null;
            m_vehicleInput.Clear();
        }
        m_project = project;
        m_nextUpdate = Time.RealTime + 0.25;
        if (refresh) {
            m_deviceStates.Clear();
            m_radioStates.Clear();
            m_inventoryStates.Clear();
            m_mobileStates.Clear();
            m_nextRefresh = Time.RealTime + 5;
        }

        SubsystemBlockEntities blocks = project.FindSubsystem<SubsystemBlockEntities>();
        SubsystemVoltNet voltage = project.FindSubsystem<SubsystemVoltNet>();
        if (blocks == null || voltage == null) {
            return;
        }

        foreach ((Point3 position, ComponentBlockEntity block) in blocks.m_blockEntities) {
            VoltElement element = voltage.GetVoltElement(position);
            Entity entity = block.Entity;
            SyncIndustrialInventories(entity);
            if (element != null || HasDeviceState(entity)) {
                QueueIfChanged(m_deviceStates, position, Capture(position, element, entity));
            }
        }
        foreach ((Point3 position, VoltElement element) in voltage.m_elements) {
            if (element is RadioVoltElement radio) {
                PublishRadioChannel(position, radio);
            }
            if (!blocks.m_blockEntities.ContainsKey(position)) {
                QueueIfChanged(m_deviceStates, position, Capture(position, element, null));
            }
        }
        foreach (Entity entity in project.Entities) {
            if (entity.FindComponent<ComponentBlockEntity>() == null && IsIndustrialMobile(entity)) {
                SyncIndustrialInventories(entity);
                if (HasDeviceState(entity)) {
                    QueueIfChanged(m_mobileStates, entity.Id, Capture(default, null, entity, byEntity: true));
                }
            }
        }
        FlushStates();
        if (project.FindSubsystem<SCIENEW.Screens.SubsystemScreens>() is SCIENEW.Screens.SubsystemScreens screens) {
            byte[] links = CompatNetPayload.Write(screens.SerializeLanLinks);
            if (refresh || m_screenLinks == null || !links.AsSpan().SequenceEqual(m_screenLinks)) {
                m_screenLinks = links;
                CompatNetCore.QueueBroadcast(AdapterId, ScreenLinks, links);
            }
        }
        m_hostTechTree.Publish(project.FindSubsystem<SubsystemTechTree>(), refresh,
            payload => CompatNetCore.QueueBroadcast(AdapterId, TechTreeState, payload));
        if (project.FindSubsystem<SubsystemQuarryDrillerMobiles>() is SubsystemQuarryDrillerMobiles quarries) {
            foreach (ComponentQuarryDriller mobile in quarries.Mobiles) {
                byte[] pose = CompatNetPayload.Write(archive => {
                    int entityId = mobile.Entity.Id;
                    archive.Value(ref entityId);
                    mobile.SerializeLanPose(archive);
                });
                CompatNetCore.QueueBroadcast(AdapterId, QuarryPose, pose);
            }
        }
    }

    internal static bool IsIndustrialMobile(Entity entity) {
        return entity != null && (entity.FindComponent<ComponentBoat>() is ComponentBoat boat
            && boat.GetType().Assembly == typeof(IndustrialModLoader).Assembly
            || entity.FindComponent<ComponentTrain>() != null
            || entity.FindComponent<ComponentRadar>() is ComponentRadar radar && radar.m_componentBlockEntity == null);
    }

    private void SyncIndustrialInventories(Entity entity) {
        foreach (IInventory inventory in entity.FindComponents<IInventory>()) {
            if (inventory.GetType().Assembly == typeof(IndustrialModLoader).Assembly || IsIndustrialMobile(entity)) {
                SyncInventory(inventory);
            }
        }
    }

    private void SyncInventory(IInventory inventory) {
        if (!m_inventoryStates.TryGetValue(inventory, out (int Value, int Count)[] slots) || slots.Length != inventory.SlotsCount) {
            slots = new (int, int)[inventory.SlotsCount];
            m_inventoryStates[inventory] = slots;
            CopySlots(inventory, slots);
            InventoryPacketHelpers.QueueInventorySync(inventory);
            return;
        }
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i] != (inventory.GetSlotValue(i), inventory.GetSlotCount(i))) {
                CopySlots(inventory, slots);
                InventoryPacketHelpers.QueueInventorySync(inventory);
                break;
            }
        }
    }

    private static void CopySlots(IInventory inventory, (int Value, int Count)[] slots) {
        for (int i = 0; i < slots.Length; i++) {
            slots[i] = (inventory.GetSlotValue(i), inventory.GetSlotCount(i));
        }
    }

    private static bool HasDeviceState(Entity entity) {
        foreach (Component component in entity.Components) {
            if (HasComponentState(component)) {
                return true;
            }
        }
        return false;
    }

    private static bool HasComponentState(Component component) {
        return component is ITank { TanksCount: > 0 and <= MaximumTanks } || IndustrialMachineState.HasState(component);
    }

    private static byte[] Capture(Point3 position, VoltElement element, Entity entity, bool byEntity = false) {
        return CompatNetPayload.Write(archive => {
            archive.Value(ref byEntity);
            archive.Value(ref position);
            bool hasVoltage = element != null;
            archive.Value(ref hasVoltage);
            if (hasVoltage) {
                int blockValue = Terrain.ReplaceLight(element.BlockValue, 0);
                int workState = (int)element.CurrentWorkState;
                element.CalcInputVoltageAndPower(out float voltage, out float power, out bool match);
                archive.Value(ref blockValue);
                archive.Value(ref workState);
                archive.Value(ref voltage);
                archive.Value(ref power);
                archive.Value(ref match);
            }

            int entityId = entity?.Id ?? 0;
            archive.Value(ref entityId);
            ushort components = 0;
            if (entity != null) {
                foreach (Component component in entity.Components) {
                    if (HasComponentState(component)) {
                        components++;
                    }
                }
            }
            archive.Value(ref components);
            if (components == 0) {
                return;
            }
            for (int index = 0; index < entity.Components.Count; index++) {
                Component component = entity.Components[index];
                if (!HasComponentState(component)) {
                    continue;
                }
                ITank tank = component as ITank;
                int componentIndex = index;
                byte count = tank?.TanksCount is > 0 and <= MaximumTanks ? (byte)tank.TanksCount : (byte)0;
                archive.Value(ref componentIndex);
                byte[] machineState = CompatNetPayload.Write(writer => IndustrialMachineState.Write(writer, component));
                archive.Buffer(ref machineState, MaximumRecordSize);
                archive.Value(ref count);
                for (int i = 0; i < count; i++) {
                    int value = tank.GetTankValue(i);
                    float volume = tank.GetTankVolume(i);
                    float capacity = tank.GetTankOriginalCapacity(i);
                    archive.Value(ref value);
                    archive.Value(ref volume);
                    archive.Value(ref capacity);
                }
            }
        });
    }

    private void QueueIfChanged<TKey>(Dictionary<TKey, byte[]> states, TKey key, byte[] state) {
        if (state.Length > MaximumRecordSize
            || states.TryGetValue(key, out byte[] previous) && state.AsSpan().SequenceEqual(previous)) {
            return;
        }
        if (m_pendingRecords.Count == MaximumRecords || m_pendingSize + state.Length + 8 > CompatNetPacket.MaximumPayloadBytes) {
            FlushStates();
        }
        states[key] = state;
        m_pendingRecords.Add(state);
        m_pendingSize += state.Length + 4;
    }

    private void FlushStates() {
        if (m_pendingRecords.Count == 0) {
            return;
        }
        byte[] batch = CompatNetPayload.Write(archive => {
            ushort count = (ushort)m_pendingRecords.Count;
            archive.Value(ref count);
            foreach (byte[] record in m_pendingRecords) {
                byte[] data = record;
                archive.Buffer(ref data, MaximumRecordSize);
            }
        });
        CompatNetCore.QueueBroadcast(AdapterId, StateSnapshot, batch);
        m_pendingRecords.Clear();
        m_pendingSize = 0;
    }

    public void HandleBroadcast(CompatNetPacket packet) {
        Project project = GameManager.Project;
        if (project == null || !IsRemote(project)) {
            return;
        }
        if (!ReferenceEquals(project, m_remoteProject)) {
            m_remoteProject = project;
            m_guestTechTree = new();
        }
        switch (packet.Operation) {
            case TechTreeState:
                m_guestTechTree.Receive(project.FindSubsystem<SubsystemTechTree>(), packet.Payload);
                return;
            case QuarryPose:
                CompatNetPayload.Read(packet.Payload, archive => {
                    int entityId = 0;
                    archive.Value(ref entityId);
                    project.FindEntity(entityId)?.FindComponent<ComponentQuarryDriller>()?.SerializeLanPose(archive);
                });
                return;
            case ScreenLinks:
                if (project.FindSubsystem<SCIENEW.Screens.SubsystemScreens>() is SCIENEW.Screens.SubsystemScreens screens) {
                    CompatNetPayload.Read(packet.Payload, screens.SerializeLanLinks);
                }
                return;
            case ProjectileState:
                Projectiles.Receive(project, packet.Payload);
                return;
            case RecipeResult:
                CompatNetPayload.Read(packet.Payload, archive => {
                    byte outcome = 0;
                    string missing = string.Empty;
                    archive.Value(ref outcome);
                    archive.Value(ref missing);
                    IndustrialRecipePlacement.ApplyFeedback(project.FindSubsystem<SubsystemPlayers>()?.MainPlayerData?.ComponentPlayer, outcome, missing);
                });
                return;
            case RadioChannel:
                CompatNetPayload.Read(packet.Payload, archive => {
                    Point3 point = default;
                    int value = 0;
                    int frequency = 0;
                    bool active = false;
                    archive.Value(ref point);
                    archive.Value(ref value);
                    archive.Value(ref frequency);
                    archive.Value(ref active);
                    if (project.FindSubsystem<SubsystemVoltNet>()?.GetVoltElement(point) is RadioVoltElement radio
                        && Terrain.ReplaceLight(radio.BlockValue, 0) == value && IsRadioFrequency(frequency)) {
                        RadioVoltElement.SetFrequency(radio.GetElectricElement(), frequency);
                        radio.SetRadioActive(active);
                    }
                });
                return;
            case StateSnapshot:
                break;
            default:
                return;
        }
        SubsystemBlockEntities blocks = project.FindSubsystem<SubsystemBlockEntities>();
        SubsystemVoltNet voltage = project.FindSubsystem<SubsystemVoltNet>();
        if (blocks == null || voltage == null) {
            return;
        }
        CompatNetPayload.Read(packet.Payload, archive => {
            ushort count = 0;
            archive.Value(ref count);
            if (count > MaximumRecords) {
                throw new InvalidDataException("Invalid industrial device record count.");
            }
            for (int i = 0; i < count; i++) {
                byte[] record = null;
                archive.Buffer(ref record, MaximumRecordSize);
                CompatNetPayload.Read(record, entry => Apply(entry, blocks, voltage));
            }
        });
    }

    private static void Apply(PacketSerializer archive, SubsystemBlockEntities blocks, SubsystemVoltNet voltageNet) {
        bool byEntity = false;
        Point3 point = default;
        bool hasVoltage = false;
        archive.Value(ref byEntity);
        archive.Value(ref point);
        archive.Value(ref hasVoltage);
        if (hasVoltage) {
            int blockValue = 0;
            int workState = 0;
            float voltage = 0;
            float power = 0;
            bool match = false;
            archive.Value(ref blockValue);
            archive.Value(ref workState);
            archive.Value(ref voltage);
            archive.Value(ref power);
            archive.Value(ref match);
            VoltElement element = voltageNet.GetVoltElement(point);
            if (element != null && Terrain.ReplaceLight(element.BlockValue, 0) == blockValue
                && workState is >= (int)WorkState.Broken and <= (int)WorkState.Overloaded
                && float.IsFinite(voltage) && float.IsFinite(power)) {
                element.ApplyLanState((WorkState)workState, voltage, power, match);
            }
        }

        int entityId = 0;
        ushort components = 0;
        archive.Value(ref entityId);
        archive.Value(ref components);
        Entity entity = byEntity ? blocks.Project.FindEntity(entityId) : blocks.GetBlockEntity(point)?.Entity;
        if (entity == null || entity.Id != entityId) {
            return;
        }
        for (int i = 0; i < components; i++) {
            int index = 0;
            byte count = 0;
            archive.Value(ref index);
            Component component = (uint)index < entity.Components.Count ? entity.Components[index] : null;
            byte[] machineState = [];
            archive.Buffer(ref machineState, MaximumRecordSize);
            if (component != null) {
                CompatNetPayload.Read(machineState, reader => IndustrialMachineState.Read(reader, component));
            }
            archive.Value(ref count);
            if (count > MaximumTanks) {
                throw new InvalidDataException("Invalid industrial tank count.");
            }
            ITank tank = component as ITank;
            for (int j = 0; j < count; j++) {
                int value = 0;
                float volume = 0;
                float capacity = 0;
                archive.Value(ref value);
                archive.Value(ref volume);
                archive.Value(ref capacity);
                if (tank?.TanksCount == count && volume >= 0 && float.IsFinite(volume) && capacity >= 0 && float.IsFinite(capacity)) {
                    ApplyTank(tank, j, value, volume, capacity);
                }
            }
        }
    }

    private static void ApplyTank(ITank tank, int index, int value, float volume, float capacity) {
        if (tank is ComponentFluidTank fluidTank) {
            fluidTank.ApplyLanState(value, volume, capacity);
            return;
        }

        float current = tank.GetTankVolume(index);
        if (tank.GetTankValue(index) != value) {
            tank.RemoveTankFluid(index, current);
            current = 0;
        }
        if (current > volume) {
            tank.RemoveTankFluid(index, current - volume);
        }
        else if (current < volume) {
            tank.AddTankFluid(index, value, volume - current, out _);
        }
    }

    public static bool QueueTankTransfer(ComponentPlayer player, int slot, DragMode mode, ITank tank, int index, int expectedValue) {
        if (tank is not Component component || !IsRemote(component.Project) || player == null) {
            return false;
        }
        int entityId = component.Entity.Id;
        int componentIndex = component.Entity.Components.IndexOf(component);
        bool single = mode == DragMode.SingleItem;
        byte[] payload = CompatNetPayload.Write(archive => {
            archive.Value(ref entityId);
            archive.Value(ref componentIndex);
            archive.Value(ref index);
            archive.Value(ref slot);
            archive.Value(ref single);
            archive.Value(ref expectedValue);
        });
        return CompatNetCore.QueueRequest(player, NetworkId, TankTransfer, payload);
    }

    public static void QueueMachineControl(ComponentPlayer player, Component component, IndustrialControl operation, float value) {
        int entityId = component.Entity.Id;
        int componentIndex = component.Entity.Components.IndexOf(component);
        byte control = (byte)operation;
        byte[] payload = CompatNetPayload.Write(archive => {
            archive.Value(ref entityId);
            archive.Value(ref componentIndex);
            archive.Value(ref control);
            archive.Value(ref value);
        });
        CompatNetCore.QueueRequest(player, NetworkId, MachineControl, payload);
    }

    public void HandleRequest(CompatNetPacket packet, PlayerData playerData) {
        ComponentPlayer player = playerData.ComponentPlayer;
        if (player == null) {
            return;
        }
        if (packet.Operation == MachineControl) {
            CompatNetPayload.Read(packet.Payload, archive => {
                int entityId = 0;
                int componentIndex = 0;
                byte operation = 0;
                float value = 0;
                archive.Value(ref entityId);
                archive.Value(ref componentIndex);
                archive.Value(ref operation);
                archive.Value(ref value);
                Component component = GetReachableComponent(player, entityId, componentIndex);
                IndustrialMachineControl.Apply(component, (IndustrialControl)operation, value);
            });
        }
        else if (packet.Operation == TargetingSettings) {
            CompatNetPayload.Read(packet.Payload, archive => {
                int entityId = 0;
                int componentIndex = 0;
                byte setting = 0;
                string value = string.Empty;
                archive.Value(ref entityId);
                archive.Value(ref componentIndex);
                archive.Value(ref setting);
                archive.Value(ref value);
                if (GetReachableComponent(player, entityId, componentIndex) is ComponentTurretTargeting targeting) {
                    targeting.ChangeSetting(setting, value);
                }
            });
        }
        else if (packet.Operation == RadioChannel) {
            CompatNetPayload.Read(packet.Payload, archive => {
                Point3 position = default;
                int frequency = 0;
                archive.Value(ref position);
                archive.Value(ref frequency);
                if (IsRadioFrequency(frequency) && Vector3.DistanceSquared(player.ComponentBody.Position, new Vector3(position) + new Vector3(0.5f)) <= 36
                    && player.Project.FindSubsystem<SubsystemVoltNet>()?.GetVoltElement(position) is RadioVoltElement radio) {
                    RadioVoltElement.SetFrequency(radio.GetElectricElement(), frequency);
                    PublishRadioChannel(position, radio);
                }
            });
        }
        else if (packet.Operation == ToolCharge) {
            HandleToolCharge(packet, player);
        }
        else if (packet.Operation == VehicleInput) {
            HandleVehicleInput(packet, player);
        }
        else if (packet.Operation == RecipePlacement) {
            HandleRecipePlacement(packet, player);
        }
        else if (packet.Operation == TankTransfer) {
            HandleTankTransfer(packet, player);
        }
    }

    private static bool IsRadioFrequency(int frequency) => frequency >= SubsystemRadioElectricElements.MinFrequency
        && frequency <= SubsystemRadioElectricElements.MaxFrequency;

    internal static void SetRadioChannel(ElectricElement radio, Point3 position, int frequency) {
        frequency = Math.Clamp(frequency, (int)SubsystemRadioElectricElements.MinFrequency, (int)SubsystemRadioElectricElements.MaxFrequency);
        Project project = radio.SubsystemElectricity.Project;
        if (!IsRemote(project)) {
            RadioVoltElement.SetFrequency(radio, frequency);
            return;
        }
        ComponentPlayer player = project.FindSubsystem<SubsystemPlayers>()?.MainPlayerData?.ComponentPlayer;
        CompatNetCore.QueueRequest(player, NetworkId, RadioChannel, CompatNetPayload.Write(archive => {
            archive.Value(ref position);
            archive.Value(ref frequency);
        }));
    }

    private void PublishRadioChannel(Point3 position, RadioVoltElement radio) {
        int value = Terrain.ReplaceLight(radio.BlockValue, 0);
        int frequency = radio.GetFrequency();
        bool active = radio.m_isActive;
        if (!IsRadioFrequency(frequency) || m_radioStates.TryGetValue(position, out var previous) && previous == (value, frequency, active)) return;
        m_radioStates[position] = (value, frequency, active);
        CompatNetCore.QueueBroadcast(AdapterId, RadioChannel, CompatNetPayload.Write(archive => {
            archive.Value(ref position);
            archive.Value(ref value);
            archive.Value(ref frequency);
            archive.Value(ref active);
        }));
    }

    internal static void QueueToolCharge(ComponentPlayer player, int slot, int expectedValue) {
        CompatNetCore.QueueRequest(player, NetworkId, ToolCharge, CompatNetPayload.Write(archive => {
            archive.Value(ref slot);
            archive.Value(ref expectedValue);
        }));
    }

    private static void HandleToolCharge(CompatNetPacket packet, ComponentPlayer player) {
        int slot = -1;
        int expectedValue = 0;
        CompatNetPayload.Read(packet.Payload, archive => {
            archive.Value(ref slot);
            archive.Value(ref expectedValue);
        });
        ComponentElectricToolCharging charger = player.Entity.FindComponent<ComponentElectricToolCharging>();
        IInventory inventory = player.ComponentMiner.Inventory;
        if (charger == null || inventory == null) return;
        if (slot == -1) {
            charger.DropBattery(player);
        }
        else if ((uint)slot < inventory.SlotsCount && inventory.GetSlotValue(slot) == expectedValue
            && charger.TryCharge(inventory, slot)) {
            InventoryPacketHelpers.QueueInventorySync(inventory, packet.From);
        }
        InventoryPacketHelpers.QueueInventorySync(charger, packet.From);
    }

    public static bool QueueRecipePlacement(ComponentPlayer player, Component component, IRecipe recipe,
        Component container, PlacementOptions options) {
        int entityId = component.Entity.Id;
        int componentIndex = component.Entity.Components.IndexOf(component);
        int containerId = container?.Entity.Id ?? 0;
        int containerIndex = container == null ? -1 : container.Entity.Components.IndexOf(container);
        int recipeIndex = RecipaediaEXManager.Recipes.IndexOf(recipe);
        byte[] fingerprint = IndustrialRecipePlacement.Fingerprint(recipe);
        byte flags = (byte)((options.FillEmptyOnly ? 1 : 0) | (options.MaxSets ? 2 : 0)
            | (options.AllowPartial ? 4 : 0) | (options.ClearGridBeforePlace ? 8 : 0));
        return CompatNetCore.QueueRequest(player, NetworkId, RecipePlacement, CompatNetPayload.Write(archive => {
            archive.Value(ref entityId);
            archive.Value(ref componentIndex);
            archive.Value(ref containerId);
            archive.Value(ref containerIndex);
            archive.Value(ref recipeIndex);
            archive.Buffer(ref fingerprint, 32);
            archive.Value(ref flags);
        }));
    }

    private void HandleRecipePlacement(CompatNetPacket packet, ComponentPlayer player) {
        int entityId = 0;
        int componentIndex = 0;
        int containerId = 0;
        int containerIndex = 0;
        int recipeIndex = 0;
        byte[] fingerprint = [];
        byte flags = 0;
        CompatNetPayload.Read(packet.Payload, archive => {
            archive.Value(ref entityId);
            archive.Value(ref componentIndex);
            archive.Value(ref containerId);
            archive.Value(ref containerIndex);
            archive.Value(ref recipeIndex);
            archive.Buffer(ref fingerprint, 32);
            archive.Value(ref flags);
        });

        Component component = GetReachableComponent(player, entityId, componentIndex);
        if (component == null && entityId == player.Entity.Id && (uint)componentIndex < player.Entity.Components.Count) {
            component = player.Entity.Components[componentIndex] as ComponentCraftingTable;
        }
        IInventory inventory = player.ComponentMiner.Inventory;
        IInventory container = containerId == 0 ? null : GetReachableComponent(player, containerId, containerIndex) as IInventory;
        IRecipePlacementTarget target = IndustrialRecipePlacement.ResolveTarget(component);
        byte outcome = 0;
        string missing = string.Empty;
        if (target != null && inventory != null && fingerprint.Length == 32) {
            if (ReferenceEquals(component, container)) {
                container = null;
            }
            List<IRecipe> recipes = RecipaediaEXManager.Recipes;
            IRecipe recipe = (uint)recipeIndex < recipes.Count && Matches(recipes[recipeIndex])
                ? recipes[recipeIndex]
                : recipes.FirstOrDefault(Matches);
            if (recipe != null) {
                PlacementResult result = target.TryPlaceRecipe(recipe,
                    new PlacementSources { PlayerInventory = inventory, ContainerInventory = container },
                    new PlacementOptions {
                        FillEmptyOnly = (flags & 1) != 0,
                        MaxSets = (flags & 2) != 0,
                        AllowPartial = (flags & 4) != 0,
                        ClearGridBeforePlace = (flags & 8) != 0
                    }, execute: true);
                outcome = result.Success ? (result.HadTransfers ? (byte)1 : (byte)2) : (byte)0;
                if (!result.Success) {
                    missing = string.Join("；", result.Missing);
                }
                InventoryPacketHelpers.QueueInventorySync(inventory, packet.From);
                InventoryPacketHelpers.QueueInventoryTransferSync(component as IInventory, container);
            }
        }
        CompatNetCore.QueueBroadcast(AdapterId, RecipeResult, CompatNetPayload.Write(archive => {
            archive.Value(ref outcome);
            archive.Value(ref missing);
        }), to: packet.From);

        bool Matches(IRecipe recipe) => target.CanAccept(recipe) && IndustrialRecipePlacement.Fingerprint(recipe).AsSpan().SequenceEqual(fingerprint);
    }

    internal static bool ChangeTargeting(ComponentTurretTargeting targeting, byte setting, string value) {
        if (!IsRemote(targeting.Project)) {
            return targeting.ChangeSetting(setting, value);
        }
        ComponentPlayer player = targeting.Project.FindSubsystem<SubsystemPlayers>()?.MainPlayerData?.ComponentPlayer;
        int entityId = targeting.Entity.Id;
        int index = targeting.Entity.Components.IndexOf(targeting);
        return player != null && CompatNetCore.QueueRequest(player, NetworkId, TargetingSettings, CompatNetPayload.Write(archive => {
            archive.Value(ref entityId);
            archive.Value(ref index);
            archive.Value(ref setting);
            archive.Value(ref value);
        }));
    }

    internal void QueueVehicleInput(ComponentPlayer player) {
        if (!player.PlayerData.IsMainPlayer || !IsRemote(player.Project) || Time.RealTime < m_nextVehicleInput) {
            return;
        }
        Entity mounted = player.ComponentRider.Mount?.Entity;
        if (!IsIndustrialMobile(mounted) || mounted.FindComponent<ComponentBoat>() == null) {
            return;
        }
        m_nextVehicleInput = Time.RealTime + 0.06;
        int id = mounted.Id;
        Vector3 move = player.ComponentInput.PlayerInput.Move;
        Vector2 axes = new(move.Z, move.X);
        CompatNetCore.QueueRequest(player, NetworkId, VehicleInput, CompatNetPayload.Write(archive => {
            archive.Value(ref id);
            archive.Value(ref axes);
        }));
    }

    private void HandleVehicleInput(CompatNetPacket packet, ComponentPlayer player) {
        int id = 0;
        Vector2 axes = default;
        CompatNetPayload.Read(packet.Payload, archive => {
            archive.Value(ref id);
            archive.Value(ref axes);
        });
        Entity mounted = player.ComponentRider.Mount?.Entity;
        if (mounted?.Id != id || !IsIndustrialMobile(mounted)
            || mounted.FindComponent<ComponentBoat>() is not ComponentBoat boat
            || !float.IsFinite(axes.X) || !float.IsFinite(axes.Y)) {
            return;
        }
        axes.X = Math.Clamp(axes.X, -1, 1);
        axes.Y = Math.Clamp(axes.Y, -1, 1);
        m_vehicleInput[player] = (boat, axes, Time.RealTime + 0.35);
    }

    internal void ApplyVehicleInput(ComponentBoat boat) {
        if (!NetworkManager.IsServerRunning || !IsIndustrialMobile(boat.Entity)
            || boat.Entity.FindComponent<ComponentMount>()?.Rider?.Entity.FindComponent<ComponentPlayer>() is not ComponentPlayer player
            || !m_vehicleInput.TryGetValue(player, out var input)) {
            return;
        }
        if (!ReferenceEquals(input.Boat, boat) || Time.RealTime >= input.Expires) {
            m_vehicleInput.Remove(player);
            return;
        }
        boat.MoveOrder = input.Axes.X;
        boat.TurnOrder = input.Axes.Y;
    }

    private static void HandleTankTransfer(CompatNetPacket packet, ComponentPlayer player) {
        int entityId = 0;
        int componentIndex = 0;
        int index = 0;
        int slot = 0;
        int expectedValue = 0;
        bool single = false;
        CompatNetPayload.Read(packet.Payload, archive => {
            archive.Value(ref entityId);
            archive.Value(ref componentIndex);
            archive.Value(ref index);
            archive.Value(ref slot);
            archive.Value(ref single);
            archive.Value(ref expectedValue);
        });
        Component component = GetReachableComponent(player, entityId, componentIndex);
        IInventory inventory = player.ComponentMiner.Inventory;
        if (component is not ITank tank || (uint)index >= tank.TanksCount
            || !InventoryPacketHelpers.IsSlotIndexValid(inventory, slot)
            || inventory.GetSlotValue(slot) != expectedValue || inventory.GetSlotCount(slot) == 0) {
            return;
        }
        TankWidget.TransferFluid(inventory, slot, single, tank, index);
        InventoryPacketHelpers.QueueInventorySync(inventory, packet.From);
    }

    private static Component GetReachableComponent(ComponentPlayer player, int entityId, int componentIndex) {
        Entity entity = player.Project.FindEntity(entityId);
        ComponentBlockEntity block = entity?.FindComponent<ComponentBlockEntity>();
        if (entity == null || (uint)componentIndex >= entity.Components.Count) {
            return null;
        }
        if (block != null) {
            Vector3 center = new Vector3(block.Coordinates) + new Vector3(0.5f);
            return Vector3.DistanceSquared(player.ComponentBody.Position, center) <= 36 ? entity.Components[componentIndex] : null;
        }
        Component component = entity.Components[componentIndex];
        if (entity.FindComponent<ComponentMount>()?.Rider?.Entity == player.Entity && IsIndustrialMobile(entity)) {
            return component;
        }
        return component is ComponentRadar radar && radar.m_componentBody?.ParentBody == player.ComponentBody ? component : null;
    }
}
