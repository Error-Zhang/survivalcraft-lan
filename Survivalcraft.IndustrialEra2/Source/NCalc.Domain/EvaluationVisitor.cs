using System;
using System.Collections.Generic;

namespace NCalc.Domain;

public class EvaluationVisitor : LogicalExpressionVisitor
{
	private delegate T Func<T>();

	private readonly EvaluateOptions _options;

	private static readonly Type[] CommonTypes = new Type[5]
	{
		typeof(long),
		typeof(double),
		typeof(bool),
		typeof(string),
		typeof(decimal)
	};

	private bool IgnoreCase => (_options & EvaluateOptions.IgnoreCase) == EvaluateOptions.IgnoreCase;

	private bool Ordinal => (_options & EvaluateOptions.MatchStringsOrdinal) == EvaluateOptions.MatchStringsOrdinal;

	private bool IgnoreCaseString => (_options & EvaluateOptions.MatchStringsWithIgnoreCase) == EvaluateOptions.MatchStringsWithIgnoreCase;

	private bool Checked => (_options & EvaluateOptions.OverflowProtection) == EvaluateOptions.OverflowProtection;

	public object Result { get; private set; }

	public Dictionary<string, object> Parameters { get; set; }

	public event EvaluateFunctionHandler EvaluateFunction;

	public event EvaluateParameterHandler EvaluateParameter;

	public EvaluationVisitor(EvaluateOptions options)
	{
		_options = options;
	}

	private object Evaluate(LogicalExpression expression)
	{
		expression.Accept(this);
		return Result;
	}

	public override void Visit(LogicalExpression expression)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	private static Type GetMostPreciseType(Type a, Type b)
	{
		Type[] commonTypes = CommonTypes;
		foreach (Type type in commonTypes)
		{
			if (a == type || b == type)
			{
				return type;
			}
		}
		return a ?? b;
	}

	public int CompareUsingMostPreciseType(object a, object b)
	{
		Type type = (((_options & EvaluateOptions.AllowNullParameter) == EvaluateOptions.AllowNullParameter) ? (GetMostPreciseType(a?.GetType(), b?.GetType()) ?? typeof(object)) : GetMostPreciseType(a.GetType(), b.GetType()));
		if (a == null && b == null)
		{
			return 0;
		}
		if (a == null || b == null)
		{
			return -1;
		}
		a = Convert.ChangeType(a, type);
		b = Convert.ChangeType(b, type);
		if (type.Equals(typeof(string)) && (Ordinal || IgnoreCaseString))
		{
			if (!Ordinal)
			{
				return StringComparer.CurrentCultureIgnoreCase.Compare(a?.ToString(), b?.ToString());
			}
			if (IgnoreCaseString)
			{
				return StringComparer.OrdinalIgnoreCase.Compare(a?.ToString(), b?.ToString());
			}
			StringComparer.Ordinal.Compare(a?.ToString(), b?.ToString());
		}
		if (a == b)
		{
			return 0;
		}
		if (a is IComparable comparable)
		{
			return comparable.CompareTo(b);
		}
		return -1;
	}

	public override void Visit(TernaryExpression expression)
	{
		expression.LeftExpression.Accept(this);
		if (Convert.ToBoolean(Result))
		{
			expression.MiddleExpression.Accept(this);
		}
		else
		{
			expression.RightExpression.Accept(this);
		}
	}

	private static bool IsReal(object value)
	{
		TypeCode typeCode = value.GetTypeCode();
		if (typeCode != TypeCode.Decimal && typeCode != TypeCode.Double)
		{
			return typeCode == TypeCode.Single;
		}
		return true;
	}

