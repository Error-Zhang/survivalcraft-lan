using System;

namespace Antlr.Runtime;

[AttributeUsage(AttributeTargets.Method)]
public sealed class GrammarRuleAttribute : Attribute
{
	public string Name { get; }

	public GrammarRuleAttribute(string name)
	{
		Name = name;
	}
}
