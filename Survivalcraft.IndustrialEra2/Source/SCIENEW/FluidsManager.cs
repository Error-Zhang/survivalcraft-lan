using System.Collections.Generic;
using System.Linq;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public static class FluidsManager
{
	public static Fluid[] Fluids;

	public static Dictionary<string, int[]> IdMap = new Dictionary<string, int[]>();

	public static BiDictionary<int, int> FluidToBlock = new BiDictionary<int, int>();

	private static Texture2D m_normalFluidTexture;

	private static Texture2D m_gasFluidTexture;

	public static Texture2D NormalFluidTexture
	{
		get
		{
			if (m_normalFluidTexture == null)
			{
				m_normalFluidTexture = ContentManager.Get<Texture2D>("Textures/NormalFluid");
			}
			return m_normalFluidTexture;
		}
	}

	public static Texture2D GasFluidTexture
	{
		get
		{
			if (m_gasFluidTexture == null)
			{
				m_gasFluidTexture = ContentManager.Get<Texture2D>("Textures/Clouds");
			}
			return m_gasFluidTexture;
		}
	}

	public static void Init()
	{
		Fluids = new Fluid[66]
		{
			new AirFluid(),
			new WaterFluid(),
			new MagmaFluid(),
			new MoltenMetalFluid(Materials.Steel),
			new MoltenMetalFluid(Materials.Gold),
			new MoltenMetalFluid(Materials.Silver),
			new MoltenMetalFluid(Materials.Lead),
			new MoltenMetalFluid(Materials.Platinum),
			new MoltenMetalFluid(Materials.Zinc),
			new MoltenMetalFluid(Materials.Stannary),
			new MoltenMetalFluid(Materials.Chromium),
			new MoltenMetalFluid(Materials.Titanium),
			new MoltenMetalFluid(Materials.Nickel),
			new MoltenMetalFluid(Materials.Aluminum),
			new MoltenMetalFluid(Materials.Iron),
			new MoltenMetalFluid(Materials.Copper),
			new MoltenMetalFluid(Materials.Germanium),
			new MoltenMetalFluid(Materials.FeAlCrAlloy),
			new MoltenMetalFluid(Materials.Brass),
			new MoltenMetalFluid(Materials.Vanadium),
			new MoltenMetalFluid(Materials.Cupronickel),
			new ChemistryFluid("SO3", ChemistryFluidColors.SO3, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.SO3")
			} }),
			new ChemistryFluid("H2SO4", ChemistryFluidColors.H2SO4, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.H2SO4")
			} }),
			new ChemistryFluid("C2H5OH", ChemistryFluidColors.C2H5OH, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C2H5OH")
			} }, 1f, 0.5f),
			new ChemistryFluid("CH4", ChemistryFluidColors.CH4, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.CH4")
			} }, 2f, 0.08f),
			new ChemistryFluid("KNO3(aq)", ChemistryFluidColors.KNO3aq, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.KNO3")
			} }),
			new ChemistryFluid("HNO3", ChemistryFluidColors.HNO3, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.HNO3")
			} }),
			new ChemistryFluid("C2H5OC2H5", ChemistryFluidColors.C2H5OC2H5, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C2H5OC2H5")
			} }),
			new PetroleumFluid(),
			new FlaxOilFluid(),
			new ChemistryFluid("LightOil", ChemistryFluidColors.LightOil, FluidType.Liquid, new BiDictionary<int, int> {
			{
				EmptyBucketBlock.Index,
				ItemsBlockManager.GetItemID("LightOilBucket")
			} }, 0f, 0f, useLangFileName: true),
			new ChemistryFluid("HeavyOil", ChemistryFluidColors.HeavyOil, FluidType.Liquid, new BiDictionary<int, int> {
			{
				EmptyBucketBlock.Index,
				ItemsBlockManager.GetItemID("HeavyOilBucket")
			} }, 0f, 0f, useLangFileName: true),
			new ChemistryFluid("Gasoline", ChemistryFluidColors.Gasoline, FluidType.Liquid, new BiDictionary<int, int>
			{
				{
					EmptyBucketBlock.Index,
					ItemsBlockManager.GetItemID("GasolineBucket")
				},
				{
					ItemsBlockManager.GetItemID("GasolineTank"),
					ItemsBlockManager.GetItemID("GasolineTank_Full")
				}
			}, 0f, 0f, useLangFileName: true),
			new ChemistryFluid("H2", ChemistryFluidColors.H2, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.H2")
			} }),
			new ChemistryFluid("O2", ChemistryFluidColors.O2, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.O2")
			} }),
			new ChemistryFluid("C2H4", ChemistryFluidColors.C2H4, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.C2H4")
			} }),
			new ChemistryFluid("CO2", ChemistryFluidColors.CO2, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.CO2")
			} }),
			new ChemistryFluid("CO", ChemistryFluidColors.CO, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.CO")
			} }),
			new ChemistryFluid("HCl", ChemistryFluidColors.HCl, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.HCl")
			} }),
			new ChemistryFluid("HF", ChemistryFluidColors.HF, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.HF")
			} }),
			new ChemistryFluid("Cl2", ChemistryFluidColors.Cl2, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.Cl2")
			} }),
			new ChemistryFluid("LPG", ChemistryFluidColors.LPG, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.LPG")
			} }, 2f, 0.06f, useLangFileName: true),
			new ChemistryFluid("C7H8", ChemistryFluidColors.C7H8, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C7H8")
			} }),
			new ChemistryFluid("NaOH(aq)", ChemistryFluidColors.NaOHaq, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.NaOH(aq)")
			} }),
			new ChemistryFluid("CoalTar", ChemistryFluidColors.CoalTar, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.CoalTar")
			} }, 0f, 0f, useLangFileName: true),
			new ChemistryFluid("DieselOil", ChemistryFluidColors.DieselOil, FluidType.Liquid, new BiDictionary<int, int> {
			{
				EmptyBucketBlock.Index,
				ItemsBlockManager.GetItemID("DieselOilBucket")
			} }, 0f, 0f, useLangFileName: true),
			new ChemistryFluid("MNT", ChemistryFluidColors.MNT, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.MNT")
			} }),
			new ChemistryFluid("H3PO4", ChemistryFluidColors.H3PO4, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.H3PO4")
			} }),
			new ChemistryFluid("CH3OH", ChemistryFluidColors.CH3OH, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.CH3OH")
			} }, 1f, 0.5f),
			new ChemistryFluid("CH3COOH", ChemistryFluidColors.CH3COOH, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.CH3COOH")
			} }, 1f, 0.5f),
			new ChemistryFluid("C6H6", ChemistryFluidColors.C6H6, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C6H6")
			} }),
			new ChemistryFluid("C3H6", ChemistryFluidColors.C3H6, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.C3H6")
			} }),
			new ChemistryFluid("C4H6", ChemistryFluidColors.C4H6, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.C4H6")
			} }),
			new ChemistryFluid("C6H5Cl", ChemistryFluidColors.C6H5Cl, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C6H5Cl")
			} }),
			new ChemistryFluid("NH3", ChemistryFluidColors.NH3, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.NH3")
			} }),
			new ChemistryFluid("N2", ChemistryFluidColors.N2, FluidType.Gas, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GasTank"),
				ItemsBlockManager.GetItemID("GasTank.N2")
			} }),
			new ChemistryFluid("C8H7ClO", ChemistryFluidColors.C8H7ClO, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C8H7ClO")
			} }),
			new ChemistryFluid("CH3CHO", ChemistryFluidColors.CH3CHO, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.CH3CHO")
			} }),
			new ChemistryFluid("Mould", ChemistryFluidColors.Mould, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.Mould")
			} }),
			new ChemistryFluid("Penicillin-A", ChemistryFluidColors.PenicillinA, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.Penicillin-A")
			} }),
			new ChemistryFluid("PenicillinLiquid", ChemistryFluidColors.PenicillinLiquid, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.PenicillinLiquid")
			} }),
			new ChemistryFluid("CH3COOC2H5", ChemistryFluidColors.CH3COOC2H5, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.CH3COOC2H5")
			} }),
			new ChemistryFluid("C10H14", ChemistryFluidColors.C10H14, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C10H14")
			} }),
			new ChemistryFluid("C4H6O3", ChemistryFluidColors.C4H6O3, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C4H6O3")
			} }),
			new ChemistryFluid("C10H12O", ChemistryFluidColors.C10H12O, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C10H12O")
			} }),
			new ChemistryFluid("C12H18O", ChemistryFluidColors.C12H18O, FluidType.Liquid, new BiDictionary<int, int> {
			{
				ItemsBlockManager.GetItemID("GlassBottle"),
				ItemsBlockManager.GetItemID("Bottle.C12H18O")
			} })
		};
		int num = 0;
		Fluid[] fluids = Fluids;
		foreach (Fluid fluid in fluids)
		{
			int[] array = new int[fluid.GetCongenericFluidCount()];
			for (int j = 0; j < fluid.GetCongenericFluidCount(); j++)
			{
				int num2 = (array[j] = SetFluidValue(num, j));
				if (fluid is IPlaceableFluid placeableFluid)
				{
					FluidToBlock.Add(num2, placeableFluid.GetFluidBlockIndex(num2));
				}
			}
			IdMap.Add(fluid.GetCraftingID(array[0]), array);
			num++;
		}
	}

	public static int GetFluidIndex(int value)
	{
		return value & 0xFFFF;
	}

	public static int GetFluidData(int value)
	{
		return (value >> 16) & 0xFFFF;
	}

	public static int SetFluidValue(int index, int data)
	{
		return ((data & 0xFFFF) << 16) | (index & 0xFFFF);
	}

	public static Fluid GetFluid(int value)
	{
		return Fluids[GetFluidIndex(value)];
	}

	public static int GetFluidID(Fluid fluid)
	{
		return IdMap.ElementAt(Fluids.FirstIndex(fluid)).Value[0];
	}

	public static int GetFluidID(string id, int data)
	{
		return IdMap[id][data];
	}

	public static int GetFluidIDByMaterial<T>(Materials material) where T : Fluid
	{
		Fluid[] fluids = Fluids;
		foreach (Fluid fluid in fluids)
		{
			if (!(fluid.GetType() != typeof(T)) && fluid is IMaterialBlock materialBlock && materialBlock.GetMaterial() == material)
			{
				return GetFluidID(fluid);
			}
		}
		return -1;
	}

	public static float GetContainerVolume(int value)
	{
		int num = ((value == BlocksManager.GetBlockIndex<EmptyBucketBlock>() || value == BlocksManager.GetBlockIndex<WaterBucketBlock>() || value == BlocksManager.GetBlockIndex<MagmaBucketBlock>()) ? 4 : ((ItemsBlockManager.GetItem(value) is BottleItem) ? 1 : ((ItemsBlockManager.GetItem(value) is BucketItem) ? 4 : ((ItemsBlockManager.GetItem(value) is GasTankItem) ? 4 : ((DevicesBlockManager.GetDevice(value) is OilTankDevice) ? 64 : ((ItemsBlockManager.GetItem(value) is GasolineTankItem) ? 16 : 0))))));
		return num;
	}
}
