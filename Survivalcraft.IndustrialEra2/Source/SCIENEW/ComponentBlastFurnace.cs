using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.ProductionIO;
using SCIENEW.Utils;
using TemplatesDatabase;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW;

public class ComponentBlastFurnace : ComponentInventoryTankBase, IUpdateable, IBlastFurnacePortHost, ITank, IInventoryProductionSlots, ITankProductionSlots
{
	private static readonly int[] m_inputSlotIndices = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

	private static readonly int[] m_consumeSlotIndices = new int[3] { 8, 9, 10 };

	private static readonly int[] m_outputTankIndices = new int[1];

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	private static readonly int[] m_emptyTankIndices = Array.Empty<int>();

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public SubsystemFluidTransfer m_subsystemFluidTransfer;

	public SubsystemExplosions m_subsystemExplosions;

	public Dictionary<Point3, int> m_ports = new Dictionary<Point3, int>();

	public IFireBoxComponent m_componentFireBox;

	public IAirBlowerComponent m_componentAirBlower;

	public int m_height;

	public float m_progress;

	private int m_previousFluidID;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

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

	public bool IsIntegrity { get; private set; }

	public bool IsPortOpen { get; private set; }

	bool IBlastFurnacePortHost.IsBlastPortOpen => IsPortOpen;

	public int MetalSlotIndex => 0;

