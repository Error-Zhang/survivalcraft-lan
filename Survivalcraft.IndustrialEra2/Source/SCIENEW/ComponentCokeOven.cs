using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Events;
using SCIENEW.Events;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using TemplatesDatabase;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW;

public class ComponentCokeOven : ComponentInventoryBase, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public SubsystemExplosions m_subsystemExplosions;

	public IFireBoxComponent m_componentFireBox;

	public IAirBlowerComponent m_componentAirBlower;

	public ComponentCokeOvenPort? m_componentCokeOvenPort;

	public Action OnRecipeFinished = () =>
	{
	};

	private CokingRecipe? m_craftingRecipe;

	private bool m_updateCraftingRecipe = true;

	public float m_craftingProgress;

	private float m_speed = 0.125f;

	public float m_ashesProbability = 0.1f;

	private int m_ashesValue;

	private ItemBlock m_ashesBlock;

	private new Game.Random m_random = new Game.Random();

	public int RemainsSlotIndex => SlotsCount - 2;

	public int ResultSlotIndex => SlotsCount - 1;

	public int AshesSlotIndex => SlotsCount - 3;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public bool IsIntegrity { get; private set; }

	public bool IsWorking
	{
		get
		{
			if (IsIntegrity && m_componentFireBox != null && m_componentAirBlower != null && m_componentFireBox.IsPreheating())
			{
				return m_componentAirBlower.IsWorking();
			}
			return false;
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		m_updateCraftingRecipe = true;
		m_ashesValue = ItemsBlockManager.GetItemID("Ashes");
		m_ashesBlock = ItemsBlockManager.GetItem("Ashes");
	}

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex < AshesSlotIndex)
		{
			if (m_slots.AsValueEnumerable().Take(AshesSlotIndex).All<Take<FromList<Slot>, Slot>, Slot>((Func<Slot, bool>)((Slot x) => x.Count == 0)))
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			int value2 = m_slots.AsValueEnumerable().Take(AshesSlotIndex).First<Take<FromList<Slot>, Slot>, Slot>((Func<Slot, bool>)((Slot x) => x.Count > 0))
				.Value;
			if (value == value2)
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			return 0;
		}
		if (slotIndex == ResultSlotIndex || slotIndex == RemainsSlotIndex)
		{
			return 0;
		}
		if (slotIndex == AshesSlotIndex)
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
			RecipaediaEventBus.GetPublisher<CrafterOutputRemovedEvent>().Publish(new CrafterOutputRemovedEvent(Project, FindInteractingPlayer(), num, num2, "CokeOven"));
		}
		return num2;
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		m_componentCokeOvenPort?.RemoveCokeOven();
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
		UpdateSmelting(dt);
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	private void UpdateSmelting(float dt)
	{
		if (IsWorking)
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
				if (m_craftingProgress <= 0f && !RecipaediaInterceptBus.TryProceed(new IndustrialCrafterCycleStartingContext(Project, this, m_componentBlockEntity.Coordinates, "CokeOven", m_craftingRecipe)))
				{
					return;
				}
				m_craftingProgress = MathUtils.Min(m_craftingProgress + m_speed * dt, 1f);
				if (m_craftingProgress >= 1f)
				{
					int consume = MathUtils.Max(1, m_craftingRecipe.IngredientCount);
					Slot slot = m_slots.AsValueEnumerable().FirstOrDefault((Slot x) => x.Count >= consume, null);
					if (slot != null)
					{
						slot.Count -= consume;
					}
					m_slots[ResultSlotIndex].Value = m_craftingRecipe.ResultID;
					m_slots[ResultSlotIndex].Count += m_craftingRecipe.ResultCount;
					m_craftingRecipe = null;
					m_craftingProgress = 0f;
					m_updateCraftingRecipe = true;
					OnRecipeFinished?.Invoke();
				}
				if (m_subsystemTime.PeriodicGameTimeEvent(1.0, (double)GetHashCode() % 1000.0 / 1000.0) && m_random.Float() < m_ashesProbability)
				{
					m_random.Seed();
					m_slots[AshesSlotIndex].Value = m_ashesValue;
					m_slots[AshesSlotIndex].Count = Math.Min(m_slots[AshesSlotIndex].Count + 1, m_ashesBlock.GetMaxStacking(m_ashesValue));
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

	private void CheckIntegrity()
	{
		m_componentFireBox = null;
		m_componentAirBlower = null;
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z));
		Point3 point = coordinates - CellFace.FaceToPoint3(direction_Static);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Point3 point2 = point + new Point3(i, -1, j);
				if (!CheckFireBrick(point2.X, point2.Y, point2.Z))
				{
					IsIntegrity = false;
					return;
				}
			}
		}
		int[] array = new int[2];
		int face = array[0];
		DevicesUtils.GetSideFaceOfXZ(direction_Static, out array[0], out array[1]);
		int[] array2 = array;
		foreach (int num in array2)
		{
			Point3 position = point - Point3.UnitY + CellFace.FaceToPoint3(num) * 2;
			if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position, out ComponentBlockEntity blockEntity))
			{
				m_componentFireBox = blockEntity.Entity.FindComponent<IFireBoxComponent>();
				if (m_componentFireBox != null)
				{
					face = num;
					break;
				}
			}
		}
		if (m_componentFireBox == null)
		{
			IsIntegrity = false;
			return;
		}
		Point3 point3 = point - CellFace.FaceToPoint3(direction_Static);
		Point3 point4 = point - CellFace.FaceToPoint3(face);
		Point3 point5 = point + CellFace.FaceToPoint3(face);
		bool flag = m_subsystemTerrain.Terrain.GetCellContentsFast(point.X, point.Y, point.Z) == BlocksManager.GetBlockIndex<AirBlock>();
		bool flag2 = CheckFireBrick(point.X - 1, point.Y, point.Z - 1);
		bool flag3 = CheckFireBrick(point.X - 1, point.Y, point.Z + 1);
		bool flag4 = CheckFireBrick(point.X + 1, point.Y, point.Z + 1);
		bool flag5 = CheckFireBrick(point.X + 1, point.Y, point.Z - 1);
		bool flag6 = CheckFireBrick(point3.X, point3.Y, point3.Z);
		bool flag7 = CheckFireBrick(point4.X, point4.Y, point4.Z);
		bool flag8 = m_subsystemTerrain.Terrain.GetCellContentsFast(point5.X, point5.Y, point5.Z) == BlocksManager.GetBlockIndex<AirBlock>();
		if (!(flag & flag2 & flag3 & flag4 & flag5 & flag6 & flag7 & flag8))
		{
			IsIntegrity = false;
			return;
		}
		Point3 position2 = point + CellFace.FaceToPoint3(face) * 2;
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position2, out ComponentBlockEntity blockEntity2))
		{
			m_componentAirBlower = blockEntity2.Entity.FindComponent<IAirBlowerComponent>(throwOnError: false);
			if (m_componentAirBlower == null)
			{
				IsIntegrity = false;
				return;
			}
			for (int l = -1; l <= 1; l++)
			{
				for (int m = -1; m <= 1; m++)
				{
					Point3 position3 = point + new Point3(l, 1, m);
					if (CheckFireBrick(position3.X, position3.Y, position3.Z))
					{
						continue;
					}
					if (l == 0 && m == 0 && CheckPort(position3.X, position3.Y, position3.Z))
					{
						if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position3, out ComponentBlockEntity blockEntity3))
						{
							ComponentCokeOvenPort componentCokeOvenPort = blockEntity3.Entity.FindComponent<ComponentCokeOvenPort>();
							if (componentCokeOvenPort != m_componentCokeOvenPort)
							{
								m_componentCokeOvenPort?.RemoveCokeOven();
								componentCokeOvenPort?.AssignCokeOven(this);
								m_componentCokeOvenPort = componentCokeOvenPort;
							}
						}
						continue;
					}
					IsIntegrity = false;
					return;
				}
			}
			IsIntegrity = true;
		}
		else
		{
			IsIntegrity = false;
		}
		bool CheckFireBrick(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseNormalBlock.Index)
			{
				return NormalBlockManager.GetNormalBlock(cellValueFast).GetCraftingId() == "FirebrickWall";
			}
			return false;
		}
		bool CheckPort(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseDeviceBlock.Index)
			{
				return DevicesBlockManager.GetDevice(cellValueFast).GetCraftingId() == "CokeOvenPortDevice";
			}
			return false;
		}
	}

	private void FindRecipe()
	{
		m_craftingRecipe = null;
		for (int i = 0; i < AshesSlotIndex; i++)
		{
			Slot slot = m_slots[i];
			if (slot.Count == 0)
			{
				continue;
			}
			CokingRecipe cokingRecipe = RecipaediaEXManager.FindMatchingRecipe<CokingRecipe>(new CokingRecipe
			{
				Ingredient = RecipesUtils.BlockToCratingID(slot.Value),
				IngredientCount = slot.Count
			});
			if (cokingRecipe != null)
			{
				Slot slot2 = m_slots[ResultSlotIndex];
				int num = Terrain.ExtractContents(cokingRecipe.ResultID);
				if (slot2.Count == 0 || (cokingRecipe.ResultID == slot2.Value && cokingRecipe.ResultCount + slot2.Count <= BlocksManager.Blocks[num].GetMaxStacking(cokingRecipe.ResultID)))
				{
					m_craftingRecipe = cokingRecipe;
					break;
				}
			}
		}
	}

	private void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<CokeOvenDevice>(IsWorking ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => EnumerateInputSlots(),
			ProductionSlotKind.Output => new _003C_003Ez__ReadOnlyArray<int>(new int[3] { ResultSlotIndex, RemainsSlotIndex, AshesSlotIndex }),
			_ => m_emptySlotIndices,
		};
	}

	private IEnumerable<int> EnumerateInputSlots()
	{
		for (int i = 0; i < AshesSlotIndex; i++)
		{
			yield return i;
		}
	}
}
