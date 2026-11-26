using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Engine.Media;
using Game;
using GameEntitySystem;
using SCIENEW.Benchmark;
using SCIENEW.Extensions;
using SCIENEW.Utils;
using TemplatesDatabase;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.VoltNet;

public class SubsystemVoltNet : Subsystem, IUpdateable, IDrawable
{
	public const float MaxPower = 64f;

	public const float MaxVoltage = 4f;

	public const float MinDamagePower = 1f;

	public const float MinDamage = 0.5f;

	public const float MaxDamagePower = 12f;

	public const float MaxDamage = 10f;

	private PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	private FlatBatch3D m_flatBath;

	private FontBatch3D m_textBatch;

	public bool m_isSuperVision = true;

	public float m_visionLimit = 64f;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	protected bool m_debugCanDraw;

	private double m_lastSimulateTime;

	public Dictionary<Point3, VoltElement> m_elements = new Dictionary<Point3, VoltElement>();

	public List<VoltConnection> m_connections = new List<VoltConnection>();

	public Dictionary<Point3, VoltWireConnection> m_wireConnections = new Dictionary<Point3, VoltWireConnection>();

	private readonly HashSet<VoltWireConnection> m_wireConnectionGroups = new HashSet<VoltWireConnection>();

	private readonly List<VoltWireConnection> m_wireConnectionSimulateBuffer = new List<VoltWireConnection>();

	private readonly List<VoltConnection> m_pendingDirectConnectionRemovals = new List<VoltConnection>();

	private readonly Game.Random m_voltageArcRandom = new Game.Random();

	public Dictionary<Type, int> m_updateTicksCount = new Dictionary<Type, int>();

	public Dictionary<Type, int> m_updateTimesCount = new Dictionary<Type, int>();

