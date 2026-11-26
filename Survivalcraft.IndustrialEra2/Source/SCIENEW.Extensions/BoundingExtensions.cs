using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace SCIENEW.Extensions;

public static class BoundingExtensions
{
	public static DynamicArray<VertexPositionColor> GetVertices(this BoundingBox box)
	{
		Vector3 min = box.Min;
		Vector3 max = box.Max;
		return new DynamicArray<VertexPositionColor>
		{
			new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), Color.White),
			new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), Color.White),
			new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), Color.White),
			new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), Color.White),
			new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), Color.White),
			new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), Color.White),
			new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), Color.White),
			new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), Color.White)
		};
	}

	public static DynamicArray<int> GetIndices(this BoundingBox box, int faceMask)
	{
		DynamicArray<int> dynamicArray = new DynamicArray<int>();
		if ((faceMask & 1) != 0)
		{
			dynamicArray.AddRange((IEnumerable<int>)new _003C_003Ez__ReadOnlyArray<int>(new int[6] { 4, 0, 3, 4, 3, 5 }));
		}
		if ((faceMask & 2) != 0)
		{
			dynamicArray.AddRange((IEnumerable<int>)new _003C_003Ez__ReadOnlyArray<int>(new int[6] { 7, 6, 2, 7, 2, 1 }));
		}
		if ((faceMask & 4) != 0)
		{
			dynamicArray.AddRange((IEnumerable<int>)new _003C_003Ez__ReadOnlyArray<int>(new int[6] { 0, 1, 2, 0, 2, 3 }));
		}
		if ((faceMask & 8) != 0)
		{
			dynamicArray.AddRange((IEnumerable<int>)new _003C_003Ez__ReadOnlyArray<int>(new int[6] { 4, 5, 6, 4, 6, 7 }));
		}
		if ((faceMask & 0x10) != 0)
		{
			dynamicArray.AddRange((IEnumerable<int>)new _003C_003Ez__ReadOnlyArray<int>(new int[6] { 4, 7, 1, 4, 1, 0 }));
		}
		if ((faceMask & 0x20) != 0)
		{
			dynamicArray.AddRange((IEnumerable<int>)new _003C_003Ez__ReadOnlyArray<int>(new int[6] { 5, 3, 2, 5, 2, 6 }));
		}
		return dynamicArray;
	}

	public static BoundingBox Wrap(this BoundingBox boundingBox, float thickness)
	{
		boundingBox.Min -= Vector3.One * thickness;
		boundingBox.Max += Vector3.One * thickness;
		return boundingBox;
	}
}
