using Engine;

namespace SCIENEW.Drawing.WorldDynamic;

public class DynamicMeshDrawingElement : WorldDrawingElement
{
	public DynamicMeshBuilder Mesh { get; } = new DynamicMeshBuilder();

	public DynamicMeshDrawingElement(DrawingMaterial material)
		: base(material)
	{
	}

	public void Clear()
	{
		Mesh.Clear();
		MarkGeometryDirty();
	}

	protected override void BakeGeometry(out DynamicArray<VertexPNCTI> vertices, out DynamicArray<int> indices, out BoundingBox bounds)
	{
		vertices = Mesh.Vertices;
		indices = Mesh.Indices;
		bounds = Mesh.CalculateBounds();
	}
}
