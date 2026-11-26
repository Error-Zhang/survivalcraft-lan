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

public class MetalIngotItem : MeshItem, IMaterialBlock, IProceduralRecipeProvider
{
	public Materials Type;

	private bool createOreRecipe;

	public Materials GetMaterial()
	{
		return Type;
	}

	public float GetMoltedVolume()
	{
		return 2f;
	}

	public MetalIngotItem(Materials type, bool createOreRecipe = false)
		: base(type.ToString() + "Ingot")
	{
		Type = type;
		this.createOreRecipe = createOreRecipe;
		string arg = type.ToDisplayName();
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "MetalIngotItem"), arg);
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "MetalIngotItem"), arg);
		m_standaloneBlockMesh.AppendMesh("Models/Ingots", "IronIngot", Matrix.CreateTranslation(0f, -0.1f, 0f), Matrix.Identity, type.GetColor());
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
	}

	public override string GetCraftingId()
	{
		return Id;
	}

	public IEnumerable<IRecipe> GetProceduralRecipes()
	{
		if (Type == Materials.None)
		{
			yield break;
		}
		int selfValue = ItemsBlockManager.GetItemID(this);
		if (createOreRecipe)
		{
			string chunkIngredient = RecipesUtils.BlockToCratingID(ItemsBlockManager.GetItemIdByMaterial<OreChunkItem>(Type));
			string powderIngredient = RecipesUtils.BlockToCratingID(ItemsBlockManager.GetItemIdByMaterial<OrePowderItem>(Type));
			float requiredFuelLevel = Type.GetRequiredFuelLevel();
			yield return CreateSmeltingRecipe(string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalIngotItem_0"), Type.ToDisplayName(), Type.ToDisplayName()), selfValue, 1, requiredFuelLevel, new Dictionary<int, string> { { 0, chunkIngredient } });
			yield return CreateSmeltingRecipe(string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalIngotItem_1"), Type.ToDisplayName(), Type.ToDisplayName()), selfValue, 4, requiredFuelLevel, new Dictionary<int, string>
			{
				{ 0, chunkIngredient },
				{ 1, chunkIngredient },
				{ 2, chunkIngredient }
			});
			yield return CreateSmeltingRecipe(string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalIngotItem_2"), Type.ToDisplayName(), Type.ToDisplayName()), selfValue, 1, requiredFuelLevel, new Dictionary<int, string> { { 0, powderIngredient } });
			yield return CreateSmeltingRecipe(string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalIngotItem_3"), Type.ToDisplayName(), Type.ToDisplayName()), selfValue, 4, requiredFuelLevel, new Dictionary<int, string>
			{
				{ 0, powderIngredient },
				{ 1, powderIngredient },
				{ 2, powderIngredient }
			});
			yield return CreateSmeltingRecipe(string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalIngotItem_4"), Type.ToDisplayName(), Type.ToDisplayName()), selfValue, 8, requiredFuelLevel, new Dictionary<int, string>
			{
				{ 0, powderIngredient },
				{ 1, powderIngredient },
				{ 2, powderIngredient },
				{ 3, powderIngredient }
			});
		}
		IECraftingRecipe iECraftingRecipe = new IECraftingRecipe();
		iECraftingRecipe.Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalIngotItem_5"), Type.ToDisplayName());
		iECraftingRecipe.Ingredients = new string[36];
		iECraftingRecipe.RemainsValue = 0;
		iECraftingRecipe.RemainsCount = 0;
		iECraftingRecipe.RequiredHeatLevel = 0f;
		iECraftingRecipe.RequiredPlayerLevel = 0f;
		iECraftingRecipe.ResultValue = selfValue;
		iECraftingRecipe.ResultCount = 9;
		IECraftingRecipe iECraftingRecipe2 = iECraftingRecipe;
		iECraftingRecipe2.Ingredients[0] = RecipesUtils.BlockToCratingID(NormalBlockManager.GetNBlockIdByMaterial<MetalNBlock>(Type));
		iECraftingRecipe2.PreTransformIngredients();
		yield return iECraftingRecipe2;
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
				string processType = (((CastingUtils.IsHeatMetal(Type) ? ComponentHearthFurnaceCastingBasin.RequiredHeatLevel : ComponentBlastFurnaceCastingBasin.RequiredHeatLevel) == ComponentHearthFurnaceCastingBasin.RequiredHeatLevel) ? "HearthCasting" : "BlastCasting");
				yield return new CastingEquationRecipe
				{
					Description = string.Format(LanguageControl.Get("IECraftingRecipes", "Descriptions", "SCIE_MetalIngotItem_6"), Type.ToDisplayName()),
					IngredientFluidValue = FluidsManager.GetFluidIDByMaterial<MoltenMetalFluid>(Type),
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

	private static IESmeltingRecipe CreateSmeltingRecipe(string description, int resultValue, int resultCount, float requiredHeatLevel, Dictionary<int, string> ingredients)
	{
		IESmeltingRecipe iESmeltingRecipe = new IESmeltingRecipe
		{
			Description = description,
			Ingredients = new string[36],
			RemainsValue = 0,
			RemainsCount = 0,
			RequiredHeatLevel = requiredHeatLevel,
			RequiredPlayerLevel = 0f,
			ResultValue = resultValue,
			ResultCount = resultCount
		};
		foreach (var (num2, text2) in ingredients)
		{
			iESmeltingRecipe.Ingredients[num2] = text2;
		}
		iESmeltingRecipe.PreTransformIngredients();
		return iESmeltingRecipe;
	}
}
