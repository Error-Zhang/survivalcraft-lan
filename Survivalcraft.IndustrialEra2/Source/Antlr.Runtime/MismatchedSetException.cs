using System;
using System.Runtime.Serialization;

namespace Antlr.Runtime;

[Serializable]
public class MismatchedSetException : RecognitionException
{
	public BitSet Expecting { get; }

	public MismatchedSetException()
	{
	}

	public MismatchedSetException(string message)
		: base(message)
	{
	}

	public MismatchedSetException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public MismatchedSetException(BitSet expecting, IIntStream input)
		: base(input)
	{
		Expecting = expecting;
	}

	public MismatchedSetException(string message, BitSet expecting, IIntStream input)
		: base(message, input)
	{
		Expecting = expecting;
	}

	public MismatchedSetException(string message, BitSet expecting, IIntStream input, Exception innerException)
		: base(message, input, innerException)
	{
		Expecting = expecting;
	}

	protected MismatchedSetException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		Expecting = (BitSet)info.GetValue("Expecting", typeof(BitSet));
	}

	public new void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("Expecting", Expecting);
	}

	public override string ToString()
	{
		return "MismatchedSetException(" + UnexpectedType + "!=" + Expecting?.ToString() + ")";
	}
}
