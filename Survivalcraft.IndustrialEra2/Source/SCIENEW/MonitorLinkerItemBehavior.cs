using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.Extensions.Graphics;
using SCIENEW.Screens;
using SCIENEW.Utils;

namespace SCIENEW;

public class MonitorLinkerItemBehavior : ItemBehavior, IUpdateable, IDrawable
{
	public SubsystemAudio m_subsystemAudio;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemScreens m_subsystemScreens;

	public SubsystemLEDScreenControllers m_subsystemLedScreens;

	public SubsystemTime m_subsystemTime;

	public Game.Random m_random = new Game.Random();

	public PrimitivesRenderer3D m_p3d = new PrimitivesRenderer3D();

	private FlatBatch3D m_flatBath;

	public IScreenProviderComponent? m_provider;

	public IScreenContainerComponent? m_container;

	public ComponentPlayer? m_operatingPlayer;

	private readonly Dictionary<ComponentPlayer, (IScreenProviderComponent Provider, IScreenContainerComponent Container)> m_remoteSelections = new(ReferenceEqualityComparer.Instance);

	public int[] DrawOrders => new int[1] { 1000 };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public MonitorLinkerItemBehavior(Project project)
		: base(project)
	{
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemScreens = Project.FindSubsystem<SubsystemScreens>(throwOnError: true);
		m_subsystemLedScreens = Project.FindSubsystem<SubsystemLEDScreenControllers>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_flatBath = m_p3d.FlatBatch(0, DepthStencilState.None);
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		ComponentPlayer player = componentMiner.ComponentPlayer;
		if (player == null)
		{
			return false;
		}
		if (!NetworkManager.IsServerRunning || player.PlayerData.IsMainPlayer)
		{
			return UseLinker(ray, componentMiner);
		}
		(IScreenProviderComponent provider, IScreenContainerComponent container, ComponentPlayer operatorPlayer) = (m_provider, m_container, m_operatingPlayer);
		try
		{
			(m_provider, m_container) = m_remoteSelections.GetValueOrDefault(player);
			m_operatingPlayer = player;
			bool handled = UseLinker(ray, componentMiner);
			if (m_provider != null || m_container != null)
			{
				m_remoteSelections[player] = (m_provider, m_container);
			}
			else
			{
				m_remoteSelections.Remove(player);
			}
			return handled;
		}
		finally
		{
			(m_provider, m_container, m_operatingPlayer) = (provider, container, operatorPlayer);
		}
	}

