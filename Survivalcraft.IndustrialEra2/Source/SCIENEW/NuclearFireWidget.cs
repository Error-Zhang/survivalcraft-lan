using System;
using Engine;
using Game;

namespace SCIENEW;

public class NuclearFireWidget : CanvasWidget
{
	public ScreenSpaceColoredSteamRenderer m_renderer = new ScreenSpaceColoredSteamRenderer(100, Color.Green, new Color[1] { Color.Green });

	public float ParticlesPerSecond
	{
		get
		{
			return m_renderer.ParticlesPerSecond;
		}
		set
		{
			m_renderer.ParticlesPerSecond = value;
		}
	}

	public NuclearFireWidget()
	{
		ClampToBounds = true;
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = true;
		base.MeasureOverride(parentAvailableSize);
	}

	public override void Draw(DrawContext dc)
	{
		m_renderer.Draw(dc.PrimitivesRenderer2D, 0f, GlobalTransform, GlobalColorTransform);
	}

	public override void Update()
	{
		float dt = Math.Clamp(Time.FrameDuration, 0f, 0.1f);
		m_renderer.Origin = new Vector2(0f, ActualSize.Y);
		m_renderer.CutoffPosition = float.NegativeInfinity;
		m_renderer.ParticleSize = 32f;
		m_renderer.ParticleSpeed = 32f;
		m_renderer.Width = ActualSize.X;
		m_renderer.MinTimeToLive = 1.5f;
		m_renderer.MaxTimeToLive = 3f;
		m_renderer.ParticleAnimationPeriod = 1.25f;
		m_renderer.Update(dt);
	}
}
