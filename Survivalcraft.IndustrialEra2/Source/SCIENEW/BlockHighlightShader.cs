using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class BlockHighlightShader : Shader
{
	public ShaderParameter m_origin;

	public ShaderParameter m_viewProjectionMatrix;

	public ShaderParameter m_color1;

	public ShaderParameter m_color2;

	public ShaderParameter m_offset;

	public ShaderParameter m_scale;

	public BlockHighlightShader()
		: base(ShaderCodeManager.GetFast("Shaders/BlockHighlight.vsh"), ShaderCodeManager.GetFast("Shaders/BlockHighlight.psh"))
	{
		m_origin = GetParameter("u_origin");
		m_viewProjectionMatrix = GetParameter("u_viewProjectionMatrix");
		m_color1 = GetParameter("u_color1");
		m_color2 = GetParameter("u_color2");
		m_offset = GetParameter("u_offset");
		m_scale = GetParameter("u_scale");
	}

	public void SetOrigin(Vector2 origin)
	{
		m_origin.SetValue(origin);
	}

	public void SetViewProjectionMatrix(Matrix viewProjectionMatrix)
	{
		m_viewProjectionMatrix.SetValue(viewProjectionMatrix);
	}

	public void SetColor1(Color color)
	{
		m_color1.SetValue(new Vector4(color));
	}

	public void SetColor2(Color color)
	{
		m_color2.SetValue(new Vector4(color));
	}

	public void SetOffset(float offset)
	{
		m_offset.SetValue(offset);
	}

	public void SetScale(float scale)
	{
		m_scale.SetValue(scale);
	}
}
