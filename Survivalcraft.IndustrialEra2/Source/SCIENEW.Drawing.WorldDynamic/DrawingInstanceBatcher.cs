using System;
using System.Collections.Generic;
using Engine;

namespace SCIENEW.Drawing.WorldDynamic;

public static class DrawingInstanceBatcher
{
	public static IEnumerable<DrawingInstanceBatch> CreateBatches(DynamicArray<VertexPNCTI> vertices, DynamicArray<int> indices, int instanceCount, int maxInstances)
	{
		if (maxInstances < 1)
		{
			throw new ArgumentOutOfRangeException("maxInstances", "Max instances must be at least 1.");
		}
		for (int transformOffset = 0; transformOffset < instanceCount; transformOffset += maxInstances)
		{
			int transformCount = Math.Min(maxInstances, instanceCount - transformOffset);
			yield return CreateBatch(vertices, indices, transformOffset, transformCount);
		}
	}

	private static DrawingInstanceBatch CreateBatch(DynamicArray<VertexPNCTI> sourceVertices, DynamicArray<int> sourceIndices, int transformOffset, int transformCount)
	{
		DynamicArray<VertexPNCTI> dynamicArray = new DynamicArray<VertexPNCTI>();
		DynamicArray<int> dynamicArray2 = new DynamicArray<int>();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < sourceVertices.Count; i++)
		{
			VertexPNCTI item = sourceVertices.Array[i];
			if (item.Instance >= transformOffset && item.Instance < transformOffset + transformCount)
			{
				int count = dynamicArray.Count;
				item.Instance -= transformOffset;
				dynamicArray.Add(item);
				dictionary.Add(i, count);
			}
		}
		for (int j = 0; j + 2 < sourceIndices.Count; j += 3)
		{
			int key = sourceIndices.Array[j];
			int key2 = sourceIndices.Array[j + 1];
			int key3 = sourceIndices.Array[j + 2];
			if (dictionary.TryGetValue(key, out var value) && dictionary.TryGetValue(key2, out var value2) && dictionary.TryGetValue(key3, out var value3))
			{
				dynamicArray2.Add(value);
				dynamicArray2.Add(value2);
				dynamicArray2.Add(value3);
			}
		}
		return new DrawingInstanceBatch(transformOffset, transformCount, dynamicArray, dynamicArray2);
	}
}
