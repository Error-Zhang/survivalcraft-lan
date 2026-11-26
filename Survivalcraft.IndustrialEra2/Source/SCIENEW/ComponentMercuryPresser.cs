using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using SCIENEW.Events;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentMercuryPresser : ComponentInventoryBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[1];

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	private One2OneRecipe? m_craftingRecipe;

	public float m_craftingProgress;

	protected bool m_updateCraftingRecipe;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTime m_subsystemTime;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemEnginePower m_subsystemEnginePower;

	public int ResultSlotIndex => SlotsCount - 1;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float CraftingProgress => m_craftingProgress;

	public bool IsIntegrity { get; private set; }

	private static float GetMercuryCraftSpeed(One2OneRecipe recipe)
	{
		return recipe.ProcessType switch
		{
			"MercuryPressingSlow" => 1f / 30f,
			"MercuryPressingMedium" => 0.2f,
			"Pressing" => 1f,
			_ => 0.2f,
		};
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_craftingProgress = valuesDictionary.GetValue("CraftingProgress", 0f);
		m_updateCraftingRecipe = true;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("CraftingProgress", m_craftingProgress);
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
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "MercuryPresser"));
		}
		return num2;
	}

	public void Update(float dt)
	{
		TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Z);
		bool flag;
		if (chunkAtCell != null)
		{
			TerrainChunkState state = chunkAtCell.State;
			if ((uint)(state - 5) <= 1u || state == TerrainChunkState.Valid)
			{
				flag = true;
				goto IL_004c;
			}
		}
		flag = false;
		goto IL_004c;
		IL_004c:
		if (flag && m_subsystemTime.PeriodicGameTimeEvent(0.20000000298023224, 0.0))
		{
			CheckIntegrity();
		}
		if (m_subsystemEnginePower.IsPowered(m_componentBlockEntity.Coordinates, out IEngineComponent _) && IsIntegrity)
		{
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
				if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "MercuryPresser", m_craftingRecipe)))
				{
					return;
				}
				m_craftingProgress = MathUtils.Min(m_craftingProgress + GetMercuryCraftSpeed(m_craftingRecipe) * dt, 1f);
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
	}

	public void CheckIntegrity()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int direction_Static = FourDirectionalDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z));
		Point3 point = coordinates - CellFace.FaceToPoint3(direction_Static);
		int num = -2;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (!CheckMetalBlock(point.X + i, point.Y + num, point.Z + j, Materials.Steel))
				{
					IsIntegrity = false;
					return;
				}
			}
		}
		num = -1;
		for (int k = -1; k <= 1; k++)
		{
			for (int l = -1; l <= 1; l++)
			{
				if ((k != 0 || l != 0) && !CheckMetalBlock(point.X + k, point.Y + num, point.Z + l, Materials.Steel))
				{
					IsIntegrity = false;
					return;
				}
				if (k == 0 && l == 0 && !CheckMagmaBlock(point.X + k, point.Y + num, point.Z + l))
				{
					IsIntegrity = false;
					return;
				}
			}
		}
		num = 0;
		if (!CheckMetalBlock(point.X, point.Y, point.Z, Materials.Chromium))
		{
			IsIntegrity = false;
			return;
		}
		for (int m = 0; m < 4; m++)
		{
			if (m != direction_Static)
			{
				Point3 point2 = CellFace.FaceToPoint3(m);
				if (!CheckShellBlock(point.X + point2.X, point.Y + point2.Y, point.Z + point2.Z))
				{
					IsIntegrity = false;
					return;
				}
			}
		}
		for (num = 1; num <= 32; num++)
		{
			for (int n = 0; n < 4; n++)
			{
				Point3 point3 = CellFace.FaceToPoint3(n);
				Point3 point4 = point + point3 + new Point3(0, num, 0);
				switch (num)
				{
				case 1:
					if (!CheckMetalBlock(point4.X, point4.Y, point4.Z, Materials.Chromium))
					{
						IsIntegrity = false;
						return;
					}
					break;
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
					if (!CheckMetalBlock(point4.X, point4.Y, point4.Z, Materials.Steel))
					{
						IsIntegrity = false;
						return;
					}
					break;
				default:
					if (!CheckConcreteBlock(point4.X, point4.Y, point4.Z))
					{
						IsIntegrity = false;
						return;
					}
					break;
				}
			}
			Point3 point5 = point + new Point3(0, num, 0);
			if (!CheckMercuryBlock(point5.X, point5.Y, point5.Z))
			{
				IsIntegrity = false;
				return;
			}
		}
		IsIntegrity = true;
		bool CheckConcreteBlock(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseNormalBlock.Index)
			{
				return NormalBlockManager.GetNormalBlockCraftingID(cellValueFast) == "ConcreteWall";
			}
			return false;
		}
		bool CheckMagmaBlock(int x, int y, int z)
		{
			return Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z)) == MagmaBlock.Index;
		}
		bool CheckMercuryBlock(int x, int y, int z)
		{
			return Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z)) == MercuryBlock.Index;
		}
		bool CheckMetalBlock(int x, int y, int z, Materials material)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseNormalBlock.Index && NormalBlockManager.GetNormalBlock(cellValueFast) is MetalNBlock metalNBlock)
			{
				return metalNBlock.GetMaterial() == material;
			}
			return false;
		}
		bool CheckShellBlock(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseNormalBlock.Index)
			{
				return NormalBlockManager.GetNormalBlock(cellValueFast) is MachineShellNBlock;
			}
			return false;
		}
	}

	public void FindRecipe()
	{
		m_craftingRecipe = null;
		string ingredient = RecipesUtils.BlockToCratingID(GetSlotValue(0));
		int slotCount = GetSlotCount(0);
		string[] array = new string[3] { "MercuryPressingSlow", "MercuryPressingMedium", "Pressing" };
		foreach (string processType in array)
		{
			One2OneRecipe one2OneRecipe = RecipaediaEXManager.FindMatchingRecipe<One2OneRecipe>(new One2OneRecipe
			{
				ProcessType = processType,
				Ingredient = ingredient,
				IngredientCount = slotCount
			});
			if (one2OneRecipe != null)
			{
				m_craftingRecipe = one2OneRecipe;
				break;
			}
		}
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
