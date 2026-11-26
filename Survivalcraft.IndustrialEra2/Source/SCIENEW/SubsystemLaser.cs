using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemLaser : Subsystem, IUpdateable, IDrawable
{
	public class LaserRendererData : IEquatable<LaserRendererData>
	{
		public readonly Vector3 m_start;

		public readonly Vector3 m_end;

		public readonly Color m_color;

		public readonly double m_energy;

		public LaserRendererData(Vector3 start, Vector3 end, Color color, double energy)
		{
			m_start = start;
			m_end = end;
			m_color = color;
			m_energy = energy;
		}

		public LaserRendererData(Vector3 start, Vector3 end, Vector3 color, double energy)
			: this(start, end, new Color(color), energy)
		{
		}

		public bool Equals(LaserRendererData? other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (m_start.Equals(other.m_start) && m_end.Equals(other.m_end) && m_color.Equals(other.m_color))
			{
				return m_energy.Equals(other.m_energy);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(m_start, m_end, m_color, m_energy);
		}
	}

	public const int MaxDistance = 100;

	public const double MinEnergy = 100.0;

	public const double MaxEnergy = 8000.0;

	public const double MinEnergyToMine = 4000.0;

	public const double MaxEnergyToMine = 500000.0;

	public const double MinBurnEnergy = 4000.0;

	public const float MaxWorkPower = 24f;

	public const float MinWorkPower = 1f;

	public double m_lastUpdateTime;

	public readonly Dictionary<Point3, double> m_currentBlockEnergy = new Dictionary<Point3, double>();

	public readonly Dictionary<Point3, double> m_accumilatedBlockEnergy = new Dictionary<Point3, double>();

	public readonly List<LaserRendererData> m_rendererRequest = new List<LaserRendererData>();

	public readonly List<LaserRendererData> m_requestToRemove = new List<LaserRendererData>();

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemBodies m_subsystemBodies;

	public SubsystemMovingBlocks m_subsystemMovingBlocks;

	public SubsystemTime m_subsystemTime;

	public SubsystemExplosions m_subsystemExplosions;

	public SubsystemFireBlockBehavior m_subsystemFireBlockBehavior;

	public SubsystemPickables m_subsystemPickables;

	public SubsystemGameInfo m_subsystemGameInfo;

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1] { 1000 };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
		m_subsystemMovingBlocks = Project.FindSubsystem<SubsystemMovingBlocks>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		m_subsystemFireBlockBehavior = Project.FindSubsystem<SubsystemFireBlockBehavior>(throwOnError: true);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
	}

	public void Spawn(Vector3 start, Vector3 direction, double energy)
	{
		Spawn(start, start, direction, energy);
	}

	public void Spawn(Vector3 start, Vector3 renderStart, Vector3 direction, double energy)
	{
		Spawn(start, renderStart, direction, Color.White, energy);
	}

	public void Spawn(Vector3 start, Vector3 renderStart, Vector3 direction, Color color, double energy)
	{
		if (energy <= 100.0)
		{
			return;
		}
		Vector3 vector = start + direction * 100f;
		BodyRaycastResult? bodyRaycastResult = m_subsystemBodies.Raycast(start, vector, 0.35f, (ComponentBody body, float distance) => true);
		m_subsystemMovingBlocks.Raycast(start, vector, extendToFillCells: true);
		TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(start, vector, useInteractionBoxes: false, skipAirBlocks: true, (int value2, float distance) => !BlocksManager.Blocks[Terrain.ExtractContents(value2)].GetIsDiggingTransparent(value2));
		if (terrainRaycastResult.HasValue || bodyRaycastResult.HasValue)
		{
			if (terrainRaycastResult.HasValue && (!bodyRaycastResult.HasValue || terrainRaycastResult.Value.Distance < bodyRaycastResult.Value.Distance))
			{
				Vector3 vector2 = terrainRaycastResult.Value.HitPoint();
				Point3 point = terrainRaycastResult.Value.CellFace.Point;
				Vector3 position = new Vector3(point) + new Vector3(0.5f);
				BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, point, out ComponentBlockEntity blockEntity);
				Block block = BlocksManager.Blocks[Terrain.ExtractContents(terrainRaycastResult.Value.Value)];
				int value = terrainRaycastResult.Value.Value;
				ILaserHandler laserHandler = blockEntity?.Entity.FindComponent<ILaserHandler>();
				if (laserHandler != null)
				{
					if (!laserHandler.OnLaserHit(renderStart, direction, terrainRaycastResult.Value, color, energy))
					{
						RenderLaser(renderStart, vector2, color, energy);
					}
					return;
				}
				if ((block is GlassBlock || block is WindowBlock || block is FramedGlassBlock) ? true : false)
				{
					RenderLaser(renderStart, vector2, color, energy);
					Spawn(vector2 + direction * 0.9f, vector2, direction, color, energy / 1.05);
					return;
				}
				double num = block.GetDigResilience(value);
				if (num != double.PositiveInfinity)
				{
					if (m_lastUpdateTime != m_subsystemTime.GameTime)
					{
						m_currentBlockEnergy.Clear();
						m_lastUpdateTime = m_subsystemTime.GameTime;
					}
					double valueOrDefault = m_currentBlockEnergy.GetValueOrDefault(point, 0.0);
					valueOrDefault += energy;
					m_currentBlockEnergy[point] = valueOrDefault;
					if (blockEntity?.Entity.FindComponent<ComponentFurnace>() == null)
					{
						if (valueOrDefault > 4000.0)
						{
							double valueOrDefault2 = m_accumilatedBlockEnergy.GetValueOrDefault(point, 0.0);
							double num2 = num * 500000.0;
							valueOrDefault2 += energy / num2;
							m_accumilatedBlockEnergy[point] = valueOrDefault2;
							if (valueOrDefault2 > 1.0)
							{
								if (block.GetExplosionPressure(value) > 0f)
								{
									m_subsystemExplosions.TryExplodeBlock(point.X, point.Y, point.Z, value);
								}
								else if (block.GetFuelFireDuration(value) > 0f)
								{
									int num3 = (int)block.GetFuelFireDuration(value) / 10;
									for (int num4 = 0; num4 <= num3; num4++)
									{
										m_subsystemPickables.AddPickable(ItemsBlockManager.GetItemID("CoalPowder"), 1, position, null, null);
									}
									m_subsystemTerrain.DestroyCell(0, point.X, point.Y, point.Z, 0, noDrop: true, noParticleSystem: false);
								}
								else
								{
									m_subsystemTerrain.DestroyCell(0, point.X, point.Y, point.Z, 0, noDrop: false, noParticleSystem: false);
								}
								m_accumilatedBlockEnergy.Remove(point);
							}
						}
						if (valueOrDefault > 4000.0 && block.GetFireDuration(value) > 0f)
						{
							m_subsystemFireBlockBehavior.SetCellOnFire(point.X, point.Y, point.Z, 1f);
						}
					}
				}
				RenderLaser(renderStart, vector2, color, energy);
			}
			else
			{
				if (!bodyRaycastResult.HasValue || (terrainRaycastResult.HasValue && !(bodyRaycastResult.Value.Distance < terrainRaycastResult.Value.Distance)))
				{
					return;
				}
				if (energy > 100.0)
				{
					double num5 = 10.0 * (energy / 8000.0);
					if (num5 > 0.0)
					{
						bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentOnFire>()?.SetOnFire(null, (float)num5);
						if (m_subsystemTime.PeriodicGameTimeEvent(0.2, 0.0) && (bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentPlayer>() == null || m_subsystemGameInfo.WorldSettings.GameMode != GameMode.Creative))
						{
							ComponentHealth componentHealth = bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentHealth>();
							componentHealth.Injure(SubsystemVoltNet.GetDamage(EnergyToElectricPower(energy), 1f) / componentHealth.AttackResilience, null, ignoreInvulnerability: true, LanguageUtils.GetText(this, "DieCause"));
						}
					}
				}
				RenderLaser(renderStart, bodyRaycastResult.Value.HitPoint(), color, energy);
			}
		}
		else
		{
			RenderLaser(renderStart, vector, color, energy);
		}
	}

	public void Update(float dt)
	{
		foreach (LaserRendererData item in m_requestToRemove)
		{
			m_rendererRequest.Remove(item);
		}
		m_requestToRemove.Clear();
		foreach (KeyValuePair<Point3, double> item2 in new Dictionary<Point3, double>(m_accumilatedBlockEnergy))
		{
			if (item2.Value <= 0.0)
			{
				m_accumilatedBlockEnergy.Remove(item2.Key);
			}
			else
			{
				m_accumilatedBlockEnergy[item2.Key] -= dt * 0.1f;
			}
		}
	}

	public void RenderLaser(Vector3 renderStart, Vector3 renderEnd, Color color, double energy)
	{
		m_rendererRequest.Add(new LaserRendererData(renderStart, renderEnd, color, energy));
	}

	public void Draw(Camera camera, int drawOrder)
	{
		FlatBatch3D flatBatch3D = m_primitivesRenderer3D.FlatBatch(0, DepthStencilState.DepthRead, null, BlendState.Additive);
		foreach (LaserRendererData item in m_rendererRequest)
		{
			Vector3 start = item.m_start;
			Vector3 end = item.m_end;
			Color color = item.m_color;
			float num = (float)(item.m_energy / 8000.0);
			flatBatch3D.QueueQuad(start + new Vector3(0f, 0.01f * num, 0f), end + new Vector3(0f, 0.01f * num, 0f), start - new Vector3(0f, 0.01f * num, 0f), end - new Vector3(0f, 0.01f * num, 0f), color);
			flatBatch3D.QueueQuad(end + new Vector3(0f, 0.01f * num, 0f), start + new Vector3(0f, 0.01f * num, 0f), end - new Vector3(0f, 0.01f * num, 0f), start - new Vector3(0f, 0.01f * num, 0f), color);
			flatBatch3D.QueueQuad(start + new Vector3(0f, 0f, 0.01f * num), end + new Vector3(0f, 0f, 0.01f * num), start - new Vector3(0f, 0f, 0.01f * num), end - new Vector3(0f, 0f, 0.01f * num), color);
			flatBatch3D.QueueQuad(end + new Vector3(0f, 0f, 0.01f * num), start + new Vector3(0f, 0f, 0.01f * num), end - new Vector3(0f, 0f, 0.01f * num), start - new Vector3(0f, 0f, 0.01f * num), color);
			flatBatch3D.QueueQuad(start + new Vector3(0.01f * num, 0f, 0f), end + new Vector3(0.01f * num, 0f, 0f), start - new Vector3(0.01f * num, 0f, 0f), end - new Vector3(0.01f * num, 0f, 0f), color);
			flatBatch3D.QueueQuad(end + new Vector3(0.01f * num, 0f, 0f), start + new Vector3(0.01f * num, 0f, 0f), end - new Vector3(0.01f * num, 0f, 0f), start - new Vector3(0.01f * num, 0f, 0f), color);
			m_requestToRemove.Add(item);
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}

	public static double ElectricPowerToEnergy(float inputPower)
	{
		return LerpUtils.Map(1.0, 24.0, 100.0, 8000.0, inputPower);
	}

	public static float EnergyToElectricPower(double inputEnergy)
	{
		return (float)LerpUtils.Map(100.0, 8000.0, 1.0, 24.0, inputEnergy);
	}
}
