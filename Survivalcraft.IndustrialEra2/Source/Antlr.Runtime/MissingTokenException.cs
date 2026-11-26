using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Antlr.Runtime;

[Serializable]
public class MissingTokenException : MismatchedTokenException
{
	private readonly object _inserted;

	public virtual int MissingType => Expecting;

	public MissingTokenException()
	{
	}

	public MissingTokenException(string message)
		: base(message)
	{
	}

	public MissingTokenException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public MissingTokenException(int expecting, IIntStream input, object inserted)
		: this(expecting, input, inserted, null)
	{
	}

	public MissingTokenException(int expecting, IIntStream input, object inserted, IList<string> tokenNames)
		: base(expecting, input, tokenNames)
	{
		_inserted = inserted;
	}

	public MissingTokenException(string message, int expecting, IIntStream input, object inserted, IList<string> tokenNames)
		: base(message, expecting, input, tokenNames)
	{
		_inserted = inserted;
	}

	public MissingTokenException(string message, int expecting, IIntStream input, object inserted, IList<string> tokenNames, Exception innerException)
		: base(message, expecting, input, tokenNames, innerException)
	{
		_inserted = inserted;
	}

	protected MissingTokenException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public override string ToString()
	{
		if (_inserted != null && Token != null)
		{
			return "MissingTokenException(inserted " + _inserted?.ToString() + " at " + Token.Text + ")";
		}
		if (Token != null)
		{
			return "MissingTokenException(at " + Token.Text + ")";
		}
		return "MissingTokenException";
	}
}
