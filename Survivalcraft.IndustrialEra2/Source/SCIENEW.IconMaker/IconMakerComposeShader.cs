using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW.IconMaker;

public class IconMakerComposeShader : Shader
{
	public ShaderParameter m_worldViewProjectionMatrixParameter;

	public ShaderParameter m_outputSize;

	public ShaderParameter m_background;

	public ShaderParameter m_bgSampler;

	public ShaderParameter m_subject;

	public ShaderParameter m_subjectSampler;

	public ShaderParameter m_outlineColor;

	public ShaderParameter m_outlineWidth;

	public ShaderParameter m_outlineEnabled;

	public ShaderParameter m_glowColor;

	public ShaderParameter m_glowRadius;

	public ShaderParameter m_glowIntensity;

	public ShaderParameter m_glowOffset;

	public ShaderParameter m_glowEnabled;

	public ShaderParameter m_glowMode;

	public readonly ShaderTransforms Transforms;

	public IconMakerComposeShader()
		: base(ShaderCodeManager.GetFast("Shaders/IconMakerCompose.vsh"), ShaderCodeManager.GetFast("Shaders/IconMakerCompose.psh"))
	{
		m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", allowNull: true);
		m_outputSize = GetParameter("u_outputSize", allowNull: true);
		m_background = GetParameter("u_background", allowNull: true);
		m_bgSampler = GetParameter("u_bgSampler", allowNull: true);
		m_subject = GetParameter("u_subject", allowNull: true);
		m_subjectSampler = GetParameter("u_subjectSampler", allowNull: true);
		m_outlineColor = GetParameter("u_outlineColor", allowNull: true);
		m_outlineWidth = GetParameter("u_outlineWidth", allowNull: true);
		m_outlineEnabled = GetParameter("u_outlineEnabled", allowNull: true);
		m_glowColor = GetParameter("u_glowColor", allowNull: true);
		m_glowRadius = GetParameter("u_glowRadius", allowNull: true);
		m_glowIntensity = GetParameter("u_glowIntensity", allowNull: true);
		m_glowOffset = GetParameter("u_glowOffset", allowNull: true);
		m_glowEnabled = GetParameter("u_glowEnabled", allowNull: true);
		m_glowMode = GetParameter("u_glowMode", allowNull: true);
		Transforms = new ShaderTransforms(1);
	}

	public void ApplyProject(IconMakerProject project, Vector2 outputSize, Texture2D background, Texture2D subject)
	{
		m_outputSize.SetValue(outputSize);
		m_background.SetValue(background);
		m_bgSampler.SetValue(SamplerState.LinearClamp);
		m_subject.SetValue(subject);
		m_subjectSampler.SetValue(SamplerState.LinearClamp);
		m_outlineColor.SetValue(new Vector4(project.OutlineColor));
		m_outlineWidth.SetValue(MathUtils.Max(project.OutlineWidth, 0f));
		m_outlineEnabled.SetValue(project.OutlineEnabled ? 1f : 0f);
		m_glowColor.SetValue(new Vector4(project.GlowColor));
		m_glowRadius.SetValue(MathUtils.Max(project.GlowRadius, 0f));
		m_glowIntensity.SetValue(MathUtils.Clamp(project.GlowIntensity, 0f, 2f));
		m_glowOffset.SetValue(project.GlowOffset);
		m_glowEnabled.SetValue(project.GlowEnabled ? 1f : 0f);
		m_glowMode.SetValue((project.GlowMode == IconMakerGlowMode.Shadow) ? 1f : 0f);
	}

	public override void PrepareForDrawingOverride()
	{
		Transforms.UpdateMatrices(1, worldView: false, viewProjection: false, worldViewProjection: true);
		m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
	}
}
