using System;
using System.Collections.Generic;
using Engine.Graphics;
using Engine.Media;
using Game;

namespace SCIENEW;

public static class LEDScreenTextureManager
{
	public struct LEDFacialData : IEquatable<LEDFacialData>
	{
		public bool[] m_points = new bool[4];

		public bool[] m_sides = new bool[4];

		public LEDFacialData(bool[] points, bool[] sides)
		{
			int num = points.Length;
			bool flag = ((num > 4 || num < 0) ? true : false);
			bool flag2 = flag;
			if (!flag2)
			{
				int num2 = sides.Length;
				bool flag3 = ((num2 > 4 || num2 < 0) ? true : false);
				flag2 = flag3;
			}
			if (flag2)
			{
				throw new IndexOutOfRangeException();
			}
			m_points = points;
			m_sides = sides;
		}

		public bool Equals(LEDFacialData other)
		{
			if (m_points.SequenceEqual(other.m_points))
			{
				return m_sides.SequenceEqual(other.m_sides);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is LEDFacialData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(m_points[0], m_points[1], m_points[2], m_points[3], m_sides[0], m_sides[1], m_sides[2], m_sides[3]);
		}

		public static bool operator ==(LEDFacialData left, LEDFacialData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LEDFacialData left, LEDFacialData right)
		{
			return !left.Equals(right);
		}
	}

	private static List<LEDFacialData> m_facialData = new List<LEDFacialData>();

	public static Dictionary<LEDFacialData, Texture2D> m_facialDataToSlot = new Dictionary<LEDFacialData, Texture2D>();

	public static void Init()
	{
		GenerateFacialData();
		DrawFaces();
	}

	private static void DrawFaces()
	{
		Image image = ContentManager.Get<Image>("Textures/LEDScreenFaceBase");
		Image rectangleImage = ContentManager.Get<Image>("Textures/LEDScreenFaceDefault");
		for (int i = 0; i < m_facialData.Count; i++)
		{
			LEDFacialData key = m_facialData[i];
			Image image2 = new Image(image);
			if (key.m_points[0])
			{
				DrawRectangle(image2, rectangleImage, 0, 0, 2, 2);
			}
			if (key.m_points[1])
			{
				DrawRectangle(image2, rectangleImage, 30, 0, 2, 2);
			}
			if (key.m_points[2])
			{
				DrawRectangle(image2, rectangleImage, 30, 30, 2, 2);
			}
			if (key.m_points[3])
			{
				DrawRectangle(image2, rectangleImage, 0, 30, 2, 2);
			}
			if (key.m_sides[0])
			{
				DrawRectangle(image2, rectangleImage, 2, 0, 28, 2);
			}
			if (key.m_sides[1])
			{
				DrawRectangle(image2, rectangleImage, 30, 2, 2, 28);
			}
			if (key.m_sides[2])
			{
				DrawRectangle(image2, rectangleImage, 2, 30, 28, 2);
			}
			if (key.m_sides[3])
			{
				DrawRectangle(image2, rectangleImage, 0, 2, 2, 28);
			}
			m_facialDataToSlot.Add(key, Texture2D.Load(image2));
		}
	}

	private static void GenerateFacialData()
	{
		bool[] points = new bool[4];
		bool[] sides = new bool[4];
		m_facialData.Add(new LEDFacialData(points, sides));
		for (int i = 0; i < 4; i++)
		{
			bool[] array = new bool[4];
			array[i] = true;
			m_facialData.Add(new LEDFacialData(array, sides));
		}
		for (int j = 0; j < 4; j++)
		{
			for (int k = 1; k < 4 - j; k++)
			{
				bool[] array2 = new bool[4];
				array2[j] = true;
				array2[k + j] = true;
				m_facialData.Add(new LEDFacialData(array2, sides));
			}
		}
		for (int l = 0; l < 4; l++)
		{
			bool[] array3 = new bool[4] { true, true, true, true };
			array3[l] = false;
			m_facialData.Add(new LEDFacialData(array3, sides));
		}
		m_facialData.Add(new LEDFacialData(new bool[4] { true, true, true, true }, sides));
		for (int m = 0; m < 4; m++)
		{
			bool[] array4 = new bool[4];
			bool[] array5 = new bool[4];
			array4[m] = true;
			array5[m] = true;
			array5[(m + 1) % 4] = true;
			m_facialData.Add(new LEDFacialData(array5, array4));
		}
		for (int n = 0; n < 4; n++)
		{
			bool[] array6 = new bool[4] { true, true, true, true };
			bool[] array7 = new bool[4] { true, true, true, true };
			array6[n] = false;
			array7[n] = false;
			array7[(n + 3) % 4] = false;
			m_facialData.Add(new LEDFacialData(array6, array7));
		}
		for (int num = 0; num < 4; num++)
		{
			for (int num2 = 0; num2 < 4; num2++)
			{
				bool[] array8 = new bool[4] { true, true, true, true };
				bool[] array9 = new bool[4];
				array8[num2] = false;
				array9[(num2 + num) % 4] = true;
				m_facialData.Add(new LEDFacialData(array8, array9));
			}
		}
		for (int num3 = 0; num3 < 4; num3++)
		{
			bool[] points2 = new bool[4] { true, true, true, true };
			bool[] array10 = new bool[4] { true, true, true, true };
			array10[num3] = false;
			m_facialData.Add(new LEDFacialData(points2, array10));
		}
		for (int num4 = 0; num4 < 4; num4++)
		{
			for (int num5 = 1; num5 < 4 - num4; num5++)
			{
				bool[] points3 = new bool[4] { true, true, true, true };
				bool[] array11 = new bool[4] { true, true, true, true };
				array11[num4] = false;
				array11[num5 + num4] = false;
				m_facialData.Add(new LEDFacialData(points3, array11));
			}
		}
		for (int num6 = 0; num6 < 4; num6++)
		{
			bool[] points4 = new bool[4] { true, true, true, true };
			bool[] array12 = new bool[4];
			array12[num6] = true;
			m_facialData.Add(new LEDFacialData(points4, array12));
		}
		m_facialData.Add(new LEDFacialData(new bool[4] { true, true, true, true }, new bool[4] { true, true, true, true }));
	}

	private static void DrawRectangle(Image image, Image rectangleImage, int x, int y, int w, int h)
	{
		for (int i = 0; i < w; i++)
		{
			for (int j = 0; j < h; j++)
			{
				int x2 = x + i;
				int y2 = y + j;
				image.SetPixel(x2, y2, rectangleImage.GetPixel(x2, y2));
			}
		}
	}
}
