using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using SCIENEW.Drawing.WorldDynamic;
using SCIENEW.Extensions;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentSifter : ComponentInventoryBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[1];

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemAudio m_subsystemAudio;

	public SubsystemSoundMaterials m_subsystemSoundMaterials;

	public SubsystemPickables m_subsystemPickables;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemParticles m_subsystemParticles;

	public SiftingRecipe m_recipe;

	public SubsystemDrawingElements m_subsystemDrawingElements;

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();

	public List<WorldDrawingElement> m_drawingElements = new List<WorldDrawingElement>();

	public float m_progress;

	public double m_lastSiftTime;

	public bool m_drawRequest;

	public int m_previousLight;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1] { 1 };

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		SiftingRecipe siftingRecipe = FindRecipe(value, 1);
		if (siftingRecipe != null)
		{
			m_recipe = siftingRecipe;
			return 1;
		}
		m_recipe = null;
		return 0;
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		base.AddSlotItems(slotIndex, value, count);
		m_drawRequest = true;
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		int result = base.RemoveSlotItems(slotIndex, count);
		m_drawRequest = true;
		return result;
	}

	public bool Sift()
	{
		m_drawRequest = true;
		int slotValue = GetSlotValue(0);
		if (slotValue == 0)
		{
			return false;
		}
		if (Time.RealTime - m_lastSiftTime > 0.2)
		{
			m_lastSiftTime = Time.RealTime;
			m_progress += 0.05f;
			m_subsystemSoundMaterials.PlayImpactSound(slotValue, new Vector3(m_componentBlockEntity.Coordinates), 1f);
		}
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(GetSlotValue(0))];
		m_subsystemParticles.AddParticleSystem(block.CreateDebrisParticleSystem(m_subsystemTerrain, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 0f, 0.5f), GetSlotValue(0), 0.5f));
		if (m_progress >= 1f)
		{
			m_progress = 0f;
			m_slots[0].Count = 0;
			m_slots[0].Value = 0;
			if (m_recipe != null && m_recipe.Results.Length != 0)
			{
				float num = 0f;
				SiftingRecipe.SiftingResult[] results = m_recipe.Results;
				foreach (SiftingRecipe.SiftingResult siftingResult in results)
				{
					num = MathUtils.Max(num, siftingResult.Weight);
				}
				if (num > 0f)
				{
					Game.Random random = new Game.Random();
					float num2 = random.Float(0f, 1f);
					Vector3 position = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, 1f, 0.5f);
					results = m_recipe.Results;
					for (int i = 0; i < results.Length; i++)
					{
						SiftingRecipe.SiftingResult siftingResult2 = results[i];
						if (siftingResult2.Weight <= 0f || siftingResult2.Weight / num < num2)
						{
							continue;
						}
						int value = siftingResult2.Value;
						int num3 = MathUtils.Max(1, siftingResult2.Count);
						RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), value, num3, "Sifter"));
						for (int j = 0; j < num3; j++)
						{
							Vector3 vector = new Vector3(random.Float(-1f, 1f), random.Float(1f, 2f), random.Float(-1f, 1f));
							if (vector.LengthSquared() > 0f)
							{
								vector.Normalize();
							}
							else
							{
								vector = Vector3.UnitY;
							}
							m_subsystemPickables.AddPickable(value, 1, position, random.Float(1f, 1.5f) * vector, null);
						}
					}
				}
			}
		}
		return true;
	}

	public SiftingRecipe FindRecipe(int mainMaterial, int materialCount)
	{
		return RecipaediaEXManager.FindMatchingRecipe<SiftingRecipe>(new SiftingRecipe
		{
			Ingredient = RecipesUtils.BlockToCratingID(mainMaterial),
			IngredientCount = materialCount,
			ProcessType = "Sifting"
		});
	}

	public CraftingRecipe MatchRecipe(SubsystemTerrain terrain, string[] ingredients, HashSet<int> heatLevel, float playerLevel)
	{
		throw new NotImplementedException();
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemSoundMaterials = Project.FindSubsystem<SubsystemSoundMaterials>(throwOnError: true);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemDrawingElements = Project.FindSubsystem<SubsystemDrawingElements>(throwOnError: true);
		m_progress = valuesDictionary.GetValue("Progress", 0f);
		m_drawRequest = true;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Progress", m_progress);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		if (m_drawingElements.Count > 0)
		{
			m_subsystemDrawingElements.Unregister(m_drawingElements);
			m_drawingElements.Clear();
		}
	}

	public void Draw()
	{
		if (m_drawingElements.Count > 0)
		{
			m_subsystemDrawingElements.Unregister(m_drawingElements);
			m_drawingElements.Clear();
		}
		int slotValue = GetSlotValue(0);
		if (Terrain.ExtractContents(slotValue) != AirBlock.Index)
		{
			Block obj = BlocksManager.Blocks[Terrain.ExtractContents(slotValue)];
			Matrix matrix = Matrix.CreateScale(0.875f, (1f - m_progress) * 0.4375f, 0.875f) * Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + new Vector3(0f, MathUtils.Lerp(9f / 32f, 0.0625f, m_progress), 0f));
			obj.DrawBlock(m_primitivesRenderer3D, slotValue, Color.White, 1f, ref matrix, m_drawBlockEnvironmentData);
			m_drawingElements = m_primitivesRenderer3D.FormDrawingElements(ModelRenderingMode.AlphaThreshold);
			m_subsystemDrawingElements.Register(m_drawingElements);
			m_primitivesRenderer3D.Clear();
		}
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, coordinates);
		if (m_previousLight != m_drawBlockEnvironmentData.Light)
		{
			m_previousLight = m_drawBlockEnvironmentData.Light;
			m_drawRequest = true;
		}
		if (m_drawRequest)
		{
			Draw();
			m_drawRequest = false;
		}
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Input)
		{
			return m_inputSlotIndices;
		}
		return m_emptySlotIndices;
	}
}
