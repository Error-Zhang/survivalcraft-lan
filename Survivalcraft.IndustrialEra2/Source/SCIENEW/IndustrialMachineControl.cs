using System;
using Game;
using Game.Network;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

internal enum IndustrialControl : byte {
    Running,
    Voltage,
    Power,
    InputStep,
    OutputStep,
    SelectArea,
    ConfirmArea,
    OperatingMode,
    RadarRadius,
    Yaw,
    Pitch,
    ElectricalControl,
    FluidPort,
    ControllerAction,
    Dispense
}

internal static class IndustrialMachineControl {
    public static void Change(Component component, IndustrialControl control, float value) {
        if (IndustrialEraLanAdapter.IsRemote(component.Project)) {
            ComponentPlayer player = component.Project.FindSubsystem<SubsystemPlayers>()?.MainPlayerData?.ComponentPlayer;
            if (player != null) {
                IndustrialEraLanAdapter.QueueMachineControl(player, component, control, value);
            }
            return;
        }
        Apply(component, control, value);
    }

    public static void UpdateSlider(Component component, SliderWidget slider, ref bool wasSliding, float value, IndustrialControl control) {
        bool sliding = slider.IsSliding;
        bool remote = IndustrialEraLanAdapter.IsRemote(component.Project);
        if (remote ? wasSliding && !sliding : sliding) {
            if (slider.Value != value) {
                Change(component, control, slider.Value);
            }
        }
        else if (!sliding) {
            slider.Value = value;
        }
        wasSliding = sliding;
    }

    public static void Apply(Component component, IndustrialControl control, float value) {
        if (!float.IsFinite(value)) {
            return;
        }
        switch (control) {
            case IndustrialControl.RadarRadius when component is ComponentRadar radar:
                radar.m_radius = (int)Math.Clamp(value, 16f, 64f);
                break;
            case IndustrialControl.Yaw:
            case IndustrialControl.Pitch:
                SetAim(component, control == IndustrialControl.Yaw, value);
                break;
            case IndustrialControl.ElectricalControl when value is 0 or 1:
                SetElectricalControl(component, value == 1);
                break;
            case IndustrialControl.FluidPort when value is 0 or 1:
                if (component is ComponentBlastFurnace blast && value == 0) {
                    blast.SwitchPortState();
                }
                else if (component is ComponentHearthFurnace hearth) {
                    hearth.SwitchPortState(input: value == 0, output: value != 0);
                }
                break;
            case IndustrialControl.ControllerAction when component is ComponentGReactorController controller && value is >= 0 and <= 6:
                SetControllerAction(controller, (int)value);
                break;
            case IndustrialControl.Dispense when component is ComponentAF_BoxGenerator box:
                box.Gacha();
                break;
            case IndustrialControl.SelectArea when component is IIndustrialQuarry { IsPreparing: true } quarry:
                quarry.SelectArea();
                break;
            case IndustrialControl.ConfirmArea when component is IIndustrialQuarry { IsPreparing: true } quarry:
                quarry.IsPreparing = false;
                break;
            case IndustrialControl.OperatingMode when value is 0 or 1:
                SetOperatingMode(component, value == 1);
                break;
            case IndustrialControl.Running when value is 0 or 1:
                SetRunning(component, value == 1);
                break;
            case IndustrialControl.Voltage when component is ComponentCreativeGenerator generator:
                generator.m_voltage = Math.Clamp(value, 0, SubsystemVoltNet.MaxVoltage);
                break;
            case IndustrialControl.Power when component is ComponentCreativeGenerator generator:
                generator.m_power = Math.Clamp(value, 0, SubsystemVoltNet.MaxPower);
                break;
            case IndustrialControl.InputStep when value is 1 or -1:
                MoveSlot(component, input: true, (int)value);
                break;
            case IndustrialControl.OutputStep when value is 1 or -1:
                MoveSlot(component, input: false, (int)value);
                break;
        }
    }

