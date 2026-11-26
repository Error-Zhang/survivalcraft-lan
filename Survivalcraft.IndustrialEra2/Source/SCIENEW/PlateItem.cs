using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Game;
using RecipaediaEX;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Extensions.Graphics;
using SCIENEW.RecipaediaEX.Loaders;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW;

public class PlateItem : MeshItem, IMaterialBlock, IProceduralRecipeProvider
{
	protected BoundingBox[] m_collisionBoxes;

	public Materials Material;

	public bool isSmall;

	public Materials GetMaterial()
	{
		return Material;
	}

	public float GetMoltedVolume()
	{
		if (Material == Materials.None)
		{
			return 0f;
		}
		if (!isSmall)
		{
			return 2f;
		}
		return 1f / 18f;
	}

	public PlateItem(string id, Color color, bool small = false, bool isCopper = false)
		: base(id)
	{
		Material = Materials.None;
		m_standaloneBlockMesh.AppendMesh("Models/Ingots", "IronPlate", small ? Matrix.CreateScale(0.5f) : Matrix.Identity, Matrix.Identity, color);
		m_collisionBoxes = new BoundingBox[1] { m_standaloneBlockMesh.CalculateBoundingBox() };
		Matrix matrix = (isCopper ? Matrix.CreateTranslation(0.125f, 0.0625f, 0f) : Matrix.CreateTranslation(0.125f, 0f, 0f));
		m_standaloneBlockMesh.TransformTextureCoordinates(matrix);
	}

	public PlateItem(Materials type, bool small = false)
		: this(type.ToString() + "Plate", (type == Materials.Copper) ? Color.White : type.GetColor(), small, type == Materials.Copper)
	{
		Material = type;
		string arg = type.ToDisplayName();
		isSmall = small;
		if (small)
		{
			Id = type.ToString() + "Sheet";
			DisplayName = string.Format(LanguageControl.Get("Items", "Names", "SheetItem"), arg);
			Description = string.Format(LanguageControl.Get("Items", "Descriptions", "SheetItem"), arg);
		}
		else
		{
			DisplayName = string.Format(LanguageControl.Get("Items", "Names", "PlateItem"), arg);
			Description = string.Format(LanguageControl.Get("Items", "Descriptions", "PlateItem"), arg);
		}
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, size * 1.5f, ref matrix, environmentData);
	}

	public override string GetCraftingId()
	{
		return Id;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_collisionBoxes;
	}

	public IEnumerable<IRecipe> GetProceduralRecipes()
	{
		if (Material == Materials.None)
		{
			yield break;
		}
		int selfValue = ItemsBlockManager.GetItemID(this);
		if (isSmall)
		{
			IECraftingRecipe iECraftingRecipe = new IECraftingRecipe();
			iECraftingRecipe.Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_PlateItem_0"), Material.ToDisplayName());
			iECraftingRecipe.RequiredHeatLevel = 0f;
			iECraftingRecipe.Ingredients = new string[36];
			iECraftingRecipe.ResultValue = ItemsBlockManager.GetItemID(Material.ToString() + "Sheet");
			iECraftingRecipe.ResultCount = 36;
			iECraftingRecipe.RequiredPlayerLevel = 0f;
			IECraftingRecipe iECraftingRecipe2 = iECraftingRecipe;
			iECraftingRecipe2.Ingredients[0] = RecipesUtils.BlockToCratingID(ItemsBlockManager.GetItemIdByMaterial<PlateItem>(Material));
			iECraftingRecipe2.PreTransformIngredients();
			yield return iECraftingRecipe2;
		}
		else
		{
			int itemIdByMaterial = ItemsBlockManager.GetItemIdByMaterial<MetalIngotItem>(Material);
			int blockValue = NormalBlockManager.GetNBlockIdByMaterial<MetalNBlock>(Material);
			yield return new One2OneRecipe
			{
				ResultID = selfValue,
				ResultCount = 1,
				Ingredient = RecipesUtils.BlockToCratingID(itemIdByMaterial),
				IngredientCount = 1,
				ProcessType = "Pressing",
				Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_PlateItem_1"), Material.ToDisplayName(), Material.ToDisplayName())
			};
			yield return new One2OneRecipe
			{
				ResultID = selfValue,
				ResultCount = 9,
				Ingredient = RecipesUtils.BlockToCratingID(blockValue),
				IngredientCount = 1,
				ProcessType = "MercuryPressingMedium",
				Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_PlateItem_2"), Material.ToDisplayName(), Material.ToDisplayName())
			};
		}
		ModuleItem[] array = ItemsBlockManager.GetItems<ModuleItem>().ToArray();
		if (array == null)
		{
			yield break;
		}
		ModuleItem[] array2 = array;
		foreach (ModuleItem moduleItem in array2)
		{
			if (!(moduleItem.GetResultType() != GetType()))
			{
				string processType = (((CastingUtils.IsHeatMetal(Material) ? ComponentHearthFurnaceCastingBasin.RequiredHeatLevel : ComponentBlastFurnaceCastingBasin.RequiredHeatLevel) == ComponentHearthFurnaceCastingBasin.RequiredHeatLevel) ? "HearthCasting" : "BlastCasting");
				yield return new CastingEquationRecipe
				{
					Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_PlateItem_3"), Material.ToDisplayName()),
					IngredientFluidValue = FluidsManager.GetFluidIDByMaterial<MoltenMetalFluid>(Material),
					IngredientFluidVolume = GetMoltedVolume(),
					ModuleValues = new int[1] { ItemsBlockManager.GetItemID(moduleItem) },
					ModuleCount = 1,
					ResultValue = selfValue,
					ResultCount = 1,
					ProcessType = processType,
					RequiredPlayerLevel = 0f
				};
				break;
			}
		}
	}
}
