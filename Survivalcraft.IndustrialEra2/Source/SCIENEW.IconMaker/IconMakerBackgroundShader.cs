using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW.IconMaker;

public class IconMakerBackgroundShader : Shader
{
	public ShaderParameter m_worldViewProjectionMatrixParameter;

	public ShaderParameter m_outputSize;

	public ShaderParameter m_cropMin;

	public ShaderParameter m_cropMax;

	public ShaderParameter m_tileScale;

	public ShaderParameter m_blurRadius;

	public ShaderParameter m_brightness;

	public ShaderParameter m_contrast;

	public ShaderParameter m_saturation;

	public ShaderParameter m_vibrance;

	public ShaderParameter m_texture;

	public ShaderParameter m_samplerState;

	public readonly ShaderTransforms Transforms;

	public IconMakerBackgroundShader()
		: base(ShaderCodeManager.GetFast("Shaders/IconMakerBackground.vsh"), ShaderCodeManager.GetFast("Shaders/IconMakerBackground.psh"))
	{
		m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", allowNull: true);
		m_outputSize = GetParameter("u_outputSize", allowNull: true);
		m_cropMin = GetParameter("u_cropMin", allowNull: true);
		m_cropMax = GetParameter("u_cropMax", allowNull: true);
		m_tileScale = GetParameter("u_tileScale", allowNull: true);
		m_blurRadius = GetParameter("u_blurRadius", allowNull: true);
		m_brightness = GetParameter("u_brightness", allowNull: true);
		m_contrast = GetParameter("u_contrast", allowNull: true);
		m_saturation = GetParameter("u_saturation", allowNull: true);
		m_vibrance = GetParameter("u_vibrance", allowNull: true);
		m_texture = GetParameter("u_texture", allowNull: true);
		m_samplerState = GetParameter("u_samplerState", allowNull: true);
		Transforms = new ShaderTransforms(1);
	}

	public void SetTexture(Texture2D texture, SamplerState samplerState)
	{
		m_texture.SetValue(texture);
		m_samplerState.SetValue(samplerState);
	}

	public void ApplyProject(IconMakerProject project, Vector2 outputSize)
	{
		m_outputSize.SetValue(outputSize);
		m_cropMin.SetValue(project.BgCropMin);
		m_cropMax.SetValue(project.BgCropMax);
		m_tileScale.SetValue(MathUtils.Max(project.BgTileScale, 0.01f));
		m_blurRadius.SetValue(MathUtils.Max(project.BgBlur, 0f));
		m_brightness.SetValue(project.BgBrightness);
		m_contrast.SetValue(project.BgContrast);
		m_saturation.SetValue(project.BgSaturation);
		m_vibrance.SetValue(project.BgVibrance);
	}

	public override void PrepareForDrawingOverride()
	{
		Transforms.UpdateMatrices(1, worldView: false, viewProjection: false, worldViewProjection: true);
		m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
	}
}
