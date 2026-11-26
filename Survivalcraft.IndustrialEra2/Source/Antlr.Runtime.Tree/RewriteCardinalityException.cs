using System;

namespace Antlr.Runtime.Tree;

[Serializable]
public class RewriteCardinalityException : Exception
{
	public RewriteCardinalityException()
	{
	}

	public RewriteCardinalityException(string elementDescription)
		: this(elementDescription, elementDescription)
	{
	}

	public RewriteCardinalityException(string elementDescription, Exception innerException)
		: this(elementDescription, elementDescription, innerException)
	{
	}

	public RewriteCardinalityException(string message, string elementDescription)
		: base(message)
	{
	}

	public RewriteCardinalityException(string message, string elementDescription, Exception innerException)
		: base(message, innerException)
	{
	}
}
