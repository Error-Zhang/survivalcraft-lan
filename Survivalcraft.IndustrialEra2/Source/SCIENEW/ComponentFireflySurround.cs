using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentFireflySurround : Component, IDrawable
{
	public class Firefly(Vector3 vector3, double time, float hue0, float saturation0)
	{
		public Vector3 position = vector3;

		public Vector3 nextPosition = vector3;

		public double timeToStopMoving;

		public double spawnTime = time;

		public float hue = hue0;

		public float saturation = saturation0;
	}

	protected ComponentCreature m_componentCreature;

	public static SubsystemTime m_subsystemTime;

	public static SubsystemSky m_subsystemSky;

	public static SubsystemWeather m_subsystemWeather;

	public static SubsystemTerrain m_subsystemTerrain;

	public Game.Random m_random = new Game.Random();

	public static PrimitivesRenderer3D m_primitivesRenderer;

	public static Texture2D m_texture;

	public DynamicArray<Firefly> m_fireFlies = new DynamicArray<Firefly>();

	public double m_lastSpawnTime;

	public bool isLivingMode;

	public int[] DrawOrders => new int[1] { 10 };

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		m_componentCreature = Entity.FindComponent<ComponentCreature>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemSky = Project.FindSubsystem<SubsystemSky>(throwOnError: true);
		m_subsystemWeather = Project.FindSubsystem<SubsystemWeather>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_texture = ContentManager.Get<Texture2D>("Textures/Sun");
		m_primitivesRenderer = Project.FindSubsystem<SubsystemModelsRenderer>(throwOnError: true).PrimitivesRenderer;
		isLivingMode = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true).WorldSettings.EnvironmentBehaviorMode == EnvironmentBehaviorMode.Living;
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (!isLivingMode || drawOrder != 10)
		{
			return;
		}
		double gameTime = m_subsystemTime.GameTime;
		float gameTimeDelta = m_subsystemTime.GameTimeDelta;
		Vector3 position = m_componentCreature.ComponentBody.Position;
		Point2 point = new Point2((int)position.X, (int)position.Z);
		if ((double)m_subsystemSky.SkyLightIntensity < 0.4 && m_subsystemWeather.PrecipitationIntensity == 0f && !SubsystemWeather.IsPlaceFrozen(m_subsystemTerrain.Terrain.GetTemperature(point.X, point.Y), (int)position.Y))
		{
			if (gameTime > m_lastSpawnTime + 1.0)
			{
				m_lastSpawnTime = gameTime;
				Vector3 vector = new Vector3(position.X + m_random.Float(-20f, 20f), 0f, position.Z + m_random.Float(-20f, 20f));
				Point2 point2 = new Point2((int)vector.X, (int)vector.Z);
				int humidity = m_subsystemTerrain.Terrain.GetHumidity(point2.X, point2.Y);
				vector.Y = (float)m_subsystemTerrain.Terrain.GetTopHeight(point2.X, point2.Y) + m_random.Float(0.4f, 4f);
				float num = m_random.Float(0f, 1f);
				if (m_subsystemTerrain.Terrain.GetCellLight(point2.X, (int)vector.Y, point2.Y) < 9 && ((humidity > 12 && num < 0.8f) || (humidity > 10 && num < 0.6f) || (humidity > 8 && num < 0.45f) || (humidity > 6 && num < 0.3f)))
				{
					m_fireFlies.Add(new Firefly(vector, gameTime, m_random.Float(60f, 150f), m_random.Float(0.5f, 1f)));
				}
				humidity = m_subsystemTerrain.Terrain.GetHumidity(point.X, point.Y);
				if (m_fireFlies.Count > 120 || (humidity < 6 && m_fireFlies.Count > 40))
				{
					m_fireFlies.RemoveAt(0);
				}
			}
		}
		else if (gameTime > m_lastSpawnTime + 0.1 && m_fireFlies.Count > 0)
		{
			m_fireFlies.RemoveAt(0);
		}
		foreach (Firefly fireFly in m_fireFlies)
		{
			float num2 = fireFly.position.Distance(camera.ViewPosition);
			if (num2 < 32f)
			{
				if (gameTime >= fireFly.timeToStopMoving)
				{
					fireFly.nextPosition += m_random.Vector3(0.3f);
					fireFly.timeToStopMoving += fireFly.position.Distance(fireFly.nextPosition) * m_random.Float(2f, 10f);
				}
				else
				{
					Vector3 vector2 = (fireFly.nextPosition - fireFly.position) / (float)(fireFly.timeToStopMoving - gameTime) * gameTimeDelta;
					fireFly.position += vector2;
				}
				double num3 = (gameTime - fireFly.spawnTime) * 0.5;
				float num4 = (float)(Math.Sin(7.0 * num3) / (7.0 * Math.Sin(num3))) * 0.01f + 0.012f;
				Vector3 vector3 = camera.ViewDirection.Cross(Vector3.UnitY).Normalize();
				Vector3 vector4 = -camera.ViewDirection.Cross(vector3).Normalize();
				Vector3 p = (fireFly.position + num4 * (-vector3 - vector4)).Transform(camera.ViewMatrix);
				Vector3 p2 = (fireFly.position + num4 * (vector3 - vector4)).Transform(camera.ViewMatrix);
				Vector3 p3 = (fireFly.position + num4 * (-vector3 + vector4)).Transform(camera.ViewMatrix);
				Vector3 p4 = (fireFly.position + num4 * (vector3 + vector4)).Transform(camera.ViewMatrix);
				m_primitivesRenderer.TexturedBatch(m_texture, useAlphaTest: true, 0, null, RasterizerState.CullCounterClockwiseScissor, null, SamplerState.AnisotropicWrap).QueueQuad(color: new Color(Color.HsvToRgb(new Vector3(fireFly.hue, fireFly.saturation, Math.Clamp(0.85f + num4 * 10f - num2 * 0.01f, 0f, 1f)))), p1: p, p2: p3, p3: p4, p4: p2, texCoord1: new Vector2(0f, 1f), texCoord2: new Vector2(0f, 0f), texCoord3: new Vector2(1f, 0f), texCoord4: new Vector2(1f, 1f));
			}
		}
	}
}
