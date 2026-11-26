using Engine;

namespace SCIENEW.Drawing.WorldDynamic;

public readonly record struct DrawingInstanceBatch(int TransformOffset, int TransformCount, DynamicArray<VertexPNCTI> Vertices, DynamicArray<int> Indices);
