using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentDrillDItemDrawer : Component, IDrawable, IUpdateable
{
	public ComponentPlayer m_componentPlayer;

	public Point3[]? m_points;

	public SubsystemTime m_subsystemTime;

	public bool m_visible = true;

	public int[] DrawOrders => new int[1] { 1000 };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public static Color Color => new Color(137, 255, 255);

	public void Draw(Camera camera, int drawOrder)
	{
		if (m_points != null && m_visible)
		{
			Point3[] points = m_points;
			foreach (Point3 p in points)
			{
				DrawingUtils.DrawBoundingBoxHighlight(camera, new BoundingBox(new Vector3(p), new Vector3(p) + Vector3.One), Color * new Color(255, 255, 255, 32), Color * Color.LightGray * new Color(255, 255, 255, 32), (float)Math.IEEERemainder(m_subsystemTime.GameTime, 1.0), 6f, int.MaxValue, null, null, DepthStencilState.None);
			}
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
	}

	public void Update(float dt)
	{
		m_points = null;
		m_visible = IESettingsManager.Get<bool>("MiningDrillDisplayPattern");
		if (!m_visible)
		{
			return;
		}
		int activeBlockValue = m_componentPlayer.ComponentMiner.ActiveBlockValue;
		if (Terrain.ExtractContents(activeBlockValue) != BlocksManager.GetBlockIndex<BaseDamageableItemBlock>() || !(DamageableItemManager.GetItem(activeBlockValue) is MiningDrillDItem miningDrillDItem))
		{
			return;
		}
		Camera activeCamera = m_componentPlayer.GameWidget.ActiveCamera;
		Ray3 ray = new Ray3(activeCamera.ViewPosition, activeCamera.ViewDirection);
		if (m_componentPlayer.ComponentMiner.Raycast(ray, RaycastMode.Digging) is TerrainRaycastResult terrainRaycastResult)
		{
			m_points = miningDrillDItem.PointsGetter(terrainRaycastResult.CellFace);
			for (int i = 0; i < m_points.Length; i++)
			{
				m_points[i] += terrainRaycastResult.CellFace.Point;
			}
		}
	}
}
