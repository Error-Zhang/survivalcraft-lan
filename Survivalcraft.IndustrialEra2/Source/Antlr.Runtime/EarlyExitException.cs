using System;
using System.Runtime.Serialization;

namespace Antlr.Runtime;

[Serializable]
public class EarlyExitException : RecognitionException
{
	public int DecisionNumber { get; }

	public EarlyExitException()
	{
	}

	public EarlyExitException(string message)
		: base(message)
	{
	}

	public EarlyExitException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public EarlyExitException(int decisionNumber, IIntStream input)
		: base(input)
	{
		DecisionNumber = decisionNumber;
	}

	public EarlyExitException(string message, int decisionNumber, IIntStream input)
		: base(message, input)
	{
		DecisionNumber = decisionNumber;
	}

	public EarlyExitException(string message, int decisionNumber, IIntStream input, Exception innerException)
		: base(message, input, innerException)
	{
		DecisionNumber = decisionNumber;
	}

	protected EarlyExitException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		DecisionNumber = info.GetInt32("DecisionNumber");
	}

	public new void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("DecisionNumber", DecisionNumber);
	}
}
