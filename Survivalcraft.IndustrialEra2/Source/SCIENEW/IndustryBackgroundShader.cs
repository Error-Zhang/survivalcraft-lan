using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class IndustryBackgroundShader : Shader
{
	public ShaderParameter m_worldViewProjectionMatrixParameter;

	public ShaderParameter m_renderSize;

	public ShaderParameter m_time;

	public readonly ShaderTransforms Transforms;

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

	public IndustryBackgroundShader()
		: base(ShaderCodeManager.GetFast("Shaders/Industry.vsh"), ShaderCodeManager.GetFast("Shaders/Industry.psh"))
	{
		m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", allowNull: true);
		m_renderSize = GetParameter("u_renderSize", allowNull: true);
		m_time = GetParameter("u_time", allowNull: true);
		Transforms = new ShaderTransforms(1);
	}

	public override void PrepareForDrawingOverride()
	{
		Transforms.UpdateMatrices(1, worldView: false, viewProjection: false, worldViewProjection: true);
		m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
	}
}
