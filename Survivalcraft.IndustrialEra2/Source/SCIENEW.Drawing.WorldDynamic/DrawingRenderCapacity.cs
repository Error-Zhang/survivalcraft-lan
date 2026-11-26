using System;
using Engine.Graphics;

namespace SCIENEW.Drawing.WorldDynamic;

public static class DrawingRenderCapacity
{
	public const int DefaultUniformOverhead = 13;

	public const int DefaultSafetyMargin = 4;

	public const int MaxSupportedInstances = 64;

	public static int CalculateMaxInstances(int maxVertexUniformVectors, int nonArrayOverhead = 13, int safetyMargin = 4)
	{
		return Math.Clamp((maxVertexUniformVectors - nonArrayOverhead - safetyMargin) / 8, 1, 64);
	}

	public static int CalculateCurrentDeviceMaxInstances()
	{
		return CalculateMaxInstances(GLWrapper.GL_MAX_VERTEX_UNIFORM_VECTORS);
	}
}
