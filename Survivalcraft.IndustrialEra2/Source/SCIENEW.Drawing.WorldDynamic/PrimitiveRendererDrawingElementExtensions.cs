using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW.Drawing.WorldDynamic;

public static class PrimitiveRendererDrawingElementExtensions
{
	public static List<WorldDrawingElement> FormDrawingElements<TFlatBatch, TTexturedBatch, TFontBatch>(this BasePrimitivesRenderer<TFlatBatch, TTexturedBatch, TFontBatch> primitivesRenderer, ModelRenderingMode renderingMode) where TFlatBatch : BaseFlatBatch, new() where TTexturedBatch : BaseTexturedBatch, new() where TFontBatch : BaseFontBatch, new()
	{
		List<WorldDrawingElement> list = new List<WorldDrawingElement>();
		foreach (TFlatBatch flatBatch in primitivesRenderer.m_flatBatches)
		{
			list.Add(FromFlatBatch(flatBatch, renderingMode));
		}
		foreach (TFontBatch fontBatch in primitivesRenderer.m_fontBatches)
		{
			list.Add(FromFontBatch(fontBatch, renderingMode));
		}
		foreach (TTexturedBatch texturedBatch in primitivesRenderer.m_texturedBatches)
		{
			list.Add(FromTexturedBatch(texturedBatch, renderingMode));
		}
		return list;
	}

	private static DynamicMeshDrawingElement FromTexturedBatch(BaseTexturedBatch batch, ModelRenderingMode renderingMode)
	{
		DynamicMeshDrawingElement dynamicMeshDrawingElement = new DynamicMeshDrawingElement(DrawingMaterial.Opaque(batch.Texture, batch.SamplerState).WithVertexNormals(useVertexNormals: false))
		{
			RenderingMode = renderingMode
		};
		dynamicMeshDrawingElement.Mesh.AppendTexturedBatch(batch);
		dynamicMeshDrawingElement.MarkGeometryDirty();
		return dynamicMeshDrawingElement;
	}

	private static DynamicMeshDrawingElement FromFontBatch(BaseFontBatch batch, ModelRenderingMode renderingMode)
	{
		DynamicMeshDrawingElement dynamicMeshDrawingElement = new DynamicMeshDrawingElement(DrawingMaterial.Opaque(batch.Font.Texture, SamplerState.PointWrap).WithVertexNormals(useVertexNormals: false))
		{
			RenderingMode = renderingMode
		};
		dynamicMeshDrawingElement.Mesh.AppendFontBatch(batch);
		dynamicMeshDrawingElement.MarkGeometryDirty();
		return dynamicMeshDrawingElement;
	}

	private static DynamicMeshDrawingElement FromFlatBatch(BaseFlatBatch batch, ModelRenderingMode renderingMode)
	{
		DynamicMeshDrawingElement dynamicMeshDrawingElement = new DynamicMeshDrawingElement(DrawingMaterial.Opaque(IndustrialModLoader.PureWhiteTexture, SamplerState.PointWrap).WithVertexNormals(useVertexNormals: false))
		{
			RenderingMode = renderingMode
		};
		dynamicMeshDrawingElement.Mesh.AppendFlatBatch(batch);
		dynamicMeshDrawingElement.MarkGeometryDirty();
		return dynamicMeshDrawingElement;
	}

	public static void AppendTexturedBatch(this DynamicMeshBuilder mesh, BaseTexturedBatch batch, int instance = 0)
	{
		int count = mesh.Vertices.Count;
		foreach (VertexPositionColorTexture triangleVertex in batch.TriangleVertices)
		{
			mesh.Vertices.Add(new VertexPNCTI(triangleVertex.Position, Vector3.Zero, triangleVertex.Color, triangleVertex.TexCoord, instance));
		}
		foreach (int triangleIndex in batch.TriangleIndices)
		{
			mesh.Indices.Add(count + triangleIndex);
		}
	}

	public static void AppendFontBatch(this DynamicMeshBuilder mesh, BaseFontBatch batch, int instance = 0)
	{
		int count = mesh.Vertices.Count;
		foreach (VertexPositionColorTexture triangleVertex in batch.TriangleVertices)
		{
			mesh.Vertices.Add(new VertexPNCTI(triangleVertex.Position, Vector3.Zero, triangleVertex.Color, triangleVertex.TexCoord, instance));
		}
		foreach (int triangleIndex in batch.TriangleIndices)
		{
			mesh.Indices.Add(count + triangleIndex);
		}
	}

	public static void AppendFlatBatch(this DynamicMeshBuilder mesh, BaseFlatBatch batch, int instance = 0)
	{
		int count = mesh.Vertices.Count;
		foreach (VertexPositionColor triangleVertex in batch.TriangleVertices)
		{
			mesh.Vertices.Add(new VertexPNCTI(triangleVertex.Position, Vector3.Zero, triangleVertex.Color, Vector2.Zero, instance));
		}
		foreach (int triangleIndex in batch.TriangleIndices)
		{
			mesh.Indices.Add(count + triangleIndex);
		}
	}
}