	public override void Visit(BinaryExpression expression)
	{
		object leftValue = null;
		bool leftEvaluated = false;
		Func<object> func = () =>
		{
			if (!leftEvaluated)
			{
				expression.LeftExpression.Accept(this);
				leftValue = Result;
				leftEvaluated = true;
			}
			return leftValue;
		};
		object rightValue = null;
		bool rightEvaluated = false;
		Func<object> func2 = () =>
		{
			if (!rightEvaluated)
			{
				expression.RightExpression.Accept(this);
				rightValue = Result;
				rightEvaluated = true;
			}
			return rightValue;
		};
		switch (expression.Type)
		{
		case BinaryExpressionType.And:
			Result = Convert.ToBoolean(func()) && Convert.ToBoolean(func2());
			break;
		case BinaryExpressionType.Or:
			Result = Convert.ToBoolean(func()) || Convert.ToBoolean(func2());
			break;
		case BinaryExpressionType.Div:
			Result = ((IsReal(func()) || IsReal(func2())) ? Numbers.Divide(func(), func2(), _options) : Numbers.Divide(Convert.ToDouble(func()), func2(), _options));
			break;
		case BinaryExpressionType.Equal:
			Result = CompareUsingMostPreciseType(func(), func2()) == 0;
			break;
		case BinaryExpressionType.Greater:
			Result = CompareUsingMostPreciseType(func(), func2()) > 0;
			break;
		case BinaryExpressionType.GreaterOrEqual:
			Result = CompareUsingMostPreciseType(func(), func2()) >= 0;
			break;
		case BinaryExpressionType.Lesser:
			Result = CompareUsingMostPreciseType(func(), func2()) < 0;
			break;
		case BinaryExpressionType.LesserOrEqual:
			Result = CompareUsingMostPreciseType(func(), func2()) <= 0;
			break;
		case BinaryExpressionType.Minus:
			Result = (Checked ? Numbers.SoustractChecked(func(), func2(), _options) : Numbers.Soustract(func(), func2(), _options));
			break;
		case BinaryExpressionType.Modulo:
			Result = Numbers.Modulo(func(), func2());
			break;
		case BinaryExpressionType.NotEqual:
			Result = CompareUsingMostPreciseType(func(), func2()) != 0;
			break;
		case BinaryExpressionType.Plus:
			if (func() is string)
			{
				Result = string.Concat(func(), func2());
			}
			else
			{
				Result = (Checked ? Numbers.AddChecked(func(), func2(), _options) : Numbers.Add(func(), func2(), _options));
			}
			break;
		case BinaryExpressionType.Times:
			Result = (Checked ? Numbers.MultiplyChecked(func(), func2(), _options) : Numbers.Multiply(func(), func2(), _options));
			break;
		case BinaryExpressionType.BitwiseAnd:
			Result = Convert.ToUInt16(func()) & Convert.ToUInt16(func2());
			break;
		case BinaryExpressionType.BitwiseOr:
			Result = Convert.ToUInt16(func()) | Convert.ToUInt16(func2());
			break;
		case BinaryExpressionType.BitwiseXOr:
			Result = Convert.ToUInt16(func()) ^ Convert.ToUInt16(func2());
			break;
		case BinaryExpressionType.LeftShift:
			Result = Convert.ToUInt16(func()) << (int)Convert.ToUInt16(func2());
			break;
		case BinaryExpressionType.RightShift:
			Result = Convert.ToUInt16(func()) >> (int)Convert.ToUInt16(func2());
			break;
		}
	}

	public override void Visit(UnaryExpression expression)
	{
		expression.Expression.Accept(this);
		switch (expression.Type)
		{
		case UnaryExpressionType.Not:
			Result = !Convert.ToBoolean(Result);
			break;
		case UnaryExpressionType.Negate:
			Result = Numbers.Soustract(0, Result, _options);
			break;
		case UnaryExpressionType.BitwiseNot:
			Result = ~Convert.ToUInt16(Result);
			break;
		}
	}

	public override void Visit(ValueExpression expression)
	{
		Result = expression.Value;
	}

