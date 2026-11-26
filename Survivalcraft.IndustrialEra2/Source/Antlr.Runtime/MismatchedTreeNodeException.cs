using System;
using System.Runtime.Serialization;
using Antlr.Runtime.Tree;

namespace Antlr.Runtime;

[Serializable]
public class MismatchedTreeNodeException : RecognitionException
{
	public int Expecting { get; }

	public MismatchedTreeNodeException()
	{
	}

	public MismatchedTreeNodeException(string message)
		: base(message)
	{
	}

	public MismatchedTreeNodeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public MismatchedTreeNodeException(int expecting, ITreeNodeStream input)
		: base(input)
	{
		Expecting = expecting;
	}

	public MismatchedTreeNodeException(string message, int expecting, ITreeNodeStream input)
		: base(message, input)
	{
		Expecting = expecting;
	}

	public MismatchedTreeNodeException(string message, int expecting, ITreeNodeStream input, Exception innerException)
		: base(message, input, innerException)
	{
		Expecting = expecting;
	}

	protected MismatchedTreeNodeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		Expecting = info.GetInt32("Expecting");
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
		return "MismatchedTreeNodeException(" + UnexpectedType + "!=" + Expecting + ")";
	}
}
