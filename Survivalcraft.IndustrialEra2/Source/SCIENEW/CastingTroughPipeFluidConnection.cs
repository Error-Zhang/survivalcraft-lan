using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class CastingTroughPipeFluidConnection(HashSet<FluidTransferrer> fromTransferrers, HashSet<FluidTransferrer> toTransferrers, HashSet<FluidConnector> fromConnectors, HashSet<FluidConnector> toConnectors, float rate, Type transferringType, SubsystemFluidTransfer subsystemFluidTransfer) : PipeFluidConnection(fromTransferrers, toTransferrers, fromConnectors, toConnectors, rate, transferringType, subsystemFluidTransfer), IDrawable
{
	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public readonly List<Point3> m_pipePoints = new List<Point3>();

	public int[] DrawOrders => new int[1] { 1000 };

	public override void Init()
	{
		foreach (KeyValuePair<Point3, PipeFluidConnection> pipeConnection in SubsystemFluidTransfer.m_pipeConnections)
		{
			if (pipeConnection.Value == this)
			{
				m_pipePoints.Add(pipeConnection.Key);
			}
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (IsTransferring)
		{
			int? transferringFluid = TransferringFluid;
			if (transferringFluid.HasValue && transferringFluid.GetValueOrDefault() > 0 && m_pipePoints.Count > 0)
			{
				Fluid fluid = FluidsManager.GetFluid(TransferringFluid.Value);
				int direction_Static = FourDirectionModelDevice.GetDirection_Static(SubsystemFluidTransfer.m_subsystemTerrain.Terrain.GetCellValueFast(m_pipePoints[0].X, m_pipePoints[0].Y, m_pipePoints[0].Z));
				foreach (Point3 pipePoint in m_pipePoints)
				{
					Vector3 p = new Vector3(pipePoint) + Vector3.UnitY * 0.575f;
					Vector3 p2 = new Vector3(pipePoint) + Vector3.UnitY * 0.575f + Vector3.UnitX;
					Vector3 p3 = new Vector3(pipePoint) + Vector3.UnitY * 0.575f + Vector3.UnitX + Vector3.UnitZ;
					Vector3 p4 = new Vector3(pipePoint) + Vector3.UnitY * 0.575f + Vector3.UnitZ;
					float num = 1f / (float)fluid.GetTextureSubdivision(TransferringFluid.Value);
					Vector2 zero = Vector2.Zero;
					Vector2 texCoord = Vector2.UnitX * num;
					Vector2 texCoord2 = Vector2.One * num;
					Vector2 texCoord3 = Vector2.UnitY * num;
					if ((direction_Static == 1 || direction_Static == 3) ? true : false)
					{
						p += Vector3.UnitZ * 0.25f;
						p2 += Vector3.UnitZ * 0.25f;
						p3 -= Vector3.UnitZ * 0.25f;
						p4 -= Vector3.UnitZ * 0.25f;
						zero += Vector2.UnitY * 0.25f * num;
						texCoord += Vector2.UnitY * 0.25f * num;
						texCoord2 -= Vector2.UnitY * 0.25f * num;
						texCoord3 -= Vector2.UnitY * 0.25f * num;
					}
					else if ((direction_Static == 0 || direction_Static == 2) ? true : false)
					{
						p += Vector3.UnitX * 0.25f;
						p2 -= Vector3.UnitX * 0.25f;
						p3 -= Vector3.UnitX * 0.25f;
						p4 += Vector3.UnitX * 0.25f;
						zero += Vector2.UnitX * 0.25f * num;
						texCoord -= Vector2.UnitX * 0.25f * num;
						texCoord2 -= Vector2.UnitX * 0.25f * num;
						texCoord3 += Vector2.UnitX * 0.25f * num;
					}
					m_primitivesRenderer3D.TexturedBatch(fluid.GetTexture(TransferringFluid.Value), useAlphaTest: false, 0, null, null, null, SamplerState.PointWrap).QueueQuad(p, p2, p3, p4, zero, texCoord, texCoord2, texCoord3, fluid.GetColor(TransferringFluid.Value));
				}
			}
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}
}
