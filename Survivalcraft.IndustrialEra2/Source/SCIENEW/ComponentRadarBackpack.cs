using System;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentRadarBackpack : ComponentIEModel, IUpdateable
{
	public ComponentPlayer m_componentPlayer;

	public ComponentHumanModel m_componentHumanModel;

	public RadarContentWidget m_radarWidget;

	public CanvasWidget m_radarHolder;

	public CanvasWidget m_radarPanel;

	public SubsystemDetectingEntities m_subsystemDetectingEntities;

	public SubsystemTime m_subsystemTime;

	public SubsystemAudio m_subsystemAudio;

	public SubsystemLaser m_subsystemLaser;

	public float m_radius = 64f;

	public bool m_isWearing;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
		m_subsystemDetectingEntities = Project.FindSubsystem<SubsystemDetectingEntities>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemLaser = Project.FindSubsystem<SubsystemLaser>(throwOnError: true);
		m_componentHumanModel = Entity.FindComponent<ComponentHumanModel>(throwOnError: true);
		Model model = ContentManager.Get<Model>("Models/RadarBackpack");
		SetModel(model);
		SetTexture(IndustrialModLoader.IETexture);
		MainBoneName = "Backpack";
	}

	public override void CalculateIsVisible(Camera camera)
	{
		m_componentHumanModel.CalculateIsVisible(camera);
		IsVisibleForCamera = m_componentHumanModel.IsVisibleForCamera && m_isWearing;
	}

	public override void Animate()
	{
		base.Animate();
		if (m_isWearing)
		{
			SetBoneTransform("Backpack", m_componentHumanModel.GetBoneTransform(m_componentHumanModel.m_bodyBone.Index));
		}
		else
		{
			SetBoneTransform("Backpack", null);
		}
	}

	public void CheckIsWearing()
	{
		ReadOnlyList<int> clothes = m_componentPlayer.ComponentClothing.GetClothes(ClothingSlot.Torso);
		ClothingBlock clothingBlock = BlocksManager.Blocks[ClothingBlock.Index] as ClothingBlock;
		int isWearing;
		if (clothes.Count > 0)
		{
			isWearing = ((clothingBlock.GetClothingData(clothes[clothes.Count - 1]).Index == 50) ? 1 : 0);
		}
		else
		{
			isWearing = 0;
		}
		m_isWearing = (byte)isWearing != 0;
	}

	private void CreateRadarHud()
	{
		m_radarHolder = new CanvasWidget();
		m_radarHolder.LoadContents(m_radarHolder, ContentManager.Get<XElement>("Widgets/RadarBackpackWidget"));
		m_radarHolder.IsHitTestVisible = false;
		m_radarWidget = m_radarHolder.Children.Find<RadarContentWidget>("RadarContent");
		m_radarPanel = m_radarHolder.Children.Find<CanvasWidget>("RadarPanel");
		m_componentPlayer.GuiWidget.Children.Add(m_radarHolder);
	}

	public override void OnEntityRemoved()
	{
		m_radarHolder?.ParentWidget?.Children.Remove(m_radarHolder);
		m_radarHolder?.Dispose();
		m_radarHolder = null;
		m_radarWidget = null;
		m_radarPanel = null;
		base.OnEntityRemoved();
	}

	public void Update(float dt)
	{
		CheckIsWearing();
		if (!m_componentPlayer.PlayerData.IsMainPlayer || !m_isWearing)
		{
			if (m_radarHolder != null)
			{
				m_radarHolder.IsVisible = false;
			}
			return;
		}

		if (m_radarHolder == null)
		{
			CreateRadarHud();
		}
		WidgetAlignmentData alignment = IESettingsManager.Get<WidgetAlignmentData>("RadarAlignment");
		m_radarPanel.HorizontalAlignment = alignment.Horizontal;
		m_radarPanel.VerticalAlignment = alignment.Vertical;
		m_radarWidget.Time = (float)m_subsystemTime.GameTime;
		Vector2[] relativePositions = m_subsystemDetectingEntities.GetRelativePositions(m_componentPlayer.ComponentBody.Position.XZ, m_radius, 8);
		m_radarWidget.Points = relativePositions;
		m_radarHolder.IsVisible = true;

		if (relativePositions.Length == 0)
		{
			return;
		}
		float distance = relativePositions[0].Length();
		(string sound, double interval) = distance switch
		{
			> 0.5f => ("Audio/RadarbackpackBeep1", 1.0),
			> 0.25f => ("Audio/RadarbackpackBeep2", 0.67),
			_ => ("Audio/RadarbackpackBeep3", 0.33)
		};
		if (m_subsystemTime.PeriodicGameTimeEvent(interval, 0.0))
		{
			m_subsystemAudio.PlaySound(sound, 1f, 0f, m_componentPlayer.ComponentBody.Position, 10f, autoDelay: false);
		}
	}
}
