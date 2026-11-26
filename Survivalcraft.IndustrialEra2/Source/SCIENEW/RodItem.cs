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

public class RodItem : MeshItem, IMaterialBlock, IProceduralRecipeProvider
{
	public Materials Material;

	public Materials GetMaterial()
	{
		return Material;
	}

	public float GetMoltedVolume()
	{
		return (Material != Materials.None) ? 1 : 0;
	}

	public RodItem(Materials type)
		: this(type.ToString() + "Rod", (type == Materials.Copper) ? Color.White : type.GetColor(), type == Materials.Copper)
	{
		Material = type;
		string arg = type.ToDisplayName();
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "RodItem"), arg);
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "RodItem"), arg);
	}

	public RodItem(string id, Color color, bool isCopper = false)
		: this(id, color, Vector3.One, isCopper)
	{
	}

	public RodItem(string id, Color color, Vector3 scale, bool isCopper = false)
		: base(id)
	{
		Material = Materials.None;
		Id = id;
		m_standaloneBlockMesh.AppendMesh("Models/Rods", "SteelRod", Matrix.CreateScale(scale) * Matrix.CreateTranslation(0f, -0.5f, 0f), Matrix.Identity, color);
		Matrix matrix = (isCopper ? Matrix.CreateTranslation(0.1875f, 0.0625f, 0f) : Matrix.CreateTranslation(0.1875f, 0f, 0f));
		m_standaloneBlockMesh.TransformTextureCoordinates(matrix);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-1f, 0.5f, 0f);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, 1.6f * size, ref matrix, environmentData);
	}

	public override float GetMeleePower(int value)
	{
		return 2f;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return Vector3.Zero;
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.85f;
	}

	public IEnumerable<IRecipe> GetProceduralRecipes()
	{
		if (Material == Materials.None)
		{
			yield break;
		}
		int selfValue = ItemsBlockManager.GetItemID(this);
		int ingotValue = ItemsBlockManager.GetItemIdByMaterial<MetalIngotItem>(Material);
		IESmeltingRecipe iESmeltingRecipe = new IESmeltingRecipe();
		iESmeltingRecipe.Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_RodItem_0"), Material.ToDisplayName());
		iESmeltingRecipe.Ingredients = new string[36];
		iESmeltingRecipe.RemainsValue = 0;
		iESmeltingRecipe.RemainsCount = 0;
		iESmeltingRecipe.RequiredHeatLevel = Material.GetRequiredFuelLevel();
		iESmeltingRecipe.RequiredPlayerLevel = 0f;
		iESmeltingRecipe.ResultValue = selfValue;
		iESmeltingRecipe.ResultCount = 1;
		IESmeltingRecipe iESmeltingRecipe2 = iESmeltingRecipe;
		iESmeltingRecipe2.Ingredients[0] = (iESmeltingRecipe2.Ingredients[1] = RecipesUtils.BlockToCratingID(ingotValue));
		iESmeltingRecipe2.PreTransformIngredients();
		yield return iESmeltingRecipe2;
		yield return new One2OneRecipe
		{
			ResultID = selfValue,
			ResultCount = 2,
			Ingredient = RecipesUtils.BlockToCratingID(ingotValue),
			IngredientCount = 1,
			ProcessType = "Squeezing",
			Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_RodItem_1"), Material.ToDisplayName())
		};
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
					Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_RodItem_2"), Material.ToDisplayName()),
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
