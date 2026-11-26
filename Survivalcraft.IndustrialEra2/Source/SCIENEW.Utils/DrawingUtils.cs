using System;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW.Utils;

public static class DrawingUtils
{
	private static BlockHighlightShader m_blockHighlightShader = new BlockHighlightShader();

	private static CherenkovRadiationShader m_cherenkovRadiationShader = new CherenkovRadiationShader();

	public static void DrawBoundingBoxHighlight(Camera camera, BoundingBox box, Color color1, Color color2, float offset, float scale, int faceMask, RasterizerState? rasterizerState = null, BlendState? blendState = null, DepthStencilState? depthStencilState = null)
	{
		if (!camera.ViewFrustum.Intersection(box))
		{
			return;
		}
		DynamicArray<int> indices = box.GetIndices(faceMask);
		DynamicArray<VertexPositionColor> vertices = box.GetVertices();
		Vector3 translation = camera.InvertedViewMatrix.Translation;
		Vector3 vector = new Vector3(MathF.Floor(translation.X), 0f, MathF.Floor(translation.Z));
		Matrix viewProjectionMatrix = Matrix.CreateTranslation(vector - translation) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
		try
		{
			Display.BlendState = blendState ?? BlendState.NonPremultiplied;
			Display.DepthStencilState = depthStencilState ?? DepthStencilState.Default;
			Display.RasterizerState = rasterizerState ?? RasterizerState.CullClockwiseScissor;
			m_blockHighlightShader.SetOrigin(vector.XZ);
			m_blockHighlightShader.SetViewProjectionMatrix(viewProjectionMatrix);
			m_blockHighlightShader.SetColor1(color1);
			m_blockHighlightShader.SetColor2(color2);
			m_blockHighlightShader.SetOffset(offset);
			m_blockHighlightShader.SetScale(scale);
			Display.DrawUserIndexed(PrimitiveType.TriangleList, m_blockHighlightShader, VertexPositionColor.VertexDeclaration, vertices.Array, 0, vertices.Count, indices.Array, 0, indices.Count);
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			throw;
		}
	}

	public static void DrawCherenkovRadiation(Camera camera, Vector2 center, float width, Color colorCore, Color colorCenter, Color colorOuter, RasterizerState? rasterizerState = null, BlendState? blendState = null, DepthStencilState? depthStencilState = null)
	{
		if (!camera.ViewFrustum.Intersection(new BoundingBox(new Vector3(center.X, 0f, center.Y), new Vector3(center.X, 255f, center.Y))))
		{
			return;
		}
		DynamicArray<int> crossIndices = GeometryUtils.GetCrossIndices();
		DynamicArray<VertexPositionColor> crossVertices = GeometryUtils.GetCrossVertices(center, width);
		Vector3 translation = camera.InvertedViewMatrix.Translation;
		Vector3 vector = new Vector3(MathF.Floor(translation.X), 0f, MathF.Floor(translation.Z));
		Matrix viewProjectionMatrix = Matrix.CreateTranslation(vector - translation) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
		try
		{
			Display.BlendState = blendState ?? BlendState.NonPremultiplied;
			Display.DepthStencilState = depthStencilState ?? DepthStencilState.Default;
			Display.RasterizerState = rasterizerState ?? RasterizerState.CullClockwiseScissor;
			m_cherenkovRadiationShader.SetOrigin(vector.XZ);
			m_cherenkovRadiationShader.SetViewProjectionMatrix(viewProjectionMatrix);
			m_cherenkovRadiationShader.SetColorCenter(colorCenter);
			m_cherenkovRadiationShader.SetColorCore(colorCore);
			m_cherenkovRadiationShader.SetColorOuter(colorOuter);
			m_cherenkovRadiationShader.SetCenter(center);
			m_cherenkovRadiationShader.SetUnitWidth(MathF.Sqrt(2f) * width);
			Display.DrawUserIndexed(PrimitiveType.TriangleList, m_cherenkovRadiationShader, VertexPositionColor.VertexDeclaration, crossVertices.Array, 0, crossVertices.Count, crossIndices.Array, 0, crossIndices.Count);
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			throw;
		}
	}

