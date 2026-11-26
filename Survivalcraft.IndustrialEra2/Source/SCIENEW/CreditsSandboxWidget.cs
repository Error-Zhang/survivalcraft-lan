using System;
using System.Collections.Generic;
using Box2D.NET;
using Engine;
using Engine.Graphics;
using Engine.Media;
using Game;

namespace SCIENEW;

public class CreditsSandboxWidget : CanvasWidget
{
	private struct CreditBody
	{
		public B2BodyId BodyId;

		public string Name;

		public string Role;

		public Vector2 HalfSizePx;
	}

	private sealed class OverlapPick
	{
		public B2Vec2 Point;

		public B2BodyId BodyId = B2Ids.b2_nullBodyId;
	}

	private const float PixelsPerMeter = 48f;

	private const float SpawnInterval = 0.45f;

	private const float FixedDt = 1f / 60f;

	private const float FontScale = 0.85f;

	private const float CullMarginMeters = 2f;

	private readonly List<CreditsEntry> m_entries = new List<CreditsEntry>();

	private readonly List<CreditBody> m_bodies = new List<CreditBody>();

	private readonly Engine.Random m_random = new Engine.Random();

	private B2WorldId m_worldId = B2Ids.b2_nullWorldId;

	private B2BodyId m_groundId = B2Ids.b2_nullBodyId;

	private B2BodyId m_leftWallId = B2Ids.b2_nullBodyId;

	private B2BodyId m_rightWallId = B2Ids.b2_nullBodyId;

	private B2BodyId m_mouseBodyId = B2Ids.b2_nullBodyId;

	private B2JointId m_mouseJointId = B2Ids.b2_nullJointId;

	private B2BodyId m_grabbedBodyId = B2Ids.b2_nullBodyId;

	private B2Vec2 m_mouseWorld;

	private float m_accumulator;

	private float m_spawnTimer;

	private int m_spawnIndex;

	private Vector2 m_lastSize;

	private bool m_loaded;

	public CreditsSandboxWidget()
	{
		IsHitTestVisible = true;
		ClampToBounds = true;
	}

	public void ResetSimulation(IReadOnlyList<CreditsEntry> entries)
	{
		DestroyWorld();
		m_entries.Clear();
		if (entries != null)
		{
			m_entries.AddRange(entries);
		}
		m_bodies.Clear();
		m_spawnIndex = 0;
		m_spawnTimer = 0.15f;
		m_accumulator = 0f;
		m_loaded = true;
		m_lastSize = Vector2.Zero;
	}

	public override void Update()
	{
		if (m_loaded && !(ActualSize.X < 32f) && !(ActualSize.Y < 32f))
		{
			if (B2Ids.B2_IS_NULL(m_worldId) || m_lastSize != ActualSize)
			{
				RebuildWorld();
			}
			HandlePointer();
			float num = Math.Clamp(Time.FrameDuration, 0f, 0.05f);
			m_accumulator += num;
			while (m_accumulator >= 1f / 60f)
			{
				StepPhysics(1f / 60f);
				CullOutOfBoundsBodies();
				m_accumulator -= 1f / 60f;
			}
			m_spawnTimer -= num;
			while (m_spawnTimer <= 0f && m_spawnIndex < m_entries.Count)
			{
				SpawnNext();
				m_spawnTimer += 0.45f;
			}
		}
	}

