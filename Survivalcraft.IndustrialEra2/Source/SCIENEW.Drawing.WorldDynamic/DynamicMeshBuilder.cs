using System.Linq;
using Engine;

namespace SCIENEW.Drawing.WorldDynamic;

public sealed class DynamicMeshBuilder
{
	public DynamicArray<VertexPNCTI> Vertices { get; } = new DynamicArray<VertexPNCTI>();

	public DynamicArray<int> Indices { get; } = new DynamicArray<int>();

	public void Clear()
	{
		Vertices.Clear();
		Indices.Clear();
	}

	public void QueueTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 texCoord1, Vector2 texCoord2, Vector2 texCoord3, Color vertexColor, int instance = 0)
	{
		Vector3 normal = Vector3.Normalize(Vector3.Cross(p1 - p2, p3 - p2));
		int count = Vertices.Count;
		Vertices.Count += 3;
		Vertices.Array[count] = new VertexPNCTI(p1, normal, vertexColor, texCoord1, instance);
		Vertices.Array[count + 1] = new VertexPNCTI(p2, normal, vertexColor, texCoord2, instance);
		Vertices.Array[count + 2] = new VertexPNCTI(p3, normal, vertexColor, texCoord3, instance);
		int count2 = Indices.Count;
		Indices.Count += 3;
		Indices.Array[count2] = count;
		Indices.Array[count2 + 1] = count + 1;
		Indices.Array[count2 + 2] = count + 2;
	}

	public void QueueQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector2 texCoord1, Vector2 texCoord2, Vector2 texCoord3, Vector2 texCoord4, Color vertexColor, int instance = 0)
	{
		Vector3 normal = Vector3.Normalize(Vector3.Cross(p1 - p2, p3 - p2));
		int count = Vertices.Count;
		Vertices.Count += 4;
		Vertices.Array[count] = new VertexPNCTI(p1, normal, vertexColor, texCoord1, instance);
		Vertices.Array[count + 1] = new VertexPNCTI(p2, normal, vertexColor, texCoord2, instance);
		Vertices.Array[count + 2] = new VertexPNCTI(p3, normal, vertexColor, texCoord3, instance);
		Vertices.Array[count + 3] = new VertexPNCTI(p4, normal, vertexColor, texCoord4, instance);
		int count2 = Indices.Count;
		Indices.Count += 6;
		Indices.Array[count2] = count;
		Indices.Array[count2 + 1] = count + 1;
		Indices.Array[count2 + 2] = count + 2;
		Indices.Array[count2 + 3] = count + 2;
		Indices.Array[count2 + 4] = count + 3;
		Indices.Array[count2 + 5] = count;
	}

	public BoundingBox CalculateBounds()
	{
		if (Vertices.Count != 0)
		{
			return new BoundingBox(from vertex in Vertices.Array.Take(Vertices.Count)
				select vertex.Position);
		}
		return default;
	}
}
