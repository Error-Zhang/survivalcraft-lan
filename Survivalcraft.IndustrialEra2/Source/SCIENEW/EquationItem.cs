using System;

namespace SCIENEW;

public struct EquationItem(int[] values, int count) : IEquatable<EquationItem>
{
	public int[] Values = values;

	public int Count = count;

	public bool Equals(EquationItem other)
	{
		if ((Values == null && other.Values == null) || Values.Equals(other.Values))
		{
			return Count == other.Count;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is EquationItem other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Values, Count);
	}

	public static bool operator ==(EquationItem left, EquationItem right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(EquationItem left, EquationItem right)
	{
		return !left.Equals(right);
	}
}
