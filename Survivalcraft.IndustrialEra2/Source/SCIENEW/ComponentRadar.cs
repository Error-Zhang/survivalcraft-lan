using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Screens;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentRadar : Component, IScreenProviderComponent, IUpdateable
{
	public ComponentBlockEntity? m_componentBlockEntity;

	public ComponentBody? m_componentBody;

	public IInventory? m_batteryInventory;

	public float m_consumingPeriod;

	public SubsystemTime m_subsystemTime;

	public RadarBatch3D m_batch;

	public ComponentIEModel m_componentIEModel;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemVoltNet m_subsystemVoltNet;

	public SubsystemDetectingEntities m_subsystemDetectingEntities;

	public bool m_highSample;

	public int m_radius = 32;

	public const int MinRadius = 16;

	public const int MaxRadius = 64;

	public WorkState m_workState;

	public Vector2[] m_detectingPoints = new Vector2[8];

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>();
		if (m_componentBlockEntity == null)
		{
			m_componentBody = Entity.FindComponent<ComponentBody>(throwOnError: true);
			m_batteryInventory = Entity.FindComponent<IInventory>(throwOnError: true);
		}
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentIEModel>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_subsystemDetectingEntities = Project.FindSubsystem<SubsystemDetectingEntities>(throwOnError: true);
		m_radius = valuesDictionary.GetValue("Radius", 32);
		m_highSample = !IESettingsManager.Get<bool>("LowQuality");
		m_batch = new RadarBatch3D
		{
			Layer = 0,
			DepthStencilState = DepthStencilState.Default,
			BlendState = BlendState.AlphaBlend,
			RasterizerState = SCIENEW.Screens.Screen.QuadRasterizerState,
			HighSample = m_highSample
		};
		RadarDevice device = DevicesBlockManager.GetDevice<RadarDevice>();
		if (device != null)
		{
			m_componentIEModel.SetModel(device.m_model);
			m_componentIEModel.SetTexture(device.m_texture);
			m_componentIEModel.MainBoneName = m_componentIEModel.Model.RootBone.Name;
		}
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Radius", m_radius);
	}

	public void Draw(SCIENEW.Screens.Screen screen, Project project, Camera camera, int drawOrder)
	{
		m_batch.HighSample = m_highSample;
		m_batch.Time = ((m_workState == WorkState.Active) ? ((float)m_subsystemTime.GameTime) : 0f);
		m_batch.Points = m_detectingPoints;
		ScreensManager.QueueQuad(m_batch, screen.Corner, screen.Right, screen.Up, Color.Cyan);
		m_batch.Flush(camera.ViewProjectionMatrix);
	}

	public Entity GetEntity()
	{
		return Entity;
	}

	public Vector3 GetPosition()
	{
		if (m_componentBlockEntity == null)
		{
			return m_componentBody.Position;
		}
		return new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f);
	}

	public BoundingBox GetBoundingBox()
	{
		if (m_componentBlockEntity == null)
		{
			return new BoundingBox(m_componentBody.Position - m_componentBody.BoxSize / 2f, m_componentBody.Position + m_componentBody.BoxSize / 2f);
		}
		return new BoundingBox(new Vector3(m_componentBlockEntity.Coordinates), new Vector3(m_componentBlockEntity.Coordinates) + Vector3.One);
	}

	public string GetDescription()
	{
		return LanguageUtils.GetText(this, (m_componentBlockEntity != null) ? "ProviderDescription_Fixed" : "ProviderDescription_Portable");
	}

	public void Update(float dt)
	{
		m_detectingPoints = Array.Empty<Vector2>();
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			UpdateDrawing();
			if (m_workState == WorkState.Active)
			{
				m_detectingPoints = m_subsystemDetectingEntities.GetRelativePositions(GetPosition().XZ, m_radius, 8);
			}
			return;
		}
		if (m_componentBlockEntity != null)
		{
			Point3 coordinates = m_componentBlockEntity.Coordinates;
			VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(coordinates);
			if (voltElement == null)
			{
				m_workState = WorkState.Broken;
				return;
			}
			m_workState = voltElement.CurrentWorkState;
			ReplaceFurnace(m_componentBlockEntity.Coordinates);
		}
		else
		{
			m_workState = WorkState.Underpowered;
			int num = -1;
			for (int i = 0; i < 4; i++)
			{
				int slotValue = m_batteryInventory.GetSlotValue(i);
				if (slotValue != 0 && Terrain.ExtractContents(slotValue) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(slotValue) is BatteryDItem batteryDItem)
				{
					if (batteryDItem.GetDurability(slotValue) - batteryDItem.GetDamage(slotValue) > 0)
					{
						m_consumingPeriod += dt;
						m_workState = WorkState.Active;
						num = i;
						break;
					}
					num = -1;
				}
			}
			if (m_consumingPeriod > 0.6f && num != -1)
			{
				m_consumingPeriod = 0f;
				int slotValue2 = m_batteryInventory.GetSlotValue(num);
				m_batteryInventory.RemoveSlotItems(num, 1);
				m_batteryInventory.AddSlotItems(num, DamageableItemManager.DamageItem(slotValue2, 1, Entity), 1);
			}
			if (m_componentBody.ParentBody == null)
			{
				DestroyWhileEntity();
			}
		}
		UpdateDrawing();
		if (m_workState == WorkState.Active)
		{
			m_detectingPoints = m_subsystemDetectingEntities.GetRelativePositions(GetPosition().XZ, m_radius, 8);
		}
	}

	public virtual void ReplaceFurnace(Point3 coordinates)
	{
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<RadarDevice>((m_workState == WorkState.Active) ? 1 : 0);
			m_subsystemTerrain.ChangeCell(coordinates.X, coordinates.Y, coordinates.Z, deviceValueByType);
		}
	}

	public void DestroyWhileEntity()
	{
		Project.FindSubsystem<SubsystemPickables>().AddPickable(DevicesBlockManager.GetDeviceValueByType<RadarDevice>(), 1, m_componentBody.Position, null, null);
		Project.RemoveEntity(Entity, disposeEntity: true);
	}

	public void UpdateDrawing()
	{
		Matrix value = Matrix.CreateTranslation(GetPosition() - new Vector3(0f, 0.5f, 0f));
		Matrix value2 = Matrix.CreateRotationY((m_workState == WorkState.Active) ? ((0f - (float)m_subsystemTime.GameTime) * ((float)Math.PI * 2f) / 5f) : 0f);
		m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, value);
		m_componentIEModel.SetBoneTransform("RadarRotor", value2);
	}
}
