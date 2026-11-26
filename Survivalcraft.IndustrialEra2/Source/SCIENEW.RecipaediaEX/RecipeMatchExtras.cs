using System.Collections.Generic;
using System.Linq;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;

namespace SCIENEW.RecipaediaEX;

public static class RecipeMatchExtras
{
	public static void ApplyRecipaediaMatchExtras(IRecipe recipe)
	{
		if (recipe != null)
		{
			ApplyMatchedResultBlockValues(recipe);
			ApplyMatchedResultFluidValues(recipe);
			ApplyMatchedIngredientBlockValues(recipe);
		}
	}

	public static void ApplyRecipaediaMatchExtras(IEnumerable<IRecipe> recipes)
	{
		foreach (IRecipe recipe in recipes)
		{
			ApplyRecipaediaMatchExtras(recipe);
		}
	}

	public static void ApplyMatchedResultBlockValues(IRecipe recipe)
	{
		HashSet<int> hashSet = CollectResultBlockValues(recipe);
		if (hashSet.Count != 0)
		{
			recipe.SetExtraValue("MatchedResultBlockValues", hashSet.ToArray());
		}
	}

	public static void ApplyMatchedResultFluidValues(IRecipe recipe)
	{
		HashSet<int> hashSet = CollectResultFluidValues(recipe);
		if (hashSet.Count != 0)
		{
			recipe.SetExtraValue("SCIENEW.MatchedResultFluidValues", hashSet.ToArray());
		}
	}

	public static void ApplyMatchedIngredientBlockValues(IRecipe recipe)
	{
		HashSet<int> hashSet = CollectIngredientBlockValues(recipe);
		if (hashSet.Count != 0)
		{
			recipe.SetExtraValue("MatchedIngredientBlockValues", hashSet.ToArray());
		}
	}

	private static HashSet<int> CollectResultBlockValues(IRecipe recipe)
	{
		HashSet<int> hashSet = new HashSet<int>();
		if (!(recipe is FormattedRecipe formattedRecipe))
		{
			if (!(recipe is One2OneRecipe one2OneRecipe))
			{
				if (!(recipe is FillingRecipe fillingRecipe))
				{
					if (!(recipe is CokingRecipe cokingRecipe))
					{
						if (!(recipe is VacuumFurnaceRecipe vacuumFurnaceRecipe))
						{
							if (!(recipe is WireComposingRecipe wireComposingRecipe))
							{
								if (!(recipe is CastingEquationRecipe castingEquationRecipe))
								{
									if (!(recipe is ThickeningEquationRecipe thickeningEquationRecipe))
									{
										if (!(recipe is SiftingRecipe siftingRecipe))
										{
											if (!(recipe is ReactionEquationRecipe reactionEquationRecipe))
											{
												if (!(recipe is AlloyingEquationRecipe alloyingEquationRecipe))
												{
													if (!(recipe is ElectrolyzingEquationRecipe electrolyzingEquationRecipe))
													{
														if (recipe is PetrochemicalEquationRecipe petrochemicalEquationRecipe)
														{
															AddFromEquationItems(hashSet, petrochemicalEquationRecipe.ResultsItems);
														}
													}
													else
													{
														AddFromEquationItems(hashSet, electrolyzingEquationRecipe.ResultsItems);
													}
												}
												else
												{
													AddFromEquationItems(hashSet, alloyingEquationRecipe.ResultsItems);
												}
											}
											else
											{
												AddFromEquationItems(hashSet, reactionEquationRecipe.ResultsItems);
											}
										}
										else
										{
											SiftingRecipe.SiftingResult[] results = siftingRecipe.Results;
											for (int i = 0; i < results.Length; i++)
											{
												SiftingRecipe.SiftingResult siftingResult = results[i];
												if (siftingResult.Value != 0)
												{
													hashSet.Add(siftingResult.Value);
												}
											}
										}
									}
									else if (thickeningEquationRecipe.ResultValue != 0)
									{
										hashSet.Add(thickeningEquationRecipe.ResultValue);
									}
								}
								else if (castingEquationRecipe.ResultValue != 0)
								{
									hashSet.Add(castingEquationRecipe.ResultValue);
								}
							}
							else if (wireComposingRecipe.ResultID != 0)
							{
								hashSet.Add(wireComposingRecipe.ResultID);
							}
						}
						else if (vacuumFurnaceRecipe.ResultID != 0)
						{
							hashSet.Add(vacuumFurnaceRecipe.ResultID);
						}
					}
					else if (cokingRecipe.ResultID != 0)
					{
						hashSet.Add(cokingRecipe.ResultID);
					}
				}
				else if (fillingRecipe.ResultID != 0)
				{
					hashSet.Add(fillingRecipe.ResultID);
				}
			}
			else if (one2OneRecipe.ResultID != 0)
			{
				hashSet.Add(one2OneRecipe.ResultID);
			}
		}
		else if (formattedRecipe.ResultValue != 0)
		{
			hashSet.Add(formattedRecipe.ResultValue);
		}
		return hashSet;
	}

