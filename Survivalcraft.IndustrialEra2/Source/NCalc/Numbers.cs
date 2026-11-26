using System;

namespace NCalc;

public class Numbers
{
	private static object ConvertIfString(object s)
	{
		if (s is string || s is char)
		{
			return decimal.Parse(s.ToString());
		}
		return s;
	}

	private static object ConvertIfBoolean(object input)
	{
		if (input is bool flag)
		{
			return flag ? 1 : 0;
		}
		return input;
	}

	public static object Add(object a, object b, EvaluateOptions options)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (options.HasFlag(EvaluateOptions.BooleanCalculation))
		{
			a = ConvertIfBoolean(a);
			b = ConvertIfBoolean(b);
		}
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		switch (typeCode)
		{
		case TypeCode.Boolean:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'bool'");
			case TypeCode.Byte:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.SByte:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Int16:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.UInt16:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Int32:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.UInt32:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Int64:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Single:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Double:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Decimal:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
			}
			break;
		case TypeCode.Byte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'byte' and 'bool'");
			case TypeCode.Byte:
				return (byte)a + (byte)b;
			case TypeCode.SByte:
				return (byte)a + (sbyte)b;
			case TypeCode.Int16:
				return (byte)a + (short)b;
			case TypeCode.UInt16:
				return (byte)a + (ushort)b;
			case TypeCode.Int32:
				return (byte)a + (int)b;
			case TypeCode.UInt32:
				return (byte)a + (uint)b;
			case TypeCode.Int64:
				return (byte)a + (long)b;
			case TypeCode.UInt64:
				return (byte)a + (ulong)b;
			case TypeCode.Single:
				return (float)(int)(byte)a + (float)b;
			case TypeCode.Double:
				return (double)(int)(byte)a + (double)b;
			case TypeCode.Decimal:
				return (decimal)(byte)a + (decimal)b;
			}
			break;
		case TypeCode.SByte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'bool'");
			case TypeCode.Byte:
				return (sbyte)a + (byte)b;
			case TypeCode.SByte:
				return (sbyte)a + (sbyte)b;
			case TypeCode.Int16:
				return (sbyte)a + (short)b;
			case TypeCode.UInt16:
				return (sbyte)a + (ushort)b;
			case TypeCode.Int32:
				return (sbyte)a + (int)b;
			case TypeCode.UInt32:
				return (sbyte)a + (uint)b;
			case TypeCode.Int64:
				return (sbyte)a + (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'ulong'");
			case TypeCode.Single:
				return (float)(sbyte)a + (float)b;
			case TypeCode.Double:
				return (double)(sbyte)a + (double)b;
			case TypeCode.Decimal:
				return (decimal)(sbyte)a + (decimal)b;
			}
			break;
		case TypeCode.Int16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'bool'");
			case TypeCode.Byte:
				return (short)a + (byte)b;
			case TypeCode.SByte:
				return (short)a + (sbyte)b;
			case TypeCode.Int16:
				return (short)a + (short)b;
			case TypeCode.UInt16:
				return (short)a + (ushort)b;
			case TypeCode.Int32:
				return (short)a + (int)b;
			case TypeCode.UInt32:
				return (short)a + (uint)b;
			case TypeCode.Int64:
				return (short)a + (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'ulong'");
			case TypeCode.Single:
				return (float)(short)a + (float)b;
			case TypeCode.Double:
				return (double)(short)a + (double)b;
			case TypeCode.Decimal:
				return (decimal)(short)a + (decimal)b;
			}
			break;
		case TypeCode.UInt16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ushort' and 'bool'");
			case TypeCode.Byte:
				return (ushort)a + (byte)b;
			case TypeCode.SByte:
				return (ushort)a + (sbyte)b;
			case TypeCode.Int16:
				return (ushort)a + (short)b;
			case TypeCode.UInt16:
				return (ushort)a + (ushort)b;
			case TypeCode.Int32:
				return (ushort)a + (int)b;
			case TypeCode.UInt32:
				return (ushort)a + (uint)b;
			case TypeCode.Int64:
				return (ushort)a + (long)b;
			case TypeCode.UInt64:
				return (ushort)a + (ulong)b;
			case TypeCode.Single:
				return (float)(int)(ushort)a + (float)b;
			case TypeCode.Double:
				return (double)(int)(ushort)a + (double)b;
			case TypeCode.Decimal:
				return (decimal)(ushort)a + (decimal)b;
			}
			break;
		case TypeCode.Int32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'bool'");
			case TypeCode.Byte:
				return (int)a + (byte)b;
			case TypeCode.SByte:
				return (int)a + (sbyte)b;
			case TypeCode.Int16:
				return (int)a + (short)b;
			case TypeCode.UInt16:
				return (int)a + (ushort)b;
			case TypeCode.Int32:
				return (int)a + (int)b;
			case TypeCode.UInt32:
				return (int)a + (uint)b;
			case TypeCode.Int64:
				return (int)a + (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'ulong'");
			case TypeCode.Single:
				return (float)(int)a + (float)b;
			case TypeCode.Double:
				return (double)(int)a + (double)b;
			case TypeCode.Decimal:
				return (decimal)(int)a + (decimal)b;
			}
			break;
		case TypeCode.UInt32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'unit' and 'bool'");
			case TypeCode.Byte:
				return (uint)a + (byte)b;
			case TypeCode.SByte:
				return (uint)a + (sbyte)b;
			case TypeCode.Int16:
				return (uint)a + (short)b;
			case TypeCode.UInt16:
				return (uint)a + (ushort)b;
			case TypeCode.Int32:
				return (uint)a + (int)b;
			case TypeCode.UInt32:
				return (uint)a + (uint)b;
			case TypeCode.Int64:
				return (uint)a + (long)b;
			case TypeCode.UInt64:
				return (uint)a + (ulong)b;
			case TypeCode.Single:
				return (float)(uint)a + (float)b;
			case TypeCode.Double:
				return (double)(uint)a + (double)b;
			case TypeCode.Decimal:
				return (decimal)(uint)a + (decimal)b;
			}
			break;
		case TypeCode.Int64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'bool'");
			case TypeCode.Byte:
				return (long)a + (byte)b;
			case TypeCode.SByte:
				return (long)a + (sbyte)b;
			case TypeCode.Int16:
				return (long)a + (short)b;
			case TypeCode.UInt16:
				return (long)a + (ushort)b;
			case TypeCode.Int32:
				return (long)a + (int)b;
			case TypeCode.UInt32:
				return (long)a + (uint)b;
			case TypeCode.Int64:
				return (long)a + (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'ulong'");
			case TypeCode.Single:
				return (float)(long)a + (float)b;
			case TypeCode.Double:
				return (double)(long)a + (double)b;
			case TypeCode.Decimal:
				return (decimal)(long)a + (decimal)b;
			}
			break;
		case TypeCode.UInt64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'bool'");
			case TypeCode.Byte:
				return (ulong)a + (byte)b;
			case TypeCode.SByte:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'sbyte'");
			case TypeCode.Int16:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'short'");
			case TypeCode.UInt16:
				return (ulong)a + (ushort)b;
			case TypeCode.Int32:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'int'");
			case TypeCode.UInt32:
				return (ulong)a + (uint)b;
			case TypeCode.Int64:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'ulong'");
			case TypeCode.UInt64:
				return (ulong)a + (ulong)b;
			case TypeCode.Single:
				return (float)(ulong)a + (float)b;
			case TypeCode.Double:
				return (double)(ulong)a + (double)b;
			case TypeCode.Decimal:
				return (decimal)(ulong)a + (decimal)b;
			}
			break;
		case TypeCode.Single:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'float' and 'bool'");
			case TypeCode.Byte:
				return (float)a + (float)(int)(byte)b;
			case TypeCode.SByte:
				return (float)a + (float)(sbyte)b;
			case TypeCode.Int16:
				return (float)a + (float)(short)b;
			case TypeCode.UInt16:
				return (float)a + (float)(int)(ushort)b;
			case TypeCode.Int32:
				return (float)a + (float)(int)b;
			case TypeCode.UInt32:
				return (float)a + (float)(uint)b;
			case TypeCode.Int64:
				return (float)a + (float)(long)b;
			case TypeCode.UInt64:
				return (float)a + (float)(ulong)b;
			case TypeCode.Single:
				return (float)a + (float)b;
			case TypeCode.Double:
				return (double)(float)a + (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) + (decimal)b;
			}
			break;
		case TypeCode.Double:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'double' and 'bool'");
			case TypeCode.Byte:
				return (double)a + (double)(int)(byte)b;
			case TypeCode.SByte:
				return (double)a + (double)(sbyte)b;
			case TypeCode.Int16:
				return (double)a + (double)(short)b;
			case TypeCode.UInt16:
				return (double)a + (double)(int)(ushort)b;
			case TypeCode.Int32:
				return (double)a + (double)(int)b;
			case TypeCode.UInt32:
				return (double)a + (double)(uint)b;
			case TypeCode.Int64:
				return (double)a + (double)(long)b;
			case TypeCode.UInt64:
				return (double)a + (double)(ulong)b;
			case TypeCode.Single:
				return (double)a + (double)(float)b;
			case TypeCode.Double:
				return (double)a + (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) + (decimal)b;
			}
			break;
		case TypeCode.Decimal:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'decimal' and 'bool'");
			case TypeCode.Byte:
				return (decimal)a + (decimal)(byte)b;
			case TypeCode.SByte:
				return (decimal)a + (decimal)(sbyte)b;
			case TypeCode.Int16:
				return (decimal)a + (decimal)(short)b;
			case TypeCode.UInt16:
				return (decimal)a + (decimal)(ushort)b;
			case TypeCode.Int32:
				return (decimal)a + (decimal)(int)b;
			case TypeCode.UInt32:
				return (decimal)a + (decimal)(uint)b;
			case TypeCode.Int64:
				return (decimal)a + (decimal)(long)b;
			case TypeCode.UInt64:
				return (decimal)a + (decimal)(ulong)b;
			case TypeCode.Single:
				return (decimal)a + Convert.ToDecimal(b);
			case TypeCode.Double:
				return (decimal)a + Convert.ToDecimal(b);
			case TypeCode.Decimal:
				return (decimal)a + (decimal)b;
			}
			break;
		}
		return null;
	}

	public static object AddChecked(object a, object b, EvaluateOptions options)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (options.HasFlag(EvaluateOptions.BooleanCalculation))
		{
			a = ConvertIfBoolean(a);
			b = ConvertIfBoolean(b);
		}
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		checked
		{
			switch (typeCode)
			{
			case TypeCode.Boolean:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'bool'");
				case TypeCode.Byte:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.SByte:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Int16:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.UInt16:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Int32:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.UInt32:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Int64:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Single:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Double:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Decimal:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
				}
				break;
			case TypeCode.Byte:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'byte' and 'bool'");
				case TypeCode.Byte:
					return (byte)a + (byte)b;
				case TypeCode.SByte:
					return (byte)a + (sbyte)b;
				case TypeCode.Int16:
					return (byte)a + (short)b;
				case TypeCode.UInt16:
					return (byte)a + (ushort)b;
				case TypeCode.Int32:
					return (byte)a + (int)b;
				case TypeCode.UInt32:
					return (byte)a + (uint)b;
				case TypeCode.Int64:
					return (byte)a + (long)b;
				case TypeCode.UInt64:
					return (byte)a + (ulong)b;
				case TypeCode.Single:
					return (float)unchecked((int)(byte)a) + (float)b;
				case TypeCode.Double:
					return (double)unchecked((int)(byte)a) + (double)b;
				case TypeCode.Decimal:
					return (decimal)(byte)a + (decimal)b;
				}
				break;
			case TypeCode.SByte:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'bool'");
				case TypeCode.Byte:
					return (sbyte)a + (byte)b;
				case TypeCode.SByte:
					return (sbyte)a + (sbyte)b;
				case TypeCode.Int16:
					return (sbyte)a + (short)b;
				case TypeCode.UInt16:
					return (sbyte)a + (ushort)b;
				case TypeCode.Int32:
					return (sbyte)a + (int)b;
				case TypeCode.UInt32:
					return (sbyte)a + (uint)b;
				case TypeCode.Int64:
					return (sbyte)a + (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'ulong'");
				case TypeCode.Single:
					return (float)(sbyte)a + (float)b;
				case TypeCode.Double:
					return (double)(sbyte)a + (double)b;
				case TypeCode.Decimal:
					return (decimal)(sbyte)a + (decimal)b;
				}
				break;
			case TypeCode.Int16:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'bool'");
				case TypeCode.Byte:
					return (short)a + (byte)b;
				case TypeCode.SByte:
					return (short)a + (sbyte)b;
				case TypeCode.Int16:
					return (short)a + (short)b;
				case TypeCode.UInt16:
					return (short)a + (ushort)b;
				case TypeCode.Int32:
					return (short)a + (int)b;
				case TypeCode.UInt32:
					return (short)a + (uint)b;
				case TypeCode.Int64:
					return (short)a + (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'ulong'");
				case TypeCode.Single:
					return (float)(short)a + (float)b;
				case TypeCode.Double:
					return (double)(short)a + (double)b;
				case TypeCode.Decimal:
					return (decimal)(short)a + (decimal)b;
				}
				break;
			case TypeCode.UInt16:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ushort' and 'bool'");
				case TypeCode.Byte:
					return (ushort)a + (byte)b;
				case TypeCode.SByte:
					return (ushort)a + (sbyte)b;
				case TypeCode.Int16:
					return (ushort)a + (short)b;
				case TypeCode.UInt16:
					return (ushort)a + (ushort)b;
				case TypeCode.Int32:
					return (ushort)a + (int)b;
				case TypeCode.UInt32:
					return (ushort)a + (uint)b;
				case TypeCode.Int64:
					return (ushort)a + (long)b;
				case TypeCode.UInt64:
					return (ushort)a + (ulong)b;
				case TypeCode.Single:
					return (float)unchecked((int)(ushort)a) + (float)b;
				case TypeCode.Double:
					return (double)unchecked((int)(ushort)a) + (double)b;
				case TypeCode.Decimal:
					return (decimal)(ushort)a + (decimal)b;
				}
				break;
			case TypeCode.Int32:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'bool'");
				case TypeCode.Byte:
					return (int)a + (byte)b;
				case TypeCode.SByte:
					return (int)a + (sbyte)b;
				case TypeCode.Int16:
					return (int)a + (short)b;
				case TypeCode.UInt16:
					return (int)a + (ushort)b;
				case TypeCode.Int32:
					return (int)a + (int)b;
				case TypeCode.UInt32:
					return (int)a + (uint)b;
				case TypeCode.Int64:
					return (int)a + (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'ulong'");
				case TypeCode.Single:
					return (float)(int)a + (float)b;
				case TypeCode.Double:
					return (double)(int)a + (double)b;
				case TypeCode.Decimal:
					return (decimal)(int)a + (decimal)b;
				}
				break;
			case TypeCode.UInt32:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'unit' and 'bool'");
				case TypeCode.Byte:
					return (uint)a + (byte)b;
				case TypeCode.SByte:
					return (uint)a + (sbyte)b;
				case TypeCode.Int16:
					return (uint)a + (short)b;
				case TypeCode.UInt16:
					return (uint)a + (ushort)b;
				case TypeCode.Int32:
					return (uint)a + (int)b;
				case TypeCode.UInt32:
					return (uint)a + (uint)b;
				case TypeCode.Int64:
					return (uint)a + (long)b;
				case TypeCode.UInt64:
					return (uint)a + (ulong)b;
				case TypeCode.Single:
					return (float)(uint)a + (float)b;
				case TypeCode.Double:
					return (double)(uint)a + (double)b;
				case TypeCode.Decimal:
					return (decimal)(uint)a + (decimal)b;
				}
				break;
			case TypeCode.Int64:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'bool'");
				case TypeCode.Byte:
					return (long)a + (byte)b;
				case TypeCode.SByte:
					return (long)a + (sbyte)b;
				case TypeCode.Int16:
					return (long)a + (short)b;
				case TypeCode.UInt16:
					return (long)a + (ushort)b;
				case TypeCode.Int32:
					return (long)a + (int)b;
				case TypeCode.UInt32:
					return (long)a + (uint)b;
				case TypeCode.Int64:
					return (long)a + (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'ulong'");
				case TypeCode.Single:
					return (float)(long)a + (float)b;
				case TypeCode.Double:
					return (double)(long)a + (double)b;
				case TypeCode.Decimal:
					return (decimal)(long)a + (decimal)b;
				}
				break;
			case TypeCode.UInt64:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'bool'");
				case TypeCode.Byte:
					return (ulong)a + (byte)b;
				case TypeCode.SByte:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'sbyte'");
				case TypeCode.Int16:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'short'");
				case TypeCode.UInt16:
					return (ulong)a + (ushort)b;
				case TypeCode.Int32:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'int'");
				case TypeCode.UInt32:
					return (ulong)a + (uint)b;
				case TypeCode.Int64:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'ulong'");
				case TypeCode.UInt64:
					return (ulong)a + (ulong)b;
				case TypeCode.Single:
					return (float)(ulong)a + (float)b;
				case TypeCode.Double:
					return (double)(ulong)a + (double)b;
				case TypeCode.Decimal:
					return (decimal)(ulong)a + (decimal)b;
				}
				break;
			case TypeCode.Single:
				unchecked
				{
					switch (typeCode2)
					{
					case TypeCode.Boolean:
						throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'float' and 'bool'");
					case TypeCode.Byte:
						return (float)a + (float)(int)(byte)b;
					case TypeCode.SByte:
						return (float)a + (float)(sbyte)b;
					case TypeCode.Int16:
						return (float)a + (float)(short)b;
					case TypeCode.UInt16:
						return (float)a + (float)(int)(ushort)b;
					case TypeCode.Int32:
						return (float)a + (float)(int)b;
					case TypeCode.UInt32:
						return (float)a + (float)(uint)b;
					case TypeCode.Int64:
						return (float)a + (float)(long)b;
					case TypeCode.UInt64:
						return (float)a + (float)(ulong)b;
					case TypeCode.Single:
						return (float)a + (float)b;
					case TypeCode.Double:
						return (double)(float)a + (double)b;
					case TypeCode.Decimal:
						return Convert.ToDecimal(a) + (decimal)b;
					}
					break;
				}
			case TypeCode.Double:
				unchecked
				{
					switch (typeCode2)
					{
					case TypeCode.Boolean:
						throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'double' and 'bool'");
					case TypeCode.Byte:
						return (double)a + (double)(int)(byte)b;
					case TypeCode.SByte:
						return (double)a + (double)(sbyte)b;
					case TypeCode.Int16:
						return (double)a + (double)(short)b;
					case TypeCode.UInt16:
						return (double)a + (double)(int)(ushort)b;
					case TypeCode.Int32:
						return (double)a + (double)(int)b;
					case TypeCode.UInt32:
						return (double)a + (double)(uint)b;
					case TypeCode.Int64:
						return (double)a + (double)(long)b;
					case TypeCode.UInt64:
						return (double)a + (double)(ulong)b;
					case TypeCode.Single:
						return (double)a + (double)(float)b;
					case TypeCode.Double:
						return (double)a + (double)b;
					case TypeCode.Decimal:
						return Convert.ToDecimal(a) + (decimal)b;
					}
					break;
				}
			case TypeCode.Decimal:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'decimal' and 'bool'");
				case TypeCode.Byte:
					return (decimal)a + (decimal)(byte)b;
				case TypeCode.SByte:
					return (decimal)a + (decimal)(sbyte)b;
				case TypeCode.Int16:
					return (decimal)a + (decimal)(short)b;
				case TypeCode.UInt16:
					return (decimal)a + (decimal)(ushort)b;
				case TypeCode.Int32:
					return (decimal)a + (decimal)(int)b;
				case TypeCode.UInt32:
					return (decimal)a + (decimal)(uint)b;
				case TypeCode.Int64:
					return (decimal)a + (decimal)(long)b;
				case TypeCode.UInt64:
					return (decimal)a + (decimal)(ulong)b;
				case TypeCode.Single:
					return (decimal)a + Convert.ToDecimal(b);
				case TypeCode.Double:
					return (decimal)a + Convert.ToDecimal(b);
				case TypeCode.Decimal:
					return (decimal)a + (decimal)b;
				}
				break;
			}
			return null;
		}
	}

	public static object Soustract(object a, object b, EvaluateOptions options)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (options.HasFlag(EvaluateOptions.BooleanCalculation))
		{
			a = ConvertIfBoolean(a);
			b = ConvertIfBoolean(b);
		}
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		switch (typeCode)
		{
		case TypeCode.Boolean:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'bool'");
			case TypeCode.Byte:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.SByte:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Int16:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.UInt16:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Int32:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.UInt32:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Int64:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Single:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Double:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			case TypeCode.Decimal:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
			}
			break;
		case TypeCode.Byte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'byte' and 'bool'");
			case TypeCode.SByte:
				return (byte)a - (sbyte)b;
			case TypeCode.Int16:
				return (byte)a - (short)b;
			case TypeCode.UInt16:
				return (byte)a - (ushort)b;
			case TypeCode.Int32:
				return (byte)a - (int)b;
			case TypeCode.UInt32:
				return (byte)a - (uint)b;
			case TypeCode.Int64:
				return (byte)a - (long)b;
			case TypeCode.UInt64:
				return (byte)a - (ulong)b;
			case TypeCode.Single:
				return (float)(int)(byte)a - (float)b;
			case TypeCode.Double:
				return (double)(int)(byte)a - (double)b;
			case TypeCode.Decimal:
				return (decimal)(byte)a - (decimal)b;
			}
			break;
		case TypeCode.SByte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'bool'");
			case TypeCode.SByte:
				return (sbyte)a - (sbyte)b;
			case TypeCode.Int16:
				return (sbyte)a - (short)b;
			case TypeCode.UInt16:
				return (sbyte)a - (ushort)b;
			case TypeCode.Int32:
				return (sbyte)a - (int)b;
			case TypeCode.UInt32:
				return (sbyte)a - (uint)b;
			case TypeCode.Int64:
				return (sbyte)a - (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'ulong'");
			case TypeCode.Single:
				return (float)(sbyte)a - (float)b;
			case TypeCode.Double:
				return (double)(sbyte)a - (double)b;
			case TypeCode.Decimal:
				return (decimal)(sbyte)a - (decimal)b;
			}
			break;
		case TypeCode.Int16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'bool'");
			case TypeCode.SByte:
				return (short)a - (sbyte)b;
			case TypeCode.Int16:
				return (short)a - (short)b;
			case TypeCode.UInt16:
				return (short)a - (ushort)b;
			case TypeCode.Int32:
				return (short)a - (int)b;
			case TypeCode.UInt32:
				return (short)a - (uint)b;
			case TypeCode.Int64:
				return (short)a - (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'ulong'");
			case TypeCode.Single:
				return (float)(short)a - (float)b;
			case TypeCode.Double:
				return (double)(short)a - (double)b;
			case TypeCode.Decimal:
				return (decimal)(short)a - (decimal)b;
			}
			break;
		case TypeCode.UInt16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ushort' and 'bool'");
			case TypeCode.SByte:
				return (ushort)a - (sbyte)b;
			case TypeCode.Int16:
				return (ushort)a - (short)b;
			case TypeCode.UInt16:
				return (ushort)a - (ushort)b;
			case TypeCode.Int32:
				return (ushort)a - (int)b;
			case TypeCode.UInt32:
				return (ushort)a - (uint)b;
			case TypeCode.Int64:
				return (ushort)a - (long)b;
			case TypeCode.UInt64:
				return (ushort)a - (ulong)b;
			case TypeCode.Single:
				return (float)(int)(ushort)a - (float)b;
			case TypeCode.Double:
				return (double)(int)(ushort)a - (double)b;
			case TypeCode.Decimal:
				return (decimal)(ushort)a - (decimal)b;
			}
			break;
		case TypeCode.Int32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'bool'");
			case TypeCode.SByte:
				return (int)a - (sbyte)b;
			case TypeCode.Int16:
				return (int)a - (short)b;
			case TypeCode.UInt16:
				return (int)a - (ushort)b;
			case TypeCode.Int32:
				return (int)a - (int)b;
			case TypeCode.UInt32:
				return (int)a - (uint)b;
			case TypeCode.Int64:
				return (int)a - (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'ulong'");
			case TypeCode.Single:
				return (float)(int)a - (float)b;
			case TypeCode.Double:
				return (double)(int)a - (double)b;
			case TypeCode.Decimal:
				return (decimal)(int)a - (decimal)b;
			}
			break;
		case TypeCode.UInt32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'uint' and 'bool'");
			case TypeCode.SByte:
				return (uint)a - (sbyte)b;
			case TypeCode.Int16:
				return (uint)a - (short)b;
			case TypeCode.UInt16:
				return (uint)a - (ushort)b;
			case TypeCode.Int32:
				return (uint)a - (int)b;
			case TypeCode.UInt32:
				return (uint)a - (uint)b;
			case TypeCode.Int64:
				return (uint)a - (long)b;
			case TypeCode.UInt64:
				return (uint)a - (ulong)b;
			case TypeCode.Single:
				return (float)(uint)a - (float)b;
			case TypeCode.Double:
				return (double)(uint)a - (double)b;
			case TypeCode.Decimal:
				return (decimal)(uint)a - (decimal)b;
			}
			break;
		case TypeCode.Int64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'bool'");
			case TypeCode.SByte:
				return (long)a - (sbyte)b;
			case TypeCode.Int16:
				return (long)a - (short)b;
			case TypeCode.UInt16:
				return (long)a - (ushort)b;
			case TypeCode.Int32:
				return (long)a - (int)b;
			case TypeCode.UInt32:
				return (long)a - (uint)b;
			case TypeCode.Int64:
				return (long)a - (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'ulong'");
			case TypeCode.Single:
				return (float)(long)a - (float)b;
			case TypeCode.Double:
				return (double)(long)a - (double)b;
			case TypeCode.Decimal:
				return (decimal)(long)a - (decimal)b;
			}
			break;
		case TypeCode.UInt64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'bool'");
			case TypeCode.SByte:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'double'");
			case TypeCode.Int16:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'short'");
			case TypeCode.UInt16:
				return (ulong)a - (ushort)b;
			case TypeCode.Int32:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'int'");
			case TypeCode.UInt32:
				return (ulong)a - (uint)b;
			case TypeCode.Int64:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'long'");
			case TypeCode.UInt64:
				return (ulong)a - (ulong)b;
			case TypeCode.Single:
				return (float)(ulong)a - (float)b;
			case TypeCode.Double:
				return (double)(ulong)a - (double)b;
			case TypeCode.Decimal:
				return (decimal)(ulong)a - (decimal)b;
			}
			break;
		case TypeCode.Single:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'float' and 'bool'");
			case TypeCode.SByte:
				return (float)a - (float)(sbyte)b;
			case TypeCode.Int16:
				return (float)a - (float)(short)b;
			case TypeCode.UInt16:
				return (float)a - (float)(int)(ushort)b;
			case TypeCode.Int32:
				return (float)a - (float)(int)b;
			case TypeCode.UInt32:
				return (float)a - (float)(uint)b;
			case TypeCode.Int64:
				return (float)a - (float)(long)b;
			case TypeCode.UInt64:
				return (float)a - (float)(ulong)b;
			case TypeCode.Single:
				return (float)a - (float)b;
			case TypeCode.Double:
				return (double)(float)a - (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) - (decimal)b;
			}
			break;
		case TypeCode.Double:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'double' and 'bool'");
			case TypeCode.SByte:
				return (double)a - (double)(sbyte)b;
			case TypeCode.Int16:
				return (double)a - (double)(short)b;
			case TypeCode.UInt16:
				return (double)a - (double)(int)(ushort)b;
			case TypeCode.Int32:
				return (double)a - (double)(int)b;
			case TypeCode.UInt32:
				return (double)a - (double)(uint)b;
			case TypeCode.Int64:
				return (double)a - (double)(long)b;
			case TypeCode.UInt64:
				return (double)a - (double)(ulong)b;
			case TypeCode.Single:
				return (double)a - (double)(float)b;
			case TypeCode.Double:
				return (double)a - (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) - (decimal)b;
			}
			break;
		case TypeCode.Decimal:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'decimal' and 'bool'");
			case TypeCode.SByte:
				return (decimal)a - (decimal)(sbyte)b;
			case TypeCode.Int16:
				return (decimal)a - (decimal)(short)b;
			case TypeCode.UInt16:
				return (decimal)a - (decimal)(ushort)b;
			case TypeCode.Int32:
				return (decimal)a - (decimal)(int)b;
			case TypeCode.UInt32:
				return (decimal)a - (decimal)(uint)b;
			case TypeCode.Int64:
				return (decimal)a - (decimal)(long)b;
			case TypeCode.UInt64:
				return (decimal)a - (decimal)(ulong)b;
			case TypeCode.Single:
				return (decimal)a - Convert.ToDecimal(b);
			case TypeCode.Double:
				return (decimal)a - Convert.ToDecimal(b);
			case TypeCode.Decimal:
				return (decimal)a - (decimal)b;
			}
			break;
		}
		return null;
	}

	public static object SoustractChecked(object a, object b, EvaluateOptions options)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (options.HasFlag(EvaluateOptions.BooleanCalculation))
		{
			a = ConvertIfBoolean(a);
			b = ConvertIfBoolean(b);
		}
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		checked
		{
			switch (typeCode)
			{
			case TypeCode.Boolean:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'bool'");
				case TypeCode.Byte:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.SByte:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Int16:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.UInt16:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Int32:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.UInt32:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Int64:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Single:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Double:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				case TypeCode.Decimal:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
				}
				break;
			case TypeCode.Byte:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'byte' and 'bool'");
				case TypeCode.SByte:
					return (byte)a - (sbyte)b;
				case TypeCode.Int16:
					return (byte)a - (short)b;
				case TypeCode.UInt16:
					return (byte)a - (ushort)b;
				case TypeCode.Int32:
					return (byte)a - (int)b;
				case TypeCode.UInt32:
					return (byte)a - (uint)b;
				case TypeCode.Int64:
					return (byte)a - (long)b;
				case TypeCode.UInt64:
					return (byte)a - (ulong)b;
				case TypeCode.Single:
					return (float)unchecked((int)(byte)a) - (float)b;
				case TypeCode.Double:
					return (double)unchecked((int)(byte)a) - (double)b;
				case TypeCode.Decimal:
					return (decimal)(byte)a - (decimal)b;
				}
				break;
			case TypeCode.SByte:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'bool'");
				case TypeCode.SByte:
					return (sbyte)a - (sbyte)b;
				case TypeCode.Int16:
					return (sbyte)a - (short)b;
				case TypeCode.UInt16:
					return (sbyte)a - (ushort)b;
				case TypeCode.Int32:
					return (sbyte)a - (int)b;
				case TypeCode.UInt32:
					return (sbyte)a - (uint)b;
				case TypeCode.Int64:
					return (sbyte)a - (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'ulong'");
				case TypeCode.Single:
					return (float)(sbyte)a - (float)b;
				case TypeCode.Double:
					return (double)(sbyte)a - (double)b;
				case TypeCode.Decimal:
					return (decimal)(sbyte)a - (decimal)b;
				}
				break;
			case TypeCode.Int16:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'bool'");
				case TypeCode.SByte:
					return (short)a - (sbyte)b;
				case TypeCode.Int16:
					return (short)a - (short)b;
				case TypeCode.UInt16:
					return (short)a - (ushort)b;
				case TypeCode.Int32:
					return (short)a - (int)b;
				case TypeCode.UInt32:
					return (short)a - (uint)b;
				case TypeCode.Int64:
					return (short)a - (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'ulong'");
				case TypeCode.Single:
					return (float)(short)a - (float)b;
				case TypeCode.Double:
					return (double)(short)a - (double)b;
				case TypeCode.Decimal:
					return (decimal)(short)a - (decimal)b;
				}
				break;
			case TypeCode.UInt16:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ushort' and 'bool'");
				case TypeCode.SByte:
					return (ushort)a - (sbyte)b;
				case TypeCode.Int16:
					return (ushort)a - (short)b;
				case TypeCode.UInt16:
					return (ushort)a - (ushort)b;
				case TypeCode.Int32:
					return (ushort)a - (int)b;
				case TypeCode.UInt32:
					return (ushort)a - (uint)b;
				case TypeCode.Int64:
					return (ushort)a - (long)b;
				case TypeCode.UInt64:
					return (ushort)a - (ulong)b;
				case TypeCode.Single:
					return (float)unchecked((int)(ushort)a) - (float)b;
				case TypeCode.Double:
					return (double)unchecked((int)(ushort)a) - (double)b;
				case TypeCode.Decimal:
					return (decimal)(ushort)a - (decimal)b;
				}
				break;
			case TypeCode.Int32:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'bool'");
				case TypeCode.SByte:
					return (int)a - (sbyte)b;
				case TypeCode.Int16:
					return (int)a - (short)b;
				case TypeCode.UInt16:
					return (int)a - (ushort)b;
				case TypeCode.Int32:
					return (int)a - (int)b;
				case TypeCode.UInt32:
					return (int)a - (uint)b;
				case TypeCode.Int64:
					return (int)a - (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'ulong'");
				case TypeCode.Single:
					return (float)(int)a - (float)b;
				case TypeCode.Double:
					return (double)(int)a - (double)b;
				case TypeCode.Decimal:
					return (decimal)(int)a - (decimal)b;
				}
				break;
			case TypeCode.UInt32:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'uint' and 'bool'");
				case TypeCode.SByte:
					return (uint)a - (sbyte)b;
				case TypeCode.Int16:
					return (uint)a - (short)b;
				case TypeCode.UInt16:
					return (uint)a - (ushort)b;
				case TypeCode.Int32:
					return (uint)a - (int)b;
				case TypeCode.UInt32:
					return (uint)a - (uint)b;
				case TypeCode.Int64:
					return (uint)a - (long)b;
				case TypeCode.UInt64:
					return (uint)a - (ulong)b;
				case TypeCode.Single:
					return (float)(uint)a - (float)b;
				case TypeCode.Double:
					return (double)(uint)a - (double)b;
				case TypeCode.Decimal:
					return (decimal)(uint)a - (decimal)b;
				}
				break;
			case TypeCode.Int64:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'bool'");
				case TypeCode.SByte:
					return (long)a - (sbyte)b;
				case TypeCode.Int16:
					return (long)a - (short)b;
				case TypeCode.UInt16:
					return (long)a - (ushort)b;
				case TypeCode.Int32:
					return (long)a - (int)b;
				case TypeCode.UInt32:
					return (long)a - (uint)b;
				case TypeCode.Int64:
					return (long)a - (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'ulong'");
				case TypeCode.Single:
					return (float)(long)a - (float)b;
				case TypeCode.Double:
					return (double)(long)a - (double)b;
				case TypeCode.Decimal:
					return (decimal)(long)a - (decimal)b;
				}
				break;
			case TypeCode.UInt64:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'bool'");
				case TypeCode.SByte:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'double'");
				case TypeCode.Int16:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'short'");
				case TypeCode.UInt16:
					return (ulong)a - (ushort)b;
				case TypeCode.Int32:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'int'");
				case TypeCode.UInt32:
					return (ulong)a - (uint)b;
				case TypeCode.Int64:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'long'");
				case TypeCode.UInt64:
					return (ulong)a - (ulong)b;
				case TypeCode.Single:
					return (float)(ulong)a - (float)b;
				case TypeCode.Double:
					return (double)(ulong)a - (double)b;
				case TypeCode.Decimal:
					return (decimal)(ulong)a - (decimal)b;
				}
				break;
			case TypeCode.Single:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'float' and 'bool'");
				case TypeCode.SByte:
					return (float)a - (float)(sbyte)b;
				case TypeCode.Int16:
					return (float)a - (float)(short)b;
				case TypeCode.UInt16:
					return (float)a - (float)unchecked((int)(ushort)b);
				case TypeCode.Int32:
					return (float)a - (float)(int)b;
				case TypeCode.UInt32:
					return (float)a - (float)(uint)b;
				case TypeCode.Int64:
					return (float)a - (float)(long)b;
				case TypeCode.UInt64:
					return (float)a - (float)(ulong)b;
				case TypeCode.Single:
					return (float)a - (float)b;
				case TypeCode.Double:
					return (double)(float)a - (double)b;
				case TypeCode.Decimal:
					return Convert.ToDecimal(a) - (decimal)b;
				}
				break;
			case TypeCode.Double:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'double' and 'bool'");
				case TypeCode.SByte:
					return (double)a - (double)(sbyte)b;
				case TypeCode.Int16:
					return (double)a - (double)(short)b;
				case TypeCode.UInt16:
					return (double)a - (double)unchecked((int)(ushort)b);
				case TypeCode.Int32:
					return (double)a - (double)(int)b;
				case TypeCode.UInt32:
					return (double)a - (double)(uint)b;
				case TypeCode.Int64:
					return (double)a - (double)(long)b;
				case TypeCode.UInt64:
					return (double)a - (double)(ulong)b;
				case TypeCode.Single:
					return (double)a - (double)(float)b;
				case TypeCode.Double:
					return (double)a - (double)b;
				case TypeCode.Decimal:
					return Convert.ToDecimal(a) - (decimal)b;
				}
				break;
			case TypeCode.Decimal:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'decimal' and 'bool'");
				case TypeCode.SByte:
					return (decimal)a - (decimal)(sbyte)b;
				case TypeCode.Int16:
					return (decimal)a - (decimal)(short)b;
				case TypeCode.UInt16:
					return (decimal)a - (decimal)(ushort)b;
				case TypeCode.Int32:
					return (decimal)a - (decimal)(int)b;
				case TypeCode.UInt32:
					return (decimal)a - (decimal)(uint)b;
				case TypeCode.Int64:
					return (decimal)a - (decimal)(long)b;
				case TypeCode.UInt64:
					return (decimal)a - (decimal)(ulong)b;
				case TypeCode.Single:
					return (decimal)a - Convert.ToDecimal(b);
				case TypeCode.Double:
					return (decimal)a - Convert.ToDecimal(b);
				case TypeCode.Decimal:
					return (decimal)a - (decimal)b;
				}
				break;
			}
			return null;
		}
	}

	public static object Multiply(object a, object b, EvaluateOptions options)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (options.HasFlag(EvaluateOptions.BooleanCalculation))
		{
			a = ConvertIfBoolean(a);
			b = ConvertIfBoolean(b);
		}
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		switch (typeCode)
		{
		case TypeCode.Byte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'byte' and 'bool'");
			case TypeCode.SByte:
				return (byte)a * (sbyte)b;
			case TypeCode.Int16:
				return (byte)a * (short)b;
			case TypeCode.UInt16:
				return (byte)a * (ushort)b;
			case TypeCode.Int32:
				return (byte)a * (int)b;
			case TypeCode.UInt32:
				return (byte)a * (uint)b;
			case TypeCode.Int64:
				return (byte)a * (long)b;
			case TypeCode.UInt64:
				return (byte)a * (ulong)b;
			case TypeCode.Single:
				return (float)(int)(byte)a * (float)b;
			case TypeCode.Double:
				return (double)(int)(byte)a * (double)b;
			case TypeCode.Decimal:
				return (decimal)(byte)a * (decimal)b;
			}
			break;
		case TypeCode.SByte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'bool'");
			case TypeCode.SByte:
				return (sbyte)a * (sbyte)b;
			case TypeCode.Int16:
				return (sbyte)a * (short)b;
			case TypeCode.UInt16:
				return (sbyte)a * (ushort)b;
			case TypeCode.Int32:
				return (sbyte)a * (int)b;
			case TypeCode.UInt32:
				return (sbyte)a * (uint)b;
			case TypeCode.Int64:
				return (sbyte)a * (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'ulong'");
			case TypeCode.Single:
				return (float)(sbyte)a * (float)b;
			case TypeCode.Double:
				return (double)(sbyte)a * (double)b;
			case TypeCode.Decimal:
				return (decimal)(sbyte)a * (decimal)b;
			}
			break;
		case TypeCode.Int16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'bool'");
			case TypeCode.SByte:
				return (short)a * (sbyte)b;
			case TypeCode.Int16:
				return (short)a * (short)b;
			case TypeCode.UInt16:
				return (short)a * (ushort)b;
			case TypeCode.Int32:
				return (short)a * (int)b;
			case TypeCode.UInt32:
				return (short)a * (uint)b;
			case TypeCode.Int64:
				return (short)a * (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'ulong'");
			case TypeCode.Single:
				return (float)(short)a * (float)b;
			case TypeCode.Double:
				return (double)(short)a * (double)b;
			case TypeCode.Decimal:
				return (decimal)(short)a * (decimal)b;
			}
			break;
		case TypeCode.UInt16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ushort' and 'bool'");
			case TypeCode.SByte:
				return (ushort)a * (sbyte)b;
			case TypeCode.Int16:
				return (ushort)a * (short)b;
			case TypeCode.UInt16:
				return (ushort)a * (ushort)b;
			case TypeCode.Int32:
				return (ushort)a * (int)b;
			case TypeCode.UInt32:
				return (ushort)a * (uint)b;
			case TypeCode.Int64:
				return (ushort)a * (long)b;
			case TypeCode.UInt64:
				return (ushort)a * (ulong)b;
			case TypeCode.Single:
				return (float)(int)(ushort)a * (float)b;
			case TypeCode.Double:
				return (double)(int)(ushort)a * (double)b;
			case TypeCode.Decimal:
				return (decimal)(ushort)a * (decimal)b;
			}
			break;
		case TypeCode.Int32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'bool'");
			case TypeCode.SByte:
				return (int)a * (sbyte)b;
			case TypeCode.Int16:
				return (int)a * (short)b;
			case TypeCode.UInt16:
				return (int)a * (ushort)b;
			case TypeCode.Int32:
				return (int)a * (int)b;
			case TypeCode.UInt32:
				return (int)a * (uint)b;
			case TypeCode.Int64:
				return (int)a * (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'ulong'");
			case TypeCode.Single:
				return (float)(int)a * (float)b;
			case TypeCode.Double:
				return (double)(int)a * (double)b;
			case TypeCode.Decimal:
				return (decimal)(int)a * (decimal)b;
			}
			break;
		case TypeCode.UInt32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'uint' and 'bool'");
			case TypeCode.SByte:
				return (uint)a * (sbyte)b;
			case TypeCode.Int16:
				return (uint)a * (short)b;
			case TypeCode.UInt16:
				return (uint)a * (ushort)b;
			case TypeCode.Int32:
				return (uint)a * (int)b;
			case TypeCode.UInt32:
				return (uint)a * (uint)b;
			case TypeCode.Int64:
				return (uint)a * (long)b;
			case TypeCode.UInt64:
				return (uint)a * (ulong)b;
			case TypeCode.Single:
				return (float)(uint)a * (float)b;
			case TypeCode.Double:
				return (double)(uint)a * (double)b;
			case TypeCode.Decimal:
				return (decimal)(uint)a * (decimal)b;
			}
			break;
		case TypeCode.Int64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'bool'");
			case TypeCode.SByte:
				return (long)a * (sbyte)b;
			case TypeCode.Int16:
				return (long)a * (short)b;
			case TypeCode.UInt16:
				return (long)a * (ushort)b;
			case TypeCode.Int32:
				return (long)a * (int)b;
			case TypeCode.UInt32:
				return (long)a * (uint)b;
			case TypeCode.Int64:
				return (long)a * (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'ulong'");
			case TypeCode.Single:
				return (float)(long)a * (float)b;
			case TypeCode.Double:
				return (double)(long)a * (double)b;
			case TypeCode.Decimal:
				return (decimal)(long)a * (decimal)b;
			}
			break;
		case TypeCode.UInt64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'bool'");
			case TypeCode.SByte:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'sbyte'");
			case TypeCode.Int16:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'short'");
			case TypeCode.UInt16:
				return (ulong)a * (ushort)b;
			case TypeCode.Int32:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'int'");
			case TypeCode.UInt32:
				return (ulong)a * (uint)b;
			case TypeCode.Int64:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'long'");
			case TypeCode.UInt64:
				return (ulong)a * (ulong)b;
			case TypeCode.Single:
				return (float)(ulong)a * (float)b;
			case TypeCode.Double:
				return (double)(ulong)a * (double)b;
			case TypeCode.Decimal:
				return (decimal)(ulong)a * (decimal)b;
			}
			break;
		case TypeCode.Single:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'float' and 'bool'");
			case TypeCode.SByte:
				return (float)a * (float)(sbyte)b;
			case TypeCode.Int16:
				return (float)a * (float)(short)b;
			case TypeCode.UInt16:
				return (float)a * (float)(int)(ushort)b;
			case TypeCode.Int32:
				return (float)a * (float)(int)b;
			case TypeCode.UInt32:
				return (float)a * (float)(uint)b;
			case TypeCode.Int64:
				return (float)a * (float)(long)b;
			case TypeCode.UInt64:
				return (float)a * (float)(ulong)b;
			case TypeCode.Single:
				return (float)a * (float)b;
			case TypeCode.Double:
				return (double)(float)a * (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) * (decimal)b;
			}
			break;
		case TypeCode.Double:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'double' and 'bool'");
			case TypeCode.SByte:
				return (double)a * (double)(sbyte)b;
			case TypeCode.Int16:
				return (double)a * (double)(short)b;
			case TypeCode.UInt16:
				return (double)a * (double)(int)(ushort)b;
			case TypeCode.Int32:
				return (double)a * (double)(int)b;
			case TypeCode.UInt32:
				return (double)a * (double)(uint)b;
			case TypeCode.Int64:
				return (double)a * (double)(long)b;
			case TypeCode.UInt64:
				return (double)a * (double)(ulong)b;
			case TypeCode.Single:
				return (double)a * (double)(float)b;
			case TypeCode.Double:
				return (double)a * (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) * (decimal)b;
			}
			break;
		case TypeCode.Decimal:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'decimal' and 'bool'");
			case TypeCode.SByte:
				return (decimal)a * (decimal)(sbyte)b;
			case TypeCode.Int16:
				return (decimal)a * (decimal)(short)b;
			case TypeCode.UInt16:
				return (decimal)a * (decimal)(ushort)b;
			case TypeCode.Int32:
				return (decimal)a * (decimal)(int)b;
			case TypeCode.UInt32:
				return (decimal)a * (decimal)(uint)b;
			case TypeCode.Int64:
				return (decimal)a * (decimal)(long)b;
			case TypeCode.UInt64:
				return (decimal)a * (decimal)(ulong)b;
			case TypeCode.Single:
				return (decimal)a * Convert.ToDecimal(b);
			case TypeCode.Double:
				return (decimal)a * Convert.ToDecimal(b);
			case TypeCode.Decimal:
				return (decimal)a * (decimal)b;
			}
			break;
		}
		return null;
	}

	public static object MultiplyChecked(object a, object b, EvaluateOptions options)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (options.HasFlag(EvaluateOptions.BooleanCalculation))
		{
			a = ConvertIfBoolean(a);
			b = ConvertIfBoolean(b);
		}
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		checked
		{
			switch (typeCode)
			{
			case TypeCode.Byte:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'byte' and 'bool'");
				case TypeCode.SByte:
					return (byte)a * (sbyte)b;
				case TypeCode.Int16:
					return (byte)a * (short)b;
				case TypeCode.UInt16:
					return (byte)a * (ushort)b;
				case TypeCode.Int32:
					return (byte)a * (int)b;
				case TypeCode.UInt32:
					return (byte)a * (uint)b;
				case TypeCode.Int64:
					return (byte)a * (long)b;
				case TypeCode.UInt64:
					return (byte)a * (ulong)b;
				case TypeCode.Single:
					return (float)unchecked((int)(byte)a) * (float)b;
				case TypeCode.Double:
					return (double)unchecked((int)(byte)a) * (double)b;
				case TypeCode.Decimal:
					return (decimal)(byte)a * (decimal)b;
				}
				break;
			case TypeCode.SByte:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'bool'");
				case TypeCode.SByte:
					return (sbyte)a * (sbyte)b;
				case TypeCode.Int16:
					return (sbyte)a * (short)b;
				case TypeCode.UInt16:
					return (sbyte)a * (ushort)b;
				case TypeCode.Int32:
					return (sbyte)a * (int)b;
				case TypeCode.UInt32:
					return (sbyte)a * (uint)b;
				case TypeCode.Int64:
					return (sbyte)a * (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'ulong'");
				case TypeCode.Single:
					return (float)(sbyte)a * (float)b;
				case TypeCode.Double:
					return (double)(sbyte)a * (double)b;
				case TypeCode.Decimal:
					return (decimal)(sbyte)a * (decimal)b;
				}
				break;
			case TypeCode.Int16:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'bool'");
				case TypeCode.SByte:
					return (short)a * (sbyte)b;
				case TypeCode.Int16:
					return (short)a * (short)b;
				case TypeCode.UInt16:
					return (short)a * (ushort)b;
				case TypeCode.Int32:
					return (short)a * (int)b;
				case TypeCode.UInt32:
					return (short)a * (uint)b;
				case TypeCode.Int64:
					return (short)a * (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'ulong'");
				case TypeCode.Single:
					return (float)(short)a * (float)b;
				case TypeCode.Double:
					return (double)(short)a * (double)b;
				case TypeCode.Decimal:
					return (decimal)(short)a * (decimal)b;
				}
				break;
			case TypeCode.UInt16:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ushort' and 'bool'");
				case TypeCode.SByte:
					return (ushort)a * (sbyte)b;
				case TypeCode.Int16:
					return (ushort)a * (short)b;
				case TypeCode.UInt16:
					return (ushort)a * (ushort)b;
				case TypeCode.Int32:
					return (ushort)a * (int)b;
				case TypeCode.UInt32:
					return (ushort)a * (uint)b;
				case TypeCode.Int64:
					return (ushort)a * (long)b;
				case TypeCode.UInt64:
					return (ushort)a * (ulong)b;
				case TypeCode.Single:
					return (float)unchecked((int)(ushort)a) * (float)b;
				case TypeCode.Double:
					return (double)unchecked((int)(ushort)a) * (double)b;
				case TypeCode.Decimal:
					return (decimal)(ushort)a * (decimal)b;
				}
				break;
			case TypeCode.Int32:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'bool'");
				case TypeCode.SByte:
					return (int)a * (sbyte)b;
				case TypeCode.Int16:
					return (int)a * (short)b;
				case TypeCode.UInt16:
					return (int)a * (ushort)b;
				case TypeCode.Int32:
					return (int)a * (int)b;
				case TypeCode.UInt32:
					return (int)a * (uint)b;
				case TypeCode.Int64:
					return (int)a * (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'ulong'");
				case TypeCode.Single:
					return (float)(int)a * (float)b;
				case TypeCode.Double:
					return (double)(int)a * (double)b;
				case TypeCode.Decimal:
					return (decimal)(int)a * (decimal)b;
				}
				break;
			case TypeCode.UInt32:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'uint' and 'bool'");
				case TypeCode.SByte:
					return (uint)a * (sbyte)b;
				case TypeCode.Int16:
					return (uint)a * (short)b;
				case TypeCode.UInt16:
					return (uint)a * (ushort)b;
				case TypeCode.Int32:
					return (uint)a * (int)b;
				case TypeCode.UInt32:
					return (uint)a * (uint)b;
				case TypeCode.Int64:
					return (uint)a * (long)b;
				case TypeCode.UInt64:
					return (uint)a * (ulong)b;
				case TypeCode.Single:
					return (float)(uint)a * (float)b;
				case TypeCode.Double:
					return (double)(uint)a * (double)b;
				case TypeCode.Decimal:
					return (decimal)(uint)a * (decimal)b;
				}
				break;
			case TypeCode.Int64:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'bool'");
				case TypeCode.SByte:
					return (long)a * (sbyte)b;
				case TypeCode.Int16:
					return (long)a * (short)b;
				case TypeCode.UInt16:
					return (long)a * (ushort)b;
				case TypeCode.Int32:
					return (long)a * (int)b;
				case TypeCode.UInt32:
					return (long)a * (uint)b;
				case TypeCode.Int64:
					return (long)a * (long)b;
				case TypeCode.UInt64:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'ulong'");
				case TypeCode.Single:
					return (float)(long)a * (float)b;
				case TypeCode.Double:
					return (double)(long)a * (double)b;
				case TypeCode.Decimal:
					return (decimal)(long)a * (decimal)b;
				}
				break;
			case TypeCode.UInt64:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'bool'");
				case TypeCode.SByte:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'sbyte'");
				case TypeCode.Int16:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'short'");
				case TypeCode.UInt16:
					return (ulong)a * (ushort)b;
				case TypeCode.Int32:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'int'");
				case TypeCode.UInt32:
					return (ulong)a * (uint)b;
				case TypeCode.Int64:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'long'");
				case TypeCode.UInt64:
					return (ulong)a * (ulong)b;
				case TypeCode.Single:
					return (float)(ulong)a * (float)b;
				case TypeCode.Double:
					return (double)(ulong)a * (double)b;
				case TypeCode.Decimal:
					return (decimal)(ulong)a * (decimal)b;
				}
				break;
			case TypeCode.Single:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'float' and 'bool'");
				case TypeCode.SByte:
					return (float)a * (float)(sbyte)b;
				case TypeCode.Int16:
					return (float)a * (float)(short)b;
				case TypeCode.UInt16:
					return (float)a * (float)unchecked((int)(ushort)b);
				case TypeCode.Int32:
					return (float)a * (float)(int)b;
				case TypeCode.UInt32:
					return (float)a * (float)(uint)b;
				case TypeCode.Int64:
					return (float)a * (float)(long)b;
				case TypeCode.UInt64:
					return (float)a * (float)(ulong)b;
				case TypeCode.Single:
					return (float)a * (float)b;
				case TypeCode.Double:
					return (double)(float)a * (double)b;
				case TypeCode.Decimal:
					return Convert.ToDecimal(a) * (decimal)b;
				}
				break;
			case TypeCode.Double:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'double' and 'bool'");
				case TypeCode.SByte:
					return (double)a * (double)(sbyte)b;
				case TypeCode.Int16:
					return (double)a * (double)(short)b;
				case TypeCode.UInt16:
					return (double)a * (double)unchecked((int)(ushort)b);
				case TypeCode.Int32:
					return (double)a * (double)(int)b;
				case TypeCode.UInt32:
					return (double)a * (double)(uint)b;
				case TypeCode.Int64:
					return (double)a * (double)(long)b;
				case TypeCode.UInt64:
					return (double)a * (double)(ulong)b;
				case TypeCode.Single:
					return (double)a * (double)(float)b;
				case TypeCode.Double:
					return (double)a * (double)b;
				case TypeCode.Decimal:
					return Convert.ToDecimal(a) * (decimal)b;
				}
				break;
			case TypeCode.Decimal:
				switch (typeCode2)
				{
				case TypeCode.Boolean:
					throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'decimal' and 'bool'");
				case TypeCode.SByte:
					return (decimal)a * (decimal)(sbyte)b;
				case TypeCode.Int16:
					return (decimal)a * (decimal)(short)b;
				case TypeCode.UInt16:
					return (decimal)a * (decimal)(ushort)b;
				case TypeCode.Int32:
					return (decimal)a * (decimal)(int)b;
				case TypeCode.UInt32:
					return (decimal)a * (decimal)(uint)b;
				case TypeCode.Int64:
					return (decimal)a * (decimal)(long)b;
				case TypeCode.UInt64:
					return (decimal)a * (decimal)(ulong)b;
				case TypeCode.Single:
					return (decimal)a * Convert.ToDecimal(b);
				case TypeCode.Double:
					return (decimal)a * Convert.ToDecimal(b);
				case TypeCode.Decimal:
					return (decimal)a * (decimal)b;
				}
				break;
			}
			return null;
		}
	}

	public static object Divide(object a, object b, EvaluateOptions options)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (options.HasFlag(EvaluateOptions.BooleanCalculation))
		{
			a = ConvertIfBoolean(a);
			b = ConvertIfBoolean(b);
		}
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		switch (typeCode)
		{
		case TypeCode.Byte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'byte' and 'bool'");
			case TypeCode.SByte:
				return (byte)a / (sbyte)b;
			case TypeCode.Int16:
				return (byte)a / (short)b;
			case TypeCode.UInt16:
				return (byte)a / (ushort)b;
			case TypeCode.Int32:
				return (byte)a / (int)b;
			case TypeCode.UInt32:
				return (byte)a / (uint)b;
			case TypeCode.Int64:
				return (byte)a / (long)b;
			case TypeCode.UInt64:
				return (byte)a / (ulong)b;
			case TypeCode.Single:
				return (float)(int)(byte)a / (float)b;
			case TypeCode.Double:
				return (double)(int)(byte)a / (double)b;
			case TypeCode.Decimal:
				return (decimal)(byte)a / (decimal)b;
			}
			break;
		case TypeCode.SByte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'sbyte' and 'bool'");
			case TypeCode.SByte:
				return (sbyte)a / (sbyte)b;
			case TypeCode.Int16:
				return (sbyte)a / (short)b;
			case TypeCode.UInt16:
				return (sbyte)a / (ushort)b;
			case TypeCode.Int32:
				return (sbyte)a / (int)b;
			case TypeCode.UInt32:
				return (sbyte)a / (uint)b;
			case TypeCode.Int64:
				return (sbyte)a / (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'sbyte' and 'ulong'");
			case TypeCode.Single:
				return (float)(sbyte)a / (float)b;
			case TypeCode.Double:
				return (double)(sbyte)a / (double)b;
			case TypeCode.Decimal:
				return (decimal)(sbyte)a / (decimal)b;
			}
			break;
		case TypeCode.Int16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'short' and 'bool'");
			case TypeCode.SByte:
				return (short)a / (sbyte)b;
			case TypeCode.Int16:
				return (short)a / (short)b;
			case TypeCode.UInt16:
				return (short)a / (ushort)b;
			case TypeCode.Int32:
				return (short)a / (int)b;
			case TypeCode.UInt32:
				return (short)a / (uint)b;
			case TypeCode.Int64:
				return (short)a / (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'short' and 'ulong'");
			case TypeCode.Single:
				return (float)(short)a / (float)b;
			case TypeCode.Double:
				return (double)(short)a / (double)b;
			case TypeCode.Decimal:
				return (decimal)(short)a / (decimal)b;
			}
			break;
		case TypeCode.UInt16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ushort' and 'bool'");
			case TypeCode.SByte:
				return (ushort)a / (sbyte)b;
			case TypeCode.Int16:
				return (ushort)a / (short)b;
			case TypeCode.UInt16:
				return (ushort)a / (ushort)b;
			case TypeCode.Int32:
				return (ushort)a / (int)b;
			case TypeCode.UInt32:
				return (ushort)a / (uint)b;
			case TypeCode.Int64:
				return (ushort)a / (long)b;
			case TypeCode.UInt64:
				return (ushort)a / (ulong)b;
			case TypeCode.Single:
				return (float)(int)(ushort)a / (float)b;
			case TypeCode.Double:
				return (double)(int)(ushort)a / (double)b;
			case TypeCode.Decimal:
				return (decimal)(ushort)a / (decimal)b;
			}
			break;
		case TypeCode.Int32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'int' and 'bool'");
			case TypeCode.SByte:
				return (int)a / (sbyte)b;
			case TypeCode.Int16:
				return (int)a / (short)b;
			case TypeCode.UInt16:
				return (int)a / (ushort)b;
			case TypeCode.Int32:
				return (int)a / (int)b;
			case TypeCode.UInt32:
				return (int)a / (uint)b;
			case TypeCode.Int64:
				return (int)a / (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'int' and 'ulong'");
			case TypeCode.Single:
				return (float)(int)a / (float)b;
			case TypeCode.Double:
				return (double)(int)a / (double)b;
			case TypeCode.Decimal:
				return (decimal)(int)a / (decimal)b;
			}
			break;
		case TypeCode.UInt32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'uint' and 'bool'");
			case TypeCode.SByte:
				return (uint)a / (sbyte)b;
			case TypeCode.Int16:
				return (uint)a / (short)b;
			case TypeCode.UInt16:
				return (uint)a / (ushort)b;
			case TypeCode.Int32:
				return (uint)a / (int)b;
			case TypeCode.UInt32:
				return (uint)a / (uint)b;
			case TypeCode.Int64:
				return (uint)a / (long)b;
			case TypeCode.UInt64:
				return (uint)a / (ulong)b;
			case TypeCode.Single:
				return (float)(uint)a / (float)b;
			case TypeCode.Double:
				return (double)(uint)a / (double)b;
			case TypeCode.Decimal:
				return (decimal)(uint)a / (decimal)b;
			}
			break;
		case TypeCode.Int64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'long' and 'bool'");
			case TypeCode.SByte:
				return (long)a / (sbyte)b;
			case TypeCode.Int16:
				return (long)a / (short)b;
			case TypeCode.UInt16:
				return (long)a / (ushort)b;
			case TypeCode.Int32:
				return (long)a / (int)b;
			case TypeCode.UInt32:
				return (long)a / (uint)b;
			case TypeCode.Int64:
				return (long)a / (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'long' and 'ulong'");
			case TypeCode.Single:
				return (float)(long)a / (float)b;
			case TypeCode.Double:
				return (double)(long)a / (double)b;
			case TypeCode.Decimal:
				return (decimal)(long)a / (decimal)b;
			}
			break;
		case TypeCode.UInt64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'bool'");
			case TypeCode.SByte:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'sbyte'");
			case TypeCode.Int16:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'short'");
			case TypeCode.UInt16:
				return (ulong)a / (ushort)b;
			case TypeCode.Int32:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'int'");
			case TypeCode.UInt32:
				return (ulong)a / (uint)b;
			case TypeCode.Int64:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'long'");
			case TypeCode.UInt64:
				return (ulong)a / (ulong)b;
			case TypeCode.Single:
				return (float)(ulong)a / (float)b;
			case TypeCode.Double:
				return (double)(ulong)a / (double)b;
			case TypeCode.Decimal:
				return (decimal)(ulong)a / (decimal)b;
			}
			break;
		case TypeCode.Single:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'float' and 'bool'");
			case TypeCode.SByte:
				return (float)a / (float)(sbyte)b;
			case TypeCode.Int16:
				return (float)a / (float)(short)b;
			case TypeCode.UInt16:
				return (float)a / (float)(int)(ushort)b;
			case TypeCode.Int32:
				return (float)a / (float)(int)b;
			case TypeCode.UInt32:
				return (float)a / (float)(uint)b;
			case TypeCode.Int64:
				return (float)a / (float)(long)b;
			case TypeCode.UInt64:
				return (float)a / (float)(ulong)b;
			case TypeCode.Single:
				return (float)a / (float)b;
			case TypeCode.Double:
				return (double)(float)a / (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) / (decimal)b;
			}
			break;
		case TypeCode.Double:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'double' and 'bool'");
			case TypeCode.SByte:
				return (double)a / (double)(sbyte)b;
			case TypeCode.Int16:
				return (double)a / (double)(short)b;
			case TypeCode.UInt16:
				return (double)a / (double)(int)(ushort)b;
			case TypeCode.Int32:
				return (double)a / (double)(int)b;
			case TypeCode.UInt32:
				return (double)a / (double)(uint)b;
			case TypeCode.Int64:
				return (double)a / (double)(long)b;
			case TypeCode.UInt64:
				return (double)a / (double)(ulong)b;
			case TypeCode.Single:
				return (double)a / (double)(float)b;
			case TypeCode.Double:
				return (double)a / (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) / (decimal)b;
			}
			break;
		case TypeCode.Decimal:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'decimal' and 'bool'");
			case TypeCode.SByte:
				return (decimal)a / (decimal)(sbyte)b;
			case TypeCode.Int16:
				return (decimal)a / (decimal)(short)b;
			case TypeCode.UInt16:
				return (decimal)a / (decimal)(ushort)b;
			case TypeCode.Int32:
				return (decimal)a / (decimal)(int)b;
			case TypeCode.UInt32:
				return (decimal)a / (decimal)(uint)b;
			case TypeCode.Int64:
				return (decimal)a / (decimal)(long)b;
			case TypeCode.UInt64:
				return (decimal)a / (decimal)(ulong)b;
			case TypeCode.Single:
				return (decimal)a / Convert.ToDecimal(b);
			case TypeCode.Double:
				return (decimal)a / Convert.ToDecimal(b);
			case TypeCode.Decimal:
				return (decimal)a / (decimal)b;
			}
			break;
		}
		return null;
	}

	public static object Modulo(object a, object b)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		switch (typeCode)
		{
		case TypeCode.Byte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'byte' and 'bool'");
			case TypeCode.SByte:
				return (byte)a % (sbyte)b;
			case TypeCode.Int16:
				return (byte)a % (short)b;
			case TypeCode.UInt16:
				return (byte)a % (ushort)b;
			case TypeCode.Int32:
				return (byte)a % (int)b;
			case TypeCode.UInt32:
				return (byte)a % (uint)b;
			case TypeCode.Int64:
				return (byte)a % (long)b;
			case TypeCode.UInt64:
				return (byte)a % (ulong)b;
			case TypeCode.Single:
				return (float)(int)(byte)a % (float)b;
			case TypeCode.Double:
				return (double)(int)(byte)a % (double)b;
			case TypeCode.Decimal:
				return (decimal)(byte)a % (decimal)b;
			}
			break;
		case TypeCode.SByte:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'sbyte' and 'bool'");
			case TypeCode.SByte:
				return (sbyte)a % (sbyte)b;
			case TypeCode.Int16:
				return (sbyte)a % (short)b;
			case TypeCode.UInt16:
				return (sbyte)a % (ushort)b;
			case TypeCode.Int32:
				return (sbyte)a % (int)b;
			case TypeCode.UInt32:
				return (sbyte)a % (uint)b;
			case TypeCode.Int64:
				return (sbyte)a % (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'sbyte' and 'ulong'");
			case TypeCode.Single:
				return (float)(sbyte)a % (float)b;
			case TypeCode.Double:
				return (double)(sbyte)a % (double)b;
			case TypeCode.Decimal:
				return (decimal)(sbyte)a % (decimal)b;
			}
			break;
		case TypeCode.Int16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'short' and 'bool'");
			case TypeCode.SByte:
				return (short)a % (sbyte)b;
			case TypeCode.Int16:
				return (short)a % (short)b;
			case TypeCode.UInt16:
				return (short)a % (ushort)b;
			case TypeCode.Int32:
				return (short)a % (int)b;
			case TypeCode.UInt32:
				return (short)a % (uint)b;
			case TypeCode.Int64:
				return (short)a % (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'short' and 'ulong'");
			case TypeCode.Single:
				return (float)(short)a % (float)b;
			case TypeCode.Double:
				return (double)(short)a % (double)b;
			case TypeCode.Decimal:
				return (decimal)(short)a % (decimal)b;
			}
			break;
		case TypeCode.UInt16:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ushort' and 'bool'");
			case TypeCode.SByte:
				return (ushort)a % (sbyte)b;
			case TypeCode.Int16:
				return (ushort)a % (short)b;
			case TypeCode.UInt16:
				return (ushort)a % (ushort)b;
			case TypeCode.Int32:
				return (ushort)a % (int)b;
			case TypeCode.UInt32:
				return (ushort)a % (uint)b;
			case TypeCode.Int64:
				return (ushort)a % (long)b;
			case TypeCode.UInt64:
				return (ushort)a % (ulong)b;
			case TypeCode.Single:
				return (float)(int)(ushort)a % (float)b;
			case TypeCode.Double:
				return (double)(int)(ushort)a % (double)b;
			case TypeCode.Decimal:
				return (decimal)(ushort)a % (decimal)b;
			}
			break;
		case TypeCode.Int32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'int' and 'bool'");
			case TypeCode.SByte:
				return (int)a % (sbyte)b;
			case TypeCode.Int16:
				return (int)a % (short)b;
			case TypeCode.UInt16:
				return (int)a % (ushort)b;
			case TypeCode.Int32:
				return (int)a % (int)b;
			case TypeCode.UInt32:
				return (int)a % (uint)b;
			case TypeCode.Int64:
				return (int)a % (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'int' and 'ulong'");
			case TypeCode.Single:
				return (float)(int)a % (float)b;
			case TypeCode.Double:
				return (double)(int)a % (double)b;
			case TypeCode.Decimal:
				return (decimal)(int)a % (decimal)b;
			}
			break;
		case TypeCode.UInt32:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'uint' and 'bool'");
			case TypeCode.SByte:
				return (uint)a % (sbyte)b;
			case TypeCode.Int16:
				return (uint)a % (short)b;
			case TypeCode.UInt16:
				return (uint)a % (ushort)b;
			case TypeCode.Int32:
				return (uint)a % (int)b;
			case TypeCode.UInt32:
				return (uint)a % (uint)b;
			case TypeCode.Int64:
				return (uint)a % (long)b;
			case TypeCode.UInt64:
				return (uint)a % (ulong)b;
			case TypeCode.Single:
				return (float)(uint)a % (float)b;
			case TypeCode.Double:
				return (double)(uint)a % (double)b;
			case TypeCode.Decimal:
				return (decimal)(uint)a % (decimal)b;
			}
			break;
		case TypeCode.Int64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'long' and 'bool'");
			case TypeCode.SByte:
				return (long)a % (sbyte)b;
			case TypeCode.Int16:
				return (long)a % (short)b;
			case TypeCode.UInt16:
				return (long)a % (ushort)b;
			case TypeCode.Int32:
				return (long)a % (int)b;
			case TypeCode.UInt32:
				return (long)a % (uint)b;
			case TypeCode.Int64:
				return (long)a % (long)b;
			case TypeCode.UInt64:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'long' and 'ulong'");
			case TypeCode.Single:
				return (float)(long)a % (float)b;
			case TypeCode.Double:
				return (double)(long)a % (double)b;
			case TypeCode.Decimal:
				return (decimal)(long)a % (decimal)b;
			}
			break;
		case TypeCode.UInt64:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'bool'");
			case TypeCode.SByte:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'sbyte'");
			case TypeCode.Int16:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'short'");
			case TypeCode.UInt16:
				return (ulong)a % (ushort)b;
			case TypeCode.Int32:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'int'");
			case TypeCode.UInt32:
				return (ulong)a % (uint)b;
			case TypeCode.Int64:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'long'");
			case TypeCode.UInt64:
				return (ulong)a % (ulong)b;
			case TypeCode.Single:
				return (float)(ulong)a % (float)b;
			case TypeCode.Double:
				return (double)(ulong)a % (double)b;
			case TypeCode.Decimal:
				return (decimal)(ulong)a % (decimal)b;
			}
			break;
		case TypeCode.Single:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'float' and 'bool'");
			case TypeCode.SByte:
				return (float)a % (float)(sbyte)b;
			case TypeCode.Int16:
				return (float)a % (float)(short)b;
			case TypeCode.UInt16:
				return (float)a % (float)(int)(ushort)b;
			case TypeCode.Int32:
				return (float)a % (float)(int)b;
			case TypeCode.UInt32:
				return (float)a % (float)(uint)b;
			case TypeCode.Int64:
				return (float)a % (float)(long)b;
			case TypeCode.UInt64:
				return (float)a % (float)(ulong)b;
			case TypeCode.Single:
				return (float)a % (float)b;
			case TypeCode.Double:
				return (double)(float)a % (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) % (decimal)b;
			}
			break;
		case TypeCode.Double:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'double' and 'bool'");
			case TypeCode.SByte:
				return (double)a % (double)(sbyte)b;
			case TypeCode.Int16:
				return (double)a % (double)(short)b;
			case TypeCode.UInt16:
				return (double)a % (double)(int)(ushort)b;
			case TypeCode.Int32:
				return (double)a % (double)(int)b;
			case TypeCode.UInt32:
				return (double)a % (double)(uint)b;
			case TypeCode.Int64:
				return (double)a % (double)(long)b;
			case TypeCode.UInt64:
				return (double)a % (double)(ulong)b;
			case TypeCode.Single:
				return (double)a % (double)(float)b;
			case TypeCode.Double:
				return (double)a % (double)b;
			case TypeCode.Decimal:
				return Convert.ToDecimal(a) % (decimal)b;
			}
			break;
		case TypeCode.Decimal:
			switch (typeCode2)
			{
			case TypeCode.Boolean:
				throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'decimal' and 'bool'");
			case TypeCode.SByte:
				return (decimal)a % (decimal)(sbyte)b;
			case TypeCode.Int16:
				return (decimal)a % (decimal)(short)b;
			case TypeCode.UInt16:
				return (decimal)a % (decimal)(ushort)b;
			case TypeCode.Int32:
				return (decimal)a % (decimal)(int)b;
			case TypeCode.UInt32:
				return (decimal)a % (decimal)(uint)b;
			case TypeCode.Int64:
				return (decimal)a % (decimal)(long)b;
			case TypeCode.UInt64:
				return (decimal)a % (decimal)(ulong)b;
			case TypeCode.Single:
				return (decimal)a % Convert.ToDecimal(b);
			case TypeCode.Double:
				return (decimal)a % Convert.ToDecimal(b);
			case TypeCode.Decimal:
				return (decimal)a % (decimal)b;
			}
			break;
		}
		return null;
	}

	public static object Max(object a, object b)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (a == null && b == null)
		{
			return null;
		}
		if (a == null)
		{
			return b;
		}
		if (b == null)
		{
			return a;
		}
		return ConvertToHighestPrecision(ref a, ref b) switch
		{
			TypeCode.Byte => (object)Math.Max((byte)a, (byte)b),
			TypeCode.SByte => Math.Max((sbyte)a, (sbyte)b),
			TypeCode.Int16 => Math.Max((short)a, (short)b),
			TypeCode.UInt16 => Math.Max((ushort)a, (ushort)b),
			TypeCode.Int32 => Math.Max((int)a, (int)b),
			TypeCode.UInt32 => Math.Max((uint)a, (uint)b),
			TypeCode.Int64 => Math.Max((long)a, (long)b),
			TypeCode.UInt64 => Math.Max((ulong)a, (ulong)b),
			TypeCode.Single => Math.Max((float)a, (float)b),
			TypeCode.Double => Math.Max((double)a, (double)b),
			TypeCode.Decimal => Math.Max((decimal)a, (decimal)b),
			_ => null,
		};
	}

	public static object Min(object a, object b)
	{
		a = ConvertIfString(a);
		b = ConvertIfString(b);
		if (a == null && b == null)
		{
			return null;
		}
		if (a == null)
		{
			return b;
		}
		if (b == null)
		{
			return a;
		}
		return ConvertToHighestPrecision(ref a, ref b) switch
		{
			TypeCode.Byte => (object)Math.Min((byte)a, (byte)b),
			TypeCode.SByte => Math.Min((sbyte)a, (sbyte)b),
			TypeCode.Int16 => Math.Min((short)a, (short)b),
			TypeCode.UInt16 => Math.Min((ushort)a, (ushort)b),
			TypeCode.Int32 => Math.Min((int)a, (int)b),
			TypeCode.UInt32 => Math.Min((uint)a, (uint)b),
			TypeCode.Int64 => Math.Min((long)a, (long)b),
			TypeCode.UInt64 => Math.Min((ulong)a, (ulong)b),
			TypeCode.Single => Math.Min((float)a, (float)b),
			TypeCode.Double => Math.Min((double)a, (double)b),
			TypeCode.Decimal => Math.Min((decimal)a, (decimal)b),
			_ => null,
		};
	}

	private static TypeCode ConvertToHighestPrecision(ref object a, ref object b)
	{
		TypeCode typeCode = a.GetTypeCode();
		TypeCode typeCode2 = b.GetTypeCode();
		if (typeCode == typeCode2)
		{
			return typeCode;
		}
		int? num = TypeCodeBitSize(typeCode, out var floatingPoint);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			num = TypeCodeBitSize(typeCode2, out var floatingPoint2);
			if (num.HasValue)
			{
				int valueOrDefault2 = num.GetValueOrDefault();
				if (floatingPoint != floatingPoint2)
				{
					if (floatingPoint)
					{
						b = ConvertTo(b, typeCode);
						return typeCode;
					}
					a = ConvertTo(a, typeCode2);
					return typeCode2;
				}
				if (valueOrDefault > valueOrDefault2)
				{
					b = ConvertTo(b, typeCode);
					return typeCode;
				}
				a = ConvertTo(a, typeCode2);
				return typeCode2;
			}
			return TypeCode.Empty;
		}
		return TypeCode.Empty;
	}

	private static int? TypeCodeBitSize(TypeCode typeCode, out bool floatingPoint)
	{
		floatingPoint = false;
		switch (typeCode)
		{
		case TypeCode.SByte:
			return 8;
		case TypeCode.Byte:
			return 8;
		case TypeCode.Int16:
			return 16;
		case TypeCode.UInt16:
			return 16;
		case TypeCode.Int32:
			return 32;
		case TypeCode.UInt32:
			return 32;
		case TypeCode.Int64:
			return 64;
		case TypeCode.UInt64:
			return 64;
		case TypeCode.Single:
			floatingPoint = true;
			return 32;
		case TypeCode.Double:
			floatingPoint = true;
			return 64;
		case TypeCode.Decimal:
			floatingPoint = true;
			return 128;
		default:
			return null;
		}
	}

	private static object ConvertTo(object value, TypeCode toType)
	{
		return toType switch
		{
			TypeCode.Byte => (object)Convert.ToByte(value),
			TypeCode.SByte => Convert.ToSByte(value),
			TypeCode.Int16 => Convert.ToInt16(value),
			TypeCode.UInt16 => Convert.ToUInt16(value),
			TypeCode.Int32 => Convert.ToInt32(value),
			TypeCode.UInt32 => Convert.ToUInt32(value),
			TypeCode.Int64 => Convert.ToInt64(value),
			TypeCode.UInt64 => Convert.ToUInt64(value),
			TypeCode.Single => Convert.ToSingle(value),
			TypeCode.Double => Convert.ToDouble(value),
			TypeCode.Decimal => Convert.ToDecimal(value),
			_ => null,
		};
	}
}
