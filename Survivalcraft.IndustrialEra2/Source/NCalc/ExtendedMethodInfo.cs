using System.Linq.Expressions;
using System.Reflection;

namespace NCalc;

public class ExtendedMethodInfo
{
	public MethodInfo BaseMethodInfo { get; set; }

	public System.Linq.Expressions.Expression[] PreparedArguments { get; set; }

	public int Score { get; set; }
}
