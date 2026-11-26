using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Engine;
using Game;
using RecipaediaEX.Events;
using SCIENEW.Events;

namespace SCIENEW;

public static class DevicesBlockManager
{
	public static DeviceBlock[] Devices = Array.Empty<DeviceBlock>();

	public static Dictionary<string, int[]> IdMap;

	public static void Init()
	{
		int num = 155;
		List<DeviceBlock> list = new List<DeviceBlock>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<DeviceBlock> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new PresserDevice();
		num2++;
		span[num2] = new KibblerDevice();
		num2++;
		span[num2] = new RiflingDevice();
		num2++;
		span[num2] = new SqueezerDevice();
		num2++;
		span[num2] = new MachineToolDevice();
		num2++;
		span[num2] = new IronFurnaceDevice();
		num2++;
		span[num2] = new LiquidPumpDevice();
		num2++;
		span[num2] = new FluidTankDevice();
		num2++;
		span[num2] = new SteamEngineDevice();
		num2++;
		span[num2] = new StirlingEngineDevice();
		num2++;
		span[num2] = new IronChestDevice();
		num2++;
		span[num2] = new DrillerDevice();
		num2++;
		span[num2] = new CrusherDevice();
		num2++;
		span[num2] = new DiversionDevice();
		num2++;
		span[num2] = new SteelChestDevice();
		num2++;
		span[num2] = new SpinnerDevice();
		num2++;
		span[num2] = new CupronickelPipeDevice();
		num2++;
		span[num2] = new SifterDevice();
		num2++;
		span[num2] = new CheckValveDevice();
		num2++;
		span[num2] = new UnloaderDevice();
		num2++;
		span[num2] = new InserterDevice();
		num2++;
		span[num2] = new FireBoxDevice();
		num2++;
		span[num2] = new AirBlowerDevice();
		num2++;
		span[num2] = new BlastFurnaceDevice();
		num2++;
		span[num2] = new BlastFurnacePortDevice();
		num2++;
		span[num2] = new BlastCastingBasinDevice();
		num2++;
		span[num2] = new SorterDevice();
		num2++;
		span[num2] = new CokeOvenDevice();
		num2++;
		span[num2] = new TransferDevice();
		num2++;
		span[num2] = new CokeOvenPortDevice();
		num2++;
		span[num2] = new CastingTroughDevice();
		num2++;
		span[num2] = new HearthFurnaceInputPortDevice();
		num2++;
		span[num2] = new HearthFurnaceOutputPortDevice();
		num2++;
		span[num2] = new OpenHearthFurnaceDevice();
		num2++;
		span[num2] = new HearthFurnaceCastingBasinDevice();
		num2++;
		span[num2] = new SteamTurbineDevice();
		num2++;
		span[num2] = new GeneratorDevice();
		num2++;
		span[num2] = new MagnetizerDevice();
		num2++;
		span[num2] = new SeparatorDevice();
		num2++;
		span[num2] = new ElectricFurnaceDevice();
		num2++;
		span[num2] = new ReactorDevice();
		num2++;
		span[num2] = new CopperWireDevice();
		num2++;
		span[num2] = new ThermalGeneratorDevice();
		num2++;
		span[num2] = new CanpackDevice();
		num2++;
		span[num2] = new FermenterDevice();
		num2++;
		span[num2] = new GasTurbineDevice();
		num2++;
		span[num2] = new GalvanicCellDevice();
		num2++;
		span[num2] = new GasFurnaceDevice();
		num2++;
		span[num2] = new CondenserDevice();
		num2++;
		span[num2] = new FluidInserterDevice();
		num2++;
		span[num2] = new ElectrolyzerDevice();
		num2++;
		span[num2] = new MotorDevice();
		num2++;
		span[num2] = new ChargerDevice();
		num2++;
		span[num2] = new InductionFurnaceDevice();
		num2++;
		span[num2] = new SilverWireDevice();
		num2++;
		span[num2] = new GoldWireDevice();
		num2++;
		span[num2] = new AluminumWireDevice();
		num2++;
		span[num2] = new AlloyFurnaceDevice();
		num2++;
		span[num2] = new QuarryDevice();
		num2++;
		span[num2] = new BeaconDevice();
		num2++;
		span[num2] = new UnpackerDevice();
		num2++;
		span[num2] = new FractionalTowerDevice();
		num2++;
		span[num2] = new FractionalPortDevice();
		num2++;
		span[num2] = new OilPlantDevice();
		num2++;
		span[num2] = new ConcreteFrameDevice();
		num2++;
		span[num2] = new ConcreteFrameWithLevelDevice("", "ConcretePowder", Color.Gray);
		num2++;
		span[num2] = new ConcreteFrameWithLevelDevice("HighStrength", "HighStrengthConcretePowder", Color.Gray * new Color(255, 255, 210));
		num2++;
		span[num2] = new ConcreteFrameWithLevelDevice("Prestressed", "HighStrengthConcretePowder", Color.Gray * new Color(255, 255, 210));
		num2++;
		span[num2] = new CombustionEngineDevice();
		num2++;
		span[num2] = new GasCompressorDevice();
		num2++;
		span[num2] = new GasDecompressorDevice();
		num2++;
		span[num2] = new FillerDevice();
		num2++;
		span[num2] = new OpenerDevice();
		num2++;
		span[num2] = new VacuumFurnaceDevice();
		num2++;
		span[num2] = new AirPumpDevice();
		num2++;
		span[num2] = new SolarPanelBaseDevice();
		num2++;
		span[num2] = new PolySiliconSolarPanelDevice();
		num2++;
		span[num2] = new SiliconSolarPanelDevice();
		num2++;
		span[num2] = new CreativeEngineDevice();
		num2++;
		span[num2] = new CreativeGeneratorDevice();
		num2++;
		span[num2] = new WindTurbineDevice();
		num2++;
		span[num2] = new SteelFrameDevice(Materials.Plastic, 0f, 0f);
		num2++;
		span[num2] = new SteelFrameDevice(Materials.Copper, IEConstants.VoltNet.CopperWire.UL, IEConstants.VoltNet.CopperWire.PL, "CopperWireDevice");
		num2++;
		span[num2] = new SteelFrameDevice(Materials.Silver, IEConstants.VoltNet.SilverWire.UL, IEConstants.VoltNet.SilverWire.PL, "SilverWireDevice");
		num2++;
		span[num2] = new SteelFrameDevice(Materials.Gold, IEConstants.VoltNet.GoldWire.UL, IEConstants.VoltNet.GoldWire.PL, "GoldWireDevice");
		num2++;
		span[num2] = new SteelFrameDevice(Materials.Aluminum, IEConstants.VoltNet.AluminumWire.UL, IEConstants.VoltNet.AluminumWire.PL, "AluminumWireDevice");
		num2++;
		span[num2] = new ElectricPreheaterDevice();
		num2++;
		span[num2] = new AdvancedCrusherDevice();
		num2++;
		span[num2] = new EmptySpaceDevice();
		num2++;
		span[num2] = new MercuryPresserDevice();
		num2++;
		span[num2] = new PetroleumPumpDevice();
		num2++;
		span[num2] = new RadioTransmitterDevice();
		num2++;
		span[num2] = new RadioReceiverDevice();
		num2++;
		span[num2] = new HydrogenGeneratorDevice();
		num2++;
		span[num2] = new CastingCheckValveDevice();
		num2++;
		span[num2] = new CreativeFluidTankDevice();
		num2++;
		span[num2] = new CRTMonitorDevice();
		num2++;
		span[num2] = new RadarDevice();
		num2++;
		span[num2] = new OilTankDevice();
		num2++;
		span[num2] = new SpringPlateDevice();
		num2++;
		span[num2] = new ElectricAirBlowerDevice();
		num2++;
		span[num2] = new CameraDevice();
		num2++;
		span[num2] = new WireCubeDevice();
		num2++;
		span[num2] = new ElectricBlastFurnaceDevice();
		num2++;
		span[num2] = new WorkshopDevice();
		num2++;
		span[num2] = new SuperCondenserDevice();
		num2++;
		span[num2] = new StepUpTransformerDevice();
		num2++;
		span[num2] = new StepDownTransformerDevice();
		num2++;
		span[num2] = new CentrifugeDevice();
		num2++;
		span[num2] = new UThickenerDevice();
		num2++;
		span[num2] = new AF_BoxGeneratorDevice();
		num2++;
		span[num2] = new AF_BoxBoobDevice();
		num2++;
		span[num2] = new GReactorCoreDevice();
		num2++;
		span[num2] = new HeatExchangerDevice();
		num2++;
		span[num2] = new ReactorTurbineDevice();
		num2++;
		span[num2] = new GReactorTopDevice();
		num2++;
		span[num2] = new GReactorControllerDevice();
		num2++;
		span[num2] = new WireComposerDevice();
		num2++;
		span[num2] = new ACSRDevice();
		num2++;
		span[num2] = new NuclearEngineDevice();
		num2++;
		span[num2] = new FamiconDevice();
		num2++;
		span[num2] = new SteelFrameDevice(Materials.Steel, IEConstants.VoltNet.SteelWire.UL, IEConstants.VoltNet.SteelWire.PL, "ACSRDevice");
		num2++;
		span[num2] = new TEDCDevice();
		num2++;
		span[num2] = new DieselEngineDevice();
		num2++;
		span[num2] = new LaserEmitterDevice();
		num2++;
		span[num2] = new MirrorDevice();
		num2++;
		span[num2] = new LensDevice();
		num2++;
		span[num2] = new LaserReceiverDevice();
		num2++;
		span[num2] = new BlastDrillDevice();
		num2++;
		span[num2] = new AutoGunDevice();
		num2++;
		span[num2] = new AutoLaserDevice();
		num2++;
		span[num2] = new LaserQuarryDevice();
		num2++;
		span[num2] = new CondenserADevice();
		num2++;
		span[num2] = new CupronickelPipeDevice(0);
		num2++;
		span[num2] = new CupronickelPipeDevice(1);
		num2++;
		span[num2] = new CupronickelPipeDevice(2);
		num2++;
		span[num2] = new CupronickelPipeDevice(3);
		num2++;
		span[num2] = new CupronickelPipeDevice(4);
		num2++;
		span[num2] = new CupronickelPipeDevice(5);
		num2++;
		span[num2] = new CupronickelPipeDevice(6);
		num2++;
		span[num2] = new CupronickelPipeDevice(7);
		num2++;
		span[num2] = new CupronickelPipeDevice(8);
		num2++;
		span[num2] = new CupronickelPipeDevice(9);
		num2++;
		span[num2] = new CupronickelPipeDevice(10);
		num2++;
		span[num2] = new CupronickelPipeDevice(11);
		num2++;
		span[num2] = new CupronickelPipeDevice(12);
		num2++;
		span[num2] = new CupronickelPipeDevice(13);
		num2++;
		span[num2] = new CupronickelPipeDevice(14);
		num2++;
		span[num2] = new CupronickelPipeDevice(15);
		num2++;
		span[num2] = new CreativeChestDevice();
		num2++;
		span[num2] = new CondenserInputPortDevice();
		num2++;
		span[num2] = new CondenserOutputPortDevice();
		num2++;
		span[num2] = new AmmeterDevice();
		num2++;
		span[num2] = new VoltmeterDevice();
		num2++;
		span[num2] = new LEDScreenDevice();
		List<DeviceBlock> list2 = list;
		RecipaediaEventBus.GetPublisher<DeviceCatalogBuildingEvent>().Publish(new DeviceCatalogBuildingEvent(list2));
		Devices = list2.ToArray();
		IdMap = new Dictionary<string, int[]>();
		for (int i = 0; i < Devices.Length; i++)
		{
			int num3 = i << 3;
			int[] array = new int[8];
			for (int j = 0; j < 8; j++)
			{
				array[j] = Terrain.MakeBlockValue(BaseDeviceBlock.Index, 0, num3 | (j & 7));
			}
			IdMap.Add(Devices[i].GetCraftingId(), array);
		}
		RecipaediaEventBus.GetPublisher<IndustrialDevicesCatalogReadyEvent>().Publish(new IndustrialDevicesCatalogReadyEvent(Devices.Length));
	}