	public override void Draw(DrawContext dc)
	{
		if (B2Ids.B2_IS_NULL(m_worldId))
		{
			return;
		}
		FlatBatch2D flatBatch2D = dc.PrimitivesRenderer2D.FlatBatch(0, DepthStencilState.None);
		int count = flatBatch2D.TriangleVertices.Count;
		flatBatch2D.QueueQuad(Vector2.Zero, ActualSize, 0f, new Color(12, 14, 18, 96));
		flatBatch2D.TransformTriangles(GlobalTransform, count);
		BitmapFont bitmapFont = LabelWidget.BitmapFont;
		FontBatch2D fontBatch2D = dc.PrimitivesRenderer2D.FontBatch(bitmapFont, 1, DepthStencilState.None, null, null, SamplerState.LinearClamp);
		Vector2 scale = new Vector2(0.85f);
		foreach (CreditBody body in m_bodies)
		{
			if (!B2Ids.B2_IS_NULL(body.BodyId) && B2Worlds.b2Body_IsValid(body.BodyId))
			{
				B2Transform b2Transform = B2Bodies.b2Body_GetTransform(body.BodyId);
				Vector2 position = WorldToWidget(b2Transform.p);
				float angle = 0f - B2MathFunction.b2Rot_GetAngle(in b2Transform.q);
				Color color = CreditsColors.ForRole(body.Role, (float)Time.RealTime);
				int count2 = fontBatch2D.TriangleVertices.Count;
				fontBatch2D.QueueText(body.Name, position, 0f, color, TextAnchor.Center, scale, Vector2.Zero, angle);
				fontBatch2D.TransformTriangles(GlobalTransform, count2);
			}
		}
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = true;
		DesiredSize = parentAvailableSize;
	}

	private void RebuildWorld()
	{
		DestroyWorld();
		m_lastSize = ActualSize;
		B2WorldDef def = B2Types.b2DefaultWorldDef();
		def.gravity = new B2Vec2(0f, -14f);
		m_worldId = B2Worlds.b2CreateWorld(in def);
		float num = ActualSize.X / 48f;
		float num2 = ActualSize.Y / 48f;
		m_groundId = CreateStaticBox(num * 0.5f, 0.125f, num * 0.5f, 0.25f);
		m_leftWallId = CreateStaticBox(0.125f, num2 * 0.5f, 0.25f, num2);
		m_rightWallId = CreateStaticBox(num - 0.125f, num2 * 0.5f, 0.25f, num2);
		m_bodies.Clear();
		m_spawnIndex = 0;
		m_spawnTimer = 0.15f;
	}

	private B2BodyId CreateStaticBox(float cx, float cy, float hx, float hy)
	{
		B2BodyDef def = B2Types.b2DefaultBodyDef();
		def.type = B2BodyType.b2_staticBody;
		def.position = new B2Vec2(cx, cy);
		B2BodyId b2BodyId = B2Bodies.b2CreateBody(m_worldId, in def);
		B2Polygon polygon = B2Geometries.b2MakeBox(hx, hy);
		B2Shapes.b2CreatePolygonShape(b2BodyId, B2Types.b2DefaultShapeDef(), in polygon);
		return b2BodyId;
	}

	private void SpawnNext()
	{
		if (m_spawnIndex < m_entries.Count && !B2Ids.B2_IS_NULL(m_worldId))
		{
			CreditsEntry entry = m_entries[m_spawnIndex++];
			float num = ActualSize.X / 48f;
			float num2 = ActualSize.Y / 48f;
			Vector2 halfPx = MeasureHalfPx(LabelWidget.BitmapFont.MeasureText(entry.Name, new Vector2(0.85f), Vector2.Zero));
			float num3 = halfPx.X / 48f;
			float num4 = halfPx.Y / 48f;
			float num5 = num3 + 0.2f;
			float x = num5 + m_random.Float(0f, 1f) * Math.Max(0.01f, num - 2f * num5);
			float y = num2 - num4 - 0.15f;
			SpawnEntryAt(entry, new B2Vec2(x, y), halfPx);
		}
	}

	private void SpawnRandomAt(B2Vec2 worldPos)
	{
		if (m_entries.Count != 0 && !B2Ids.B2_IS_NULL(m_worldId))
		{
			CreditsEntry entry = m_entries[m_random.Int(m_entries.Count)];
			Vector2 measure = LabelWidget.BitmapFont.MeasureText(entry.Name, new Vector2(0.85f), Vector2.Zero);
			SpawnEntryAt(entry, worldPos, MeasureHalfPx(measure));
		}
	}

	private static Vector2 MeasureHalfPx(Vector2 measure)
	{
		return new Vector2(Math.Max(measure.X * 0.5f + 8f, 20f), Math.Max(measure.Y * 0.5f + 6f, 12f));
	}