	public bool UpdateTimeDebug;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1] { 1000 };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_flatBath = m_primitivesRenderer3D.FlatBatch(0, DepthStencilState.None);
		m_textBatch = m_primitivesRenderer3D.FontBatch(BitmapFont.DebugFont, 0, DepthStencilState.None);
		m_lastSimulateTime = m_subsystemTime.GameTime;
	}

	public override void Save(ValuesDictionary valuesDictionary)
	{
		base.Save(valuesDictionary);
		if (!UpdateTimeDebug)
		{
			return;
		}
		Log.Information(LanguageUtils.GetText(this, "AnalysisText"));
		List<Type> list = m_updateTicksCount.Keys.AsValueEnumerable().ToList();
		int num = (from t in list.AsValueEnumerable()
			select t.FullName.Length).Prepend<ListSelect<Type, int>, int>(0).Max<Prepend<ListSelect<Type, int>, int>, int>();
		foreach (Type item in list)
		{
			string text = string.Format("{0, -" + (num + 5) + "}", item.FullName);
			bool flag = m_updateTimesCount.TryGetValue(item, out var value);
			bool flag2 = m_updateTicksCount.TryGetValue(item, out var value2);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted(flag ? ((object)value) : "Error", -8);
			string text2 = "TimesOfUpdate: " + defaultInterpolatedStringHandler.ToStringAndClear();
			string text3 = "TimeOfUpdate: " + $"{(flag2 ? ((object)value2) : "Error")?.ToString() + "ms",-10}";
			string text4 = $"Average: {(float)value2 / (float)value + "ms",-10}";
			Log.Information(text + text2 + text3 + text4);
		}
		Log.Information(LanguageUtils.GetText(this, "AnalysisText"));
		m_updateTicksCount.Clear();
		m_updateTimesCount.Clear();
	}

	public void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		OnBlockAdded(value, 0, x, y, z);
	}

	public void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		IVoltDevice voltDevice = BlockInterfaceResolver.ResolveFromValue<IVoltDevice>(value);
		if (voltDevice == null)
		{
			return;
		}
		Point3 point = new Point3(x, y, z);
		VoltElement voltElement = voltDevice.GetVoltElement(this, m_subsystemTerrain, point, value);
		if (voltElement == null)
		{
			return;
		}
		if (m_elements.TryGetValue(point, out VoltElement value2))
		{
			if (value2.GetType() != voltElement.GetType())
			{
				m_elements[point].OnRemoved();
				m_elements.Remove(point);
				voltElement.Position = point;
				m_elements.Add(point, voltElement);
				voltElement.OnAdded();
			}
		}
		else
		{
			voltElement.Position = point;
			m_elements.Add(point, voltElement);
			voltElement.OnAdded();
		}
		UpdateConnections(point, voltDevice, value);
	}

	public void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		Point3 point = new Point3(x, y, z);
		if (m_elements.ContainsKey(point))
		{
			m_elements[point].OnRemoved();
			m_elements.Remove(point);
		}
		UpdateConnections(point, null, -1);
	}

	public void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		OnBlockRemoved(oldValue, value, x, y, z);
		OnBlockAdded(value, oldValue, x, y, z);
	}

	public void UpdateConnections(Point3 position, IVoltDevice? voltDevice, int blockValue)
	{
		List<VoltConnection> list = new List<VoltConnection>(m_connections);
		foreach (VoltConnection item2 in list)
		{
			if (item2.m_containsElements.Keys.AsValueEnumerable().Any((CellFace x) => x.Point == position))
			{
				item2.OnRemove();
				m_connections.Remove(item2);
			}
		}
		list.Clear();
		for (int num = 0; num < 6; num++)
		{
			if (voltDevice == null)
			{
				UpdateWireGroup(position + CellFace.FaceToPoint3(num));
			}
			else
			{
				if (!voltDevice.GetFaceMask(blockValue).Contains(num))
				{
					continue;
				}
				CellFace cellFace = new CellFace(position.X, position.Y, position.Z, num);
				Point3 point = position + CellFace.FaceToPoint3(num);
				CellFace key = new CellFace(point.X, point.Y, point.Z, CellFace.OppositeFace(num));
				VoltElement value3;
				if (m_elements.TryGetValue(point, out VoltElement value) && m_elements.TryGetValue(position, out VoltElement value2))
				{
					IVoltDevice voltDevice2 = BlockInterfaceResolver.ResolveFromValue<IVoltDevice>(value.BlockValue);
					if (voltDevice2 != null && voltDevice2.GetFaceMask(value.BlockValue).Contains(key.Face))
					{
						VoltConnection item = new VoltConnection(new Dictionary<CellFace, VoltElement>
						{
							{ cellFace, value2 },
							{ key, value }
						}, this);
						m_connections.Add(item);
					}
				}
				else if (m_elements.TryGetValue(position, out value3) && m_wireConnections.ContainsKey(point))
				{
					IVoltWireDevice voltWireDevice = BlockInterfaceResolver.ResolveAt<IVoltWireDevice>(m_subsystemTerrain, point, out var blockValue2);
					if (voltWireDevice != null && voltWireDevice.GetFaceMask(blockValue2).Contains(key.Face))
					{
						m_wireConnections[point].AddVoltElement(value3, cellFace);
					}
				}
			}
		}
	}

	public void OnWireAdded(int value, int oldValue, int x, int y, int z)
	{
		UpdateWireGroup(new Point3(x, y, z));
	}

	public void OnWireRemoved(int value, int newValue, int x, int y, int z)
	{
		Point3 point = new Point3(x, y, z);
		if (m_wireConnections.TryGetValue(point, out VoltWireConnection value2))
		{
			RemoveWireConnectionGroup(value2);
			for (int i = 0; i < 6; i++)
			{
				UpdateWireGroup(point + CellFace.FaceToPoint3(i));
			}
		}
	}

	public void OnWireGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		OnWireAdded(value, 0, x, y, z);
	}

	public void OnWireModified(int value, int oldValue, int x, int y, int z)
	{
		OnWireRemoved(oldValue, value, x, y, z);
		OnWireAdded(value, oldValue, x, y, z);
	}

	public void UpdateWireGroup(Point3 point)
	{
		HashSet<Point3> hashSet = new HashSet<Point3>();
		if (BlockInterfaceResolver.ResolveAt<IVoltWireDevice>(m_subsystemTerrain, point, out var _) == null)
		{
			return;
		}
		GetWireGroup(point, hashSet);
		RemoveWireConnectionMappingsInGroup(hashSet);
		VoltWireConnection voltWireConnection = new VoltWireConnection(new Dictionary<CellFace, VoltElement>(), this, hashSet);
		List<Point3> list = new List<Point3>();
		foreach (Point3 item in hashSet)
		{
			IVoltWireDevice voltWireDevice = BlockInterfaceResolver.ResolveAt<IVoltWireDevice>(m_subsystemTerrain, item, out var blockValue2);
			if (voltWireDevice == null)
			{
				list.Add(item);
				continue;
			}
			foreach (int item2 in voltWireDevice.GetFaceMask(blockValue2))
			{
				Point3 point2 = item + CellFace.FaceToPoint3(item2);
				int num = CellFace.OppositeFace(item2);
				if (!hashSet.Contains(point2))
				{
					IVoltDevice voltDevice = BlockInterfaceResolver.ResolveAt<IVoltDevice>(m_subsystemTerrain, point2, out var blockValue3);
					if (m_elements.ContainsKey(point2) && voltDevice != null && voltDevice.GetFaceMask(blockValue3).Contains(num))
					{
						VoltElement voltElement = m_elements[point2];
						voltWireConnection.AddVoltElement(voltElement, new CellFace(point2.X, point2.Y, point2.Z, num));
					}
				}
			}
		}
		foreach (Point3 item3 in list)
		{
			hashSet.Remove(item3);
		}
		foreach (Point3 item4 in hashSet)
		{
			m_wireConnections.Add(item4, voltWireConnection);
		}
		m_wireConnectionGroups.Add(voltWireConnection);
	}

	private void RemoveWireConnectionMappingsInGroup(HashSet<Point3> wireGroup)
	{
		HashSet<VoltWireConnection> hashSet = new HashSet<VoltWireConnection>();
		foreach (Point3 item in wireGroup)
		{
			if (m_wireConnections.Remove(item, out VoltWireConnection value))
			{
				hashSet.Add(value);
			}
		}
		foreach (VoltWireConnection item2 in hashSet)
		{
			item2.OnRemove();
			m_wireConnectionGroups.Remove(item2);
		}
	}

	private void RemoveWireConnectionGroup(VoltWireConnection connection)
	{
		List<Point3> list = new List<Point3>();
		foreach (KeyValuePair<Point3, VoltWireConnection> wireConnection in m_wireConnections)
		{
			if (wireConnection.Value == connection)
			{
				list.Add(wireConnection.Key);
			}
		}
		foreach (Point3 item in list)
		{
			m_wireConnections.Remove(item);
		}
		connection.OnRemove();
		m_wireConnectionGroups.Remove(connection);
	}

	public void GetWireGroup(Point3 currentPoint, HashSet<Point3> parents)
	{
		if (parents.Contains(currentPoint))
		{
			return;
		}
		IVoltWireDevice voltWireDevice = BlockInterfaceResolver.ResolveAt<IVoltWireDevice>(m_subsystemTerrain, currentPoint, out var blockValue);
		if (voltWireDevice == null)
		{
			return;
		}
		parents.Add(currentPoint);
		foreach (int item in voltWireDevice.GetFaceMask(blockValue))
		{
			Point3 point = currentPoint + CellFace.FaceToPoint3(item);
			if (!parents.Contains(point))
			{
				IVoltWireDevice voltWireDevice2 = BlockInterfaceResolver.ResolveAt<IVoltWireDevice>(m_subsystemTerrain, point, out var _);
				if (voltWireDevice2 != null && CanTraverseWireMaterials(voltWireDevice.GetMaterial(), voltWireDevice2.GetMaterial()))
				{
					GetWireGroup(point, parents);
				}
			}
		}
	}

	private static bool CanTraverseWireMaterials(Materials from, Materials to)
	{
		if (from == Materials.None || to == Materials.None)
		{
			return true;
		}
		return from == to;
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (!m_debugCanDraw)
		{
			return;
		}
		foreach (KeyValuePair<Point3, VoltElement> element in m_elements)
		{
			if (m_isSuperVision || !((new Vector2(element.Key.X, element.Key.Z) - new Vector2(camera.ViewPosition.X, camera.ViewPosition.Z)).Length() > m_visionLimit))
			{
				m_flatBath.QueueBoundingBox(new BoundingBox(new Vector3(element.Key), new Vector3(element.Key) + Vector3.One), Color.Green);
				Vector3 vector = camera.ViewDirection.Cross(Vector3.UnitY).Normalize();
				Vector3 vector2 = -vector.Cross(camera.ViewDirection).Normalize();
				float num = 0.006f;
				m_textBatch.QueueText(element.Value.CurrentWorkState.ToString(), new Vector3(element.Key) + new Vector3(0.5f), vector * num, vector2 * num, Color.White, TextAnchor.Center, Vector2.Zero);
			}
		}
		foreach (VoltConnection connection in m_connections)
		{
			Vector3 p = connection.CalcCenterPos();
			if (!m_isSuperVision && (new Vector2(p.X, p.Z) - new Vector2(camera.ViewPosition.X, camera.ViewPosition.Z)).Length() > m_visionLimit)
			{
				continue;
			}
			foreach (KeyValuePair<CellFace, VoltElement> containsElement in connection.m_containsElements)
			{
				if (m_isSuperVision || !((new Vector2(containsElement.Key.X, containsElement.Key.Z) - new Vector2(camera.ViewPosition.X, camera.ViewPosition.Z)).Length() > m_visionLimit))
				{
					Vector3 p2 = new Vector3(containsElement.Key.Point) + new Vector3(0.5f) + CellFace.FaceToVector3(containsElement.Key.Face) * 0.4f;
					m_flatBath.QueueLine(p2, p, Color.Yellow, Color.Orange);
				}
			}
		}
		foreach (VoltWireConnection wireConnectionGroup in m_wireConnectionGroups)
		{
			Vector3 p3 = wireConnectionGroup.CalcCenterPos();
			if (!m_isSuperVision && (new Vector2(p3.X, p3.Z) - new Vector2(camera.ViewPosition.X, camera.ViewPosition.Z)).Length() > m_visionLimit)
			{
				continue;
			}
			foreach (KeyValuePair<CellFace, VoltElement> containsElement2 in wireConnectionGroup.m_containsElements)
			{
				if (m_isSuperVision || !((new Vector2(containsElement2.Key.X, containsElement2.Key.Z) - new Vector2(camera.ViewPosition.X, camera.ViewPosition.Z)).Length() > m_visionLimit))
				{
					Vector3 p4 = new Vector3(containsElement2.Key.Point) + new Vector3(0.5f) + CellFace.FaceToVector3(containsElement2.Key.Face) * 0.4f;
					m_flatBath.QueueLine(p4, p3, Color.Yellow, Color.Orange);
				}
			}
		}
		foreach (KeyValuePair<Point3, VoltWireConnection> wireConnection in m_wireConnections)
		{
			if (m_isSuperVision || !((new Vector2(wireConnection.Key.X, wireConnection.Key.Z) - new Vector2(camera.ViewPosition.X, camera.ViewPosition.Z)).Length() > m_visionLimit))
			{
				m_flatBath.QueueBoundingBox(new BoundingBox(new Vector3(wireConnection.Key), new Vector3(wireConnection.Key) + Vector3.One), new Color(0, 255, 255));
				Vector3 vector3 = camera.ViewDirection.Cross(Vector3.UnitY).Normalize();
				Vector3 vector4 = -vector3.Cross(camera.ViewDirection).Normalize();
				float num2 = 0.006f;
				m_textBatch.QueueText(wireConnection.Value.GetHashCode() + "\r\n\r\n" + m_wireConnections.Values.Distinct().Count(), new Vector3(wireConnection.Key) + new Vector3(0.5f), vector3 * num2, vector4 * num2, Color.White, TextAnchor.Center, Vector2.Zero);
			}
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}

	public void Update(float dt)
	{
		if (m_subsystemTime.PeriodicGameTimeEvent(0.019999999552965164, 0.0))
		{
			long timestamp = Stopwatch.GetTimestamp();
			float dt2 = (float)(m_subsystemTime.GameTime - m_lastSimulateTime);
			foreach (VoltConnection connection in m_connections)
			{
				Type type = connection.GetType();
				int tickCount = Environment.TickCount;
				connection.Simulate(dt2);
				int tickCount2 = Environment.TickCount;
				bool flag = m_updateTicksCount.TryGetValue(type, out var value);
				m_updateTicksCount[type] = (flag ? value : 0) + (tickCount2 - tickCount);
				bool flag2 = m_updateTimesCount.TryGetValue(type, out var value2);
				m_updateTimesCount[type] = (flag2 ? value2 : 0) + 1;
			}
			FlushPendingDirectConnectionRemovals();
			m_wireConnectionSimulateBuffer.Clear();
			m_wireConnectionSimulateBuffer.AddRange(m_wireConnectionGroups);
			foreach (VoltWireConnection item in m_wireConnectionSimulateBuffer)
			{
				Type type2 = item.GetType();
				int tickCount3 = Environment.TickCount;
				item.Simulate(dt2);
				int tickCount4 = Environment.TickCount;
				bool flag3 = m_updateTicksCount.TryGetValue(type2, out var value3);
				m_updateTicksCount[type2] = (flag3 ? value3 : 0) + (tickCount4 - tickCount3);
				bool flag4 = m_updateTimesCount.TryGetValue(type2, out var value4);
				m_updateTimesCount[type2] = (flag4 ? value4 : 0) + 1;
			}
			foreach (VoltElement value7 in m_elements.Values)
			{
				Type type3 = value7.GetType();
				int tickCount5 = Environment.TickCount;
				value7.Simulate(dt2);
				int tickCount6 = Environment.TickCount;
				bool flag5 = m_updateTicksCount.TryGetValue(type3, out var value5);
				m_updateTicksCount[type3] = (flag5 ? value5 : 0) + (tickCount6 - tickCount5);
				bool flag6 = m_updateTimesCount.TryGetValue(type3, out var value6);
				m_updateTimesCount[type3] = (flag6 ? value6 : 0) + 1;
			}
			m_lastSimulateTime = m_subsystemTime.GameTime;
			float elapsedSeconds = (float)(Stopwatch.GetTimestamp() - timestamp) / (float)Stopwatch.Frequency;
			IEBenchmarkEvents.PublishSubsystemUpdate("SubsystemVoltNet", elapsedSeconds, m_elements.Count);
		}
		if (Keyboard.IsKeyDownOnce(Key.F4))
		{
			m_debugCanDraw = !m_debugCanDraw;
		}
	}

	internal void QueueRemoveDirectConnection(VoltConnection connection)
	{
		if (!(connection is VoltWireConnection) && !m_pendingDirectConnectionRemovals.Contains(connection))
		{
			m_pendingDirectConnectionRemovals.Add(connection);
		}
	}

	private void FlushPendingDirectConnectionRemovals()
	{
		foreach (VoltConnection pendingDirectConnectionRemoval in m_pendingDirectConnectionRemovals)
		{
			if (m_connections.Remove(pendingDirectConnectionRemoval))
			{
				pendingDirectConnectionRemoval.OnRemove();
			}
		}
		m_pendingDirectConnectionRemovals.Clear();
	}

	internal void PlayVoltageArc(Vector3 position)
	{
		Project.FindSubsystem<SubsystemParticles>().AddParticleSystem(new MagmaSplashParticleSystem(m_subsystemTerrain, position, large: false));
		Project.FindSubsystem<SubsystemAudio>().PlayRandomSound("Audio/Sizzles", 0.5f, m_voltageArcRandom.Float(-0.2f, 0.2f), position, 3f, autoDelay: false);
	}

	public VoltElement? GetVoltElement(Point3 point)
	{
		return m_elements.GetValueOrDefault(point);
	}

	public float GetWireGroupCurrent(Point3 wirePoint)
	{
		if (!m_wireConnections.TryGetValue(wirePoint, out VoltWireConnection value))
		{
			return 0f;
		}
		return value.GetTransmissionCurrent();
	}

	public float GetConnectedVoltage(Point3 devicePoint)
	{
		if (!m_elements.TryGetValue(devicePoint, out VoltElement value))
		{
			return 0f;
		}
		foreach (VoltConnection belongingConnection in value.m_belongingConnections)
		{
			if (belongingConnection.InputVoltage > 0f)
			{
				return belongingConnection.InputVoltage;
			}
		}
		return 0f;
	}

	public void SwitchDraw(bool isSuperVision)
	{
		m_debugCanDraw = !m_debugCanDraw;
		m_isSuperVision = isSuperVision;
	}

	public static float GetDamage(float power, float voltage)
	{
		return LerpUtils.Map(1f, 12f, 0.5f, 10f, power) * voltage;
	}
}
