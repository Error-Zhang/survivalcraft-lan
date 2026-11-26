using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class RadiationFireParticleSystem : ParticleSystem<RadiationFireParticleSystem.Particle>
{
	public class Particle : Game.Particle
	{
		public float Time;

		public float TimeToLive;

		public float Speed;
	}

	public Game.Random m_random = new Game.Random();

	public Vector3 m_position;

	public float m_size;

	public float m_toGenerate;

	public bool m_visible;

	public float m_maxVisibilityDistance;

	public float m_age;

	public bool m_isStopped;

	public bool IsStopped
	{
		get
		{
			return m_isStopped;
		}
		set
		{
			m_isStopped = value;
		}
	}

	public RadiationFireParticleSystem(Vector3 position, float size, float maxVisibilityDistance)
		: base(10)
	{
		m_position = position;
		m_size = size;
		m_maxVisibilityDistance = maxVisibilityDistance;
		Texture = ContentManager.Get<Texture2D>("Textures/FireParticle2");
		TextureSlotsCount = 3;
	}

	public override bool Simulate(float dt)
	{
		m_age += dt;
		bool flag = false;
		_ = m_visible;
		m_toGenerate += (IsStopped ? 0f : (5f * dt));
		for (int i = 0; i < Particles.Length; i++)
		{
			Particle particle = Particles[i];
			if (particle.IsActive)
			{
				flag = true;
				particle.Time += dt;
				particle.TimeToLive -= dt;
				if (particle.TimeToLive > 0f)
				{
					particle.Position.Y += particle.Speed * dt;
					particle.TextureSlot = (int)MathUtils.Min(9f * particle.Time / 1.25f, 8f);
				}
				else
				{
					particle.IsActive = false;
				}
			}
			else if (m_toGenerate >= 1f)
			{
				particle.IsActive = true;
				particle.Position = m_position + 0.25f * m_size * new Vector3(m_random.Float(-1f, 1f), 0f, m_random.Float(-1f, 1f));
				particle.Color = Color.White;
				particle.Size = new Vector2(m_size);
				particle.Speed = m_random.Float(0.45f, 0.55f) * m_size / 0.15f;
				particle.Time = 0f;
				particle.TimeToLive = m_random.Float(0.5f, 2f);
				particle.FlipX = m_random.Int(0, 1) == 0;
				particle.FlipY = m_random.Int(0, 1) == 0;
				m_toGenerate--;
			}
		}
		m_toGenerate = MathUtils.Remainder(m_toGenerate, 1f);
		m_visible = false;
		if (IsStopped)
		{
			return !flag;
		}
		return false;
	}
}