	private void SpawnEntryAt(CreditsEntry entry, B2Vec2 worldPos, Vector2 halfPx)
	{
		if (!B2Ids.B2_IS_NULL(m_worldId))
		{
			float halfWidth = halfPx.X / 48f;
			float halfHeight = halfPx.Y / 48f;
			B2BodyDef def = B2Types.b2DefaultBodyDef();
			def.type = B2BodyType.b2_dynamicBody;
			def.position = worldPos;
			def.rotation = B2MathFunction.b2MakeRot(m_random.Float(-0.25f, 0.25f));
			B2BodyId bodyId = B2Bodies.b2CreateBody(m_worldId, in def);
			B2Polygon polygon = B2Geometries.b2MakeBox(halfWidth, halfHeight);
			B2ShapeDef def2 = B2Types.b2DefaultShapeDef();
			def2.density = 1f;
			def2.material.friction = 0.45f;
			def2.material.restitution = 0.08f;
			B2Shapes.b2CreatePolygonShape(bodyId, in def2, in polygon);
			m_bodies.Add(new CreditBody
			{
				BodyId = bodyId,
				Name = entry.Name,
				Role = entry.Role,
				HalfSizePx = halfPx
			});
		}
	}

	private void CullOutOfBoundsBodies()
	{
		if (m_bodies.Count == 0 || B2Ids.B2_IS_NULL(m_worldId))
		{
			return;
		}
		float num = ActualSize.X / 48f;
		float num2 = ActualSize.Y / 48f;
		float num3 = -2f;
		float num4 = num + 2f;
		float num5 = -2f;
		float num6 = num2 + 2f;
		for (int num7 = m_bodies.Count - 1; num7 >= 0; num7--)
		{
			CreditBody creditBody = m_bodies[num7];
			if (B2Ids.B2_IS_NULL(creditBody.BodyId) || !B2Worlds.b2Body_IsValid(creditBody.BodyId))
			{
				m_bodies.RemoveAt(num7);
			}
			else
			{
				B2Vec2 p = B2Bodies.b2Body_GetTransform(creditBody.BodyId).p;
				if (!(p.X >= num3) || !(p.X <= num4) || !(p.Y >= num5) || !(p.Y <= num6))
				{
					if (B2Ids.B2_IS_NON_NULL(m_grabbedBodyId) && B2Ids.B2_ID_EQUALS(m_grabbedBodyId, creditBody.BodyId))
					{
						ReleaseMouseJoint();
					}
					B2Bodies.b2DestroyBody(creditBody.BodyId);
					m_bodies.RemoveAt(num7);
				}
			}
		}
	}

	private void StepPhysics(float dt)
	{
		if (B2Ids.B2_IS_NON_NULL(m_mouseBodyId) && B2Ids.B2_IS_NON_NULL(m_mouseJointId))
		{
			B2Bodies.b2Body_SetTargetTransform(m_mouseBodyId, new B2Transform(m_mouseWorld, B2MathFunction.b2Rot_identity), dt, wake: true);
		}
		B2Worlds.b2World_Step(m_worldId, dt, 4);
	}

	private void HandlePointer()
	{
		Vector2? vector = Input.Press ?? Input.Tap;
		bool flag = vector.HasValue && HitTestGlobal(vector.Value) == this;
		if (!flag && !Input.Press.HasValue)
		{
			ReleaseMouseJoint();
			return;
		}
		if (!vector.HasValue)
		{
			ReleaseMouseJoint();
			return;
		}
		Vector2 local = ScreenToWidget(vector.Value);
		m_mouseWorld = WidgetToWorld(local);
		if (Input.Press.HasValue & flag)
		{
			if (B2Ids.B2_IS_NULL(m_mouseJointId) && Input.Tap.HasValue && !TryGrab(m_mouseWorld))
			{
				SpawnRandomAt(m_mouseWorld);
			}
		}
		else
		{
			ReleaseMouseJoint();
		}
	}

