using System;
using Engine;

namespace SCIENEW.Ponder;

public class DynamicPonderBlock : IPonderBlock, IEquatable<DynamicPonderBlock>
{
	protected int m_value;

	protected Matrix m_matrix = Matrix.Identity;

	public int Value => m_value;

	public Matrix Matrix
	{
		get
		{
			return m_matrix;
		}
		set
		{
			m_matrix = value;
		}
	}

	public Vector3 Position => m_matrix.Translation;

	public Color Color { get; set; }

	public DynamicPonderBlock(int value, Matrix matrix, Color color)
	{
		m_value = value;
		m_matrix = matrix;
		Color = color;
	}

	public bool Equals(DynamicPonderBlock? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (m_value == other.m_value && m_matrix.Equals(other.m_matrix))
		{
			return Color.Equals(other.Color);
		}
		return false;
	}
}
