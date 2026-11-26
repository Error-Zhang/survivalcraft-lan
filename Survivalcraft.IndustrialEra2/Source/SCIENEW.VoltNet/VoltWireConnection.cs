using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.VoltNet;

public class VoltWireConnection : VoltConnection
{
	private const float OverloadCompareEpsilon = 0.01f;

	public readonly float m_standardUL;

	public readonly float m_standardPL;

	private Game.Random m_random = new Game.Random();

	private float m_overLoadedTime;

	private static bool ExceedsRating(float actual, float limit)
	{
		if (limit <= 0f)
		{
			return actual > 0.01f;
		}
		return actual > limit + Math.Max(0.01f, limit * 0.0001f);
	}

	public VoltWireConnection(Dictionary<CellFace, VoltElement> containsElements, SubsystemVoltNet subsystemVoltNet, HashSet<Point3> wireGroup)
		: base(containsElements, subsystemVoltNet)
	{
		m_standardPL = float.MaxValue;
		m_standardUL = float.MaxValue;
		foreach (Point3 item in wireGroup)
		{
			IVoltWireDevice voltWireDevice = BlockInterfaceResolver.ResolveAt<IVoltWireDevice>(subsystemVoltNet.m_subsystemTerrain, item, out var blockValue);
			if (voltWireDevice != null && voltWireDevice.GetMaterial() != Materials.None && !(voltWireDevice is SteelFrameDevice { m_containsWireID: null }))
			{
				m_standardPL = Math.Min(m_standardPL, voltWireDevice.GetPL(blockValue));
				m_standardUL = Math.Min(m_standardUL, voltWireDevice.GetUL(blockValue));
			}
		}
		if (m_standardPL == float.MaxValue)
		{
			m_standardPL = 0f;
		}
		if (m_standardUL == float.MaxValue)
		{
			m_standardUL = 0f;
		}
	}

	public void AddVoltElement(VoltElement voltElement, CellFace cellFace)
	{
		if (m_containsElements != null && !m_containsElements.Values.Contains(voltElement))
		{
			voltElement.m_belongingConnections.Add(this);
			m_containsElements[cellFace] = voltElement;
			m_outputToElements.TryAdd(voltElement, (0f, 0f));
		}
	}

	public override void Simulate(float dt)
	{
		base.Simulate(dt);
		if (ExceedsRating(m_totalInputPower, m_standardPL) || ExceedsRating(m_inputVoltage, m_standardUL))
		{
			if (m_overLoadedTime >= 2f)
			{
				DestroyRandomWire();
				m_overLoadedTime = 0f;
			}
			else
			{
				m_overLoadedTime = Math.Min(m_overLoadedTime + dt, 2f);
			}
		}
		else
		{
			m_overLoadedTime = 0f;
		}
	}

	protected override void HandleVoltageMismatch()
	{
		DestroyRandomWire();
	}

	private void DestroyRandomWire()
	{
		Point3[] array = (from x in SubsystemVoltNet.m_wireConnections.AsValueEnumerable()
			where x.Value == this
			select x).Select<FromDictionary<Point3, VoltWireConnection>, KeyValuePair<Point3, VoltWireConnection>, Point3>((Func<KeyValuePair<Point3, VoltWireConnection>, Point3>)((KeyValuePair<Point3, VoltWireConnection> x) => x.Key)).ToArray<FromDictionary<Point3, VoltWireConnection>, KeyValuePair<Point3, VoltWireConnection>, Point3>();
		Point3[] array2 = array.AsValueEnumerable().Where((Point3 point) =>
		{
			IVoltWireDevice voltWireDevice = BlockInterfaceResolver.ResolveAt<IVoltWireDevice>(SubsystemVoltNet.m_subsystemTerrain, point, out var _);
			return voltWireDevice != null && voltWireDevice.GetMaterial() != Materials.None && (!(voltWireDevice is SteelFrameDevice steelFrameDevice) || steelFrameDevice.m_containsWireID != null);
		}).ToArray();
		if (array2.Length == 0)
		{
			array2 = array;
		}
		Point3 p = array2[m_random.Int(0, array2.Length - 1)];
		SubsystemVoltNet.m_subsystemTerrain.DestroyCell(0, p.X, p.Y, p.Z, 0, noDrop: false, noParticleSystem: true);
		SubsystemVoltNet.Project.FindSubsystem<SubsystemParticles>().AddParticleSystem(new MagmaSplashParticleSystem(SubsystemVoltNet.m_subsystemTerrain, new Vector3(p) + new Vector3(0.5f), large: false));
		SubsystemVoltNet.Project.FindSubsystem<SubsystemAudio>().PlayRandomSound("Audio/Sizzles", 0.5f, m_random.Float(-0.2f, 0.2f), new Vector3(p) + new Vector3(0.5f), 3f, autoDelay: false);
	}
}
