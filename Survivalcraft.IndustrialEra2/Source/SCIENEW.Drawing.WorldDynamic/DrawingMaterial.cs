using Engine;
using Engine.Graphics;

namespace SCIENEW.Drawing.WorldDynamic;

public readonly record struct DrawingMaterial(Texture2D Texture, SamplerState SamplerState, Vector3 Tint, float Opacity, Vector4 EmissionColor, bool UseVertexNormals, bool IsUnlit)
{
	public static DrawingMaterial Opaque(Texture2D texture, SamplerState? samplerState = null)
	{
		return new DrawingMaterial(texture, samplerState ?? Engine.Graphics.SamplerState.PointWrap, Vector3.One, 1f, Vector4.Zero, UseVertexNormals: true, IsUnlit: false);
	}

	public DrawingMaterial WithTint(Color color)
	{
		return this with
		{
			Tint = new Vector3(color),
			Opacity = (float)(int)color.A / 255f
		};
	}

	public DrawingMaterial WithOpacity(float opacity)
	{
		return this with
		{
			Opacity = MathUtils.Saturate(opacity)
		};
	}

	public DrawingMaterial WithEmission(Vector4 emissionColor)
	{
		return this with
		{
			EmissionColor = emissionColor
		};
	}

	public DrawingMaterial WithVertexNormals(bool useVertexNormals)
	{
		return this with
		{
			UseVertexNormals = useVertexNormals
		};
	}

	public DrawingMaterial WithUnlit(bool isUnlit)
	{
		return this with
		{
			IsUnlit = isUnlit
		};
	}
}
