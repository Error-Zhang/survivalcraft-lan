using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Extensions.Graphics;
using SCIENEW.ProductionIO;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentGReactorCore : ComponentInventoryBase, IUpdateable, IDrawable, IInventoryProductionSlots
{
	private static readonly int[] m_rodSlotIndices = new int[16]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15
	};

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	public const float MaxTemperature = 3000f;

	public float m_reactionSpeed;

	public float m_fireTimeRemaining;

	public bool m_updateRods;

	protected float m_heatLevel;

	private bool m_allocate996ChunksPending;

	public HashSet<Point2> m_allocatedChunks = new HashSet<Point2>();

	public SubsystemTime m_subsystemTime;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemExplosions m_subsystemExplosions;

	public SubsystemParticles m_subsystemParticles;

	public SubsystemRadiation m_subsystemRadiation;

	public SubsystemTerrain m_subsystemTerrain;

	public Subsystem996Chunks m_subsystem996Chunks;

	private PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

	private DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();

	public float HeatLevel
	{
		get
		{
			return m_heatLevel;
		}
		set
		{
			m_heatLevel = value;
		}
	}

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1];

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
		m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
		m_subsystemRadiation = Project.FindSubsystem<SubsystemRadiation>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystem996Chunks = Project.FindSubsystem<Subsystem996Chunks>(throwOnError: true);
		m_fireTimeRemaining = valuesDictionary.GetValue("FireTimeRemaining", 0f);
		m_heatLevel = valuesDictionary.GetValue("HeatLevel", 0f);
		m_updateRods = true;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
		valuesDictionary.SetValue("HeatLevel", m_heatLevel);
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		base.AddSlotItems(slotIndex, value, count);
		m_updateRods = true;
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		int result = base.RemoveSlotItems(slotIndex, count);
		m_updateRods = true;
		return result;
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<GReactorCoreDevice>((m_heatLevel > 100f) ? 1 : 0);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		m_allocate996ChunksPending = true;
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		Free996Chunks();
	}

	public void Allocate996Chunks()
	{
		m_allocatedChunks.Clear();
		Point2 point = new Point2(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Z);
		for (int i = -8; i < 8; i++)
		{
			for (int j = -8; j < 8; j++)
			{
				Point2 point2 = point + new Point2(i, j);
				point2 = Terrain.ToChunk(point2.X, point2.Y);
				if (m_allocatedChunks.Add(point2))
				{
					m_subsystem996Chunks.Allocate996Chunk(point2, isPermaent: true);
				}
			}
		}
		FindInteractingPlayer()?.ComponentGui.DisplaySmallMessage(string.Format(LanguageUtils.GetText(this, "AllocateChunks"), m_allocatedChunks.Count), Color.White, blinking: false, playNotificationSound: false);
	}

	public void Free996Chunks()
	{
		foreach (Point2 allocatedChunk in m_allocatedChunks)
		{
			m_subsystem996Chunks.Free996Chunk(allocatedChunk);
		}
		FindInteractingPlayer()?.ComponentGui.DisplaySmallMessage(string.Format(LanguageUtils.GetText(this, "FreeChunks"), m_allocatedChunks.Count), Color.White, blinking: false, playNotificationSound: false);
		m_allocatedChunks.Clear();
	}

	public void Update(float dt)
	{
		if (m_allocate996ChunksPending)
		{
			m_allocate996ChunksPending = false;
			Allocate996Chunks();
		}
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, coordinates);
		if (m_subsystemTime.PeriodicGameTimeEvent(0.2, 0.0))
		{
			int num = 0;
			if (m_updateRods)
			{
				m_fireTimeRemaining = 0f;
				m_reactionSpeed = 1f;
				for (int i = 0; i < SlotsCount; i++)
				{
					int slotValue = GetSlotValue(i);
					if (Terrain.ExtractContents(slotValue) == BaseDamageableItemBlock.Index)
					{
						if (DamageableItemManager.GetItem(slotValue) is FuelRodDItem { m_type: FuelRodDItem.RodType.UFuelRod })
						{
							m_fireTimeRemaining += 5f;
							num++;
						}
						if (DamageableItemManager.GetItem(slotValue) is FuelRodDItem { m_type: FuelRodDItem.RodType.ControlRod })
						{
							m_fireTimeRemaining -= 4f;
							m_reactionSpeed -= 0.2f;
						}
						if (DamageableItemManager.GetItem(slotValue) is FuelRodDItem { m_type: FuelRodDItem.RodType.CarbonRod })
						{
							m_fireTimeRemaining += 3f;
							m_reactionSpeed += 0.2f;
						}
						if (DamageableItemManager.GetItem(slotValue) is FuelRodDItem { m_type: FuelRodDItem.RodType.PFuelRod })
						{
							m_fireTimeRemaining += 25f;
							m_reactionSpeed += 0f;
							num++;
						}
					}
				}
			}
			m_reactionSpeed = MathUtils.Max(m_reactionSpeed, 0f);
			if (num > 0)
			{
				Point3 coordinates2 = m_componentBlockEntity.Coordinates;
				if (!m_subsystemRadiation.TryGet(coordinates2, out var radiation) || radiation != 10f)
				{
					m_subsystemRadiation.Add(coordinates2, 10f);
				}
				m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining);
				Point3 coordinates3 = m_componentBlockEntity.Coordinates;
				m_heatLevel = MathUtils.Max(0f, m_heatLevel + m_fireTimeRemaining * 0.5f - 0.1f);
				if (m_heatLevel > 3000f)
				{
					m_subsystemRadiation.Add(coordinates2, 15000f);
					m_subsystemRadiation.AddCherenkov(coordinates2);
					m_subsystemExplosions.TryExplodeBlock(coordinates3.X, coordinates3.Y, coordinates3.Z, LargeGunpowderKegBlock.Index);
				}
				if (m_fireTimeRemaining > 0f)
				{
					for (int j = 0; j < SlotsCount; j++)
					{
						int slotValue2 = GetSlotValue(j);
						if (Terrain.ExtractContents(slotValue2) != BaseDamageableItemBlock.Index || !(DamageableItemManager.GetItem(slotValue2) is FuelRodDItem fuelRodDItem5) || !new Game.Random().Bool(0.006f * m_reactionSpeed))
						{
							continue;
						}
						RemoveSlotItems(j, 1);
						if (DamageableItemManager.DamageItem(slotValue2, 1) != slotValue2)
						{
							AddSlotItems(j, DamageableItemManager.DamageItem(slotValue2, 1), 1);
							continue;
						}
						if (fuelRodDItem5.m_type == FuelRodDItem.RodType.UFuelRod)
						{
							AddSlotItems(j, ItemsBlockManager.GetItemID("UsedUpFuel"), 1);
						}
						if (fuelRodDItem5.m_type == FuelRodDItem.RodType.ControlRod)
						{
							AddSlotItems(j, ItemsBlockManager.GetItemID("B"), 1);
						}
						if (fuelRodDItem5.m_type == FuelRodDItem.RodType.CarbonRod)
						{
							AddSlotItems(j, ItemsBlockManager.GetItemID("CoalPowder"), 1);
						}
					}
				}
			}
			else
			{
				if (m_subsystemTime.PeriodicGameTimeEvent(1.2, 0.0))
				{
					Point3 coordinates4 = m_componentBlockEntity.Coordinates;
					m_subsystemRadiation.Remove(coordinates4);
				}
				m_heatLevel = MathUtils.Max(0f, m_heatLevel - 0.1f);
			}
		}
		ReplaceFurnace(m_componentBlockEntity.Coordinates);
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (!m_subsystemTerrain.IsVisible(camera, new Vector3(m_componentBlockEntity.Coordinates)) || !camera.ViewFrustum.Intersection(new BoundingSphere(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f), 1f)))
		{
			return;
		}
		Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.1875f) + new Vector3(0f, 0.65f, 0f);
		_ = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(25f / 32f) + new Vector3(0f, 0.65f, 0f);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				int slotIndex = i * 4 + j;
				if (GetSlotCount(slotIndex) != 0)
				{
					int slotValue = GetSlotValue(slotIndex);
					if (Terrain.ExtractContents(slotValue) == BlocksManager.GetBlockIndex<BaseDamageableItemBlock>() && DamageableItemManager.GetItem(slotValue) is FuelRodDItem fuelRodDItem)
					{
						int num = 2 + i * 5;
						int num2 = 2 + j * 5;
						Matrix matrix = Matrix.CreateTranslation(new Vector3(vector.X + (float)num / 32f, vector.Y, vector.Z + (float)num2 / 32f));
						fuelRodDItem.DrawBlock(m_primitivesRenderer, slotValue, Color.White, 0.6f, ref matrix, m_drawBlockEnvironmentData);
					}
				}
			}
		}
		m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Input)
		{
			return m_rodSlotIndices;
		}
		return m_emptySlotIndices;
	}
}
