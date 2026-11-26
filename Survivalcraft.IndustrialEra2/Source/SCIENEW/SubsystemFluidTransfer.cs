using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Engine.Media;
using Game;
using GameEntitySystem;
using SCIENEW.Benchmark;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemFluidTransfer : Subsystem, IDrawable, IUpdateable
{
	private PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	private FlatBatch3D m_flatBath;

	private FontBatch3D m_textBatch;

	protected double m_lastUpdateRealTime;

	protected float m_gameTimeDT;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	protected bool m_debugCanDraw;

	private readonly Dictionary<Point3, FluidTransferrer> m_elements = new Dictionary<Point3, FluidTransferrer>();

	public Dictionary<CellFace, FluidConnector> m_connectors = new Dictionary<CellFace, FluidConnector>();

	public List<FluidConnection> m_connections = new List<FluidConnection>();

	public Dictionary<Point3, PipeFluidConnection> m_pipeConnections = new Dictionary<Point3, PipeFluidConnection>();

	private readonly HashSet<PipeFluidConnection> m_pipeConnectionGroups = new HashSet<PipeFluidConnection>();

	public int[] DrawOrders => new int[1] { 1000 };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_flatBath = m_primitivesRenderer3D.FlatBatch(0, DepthStencilState.None);
		m_textBatch = m_primitivesRenderer3D.FontBatch(BitmapFont.DebugFont, 0, DepthStencilState.None);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_lastUpdateRealTime = Time.RealTime;
		m_gameTimeDT = 0f;
	}

	public void OnDeviceGenerated(int x, int y, int z)
	{
		OnDeviceAdded(x, y, z);
	}

	public void OnDeviceAdded(int x, int y, int z)
	{
		Point3 point = new Point3(x, y, z);
		IFluidTransferrerDevice fluidTransferrerDevice = BlockInterfaceResolver.ResolveAt<IFluidTransferrerDevice>(m_subsystemTerrain, point, out var blockValue);
		if (fluidTransferrerDevice == null)
		{
			return;
		}
		FluidTransferrer fluidTransferrer = fluidTransferrerDevice.CreateFluidTransferrer(this, m_subsystemTerrain, blockValue, new Point3(x, y, z));
		if (fluidTransferrer == null)
		{
			return;
		}
		if (m_elements.TryGetValue(point, out FluidTransferrer value))
		{
			if (value.GetType() != fluidTransferrer.GetType())
			{
				m_elements.Remove(point);
				fluidTransferrer.Position = point;
				m_elements.Add(point, fluidTransferrer);
				fluidTransferrer.OnAdded();
			}
		}
		else
		{
			fluidTransferrer.Position = point;
			m_elements.Add(point, fluidTransferrer);
			fluidTransferrer.OnAdded();
		}
		UpdateConnectorsAndConnections(point, fluidTransferrerDevice, blockValue);
	}

	public void OnDeviceRemoved(int x, int y, int z)
	{
		Point3 point = new Point3(x, y, z);
		if (m_elements.ContainsKey(point))
		{
			m_elements[point].OnRemoved();
			m_elements.Remove(point);
		}
		UpdateConnectorsAndConnections(point, null, -1);
	}

	public void OnDeviceModified(int x, int y, int z)
	{
		OnDeviceRemoved(x, y, z);
		OnDeviceAdded(x, y, z);
	}

	public void UpdateConnectorsAndConnections(Point3 point, IFluidTransferrerDevice? device, int blockValue)
	{
		Dictionary<CellFace, FluidConnector> dictionary = new Dictionary<CellFace, FluidConnector>(m_connectors);
		foreach (KeyValuePair<CellFace, FluidConnector> item in dictionary)
		{
			if (item.Key.Point == point)
			{
				m_connectors.Remove(item.Key);
			}
		}
		dictionary.Clear();
		List<FluidConnection> list = new List<FluidConnection>(m_connections);
		foreach (FluidConnection item2 in list)
		{
			if (!m_connectors.ContainsValue(item2.FromConnector))
			{
				m_connections.Remove(item2);
			}
			if (!m_connectors.ContainsValue(item2.ToConnector))
			{
				m_connections.Remove(item2);
			}
		}
		list.Clear();
		for (int i = 0; i < 6; i++)
		{
			if (device == null)
			{
				UpdatePipeGroup(point + CellFace.FaceToPoint3(i));
				continue;
			}
			FluidConnector connector = device.GetConnector(m_subsystemTerrain.Terrain, blockValue, i, point.X, point.Y, point.Z);
			if (connector == null)
			{
				continue;
			}
			m_connectors.Add(connector.MountedFace, connector);
			FluidConnector neighborConnector = GetNeighborConnector(connector.MountedFace);
			PipeFluidConnection connection;
			if (neighborConnector != null)
			{
				FluidConnection fluidConnection = CreateConnection(connector, neighborConnector);
				if (fluidConnection != null)
				{
					m_connections.Add(fluidConnection);
				}
			}
			else if (GetNeighborPipeConnection(connector.MountedFace, out connection))
			{
				UpdatePipeGroup(connector.MountedFace.Point + CellFace.FaceToPoint3(connector.MountedFace.Face));
			}
		}
	}

	public FluidConnector? GetNeighborConnector(CellFace cellFace)
	{
		Point3 point = cellFace.Point + CellFace.FaceToPoint3(cellFace.Face);
		int face = CellFace.OppositeFace(cellFace.Face);
		CellFace key = new CellFace(point.X, point.Y, point.Z, face);
		if (m_connectors.TryGetValue(key, out FluidConnector value))
		{
			return value;
		}
		return null;
	}

	public FluidConnection? CreateConnection(FluidConnector connector1, FluidConnector connector2)
	{
		if (connector1.ConnectorType == connector2.ConnectorType)
		{
			return null;
		}
		float rate = MathUtils.Min(connector1.Rate, connector2.Rate);
		if ((connector1.ConnectorType & FluidConnectorType.Input) != 0 && (connector2.ConnectorType & FluidConnectorType.Output) != 0)
		{
			return new FluidConnection(m_elements[connector2.MountedFace.Point], m_elements[connector1.MountedFace.Point], connector2, connector1, rate);
		}
		if ((connector1.ConnectorType & FluidConnectorType.Output) != 0 && (connector2.ConnectorType & FluidConnectorType.Input) != 0)
		{
			return new FluidConnection(m_elements[connector1.MountedFace.Point], m_elements[connector2.MountedFace.Point], connector1, connector2, rate);
		}
		return null;
	}

	public bool GetNeighborPipeConnection(CellFace cellFace, out PipeFluidConnection? connection)
	{
		Point3 key = cellFace.Point + CellFace.FaceToPoint3(cellFace.Face);
		return m_pipeConnections.TryGetValue(key, out connection);
	}

	public void OnPipeGenerated(int x, int y, int z)
	{
		OnPipeAdded(x, y, z);
	}

	public void OnPipeAdded(int x, int y, int z)
	{
		UpdatePipeGroup(new Point3(x, y, z));
	}

	public void OnPipeRemoved(int x, int y, int z)
	{
		Point3 point = new Point3(x, y, z);
		if (!m_pipeConnections.TryGetValue(point, out PipeFluidConnection value))
		{
			return;
		}
		foreach (KeyValuePair<Point3, PipeFluidConnection> pipeConnection in m_pipeConnections)
		{
			if (pipeConnection.Value == value)
			{
				m_pipeConnections.Remove(pipeConnection.Key);
			}
		}
		m_pipeConnectionGroups.Remove(value);
		for (int i = 0; i < 6; i++)
		{
			UpdatePipeGroup(point + CellFace.FaceToPoint3(i));
		}
	}

	public void OnPipeModified(int x, int y, int z)
	{
		OnPipeRemoved(x, y, z);
		OnPipeAdded(x, y, z);
	}

	private void UpdatePipeGroup(Point3 point)
	{
		HashSet<Point3> hashSet = new HashSet<Point3>();
		IFluidTransferrerPipeDevice fluidTransferrerPipeDevice = BlockInterfaceResolver.ResolveAt<IFluidTransferrerPipeDevice>(m_subsystemTerrain, point, out var blockValue);
		if (fluidTransferrerPipeDevice == null)
		{
			return;
		}
		GetPipeGroup(point, hashSet, fluidTransferrerPipeDevice.GetTransferringFluidType());
		HashSet<PipeFluidConnection> hashSet2 = new HashSet<PipeFluidConnection>();
		foreach (Point3 item in hashSet)
		{
			if (m_pipeConnections.TryGetValue(item, out PipeFluidConnection value))
			{
				hashSet2.Add(value);
			}
			m_pipeConnections.Remove(item);
		}
		foreach (PipeFluidConnection item2 in hashSet2)
		{
			m_pipeConnectionGroups.Remove(item2);
		}
		HashSet<FluidConnector> hashSet3 = new HashSet<FluidConnector>();
		HashSet<FluidConnector> hashSet4 = new HashSet<FluidConnector>();
		HashSet<FluidTransferrer> hashSet5 = new HashSet<FluidTransferrer>();
		HashSet<FluidTransferrer> hashSet6 = new HashSet<FluidTransferrer>();
		float num = float.MaxValue;
		foreach (Point3 item3 in hashSet)
		{
			for (int i = 0; i < 6; i++)
			{
				if (!fluidTransferrerPipeDevice.CanConnect(blockValue, new CellFace(point.X, point.Y, point.Z, i), m_subsystemTerrain.Terrain))
				{
					continue;
				}
				Point3 point2 = item3 + CellFace.FaceToPoint3(i);
				int face = CellFace.OppositeFace(i);
				if (hashSet.Contains(point2))
				{
					continue;
				}
				CellFace key = new CellFace(point2.X, point2.Y, point2.Z, face);
				if (m_connectors.TryGetValue(key, out FluidConnector value2))
				{
					if (value2.ConnectorType == FluidConnectorType.Input)
					{
						hashSet4.Add(value2);
						hashSet6.Add(m_elements[point2]);
						num = MathF.Min(num, value2.Rate);
					}
					else if (value2.ConnectorType == FluidConnectorType.Output)
					{
						hashSet3.Add(value2);
						hashSet5.Add(m_elements[point2]);
						num = MathF.Min(num, value2.Rate);
					}
				}
			}
		}
		PipeFluidConnection connection = fluidTransferrerPipeDevice.GetConnection(hashSet5, hashSet6, hashSet3, hashSet4, num, fluidTransferrerPipeDevice.GetTransferringFluidType(), this);
		foreach (Point3 item4 in hashSet)
		{
			m_pipeConnections.Add(item4, connection);
		}
		m_pipeConnectionGroups.Add(connection);
		connection.Init();
	}

	private void GetPipeGroup(Point3 point, HashSet<Point3> parents, Type acquiredType, int? parentFace = null)
	{
		if (parents.Contains(point))
		{
			return;
		}
		IFluidTransferrerPipeDevice fluidTransferrerPipeDevice = BlockInterfaceResolver.ResolveAt<IFluidTransferrerPipeDevice>(m_subsystemTerrain, point, out var blockValue);
		if (fluidTransferrerPipeDevice == null || !(fluidTransferrerPipeDevice.GetTransferringFluidType() == acquiredType))
		{
			return;
		}
		if (!parentFace.HasValue)
		{
			parents.Add(point);
		}
		else
		{
			int face = CellFace.OppositeFace(parentFace.Value);
			if (!fluidTransferrerPipeDevice.CanConnect(blockValue, new CellFace(point.X, point.Y, point.Z, face), m_subsystemTerrain.Terrain))
			{
				return;
			}
			parents.Add(point);
		}
		for (int i = 0; i < 6; i++)
		{
			if (fluidTransferrerPipeDevice.CanConnect(blockValue, new CellFace(point.X, point.Y, point.Z, i), m_subsystemTerrain.Terrain))
			{
				GetPipeGroup(point + CellFace.FaceToPoint3(i), parents, acquiredType, i);
			}
		}
	}

	public void Update(float dt)
	{
		if (Keyboard.IsKeyDownOnce(Key.F3))
		{
			m_debugCanDraw = !m_debugCanDraw;
		}
		double num = Time.RealTime - m_lastUpdateRealTime;
		m_gameTimeDT += dt;
		if (!(num >= 0.05000000074505806))
		{
			return;
		}
		long timestamp = Stopwatch.GetTimestamp();
		m_lastUpdateRealTime = Time.RealTime;
		foreach (KeyValuePair<Point3, FluidTransferrer> element in m_elements)
		{
			element.Value.Simulate(m_gameTimeDT);
		}
		foreach (FluidConnection connection in m_connections)
		{
			connection.Simulate(m_gameTimeDT);
		}
		foreach (PipeFluidConnection pipeConnectionGroup in m_pipeConnectionGroups)
		{
			pipeConnectionGroup.Simulate(m_gameTimeDT);
		}
		m_gameTimeDT = 0f;
		float elapsedSeconds = (float)(Stopwatch.GetTimestamp() - timestamp) / (float)Stopwatch.Frequency;
		IEBenchmarkEvents.PublishSubsystemUpdate("SubsystemFluidTransfer", elapsedSeconds, m_elements.Count);
	}

	public void Draw(Camera camera, int drawOrder)
	{
		foreach (KeyValuePair<Point3, FluidTransferrer> element in m_elements)
		{
			if (element.Value is IDrawable drawable)
			{
				drawable.Draw(camera, drawOrder);
			}
		}
		foreach (KeyValuePair<Point3, PipeFluidConnection> pipeConnection in m_pipeConnections)
		{
			if (pipeConnection.Value is IDrawable drawable2)
			{
				drawable2.Draw(camera, drawOrder);
			}
		}
		if (!m_debugCanDraw)
		{
			return;
		}
		foreach (KeyValuePair<Point3, FluidTransferrer> element2 in m_elements)
		{
			m_flatBath.QueueBoundingBox(new BoundingBox(new Vector3(element2.Key), new Vector3(element2.Key) + Vector3.One), Color.Yellow);
		}
		foreach (KeyValuePair<CellFace, FluidConnector> connector in m_connectors)
		{
			Vector3 vector = new Vector3(connector.Key.Point) + new Vector3(0.5f);
			vector += CellFace.FaceToVector3(connector.Key.Face) * 0.4f;
			Color color = new Color(0, 255, 255);
			switch (connector.Value.ConnectorType)
			{
			case FluidConnectorType.Input:
				color = new Color(0, 255, 0);
				break;
			case FluidConnectorType.Output:
				color = new Color(0, 0, 255);
				break;
			}
			m_flatBath.QueueBoundingBox(new BoundingBox(vector - Vector3.One * 0.1f, vector + Vector3.One * 0.1f), color);
		}
		foreach (FluidConnection connection in m_connections)
		{
			Vector3 p = new Vector3(connection.FromConnector.MountedFace.Point) + new Vector3(0.5f);
			p += CellFace.FaceToVector3(connection.FromConnector.MountedFace.Face) * 0.4f;
			Vector3 p2 = new Vector3(connection.ToConnector.MountedFace.Point) + new Vector3(0.5f);
			p2 += CellFace.FaceToVector3(connection.ToConnector.MountedFace.Face) * 0.4f;
			m_flatBath.QueueLine(p, p2, Color.Purple, Color.Pink);
		}
		foreach (KeyValuePair<Point3, PipeFluidConnection> pipeConnection2 in m_pipeConnections)
		{
			m_flatBath.QueueBoundingBox(new BoundingBox(new Vector3(pipeConnection2.Key), new Vector3(pipeConnection2.Key) + Vector3.One), Color.Orange);
			Vector3 vector2 = camera.ViewDirection.Cross(Vector3.UnitY).Normalize();
			Vector3 vector3 = -Vector3.Normalize(vector2.Cross(camera.ViewDirection));
			float num = 0.006f;
			m_textBatch.QueueText(pipeConnection2.Value.GetHashCode() + "\r\n\r\n" + m_pipeConnectionGroups.Count, new Vector3(pipeConnection2.Key) + new Vector3(0.5f), vector2 * num, vector3 * num, Color.White, TextAnchor.Center, Vector2.Zero);
		}
		foreach (PipeFluidConnection pipeConnectionGroup in m_pipeConnectionGroups)
		{
			Vector3 vector4 = pipeConnectionGroup.CalcAveragePosition();
			Color color2 = Color.Lerp(Color.Purple, Color.Pink, 0.5f);
			foreach (FluidConnector fromConnector in pipeConnectionGroup.FromConnectors)
			{
				Vector3 p3 = new Vector3(fromConnector.MountedFace.Point) + new Vector3(0.5f);
				p3 += CellFace.FaceToVector3(fromConnector.MountedFace.Face) * 0.4f;
				m_flatBath.QueueLine(p3, vector4, Color.Purple, color2);
			}
			foreach (FluidConnector toConnector in pipeConnectionGroup.ToConnectors)
			{
				Vector3 p4 = new Vector3(toConnector.MountedFace.Point) + new Vector3(0.5f);
				p4 += CellFace.FaceToVector3(toConnector.MountedFace.Face) * 0.4f;
				m_flatBath.QueueLine(vector4, p4, color2, Color.Pink);
			}
			Vector3 vector5 = Vector3.Normalize(camera.ViewDirection.Cross(Vector3.UnitY));
			Vector3 vector6 = -Vector3.Normalize(vector5.Cross(camera.ViewDirection));
			float num2 = 0.006f;
			foreach (FluidTransferrer fromTransferrer in pipeConnectionGroup.FromTransferrers)
			{
				m_textBatch.QueueText(pipeConnectionGroup.GetHashCode() + "\r\n\r\n" + m_pipeConnectionGroups.Count, new Vector3(fromTransferrer.Position) + new Vector3(0.5f), vector5 * num2, vector6 * num2, Color.White, TextAnchor.Center, Vector2.Zero);
			}
			foreach (FluidTransferrer toTransferrer in pipeConnectionGroup.ToTransferrers)
			{
				m_textBatch.QueueText(pipeConnectionGroup.GetHashCode() + "\r\n\r\n" + m_pipeConnectionGroups.Count, new Vector3(toTransferrer.Position) + new Vector3(0.5f), vector5 * num2, vector6 * num2, Color.White, TextAnchor.Center, Vector2.Zero);
			}
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}

	public FluidTransferrer? GetTransferrer(Point3 point)
	{
		return m_elements.GetValueOrDefault(point);
	}

	public bool TryGetTransferrer(Point3 point, out FluidTransferrer? transferrer)
	{
		return m_elements.TryGetValue(point, out transferrer);
	}

	public bool TryGetTransferrer<T>(Point3 point, out T? transferrer) where T : FluidTransferrer
	{
		if (m_elements.TryGetValue(point, out FluidTransferrer value) && value is T val)
		{
			transferrer = val;
			return true;
		}
		transferrer = null;
		return false;
	}
}
