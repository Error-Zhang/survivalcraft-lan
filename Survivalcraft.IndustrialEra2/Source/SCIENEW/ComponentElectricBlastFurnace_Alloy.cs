using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;
using ZLinq;

namespace SCIENEW;

public class ComponentElectricBlastFurnace_Alloy : ComponentAlloyFurnace, IUpdateable
{
	public SubsystemTime m_subsystemTime;

	public ComponentElectricBlastFurnace_Induction[] m_inductionFurnaces;

	public IAirBlowerComponent[] m_componentAirBlowers;

	public WorkState m_workState;

	public bool IsIntegrity { get; private set; }

	protected override float Speed => 0.5f;

	public bool IsWorking
	{
		get
		{
			if (IsIntegrity)
			{
				IAirBlowerComponent[] componentAirBlowers = m_componentAirBlowers;
				if (componentAirBlowers != null && componentAirBlowers.Length == 2 && m_componentAirBlowers.AsValueEnumerable().All((IAirBlowerComponent x) => x?.IsWorking() ?? false))
				{
					return m_workState == WorkState.Active;
				}
			}
			return false;
		}
	}

	public override int TanksCount => m_inductionFurnaces.Length;

	public override int GetTankValue(int tankIndex)
	{
		return GetInductionFurnace(tankIndex).GetTankValue(0);
	}

	public override float GetTankVolume(int tankIndex)
	{
		return GetInductionFurnace(tankIndex).GetTankVolume(0);
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		GetInductionFurnace(tankIndex).AddTankFluid(tankIndex, value, volume, out overflow);
	}

	public override float GetTankCapacity(int tankIndex, int value)
	{
		return GetInductionFurnace(tankIndex).GetTankCapacity(tankIndex, value);
	}

