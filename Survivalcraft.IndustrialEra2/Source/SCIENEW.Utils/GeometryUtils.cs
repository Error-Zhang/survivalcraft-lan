using Engine;
using Engine.Graphics;

namespace SCIENEW.Utils;

public static class GeometryUtils
{
	public static DynamicArray<VertexPositionColor> GetCrossVertices(Vector2 center, float width = 0.5f)
	{
		return new DynamicArray<VertexPositionColor>
		{
			new VertexPositionColor(new Vector3(center.X - width, 0f, center.Y - width), Color.White),
			new VertexPositionColor(new Vector3(center.X + width, 0f, center.Y + width), Color.White),
			new VertexPositionColor(new Vector3(center.X + width, 255f, center.Y + width), Color.White),
			new VertexPositionColor(new Vector3(center.X - width, 255f, center.Y - width), Color.White),
			new VertexPositionColor(new Vector3(center.X + width, 0f, center.Y - width), Color.White),
			new VertexPositionColor(new Vector3(center.X - width, 0f, center.Y + width), Color.White),
			new VertexPositionColor(new Vector3(center.X - width, 255f, center.Y + width), Color.White),
			new VertexPositionColor(new Vector3(center.X + width, 255f, center.Y - width), Color.White)
		};
	}

	public static DynamicArray<int> GetCrossIndices()
	{
		return new DynamicArray<int>
		{
			0, 1, 2, 0, 3, 2, 4, 5, 6, 4,
			7, 6, 2, 1, 0, 2, 3, 0, 6, 5,
			4, 6, 7, 4
		};
	}
}