	public static DeviceBlock GetDevice(int value)
	{
		int num = Terrain.ExtractData(value) >> 3;
		if (num >= Devices.Length)
		{
			return Devices[0];
		}
		return Devices[num];
	}

	public static DeviceBlock GetDevice(string id)
	{
		return GetDevice(IdMap[id][0]);
	}

	public static T? GetDevice<T>() where T : DeviceBlock
	{
		DeviceBlock[] devices = Devices;
		foreach (DeviceBlock deviceBlock in devices)
		{
			if (deviceBlock.GetType() == typeof(T))
			{
				return deviceBlock as T;
			}
		}
		return null;
	}

	public static T[] GetDevices<T>() where T : DeviceBlock
	{
		return Devices.OfType<T>().ToArray();
	}

	public static int GetDeviceValue(string id, int innerData)
	{
		return IdMap[id][innerData];
	}

	public static string GetDeviceCraftingID(int value)
	{
		return GetDevice(value).GetCraftingId();
	}

	public static int GetDeviceValueByType(Type deviceType, int innerData = 0)
	{
		if (!deviceType.IsSubclassOf(typeof(DeviceBlock)))
		{
			return 0;
		}
		return GetDeviceValue(Devices.First((DeviceBlock x) => x.GetType() == deviceType).GetCraftingId(), innerData);
	}

	public static int GetDeviceValueByType<T>(int innerData = 0) where T : DeviceBlock
	{
		return GetDeviceValueByType(typeof(T), innerData);
	}
}
