using System;

namespace SCIENEW;

public struct EquationFluid(int value, float volume) : IEquatable<EquationFluid>
{
	public int Value = value;

	public float Volume = volume;

	public bool Equals(EquationFluid other)
	{
		if (Value == other.Value)
		{
			return Volume.Equals(other.Volume);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is EquationFluid other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Value, Volume);
	}

	public static bool operator ==(EquationFluid left, EquationFluid right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(EquationFluid left, EquationFluid right)
	{
		return !left.Equals(right);
	}
}