    private static void SetAim(Component component, bool yaw, float value) {
        switch (component) {
            case ComponentCamera camera:
                if (yaw) {
                    camera.Yaw = (int)Math.Clamp(value, -180, 180);
                }
                else {
                    camera.Pitch = (int)Math.Clamp(value, -60, 60);
                }
                break;
            case ComponentLens lens:
                (int lensYaw, int lensPitch) = lens.GetYawPitch();
                lens.SetYawPitch(yaw ? Math.Clamp(value, -180, 180) : lensYaw, yaw ? lensPitch : Math.Clamp(value, -90, 90));
                break;
            case ComponentMirror mirror:
                (int mirrorYaw, int mirrorPitch) = mirror.GetYawPitch();
                mirror.SetYawPitch(yaw ? Math.Clamp(value, -180, 180) : mirrorYaw, yaw ? mirrorPitch : Math.Clamp(value, -90, 90));
                break;
        }
    }

    private static void SetElectricalControl(Component component, bool enabled) {
        switch (component) {
            case ComponentCharger charger:
                charger.ElectricalControl = enabled;
                break;
            case ComponentCondenser condenser:
                condenser.ElectricalControl = enabled;
                break;
        }
    }

    private static void SetControllerAction(ComponentGReactorController controller, int action) {
        switch (action) {
            case 0 when controller.m_sorterUFuelRodCount > 0: controller.fr1 = true; break;
            case 1 when controller.m_coreUFuelRodCount > 0: controller.fr0 = true; break;
            case 2 when controller.m_sorterControlRodCount > 0: controller.cr1 = true; break;
            case 3 when controller.m_coreControlRodCount > 0: controller.cr0 = true; break;
            case 4 when controller.m_sorterCarbonRodCount > 0: controller.gr1 = true; break;
            case 5 when controller.m_coreCarbonRodCount > 0: controller.gr0 = true; break;
            case 6 when controller.m_sorterControlRodCount > 0: controller.az5 = true; break;
        }
    }

    private static void SetOperatingMode(Component component, bool alternate) {
        switch (component) {
            case IIndustrialQuarry quarry:
                quarry.SetDisposalMode(alternate);
                break;
            case ComponentDriller drill:
                drill.Mode = alternate ? DrillerDevice.Mode.Dispose : DrillerDevice.Mode.Drill;
                break;
            case ComponentCreativeChest chest:
                chest.m_mode = alternate ? CreativeChestDevice.Mode.Dispense : CreativeChestDevice.Mode.Shoot;
                break;
            case ComponentCharger charger:
                charger.Mode = alternate ? ComponentCondenser.Mode.Discharge : ComponentCondenser.Mode.Charge;
                break;
        }
    }

    private static void SetRunning(Component component, bool running) {
        switch (component) {
            case ComponentCombustionEngine engine:
                engine.start = running;
                break;
            case ComponentDieselEngine engine:
                engine.start = running;
                break;
            case ComponentGasTurbine turbine:
                turbine.start = running;
                break;
            case ComponentHydrogenGenerator generator:
                generator.m_start = running;
                break;
            case ComponentSteamEngineV engine:
                engine.start = running;
                break;
            case ComponentCreativeEngine engine:
                engine.m_isPowering = running;
                break;
            case ComponentNuclearEngine engine:
                engine.Ignited = running;
                break;
        }
    }

    private static void MoveSlot(Component component, bool input, int delta) {
        if (component is ComponentInserter inserter) {
            ref int index = ref (input ? ref inserter.m_inNum : ref inserter.m_outNum);
            index = (int)Math.Clamp((long)index + delta, 0, int.MaxValue);
        }
        else if (component is ComponentFluidInserter fluidInserter) {
            ref int index = ref (input ? ref fluidInserter.m_inIndex : ref fluidInserter.m_outIndex);
            index = (int)Math.Clamp((long)index + delta, 0, int.MaxValue);
        }
    }
}
