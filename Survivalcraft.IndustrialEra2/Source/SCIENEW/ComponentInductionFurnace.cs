using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using SCIENEW.Drawing.WorldDynamic;
using SCIENEW.Events;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentInductionFurnace : ComponentInventoryTankBase, IUpdateable, IInventoryProductionSlots, ITankProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[1];

	private static readonly int[] m_outputTankIndices = new int[1];

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	protected bool m_isWorking;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemDrawingElements m_subsystemDrawingElements;

	public float m_craftingProgress;

	public MoltingEquationRecipe? m_craftingEquation;

	public DynamicMeshDrawingElement m_drawingElement;

	public bool m_updateEquation;

	public bool m_drawRequire = true;

	protected WorkState m_previousWorkState;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public bool IsWorking => m_isWorking;

	public float CraftingProgress => m_craftingProgress;

	public virtual float Speed => 0.1f;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_tanks[0].Capacity = 16f;
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemDrawingElements = Project.FindSubsystem<SubsystemDrawingElements>(throwOnError: true);
		m_updateEquation = true;
	}

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (!(FluidsManager.GetFluid(value) is MoltenMetalFluid))
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		m_updateEquation = true;
		base.AddSlotItems(slotIndex, value, count);
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		m_updateEquation = true;
		return base.RemoveSlotItems(slotIndex, count);
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		m_updateEquation = true;
		m_drawRequire = true;
		base.AddTankFluid(tankIndex, value, volume, out overflow);
	}

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		m_updateEquation = true;
		m_drawRequire = true;
		return base.RemoveTankFluid(tankIndex, volume);
	}

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		m_drawingElement = new DynamicMeshDrawingElement(DrawingMaterial.Opaque(IndustrialModLoader.PureWhiteTexture))
		{
			RenderingMode = ModelRenderingMode.TransparentAfterWater
		};
		m_subsystemDrawingElements.Register(m_drawingElement);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		m_subsystemDrawingElements.Unregister(m_drawingElement);
		m_drawingElement = null;
	}

	public void Update(float dt)
	{
		if (m_drawRequire)
		{
			Draw();
			m_drawRequire = false;
		}
		UpdateWorkState();
		if (m_updateEquation)
		{
			m_updateEquation = false;
			FindEquation();
		}
		if (m_craftingEquation == null)
		{
			m_craftingProgress = 0f;
		}
		else
		{
			if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "InductionFurnace", m_craftingEquation)))
			{
				return;
			}
			m_craftingProgress = MathUtils.Min(m_craftingProgress + Speed * dt, 1f);
			if (m_craftingProgress >= 1f)
			{
				ComponentInventoryTankBase.FindRemoveSlotForItem(this, m_craftingEquation.IngredientValue, m_craftingEquation.IngredientCount);
				ComponentInventoryTankBase.AcquireFluid(this, m_craftingEquation.ResultFluidValue, m_craftingEquation.ResultFluidVolume);
				m_craftingEquation = null;
				m_craftingProgress = 0f;
				m_updateEquation = true;
			}
		}
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public virtual void UpdateWorkState()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		InductionFurnaceVoltElement inductionFurnaceVoltElement = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true).GetVoltElement(coordinates) as InductionFurnaceVoltElement;
		m_isWorking = inductionFurnaceVoltElement != null && inductionFurnaceVoltElement.CurrentWorkState == WorkState.Active;
		if (inductionFurnaceVoltElement == null || inductionFurnaceVoltElement.CurrentWorkState != WorkState.Active)
		{
			m_craftingProgress = 0f;
			m_craftingEquation = null;
			m_updateEquation = true;
		}
		if (m_previousWorkState != inductionFurnaceVoltElement?.CurrentWorkState && inductionFurnaceVoltElement != null)
		{
			m_previousWorkState = inductionFurnaceVoltElement.CurrentWorkState;
			m_updateEquation = true;
		}
	}

	public void FindEquation()
	{
		m_craftingEquation = null;
		if (GetSlotValue(0) != 0)
		{
			float requiredPlayerLevel = FindInteractingPlayer()?.PlayerData.Level ?? 1f;
			MoltingEquationRecipe moltingEquationRecipe = RecipaediaEXManager.FindMatchingRecipe<MoltingEquationRecipe>(new MoltingEquationRecipe
			{
				IngredientValue = GetSlotValue(0),
				IngredientCount = GetSlotCount(0),
				RequiredPlayerLevel = requiredPlayerLevel
			});
			if (moltingEquationRecipe != null && ComponentInventoryTankBase.FindAcquireTankForFluid(this, moltingEquationRecipe.ResultFluidValue) != -1)
			{
				m_craftingEquation = moltingEquationRecipe;
			}
		}
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = FourDirectionModelDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<InductionFurnaceDevice>(IsWorking ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public void Draw()
	{
		m_drawingElement.Clear();
		if (GetTankVolume(0) > 0f)
		{
			Fluid fluid = FluidsManager.GetFluid(GetTankValue(0));
			float f = GetTankVolume(0) / GetTankOriginalCapacity(0);
			float y = MathUtils.Lerp(0.25f, 0.875f, f);
			Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f, y, 0.5f);
			Vector3 p = vector + new Vector3(-0.375f, 0f, -0.375f);
			Vector3 p2 = vector + new Vector3(-0.375f, 0f, 0.375f);
			Vector3 p3 = vector + new Vector3(0.375f, 0f, 0.375f);
			Vector3 p4 = vector + new Vector3(0.375f, 0f, -0.375f);
			float num = 0.75f / (float)fluid.GetTextureSubdivision(GetTankValue(0));
			Vector2 zero = Vector2.Zero;
			Vector2 texCoord = Vector2.UnitX * num;
			Vector2 texCoord2 = Vector2.One * num;
			Vector2 texCoord3 = Vector2.UnitY * num;
			m_drawingElement.Material = DrawingMaterial.Opaque(fluid.GetTexture(GetTankValue(0))).WithTint(fluid.GetColor(GetTankValue(0))).WithUnlit(isUnlit: true);
			m_drawingElement.Mesh.QueueQuad(p4, p3, p2, p, texCoord3, texCoord2, texCoord, zero, Color.White);
			m_drawingElement.MarkGeometryDirty();
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

	IEnumerable<int> ITankProductionSlots.GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Output)
		{
			return m_outputTankIndices;
		}
		return m_emptySlotIndices;
	}
}