	private bool TryGrab(B2Vec2 p)
	{
		B2Vec2 b = new B2Vec2(0.05f, 0.05f);
		B2AABB aabb = default;
		aabb.lowerBound = B2MathFunction.b2Sub(in p, in b);
		aabb.upperBound = B2MathFunction.b2Add(in p, in b);
		OverlapPick overlapPick = new OverlapPick
		{
			Point = p
		};
		B2Worlds.b2World_OverlapAABB(m_worldId, in aabb, B2Types.b2DefaultQueryFilter(), OverlapCallback, overlapPick);
		if (B2Ids.B2_IS_NULL(overlapPick.BodyId))
		{
			return false;
		}
		B2BodyDef def = B2Types.b2DefaultBodyDef();
		def.type = B2BodyType.b2_kinematicBody;
		def.position = p;
		def.enableSleep = false;
		m_mouseBodyId = B2Bodies.b2CreateBody(m_worldId, in def);
		B2MotorJointDef def2 = B2Joints.b2DefaultMotorJointDef();
		def2.@base.bodyIdA = m_mouseBodyId;
		def2.@base.bodyIdB = overlapPick.BodyId;
		def2.@base.localFrameB.p = B2Bodies.b2Body_GetLocalPoint(overlapPick.BodyId, p);
		def2.linearHertz = 7.5f;
		def2.linearDampingRatio = 1f;
		B2MassData b2MassData = B2Bodies.b2Body_GetMassData(overlapPick.BodyId);
		float val = B2MathFunction.b2Length(B2Worlds.b2World_GetGravity(m_worldId));
		float num = b2MassData.mass * Math.Max(val, 1f);
		def2.maxSpringForce = 120f * num;
		if (b2MassData.mass > 0f)
		{
			float num2 = MathF.Sqrt(b2MassData.rotationalInertia / b2MassData.mass);
			def2.maxVelocityTorque = 0.25f * num2 * num;
		}
		m_mouseJointId = B2Joints.b2CreateMotorJoint(m_worldId, in def2);
		m_grabbedBodyId = overlapPick.BodyId;
		return true;
	}

	private static bool OverlapCallback(B2ShapeId shapeId, object context)
	{
		if (!(context is OverlapPick overlapPick))
		{
			return true;
		}
		B2BodyId bodyId = B2Shapes.b2Shape_GetBody(shapeId);
		if (B2Bodies.b2Body_GetType(bodyId) != B2BodyType.b2_dynamicBody)
		{
			return true;
		}
		if (!B2Shapes.b2Shape_TestPoint(shapeId, overlapPick.Point))
		{
			return true;
		}
		overlapPick.BodyId = bodyId;
		return false;
	}

	private void ReleaseMouseJoint()
	{
		if (B2Ids.B2_IS_NON_NULL(m_mouseJointId))
		{
			B2Joints.b2DestroyJoint(m_mouseJointId, wakeAttached: true);
			m_mouseJointId = B2Ids.b2_nullJointId;
		}
		if (B2Ids.B2_IS_NON_NULL(m_mouseBodyId))
		{
			B2Bodies.b2DestroyBody(m_mouseBodyId);
			m_mouseBodyId = B2Ids.b2_nullBodyId;
		}
		m_grabbedBodyId = B2Ids.b2_nullBodyId;
	}

	private void DestroyWorld()
	{
		ReleaseMouseJoint();
		if (B2Ids.B2_IS_NON_NULL(m_worldId))
		{
			B2Worlds.b2DestroyWorld(m_worldId);
			m_worldId = B2Ids.b2_nullWorldId;
		}
		m_groundId = B2Ids.b2_nullBodyId;
		m_leftWallId = B2Ids.b2_nullBodyId;
		m_rightWallId = B2Ids.b2_nullBodyId;
		m_bodies.Clear();
	}

	private B2Vec2 WidgetToWorld(Vector2 local)
	{
		return new B2Vec2(local.X / 48f, (ActualSize.Y - local.Y) / 48f);
	}

	private Vector2 WorldToWidget(B2Vec2 world)
	{
		return new Vector2(world.X * 48f, ActualSize.Y - world.Y * 48f);
	}
}
