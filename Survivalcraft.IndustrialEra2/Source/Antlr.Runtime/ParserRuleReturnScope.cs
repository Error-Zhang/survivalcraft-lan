namespace Antlr.Runtime;

public class ParserRuleReturnScope<TToken> : IRuleReturnScope<TToken>, IRuleReturnScope
{
	public TToken Start { get; set; }

	public TToken Stop { get; set; }

	object IRuleReturnScope.Start => Start;

	object IRuleReturnScope.Stop => Stop;
}
