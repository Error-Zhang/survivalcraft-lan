using System;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW;

public class AirFlowParticleSystem : ParticleSystem<AirFlowParticleSystem.Particle>
{
	public class Particle : Game.Particle
	{
		public float ElapsedTime;

		public float Lifetime;

		public float Speed;

		public float Phase;

		public float PhaseSpeed;

		public Color BaseColor;

		public int LightLevel;

		public float Angle;

		public float AngleSpeed;

		public new Vector3 Right;

		public new Vector3 Up;
	}

	private SubsystemTerrain m_subsystemTerrain;

	private Game.Random m_random = new Game.Random();

	private Point3 m_originPoint;

	private bool m_isFadeIn;

	public AirFlowParticleSystem(SubsystemTerrain subsystemTerrain, Point3 originPoint, int particleCount, bool isFadeIn, float areaRange = 0.4f)
		: base(particleCount)
	{
		m_subsystemTerrain = subsystemTerrain;
		m_originPoint = originPoint;
		m_isFadeIn = isFadeIn;
		Texture = ContentManager.Get<Texture2D>("Textures/GunSmokeParticle");
		TextureSlotsCount = 3;
		Color white = Color.White;
		for (int i = 0; i < Particles.Length; i++)
		{
			float f = m_random.Float();
			Color white2 = Color.White;
			Particle particle = Particles[i];
			particle.IsActive = true;
			particle.Lifetime = 3f;
			particle.Position = new Vector3(originPoint) + new Vector3(0.5f) + areaRange * new Vector3(m_random.Float(-1f, 1f), MathUtils.Lerp(1f, -1f, f), m_random.Float(-1f, 1f));
			particle.LightLevel = 15;
			particle.BaseColor = white * white2;
			particle.Color = Color.Transparent;
			particle.BillboardingMode = ParticleBillboardingMode.Camera;
			particle.Size = new Vector2(0.09f) * m_random.Float(0.5f, 1f);
			particle.Speed = MathUtils.Lerp(5f, 8f, f);
			particle.Phase = m_random.Float(0f, (float)Math.PI * 2f);
			particle.PhaseSpeed = 2f * particle.Speed * m_random.Float(0.75f, 1.25f);
			particle.Angle = m_random.Float(0f, (float)Math.PI * 2f);
			particle.AngleSpeed = (float)m_random.Sign() * m_random.Float(1f, 3f);
			particle.FlipX = m_random.Bool();
			particle.FlipY = m_random.Bool();
		}
	}

	public override bool Simulate(float deltaTime)
	{
		Terrain terrain = m_subsystemTerrain.Terrain;
		bool flag = false;
		for (int i = 0; i < Particles.Length; i++)
		{
			Particle particle = Particles[i];
			if (particle.IsActive)
			{
				if (particle.BillboardingMode == ParticleBillboardingMode.Camera)
				{
					particle.Phase += particle.PhaseSpeed * deltaTime;
					particle.Rotation = -0.5f * MathF.Sin(particle.Phase) + (float)Math.PI / 2f;
					particle.Angle += particle.AngleSpeed * deltaTime;
					Vector2 v = Vector2.UnitX.Rotate(particle.Rotation);
					Vector2 vector = v.Perpendicular();
					Vector2 vector2 = Vector2.UnitY.Rotate(particle.Angle);
					particle.Right = particle.Size.X * new Vector3(v.X * vector2.X, v.Y, v.X * vector2.Y);
					particle.Up = particle.Size.Y * new Vector3(vector.X * vector2.X, vector.Y, vector.X * vector2.Y);
					float num = MathUtils.Saturate(4f * particle.ElapsedTime);
					particle.Position += 0.8f * particle.Speed * num * deltaTime * new Vector3(0f, 1f, 0f);
					particle.Position += 0.1f * particle.Speed * num * deltaTime * MathF.Cos(particle.Phase) * new Vector3(vector2.X, 0f, vector2.Y);
					particle.Position += 0.1f * particle.Speed * num * deltaTime * MathF.Cos(2f * particle.Phase) * new Vector3(0f, -1f, 0f);
				}
				int x = Terrain.ToCell(particle.Position.X);
				int y = Terrain.ToCell(particle.Position.Y);
				int z = Terrain.ToCell(particle.Position.Z);
				TerrainChunk chunkAtCell = terrain.GetChunkAtCell(x, z);
				if (chunkAtCell != null && chunkAtCell.State >= TerrainChunkState.InvalidVertices1)
				{
					particle.LightLevel = terrain.GetCellLight(x, y, z);
				}
				particle.Color = Color.MultiplyColorOnly(particle.BaseColor, LightingManager.LightIntensityByLightValue[particle.LightLevel]);
				float num2 = MathUtils.Saturate(0.5f * (particle.Lifetime - particle.ElapsedTime));
				if (m_isFadeIn)
				{
					num2 *= MathUtils.Saturate(1f * particle.ElapsedTime);
				}
				particle.Color *= num2;
				particle.ElapsedTime += deltaTime;
				if (particle.ElapsedTime >= particle.Lifetime)
				{
					particle.IsActive = false;
				}
				else
				{
					flag = true;
				}
			}
		}
		return !flag;
	}
}