	private static HashSet<int> CollectResultFluidValues(IRecipe recipe)
	{
		HashSet<int> hashSet = new HashSet<int>();
		if (!(recipe is MoltingEquationRecipe moltingEquationRecipe))
		{
			if (!(recipe is FermentingEquationRecipe fermentingEquationRecipe))
			{
				if (!(recipe is FractionatingEquationRecipe fractionatingEquationRecipe))
				{
					if (!(recipe is ReactionEquationRecipe reactionEquationRecipe))
					{
						if (!(recipe is ElectrolyzingEquationRecipe electrolyzingEquationRecipe))
						{
							if (recipe is PetrochemicalEquationRecipe petrochemicalEquationRecipe)
							{
								AddFromEquationFluids(hashSet, petrochemicalEquationRecipe.ResultsFluids);
							}
						}
						else
						{
							AddFromEquationFluids(hashSet, electrolyzingEquationRecipe.ResultsFluids);
						}
					}
					else
					{
						AddFromEquationFluids(hashSet, reactionEquationRecipe.ResultsFluids);
					}
				}
				else
				{
					AddFromEquationFluids(hashSet, fractionatingEquationRecipe.ResultsFluids);
				}
			}
			else if (fermentingEquationRecipe.ResultFluidValue != 0)
			{
				hashSet.Add(fermentingEquationRecipe.ResultFluidValue);
			}
		}
		else if (moltingEquationRecipe.ResultFluidValue != 0)
		{
			hashSet.Add(moltingEquationRecipe.ResultFluidValue);
		}
		return hashSet;
	}

