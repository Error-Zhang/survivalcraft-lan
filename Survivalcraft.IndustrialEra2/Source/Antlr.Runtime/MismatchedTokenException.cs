using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Antlr.Runtime;

[Serializable]
public class MismatchedTokenException : RecognitionException
{
	public int Expecting { get; }

	public ReadOnlyCollection<string> TokenNames { get; }

	public MismatchedTokenException()
	{
	}

	public MismatchedTokenException(string message)
		: base(message)
	{
	}

	public MismatchedTokenException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public MismatchedTokenException(int expecting, IIntStream input)
		: this(expecting, input, null)
	{
	}

	public MismatchedTokenException(int expecting, IIntStream input, IList<string> tokenNames)
		: base(input)
	{
		Expecting = expecting;
		if (tokenNames != null)
		{
			TokenNames = tokenNames.ToList().AsReadOnly();
		}
	}

	public MismatchedTokenException(string message, int expecting, IIntStream input, IList<string> tokenNames)
		: base(message, input)
	{
		Expecting = expecting;
		if (tokenNames != null)
		{
			TokenNames = tokenNames.ToList().AsReadOnly();
		}
	}

	public MismatchedTokenException(string message, int expecting, IIntStream input, IList<string> tokenNames, Exception innerException)
		: base(message, input, innerException)
	{
		Expecting = expecting;
		if (tokenNames != null)
		{
			TokenNames = tokenNames.ToList().AsReadOnly();
		}
	}

	protected MismatchedTokenException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		Expecting = info.GetInt32("Expecting");
		TokenNames = new ReadOnlyCollection<string>((string[])info.GetValue("TokenNames", typeof(string[])));
	}

	public new void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("Expecting", Expecting);
		info.AddValue("TokenNames", TokenNames?.ToArray());
	}

	public override string ToString()
	{
		int unexpectedType = UnexpectedType;
		string text = ((TokenNames != null && unexpectedType >= 0 && unexpectedType < TokenNames.Count) ? TokenNames[unexpectedType] : unexpectedType.ToString());
		string text2 = ((TokenNames != null && Expecting >= 0 && Expecting < TokenNames.Count) ? TokenNames[Expecting] : Expecting.ToString());
		return "MismatchedTokenException(" + text + "!=" + text2 + ")";
	}
}
