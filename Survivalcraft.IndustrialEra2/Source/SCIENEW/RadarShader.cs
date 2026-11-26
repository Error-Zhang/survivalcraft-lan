using System.Globalization;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class RadarShader : Shader
{
	public ShaderParameter m_worldViewProjectionMatrixParameter;

	public ShaderParameter m_renderSize;

	public ShaderParameter m_time;

	public ShaderParameter m_pointsPos;

	public readonly ShaderTransforms Transforms;

	public const int MaxPointsCount = 8;

	public Vector2[] m_pointsPosition = new Vector2[8];

	public Vector2 RenderSize
	{
		set
		{
			m_renderSize.SetValue(value);
		}
	}

	public float Time
	{
		set
		{
			m_time.SetValue(value);
		}
	}

	public RadarShader()
		: base(ShaderCodeManager.GetFast("Shaders/Radar.vsh"), ShaderCodeManager.GetFast("Shaders/Radar.psh"), new ShaderMacro("MAX_POINTS_COUNT", 8.ToString(CultureInfo.InvariantCulture)))
	{
		m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", allowNull: true);
		m_renderSize = GetParameter("u_renderSize", allowNull: true);
		m_time = GetParameter("u_time", allowNull: true);
		m_pointsPos = GetParameter("u_pointsPos", allowNull: true);
		Transforms = new ShaderTransforms(1);
	}

	public override void PrepareForDrawingOverride()
	{
		Transforms.UpdateMatrices(1, worldView: false, viewProjection: false, worldViewProjection: true);
		m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
		m_pointsPos.SetValue(m_pointsPosition, 8);
	}
}
