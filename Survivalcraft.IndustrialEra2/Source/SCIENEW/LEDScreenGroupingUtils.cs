using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using SCIENEW.Screens;

namespace SCIENEW;

public static class LEDScreenGroupingUtils
{
	public static Point2[] Point3ArrayToPoint2(Point3[] points, int face)
	{
		switch (face)
		{
		case 0:
		case 2:
			return points.Select((Point3 p) => new Point2(p.X, p.Y)).ToArray();
		case 1:
		case 3:
			return points.Select((Point3 p) => new Point2(p.Y, p.Z)).ToArray();
		default:
			return points.Select((Point3 p) => new Point2(p.X, p.Z)).ToArray();
		}
	}

	public static void QueueInscribedRectOutline(FlatBatch3D batch, int face, Point3 anchor, LEDScreenInscribedRect rect, Color color)
	{
		if (rect.IsValid)
		{
			Screen screen = ScreenGeometry.FromFlatInscribedRect(face, anchor, rect.TopLeft.X, rect.TopLeft.Y, rect.Width, rect.Height, 0.501f, 0.0625f);
			batch.QueueLine(screen.WorldPos1, screen.WorldPos2, color, color);
			batch.QueueLine(screen.WorldPos2, screen.WorldPos3, color, color);
			batch.QueueLine(screen.WorldPos3, screen.WorldPos4, color, color);
			batch.QueueLine(screen.WorldPos4, screen.WorldPos1, color, color);
		}
	}

	public static LEDScreenInscribedRect ComputeMaxInscribedRect(Point3[] points, int face)
	{
		if (points.Length == 0)
		{
			return default;
		}
		Point2[] array = Point3ArrayToPoint2(points, face);
		int num = array.Min((Point2 p) => p.X);
		int num2 = array.Min((Point2 p) => p.Y);
		int num3 = array.Max((Point2 p) => p.X);
		int num4 = array.Max((Point2 p) => p.Y) - num2 + 1;
		int num5 = num3 - num + 1;
		bool[,] array2 = new bool[num4, num5];
		Point2[] array3 = array;
		for (int num6 = 0; num6 < array3.Length; num6++)
		{
			Point2 point = array3[num6];
			array2[point.Y - num2, point.X - num] = true;
		}
		int[] array4 = new int[num5];
		LEDScreenInscribedRect result = default;
		for (int num7 = 0; num7 < num4; num7++)
		{
			for (int num8 = 0; num8 < num5; num8++)
			{
				array4[num8] = (array2[num7, num8] ? (array4[num8] + 1) : 0);
			}
			LEDScreenInscribedRect lEDScreenInscribedRect = LargestRectangleArea(array4, num7, num, num2);
			if (lEDScreenInscribedRect.Area > result.Area)
			{
				result = lEDScreenInscribedRect;
			}
		}
		return result;
	}

	private static LEDScreenInscribedRect LargestRectangleArea(int[] heights, int bottomRowIndex, int gridMinX, int gridMinY)
	{
		Stack<int> stack = new Stack<int>();
		int[] array = new int[heights.Length + 1];
		Array.Copy(heights, array, heights.Length);
		LEDScreenInscribedRect result = default;
		for (int i = 0; i < array.Length; i++)
		{
			while (stack.Count > 0 && array[stack.Peek()] > array[i])
			{
				int num = stack.Pop();
				int num2 = array[num];
				int num3 = ((stack.Count != 0) ? (stack.Peek() + 1) : 0);
				int num4 = i - num3;
				int num5 = num2 * num4;
				if (num5 > result.Area)
				{
					result = new LEDScreenInscribedRect
					{
						Area = num5,
						TopLeft = new Point2(gridMinX + num3, gridMinY + bottomRowIndex - num2 + 1),
						Width = num4,
						Height = num2
					};
				}
			}
			stack.Push(i);
		}
		return result;
	}
}
