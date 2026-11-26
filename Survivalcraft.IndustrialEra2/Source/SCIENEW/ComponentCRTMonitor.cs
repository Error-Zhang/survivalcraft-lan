using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Screens;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCRTMonitor : Component, IScreenContainerComponent, IUpdateable
{
	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemVoltNet m_subsystemVoltNet;

	public int? m_direction;

	public WorkState m_workState;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
	}

	public bool TryGetMetricScreen(out SCIENEW.Screens.Screen screen)
	{
		screen = default;
		if (!m_direction.HasValue || m_workState != WorkState.Active)
		{
			return false;
		}
		Vector3 vector = CellFace.FaceToVector3(m_direction.Value);
		Vector3 center = GetPosition() + 0.5f * vector;
		screen = ScreenGeometry.FromMountFace(m_direction.Value, center, new Vector2(25f / 32f, 25f / 32f));
		return true;
	}

	public SCIENEW.Screens.Screen[] GetScreens(Camera camera)
	{
		if (!TryGetMetricScreen(out var screen))
		{
			return Array.Empty<SCIENEW.Screens.Screen>();
		}
		if (!ScreenGeometry.TryComputeViewParallaxBias(screen.CenterPos, camera, out var bias))
		{
			return Array.Empty<SCIENEW.Screens.Screen>();
		}
		return new SCIENEW.Screens.Screen[1] { ScreenGeometry.ApplyParallaxBias(screen, bias) };
	}

	public Entity GetEntity()
	{
		return Entity;
	}

	public Vector3 GetPosition()
	{
		return new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f);
	}

	public BoundingBox GetBoundingBox()
	{
		return new BoundingBox(new Vector3(m_componentBlockEntity.Coordinates), new Vector3(m_componentBlockEntity.Coordinates) + Vector3.One);
	}

	public string GetDescription()
	{
		return LanguageUtils.GetText(this, "ContainerDescription");
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(coordinates);
		if (voltElement == null)
		{
			m_workState = WorkState.Broken;
			return;
		}
		m_workState = voltElement.CurrentWorkState;
		if (!m_direction.HasValue && m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z).IsValid())
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z);
			m_direction = FourDirectionalDevice.GetDirection_Static(cellValueFast);
		}
	}
}
