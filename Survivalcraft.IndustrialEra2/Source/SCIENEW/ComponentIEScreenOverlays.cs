using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ComponentIEScreenOverlays : Component, IDrawable, IUpdateable
{
	public PrimitivesRenderer2D m_primitivesRenderer2D = new PrimitivesRenderer2D();

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public UpdateOrder UpdateOrder => UpdateOrder.Reset;

	public int[] DrawOrders => new int[1] { 1101 };

	public float SludgeFactor { get; set; }

	public float PetroleumFactor { get; set; }

	public float YellowFactor { get; set; }

	public void Update(float dt)
	{
		SludgeFactor = 0f;
		PetroleumFactor = 0f;
		YellowFactor = 0f;
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (SludgeFactor > 0.01f)
		{
			DrawOverlay(camera, new Color(36, 28, 20), SludgeFactor, MathUtils.Saturate(2f * SludgeFactor));
		}
		if (PetroleumFactor > 0.01f)
		{
			DrawOverlay(camera, Color.Black, PetroleumFactor, MathUtils.Saturate(2f * PetroleumFactor));
		}
		if (YellowFactor > 0.01f)
		{
			DrawOverlay(camera, new Color(255, 200, 0), YellowFactor, MathUtils.Saturate(2f * YellowFactor));
		}
	}

	public virtual void DrawOverlay(Camera camera, Color color, float innerFactor, float outerFactor)
	{
		Vector2 viewportSize = camera.ViewportSize;
		Vector2 vector = new Vector2(0f, 0f);
		Vector2 vector2 = new Vector2(viewportSize.X, 0f);
		Vector2 vector3 = new Vector2(viewportSize.X, viewportSize.Y);
		Vector2 vector4 = new Vector2(0f, viewportSize.Y);
		Vector2 p = new Vector2(viewportSize.X / 2f, viewportSize.Y / 2f);
		Color color2 = color * outerFactor;
		Color color3 = color * innerFactor;
		FlatBatch2D flatBatch2D = m_primitivesRenderer2D.FlatBatch(0, DepthStencilState.None, null, BlendState.AlphaBlend);
		int count = flatBatch2D.TriangleVertices.Count;
		flatBatch2D.QueueTriangle(vector, vector2, p, 0f, color2, color2, color3);
		flatBatch2D.QueueTriangle(vector2, vector3, p, 0f, color2, color2, color3);
		flatBatch2D.QueueTriangle(vector3, vector4, p, 0f, color2, color2, color3);
		flatBatch2D.QueueTriangle(vector4, vector, p, 0f, color2, color2, color3);
		flatBatch2D.TransformTriangles(camera.ViewportMatrix, count);
		flatBatch2D.Flush();
	}
}