	private bool UseLinker(Ray3 ray, ComponentMiner componentMiner)
	{
		object obj = componentMiner.Raycast(ray, RaycastMode.Digging);
		Entity entity = null;
		if (obj is TerrainRaycastResult)
		{
			CellFace cellFace = ((TerrainRaycastResult)obj).CellFace;
			ComponentLEDScreen component;
			if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, cellFace.Point, out ComponentBlockEntity blockEntity))
			{
				entity = blockEntity.Entity;
			}
			else if (m_subsystemLedScreens.TryGetContainerAtBlock(cellFace.Point, out component) && component != null)
			{
				entity = component.GetEntity();
			}
		}
		else if (obj is BodyRaycastResult)
		{
			entity = ((BodyRaycastResult)obj).ComponentBody.Entity;
		}
		if (entity == null)
		{
			if (m_provider != null || m_container != null || m_operatingPlayer != null)
			{
				componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get("Behaviors", "MonitorLinkerItemBehavior", "CancelTip"), Color.White, blinking: false, playNotificationSound: false);
			}
			m_provider = null;
			m_container = null;
			m_operatingPlayer = null;
			return false;
		}
		bool flag = false;
		IScreenContainerComponent[] containerComponents;
		if (m_subsystemScreens.HasProvider(entity, out IScreenProviderComponent[] providerComponents) && providerComponents != null)
		{
			flag = true;
			m_provider = providerComponents[0];
			m_operatingPlayer = componentMiner.ComponentPlayer;
		}
		else if (m_subsystemScreens.HasContainer(entity, out containerComponents) && containerComponents != null)
		{
			flag = true;
			m_container = containerComponents[0];
			m_operatingPlayer = componentMiner.ComponentPlayer;
		}
		if (m_provider != null && m_container != null)
		{
			if (m_provider.GetEntity().IsAddedToProject && m_container.GetEntity().IsAddedToProject)
			{
				if (!IndustrialEraLanAdapter.IsRemote(Project))
				{
					m_subsystemScreens.AddLink(m_provider, m_container);
				}
				string text = string.Format(LanguageControl.Get("Behaviors", "MonitorLinkerItemBehavior", "LinkTip"), m_provider.GetDescription(), m_container.GetDescription());
				componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(text, Color.White, blinking: true, playNotificationSound: true);
			}
			else
			{
				componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get("Behaviors", "MonitorLinkerItemBehavior", "FailedToCreateLink-1"), Color.White, blinking: true, playNotificationSound: true);
			}
			m_provider = null;
			m_container = null;
			m_operatingPlayer = null;
		}
		if (flag)
		{
			m_subsystemAudio.PlaySound("Audio/UI/ButtonClick", 1f, m_random.Float(-0.1f, 0.1f), ray.Position, 1f, autoDelay: true);
		}
		return flag;
	}

	public void Update(float dt)
	{
		if (m_operatingPlayer != null && m_subsystemTime.PeriodicGameTimeEvent(1.0, 0.0))
		{
			if (m_provider != null)
			{
				string text = string.Format(LanguageControl.Get("Behaviors", "MonitorLinkerItemBehavior", "ProviderTip"), m_provider.GetDescription());
				m_operatingPlayer.ComponentGui.DisplaySmallMessage(text, SubsystemScreens.ProviderColor, blinking: false, playNotificationSound: false);
			}
			else if (m_container != null)
			{
				string text2 = string.Format(LanguageControl.Get("Behaviors", "MonitorLinkerItemBehavior", "ContainerTip"), m_container.GetDescription());
				m_operatingPlayer.ComponentGui.DisplaySmallMessage(text2, SubsystemScreens.ContainerColor, blinking: false, playNotificationSound: false);
			}
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (m_provider != null)
		{
			BoundingBox boundingBox = m_provider.GetBoundingBox().Wrap(0.05f);
			if (Math.IEEERemainder(m_subsystemTime.GameTime, 0.5) <= 0.0)
			{
				m_flatBath.QueueBoundingBoxFace(boundingBox, SubsystemScreens.ProviderColor);
			}
			DrawingUtils.DrawBoundingBoxHighlight(camera, boundingBox, SubsystemScreens.ProviderColor * new Color(255, 255, 255, 64), SubsystemScreens.ProviderColor * Color.LightGray * new Color(255, 255, 255, 64), (float)Math.IEEERemainder(m_subsystemTime.GameTime, 1.0), 6f, int.MaxValue);
		}
		else if (m_container != null)
		{
			BoundingBox boundingBox2 = m_container.GetBoundingBox().Wrap(0.05f);
			if (Math.IEEERemainder(m_subsystemTime.GameTime, 0.5) <= 0.0)
			{
				m_flatBath.QueueBoundingBoxFace(boundingBox2, SubsystemScreens.ContainerColor);
			}
			DrawingUtils.DrawBoundingBoxHighlight(camera, boundingBox2, SubsystemScreens.ContainerColor * new Color(255, 255, 255, 64), SubsystemScreens.ContainerColor * Color.LightGray * new Color(255, 255, 255, 64), (float)Math.IEEERemainder(m_subsystemTime.GameTime, 1.0), 6f, int.MaxValue);
		}
		m_p3d.Flush(camera.ViewProjectionMatrix);
	}
}