	public static void DrawBoundingBoxFace(FlatBatch3D batch, Vector3 c1, Vector3 c2, Color color)
	{
		float num = 0.005f;
		batch.QueueQuad(new Vector3(c1.X, c1.Y, c2.Z), new Vector3(c2.X, c1.Y, c2.Z), new Vector3(c2.X, c1.Y + num, c2.Z), new Vector3(c1.X, c1.Y + num, c2.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c2.Y, c2.Z), new Vector3(c1.X, c2.Y, c2.Z), new Vector3(c1.X, c2.Y - num, c2.Z), new Vector3(c2.X, c2.Y - num, c2.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c1.Y, c2.Z), new Vector3(c2.X, c2.Y, c2.Z), new Vector3(c2.X - num, c2.Y, c2.Z), new Vector3(c2.X - num, c1.Y, c2.Z), color);
		batch.QueueQuad(new Vector3(c1.X, c2.Y, c2.Z), new Vector3(c1.X, c1.Y, c2.Z), new Vector3(c1.X + num, c1.Y, c2.Z), new Vector3(c1.X + num, c2.Y, c2.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c1.Y, c2.Z), new Vector3(c2.X, c2.Y, c2.Z), new Vector3(c2.X, c2.Y, c2.Z - num), new Vector3(c2.X, c1.Y, c2.Z - num), color);
		batch.QueueQuad(new Vector3(c2.X, c1.Y, c1.Z), new Vector3(c2.X, c2.Y, c1.Z), new Vector3(c2.X, c2.Y, c1.Z + num), new Vector3(c2.X, c1.Y, c1.Z + num), color);
		batch.QueueQuad(new Vector3(c2.X, c2.Y, c1.Z), new Vector3(c2.X, c2.Y, c2.Z), new Vector3(c2.X, c2.Y - num, c2.Z), new Vector3(c2.X, c2.Y - num, c1.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c1.Y, c1.Z), new Vector3(c2.X, c1.Y, c2.Z), new Vector3(c2.X, c1.Y + num, c2.Z), new Vector3(c2.X, c1.Y + num, c1.Z), color);
		batch.QueueQuad(new Vector3(c1.X, c1.Y, c1.Z), new Vector3(c2.X, c1.Y, c1.Z), new Vector3(c2.X, c1.Y + num, c1.Z), new Vector3(c1.X, c1.Y + num, c1.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c1.Y, c1.Z), new Vector3(c2.X, c2.Y, c1.Z), new Vector3(c2.X - num, c2.Y, c1.Z), new Vector3(c2.X - num, c1.Y, c1.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c2.Y, c1.Z), new Vector3(c1.X, c2.Y, c1.Z), new Vector3(c1.X, c2.Y - num, c1.Z), new Vector3(c2.X, c2.Y - num, c1.Z), color);
		batch.QueueQuad(new Vector3(c1.X, c2.Y, c1.Z), new Vector3(c1.X, c1.Y, c1.Z), new Vector3(c1.X + num, c1.Y, c1.Z), new Vector3(c1.X + num, c2.Y, c1.Z), color);
		batch.QueueQuad(new Vector3(c1.X, c2.Y, c2.Z), new Vector3(c1.X, c1.Y, c2.Z), new Vector3(c1.X, c1.Y, c2.Z - num), new Vector3(c1.X, c2.Y, c2.Z - num), color);
		batch.QueueQuad(new Vector3(c1.X, c2.Y, c1.Z), new Vector3(c1.X, c1.Y, c1.Z), new Vector3(c1.X, c1.Y, c1.Z + num), new Vector3(c1.X, c2.Y, c1.Z + num), color);
		batch.QueueQuad(new Vector3(c1.X, c1.Y, c1.Z), new Vector3(c1.X, c1.Y, c2.Z), new Vector3(c1.X, c1.Y + num, c2.Z), new Vector3(c1.X, c1.Y + num, c1.Z), color);
		batch.QueueQuad(new Vector3(c1.X, c2.Y, c1.Z), new Vector3(c1.X, c2.Y, c2.Z), new Vector3(c1.X, c2.Y - num, c2.Z), new Vector3(c1.X, c2.Y - num, c1.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c2.Y, c2.Z), new Vector3(c1.X, c2.Y, c2.Z), new Vector3(c1.X, c2.Y, c2.Z - num), new Vector3(c2.X, c2.Y, c2.Z - num), color);
		batch.QueueQuad(new Vector3(c2.X, c2.Y, c1.Z), new Vector3(c1.X, c2.Y, c1.Z), new Vector3(c1.X, c2.Y, c1.Z + num), new Vector3(c2.X, c2.Y, c1.Z + num), color);
		batch.QueueQuad(new Vector3(c1.X, c2.Y, c1.Z), new Vector3(c1.X, c2.Y, c2.Z), new Vector3(c1.X + num, c2.Y, c2.Z), new Vector3(c1.X + num, c2.Y, c1.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c2.Y, c1.Z), new Vector3(c2.X, c2.Y, c2.Z), new Vector3(c2.X - num, c2.Y, c2.Z), new Vector3(c2.X - num, c2.Y, c1.Z), color);
		batch.QueueQuad(new Vector3(c1.X, c1.Y, c2.Z), new Vector3(c2.X, c1.Y, c2.Z), new Vector3(c2.X, c1.Y, c2.Z - num), new Vector3(c1.X, c1.Y, c2.Z - num), color);
		batch.QueueQuad(new Vector3(c1.X, c1.Y, c1.Z), new Vector3(c2.X, c1.Y, c1.Z), new Vector3(c2.X, c1.Y, c1.Z + num), new Vector3(c1.X, c1.Y, c1.Z + num), color);
		batch.QueueQuad(new Vector3(c1.X, c1.Y, c1.Z), new Vector3(c1.X, c1.Y, c2.Z), new Vector3(c1.X + num, c1.Y, c2.Z), new Vector3(c1.X + num, c1.Y, c1.Z), color);
		batch.QueueQuad(new Vector3(c2.X, c1.Y, c1.Z), new Vector3(c2.X, c1.Y, c2.Z), new Vector3(c2.X - num, c1.Y, c2.Z), new Vector3(c2.X - num, c1.Y, c1.Z), color);
	}
}
