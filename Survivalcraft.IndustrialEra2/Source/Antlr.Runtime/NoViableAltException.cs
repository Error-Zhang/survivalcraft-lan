using System;
using System.Runtime.Serialization;

namespace Antlr.Runtime;

[Serializable]
public class NoViableAltException : RecognitionException
{
	public int DecisionNumber { get; }

	public string GrammarDecisionDescription { get; }

	public int StateNumber { get; }

	public NoViableAltException()
	{
	}

	public NoViableAltException(string grammarDecisionDescription)
	{
		GrammarDecisionDescription = grammarDecisionDescription;
	}

	public NoViableAltException(string message, string grammarDecisionDescription)
		: base(message)
	{
		GrammarDecisionDescription = grammarDecisionDescription;
	}

	public NoViableAltException(string message, string grammarDecisionDescription, Exception innerException)
		: base(message, innerException)
	{
		GrammarDecisionDescription = grammarDecisionDescription;
	}

	public NoViableAltException(string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input)
		: base(input)
	{
		GrammarDecisionDescription = grammarDecisionDescription;
		DecisionNumber = decisionNumber;
		StateNumber = stateNumber;
	}

	public NoViableAltException(string message, string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input)
		: base(message, input)
	{
		GrammarDecisionDescription = grammarDecisionDescription;
		DecisionNumber = decisionNumber;
		StateNumber = stateNumber;
	}

	public NoViableAltException(string message, string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input, int k)
		: base(message, input)
	{
		GrammarDecisionDescription = grammarDecisionDescription;
		DecisionNumber = decisionNumber;
		StateNumber = stateNumber;
	}

	public NoViableAltException(string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input, int k)
		: base(input)
	{
		GrammarDecisionDescription = grammarDecisionDescription;
		DecisionNumber = decisionNumber;
		StateNumber = stateNumber;
	}

	public NoViableAltException(string message, string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input, Exception innerException)
		: base(message, input, innerException)
	{
		GrammarDecisionDescription = grammarDecisionDescription;
		DecisionNumber = decisionNumber;
		StateNumber = stateNumber;
	}

	protected NoViableAltException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		GrammarDecisionDescription = info.GetString("GrammarDecisionDescription");
		DecisionNumber = info.GetInt32("DecisionNumber");
		StateNumber = info.GetInt32("StateNumber");
	}

	public new void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("GrammarDecisionDescription", GrammarDecisionDescription);
		info.AddValue("DecisionNumber", DecisionNumber);
		info.AddValue("StateNumber", StateNumber);
	}

	public override string ToString()
	{
		if (Input is ICharStream)
		{
			return "NoViableAltException('" + (char)UnexpectedType + "'@[" + GrammarDecisionDescription + "])";
		}
		return "NoViableAltException(" + UnexpectedType + "@[" + GrammarDecisionDescription + "])";
	}
}
