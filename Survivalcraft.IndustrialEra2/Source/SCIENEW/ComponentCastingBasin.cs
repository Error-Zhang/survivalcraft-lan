using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using SCIENEW.Drawing.WorldDynamic;
using SCIENEW.Events;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using TemplatesDatabase;

namespace SCIENEW;

public abstract class ComponentCastingBasin : ComponentInventoryTankBase, IUpdateable, IInventoryProductionSlots, ITankProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[1];

	private static readonly int[] m_inputTankIndices = new int[1];

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	private CastingEquationRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	protected bool m_drawRequire;

	protected float m_speed = 0.25f;

	protected bool m_isCrafting;

	public Materials m_material;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemDrawingElements m_subsystemDrawingElements;

	public List<WorldDrawingElement> m_drawingElements = new List<WorldDrawingElement>();

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();

	public float m_previousLight;

	public virtual string ProcessType => "BlastCasting";

	public float CraftingProgress => m_craftingProgress;

	public int ModuleSlotIndex => 0;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int ResultSlotIndex => SlotsCount - 1;

	protected virtual string CrafterKindName => "CastingBasin";

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (m_isCrafting || GetSlotCount(ResultSlotIndex) > 0)
		{
			return 0f;
		}
		return base.GetSlotCapacity(tankIndex, value);
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		m_updateCraftingRecipe = true;
		m_drawRequire = true;
		base.AddSlotItems(slotIndex, value, count);
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		m_updateCraftingRecipe = true;
		m_drawRequire = true;
		int num = ((slotIndex == ResultSlotIndex) ? GetSlotValue(ResultSlotIndex) : 0);
		int num2 = base.RemoveSlotItems(slotIndex, count);
		if (num2 > 0 && slotIndex == ResultSlotIndex && num != 0)
		{
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, CrafterKindName));
		}
		return num2;
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		m_updateCraftingRecipe = true;
		m_drawRequire = true;
		base.AddTankFluid(tankIndex, value, volume, out overflow);
	}

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		m_updateCraftingRecipe = true;
		m_drawRequire = true;
		return base.RemoveTankFluid(tankIndex, volume);
	}

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex == ModuleSlotIndex && Terrain.ExtractContents(value) == BaseItemBlock.Index && ItemsBlockManager.GetItem(value) is ModuleItem)
		{
			return 1;
		}
		return 0;
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		if (m_drawingElements.Count > 0)
		{
			m_drawingElements.ForEach((WorldDrawingElement x) =>
			{
				m_subsystemDrawingElements.Unregister(x);
			});
			m_drawingElements.Clear();
		}
	}

	public void Update(float dt)
	{
		m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, m_componentBlockEntity.Coordinates);
		if (m_previousLight != (float)m_drawBlockEnvironmentData.Light)
		{
			m_previousLight = m_drawBlockEnvironmentData.Light;
			m_drawRequire = true;
		}
		if (m_drawRequire)
		{
			Draw();
			m_drawRequire = false;
		}
		m_isCrafting = false;
		if (m_updateCraftingRecipe)
		{
			m_updateCraftingRecipe = false;
			FindEquation();
		}
		if (m_craftingRecipe == null)
		{
			m_craftingProgress = 0f;
		}
		else
		{
			if (!(GetTankVolume(0) >= m_craftingRecipe.IngredientFluidVolume) || (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, CrafterKindName, m_craftingRecipe))))
			{
				return;
			}
			m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
			m_isCrafting = true;
			if (m_craftingProgress >= 1f)
			{
				ComponentInventoryTankBase.FindRemoveTankFluid(this, m_craftingRecipe.IngredientFluidValue, m_craftingRecipe.IngredientFluidVolume);
				if (GetSlotValue(ResultSlotIndex) == m_craftingRecipe.ResultValue)
				{
					m_slots[ResultSlotIndex].Count += m_craftingRecipe.ResultCount;
				}
				else
				{
					m_slots[ResultSlotIndex].Value = m_craftingRecipe.ResultValue;
					m_slots[ResultSlotIndex].Count = m_craftingRecipe.ResultCount;
				}
				m_craftingRecipe = null;
				m_craftingProgress = 0f;
				m_updateCraftingRecipe = true;
				m_isCrafting = false;
			}
		}
	}

	public void FindEquation()
	{
		m_craftingRecipe = null;
		if (GetTankValue(0) == 0)
		{
			UpdateState();
			return;
		}
		float requiredPlayerLevel = FindInteractingPlayer()?.PlayerData.Level ?? 1f;
		CastingEquationRecipe castingEquationRecipe = RecipaediaEXManager.FindMatchingRecipe<CastingEquationRecipe>(new CastingEquationRecipe
		{
			ProcessType = ProcessType,
			IngredientFluidValue = GetTankValue(0),
			IngredientFluidVolume = float.MaxValue,
			ModuleValues = ((GetSlotValue(ModuleSlotIndex) == 0) ? Array.Empty<int>() : new int[1] { GetSlotValue(ModuleSlotIndex) }),
			ModuleCount = ((GetSlotValue(ModuleSlotIndex) != 0) ? GetSlotCount(ModuleSlotIndex) : 0),
			RequiredPlayerLevel = requiredPlayerLevel
		});
		if (castingEquationRecipe == null)
		{
			UpdateState();
			return;
		}
		if (GetSlotCount(ResultSlotIndex) > 0 && GetSlotValue(ResultSlotIndex) != castingEquationRecipe.ResultValue)
		{
			UpdateState();
			return;
		}
		m_craftingRecipe = castingEquationRecipe;
		UpdateState();
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemDrawingElements = Project.FindSubsystem<SubsystemDrawingElements>(throwOnError: true);
		m_craftingProgress = valuesDictionary.GetValue("CraftingProgress", 0f);
		m_updateCraftingRecipe = true;
		m_drawRequire = true;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("CraftingProgress", m_craftingProgress);
	}

	private void UpdateState()
	{
		float num = ((m_craftingRecipe == null) ? NormalBlockManager.GetNBlockByMaterial<MetalNBlock>(Materials.Steel).GetMoltedVolume() : m_craftingRecipe.IngredientFluidVolume);
		m_tanks[0].Capacity = num;
		m_tanks[0].Volume = Math.Clamp(m_tanks[0].Volume, 0f, num);
		m_material = ((m_craftingRecipe != null) ? ((IMaterialBlock)FluidsManager.GetFluid(m_craftingRecipe.IngredientFluidValue)).GetMaterial() : Materials.None);
	}

	public void Draw()
	{
		if (m_drawingElements.Count > 0)
		{
			m_subsystemDrawingElements.Unregister(m_drawingElements);
			m_drawingElements.Clear();
		}
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int slotValue = GetSlotValue(ModuleSlotIndex);
		Color white = Color.White;
		if (slotValue != 0)
		{
			Block obj = BlocksManager.Blocks[Terrain.ExtractContents(slotValue)];
			Matrix matrix = Matrix.CreateScale(0.75f, 1f, 0.75f) * Matrix.CreateTranslation((float)coordinates.X + 0.5f, (float)coordinates.Y + 0.235f, (float)coordinates.Z + 0.5f);
			obj.DrawBlock(color: Color.White, primitivesRenderer: m_primitivesRenderer3D, value: slotValue, size: 1f, matrix: ref matrix, environmentData: m_drawBlockEnvironmentData);
		}
		if (GetTankVolume(0) > 0f)
		{
			Block block = BlocksManager.Blocks[WaterBlock.Index];
			float f = GetTankVolume(0) / GetTankOriginalCapacity(0);
			Matrix matrix2 = Matrix.CreateScale(0.875f, MathUtils.Lerp(0f, 0.25f, f), 0.875f) * Matrix.CreateTranslation((float)coordinates.X + 0.5f, (float)coordinates.Y + 0.36f, (float)coordinates.Z + 0.5f);
			white = m_material.GetMetalColor();
			block.DrawBlock(m_primitivesRenderer3D, block.BlockIndex, white, 1f, ref matrix2, m_drawBlockEnvironmentData);
		}
		if (GetSlotCount(ResultSlotIndex) > 0)
		{
			Block block2 = BlocksManager.Blocks[IronBlock.Index];
			Matrix matrix3 = Matrix.CreateScale(0.875f, 0.25f, 0.875f) * Matrix.CreateTranslation((float)coordinates.X + 0.5f, (float)coordinates.Y + 0.36f, (float)coordinates.Z + 0.5f);
			white = m_material.GetColor();
			block2.DrawBlock(m_primitivesRenderer3D, block2.BlockIndex, white, 1f, ref matrix3, m_drawBlockEnvironmentData);
		}
		m_drawingElements = m_primitivesRenderer3D.FormDrawingElements(ModelRenderingMode.TransparentAfterWater);
		m_subsystemDrawingElements.Register(m_drawingElements);
		m_primitivesRenderer3D.Clear();
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => (IEnumerable<int>)m_inputSlotIndices,
			ProductionSlotKind.Output => new int[1] { ResultSlotIndex },
			_ => m_emptySlotIndices,
		};
	}

	IEnumerable<int> ITankProductionSlots.GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Input)
		{
			return m_inputTankIndices;
		}
		return m_emptySlotIndices;
	}
}
