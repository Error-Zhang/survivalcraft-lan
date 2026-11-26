using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Utils;
using TemplatesDatabase;
using ZLinq;

namespace SCIENEW;

public class ComponentHearthFurnace : ComponentInventoryTankBase, IUpdateable, IHearthFurnacePortHost, ITank
{
	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public SubsystemExplosions m_subsystemExplosions;

	public SubsystemFluidTransfer m_subsystemFluidTransfer;

	public IFireBoxComponent[] m_componentFireBox = new IFireBoxComponent[2];

	public IAirBlowerComponent[] m_componentAirBlower = new IAirBlowerComponent[2];

	public Dictionary<Point3, int> m_ports = new Dictionary<Point3, int>();

	public float SmeltingSpeed = 1f;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public bool IsIntegrity { get; private set; }

	public bool IsWorking
	{
		get
		{
			if (IsIntegrity && m_componentAirBlower.AsValueEnumerable().All((IAirBlowerComponent x) => x?.IsWorking() ?? false))
			{
				return m_componentFireBox.AsValueEnumerable().All((IFireBoxComponent x) => x?.IsPreheating() ?? false);
			}
			return false;
		}
	}

	public int MoltenIronTankIndex => 0;

	public int MoltenSteelTankIndex => 1;

	public bool IsInputOpen { get; private set; } = true;