	private static HashSet<int> CollectIngredientBlockValues(IRecipe recipe)
	{
		HashSet<int> hashSet = new HashSet<int>();
		if (!(recipe is FormattedRecipe formattedRecipe))
		{
			if (!(recipe is One2OneRecipe one2OneRecipe))
			{
				if (!(recipe is FillingRecipe fillingRecipe))
				{
					if (!(recipe is SiftingRecipe siftingRecipe))
					{
						if (!(recipe is CokingRecipe cokingRecipe))
						{
							if (!(recipe is WireComposingRecipe wireComposingRecipe))
							{
								if (!(recipe is VacuumFurnaceRecipe vacuumFurnaceRecipe))
								{
									if (!(recipe is CastingEquationRecipe castingEquationRecipe))
									{
										if (!(recipe is ThickeningEquationRecipe thickeningEquationRecipe))
										{
											if (!(recipe is FermentingEquationRecipe fermentingEquationRecipe))
											{
												if (!(recipe is ReactionEquationRecipe reactionEquationRecipe))
												{
													if (!(recipe is AlloyingEquationRecipe alloyingEquationRecipe))
													{
														if (!(recipe is ElectrolyzingEquationRecipe electrolyzingEquationRecipe))
														{
															if (!(recipe is PetrochemicalEquationRecipe petrochemicalEquationRecipe))
															{
																if (!(recipe is MoltingEquationRecipe moltingEquationRecipe))
																{
																	if (recipe is FractionatingEquationRecipe fractionatingEquationRecipe)
																	{
																		AddFromEquationFluids(hashSet, fractionatingEquationRecipe.MaterialsFluids);
																	}
																}
																else if (moltingEquationRecipe.IngredientValue != 0)
																{
																	hashSet.Add(moltingEquationRecipe.IngredientValue);
																}
															}
															else
															{
																AddFromEquationItems(hashSet, petrochemicalEquationRecipe.MaterialsItems);
																AddFromEquationFluids(hashSet, petrochemicalEquationRecipe.MaterialsFluids);
															}
														}
														else
														{
															AddFromEquationItems(hashSet, electrolyzingEquationRecipe.MaterialsItems);
															AddFromEquationFluids(hashSet, electrolyzingEquationRecipe.MaterialsFluids);
														}
													}
													else
													{
														AddFromEquationItems(hashSet, alloyingEquationRecipe.MaterialsItems);
														AddFromEquationFluids(hashSet, alloyingEquationRecipe.MaterialsFluids);
													}
												}
												else
												{
													AddFromEquationItems(hashSet, reactionEquationRecipe.MaterialsItems);
													AddFromEquationFluids(hashSet, reactionEquationRecipe.MaterialsFluids);
												}
											}
											else if (fermentingEquationRecipe.IngredientValue != 0)
											{
												hashSet.Add(fermentingEquationRecipe.IngredientValue);
											}
										}
										else
										{
											if (thickeningEquationRecipe.IngredientValue != 0)
											{
												hashSet.Add(thickeningEquationRecipe.IngredientValue);
											}
											if (thickeningEquationRecipe.IngredientFluidValue != 0)
											{
												hashSet.Add(thickeningEquationRecipe.IngredientFluidValue);
											}
										}
									}
									else
									{
										AddBlockValues(hashSet, castingEquationRecipe.ModuleValues);
									}
								}
								else
								{
									AddFromIngredientStrings(hashSet, vacuumFurnaceRecipe.Ingredient1, vacuumFurnaceRecipe.Ingredient2, vacuumFurnaceRecipe.Ingredient3, vacuumFurnaceRecipe.Ingredient4);
								}
							}
							else
							{
								AddFromIngredientStrings(hashSet, wireComposingRecipe.Ingredient, wireComposingRecipe.WireIngredient);
							}
						}
						else
						{
							AddFromIngredientStrings(hashSet, cokingRecipe.Ingredient);
						}
					}
					else
					{
						AddFromIngredientStrings(hashSet, siftingRecipe.Ingredient);
					}
				}
				else
				{
					AddFromIngredientStrings(hashSet, fillingRecipe.Ingredient, fillingRecipe.CanIngredient);
				}
			}
			else
			{
				AddFromIngredientStrings(hashSet, one2OneRecipe.Ingredient);
			}
		}
		else
		{
			AddFromIngredientStrings(hashSet, formattedRecipe.Ingredients);
		}
		return hashSet;
	}

	private static void AddFromIngredientStrings(HashSet<int> blockValues, params string[] ingredients)
	{
		foreach (string text in ingredients)
		{
			if (!string.IsNullOrEmpty(text))
			{
				int[] array = RecipesUtils.ExpandIngredientToBlockValues(text);
				foreach (int item in array)
				{
					blockValues.Add(item);
				}
			}
		}
	}

	private static void AddFromEquationItems(HashSet<int> blockValues, EquationItem[] items)
	{
		for (int i = 0; i < items.Length; i++)
		{
			EquationItem equationItem = items[i];
			if (equationItem.Values == null)
			{
				continue;
			}
			int[] values = equationItem.Values;
			foreach (int num in values)
			{
				if (num != 0)
				{
					blockValues.Add(num);
				}
			}
		}
	}

	private static void AddFromEquationFluids(HashSet<int> blockValues, EquationFluid[] fluids)
	{
		for (int i = 0; i < fluids.Length; i++)
		{
			EquationFluid equationFluid = fluids[i];
			if (equationFluid.Value != 0)
			{
				blockValues.Add(equationFluid.Value);
			}
		}
	}

	private static void AddBlockValues(HashSet<int> blockValues, int[] values)
	{
		foreach (int num in values)
		{
			if (num != 0)
			{
				blockValues.Add(num);
			}
		}
	}
}