	public override bool GetTankScoopable(int tankIndex, int container)
	{
		return GetInductionFurnace(tankIndex).GetTankScoopable(tankIndex, container);
	}

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		return GetInductionFurnace(tankIndex).RemoveTankFluid(tankIndex, volume);
	}

	private ComponentElectricBlastFurnace_Induction GetInductionFurnace(int tankIndex)
	{
		return m_inductionFurnaces.AsValueEnumerable().First((ComponentElectricBlastFurnace_Induction x) => x.m_tankIndex == tankIndex);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_inductionFurnaces = Entity.FindComponents<ComponentElectricBlastFurnace_Induction>().AsValueEnumerable().ToArray();
	}

	public new void Update(float dt)
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
		VoltElement voltElement = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true).GetVoltElement(m_componentBlockEntity.Coordinates);
		if (voltElement == null)
		{
			m_workState = WorkState.Broken;
			return;
		}
		m_workState = voltElement.CurrentWorkState;
		if (IsWorking)
		{
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
				m_craftingProgress = MathUtils.Min(m_craftingProgress + Speed * dt, 1f);
				if (m_craftingProgress >= 1f)
				{
					EquationItem[] materialsItems = m_craftingRecipe.MaterialsItems;
					for (int i = 0; i < materialsItems.Length; i++)
					{
						EquationItem equationItem = materialsItems[i];
						int[] values = equationItem.Values;
						foreach (int value in values)
						{
							if (ComponentInventoryTankBase.FindRemoveSlotForItem(this, value, equationItem.Count) >= 0)
							{
								break;
							}
						}
					}
					EquationFluid[] materialsFluids = m_craftingRecipe.MaterialsFluids;
					for (int i = 0; i < materialsFluids.Length; i++)
					{
						EquationFluid equationFluid = materialsFluids[i];
						ComponentElectricBlastFurnace_Induction[] inductionFurnaces = m_inductionFurnaces;
						for (int j = 0; j < inductionFurnaces.Length && ComponentInventoryTankBase.FindRemoveTankFluid(inductionFurnaces[j], equationFluid.Value, equationFluid.Volume) < 0; j++)
						{
						}
					}
					materialsItems = m_craftingRecipe.ResultsItems;
					for (int i = 0; i < materialsItems.Length; i++)
					{
						EquationItem equationItem2 = materialsItems[i];
						int[] values = equationItem2.Values;
						foreach (int value2 in values)
						{
							if (ComponentInventoryTankBase.AcquireItems(this, value2, equationItem2.Count, ResultSlotIndex..(ResultSlotIndex + 1)) > 0)
							{
								break;
							}
						}
					}
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
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public void CheckIntegrity()
	{
		m_componentAirBlowers = new IAirBlowerComponent[2];
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int direction_Static = FourDirectionalDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z));
		Point3 point = coordinates - CellFace.FaceToPoint3(direction_Static);
		int num = -1;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (!CheckShellBlock(point.X + i, point.Y + num, point.Z + j))
				{
					IsIntegrity = false;
					return;
				}
			}
		}
		num = 0;
		for (int k = -1; k <= 1; k++)
		{
			for (int l = -1; l <= 1; l++)
			{
				if ((k != l || l != 0) && !(coordinates == new Point3(point.X + k, point.Y + num, point.Z + l)) && !CheckWireCubeBlock(point.X + k, point.Y + num, point.Z + l))
				{
					IsIntegrity = false;
					return;
				}
			}
		}
		num = 1;
		DevicesUtils.GetSideFaceOfXZ(direction_Static, out var face, out var face2);
		int num2 = 0;
		for (int m = -1; m <= 1; m++)
		{
			for (int n = -1; n <= 1; n++)
			{
				if (new Point3(m, 0, n) == CellFace.FaceToPoint3(face) || new Point3(m, 0, n) == CellFace.FaceToPoint3(face2))
				{
					if (!BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, point + new Point3(m, num, n), out ComponentBlockEntity blockEntity))
					{
						IsIntegrity = false;
						return;
					}
					m_componentAirBlowers[num2] = blockEntity.Entity.FindComponent<IAirBlowerComponent>();
					if (m_componentAirBlowers[num2] == null || m_componentAirBlowers[num2].PressureClass < AirBlowerPressureClass.Turbo)
					{
						IsIntegrity = false;
						return;
					}
					num2++;
				}
				else if (!CheckShellBlock(point.X + m, point.Y + num, point.Z + n))
				{
					IsIntegrity = false;
					return;
				}
			}
		}
		IsIntegrity = true;
		bool CheckShellBlock(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseNormalBlock.Index)
			{
				return NormalBlockManager.GetNormalBlockCraftingID(cellValueFast) == "AdvancedMechanicalShell";
			}
			return false;
		}
		bool CheckWireCubeBlock(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseDeviceBlock.Index)
			{
				return DevicesBlockManager.GetDevice(cellValueFast) is WireCubeDevice;
			}
			return false;
		}
	}

	public override void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int direction_Static = IlluminableDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<ElectricBlastFurnaceDevice>(IsWorking ? (4 | direction_Static) : direction_Static);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public override void FindEquation()
	{
		m_craftingRecipe = null;
		float requiredPlayerLevel = FindInteractingPlayer()?.PlayerData.Level ?? 1f;
		List<EquationItem> list = new List<EquationItem>();
		List<EquationFluid> list2 = new List<EquationFluid>();
		for (int i = 0; i < 3; i++)
		{
			if (GetTankValue(i) != 0)
			{
				list2.Add(new EquationFluid(GetTankValue(i), GetTankVolume(i)));
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (GetSlotValue(j) != 0)
			{
				list.Add(new EquationItem(new int[1] { GetSlotValue(j) }, GetSlotCount(j)));
			}
		}
		AlloyingEquationRecipe alloyingEquationRecipe = RecipaediaEXManager.FindMatchingRecipe<AlloyingEquationRecipe>(new AlloyingEquationRecipe
		{
			MaterialsItems = list.ToArray(),
			MaterialsFluids = list2.ToArray(),
			RequiredPlayerLevel = requiredPlayerLevel
		});
		if (alloyingEquationRecipe == null)
		{
			return;
		}
		EquationItem[] resultsItems = alloyingEquationRecipe.ResultsItems;
		for (int k = 0; k < resultsItems.Length; k++)
		{
			int[] values = resultsItems[k].Values;
			foreach (int value in values)
			{
				if (ComponentInventoryTankBase.FindAcquireSlotForItem(this, value, ResultSlotIndex..(ResultSlotIndex + 1)) == -1)
				{
					return;
				}
			}
		}
		m_craftingRecipe = alloyingEquationRecipe;
	}
}