	public bool IsOutputOpen { get; private set; } = true;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		m_subsystemFluidTransfer = Project.FindSubsystem<SubsystemFluidTransfer>(throwOnError: true);
		IsInputOpen = valuesDictionary.GetValue("IsInputOpen", defaultValue: true);
		IsOutputOpen = valuesDictionary.GetValue("IsOutputOpen", defaultValue: true);
		UpdateVolume();
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("IsInputOpen", IsInputOpen);
		valuesDictionary.SetValue("IsOutputOpen", IsOutputOpen);
	}

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (Terrain.ExtractContents(value) != BaseItemBlock.Index || !(ItemsBlockManager.GetItemCraftingID(value) == "CokeCoalPowder"))
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (tankIndex == MoltenIronTankIndex && FluidsManager.GetFluid(value) is MoltenMetalFluid moltenMetalFluid && moltenMetalFluid.GetMaterial() == Materials.Iron)
		{
			return base.GetTankCapacity(tankIndex, value);
		}
		if (tankIndex == MoltenSteelTankIndex && FluidsManager.GetFluid(value) is MoltenMetalFluid moltenMetalFluid2 && moltenMetalFluid2.GetMaterial() == Materials.Steel)
		{
			return base.GetTankCapacity(tankIndex, value);
		}
		return 0f;
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
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
		UpdateSmelting(dt);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		foreach (KeyValuePair<Point3, int> port in m_ports)
		{
			HearthFurnaceOutputPortTransferrer transferrer2;
			if (m_subsystemFluidTransfer.TryGetTransferrer(port.Key, out HearthFurnaceInputPortTransferrer transferrer))
			{
				transferrer.ResetHost();
			}
			else if (m_subsystemFluidTransfer.TryGetTransferrer(port.Key, out transferrer2))
			{
				transferrer2.ResetHost();
			}
		}
		m_ports.Clear();
	}

	private void UpdateSmelting(float dt)
	{
		float num = SmeltingSpeed * dt;
		int fluidIDByMaterial = FluidsManager.GetFluidIDByMaterial<MoltenMetalFluid>(Materials.Steel);
		bool flag = m_slots.AsValueEnumerable().Any((Slot x) => x.Count > 0) && GetTankVolume(MoltenIronTankIndex) > 0f;
		bool flag2 = GetTankVolume(MoltenSteelTankIndex) + num >= GetTankCapacity(MoltenSteelTankIndex, fluidIDByMaterial);
		if (!(IsWorking & flag) || flag2)
		{
			return;
		}
		float volume = RemoveTankFluid(MoltenIronTankIndex, num);
		AddTankFluid(MoltenSteelTankIndex, fluidIDByMaterial, volume, out var _);
		if (m_subsystemTime.PeriodicGameTimeEvent(4.0, 0.0))
		{
			Slot[] array = (from x in m_slots.AsValueEnumerable()
				where x.Count > 0
				select x).ToArray();
			if (array.Length != 0)
			{
				int num2 = new Game.Random().Int(0, array.Length - 1);
				array[num2].Count--;
			}
		}
	}

	private void CheckIntegrity()
	{
		foreach (KeyValuePair<Point3, int> port in m_ports)
		{
			HearthFurnaceOutputPortTransferrer transferrer2;
			if (m_subsystemFluidTransfer.TryGetTransferrer(port.Key, out HearthFurnaceInputPortTransferrer transferrer))
			{
				transferrer.ResetHost();
			}
			else if (m_subsystemFluidTransfer.TryGetTransferrer(port.Key, out transferrer2))
			{
				transferrer2.ResetHost();
			}
		}
		m_ports.Clear();
		m_componentFireBox = new IFireBoxComponent[2];
		m_componentAirBlower = new IAirBlowerComponent[2];
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z));
		Point3 point = coordinates - CellFace.FaceToPoint3(direction_Static);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Point3 point2 = point + new Point3(i, -1, j);
				if (CheckFireBrick(point2.X, point2.Y, point2.Z))
				{
					continue;
				}
				DeviceBlock deviceBlock = DevicesUtils.FindDevice(m_subsystemTerrain, point2, out var blockValue);
				if (deviceBlock is HearthFurnaceInputPortDevice)
				{
					m_ports.Add(point2, blockValue);
					if (m_subsystemFluidTransfer.TryGetTransferrer(point2, out HearthFurnaceInputPortTransferrer transferrer3))
					{
						transferrer3.BindHost(this);
					}
					continue;
				}
				if (deviceBlock is HearthFurnaceOutputPortDevice)
				{
					m_ports.Add(point2, blockValue);
					if (m_subsystemFluidTransfer.TryGetTransferrer(point2, out HearthFurnaceOutputPortTransferrer transferrer4))
					{
						transferrer4.BindHost(this);
					}
					continue;
				}
				IsIntegrity = false;
				return;
			}
		}
		int[] array = new int[2];
		DevicesUtils.GetSideFaceOfXZ(direction_Static, out array[0], out array[1]);
		Point3 position = point - Point3.UnitY + CellFace.FaceToPoint3(array[0]) * 2;
		Point3 position2 = point - Point3.UnitY + CellFace.FaceToPoint3(array[1]) * 2;
		bool flag = false;
		bool flag2 = false;
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position, out ComponentBlockEntity blockEntity))
		{
			m_componentFireBox[0] = blockEntity.Entity.FindComponent<IFireBoxComponent>();
			if (m_componentFireBox[0] != null)
			{
				flag = true;
			}
		}
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position2, out ComponentBlockEntity blockEntity2))
		{
			m_componentFireBox[1] = blockEntity2.Entity.FindComponent<IFireBoxComponent>();
			if (m_componentFireBox[1] != null)
			{
				flag2 = true;
			}
		}
		if (!flag || !flag2)
		{
			IsIntegrity = false;
			return;
		}
		Point3 point3 = point - CellFace.FaceToPoint3(direction_Static);
		Point3 point4 = point + CellFace.FaceToPoint3(array[0]);
		Point3 point5 = point + CellFace.FaceToPoint3(array[1]);
		bool flag3 = m_subsystemTerrain.Terrain.GetCellContentsFast(point.X, point.Y, point.Z) == BlocksManager.GetBlockIndex<AirBlock>();
		bool flag4 = CheckSteelBlock(point.X - 1, point.Y, point.Z - 1);
		bool flag5 = CheckSteelBlock(point.X - 1, point.Y, point.Z + 1);
		bool flag6 = CheckSteelBlock(point.X + 1, point.Y, point.Z + 1);
		bool flag7 = CheckSteelBlock(point.X + 1, point.Y, point.Z - 1);
		bool flag8 = CheckSteelBlock(point3.X, point3.Y, point3.Z);
		bool flag9 = m_subsystemTerrain.Terrain.GetCellContentsFast(point4.X, point4.Y, point4.Z) == BlocksManager.GetBlockIndex<AirBlock>();
		bool flag10 = m_subsystemTerrain.Terrain.GetCellContentsFast(point5.X, point5.Y, point5.Z) == BlocksManager.GetBlockIndex<AirBlock>();
		if (!(flag3 & flag4 & flag5 & flag6 & flag7 & flag8 & flag9 & flag10))
		{
			IsIntegrity = false;
			return;
		}
		Point3 position3 = point + CellFace.FaceToPoint3(array[0]) * 2;
		Point3 position4 = point + CellFace.FaceToPoint3(array[1]) * 2;
		bool flag11 = false;
		bool flag12 = false;
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position3, out ComponentBlockEntity blockEntity3))
		{
			m_componentAirBlower[0] = blockEntity3.Entity.FindComponent<IAirBlowerComponent>();
			if (m_componentAirBlower[0] != null)
			{
				flag11 = true;
			}
		}
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position4, out ComponentBlockEntity blockEntity4))
		{
			m_componentAirBlower[1] = blockEntity4.Entity.FindComponent<IAirBlowerComponent>();
			if (m_componentAirBlower[1] != null)
			{
				flag12 = true;
			}
		}
		if (!flag11 || !flag12)
		{
			IsIntegrity = false;
			return;
		}
		for (int k = -1; k <= 1; k++)
		{
			for (int l = -1; l <= 1; l++)
			{
				Point3 point6 = point + new Point3(k, 1, l);
				if (!CheckSteelBlock(point6.X, point6.Y, point6.Z))
				{
					IsIntegrity = false;
					return;
				}
			}
		}
		IsIntegrity = true;
		bool CheckFireBrick(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseNormalBlock.Index)
			{
				return NormalBlockManager.GetNormalBlock(cellValueFast).GetCraftingId() == "FirebrickWall";
			}
			return false;
		}
		bool CheckSteelBlock(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseNormalBlock.Index && NormalBlockManager.GetNormalBlock(cellValueFast) is MetalNBlock metalNBlock)
			{
				return metalNBlock.GetMaterial() == Materials.Steel;
			}
			return false;
		}
	}

	private void UpdateVolume()
	{
		m_tanks[MoltenIronTankIndex].Capacity = 108f;
		m_tanks[MoltenIronTankIndex].Volume = MathUtils.Min(m_tanks[MoltenIronTankIndex].Volume, m_tanks[MoltenIronTankIndex].Capacity);
		m_tanks[MoltenSteelTankIndex].Capacity = 108f;
		m_tanks[MoltenSteelTankIndex].Volume = MathUtils.Min(m_tanks[MoltenSteelTankIndex].Volume, m_tanks[MoltenSteelTankIndex].Capacity);
	}

	private void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<OpenHearthFurnaceDevice>(IsWorking ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public void SwitchPortState(bool input, bool output)
	{
		if (input)
		{
			IsInputOpen = !IsInputOpen;
		}
		if (output)
		{
			IsOutputOpen = !IsOutputOpen;
		}
	}
}
