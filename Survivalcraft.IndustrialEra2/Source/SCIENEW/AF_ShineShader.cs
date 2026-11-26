using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class AF_ShineShader : Shader
{
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

	public AF_ShineShader()
		: base(ShaderCodeManager.GetFast("Shaders/AF_Shine.vsh"), ShaderCodeManager.GetFast("Shaders/AF_Shine.psh"))
	{
		m_renderSize = GetParameter("u_renderSize", allowNull: true);
		m_time = GetParameter("u_time", allowNull: true);
		Transforms = new ShaderTransforms(1);
	}

	public override void PrepareForDrawingOverride()
	{
		Transforms.UpdateMatrices(1, worldView: false, viewProjection: false, worldViewProjection: true);
	}
}