	public override void Visit(Function function)
	{
		FunctionArgs functionArgs = new FunctionArgs
		{
			Parameters = new Expression[function.Expressions.Length]
		};
		for (int i = 0; i < function.Expressions.Length; i++)
		{
			functionArgs.Parameters[i] = new Expression(function.Expressions[i], _options);
			functionArgs.Parameters[i].EvaluateFunction += EvaluateFunction;
			functionArgs.Parameters[i].EvaluateParameter += EvaluateParameter;
			functionArgs.Parameters[i].Parameters = Parameters;
		}
		OnEvaluateFunction(IgnoreCase ? function.Identifier.Name.ToLower() : function.Identifier.Name, functionArgs);
		if (functionArgs.HasResult)
		{
			Result = functionArgs.Result;
			return;
		}
		string name = function.Identifier.Name;
		if (name != null)
		{
			if (name.Equals("abs", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Abs", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Abs() takes exactly 1 argument");
				}
				if ((_options & EvaluateOptions.UseDoubleForAbsFunction) == EvaluateOptions.UseDoubleForAbsFunction)
				{
					Result = Math.Abs(Convert.ToDouble(Evaluate(function.Expressions[0])));
				}
				else
				{
					Result = Math.Abs(Convert.ToDecimal(Evaluate(function.Expressions[0])));
				}
				return;
			}
			if (name.Equals("acos", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Acos", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Acos() takes exactly 1 argument");
				}
				Result = Math.Acos(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("asin", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Asin", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Asin() takes exactly 1 argument");
				}
				Result = Math.Asin(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("atan", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Atan", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Atan() takes exactly 1 argument");
				}
				Result = Math.Atan(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("ceiling", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Ceiling", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Ceiling() takes exactly 1 argument");
				}
				Result = Math.Ceiling(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("cos", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Cos", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Cos() takes exactly 1 argument");
				}
				Result = Math.Cos(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("exp", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Exp", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Exp() takes exactly 1 argument");
				}
				Result = Math.Exp(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("floor", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Floor", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Floor() takes exactly 1 argument");
				}
				Result = Math.Floor(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("ieeeremainder", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("IEEERemainder", function.Identifier.Name);
				if (function.Expressions.Length != 2)
				{
					throw new ArgumentException("IEEERemainder() takes exactly 2 arguments");
				}
				Result = Math.IEEERemainder(Convert.ToDouble(Evaluate(function.Expressions[0])), Convert.ToDouble(Evaluate(function.Expressions[1])));
				return;
			}
			if (name.Equals("log", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Log", function.Identifier.Name);
				if (function.Expressions.Length != 2)
				{
					throw new ArgumentException("Log() takes exactly 2 arguments");
				}
				Result = Math.Log(Convert.ToDouble(Evaluate(function.Expressions[0])), Convert.ToDouble(Evaluate(function.Expressions[1])));
				return;
			}
			if (name.Equals("log10", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Log10", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Log10() takes exactly 1 argument");
				}
				Result = Math.Log10(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("pow", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Pow", function.Identifier.Name);
				if (function.Expressions.Length != 2)
				{
					throw new ArgumentException("Pow() takes exactly 2 arguments");
				}
				Result = Math.Pow(Convert.ToDouble(Evaluate(function.Expressions[0])), Convert.ToDouble(Evaluate(function.Expressions[1])));
				return;
			}
			if (name.Equals("round", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Round", function.Identifier.Name);
				if (function.Expressions.Length != 2)
				{
					throw new ArgumentException("Round() takes exactly 2 arguments");
				}
				MidpointRounding mode = (((_options & EvaluateOptions.RoundAwayFromZero) == EvaluateOptions.RoundAwayFromZero) ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
				Result = Math.Round(Convert.ToDouble(Evaluate(function.Expressions[0])), Convert.ToInt16(Evaluate(function.Expressions[1])), mode);
				return;
			}
			if (name.Equals("sign", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Sign", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Sign() takes exactly 1 argument");
				}
				Result = Math.Sign(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("sin", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Sin", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Sin() takes exactly 1 argument");
				}
				Result = Math.Sin(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("sqrt", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Sqrt", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Sqrt() takes exactly 1 argument");
				}
				Result = Math.Sqrt(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("tan", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Tan", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Tan() takes exactly 1 argument");
				}
				Result = Math.Tan(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("truncate", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Truncate", function.Identifier.Name);
				if (function.Expressions.Length != 1)
				{
					throw new ArgumentException("Truncate() takes exactly 1 argument");
				}
				Result = Math.Truncate(Convert.ToDouble(Evaluate(function.Expressions[0])));
				return;
			}
			if (name.Equals("max", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Max", function.Identifier.Name);
				if (function.Expressions.Length != 2)
				{
					throw new ArgumentException("Max() takes exactly 2 arguments");
				}
				object a = Evaluate(function.Expressions[0]);
				object b = Evaluate(function.Expressions[1]);
				Result = Numbers.Max(a, b);
				return;
			}
			if (name.Equals("min", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("Min", function.Identifier.Name);
				if (function.Expressions.Length != 2)
				{
					throw new ArgumentException("Min() takes exactly 2 arguments");
				}
				object a2 = Evaluate(function.Expressions[0]);
				object b2 = Evaluate(function.Expressions[1]);
				Result = Numbers.Min(a2, b2);
				return;
			}
			if (name.Equals("if", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("if", function.Identifier.Name);
				if (function.Expressions.Length != 3)
				{
					throw new ArgumentException("if() takes exactly 3 arguments");
				}
				bool flag = Convert.ToBoolean(Evaluate(function.Expressions[0]));
				Result = (flag ? Evaluate(function.Expressions[1]) : Evaluate(function.Expressions[2]));
				return;
			}
			if (name.Equals("in", StringComparison.OrdinalIgnoreCase))
			{
				CheckCase("in", function.Identifier.Name);
				if (function.Expressions.Length < 2)
				{
					throw new ArgumentException("in() takes at least 2 arguments");
				}
				object a3 = Evaluate(function.Expressions[0]);
				bool flag2 = false;
				for (int j = 1; j < function.Expressions.Length; j++)
				{
					object b3 = Evaluate(function.Expressions[j]);
					if (CompareUsingMostPreciseType(a3, b3) == 0)
					{
						flag2 = true;
						break;
					}
				}
				Result = flag2;
				return;
			}
		}
		throw new ArgumentException("Function not found", function.Identifier.Name);
	}

	private void CheckCase(string function, string called)
	{
		if (IgnoreCase)
		{
			if (!function.Equals(called, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("Function not found", called);
			}
		}
		else if (function != called)
		{
			throw new ArgumentException($"Function not found {called}. Try {function} instead.");
		}
	}

	private void OnEvaluateFunction(string name, FunctionArgs args)
	{
		if (EvaluateFunction != null)
		{
			EvaluateFunction(name, args);
		}
	}

	public override void Visit(Identifier parameter)
	{
		if (Parameters.ContainsKey(parameter.Name))
		{
			if (Parameters[parameter.Name] is Expression)
			{
				Expression expression = (Expression)Parameters[parameter.Name];
				foreach (KeyValuePair<string, object> parameter2 in Parameters)
				{
					expression.Parameters[parameter2.Key] = parameter2.Value;
				}
				expression.EvaluateFunction += EvaluateFunction;
				expression.EvaluateParameter += EvaluateParameter;
				Result = ((Expression)Parameters[parameter.Name]).Evaluate();
			}
			else
			{
				Result = Parameters[parameter.Name];
			}
		}
		else
		{
			ParameterArgs parameterArgs = new ParameterArgs();
			OnEvaluateParameter(parameter.Name, parameterArgs);
			if (!parameterArgs.HasResult)
			{
				throw new ArgumentException("Parameter was not defined", parameter.Name);
			}
			Result = parameterArgs.Result;
		}
	}

	private void OnEvaluateParameter(string name, ParameterArgs args)
	{
		if (EvaluateParameter != null)
		{
			EvaluateParameter(name, args);
		}
	}
}