	public int CatSlotIndex => 8;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemFluidTransfer = Project.FindSubsystem<SubsystemFluidTransfer>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		IsPortOpen = valuesDictionary.GetValue("IsPortOpen", defaultValue: false);
		m_tanks[0].Capacity = 0f;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("IsPortOpen", IsPortOpen);
	}

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex >= MetalSlotIndex && slotIndex < CatSlotIndex)
		{
			if (CastingUtils.GetIronItemVolume(value) > 0f)
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			return 0;
		}
		if (slotIndex >= CatSlotIndex)
		{
			if (!(GetCatEfficiency(value) > 0f))
			{
				return 0;
			}
			return base.GetSlotCapacity(slotIndex, value);
		}
		return base.GetSlotCapacity(slotIndex, value);
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
			UpdateVolume();
		}
		UpdateMelting(dt);
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
		if (m_subsystemTime.PeriodicGameTimeEvent(0.5, 0.0) && IsIntegrity)
		{
			UpdateMoltenIron(placeMode: true);
		}
		UpdateFluidChange();
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		foreach (KeyValuePair<Point3, int> port in m_ports)
		{
			if (m_subsystemFluidTransfer.TryGetTransferrer(port.Key, out BlastPortTransferrer transferrer))
			{
				transferrer.ResetHost();
			}
		}
		m_ports.Clear();
	}

	private void CheckIntegrity()
	{
		foreach (KeyValuePair<Point3, int> port in m_ports)
		{
			if (m_subsystemFluidTransfer.TryGetTransferrer(port.Key, out BlastPortTransferrer transferrer))
			{
				transferrer.ResetHost();
			}
		}
		m_ports.Clear();
		m_componentFireBox = null;
		m_componentAirBlower = null;
		m_height = 0;
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z));
		Point3 point = coordinates - CellFace.FaceToPoint3(direction_Static);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Point3 point2 = new Point3(point.X - 1 + i, point.Y - 1, point.Z - 1 + j);
				int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(point2.X, point2.Y, point2.Z);
				if (Terrain.ExtractContents(cellValueFast) != BricksBlock.Index)
				{
					if (Terrain.ExtractContents(cellValueFast) != BaseDeviceBlock.Index || !(DevicesBlockManager.GetDevice(cellValueFast) is BlastFurnacePortDevice))
					{
						IsIntegrity = false;
						return;
					}
					m_ports.Add(point2, cellValueFast);
					if (m_subsystemFluidTransfer.TryGetTransferrer(point2, out BlastPortTransferrer transferrer2))
					{
						transferrer2.BindHost(this);
					}
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
		bool num2 = m_subsystemTerrain.Terrain.GetCellContentsFast(point.X, point.Y, point.Z) == BlocksManager.GetBlockIndex<AirBlock>() || m_subsystemTerrain.Terrain.GetCellContentsFast(point.X, point.Y, point.Z) == BlocksManager.GetBlockIndex<MoltenIronBlock>();
		bool flag = m_subsystemTerrain.Terrain.GetCellContentsFast(point.X - 1, point.Y, point.Z - 1) == BricksBlock.Index;
		bool flag2 = m_subsystemTerrain.Terrain.GetCellContentsFast(point.X - 1, point.Y, point.Z + 1) == BricksBlock.Index;
		bool flag3 = m_subsystemTerrain.Terrain.GetCellContentsFast(point.X + 1, point.Y, point.Z + 1) == BricksBlock.Index;
		bool flag4 = m_subsystemTerrain.Terrain.GetCellContentsFast(point.X + 1, point.Y, point.Z - 1) == BricksBlock.Index;
		bool flag5 = m_subsystemTerrain.Terrain.GetCellContentsFast(point3.X, point3.Y, point3.Z) == BricksBlock.Index;
		bool flag6 = m_subsystemTerrain.Terrain.GetCellContentsFast(point4.X, point4.Y, point4.Z) == BricksBlock.Index;
		bool flag7 = m_subsystemTerrain.Terrain.GetCellContentsFast(point5.X, point5.Y, point5.Z) == BlocksManager.GetBlockIndex<AirBlock>() || m_subsystemTerrain.Terrain.GetCellContentsFast(point.X, point.Y, point.Z) == BlocksManager.GetBlockIndex<MoltenIronBlock>();
		if (!(num2 & flag & flag2 & flag3 & flag4 & flag5 & flag6 & flag7))
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
			Point3[] points = new Point3[8]
			{
				new Point3(-1, 0, -1),
				new Point3(-1, 0, 0),
				new Point3(-1, 0, 1),
				new Point3(0, 0, -1),
				new Point3(0, 0, 1),
				new Point3(1, 0, -1),
				new Point3(1, 0, 0),
				new Point3(1, 0, 1)
			};
			int l;
			for (l = 1; CheckChimney(points, point + Point3.UnitY * l); l++)
			{
			}
			l--;
			if (l < 2)
			{
				IsIntegrity = false;
				return;
			}
			m_height = l;
			IsIntegrity = true;
		}
		else
		{
			IsIntegrity = false;
		}
		bool CheckChimney(Point3[] array3, Point3 center)
		{
			if (m_subsystemTerrain.Terrain.GetCellContentsFast(center.X, center.Y, center.Z) != AirBlock.Index && m_subsystemTerrain.Terrain.GetCellContentsFast(center.X, center.Y, center.Z) != MoltenIronBlock.Index)
			{
				return false;
			}
			foreach (Point3 point6 in array3)
			{
				Point3 point7 = center + point6;
				if (m_subsystemTerrain.Terrain.GetCellContentsFast(point7.X, point7.Y, point7.Z) != BricksBlock.Index)
				{
					return false;
				}
			}
			return true;
		}
	}

	private void UpdateVolume()
	{
		m_tanks[0].Capacity = (float)MathUtils.Max(m_height - 1, 0) * 36f;
		m_tanks[0].Volume = MathUtils.Min(m_tanks[0].Volume, m_tanks[0].Capacity);
	}

	private void UpdateMelting(float dt)
	{
		float num = 5f;
		float num2 = 1f;
		bool flag = m_slots.AsValueEnumerable().Take(CatSlotIndex).Any<Take<FromList<Slot>, Slot>, Slot>((Func<Slot, bool>)((Slot x) => x.Count > 0));
		bool flag2 = GetTankVolume(0) >= GetTankCapacity(0, GetTankValue(0));
		if ((IsWorking & flag) && !flag2)
		{
			float f = CalcCatsEfficiency();
			num = MathUtils.Lerp(num, num + 3f, f);
			if (m_progress >= 1f)
			{
				m_progress = 0f;
				int fluidID = FluidsManager.GetFluidID("MoltenIron", 0);
				int num3 = -1;
				float num4 = 0f;
				bool flag3 = false;
				while ((GetSlotCount(num3) == 0 || ((num4 <= 0f) | flag3)) && num3 < CatSlotIndex)
				{
					num3++;
					if (GetSlotCount(num3) <= 0)
					{
						continue;
					}
					int slotValue = GetSlotValue(num3);
					if (Terrain.ExtractContents(slotValue) == BaseItemBlock.Index)
					{
						if (ItemsBlockManager.GetItem(slotValue) is ScrapIronItem)
						{
							num2 = MathUtils.Lerp(0.75f, 0.9f, f);
						}
						else if (ItemsBlockManager.GetItem(slotValue) is OrePowderItem)
						{
							num2 = MathUtils.Lerp(1.5f, 2f, f);
						}
					}
					num4 = CastingUtils.GetIronItemVolume(slotValue) * num2;
					flag3 = GetTankVolume(0) + num4 > GetTankCapacity(0, fluidID);
				}
				if (num3 >= CatSlotIndex || flag3)
				{
					return;
				}
				m_slots[num3].Count--;
				AddTankFluid(0, fluidID, num4, out var _);
				if (new Game.Random().Float(0f, 1f) < 0.25f)
				{
					Slot[] array = m_slots.AsValueEnumerable().Take(8..11).Where<TakeRange<FromList<Slot>, Slot>, Slot>((Func<Slot, bool>)((Slot x) => x.Count > 0))
						.ToArray<TakeRange<FromList<Slot>, Slot>, Slot>();
					if (array.Length != 0)
					{
						int num5 = new Game.Random().Int(0, array.Length - 1);
						array[num5].Count--;
					}
				}
			}
			else
			{
				m_progress += dt * num;
			}
		}
		else
		{
			m_progress = 0f;
		}
	}

	private float GetCatEfficiency(int value)
	{
		int num = Terrain.ExtractContents(value);
		if (num == SandBlock.Index)
		{
			return 0.2f;
		}
		if (num == PigmentBlock.Index)
		{
			return 0.3f;
		}
		if (num == BaseItemBlock.Index && ItemsBlockManager.GetItem(value).GetCraftingId() == "CoalPowder")
		{
			return 0.5f;
		}
		return 0f;
	}

	private float CalcCatsEfficiency()
	{
		HashSet<int> hashSet = m_slots.AsValueEnumerable().Take(8..11).Where<TakeRange<FromList<Slot>, Slot>, Slot>((Func<Slot, bool>)((Slot x) => x.Count > 0))
			.Select<TakeRange<FromList<Slot>, Slot>, Slot, int>((Func<Slot, int>)((Slot x) => x.Value))
			.ToHashSet<WhereSelect<TakeRange<FromList<Slot>, Slot>, Slot, int>, int>();
		float num = 0f;
		foreach (int item in hashSet)
		{
			num += GetCatEfficiency(item);
		}
		return num;
	}

	private void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<BlastFurnaceDevice>(IsWorking ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	private void UpdateMoltenIron(bool placeMode)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z);
		bool flag;
		if (chunkAtCell != null)
		{
			TerrainChunkState state = chunkAtCell.State;
			if ((uint)(state - 5) <= 1u || state == TerrainChunkState.Valid)
			{
				flag = true;
				goto IL_0044;
			}
		}
		flag = false;
		goto IL_0044;
		IL_0044:
		if (!flag)
		{
			return;
		}
		int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z));
		Point3 point = coordinates - CellFace.FaceToPoint3(direction_Static);
		for (int i = 0; i < m_height + 1; i++)
		{
			Point3 point2 = point + Point3.UnitY * i;
			float num = Math.Clamp(m_tanks[0].Volume - (float)i * 36f, 0f, 36f);
			if (placeMode)
			{
				m_subsystemTerrain.ChangeCell(point2.X, point2.Y, point2.Z, (num != 0f) ? Terrain.MakeBlockValue(MoltenIronBlock.Index, m_subsystemTerrain.Terrain.GetCellLight(point2.X, point2.Y, point2.Z), FocalorsFluidBlock.SetIsTop(FocalorsFluidBlock.SetLevel(0, 1f - num / 36f), isTop: true)) : 0);
			}
			else
			{
				m_subsystemTerrain.Terrain.SetCellValueFast(point2.X, point2.Y, point2.Z, (num != 0f) ? Terrain.MakeBlockValue(MoltenIronBlock.Index, m_subsystemTerrain.Terrain.GetCellLight(point2.X, point2.Y, point2.Z), FocalorsFluidBlock.SetIsTop(FocalorsFluidBlock.SetLevel(0, 1f - num / 36f), isTop: true)) : 0);
			}
		}
	}

	public void SwitchPortState()
	{
		IsPortOpen = !IsPortOpen;
	}

	public void UpdateFluidChange()
	{
		int tankValue = GetTankValue(0);
		if (tankValue != m_previousFluidID)
		{
			m_previousFluidID = tankValue;
			OnFluidIDChange(tankValue);
		}
	}

	private void OnFluidIDChange(int newValue)
	{
		if (newValue == FluidsManager.GetFluidID("MoltenIron", 0) || newValue == 0)
		{
			return;
		}
		ComponentPlayer componentPlayer = FindInteractingPlayer();
		if (componentPlayer != null)
		{
			Task.Run(() =>
			{
				componentPlayer.ComponentGui.DisplayLargeMessage(LanguageUtils.GetText(this, "EasterEggTitle"), LanguageUtils.GetText(this, "EasterEggContent"), 5f, 0f);
				Thread.Sleep(1000);
				componentPlayer.ComponentGui.DisplaySmallMessage("5", Color.White, blinking: true, playNotificationSound: true);
				Thread.Sleep(1000);
				componentPlayer.ComponentGui.DisplaySmallMessage("4", Color.White, blinking: true, playNotificationSound: true);
				Thread.Sleep(1000);
				componentPlayer.ComponentGui.DisplaySmallMessage("3", Color.White, blinking: true, playNotificationSound: true);
				Thread.Sleep(1000);
				componentPlayer.ComponentGui.DisplaySmallMessage("2", Color.White, blinking: true, playNotificationSound: true);
				Thread.Sleep(1000);
				componentPlayer.ComponentGui.DisplaySmallMessage("1", Color.White, blinking: true, playNotificationSound: true);
				Thread.Sleep(1000);
				m_subsystemExplosions.TryExplodeBlock(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, MediumIncendiaryKegBlock.Index);
			});
		}
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => (IEnumerable<int>)m_inputSlotIndices,
			ProductionSlotKind.Consume => m_consumeSlotIndices,
			_ => m_emptySlotIndices,
		};
	}

	IEnumerable<int> ITankProductionSlots.GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Output)
		{
			return m_outputTankIndices;
		}
		return m_emptyTankIndices;
	}
}
