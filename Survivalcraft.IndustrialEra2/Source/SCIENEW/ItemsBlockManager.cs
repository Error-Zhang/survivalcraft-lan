using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Extensions.Graphics;

namespace SCIENEW;

public static class ItemsBlockManager
{
	public static ItemBlock[] Items;

	public static Dictionary<string, int> IdMap;

	public static void Init()
	{
		try
		{
			Items = new ItemBlock[390]
			{
				new OrePowderItem(Materials.Iron),
				new OrePowderItem(Materials.Copper),
				new OrePowderItem(Materials.Germanium),
				new OrePowderItem(Materials.Gold),
				new OrePowderItem(Materials.Silver),
				new OrePowderItem(Materials.Platinum),
				new OrePowderItem(Materials.Lead),
				new OrePowderItem(Materials.Stannary),
				new OrePowderItem(Materials.Zinc),
				new OrePowderItem(Materials.Chromium),
				new OrePowderItem(Materials.Nickel),
				new OrePowderItem(Materials.Aluminum, createRecipe: false),
				new OrePowderItem(Materials.Titanium),
				new OrePowderItem(Materials.Uranium),
				new OreChunkItem(Matrix.CreateRotationX(4f) * Matrix.CreateRotationZ(2f), Matrix.CreateTranslation(0f, 0.125f, 0f), Color.White, smooth: false, Materials.Gold),
				new OreChunkItem(Matrix.CreateRotationX(5f) * Matrix.CreateRotationZ(2f), Matrix.CreateTranslation(0.0625f, 0.125f, 0f), Color.White, smooth: false, Materials.Silver),
				new OreChunkItem(Matrix.CreateRotationX(6f) * Matrix.CreateRotationZ(2f), Matrix.CreateTranslation(0.125f, 0.125f, 0f), Color.White, smooth: false, Materials.Platinum),
				new OreChunkItem(Matrix.CreateRotationX(1f) * Matrix.CreateRotationZ(3f), Matrix.CreateTranslation(0.1875f, 0.125f, 0f), Color.LightGray, smooth: false, Materials.Lead),
				new OreChunkItem(Matrix.CreateRotationX(1f) * Matrix.CreateRotationZ(4f), Matrix.CreateTranslation(0.25f, 0.125f, 0f), new Color(65, 224, 205), smooth: false, Materials.Zinc),
				new OreChunkItem(Matrix.CreateRotationX(1f) * Matrix.CreateRotationZ(5f), Matrix.CreateTranslation(0.3125f, 0.125f, 0f), new Color(225, 225, 225), smooth: false, Materials.Stannary),
				new OreChunkItem(Matrix.CreateRotationX(1f) * Matrix.CreateRotationZ(6f), Matrix.CreateTranslation(0.375f, 0.125f, 0f), Color.White, smooth: false, Materials.Mercury),
				new OreChunkItem(Matrix.CreateRotationX(1f) * Matrix.CreateRotationZ(7f), Matrix.CreateTranslation(0.4375f, 0.125f, 0f), Color.White, smooth: false, Materials.Chromium),
				new OreChunkItem(Matrix.CreateRotationX(2f) * Matrix.CreateRotationZ(-1f), Matrix.CreateTranslation(0.5f, 0.125f, 0f), new Color(190, 190, 190), smooth: false, Materials.Titanium),
				new OreChunkItem(Matrix.CreateRotationX(3f) * Matrix.CreateRotationZ(-1f), Matrix.CreateTranslation(0.5625f, 0.125f, 0f), Color.White, smooth: false, Materials.Nickel),
				new OreChunkItem(Matrix.CreateRotationX(3f) * Matrix.CreateRotationZ(-2f), Matrix.CreateTranslation(0.625f, 0.125f, 0f), Color.White, smooth: false, Materials.Uranium),
				new OreChunkItem(Matrix.CreateRotationX(3f) * Matrix.CreateRotationZ(-3f), Matrix.CreateTranslation(0.6875f, 0.125f, 0f), Color.White, smooth: true, Materials.Phosphorus),
				new ScrapIronItem(),
				new MetalIngotItem(Materials.Steel),
				new MetalIngotItem(Materials.Gold, createOreRecipe: true),
				new MetalIngotItem(Materials.Silver, createOreRecipe: true),
				new MetalIngotItem(Materials.Platinum, createOreRecipe: true),
				new MetalIngotItem(Materials.Lead, createOreRecipe: true),
				new MetalIngotItem(Materials.Zinc, createOreRecipe: true),
				new MetalIngotItem(Materials.Stannary, createOreRecipe: true),
				new MetalIngotItem(Materials.Chromium),
				new MetalIngotItem(Materials.Titanium),
				new MetalIngotItem(Materials.Nickel, createOreRecipe: true),
				new MetalIngotItem(Materials.Aluminum),
				new MetalIngotItem(Materials.Germanium, createOreRecipe: true),
				new MetalIngotItem(Materials.FeAlCrAlloy),
				new MetalIngotItem(Materials.Brass),
				new MetalIngotItem(Materials.Vanadium),
				new MetalIngotItem(Materials.Cupronickel),
				new PlateItem(Materials.Steel),
				new PlateItem(Materials.Gold),
				new PlateItem(Materials.Silver),
				new PlateItem(Materials.Lead),
				new PlateItem(Materials.Platinum),
				new PlateItem(Materials.Zinc),
				new PlateItem(Materials.Stannary),
				new PlateItem(Materials.Chromium),
				new PlateItem(Materials.Titanium),
				new PlateItem(Materials.Nickel),
				new PlateItem(Materials.Aluminum),
				new PlateItem(Materials.Iron),
				new PlateItem(Materials.Copper),
				new PlateItem(Materials.Germanium),
				new PlateItem(Materials.FeAlCrAlloy),
				new PlateItem(Materials.Brass),
				new PlateItem(Materials.Vanadium),
				new PlateItem(Materials.Cupronickel),
				new MetalLineItem(Materials.Steel),
				new MetalLineItem(Materials.Gold),
				new MetalLineItem(Materials.Silver),
				new MetalLineItem(Materials.Lead),
				new MetalLineItem(Materials.Platinum),
				new MetalLineItem(Materials.Zinc),
				new MetalLineItem(Materials.Stannary),
				new MetalLineItem(Materials.Chromium),
				new MetalLineItem(Materials.Titanium),
				new MetalLineItem(Materials.Nickel),
				new MetalLineItem(Materials.Aluminum),
				new MetalLineItem(Materials.Iron),
				new MetalLineItem(Materials.Copper),
				new MetalLineItem(Materials.Germanium),
				new MetalLineItem(Materials.FeAlCrAlloy),
				new MetalLineItem(Materials.Brass),
				new MetalLineItem(Materials.Vanadium),
				new MetalLineItem(Materials.Cupronickel),
				new RodItem("RifleBarrel", Color.Gray),
				new PowderItem(Materials.Steel),
				new PowderItem(Materials.Gold),
				new PowderItem(Materials.Silver),
				new PowderItem(Materials.Lead),
				new PowderItem(Materials.Platinum),
				new PowderItem(Materials.Zinc),
				new PowderItem(Materials.Stannary),
				new PowderItem(Materials.Chromium),
				new PowderItem(Materials.Titanium),
				new PowderItem(Materials.Nickel),
				new PowderItem(Materials.Aluminum),
				new PowderItem(Materials.Iron),
				new PowderItem(Materials.Copper),
				new PowderItem(Materials.Germanium),
				new PowderItem(Materials.FeAlCrAlloy),
				new PowderItem(Materials.Brass),
				new PowderItem(Materials.Vanadium),
				new PowderItem(Materials.Cupronickel),
				new CoalPowderItem("CoalPowder", new Color(28, 28, 28), 2f, 60f),
				new CoalPowderItem("CokeCoalPowder", Color.DarkGray, 3f, 100f),
				new PlateItem(Materials.Steel, small: true),
				new PlateItem(Materials.Gold, small: true),
				new PlateItem(Materials.Silver, small: true),
				new PlateItem(Materials.Lead, small: true),
				new PlateItem(Materials.Platinum, small: true),
				new PlateItem(Materials.Zinc, small: true),
				new PlateItem(Materials.Stannary, small: true),
				new PlateItem(Materials.Chromium, small: true),
				new PlateItem(Materials.Titanium, small: true),
				new PlateItem(Materials.Nickel, small: true),
				new PlateItem(Materials.Aluminum, small: true),
				new PlateItem(Materials.Iron, small: true),
				new PlateItem(Materials.Copper, small: true),
				new PlateItem(Materials.Germanium, small: true),
				new PlateItem(Materials.FeAlCrAlloy, small: true),
				new PlateItem(Materials.Brass, small: true),
				new PlateItem(Materials.Vanadium, small: true),
				new PlateItem(Materials.Cupronickel, small: true),
				new RodItem(Materials.Steel),
				new RodItem(Materials.Gold),
				new RodItem(Materials.Silver),
				new RodItem(Materials.Lead),
				new RodItem(Materials.Platinum),
				new RodItem(Materials.Zinc),
				new RodItem(Materials.Stannary),
				new RodItem(Materials.Chromium),
				new RodItem(Materials.Titanium),
				new RodItem(Materials.Nickel),
				new RodItem(Materials.Aluminum),
				new RodItem(Materials.Copper),
				new RodItem(Materials.Germanium),
				new RodItem(Materials.FeAlCrAlloy),
				new RodItem(Materials.Brass),
				new RodItem(Materials.Vanadium),
				new RodItem(Materials.Cupronickel),
				new SteelGearItem(),
				new SteelWheelItem(),
				new MeshItem("IndustrialPiston", "Models/Piston", "Piston", ContentManager.Get<Texture2D>("Textures/Iron"), Materials.Steel.GetColor(), Matrix.CreateRotationZ(-(float)Math.PI / 4f) * Matrix.CreateRotationX(MathUtils.DegToRad(90f)) * Matrix.CreateTranslation(0.6f, 1f, 0.5f), Matrix.Identity, 1.5f),
				new WireItem("CopperWire"),
				new MeshItem("IndustrialMagnet", "Models/UIronSheet", "UIronSheet", ContentManager.Get<Texture2D>("Textures/Iron"), Materials.Lead.GetColor(), Matrix.CreateScale(1.5f) * Matrix.CreateTranslation(0f, 0.5f, 0f), Matrix.Identity),
				new BrickItem("RefractoryBrick", Color.White, Matrix.CreateTranslation(-0.125f, -0.25f, 0f), IndustrialModLoader.IETexture),
				new CokeCoalItem(),
				new FanItem(Materials.Steel),
				new FanItem(Materials.Aluminum),
				new SteamBoatItem(),
				new TrainItem(),
				new CarriageItem(),
				new CoalPowderItem("NaCLPowder", Color.White, 0f, 0f),
				new IEFlatItem("SifterMeshItem", 218),
				new BulletItem("FixedBullet", IndustrialModLoader.IETexture, 177, 0f, 0f, 0.5f),
				new BulletItem("IronBullet", IndustrialModLoader.BaseTexture, 229, 90f, 0.1f, 1f, isDisplay: false),
				new FlaxSeedItem(),
				new IEFlatItem("FlaxFiber", 6)
				{
					Category = "Plants"
				},
				new WrenchsItem(),
				new ModuleItem("IngotModule", "IngotModule", typeof(MetalIngotItem)),
				new ModuleItem("PlateModule", "PlateModule", typeof(PlateItem)),
				new ModuleItem("RodModule", "RodModule", typeof(RodItem)),
				new ModuleItem("GearModule", "GearModule", typeof(SteelGearItem)),
				new ModuleItem("WheelModule", "WheelModule", typeof(SteelWheelItem)),
				new ModuleItem("UIronSheetModule", "UIronSheetModule", typeof(UIronSheetItem)),
				new UIronSheetItem(),
				new MultimeterItem(),
				new OrePowderItem(Materials.FeAlCrAlloy, createRecipe: false)
				{
					Color = new Color(55, 55, 55),
					DisplayName = LanguageControl.Get("Items", "Names", "FeAlCrAlloyOrePowder"),
					Description = LanguageControl.Get("Items", "Descriptions", "FeAlCrAlloyOrePowder")
				},
				new ResistorItem(Materials.FeAlCrAlloy),
				new BottleItem("GlassBottle", Color.Transparent),
				new BottleItem("Bottle.C2H5OH", ChemistryFluidColors.C2H5OH),
				new BottleItem("Bottle.H2SO4", ChemistryFluidColors.H2SO4),
				new MeshItem("Chaff", "Models/Chaff", "Chaff", IndustrialModLoader.BaseTexture, Color.Yellow, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity),
				new CoalPowderItem("K2SO4Powder", Color.White, 0f, 0f),
				new CoalPowderItem("Nitrocellulose", Color.Yellow, 0f, 0f, 10f),
				new CoalPowderItem("Propellant", Color.LightYellow, 0f, 0f, 20f),
				new BulletItem("StandardBullet", IndustrialModLoader.IETexture, 193, 0f, 0f, 0.5f),
				new BulletItem("AntiTankBullet", IndustrialModLoader.IETexture, 193, 0f, 0f, 0.6f),
				new BulletItem("LeadBullet", IndustrialModLoader.BaseTexture, 229, 50f, 0.1f, 1f, isDisplay: false),
				new BulletItem("LeadBullet_Sniper", IndustrialModLoader.BaseTexture, 229, 120f, 0.1f, 1f, isDisplay: false),
				new BulletItem("AntiTankBullet_Head", IndustrialModLoader.BaseTexture, 229, 250f, 20f, 1f, isDisplay: false),
				new BottleItem("Bottle.KNO3", ChemistryFluidColors.KNO3aq),
				new BottleItem("Bottle.HNO3", ChemistryFluidColors.HNO3),
				new BottleItem("Bottle.C2H5OC2H5", ChemistryFluidColors.C2H5OC2H5),
				new PowderItem("Cryolite", Color.White),
				new OrePowderItem(Materials.Mercury),
				new CoalPowderItem("Hg(NO3)2", Color.White, 0f, 0f, 8f),
				new CoalPowderItem("Hg(ONC)2", Color.LightGray, 0f, 0f, 12f),
				new BulletItem("ShotGunBullet", IndustrialModLoader.IETexture, 196, 0f, 0f, 0.5f),
				new BulletItem("LeadBullet_ShotGun", IndustrialModLoader.BaseTexture, 229, 8f, 0.1f, 1f, isDisplay: false),
				new AlloyItem(Materials.Steel, "StainlessSteel"),
				new AlloyItem(Materials.Aluminum, "SuperAluminium"),
				new AlloyItem(Materials.Steel, "SiSteel"),
				new AlloyItem(Materials.Steel, "IndustrialSteel"),
				new AlloyItem(Materials.Copper, "IndustrialCopper"),
				new AlloyItem(Materials.Steel, "TitaniumSteel"),
				new AlloyItem(Materials.Chromium, "GunSteel"),
				new RodItem("GunSteelRod", Materials.Steel.GetColor()),
				new RodItem("GunSteelBarrel", Color.Gray),
				new PowderItem("PlantAsh", new Color(150, 150, 128)),
				new PowderItem("K2Cr2O4Powder", Color.Yellow),
				new PetroleumBucketItem(),
				new BottleItem("Bottle.FlaxOil", new Color(255, 200, 0, 200)),
				new FlaxOilBucketItem(),
				new MeshItem("CompressedFlaxSeeds", "Models/CompressedFlaxSeeds", "CompressedFlaxSeeds", IndustrialModLoader.IETexture, new Color(185, 124, 64), Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.CreateTranslation(0.3125f, 0.125f, 0f), 2.5f),
				new MeshItem("UsedCompressedFlaxSeeds", "Models/CompressedFlaxSeeds", "CompressedFlaxSeeds", IndustrialModLoader.IETexture, new Color(185, 124, 64) * Color.DarkYellow, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.CreateTranslation(0.3125f, 0.125f, 0f), 2.5f),
				new BucketItem("LightOilBucket", ChemistryFluidColors.LightOil, "Tools"),
				new BucketItem("HeavyOilBucket", ChemistryFluidColors.HeavyOil, "Tools"),
				new BucketItem("GasolineBucket", ChemistryFluidColors.Gasoline, "Tools"),
				new MeshItem("Rubber", "Models/Snowball", "Snowball", IndustrialModLoader.BaseTexture, Color.White, Matrix.CreateScale(3f) * Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.CreateTranslation(0.75f, -0.0625f, 0f)),
				new SpringItem(),
				new SparkPlugItem(),
				new PowderItem("Ashes", new Color(125, 70, 70)),
				new PowderItem("ConcretePowder", Color.Gray),
				new PowderItem("HighStrengthConcretePowder", Color.Gray * new Color(255, 255, 210)),
				new MeshItem("Cylinder", "Models/PistonCylinder", "Cylinder", ContentManager.Get<Texture2D>("Textures/Iron"), Materials.Steel.GetColor(), Matrix.CreateRotationX(-(float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0.6f, 0f), Matrix.Identity, 0.9f),
				new AlloyPlateItem(Materials.Steel, "StainlessSteel"),
				new AlloyPlateItem(Materials.Aluminum, "SuperAluminium"),
				new AlloyPlateItem(Materials.Steel, "SiSteel"),
				new AlloyPlateItem(Materials.Steel, "IndustrialSteel"),
				new AlloyPlateItem(Materials.Copper, "IndustrialCopper"),
				new AlloyPlateItem(Materials.Steel, "TitaniumSteel"),
				new AlloyPlateItem(Materials.Chromium, "GunSteel"),
				new GasolineBoatItem(),
				new AluminumBucketItem("AluminumBucket", Color.Transparent, Matrix.Identity, "Tools"),
				new AirshipItem(),
				new IcebreakerItem(),
				new CarItem(),
				new TractorItem(),
				new GasTankItem("GasTank"),
				new GasTankItem("GasTank.CH4"),
				new GasTankItem("GasTank.H2"),
				new GasTankItem("GasTank.O2"),
				new GasTankItem("GasTank.C2H4"),
				new GasTankItem("GasTank.CO2"),
				new GasTankItem("GasTank.CO"),
				new GasTankItem("GasTank.HCl"),
				new GasTankItem("GasTank.HF"),
				new GasTankItem("GasTank.Cl2"),
				new GasTankItem("GasTank.LPG"),
				new PowderItem("QuartzPowder", Color.White),
				new IEFlatItem("CoarseSilicon", 86)
				{
					Color = Color.Gray,
					Size = 0.9f
				},
				new CanItem("MeatCan", ContentManager.Get<Texture2D>("Textures/CookedMeatBlock")),
				new CanItem("FishCan", ContentManager.Get<Texture2D>("Textures/CookedFishBlock")),
				new CanItem("BirdCan", ContentManager.Get<Texture2D>("Textures/CookedBirdBlock")),
				new CanItem("BreadCan", ContentManager.Get<Texture2D>("Textures/BreadBlock")),
				new CanItem("PumpkinCan", ContentManager.Get<Texture2D>("Textures/PumpkinBlock")),
				new MeshItem("PolySilicon", "Models/SiliconPlane", "SiliconPlane", ContentManager.Get<Texture2D>("Textures/M-Silicon"), new Color(147, 159, 161) * Color.Gray, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity),
				new MeshItem("Silicon", "Models/SiliconPlane", "SiliconPlane", ContentManager.Get<Texture2D>("Textures/S-Silicon"), new Color(147, 159, 161) * Color.Gray, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity),
				new BrickItem("PlasticBar", new Color(255, 255, 255), Matrix.CreateTranslation(-0.0625f, 0f, 0f), IndustrialModLoader.IETexture, 1.25f),
				new BottleItem("Bottle.NaOH(aq)", ChemistryFluidColors.NaOHaq),
				new BottleItem("Bottle.C7H8", ChemistryFluidColors.C7H8),
				new BottleItem("Bottle.CoalTar", ChemistryFluidColors.CoalTar),
				new BrickItem("Graphite", new Color(44, 44, 44), Matrix.CreateTranslation(-0.0625f, 0f, 0f), IndustrialModLoader.IETexture),
				new MercuryBucketItem(),
				new MeshItem("Tire", "Models/Tire", "Tire", ContentManager.Get<Texture2D>("Textures/Tire"), Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity, 1.75f),
				new DiodeTubeItem(),
				new BasicCircuitItem(),
				new MeshItem("CylinderResistor", "Models/CylinderResistor", "CylinderResistor", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity, 2.4f),
				new MeshItem("Capacitor", "Models/Capacitor", "Capacitor", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity, 2.4f),
				new MonitorLinkerItem(),
				new MeshItem("Telescreen", "Models/Telescreen", "Telescreen", ContentManager.Get<Texture2D>("Textures/White"), new Color(120, 172, 120, 92), Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity, 1.4f),
				new PowderItem("ZnS", Color.White),
				new OreChunkItem("MagnetiteChunk", Matrix.CreateRotationX(1f) * Matrix.CreateRotationZ(-4f), Matrix.CreateTranslation(0.75f, 0.125f, 0f), Color.White, smooth: false),
				new PowderItem("MagnetitePowder", new Color(46, 38, 25)),
				new GasolineTankItem(isFull: false),
				new GasolineTankItem(isFull: true),
				new PowderItem("U235P", Color.LightGreen),
				new PowderItem("U238P", Color.DarkGreen),
				new PowderItem("MixedU238U235P", Color.Lerp(Color.LightGreen, Color.DarkGreen, 0.5f)),
				new AF_CardPaperItem(),
				new AF_PostItem("AF_What", ContentManager.Get<Texture2D>("Textures/AF_What")),
				new AF_PostItem("AF_Post238", ContentManager.Get<Texture2D>("Textures/AF_Post238")),
				new AF_PostItem("AF_Post235", ContentManager.Get<Texture2D>("Textures/AF_Post235")),
				new AF_PostItem("AF_PostWhat", ContentManager.Get<Texture2D>("Textures/AF_PostWhat")),
				new MeshItem("U235C", "Models/UCake", "UCake", ContentManager.Get<Texture2D>("Textures/Iron"), Color.LightGreen, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity, 1.5f),
				new MeshItem("U238C", "Models/UCake", "UCake", ContentManager.Get<Texture2D>("Textures/Iron"), Color.DarkGreen, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity, 1.5f),
				new RodItem("CarbonTunnel", Color.DarkGray, new Vector3(1.4f, 1f, 1.4f)),
				new MGunItem(),
				new PowderItem("PolySiliconPowder", new Color(147, 159, 161) * Color.Gray * Color.LightGray),
				new PowderItem("SiliconPowder", new Color(147, 159, 161) * Color.Gray),
				new MeshItem("Wafer", "Models/Wafer", "Wafer", ContentManager.Get<Texture2D>("Textures/S-Silicon"), new Color(147, 159, 161) * Color.LightGray, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity),
				new MeshItem("CrystalTriode", "Models/CrystalTriode", "CrystalTriode", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity),
				new AdvancedCircuitItem(),
				new MagazineItem("Clip.", 0, display: true),
				new MagazineItem("Clip.", 1, display: false),
				new MagazineItem("Clip.", 2, display: false),
				new MagazineItem("Clip.", 3, display: false),
				new MagazineItem("Clip.", 4, display: false),
				new MagazineItem("Clip.", 5, display: false),
				new MagazineItem("Clip.", 6, display: false),
				new MagazineItem("Clip.", 7, display: false),
				new MagazineItem("Clip.", 8, display: false),
				new MagazineItem("Clip.", 9, display: false),
				new MagazineItem("Clip.", 10, display: false),
				new MagazineItem("Clip.", 11, display: false),
				new MagazineItem("Clip.", 12, display: false),
				new MagazineItem("Clip.", 13, display: false),
				new MagazineItem("Clip.", 14, display: false),
				new MagazineItem("Clip.", 15, display: false),
				new MagazineItem("Clip.", 16, display: false),
				new MagazineItem("Clip.", 17, display: false),
				new MagazineItem("Clip.", 18, display: false),
				new MagazineItem("Clip.", 19, display: false),
				new MagazineItem("Clip.", 20, display: false),
				new MagazineItem("Clip.", 21, display: false),
				new MagazineItem("Clip.", 22, display: false),
				new MagazineItem("Clip.", 23, display: false),
				new MagazineItem("Clip.", 24, display: false),
				new MagazineItem("Clip.", 25, display: false),
				new MagazineItem("Clip.", 26, display: false),
				new MagazineItem("Clip.", 27, display: false),
				new MagazineItem("Clip.", 28, display: false),
				new MagazineItem("Clip.", 29, display: false),
				new MagazineItem("Clip.", 30, display: false),
				new MagazineItem("Clip.", 31, display: false),
				new MagazineItem("Clip.", 32, display: false),
				new MagazineItem("Clip.", 33, display: false),
				new MagazineItem("Clip.", 34, display: false),
				new MagazineItem("Clip.", 35, display: false),
				new MagazineItem("Clip.", 36, display: false),
				new MagazineItem("Clip.", 37, display: false),
				new MagazineItem("Clip.", 38, display: false),
				new MagazineItem("Clip.", 39, display: false),
				new MagazineItem("Clip.", 40, display: false),
				new MagazineItem("Clip.", 41, display: false),
				new MagazineItem("Clip.", 42, display: false),
				new MagazineItem("Clip.", 43, display: false),
				new MagazineItem("Clip.", 44, display: false),
				new MagazineItem("Clip.", 45, display: false),
				new MagazineItem("Clip.", 46, display: false),
				new MagazineItem("Clip.", 47, display: false),
				new MagazineItem("Clip.", 48, display: false),
				new MagazineItem("Clip.", 49, display: false),
				new MagazineItem("Clip.", 50, display: false),
				new MagazineItem("Clip.", 51, display: false),
				new MagazineItem("Clip.", 52, display: false),
				new MagazineItem("Clip.", 53, display: false),
				new MagazineItem("Clip.", 54, display: false),
				new MagazineItem("Clip.", 55, display: false),
				new MagazineItem("Clip.", 56, display: false),
				new MagazineItem("Clip.", 57, display: false),
				new MagazineItem("Clip.", 58, display: false),
				new MagazineItem("Clip.", 59, display: false),
				new MagazineItem("Clip.", 60, display: false),
				new MagazineItem("Clip.", 61, display: false),
				new MagazineItem("Clip.", 62, display: false),
				new PowderItem("SaltpeterPowder", Color.White),
				new PowderItem("SulfurPowder", Color.Yellow),
				new CoalBrickItem("SolidFuel", Color.White, Matrix.CreateTranslation(0.375f, -0.125f, 0f), IndustrialModLoader.IETexture, 1f, 240f),
				new ScrewDriversItem(reverse: false),
				new PowderItem("Al2O3Powder", Color.White),
				new MeshItem("Ruby", "Models/Ruby", "Ruby", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity, 2f),
				new BrickItem("RubyMaterial", Color.White, Matrix.CreateTranslation(0.3125f, -0.1875f, 0f), IndustrialModLoader.IETexture),
				new BottleItem("Bottle.MNT", ChemistryFluidColors.MNT),
				new CoalPowderItem("DNT", Color.LightYellow, 2f, 50f),
				new CoalPowderItem("TNT", Color.LightYellow, 2f, 200f),
				new CoalPowderItem("MiningTNT", Color.LightYellow, 2f, 20f),
				new OrePowderItem(Materials.Phosphorus),
				new BottleItem("Bottle.H3PO4", ChemistryFluidColors.H3PO4),
				new PillItem("SleepingPill", Color.White),
				new PillItem("IbuprofenPill", Color.LightMagenta),
				new PillItem("PenicillinPill", Color.LightYellow),
				new MagazineItem("Clip.", 63, display: true),
				new BottleItem("Bottle.CH3OH", ChemistryFluidColors.CH3OH),
				new BottleItem("Bottle.CH3COOH", ChemistryFluidColors.CH3COOH),
				new BottleItem("Bottle.C6H6", ChemistryFluidColors.C6H6),
				new GasTankItem("GasTank.C3H6"),
				new GasTankItem("GasTank.C4H6"),
				new BottleItem("Bottle.C6H5Cl", ChemistryFluidColors.C6H5Cl),
				new PowderItem("C6H4ClNO2", Color.LightYellow),
				new PowderItem("C6H6N2O2", Color.Yellow),
				new GasTankItem("GasTank.NH3"),
				new PowderItem("KNO2", Color.Yellow),
				new GasTankItem("GasTank.N2"),
				new BottleItem("Bottle.C8H7ClO", ChemistryFluidColors.C8H7ClO),
				new BottleItem("Bottle.CH3CHO", ChemistryFluidColors.CH3CHO),
				new PowderItem("DiazepamA", Color.White),
				new PowderItem("Diazepam", Color.White),
				new PowderItem("Mycete", Color.DarkGray),
				new BottleItem("Bottle.Mould", ChemistryFluidColors.Mould),
				new BottleItem("Bottle.Penicillin-A", ChemistryFluidColors.PenicillinA),
				new BottleItem("Bottle.PenicillinLiquid", ChemistryFluidColors.PenicillinLiquid),
				new BottleItem("Bottle.CH3COOC2H5", ChemistryFluidColors.CH3COOC2H5),
				new PowderItem("Penicillin", Color.White),
				new BottleItem("Bottle.C10H14", ChemistryFluidColors.C10H14),
				new BottleItem("Bottle.C4H6O3", ChemistryFluidColors.C4H6O3),
				new BottleItem("Bottle.C10H12O", ChemistryFluidColors.C10H12O),
				new BottleItem("Bottle.C12H18O", ChemistryFluidColors.C12H18O),
				new PowderItem("Ibuprofen", Color.White),
				new ScrewDriversItem(reverse: true),
				new BucketItem("DieselOilBucket", ChemistryFluidColors.DieselOil, "Tools"),
				new GasTankItem("GasTank.SO3"),
				new SubMarineItem(),
				new MeshItem("LEDSheet", "Models/LEDSheet", "LEDSheet", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity)
			};
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			throw;
		}
		IdMap = new Dictionary<string, int>();
		int num = 0;
		ItemBlock[] items = Items;
		foreach (ItemBlock itemBlock in items)
		{
			int value2 = Terrain.MakeBlockValue(BaseItemBlock.Index, 0, num++);
			IdMap.Add(itemBlock.GetCraftingId(), value2);
		}
	}

	public static ItemBlock GetItem(int value)
	{
		int num = Terrain.ExtractData(value);
		if (num < Items.Length)
		{
			return Items[num];
		}
		return Items[0];
	}

	public static ItemBlock GetItem(string craftingID)
	{
		return GetItem(IdMap[craftingID]);
	}

	public static int GetItemID(string craftingID)
	{
		if (!IdMap.ContainsKey(craftingID))
		{
			return -1;
		}
		return IdMap[craftingID];
	}

	public static int GetItemID(ItemBlock item)
	{
		return IdMap[item.GetCraftingId()];
	}

	public static T? GetItemByMaterial<T>(Materials material) where T : ItemBlock, IMaterialBlock
	{
		ItemBlock[] array = Items.Where((ItemBlock x) => x.GetType() == typeof(T)).ToArray();
		foreach (ItemBlock itemBlock in array)
		{
			IMaterialBlock materialBlock = (itemBlock as IMaterialBlock) ?? null;
			if (materialBlock == null || materialBlock.GetMaterial() == material)
			{
				return itemBlock as T;
			}
		}
		return null;
	}

	public static ItemBlock? GetItemByMaterial(Type itemType, Materials material)
	{
		if (!typeof(ItemBlock).IsAssignableFrom(itemType) || !typeof(IMaterialBlock).IsAssignableFrom(itemType))
		{
			throw new ArgumentException("itemType must be of type ItemBlock and implement IMaterialBlock");
		}
		ItemBlock[] array = Items.Where((ItemBlock x) => x.GetType() == itemType).ToArray();
		foreach (ItemBlock itemBlock in array)
		{
			if (!(itemBlock is IMaterialBlock materialBlock) || materialBlock.GetMaterial() == material)
			{
				return itemBlock;
			}
		}
		return null;
	}

	public static int GetItemIdByMaterial<T>(Materials material) where T : ItemBlock, IMaterialBlock
	{
		switch (material)
		{
		case Materials.Iron:
			if (typeof(T) == typeof(MetalIngotItem))
			{
				return IronIngotBlock.Index;
			}
			if (typeof(T) == typeof(RodItem))
			{
				return RodBlock.Index;
			}
			if (typeof(T) == typeof(OreChunkItem))
			{
				return IronOreChunkBlock.Index;
			}
			break;
		case Materials.Copper:
			if (typeof(T) == typeof(MetalIngotItem))
			{
				return CopperIngotBlock.Index;
			}
			if (typeof(T) == typeof(OreChunkItem))
			{
				return MalachiteChunkBlock.Index;
			}
			break;
		case Materials.Germanium:
			if (typeof(T) == typeof(OreChunkItem))
			{
				return GermaniumOreChunkBlock.Index;
			}
			break;
		}
		T itemByMaterial = GetItemByMaterial<T>(material);
		if (itemByMaterial != null)
		{
			return GetItemID(itemByMaterial);
		}
		return 0;
	}

	public static int GetItemIdByMaterial(Type itemType, Materials material)
	{
		if (!typeof(ItemBlock).IsAssignableFrom(itemType) || !typeof(IMaterialBlock).IsAssignableFrom(itemType))
		{
			throw new ArgumentException("itemType must be of type ItemBlock and implement IMaterialBlock");
		}
		switch (material)
		{
		case Materials.Iron:
			if (itemType == typeof(MetalIngotItem))
			{
				return IronIngotBlock.Index;
			}
			if (itemType == typeof(RodItem))
			{
				return RodBlock.Index;
			}
			if (itemType == typeof(OreChunkItem))
			{
				return IronOreChunkBlock.Index;
			}
			break;
		case Materials.Copper:
			if (itemType == typeof(MetalIngotItem))
			{
				return CopperIngotBlock.Index;
			}
			if (itemType == typeof(OreChunkItem))
			{
				return MalachiteChunkBlock.Index;
			}
			break;
		case Materials.Germanium:
			if (itemType == typeof(OreChunkItem))
			{
				return GermaniumOreChunkBlock.Index;
			}
			break;
		}
		ItemBlock itemByMaterial = GetItemByMaterial(itemType, material);
		if (itemByMaterial != null)
		{
			return GetItemID(itemByMaterial);
		}
		return 0;
	}

	public static FilteredItemsEnumerable<T> GetItems<T>() where T : ItemBlock
	{
		return new FilteredItemsEnumerable<T>();
	}

	public static string GetItemCraftingID(int value)
	{
		return GetItem(value).GetCraftingId();
	}

	public static BlockMesh CreateRing()
	{
		BlockMesh blockMesh = new BlockMesh();
		Matrix matrix = Matrix.CreateScale(0.7f, 0.15f, 0.7f) * Matrix.CreateTranslation(0.5f, 0f, 0f) * Matrix.CreateRotationX((float)Math.PI / 2f);
		for (float num = 0f; num < (float)Math.PI * 8f; num += (float)Math.PI / 12f)
		{
			blockMesh.AppendMesh("Models/Rods", "SteelRod", matrix * Matrix.CreateTranslation(0f, 0.03f * num - 0.5f, 0f) * Matrix.CreateRotationY(num), Matrix.Identity, Color.White);
		}
		return blockMesh;
	}
}
