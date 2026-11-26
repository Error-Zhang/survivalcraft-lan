using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using SCIENEW.Events;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAdvancedCrusher : ComponentInventoryBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[1];

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	public SubsystemTerrain m_subsystemTerrain;

	public ComponentBlockEntity m_componentBlockEntity;

	public ComponentIEModel m_componentModel;

	public Model m_model;

	public Texture2D m_texture;

	public bool m_initialAssigned;

	public bool m_isWorking;

	private One2OneRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	protected float m_speed = 1f;

	public float m_rotation;

	public List<Point3> m_emptySpacePoint
	{
		get
		{
			int num = 3;
			List<Point3> list = new List<Point3>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<Point3> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = new Point3(0, 0, 1);
			num2++;
			span[num2] = new Point3(1, 0, 0);
			num2++;
			span[num2] = new Point3(1, 0, 1);
			return list;
		}
	}

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float CraftingProgress => m_craftingProgress;

	public int ResultSlotIndex => 1;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_componentModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		AdvancedCrusherDevice device = DevicesBlockManager.GetDevice<AdvancedCrusherDevice>();
		if (device != null)
		{
			m_model = device.m_model;
			m_texture = device.m_texture;
			m_componentModel.MainBoneName = m_model.RootBone.Name;
		}
		m_updateCraftingRecipe = true;
	}

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex != 0)
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		m_updateCraftingRecipe = true;
		base.AddSlotItems(slotIndex, value, count);
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		m_updateCraftingRecipe = true;
		int num = ((slotIndex == ResultSlotIndex) ? GetSlotValue(ResultSlotIndex) : 0);
		int num2 = base.RemoveSlotItems(slotIndex, count);
		if (num2 > 0 && slotIndex == ResultSlotIndex && num != 0)
		{
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "AdvancedCrusher"));
		}
		return num2;
	}

	public override void OnEntityRemoved()
	{
		Point3 point = m_componentBlockEntity.Coordinates;
		m_emptySpacePoint.ForEach((Point3 p) =>
		{
			m_subsystemTerrain.DestroyCell(0, point.X + p.X, point.Y + p.Y, point.Z + p.Z, 0, noDrop: true, noParticleSystem: true);
		});
	}

	public void Update(float dt)
	{
		if (!m_initialAssigned)
		{
			m_initialAssigned = true;
			foreach (Point3 item in m_emptySpacePoint)
			{
				if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, m_componentBlockEntity.Coordinates + item, out ComponentBlockEntity blockEntity))
				{
					blockEntity.Entity.FindComponent<ComponentEmptySpace>(throwOnError: true).m_root = m_componentBlockEntity.Coordinates;
				}
			}
		}
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		VoltElement voltElement = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true).GetVoltElement(coordinates) as AdvancedCrusherVoltElement;
		m_isWorking = voltElement != null && voltElement.CurrentWorkState == WorkState.Active;
		if (m_isWorking)
		{
			m_rotation += dt * 2f;
			m_rotation %= 360f;
			if (m_updateCraftingRecipe)
			{
				m_updateCraftingRecipe = false;
				FindRecipe();
			}
			if (m_craftingRecipe == null)
			{
				m_craftingProgress = 0f;
			}
			else
			{
				if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "AdvancedCrusher", m_craftingRecipe)))
				{
					return;
				}
				m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
				if (m_craftingProgress >= 1f)
				{
					int num = MathUtils.Max(1, m_craftingRecipe.IngredientCount);
					if (m_slots[0].Count < num)
					{
						m_craftingRecipe = null;
						m_craftingProgress = 0f;
						m_updateCraftingRecipe = true;
						return;
					}
					m_slots[0].Count -= num;
					m_slots[ResultSlotIndex].Value = m_craftingRecipe.ResultID;
					m_slots[ResultSlotIndex].Count += m_craftingRecipe.ResultCount;
					m_craftingRecipe = null;
					m_craftingProgress = 0f;
					m_updateCraftingRecipe = true;
				}
			}
		}
		else
		{
			m_craftingProgress = 0f;
			m_craftingRecipe = null;
			m_updateCraftingRecipe = true;
		}
		UpdateDrawing();
	}

	public void UpdateDrawing()
	{
		if (m_componentModel.Model == null)
		{
			m_componentModel.SetModel(m_model);
			m_componentModel.SetTexture(m_texture);
		}
		Matrix value = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates)) * Matrix.CreateTranslation(1f, 0f, 1f);
		Matrix value2 = Matrix.CreateRotationX(m_rotation);
		Matrix value3 = Matrix.CreateRotationX(0f - m_rotation);
		m_componentModel.SetBoneTransform("Crusher_Wheel0", value2);
		m_componentModel.SetBoneTransform("Crusher_Wheel1", value3);
		m_componentModel.SetBoneTransform(m_componentModel.Model.RootBone.Index, value);
	}

	public void FindRecipe()
	{
		m_craftingRecipe = null;
		string ingredient = RecipesUtils.BlockToCratingID(GetSlotValue(0));
		One2OneRecipe actual = new One2OneRecipe
		{
			ProcessType = "Kibbling",
			Ingredient = ingredient,
			IngredientCount = GetSlotCount(0)
		};
		m_craftingRecipe = RecipaediaEXManager.FindMatchingRecipe<One2OneRecipe>(actual);
		if (m_craftingRecipe != null)
		{
			Slot slot = m_slots[ResultSlotIndex];
			int num = Terrain.ExtractContents(m_craftingRecipe.ResultID);
			if (slot.Count != 0 && (m_craftingRecipe.ResultID != slot.Value || m_craftingRecipe.ResultCount + slot.Count > BlocksManager.Blocks[num].GetMaxStacking(m_craftingRecipe.ResultID)))
			{
				m_craftingRecipe = null;
			}
		}
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
}
