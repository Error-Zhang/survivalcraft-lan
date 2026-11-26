using System;
using System.Collections;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using Game.Network;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentQuarry : ComponentInventoryBase, IUpdateable, IDrawable, IIndustrialQuarry
{
	public SubsystemBeacons m_subsystemBeacons;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public SubsystemQuarryDrillerMobiles m_subsystemQuarryDrillerMobiles;

	public SubsystemPickables m_subsystemPickables;

	public Subsystem996Chunks m_subsystem996Chunks;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemAudio m_subsystemAudio;

	public SubsystemVoltNet m_subsystemVoltNet;

	public CoroutineScheduler m_coroutineScheduler = new CoroutineScheduler();

	public SubsystemBeacons.RectanglePoints? m_area;

	private bool m_isPreparing = true;

	public PrimitivesRenderer3D m_p3d = new PrimitivesRenderer3D();

	public int m_mobileID = -1;

	public QuarryDevice.Mode Mode;

	public bool m_displayArea;

	public WorkState m_workState;

	public int[] DrawOrders => new int[1] { 1000 };

	public bool IsPreparing
	{
		get
		{
			return m_isPreparing;
		}
		set
		{
			if (m_isPreparing && !value && m_area.HasValue)
			{
				FinishPreparing();
				StartDrilling();
				m_isPreparing = false;
			}
		}
	}

	public Point3 CurrentDrillingPos { get; set; }

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int DrillerIndex => 0;

	public int RocksIndex => 1;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex == DrillerIndex)
		{
			if (Terrain.ExtractContents(value) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(value) is DrillDItem)
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemBeacons = Project.FindSubsystem<SubsystemBeacons>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemQuarryDrillerMobiles = Project.FindSubsystem<SubsystemQuarryDrillerMobiles>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystem996Chunks = Project.FindSubsystem<Subsystem996Chunks>(throwOnError: true);
		m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		Point3 value = valuesDictionary.GetValue("AreaP1", Point3.Zero);
		Point3 value2 = valuesDictionary.GetValue("AreaP2", Point3.Zero);
		Point3 value3 = valuesDictionary.GetValue("AreaP3", Point3.Zero);
		Point3 value4 = valuesDictionary.GetValue("AreaP4", Point3.Zero);
		if (value != Point3.Zero && value2 != Point3.Zero && value3 != Point3.Zero && value4 != Point3.Zero)
		{
			m_area = new SubsystemBeacons.RectanglePoints(value, value2, value3, value4);
		}
		m_isPreparing = valuesDictionary.GetValue("IsPreparing", defaultValue: true);
		m_mobileID = valuesDictionary.GetValue("MobileID", -1);
		CurrentDrillingPos = valuesDictionary.GetValue("CurrentDrillingPos", Point3.Zero);
		Mode = valuesDictionary.GetValue("Mode", QuarryDevice.Mode.Drill);
		m_displayArea = valuesDictionary.GetValue("DisplayArea", defaultValue: true);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		if (m_area.HasValue)
		{
			valuesDictionary.SetValue("AreaP1", m_area.Value.Point1);
			valuesDictionary.SetValue("AreaP2", m_area.Value.Point2);
			valuesDictionary.SetValue("AreaP3", m_area.Value.Point3);
			valuesDictionary.SetValue("AreaP4", m_area.Value.Point4);
		}
		valuesDictionary.SetValue("IsPreparing", m_isPreparing);
		valuesDictionary.SetValue("MobileID", m_mobileID);
		valuesDictionary.SetValue("CurrentDrillingPos", CurrentDrillingPos);
		valuesDictionary.SetValue("Mode", Mode);
		valuesDictionary.SetValue("DisplayArea", m_displayArea);
	}

	void IIndustrialQuarry.SetDisposalMode(bool dispose)
	{
		Mode = dispose ? QuarryDevice.Mode.Dispose : QuarryDevice.Mode.Drill;
	}

	void IIndustrialQuarry.SerializeLanState(PacketSerializer archive)
	{
		IndustrialQuarryState.Serialize(archive, ref m_area, ref m_isPreparing, ref m_mobileID);
	}

	public void SelectArea()
	{
		m_subsystemBeacons.GetRectangle(m_componentBlockEntity.Coordinates, out m_area);
	}

	public void Update(float dt)
	{
		ComponentQuarryDriller mobile = m_subsystemQuarryDrillerMobiles.GetMobile(m_mobileID);
		m_coroutineScheduler.UpdateCoroutines();
		if (m_isPreparing)
		{
			return;
		}
		mobile.m_displayDrillerHead = GetSlotCount(0) > 0;
		mobile.m_drillRotateSpeed = 0f;
		if (GetSlotCount(DrillerIndex) <= 0)
		{
			return;
		}
		VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(m_componentBlockEntity.Coordinates);
		if (voltElement == null)
		{
			m_workState = WorkState.Broken;
			return;
		}
		m_workState = voltElement.CurrentWorkState;
		if (m_workState != WorkState.Active)
		{
			mobile.WheelRotate = 0f;
			return;
		}
		SubsystemBeacons.RectanglePoints value = m_area.Value;
		if ((float)(value.MinPoint.Y - CurrentDrillingPos.Y) > 40f || CurrentDrillingPos.Y <= 0 || m_subsystemTerrain.Terrain.GetCellContents(CurrentDrillingPos.X, CurrentDrillingPos.Y, CurrentDrillingPos.Z) == BedrockBlock.Index)
		{
			return;
		}
		mobile.m_drillRotateSpeed = 4f;
		if (m_subsystemTime.PeriodicGameTimeEvent(0.5, (double)GetHashCode() % 1000.0 / 1000.0))
		{
			Point3 currentDrillingPos = CurrentDrillingPos;
			do
			{
				CurrentDrillingPos = MoveNext(CurrentDrillingPos, value);
			}
			while (!CheckBlock(CurrentDrillingPos));
			m_coroutineScheduler.AddCoroutine(MoveToPosition(new Vector3(CurrentDrillingPos) + Vector3.One * 0.5f, 0.4f));
			DrillBlock(currentDrillingPos);
		}
		if (!m_subsystemTime.PeriodicGameTimeEvent(0.25, 0.0) || Mode != QuarryDevice.Mode.Dispose)
		{
			return;
		}
		int num = 0;
		foreach (Slot slot in m_slots)
		{
			if (num++ > DrillerIndex && slot != null && slot.Count > 0)
			{
				if (m_subsystemProjectiles.FireProjectile(slot.Value, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + Vector3.UnitY * 0.6f, Vector3.UnitY * 40f, Vector3.Zero, null) != null)
				{
					m_subsystemAudio.PlaySound("Audio/DispenserShoot", 1f, 0f, new Vector3(m_componentBlockEntity.Coordinates), 4f, autoDelay: true);
				}
				else
				{
					m_subsystemPickables.AddPickable(slot.Value, 1, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + Vector3.UnitY * 0.5f, Vector3.UnitY * 5f, null);
				}
				slot.Count--;
				break;
			}
		}
	}

	private bool CheckBlock(Point3 position)
	{
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(position.X, position.Y, position.Z);
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast)];
		if ((block is AirBlock || block is FluidBlock || block is BedrockBlock || block is FocalorsFluidBlock) ? true : false)
		{
			return false;
		}
		return true;
	}

	private void DrillBlock(Point3 position)
	{
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(position.X, position.Y, position.Z);
		m_subsystemTerrain.DestroyCell(0, position.X, position.Y, position.Z, 0, noDrop: true, noParticleSystem: false);
		int damage = DamageableItemManager.GetDamage(GetSlotValue(DrillerIndex));
		m_slots[DrillerIndex].Value = DamageableItemManager.SetDamage(GetSlotValue(DrillerIndex), damage + 1);
		if (BlocksManager.Blocks[BaseDamageableItemBlock.Index].GetBlockHealth(GetSlotValue(DrillerIndex)) < 0f)
		{
			m_slots[DrillerIndex].Count--;
		}
		cellValueFast = Terrain.ReplaceLight(cellValueFast, 0);
		if (ComponentInventoryBase.AcquireItems(this, cellValueFast, 1) > 0)
		{
			m_subsystemPickables.AddPickable(cellValueFast, 1, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + Vector3.UnitY * 0.5f, Vector3.UnitY * 5f, null);
		}
	}

	protected Point3 MoveNext(Point3 current, SubsystemBeacons.RectanglePoints area)
	{
		current.X++;
		if (current.X > area.MaxPoint.X)
		{
			current.X = area.MinPoint.X;
			current.Z++;
			if (current.Z > area.MaxPoint.Z)
			{
				current.Z = area.MinPoint.Z;
				current.Y--;
			}
		}
		return current;
	}

	protected void FinishPreparing()
	{
		SubsystemBeacons.RectanglePoints value = m_area.Value;
		Point3 minPoint = value.MinPoint;
		do
		{
			m_subsystemTerrain.DestroyCell(0, minPoint.X, minPoint.Y, minPoint.Z, 0, noDrop: false, noParticleSystem: false);
			minPoint.X++;
			if (minPoint.X > value.MaxPoint.X)
			{
				minPoint.X = value.MinPoint.X;
				minPoint.Z++;
			}
		}
		while (minPoint != value.MaxPoint);
		m_subsystemTerrain.DestroyCell(0, value.MaxPoint.X, value.MaxPoint.Y, value.MaxPoint.Z, 0, noDrop: false, noParticleSystem: false);
		Allocate996Chunks();
	}

	protected void StartDrilling()
	{
		SubsystemBeacons.RectanglePoints value = m_area.Value;
		int num = value.MaxPoint.Z - value.MinPoint.Z;
		int num2 = value.MaxPoint.X - value.MinPoint.X;
		m_mobileID = m_subsystemQuarryDrillerMobiles.CreateMobile(entity =>
		{
			entity.FindComponent<ComponentQuarryDriller>().InitialRotate = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (num < num2) ? 0f : ((float)Math.PI / 2f));
			entity.FindComponent<ComponentFrame>(throwOnError: true).Position = new Vector3(value.MinPoint) + ((num < num2) ? (Vector3.UnitZ * num / 2f) : (Vector3.UnitX * num2 / 2f)) + new Vector3(0.5f, 0f, 0.5f);
			entity.FindComponent<ComponentBody>(throwOnError: true).BoxSize = ((num < num2) ? new Vector3(1f, 1f, num) : new Vector3(num2, 1f, 1f));
			entity.FindComponent<ComponentQuarryDriller>(throwOnError: true).Length = MathF.Min(num, num2);
		});
		CurrentDrillingPos = value.MinPoint + new Point3(0, -1, 0);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			return;
		}
		if (m_area.HasValue)
		{
			Free996Chunks();
		}
		m_coroutineScheduler.RemoveAllCoroutines();
		if (m_mobileID >= 0)
		{
			m_subsystemQuarryDrillerMobiles.RemoveMobile(m_mobileID);
			m_mobileID = -1;
		}
	}

	public void Allocate996Chunks()
	{
		SubsystemBeacons.RectanglePoints value = m_area.Value;
		Point3 minPoint = value.MinPoint;
		HashSet<Point2> hashSet = new HashSet<Point2>();
		Point2 point = Terrain.ToChunk(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Z);
		m_subsystem996Chunks.Allocate996Chunk(point, isPermaent: true);
		hashSet.Add(point);
		do
		{
			point = Terrain.ToChunk(minPoint.X, minPoint.Z);
			hashSet.Add(point);
			m_subsystem996Chunks.Allocate996Chunk(point, isPermaent: true);
			minPoint.X++;
			if (minPoint.X > value.MaxPoint.X)
			{
				minPoint.X = value.MinPoint.X;
				minPoint.Z++;
			}
		}
		while (minPoint != value.MaxPoint);
		FindInteractingPlayer().ComponentGui.DisplaySmallMessage(string.Format(LanguageUtils.GetText(this, "AllocateChunks"), hashSet.Count), Color.White, blinking: false, playNotificationSound: false);
	}

	public void Free996Chunks()
	{
		SubsystemBeacons.RectanglePoints value = m_area.Value;
		Point3 minPoint = value.MinPoint;
		HashSet<Point2> hashSet = new HashSet<Point2>();
		Point2 point = Terrain.ToChunk(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Z);
		m_subsystem996Chunks.Free996Chunk(point);
		hashSet.Add(point);
		do
		{
			point = Terrain.ToChunk(minPoint.X, minPoint.Z);
			hashSet.Add(point);
			m_subsystem996Chunks.Free996Chunk(point);
			minPoint.X++;
			if (minPoint.X > value.MaxPoint.X)
			{
				minPoint.X = value.MinPoint.X;
				minPoint.Z++;
			}
		}
		while (minPoint != value.MaxPoint);
		FindInteractingPlayer().ComponentGui.DisplaySmallMessage(string.Format(LanguageUtils.GetText(this, "FreeChunks"), hashSet.Count), Color.White, blinking: false, playNotificationSound: false);
	}

	public IEnumerator MoveToPosition(Vector3 endPosition, float duration)
	{
		float timeElapsed = 0f;
		SubsystemBeacons.RectanglePoints value = m_area.Value;
		int num = value.MaxPoint.Z - value.MinPoint.Z;
		int num2 = value.MaxPoint.X - value.MinPoint.X;
		ComponentQuarryDriller driller = m_subsystemQuarryDrillerMobiles.GetMobile(m_mobileID);
		Vector3 startPos = driller.m_componentBody.Position;
		float startPhase = driller.Phase;
		float startDepth = driller.Depth;
		Vector3 vector = new Vector3(value.MinPoint) + ((num < num2) ? (Vector3.UnitZ * num / 2f) : (Vector3.UnitX * num2 / 2f)) + new Vector3(0.5f, 0f, 0.5f);
		Vector3 endPos = ((num < num2) ? new Vector3(endPosition.X, vector.Y, vector.Z) : new Vector3(vector.X, vector.Y, endPosition.Z));
		float endPhase = ((num < num2) ? (((float)value.MaxPoint.Z + 0.5f - endPosition.Z) / (float)num) : (((float)value.MaxPoint.X + 0.5f - endPosition.X) / (float)num2));
		float endLength = (float)value.MinPoint.Y - endPosition.Y;
		driller.m_wheelRotateSpeed = ((num < num2) ? (endPosition.X - startPos.X) : (startPos.Z - endPosition.Z));
		while (timeElapsed < duration)
		{
			driller.m_componentBody.Position = startPos.Lerp(endPos, timeElapsed / duration);
			driller.Phase = MathUtils.Lerp(startPhase, endPhase, timeElapsed / duration);
			driller.Depth = MathUtils.Lerp(startDepth, endLength, timeElapsed / duration);
			timeElapsed += m_subsystemTime.GameTimeDelta;
			yield return default(WaitForFrameCondition);
		}
		driller.m_componentBody.Position = endPos;
		driller.Phase = endPhase;
		driller.Depth = endLength;
		driller.m_wheelRotateSpeed = 0f;
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (!m_isPreparing && m_displayArea && m_area.HasValue)
		{
			DrawingUtils.DrawBoundingBoxHighlight(camera, m_area.Value.GetOutRangeBoundingBox(), Color.Black * new Color(255, 255, 255, 100), Color.Yellow * new Color(255, 255, 255, 100), 0f, 2f, 15, RasterizerState.CullNone);
		}
	}
}
