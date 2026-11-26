using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW.Drawing.WorldDynamic;

public abstract class WorldDrawingElement : IDisposable, IComparable<WorldDrawingElement>
{
	private VertexBuffer? m_vertexBuffer;

	private IndexBuffer? m_indexBuffer;

	private bool m_geometryDirty = true;

	public ModelRenderingMode RenderingMode { get; set; }

	public int PrepareOrder { get; set; }

	public DrawingMaterial Material { get; set; }

	public BoundingBox Bounds { get; protected set; }

	public float Light { get; set; } = 1f;

	public Matrix[] WorldTransformsForCamera { get; protected set; } = new Matrix[1] { Matrix.Identity };

	public VertexBuffer? VertexBuffer => m_vertexBuffer;

	public IndexBuffer? IndexBuffer => m_indexBuffer;

	protected WorldDrawingElement(DrawingMaterial material)
	{
		Material = material;
	}

	public virtual void Preprocess(Project project, Camera camera)
	{
	}

	public virtual bool IsVisible(Project project, Camera camera, SubsystemSky sky)
	{
		if (Material.Texture == null || Bounds.Min == Bounds.Max)
		{
			return false;
		}
		float num = MathUtils.Sqr(sky.VisibilityRange);
		Vector3 boundsCenter = GetBoundsCenter();
		Vector3 vector = boundsCenter - camera.ViewPosition;
		vector.Y *= sky.VisibilityRangeYMultiplier;
		if (vector.LengthSquared() >= num)
		{
			return false;
		}
		BoundingSphere sphere = new BoundingSphere(boundsCenter, (Bounds.Max - Bounds.Min).Length() * 0.5f);
		return camera.ViewFrustum.Intersection(sphere);
	}

	public virtual Vector3 GetLightSamplePosition()
	{
		return GetBoundsCenter();
	}

	public virtual void UpdateLight(Project project, DrawingLightSampler lightSampler)
	{
		if (Material.IsUnlit)
		{
			Light = 1f;
		}
		else
		{
			Light = lightSampler.Sample(GetLightSamplePosition(), Light);
		}
	}

	public virtual void UpdateCameraTransforms(Camera camera)
	{
		WorldTransformsForCamera[0] = camera.ViewMatrix;
	}

	public void MarkGeometryDirty()
	{
		m_geometryDirty = true;
	}

	public void BakeIfNeeded()
	{
		if (m_geometryDirty)
		{
			BakeGeometry(out DynamicArray<VertexPNCTI> vertices, out DynamicArray<int> indices, out BoundingBox bounds);
			Bounds = bounds;
			ReplaceBuffers(vertices, indices);
			m_geometryDirty = false;
		}
	}

	protected abstract void BakeGeometry(out DynamicArray<VertexPNCTI> vertices, out DynamicArray<int> indices, out BoundingBox bounds);

	private void ReplaceBuffers(DynamicArray<VertexPNCTI> vertices, DynamicArray<int> indices)
	{
		DisposeBuffers();
		if (vertices.Count > 0)
		{
			m_vertexBuffer = new VertexBuffer(VertexPNCTI.VertexDeclaration, vertices.Count);
			m_vertexBuffer.SetData(vertices.Array, 0, vertices.Count);
			m_vertexBuffer.Tag = vertices;
		}
		if (indices.Count > 0)
		{
			m_indexBuffer = new IndexBuffer(IndexFormat.ThirtyTwoBits, indices.Count);
			m_indexBuffer.SetData(indices.Array, 0, indices.Count);
			m_indexBuffer.Tag = indices;
		}
	}

	private Vector3 GetBoundsCenter()
	{
		return (Bounds.Min + Bounds.Max) * 0.5f;
	}

	public int CompareTo(WorldDrawingElement? other)
	{
		if (other != null)
		{
			return PrepareOrder.CompareTo(other.PrepareOrder);
		}
		return 1;
	}

	public void Dispose()
	{
		DisposeBuffers();
		GC.SuppressFinalize(this);
	}

	private void DisposeBuffers()
	{
		m_vertexBuffer?.Dispose();
		m_vertexBuffer = null;
		m_indexBuffer?.Dispose();
		m_indexBuffer = null;
	}
}
