using System;
using System.Linq;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Screens;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentLEDScreen : Component, IScreenContainerComponent, IUpdateable
{
	private const float FacePlaneOffset = -0.4325f;

	private const float BorderInset = 3f / 32f;

	public ComponentBlockEntity m_componentBlockEntity;

	private SubsystemTerrain m_subsystemTerrain;

	private SubsystemVoltNet m_subsystemVoltNet;

	private SubsystemLEDScreenControllers m_subsystemControllers;

	private LEDScreenController? m_controller;

	private int? m_mountFace;

	private WorkState m_workState;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_subsystemControllers = Project.FindSubsystem<SubsystemLEDScreenControllers>(throwOnError: true);
	}

	public bool TryGetMetricScreen(out SCIENEW.Screens.Screen screen)
	{
		screen = default;
		if (m_controller == null || !m_controller.InscribedRect.IsValid || !m_mountFace.HasValue)
		{
			return false;
		}
		if (m_workState != WorkState.Active)
		{
			return false;
		}
		LEDScreenInscribedRect inscribedRect = m_controller.InscribedRect;
		if ((float)inscribedRect.Width - 0.1875f <= 0f || (float)inscribedRect.Height - 0.1875f <= 0f)
		{
			return false;
		}
		screen = ScreenGeometry.FromFlatInscribedRect(m_mountFace.Value, m_controller.AnchorPoint, inscribedRect.TopLeft.X, inscribedRect.TopLeft.Y, inscribedRect.Width, inscribedRect.Height, -0.4325f, 3f / 32f);
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
		if (m_controller == null)
		{
			Point3 coordinates = m_componentBlockEntity.Coordinates;
			return new BoundingBox(new Vector3(coordinates), new Vector3(coordinates) + Vector3.One);
		}
		Point3[] cellPoints = m_controller.GetCellPoints();
		int num = cellPoints.Min((Point3 p) => p.X);
		int num2 = cellPoints.Min((Point3 p) => p.Y);
		int num3 = cellPoints.Min((Point3 p) => p.Z);
		int num4 = cellPoints.Max((Point3 p) => p.X);
		int num5 = cellPoints.Max((Point3 p) => p.Y);
		int num6 = cellPoints.Max((Point3 p) => p.Z);
		return new BoundingBox(new Vector3(num, num2, num3), new Vector3(num4 + 1, num5 + 1, num6 + 1));
	}

	public string GetDescription()
	{
		return LanguageUtils.GetText(this, "ContainerDescription");
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z);
		if (Terrain.ExtractContents(cellValueFast) != BaseDeviceBlock.Index || !(DevicesBlockManager.GetDevice(cellValueFast) is LEDScreenDevice))
		{
			m_controller = null;
			m_mountFace = null;
			m_workState = WorkState.Broken;
		}
		else
		{
			m_workState = m_subsystemVoltNet.GetVoltElement(coordinates)?.CurrentWorkState ?? WorkState.Broken;
			int direction_Static = LEDScreenDevice.GetDirection_Static(cellValueFast);
			m_mountFace = direction_Static;
			m_controller = m_subsystemControllers.GetController(new CellFace(coordinates.X, coordinates.Y, coordinates.Z, direction_Static));
			m_workState = ResolveGroupWorkState(m_controller);
		}
	}

	private WorkState ResolveGroupWorkState(LEDScreenController? controller)
	{
		if (controller == null)
		{
			return WorkState.Broken;
		}
		bool flag = false;
		foreach (CellFace key in controller.m_cells.Keys)
		{
			VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(key.Point);
			if (voltElement != null)
			{
				if (voltElement.CurrentWorkState == WorkState.Active)
				{
					return WorkState.Active;
				}
				if (voltElement.CurrentWorkState == WorkState.Underpowered)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return WorkState.Broken;
		}
		return WorkState.Underpowered;
	}
}
