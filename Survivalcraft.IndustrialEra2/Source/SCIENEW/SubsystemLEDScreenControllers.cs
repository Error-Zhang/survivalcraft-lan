using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Engine.Media;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemLEDScreenControllers : Subsystem, IDrawable, IUpdateable
{
	public Dictionary<CellFace, LEDScreenController> m_positionToControllers = new Dictionary<CellFace, LEDScreenController>();

	private PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	private FlatBatch3D m_flatBatch;

	private FontBatch3D m_textBatch;

	private SubsystemTerrain m_subsystemTerrain;

	private bool m_debugCanDraw;

	private readonly HashSet<Point3> m_groupEntityAnchors = new HashSet<Point3>();

	private bool m_pendingPostLoadSync;

	private int m_postLoadSyncDelayFrames;

	private const int PostLoadSyncDelayFrames = 5;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1] { 1000 };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_flatBatch = m_primitivesRenderer3D.FlatBatch(0, DepthStencilState.None);
		m_textBatch = m_primitivesRenderer3D.FontBatch(BitmapFont.DebugFont, 0, DepthStencilState.None);
		Project.OnProjectLoad += OnProjectLoad;
	}

	private void OnProjectLoad(Project project)
	{
		Project.OnProjectLoad -= OnProjectLoad;
		foreach (Entity entity in Project.Entities)
		{
			ComponentLEDScreen componentLEDScreen = entity.FindComponent<ComponentLEDScreen>();
			if (componentLEDScreen != null)
			{
				m_groupEntityAnchors.Add(componentLEDScreen.m_componentBlockEntity.Coordinates);
			}
		}
		if (m_groupEntityAnchors.Count > 0)
		{
			SchedulePostLoadSync();
		}
	}

	public void SchedulePostLoadSync()
	{
		m_pendingPostLoadSync = true;
		m_postLoadSyncDelayFrames = 0;
	}

	public bool IsThereAController(CellFace cellFace)
	{
		return m_positionToControllers.ContainsKey(cellFace);
	}

	public LEDScreenController? GetController(CellFace cellFace)
	{
		if (!m_positionToControllers.TryGetValue(cellFace, out LEDScreenController value))
		{
			return null;
		}
		return value;
	}

	public CellFace[] GetCellFaces(LEDScreenController controller)
	{
		List<CellFace> list = new List<CellFace>();
		foreach (KeyValuePair<CellFace, LEDScreenController> positionToController in m_positionToControllers)
		{
			if (positionToController.Value == controller)
			{
				list.Add(positionToController.Key);
			}
		}
		return list.ToArray();
	}

	public void AddControllerByPoints(CellFace[] cellFaces, LEDScreenController controller)
	{
		foreach (CellFace key in cellFaces)
		{
			m_positionToControllers.TryAdd(key, controller);
		}
	}

	public void RemoveController(LEDScreenController controller)
	{
		RemoveControllerFromMap(controller);
		SyncGroupEntities();
	}

	private void RemoveControllerFromMap(LEDScreenController controller)
	{
		CellFace[] cellFaces = GetCellFaces(controller);
		foreach (CellFace key in cellFaces)
		{
			m_positionToControllers.Remove(key);
		}
	}

	public int GetControllersCount()
	{
		return m_positionToControllers.Values.Distinct().Count();
	}

	public void UpdateController(int x, int y, int z, int face, bool syncEntities = true)
	{
		Dictionary<CellFace, int> dictionary = new Dictionary<CellFace, int>();
		CollectConnectedLEDScreens(x, y, z, face, dictionary);
		foreach (CellFace key in dictionary.Keys)
		{
			if (IsThereAController(key))
			{
				RemoveControllerFromMap(GetController(key));
			}
		}
		if (dictionary.Count != 0)
		{
			AddControllerByPoints(dictionary.Keys.ToArray(), new LEDScreenController(dictionary));
			if (syncEntities)
			{
				SyncGroupEntities();
			}
		}
	}

	public CellFace GetAnchorCellFace(LEDScreenController controller)
	{
		return controller.GetAnchorCellFace();
	}

	public bool TryGetEntityForController(LEDScreenController controller, out Entity? entity)
	{
		if (TryFindGroupContainer(controller, out ComponentLEDScreen component) && component != null)
		{
			entity = component.Entity;
			return true;
		}
		entity = null;
		return false;
	}

	private bool TryFindGroupContainer(LEDScreenController controller, out ComponentLEDScreen? component)
	{
		foreach (CellFace key in controller.m_cells.Keys)
		{
			if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, key.Point, out ComponentBlockEntity blockEntity))
			{
				component = blockEntity.Entity.FindComponent<ComponentLEDScreen>();
				if (component != null)
				{
					return true;
				}
			}
		}
		component = null;
		return false;
	}

	private static bool IsLedBlockAt(SubsystemTerrain terrain, Point3 point)
	{
		int cellValueFast = terrain.Terrain.GetCellValueFast(point.X, point.Y, point.Z);
		if (Terrain.ExtractContents(cellValueFast) == BaseDeviceBlock.Index)
		{
			return DevicesBlockManager.GetDevice(cellValueFast) is LEDScreenDevice;
		}
		return false;
	}

	private bool ShouldRemoveOrphanLedEntity(Point3 anchor, HashSet<Point3> desiredAnchors, HashSet<LEDScreenController> controllers)
	{
		if (desiredAnchors.Contains(anchor))
		{
			return false;
		}
		if (IsLedBlockAt(m_subsystemTerrain, anchor))
		{
			return false;
		}
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, anchor, out ComponentBlockEntity blockEntity))
		{
			ComponentLEDScreen componentLEDScreen = blockEntity.Entity.FindComponent<ComponentLEDScreen>();
			if (componentLEDScreen != null)
			{
				foreach (LEDScreenController controller in controllers)
				{
					if (TryFindGroupContainer(controller, out ComponentLEDScreen component) && component == componentLEDScreen)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool TryGetContainerAtBlock(Point3 point, out ComponentLEDScreen? component)
	{
		component = null;
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(point.X, point.Y, point.Z);
		if (Terrain.ExtractContents(cellValueFast) != BaseDeviceBlock.Index)
		{
			return false;
		}
		if (!(DevicesBlockManager.GetDevice(cellValueFast) is LEDScreenDevice))
		{
			return false;
		}
		int direction_Static = LEDScreenDevice.GetDirection_Static(cellValueFast);
		LEDScreenController controller = GetController(new CellFace(point.X, point.Y, point.Z, direction_Static));
		if (controller == null || !TryGetEntityForController(controller, out Entity entity) || entity == null)
		{
			return false;
		}
		component = entity.FindComponent<ComponentLEDScreen>();
		return component != null;
	}

	public void SyncGroupEntities()
	{
		HashSet<Point3> hashSet = new HashSet<Point3>();
		HashSet<LEDScreenController> hashSet2 = new HashSet<LEDScreenController>();
		foreach (LEDScreenController value in m_positionToControllers.Values)
		{
			if (hashSet2.Add(value))
			{
				hashSet.Add(GetAnchorCellFace(value).Point);
			}
		}
		if (hashSet.Count > 0)
		{
			Point3[] array = m_groupEntityAnchors.ToArray();
			foreach (Point3 anchor in array)
			{
				if (ShouldRemoveOrphanLedEntity(anchor, hashSet, hashSet2))
				{
					RemoveLedGroupEntity(anchor);
				}
			}
			foreach (Entity entity in Project.Entities)
			{
				ComponentLEDScreen componentLEDScreen = entity.FindComponent<ComponentLEDScreen>();
				if (componentLEDScreen != null)
				{
					Point3 coordinates = componentLEDScreen.m_componentBlockEntity.Coordinates;
					if (ShouldRemoveOrphanLedEntity(coordinates, hashSet, hashSet2))
					{
						RemoveLedGroupEntity(coordinates);
					}
				}
			}
		}
		foreach (LEDScreenController item in hashSet2)
		{
			EnsureGroupEntity(item);
		}
	}

	private void EnsureGroupEntity(LEDScreenController controller)
	{
		if (TryFindGroupContainer(controller, out ComponentLEDScreen component) && component != null)
		{
			m_groupEntityAnchors.Add(component.m_componentBlockEntity.Coordinates);
			return;
		}
		CellFace anchorCellFace = GetAnchorCellFace(controller);
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, anchorCellFace.Point, out ComponentBlockEntity _))
		{
			BlockEntityUtils.RemoveBlockEntity(m_subsystemTerrain, anchorCellFace.Point);
		}
		BlockEntityUtils.CreateBlockEntity(m_subsystemTerrain, "LEDScreenGroup", anchorCellFace.Point);
		m_groupEntityAnchors.Add(anchorCellFace.Point);
	}

	internal void RemoveLedGroupEntity(Point3 anchor)
	{
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, anchor, out ComponentBlockEntity blockEntity) && blockEntity.Entity.FindComponent<ComponentLEDScreen>() != null)
		{
			BlockEntityUtils.RemoveBlockEntity(m_subsystemTerrain, anchor);
		}
		m_groupEntityAnchors.Remove(anchor);
	}

	private void CollectConnectedLEDScreens(int x, int y, int z, int face, Dictionary<CellFace, int> parent)
	{
		CellFace key = new CellFace(x, y, z, face);
		if (parent.ContainsKey(key))
		{
			return;
		}
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
		if (Terrain.ExtractContents(cellValueFast) == BaseDeviceBlock.Index && DevicesBlockManager.GetDevice(cellValueFast) is LEDScreenDevice && LEDScreenDevice.GetDirection_Static(cellValueFast) == face)
		{
			parent.Add(key, cellValueFast);
			DevicesUtils.FaceToAxesAndConner(face, out Point3[] axes, out Point3[] _);
			Point3 point = new Point3(x, y, z);
			for (int i = 0; i < 4; i++)
			{
				Point3 point2 = point + axes[i];
				CollectConnectedLEDScreens(point2.X, point2.Y, point2.Z, face, parent);
			}
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (!m_debugCanDraw)
		{
			return;
		}
		HashSet<LEDScreenController> hashSet = new HashSet<LEDScreenController>();
		foreach (KeyValuePair<CellFace, LEDScreenController> positionToController in m_positionToControllers)
		{
			CellFace key = positionToController.Key;
			LEDScreenController value = positionToController.Value;
			m_flatBatch.QueueBoundingBox(new BoundingBox(new Vector3(key.X, key.Y, key.Z), new Vector3(key.X, key.Y, key.Z) + Vector3.One), Color.DarkCyan);
			if (hashSet.Add(value))
			{
				string text = ((TryGetEntityForController(value, out Entity entity) && entity != null) ? ("entity " + entity.Id) : "no entity");
				CellFace anchorCellFace = GetAnchorCellFace(value);
				Vector3 vector = camera.ViewDirection.Cross(Vector3.UnitY).Normalize();
				Vector3 vector2 = -vector.Cross(camera.ViewDirection).Normalize();
				float num = 0.006f;
				Vector3 position = new Vector3(anchorCellFace.X, anchorCellFace.Y, anchorCellFace.Z) + new Vector3(0.5f);
				m_textBatch.QueueText(value.ID + "\r\n" + value.ControlledCount + " cells\r\n" + GetControllersCount() + " groups\r\n" + text + "\r\nanchor " + anchorCellFace.X + "," + anchorCellFace.Y + "," + anchorCellFace.Z + "\r\nmax rect " + value.InscribedRect.Width + "x" + value.InscribedRect.Height + " area=" + value.InscribedRect.Area, position, vector * num, vector2 * num, Color.White, TextAnchor.Center, Vector2.Zero);
				LEDScreenGroupingUtils.QueueInscribedRectOutline(m_flatBatch, value.MountFace, value.AnchorPoint, value.InscribedRect, Color.Yellow);
			}
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}

	public void Update(float dt)
	{
		if (Keyboard.IsKeyDownOnce(Key.F5))
		{
			m_debugCanDraw = !m_debugCanDraw;
		}
		if (m_pendingPostLoadSync)
		{
			m_postLoadSyncDelayFrames++;
			if (m_postLoadSyncDelayFrames >= 5)
			{
				m_pendingPostLoadSync = false;
				SyncGroupEntities();
			}
		}
	}
}
