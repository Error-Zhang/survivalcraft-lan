using System;
using System.Runtime.Serialization;

namespace Antlr.Runtime;

[Serializable]
public class MismatchedRangeException : RecognitionException
{
	public int A { get; }

	public int B { get; }

	public MismatchedRangeException()
	{
	}

	public MismatchedRangeException(string message)
		: base(message)
	{
	}

	public MismatchedRangeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public MismatchedRangeException(int a, int b, IIntStream input)
		: base(input)
	{
		A = a;
		B = b;
	}

	public MismatchedRangeException(string message, int a, int b, IIntStream input)
		: base(message, input)
	{
		A = a;
		B = b;
	}

	public MismatchedRangeException(string message, int a, int b, IIntStream input, Exception innerException)
		: base(message, input, innerException)
	{
		A = a;
		B = b;
	}

	protected MismatchedRangeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		A = info.GetInt32("A");
		B = info.GetInt32("B");
	}

	public new void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("A", A);
		info.AddValue("B", B);
	}

	public override string ToString()
	{
		return "MismatchedRangeException(" + UnexpectedType + " not in [" + A + "," + B + "])";
	}
}
