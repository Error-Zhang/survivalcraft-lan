namespace Antlr.Runtime;

public class AstParserRuleReturnScope<TAstLabel, TToken> : ParserRuleReturnScope<TToken>, IAstRuleReturnScope<TAstLabel>, IAstRuleReturnScope, IRuleReturnScope
{
	public TAstLabel Tree { get; set; }

	object IAstRuleReturnScope.Tree => Tree;
}
