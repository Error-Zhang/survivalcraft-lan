using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class CherenkovRadiationShader : Shader
{
	public ShaderParameter m_origin;

	public ShaderParameter m_viewProjectionMatrix;

	public ShaderParameter m_colorCore;

	public ShaderParameter m_colorCenter;

	public ShaderParameter m_colorOuter;

	public ShaderParameter m_center;

	public ShaderParameter m_unitWidth;

	public CherenkovRadiationShader()
		: base(ShaderCodeManager.GetFast("Shaders/CherenkovRadiation.vsh"), ShaderCodeManager.GetFast("Shaders/CherenkovRadiation.psh"))
	{
		m_origin = GetParameter("u_origin");
		m_viewProjectionMatrix = GetParameter("u_viewProjectionMatrix");
		m_colorCore = GetParameter("u_colorCore");
		m_colorCenter = GetParameter("u_colorCenter");
		m_colorOuter = GetParameter("u_colorOuter");
		m_center = GetParameter("u_center");
		m_unitWidth = GetParameter("u_unitWidth");
	}

	public void SetOrigin(Vector2 origin)
	{
		m_origin.SetValue(origin);
	}

	public void SetViewProjectionMatrix(Matrix viewProjectionMatrix)
	{
		m_viewProjectionMatrix.SetValue(viewProjectionMatrix);
	}

	public void SetColorCore(Color color)
	{
		m_colorCore.SetValue(new Vector4(color));
	}

	public void SetColorCenter(Color color)
	{
		m_colorCenter.SetValue(new Vector4(color));
	}

	public void SetColorOuter(Color color)
	{
		m_colorOuter.SetValue(new Vector4(color));
	}

	public void SetCenter(Vector2 center)
	{
		m_center.SetValue(center);
	}

	public void SetUnitWidth(float width)
	{
		m_unitWidth.SetValue(width);
	}
}
