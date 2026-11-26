using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Loaders.Readers;
using SCIENEW.Utils;
using XmlUtilities;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.RecipaediaEX.Loaders;

public class IndustrialRecipesLoader : IRecipesLoader
{
	public const string ControlledFileExtension = ".ier";

	public const string DefaultString = "Default";

	private IRecipesReader m_defaultReader = new FormattedRecipeReader();

	private Dictionary<string, IRecipesReader> m_readers = new Dictionary<string, IRecipesReader>();

	private List<XElement> m_iceFiles = new List<XElement>();

	private List<IRecipe> m_cacheRecipes = new List<IRecipe>();

	private static List<Assembly> m_scannedAssemblies = new List<Assembly>();

	public int Order => 1;

	private void CreateReaders()
	{
		ValueEnumerator<Where<FromEnumerable<Assembly>, Assembly>, Assembly> enumerator = ValueEnumerableExtensions.GetEnumerator<Where<FromEnumerable<Assembly>, Assembly>, Assembly>(in ILSpyHelper_AsRefReadOnly(from a in TypeCache.LoadedAssemblies.AsValueEnumerable()
			where !TypeCache.IsKnownSystemAssembly(a)
			select a));
		try
		{
			while (enumerator.MoveNext())
			{
				Assembly current = enumerator.Current;
				if (m_scannedAssemblies.Contains(current))
				{
					continue;
				}
				foreach (TypeInfo definedType in current.DefinedTypes)
				{
					if (typeof(IRecipesReader).IsAssignableFrom(definedType) && !definedType.IsAbstract)
					{
						try
						{
							IRecipesReader recipesReader = (IRecipesReader)Activator.CreateInstance(definedType.AsType());
							m_readers.Add(recipesReader.Identifier, recipesReader);
						}
						catch (Exception e)
						{
							Log.Error("[SCIENEW.RecipaediaEX]Error Creating Instance of " + definedType.FullName);
							Log.Error(e);
						}
					}
				}
				m_scannedAssemblies.Add(current);
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		static ref readonly T ILSpyHelper_AsRefReadOnly<T>(in T temp) where T : allows ref struct
		{
			//ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
			return ref temp;
		}
	}

	private void FindRecipeFiles()
	{
		foreach (ModEntity mod in ModsManager.ModList)
		{
			mod.GetFiles(".ier", (string _, Stream stream) =>
			{
				XElement xElement = XmlUtils.LoadXmlFromStream(stream, Encoding.UTF8, throwOnError: true);
				if (xElement != null)
				{
					m_iceFiles.Add(xElement);
				}
			});
		}
	}

	private void UpdateRecipes()
	{
		m_cacheRecipes.Clear();
		LoadRecipesInXml();
		LoadRecipesProcedural();
		RecipeMatchExtras.ApplyRecipaediaMatchExtras(m_cacheRecipes);
	}

	private void LoadRecipesProcedural()
	{
		ValueEnumerable<OfType<FromArray<DeviceBlock>, DeviceBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider> source = DevicesBlockManager.Devices.AsValueEnumerable().OfType<IProceduralRecipeProvider>();
		ValueEnumerable<OfType<FromArray<NormalBlock>, NormalBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider> second = NormalBlockManager.Blocks.AsValueEnumerable().OfType<IProceduralRecipeProvider>();
		ValueEnumerable<OfType<FromArray<ItemBlock>, ItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider> second2 = ItemsBlockManager.Items.AsValueEnumerable().OfType<IProceduralRecipeProvider>();
		ValueEnumerable<OfType<FromArray<DamageableItemBlock>, DamageableItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider> second3 = DamageableItemManager.Items.AsValueEnumerable().OfType<IProceduralRecipeProvider>();
		ValueEnumerable<Concat<Concat<Concat<Concat<OfType<FromArray<DeviceBlock>, DeviceBlock, IProceduralRecipeProvider>, OfType<FromArray<NormalBlock>, NormalBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<ItemBlock>, ItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<DamageableItemBlock>, DamageableItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<Fluid>, Fluid, IProceduralRecipeProvider>, IProceduralRecipeProvider>, IProceduralRecipeProvider> source2 = ValueEnumerableExtensions.Concat<Concat<Concat<Concat<OfType<FromArray<DeviceBlock>, DeviceBlock, IProceduralRecipeProvider>, OfType<FromArray<NormalBlock>, NormalBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<ItemBlock>, ItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<DamageableItemBlock>, DamageableItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<Fluid>, Fluid, IProceduralRecipeProvider>, IProceduralRecipeProvider>(second: FluidsManager.Fluids.AsValueEnumerable().OfType<IProceduralRecipeProvider>(), source: source.Concat<OfType<FromArray<DeviceBlock>, DeviceBlock, IProceduralRecipeProvider>, OfType<FromArray<NormalBlock>, NormalBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>(second).Concat<Concat<OfType<FromArray<DeviceBlock>, DeviceBlock, IProceduralRecipeProvider>, OfType<FromArray<NormalBlock>, NormalBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<ItemBlock>, ItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>(second2).Concat<Concat<Concat<OfType<FromArray<DeviceBlock>, DeviceBlock, IProceduralRecipeProvider>, OfType<FromArray<NormalBlock>, NormalBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<ItemBlock>, ItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<DamageableItemBlock>, DamageableItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>(second3));
		m_cacheRecipes.AddRange(source2.SelectMany<Concat<Concat<Concat<Concat<OfType<FromArray<DeviceBlock>, DeviceBlock, IProceduralRecipeProvider>, OfType<FromArray<NormalBlock>, NormalBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<ItemBlock>, ItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<DamageableItemBlock>, DamageableItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<Fluid>, Fluid, IProceduralRecipeProvider>, IProceduralRecipeProvider>, IProceduralRecipeProvider, IRecipe>((Func<IProceduralRecipeProvider, IEnumerable<IRecipe>>)((IProceduralRecipeProvider x) => x.GetProceduralRecipes())).ToArray<SelectMany<Concat<Concat<Concat<Concat<OfType<FromArray<DeviceBlock>, DeviceBlock, IProceduralRecipeProvider>, OfType<FromArray<NormalBlock>, NormalBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<ItemBlock>, ItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<DamageableItemBlock>, DamageableItemBlock, IProceduralRecipeProvider>, IProceduralRecipeProvider>, OfType<FromArray<Fluid>, Fluid, IProceduralRecipeProvider>, IProceduralRecipeProvider>, IProceduralRecipeProvider, IRecipe>, IRecipe>());
	}

	private void LoadRecipesInXml()
	{
		foreach (XElement iceFile in m_iceFiles)
		{
			try
			{
				LoadRecipeItems(iceFile);
			}
			catch (Exception e)
			{
				Log.Error($"[RecipaediaEX.XmlRecipesLoader]Error loading Recipe Item{iceFile}");
				Log.Error(e);
			}
		}
	}

	private void LoadRecipeItems(XElement recipesXml)
	{
		if (recipesXml == null)
		{
			return;
		}
		foreach (XElement item in recipesXml.Elements())
		{
			if (item.Name.LocalName == "Recipe")
			{
				string attributeValue = XmlUtils.GetAttributeValue(item, "Reader", "Default");
				IRecipe recipe = null;
				IRecipesReader value;
				if (attributeValue == "Default")
				{
					recipe = m_defaultReader.Decode(item);
				}
				else if (m_readers.TryGetValue(attributeValue, out value) && value != null)
				{
					recipe = value.Decode(item);
				}
				if (recipe != null)
				{
					m_cacheRecipes.Add(recipe);
				}
			}
			else
			{
				LoadRecipeItems(item);
			}
		}
	}

	public void Initialize()
	{
		CreateReaders();
		FindRecipeFiles();
	}

	public IEnumerable<IRecipe> GetRecipes()
	{
		RecipesUtils.InvalidateBlockLookupCache();
		UpdateRecipes();
		return m_cacheRecipes;
	}
}
