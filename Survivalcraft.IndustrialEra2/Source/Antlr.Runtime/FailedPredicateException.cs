using System;
using System.Runtime.Serialization;

namespace Antlr.Runtime;

[Serializable]
public class FailedPredicateException : RecognitionException
{
	public string RuleName { get; }

	public string PredicateText { get; }

	public FailedPredicateException()
	{
	}

	public FailedPredicateException(string message)
		: base(message)
	{
	}

	public FailedPredicateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public FailedPredicateException(IIntStream input, string ruleName, string predicateText)
		: base(input)
	{
		RuleName = ruleName;
		PredicateText = predicateText;
	}

	public FailedPredicateException(string message, IIntStream input, string ruleName, string predicateText)
		: base(message, input)
	{
		RuleName = ruleName;
		PredicateText = predicateText;
	}

	public FailedPredicateException(string message, IIntStream input, string ruleName, string predicateText, Exception innerException)
		: base(message, input, innerException)
	{
		RuleName = ruleName;
		PredicateText = predicateText;
	}

	protected FailedPredicateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		RuleName = info.GetString("RuleName");
		PredicateText = info.GetString("PredicateText");
	}

	public new void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("RuleName", RuleName);
		info.AddValue("PredicateText", PredicateText);
	}

	public override string ToString()
	{
		return "FailedPredicateException(" + RuleName + ",{" + PredicateText + "}?)";
	}
}
