using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.Extensions;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Extensions.Graphics;
using SCIENEW.ProductionIO;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentFractionalTower : ComponentTankBase, IUpdateable, IDrawable, IFractionalTowerPortHost, ITank, ITankProductionSlots
{
	private static readonly int[] m_inputTankIndices = new int[1];

	private static readonly int[] m_emptyTankIndices = Array.Empty<int>();

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public SubsystemExplosions m_subsystemExplosions;

	public SubsystemFluidTransfer m_subsystemFluidTransfer;

	public IFireBoxComponent? m_componentFireBox;

	public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

	public TexturedBatch3D m_batch;

	public DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();

	public Fluid m_containsFluid;

	public Dictionary<Point3, int> m_ports = new Dictionary<Point3, int>();

	public bool m_updateEquation;

	private FractionatingEquationRecipe? m_equation;

	public float m_speed = 0.25f;

	public int m_lastFluidID = -1;

	public int[] DrawOrders => new int[1] { 1500 };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int PetroleumTankIndex => 0;

	public bool IsIntegrity { get; private set; }

	public bool IsWorking
	{
		get
		{
			if (IsIntegrity && m_componentFireBox != null)
			{
				return m_componentFireBox.IsPreheating();
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
		m_subsystemFluidTransfer = Project.FindSubsystem<SubsystemFluidTransfer>(throwOnError: true);
		UpdateVolume();
		m_updateEquation = true;
	}

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (tankIndex == PetroleumTankIndex)
		{
			if (value != FluidsManager.GetFluidID("PetroleumFluid", 0))
			{
				return 0f;
			}
			return base.GetTankCapacity(tankIndex, value);
		}
		return base.GetTankCapacity(tankIndex, value);
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		base.AddTankFluid(tankIndex, value, volume, out overflow);
		if (tankIndex == PetroleumTankIndex)
		{
			m_updateEquation = true;
		}
	}

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		if (tankIndex == PetroleumTankIndex && volume >= GetTankVolume(PetroleumTankIndex))
		{
			m_updateEquation = true;
		}
		return base.RemoveTankFluid(tankIndex, volume);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		foreach (KeyValuePair<Point3, int> port in m_ports)
		{
			if (m_subsystemFluidTransfer.TryGetTransferrer(port.Key, out FractionalTowerPortTransferrer transferrer))
			{
				transferrer.ResetHost();
			}
		}
		m_ports.Clear();
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
		UpdateFractional(dt);
		if (m_tanks[0].Value != m_lastFluidID)
		{
			m_lastFluidID = m_tanks[0].Value;
			OnSwitchFluid();
		}
	}

	public void OnSwitchFluid()
	{
		m_containsFluid = FluidsManager.GetFluid(m_tanks[0].Value);
		m_batch = m_primitivesRenderer.TexturedBatch(m_containsFluid.GetTexture(m_tanks[PetroleumTankIndex].Value), useAlphaTest: false, 0, null, null, null, SamplerState.PointWrap);
	}

	private void UpdateFractional(float dt)
	{
		if (m_updateEquation)
		{
			m_updateEquation = false;
			FindEquation();
		}
		if (!IsWorking || m_equation == null || !(GetTankVolume(PetroleumTankIndex) > 0f))
		{
			return;
		}
		for (int i = 0; i < m_equation.ResultsFluids.Length; i++)
		{
			if ((GetTankVolume(i + PetroleumTankIndex + 1) > 0f && GetTankValue(i + PetroleumTankIndex + 1) != m_equation.ResultsFluids[i].Value) || GetTankVolume(i + PetroleumTankIndex + 1) >= GetTankOriginalCapacity(i + PetroleumTankIndex + 1))
			{
				return;
			}
		}
		RemoveTankFluid(PetroleumTankIndex, m_speed * dt);
		for (int j = 0; j < m_equation.ResultsFluids.Length; j++)
		{
			m_tanks[j + PetroleumTankIndex + 1].Value = m_equation.ResultsFluids[j].Value;
			m_tanks[j + PetroleumTankIndex + 1].Volume = MathUtils.Min(GetTankOriginalCapacity(j + 1), GetTankVolume(j + 1) + m_speed * m_equation.ResultsFluids[j].Volume * dt);
		}
	}

	private void CheckIntegrity()
	{
		m_componentFireBox = null;
		foreach (KeyValuePair<Point3, int> port in m_ports)
		{
			if (m_subsystemFluidTransfer.TryGetTransferrer(port.Key, out FractionalTowerPortTransferrer transferrer))
			{
				transferrer.ResetHost();
			}
		}
		m_ports.Clear();
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		Point3[] array = new Point3[4]
		{
			new Point3(-1, 0, -1),
			new Point3(-1, 0, 1),
			new Point3(1, 0, 1),
			new Point3(1, 0, -1)
		};
		Point3[] array2 = array;
		foreach (Point3 point in array2)
		{
			Point3 point2 = coordinates + point - Point3.UnitY;
			if (!CheckShellBlock(point2.X, point2.Y, point2.Z))
			{
				IsIntegrity = false;
				return;
			}
		}
		Point3 position = coordinates - Point3.UnitY;
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position, out ComponentBlockEntity blockEntity))
		{
			m_componentFireBox = blockEntity.Entity.FindComponent<IFireBoxComponent>();
			if (m_componentFireBox == null)
			{
				IsIntegrity = false;
				return;
			}
			array2 = array;
			foreach (Point3 point3 in array2)
			{
				Point3 point4 = coordinates + point3;
				if (!CheckShellBlock(point4.X, point4.Y, point4.Z))
				{
					IsIntegrity = false;
					return;
				}
			}
			Point3[] array3 = new Point3[4]
			{
				Point3.UnitX,
				Point3.UnitZ,
				-Point3.UnitX,
				-Point3.UnitZ
			};
			for (int j = 1; j <= 3; j++)
			{
				array2 = array;
				foreach (Point3 point5 in array2)
				{
					Point3 point6 = coordinates + point5 + Point3.UnitY * j;
					if (!CheckShellBlock(point6.X, point6.Y, point6.Z))
					{
						IsIntegrity = false;
						return;
					}
				}
				array2 = array3;
				foreach (Point3 point7 in array2)
				{
					Point3 point8 = coordinates + point7 + Point3.UnitY * j;
					if (!CheckShellBlock(point8.X, point8.Y, point8.Z))
					{
						if (!(DevicesUtils.FindDevice(m_subsystemTerrain, point8, out var blockValue) is FractionalPortDevice))
						{
							IsIntegrity = false;
							return;
						}
						m_ports.Add(point8, blockValue);
						if (m_subsystemFluidTransfer.TryGetTransferrer(point8, out FractionalTowerPortTransferrer transferrer2))
						{
							transferrer2.BindHost(this, j);
						}
					}
				}
				Point3 point9 = coordinates + new Point3(0, j, 0);
				if (!CheckAir(point9.X, point9.Y, point9.Z))
				{
					IsIntegrity = false;
					return;
				}
			}
			for (int k = -1; k <= 1; k++)
			{
				for (int l = -1; l <= 1; l++)
				{
					Point3 point10 = coordinates + new Point3(k, 4, l);
					if (!CheckShellBlock(point10.X, point10.Y, point10.Z))
					{
						IsIntegrity = false;
						return;
					}
				}
			}
			IsIntegrity = true;
		}
		else
		{
			IsIntegrity = false;
		}
		bool CheckAir(int x, int y, int z)
		{
			return Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z)) == AirBlock.Index;
		}
		bool CheckShellBlock(int x, int y, int z)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			if (Terrain.ExtractContents(cellValueFast) == BaseNormalBlock.Index)
			{
				return cellValueFast == NormalBlockManager.GetNormalBlockID("BasicMechanicalShell");
			}
			return false;
		}
	}

	private void UpdateVolume()
	{
		m_tanks[PetroleumTankIndex].Capacity = 12f;
		m_tanks[PetroleumTankIndex].Volume = MathUtils.Min(m_tanks[PetroleumTankIndex].Volume, m_tanks[PetroleumTankIndex].Capacity);
		for (int i = PetroleumTankIndex + 1; i < TanksCount; i++)
		{
			m_tanks[i].Capacity = 12f;
			m_tanks[i].Volume = MathUtils.Min(m_tanks[i].Volume, m_tanks[i].Capacity);
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (m_subsystemTerrain.IsVisible(camera, new Vector3(m_componentBlockEntity.Coordinates)))
		{
			if (m_containsFluid != null && GetTankVolume(PetroleumTankIndex) > 0f)
			{
				m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, m_componentBlockEntity.Coordinates);
				float num = 0.0625f;
				float x = 0.9375f;
				float num2 = MathUtils.Lerp(num, x, GetTankVolume(PetroleumTankIndex) / GetTankOriginalCapacity(PetroleumTankIndex));
				Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates);
				Vector3 vector2 = vector + new Vector3(0.075f, num, 0.075f);
				Vector3 vector3 = vector + new Vector3(0.075f, num, 0.925f);
				Vector3 vector4 = vector + new Vector3(0.925f, num, 0.925f);
				Vector3 vector5 = vector + new Vector3(0.925f, num, 0.075f);
				Vector3 vector6 = vector + new Vector3(0.075f, num2, 0.075f);
				Vector3 vector7 = vector + new Vector3(0.075f, num2, 0.925f);
				Vector3 vector8 = vector + new Vector3(0.925f, num2, 0.925f);
				Vector3 vector9 = vector + new Vector3(0.925f, num2, 0.075f);
				int value = m_tanks[0].Value;
				float num3 = 1f / (float)m_containsFluid.GetTextureSubdivision(value);
				int textureSlot = m_containsFluid.GetTextureSlot(value);
				Vector2 texCoord;
				Vector2 vector10 = (texCoord = new Vector2(num3 * (float)(textureSlot % m_containsFluid.GetTextureSubdivision(value)), num3 * (float)(textureSlot / m_containsFluid.GetTextureSubdivision(value))));
				Vector2 texCoord2 = vector10 + Vector2.UnitX * num3;
				Vector2 texCoord3 = vector10 + Vector2.One * num3;
				Vector2 texCoord4 = vector10 + Vector2.UnitY * num3;
				float num4 = (num2 - num) / 0.85f;
				Vector2 texCoord5 = vector10 + new Vector2(num3, num3 * num4);
				Vector2 texCoord6 = vector10 + new Vector2(0f, num3 * num4);
				float s = LightingManager.LightIntensityByLightValue[m_drawBlockEnvironmentData.Light];
				Color c = (m_containsFluid.IsGlow(value) ? m_containsFluid.GetColor(value) : Color.MultiplyColorOnly(m_containsFluid.GetColor(value), s));
				Vector3 normal = (vector3 - vector2).Cross(vector6 - vector2).Normalize();
				m_batch.QueueQuad(vector2, vector3, vector7, vector6, texCoord, texCoord2, texCoord5, texCoord6, Color.MultiplyColorOnly(c, LightingManager.CalculateLighting(normal)));
				normal = (vector4 - vector3).Cross(vector7 - vector3).Normalize();
				m_batch.QueueQuad(vector3, vector4, vector8, vector7, texCoord, texCoord2, texCoord5, texCoord6, Color.MultiplyColorOnly(c, LightingManager.CalculateLighting(normal)));
				normal = (vector5 - vector4).Cross(vector8 - vector4).Normalize();
				m_batch.QueueQuad(vector4, vector5, vector9, vector8, texCoord, texCoord2, texCoord5, texCoord6, Color.MultiplyColorOnly(c, LightingManager.CalculateLighting(normal)));
				normal = (vector2 - vector5).Cross(vector9 - vector5).Normalize();
				m_batch.QueueQuad(vector5, vector2, vector6, vector9, texCoord, texCoord2, texCoord5, texCoord6, Color.MultiplyColorOnly(c, LightingManager.CalculateLighting(normal)));
				m_batch.QueueQuad(vector2, vector3, vector4, vector5, texCoord, texCoord2, texCoord3, texCoord4, Color.MultiplyColorOnly(c, LightingManager.CalculateLighting(-Vector3.UnitY)));
				m_batch.QueueQuad(vector6, vector7, vector8, vector9, texCoord, texCoord2, texCoord3, texCoord4, Color.MultiplyColorOnly(c, LightingManager.CalculateLighting(Vector3.UnitY)));
			}
			m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
		}
	}

	public void FindEquation()
	{
		m_equation = null;
		if (!(GetTankVolume(PetroleumTankIndex) <= 0f))
		{
			float requiredPlayerLevel = FindInteractingPlayer()?.PlayerData.Level ?? 1f;
			FractionatingEquationRecipe actual = new FractionatingEquationRecipe
			{
				MaterialsFluids = new EquationFluid[1]
				{
					new EquationFluid(GetTankValue(PetroleumTankIndex), GetTankVolume(PetroleumTankIndex))
				},
				RequiredPlayerLevel = requiredPlayerLevel
			};
			m_equation = RecipaediaEXManager.FindMatchingRecipe<FractionatingEquationRecipe>(actual);
		}
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => m_inputTankIndices,
			ProductionSlotKind.Output => EnumerateOutputTanks(),
			_ => m_emptyTankIndices,
		};
	}

	private IEnumerable<int> EnumerateOutputTanks()
	{
		for (int i = PetroleumTankIndex + 1; i < TanksCount; i++)
		{
			yield return i;
		}
	}
}
