using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NCalc.Domain;

namespace NCalc;

internal class LambdaExpressionVistor : LogicalExpressionVisitor
{
	private readonly IDictionary<string, object> _parameters;

	private readonly System.Linq.Expressions.Expression _context;

	private readonly EvaluateOptions _options = EvaluateOptions.None;

	private readonly Dictionary<Type, HashSet<Type>> _implicitPrimitiveConversionTable = new Dictionary<Type, HashSet<Type>>
	{
		{
			typeof(sbyte),
			new HashSet<Type>
			{
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			typeof(byte),
			new HashSet<Type>
			{
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			typeof(short),
			new HashSet<Type>
			{
				typeof(int),
				typeof(long),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			typeof(ushort),
			new HashSet<Type>
			{
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			typeof(int),
			new HashSet<Type>
			{
				typeof(long),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			typeof(uint),
			new HashSet<Type>
			{
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			typeof(long),
			new HashSet<Type>
			{
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			typeof(char),
			new HashSet<Type>
			{
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			typeof(float),
			new HashSet<Type> { typeof(double) }
		},
		{
			typeof(ulong),
			new HashSet<Type>
			{
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		}
	};

	private bool Ordinal => (_options & EvaluateOptions.MatchStringsOrdinal) == EvaluateOptions.MatchStringsOrdinal;

	private bool IgnoreCaseString => (_options & EvaluateOptions.MatchStringsWithIgnoreCase) == EvaluateOptions.MatchStringsWithIgnoreCase;

	private bool Checked => (_options & EvaluateOptions.OverflowProtection) == EvaluateOptions.OverflowProtection;

	public System.Linq.Expressions.Expression Result { get; private set; }

	public LambdaExpressionVistor(IDictionary<string, object> parameters, EvaluateOptions options)
	{
		_parameters = parameters;
		_options = options;
	}

	public LambdaExpressionVistor(ParameterExpression context, EvaluateOptions options)
	{
		_context = context;
		_options = options;
	}

	public override void Visit(LogicalExpression expression)
	{
		throw new NotImplementedException();
	}

	public override void Visit(TernaryExpression expression)
	{
		expression.LeftExpression.Accept(this);
		System.Linq.Expressions.Expression result = Result;
		expression.MiddleExpression.Accept(this);
		System.Linq.Expressions.Expression result2 = Result;
		expression.RightExpression.Accept(this);
		System.Linq.Expressions.Expression result3 = Result;
		Result = System.Linq.Expressions.Expression.Condition(result, result2, result3);
	}

	public override void Visit(NCalc.Domain.BinaryExpression expression)
	{
		expression.LeftExpression.Accept(this);
		System.Linq.Expressions.Expression result = Result;
		expression.RightExpression.Accept(this);
		System.Linq.Expressions.Expression result2 = Result;
		switch (expression.Type)
		{
		case BinaryExpressionType.And:
			Result = System.Linq.Expressions.Expression.AndAlso(result, result2);
			break;
		case BinaryExpressionType.Or:
			Result = System.Linq.Expressions.Expression.OrElse(result, result2);
			break;
		case BinaryExpressionType.NotEqual:
			Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.NotEqual, expression.Type);
			break;
		case BinaryExpressionType.LesserOrEqual:
			Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.LessThanOrEqual, expression.Type);
			break;
		case BinaryExpressionType.GreaterOrEqual:
			Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.GreaterThanOrEqual, expression.Type);
			break;
		case BinaryExpressionType.Lesser:
			Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.LessThan, expression.Type);
			break;
		case BinaryExpressionType.Greater:
			Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.GreaterThan, expression.Type);
			break;
		case BinaryExpressionType.Equal:
			Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.Equal, expression.Type);
			break;
		case BinaryExpressionType.Minus:
			if (Checked)
			{
				Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.SubtractChecked);
			}
			else
			{
				Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.Subtract);
			}
			break;
		case BinaryExpressionType.Plus:
			if (Checked)
			{
				Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.AddChecked);
			}
			else
			{
				Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.Add);
			}
			break;
		case BinaryExpressionType.Modulo:
			Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.Modulo);
			break;
		case BinaryExpressionType.Div:
			Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.Divide);
			break;
		case BinaryExpressionType.Times:
			if (Checked)
			{
				Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.MultiplyChecked);
			}
			else
			{
				Result = WithCommonNumericType(result, result2, System.Linq.Expressions.Expression.Multiply);
			}
			break;
		case BinaryExpressionType.BitwiseOr:
			Result = System.Linq.Expressions.Expression.Or(result, result2);
			break;
		case BinaryExpressionType.BitwiseAnd:
			Result = System.Linq.Expressions.Expression.And(result, result2);
			break;
		case BinaryExpressionType.BitwiseXOr:
			Result = System.Linq.Expressions.Expression.ExclusiveOr(result, result2);
			break;
		case BinaryExpressionType.LeftShift:
			Result = System.Linq.Expressions.Expression.LeftShift(result, result2);
			break;
		case BinaryExpressionType.RightShift:
			Result = System.Linq.Expressions.Expression.RightShift(result, result2);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void Visit(NCalc.Domain.UnaryExpression expression)
	{
		expression.Expression.Accept(this);
		switch (expression.Type)
		{
		case UnaryExpressionType.Not:
			Result = System.Linq.Expressions.Expression.Not(Result);
			break;
		case UnaryExpressionType.Negate:
			Result = System.Linq.Expressions.Expression.Negate(Result);
			break;
		case UnaryExpressionType.BitwiseNot:
			Result = System.Linq.Expressions.Expression.Not(Result);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void Visit(ValueExpression expression)
	{
		Result = System.Linq.Expressions.Expression.Constant(expression.Value);
	}

	public override void Visit(Function function)
	{
		int num = function.Expressions.Length;
		System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[num];
		for (int i = 0; i < num; i++)
		{
			function.Expressions[i].Accept(this);
			array[i] = Result;
		}
		string text = function.Identifier.Name.ToLowerInvariant();
		Type[] array2 = new Type[5]
		{
			typeof(double),
			typeof(float),
			typeof(long),
			typeof(int),
			typeof(short)
		};
		if (text == "if")
		{
			int num2 = Array.IndexOf(array2, array[1].Type);
			int num3 = Array.IndexOf(array2, array[2].Type);
			if (num2 >= 0 && num3 >= 0 && num2 != num3)
			{
				array[1] = System.Linq.Expressions.Expression.Convert(array[1], array2[Math.Min(num2, num3)]);
				array[2] = System.Linq.Expressions.Expression.Convert(array[2], array2[Math.Min(num2, num3)]);
			}
			Result = System.Linq.Expressions.Expression.Condition(array[0], array[1], array[2]);
			return;
		}
		if (text == "in")
		{
			int num4 = int.MaxValue;
			System.Linq.Expressions.Expression[] array3 = array;
			foreach (System.Linq.Expressions.Expression expression in array3)
			{
				num4 = Math.Min(num4, Array.IndexOf(array2, expression.Type));
			}
			if (num4 >= array2.Length)
			{
				throw new Exception("Not Supported Arg Type");
			}
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = System.Linq.Expressions.Expression.Convert(array[k], array2[num4]);
			}
			NewArrayExpression expression2 = System.Linq.Expressions.Expression.NewArrayInit(array[0].Type, new ArraySegment<System.Linq.Expressions.Expression>(array, 1, array.Length - 1));
			MethodCallExpression left = System.Linq.Expressions.Expression.Call(typeof(Array).GetRuntimeMethod("IndexOf", new Type[2]
			{
				typeof(Array),
				typeof(object)
			}), System.Linq.Expressions.Expression.Convert(expression2, typeof(Array)), System.Linq.Expressions.Expression.Convert(array[0], typeof(object)));
			Result = System.Linq.Expressions.Expression.GreaterThanOrEqual(left, System.Linq.Expressions.Expression.Constant(0));
			return;
		}
		ExtendedMethodInfo extendedMethodInfo = FindMethod(function.Identifier.Name, array);
		if (extendedMethodInfo != null)
		{
			Result = System.Linq.Expressions.Expression.Call(_context, extendedMethodInfo.BaseMethodInfo, extendedMethodInfo.PreparedArguments);
			return;
		}
		if (num == 0)
		{
			throw new TargetParameterCountException();
		}
		System.Linq.Expressions.UnaryExpression unaryExpression = System.Linq.Expressions.Expression.Convert(array[0], typeof(double));
		System.Linq.Expressions.UnaryExpression unaryExpression2 = ((num >= 2) ? System.Linq.Expressions.Expression.Convert(array[1], typeof(double)) : null);
		switch (text)
		{
		case "min":
			if (num < 2)
			{
				throw new TargetParameterCountException();
			}
			Result = System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.LessThan(unaryExpression, unaryExpression2), unaryExpression, unaryExpression2);
			break;
		case "max":
			if (num < 2)
			{
				throw new TargetParameterCountException();
			}
			Result = System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.GreaterThan(unaryExpression, unaryExpression2), unaryExpression, unaryExpression2);
			break;
		case "pow":
			if (num < 2)
			{
				throw new TargetParameterCountException();
			}
			Result = System.Linq.Expressions.Expression.Power(unaryExpression, unaryExpression2);
			break;
		case "abs":
			Result = MethExpressionWithOneParameter("Abs", unaryExpression);
			break;
		case "acos":
			Result = MethExpressionWithOneParameter("Acos", unaryExpression);
			break;
		case "asin":
			Result = MethExpressionWithOneParameter("Asin", unaryExpression);
			break;
		case "atan":
			Result = MethExpressionWithOneParameter("Atan", unaryExpression);
			break;
		case "atan2":
			if (num < 2)
			{
				throw new TargetParameterCountException();
			}
			Result = MethExpressionWithTwoParameter("Atan2", unaryExpression, unaryExpression2);
			break;
		case "ceiling":
			Result = MethExpressionWithOneParameter("Ceiling", unaryExpression);
			break;
		case "cos":
			Result = MethExpressionWithOneParameter("Cos", unaryExpression);
			break;
		case "cosh":
			Result = MethExpressionWithOneParameter("Cosh", unaryExpression);
			break;
		case "exp":
			Result = MethExpressionWithOneParameter("Exp", unaryExpression);
			break;
		case "floor":
			Result = MethExpressionWithOneParameter("Floor", unaryExpression);
			break;
		case "ieeremainder":
			if (num < 2)
			{
				throw new TargetParameterCountException();
			}
			Result = MethExpressionWithTwoParameter("IEEERemainder", unaryExpression, unaryExpression2);
			break;
		case "log":
			Result = ((num >= 2) ? MethExpressionWithTwoParameter("Log", unaryExpression, unaryExpression2) : MethExpressionWithOneParameter("Log", unaryExpression));
			break;
		case "log10":
			Result = MethExpressionWithOneParameter("Log10", unaryExpression);
			break;
		case "round":
			if (num >= 2)
			{
				System.Linq.Expressions.Expression.Call(typeof(Math).GetRuntimeMethod("Round", new Type[2]
				{
					typeof(double),
					typeof(int)
				}), unaryExpression, System.Linq.Expressions.Expression.Convert(array[1], typeof(int)));
			}
			else
			{
				Result = MethExpressionWithOneParameter("Round", unaryExpression);
			}
			break;
		case "sign":
			Result = MethExpressionWithOneParameter("Sign", unaryExpression);
			break;
		case "sin":
			Result = MethExpressionWithOneParameter("Sin", unaryExpression);
			break;
		case "sinh":
			Result = MethExpressionWithOneParameter("Sinh", unaryExpression);
			break;
		case "sqrt":
			Result = MethExpressionWithOneParameter("Sqrt", unaryExpression);
			break;
		case "tan":
			Result = MethExpressionWithOneParameter("Tan", unaryExpression);
			break;
		case "tanh":
			Result = MethExpressionWithOneParameter("Tanh", unaryExpression);
			break;
		case "truncate":
			Result = MethExpressionWithOneParameter("Truncate", unaryExpression);
			break;
		default:
			throw new MissingMethodException("method not found: " + text);
		}
	}

	public MethodCallExpression MethExpressionWithOneParameter(string methodName, System.Linq.Expressions.Expression arg)
	{
		return System.Linq.Expressions.Expression.Call(typeof(Math).GetRuntimeMethod(methodName, new Type[1] { typeof(double) }), arg);
	}

	public MethodCallExpression MethExpressionWithTwoParameter(string methodName, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2)
	{
		return System.Linq.Expressions.Expression.Call(typeof(Math).GetRuntimeMethod(methodName, new Type[2]
		{
			typeof(double),
			typeof(double)
		}), arg1, arg2);
	}

	public override void Visit(Identifier function)
	{
		if (_context == null)
		{
			Result = System.Linq.Expressions.Expression.Constant(_parameters[function.Name]);
		}
		else
		{
			Result = System.Linq.Expressions.Expression.PropertyOrField(_context, function.Name);
		}
	}

	private ExtendedMethodInfo FindMethod(string methodName, System.Linq.Expressions.Expression[] methodArgs)
	{
		if (_context == null)
		{
			return null;
		}
		TypeInfo typeInfo = _context.Type.GetTypeInfo();
		TypeInfo typeInfo2 = typeof(object).GetTypeInfo();
		do
		{
			IEnumerable<MethodInfo> enumerable = typeInfo.DeclaredMethods.Where((MethodInfo m) => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) && (object)m != null && m.IsPublic && !m.IsStatic);
			List<ExtendedMethodInfo> list = new List<ExtendedMethodInfo>();
			foreach (MethodInfo item in enumerable)
			{
				ParameterInfo[] parameters = item.GetParameters();
				Tuple<int, System.Linq.Expressions.Expression[]> tuple = PrepareMethodArgumentsIfValid(parameters, methodArgs);
				if (tuple != null)
				{
					ExtendedMethodInfo extendedMethodInfo = new ExtendedMethodInfo
					{
						BaseMethodInfo = item,
						PreparedArguments = tuple.Item2,
						Score = tuple.Item1
					};
					if (extendedMethodInfo.Score == 0)
					{
						return extendedMethodInfo;
					}
					list.Add(extendedMethodInfo);
				}
			}
			if (list.Any())
			{
				return list.OrderBy((ExtendedMethodInfo method) => method.Score).First();
			}
			typeInfo = typeInfo.BaseType.GetTypeInfo();
		}
		while (typeInfo != typeInfo2);
		return null;
	}

	private Tuple<int, System.Linq.Expressions.Expression[]> PrepareMethodArgumentsIfValid(ParameterInfo[] parameters, System.Linq.Expressions.Expression[] arguments)
	{
		if (!parameters.Any() && !arguments.Any())
		{
			return Tuple.Create(0, arguments);
		}
		if (!parameters.Any())
		{
			return null;
		}
		ParameterInfo parameterInfo = parameters.Last();
		bool flag = parameterInfo.IsDefined(typeof(ParamArrayAttribute));
		if (flag && parameters.Length > arguments.Length)
		{
			return null;
		}
		System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[parameters.Length];
		System.Linq.Expressions.Expression[] array2 = null;
		Type type = null;
		int num = 0;
		if (!flag)
		{
			if (parameters.Length != arguments.Length)
			{
				return null;
			}
		}
		else
		{
			num = parameterInfo.Position;
			type = parameterInfo.ParameterType.GetElementType();
			array2 = new System.Linq.Expressions.Expression[arguments.Length - parameters.Length + 1];
		}
		int num2 = 0;
		for (int i = 0; i < arguments.Length; i++)
		{
			bool flag2 = flag && i >= num;
			System.Linq.Expressions.Expression argument = arguments[i];
			Type type2 = argument.Type;
			Type type3 = (flag2 ? type : parameters[i].ParameterType);
			if (type2 != type3)
			{
				if (!TryCastImplicitly(type2, type3, ref argument))
				{
					return null;
				}
				num2++;
			}
			if (!flag2)
			{
				array[i] = argument;
			}
			else
			{
				array2[i - num] = argument;
			}
		}
		if (flag)
		{
			array[num] = System.Linq.Expressions.Expression.NewArrayInit(type, array2);
		}
		return Tuple.Create(num2, array);
	}

	private bool TryCastImplicitly(Type from, Type to, ref System.Linq.Expressions.Expression argument)
	{
		if (!_implicitPrimitiveConversionTable.TryGetValue(from, out HashSet<Type> value) || !value.Contains(to))
		{
			argument = null;
			return false;
		}
		argument = System.Linq.Expressions.Expression.Convert(argument, to);
		return true;
	}

	private System.Linq.Expressions.Expression WithCommonNumericType(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right, Func<System.Linq.Expressions.Expression, System.Linq.Expressions.Expression, System.Linq.Expressions.Expression> action, BinaryExpressionType expressiontype = BinaryExpressionType.Unknown)
	{
		left = UnwrapNullable(left);
		right = UnwrapNullable(right);
		if (_options.HasFlag(EvaluateOptions.BooleanCalculation))
		{
			if (left.Type == typeof(bool))
			{
				left = System.Linq.Expressions.Expression.Condition(left, System.Linq.Expressions.Expression.Constant(1.0), System.Linq.Expressions.Expression.Constant(0.0));
			}
			if (right.Type == typeof(bool))
			{
				right = System.Linq.Expressions.Expression.Condition(right, System.Linq.Expressions.Expression.Constant(1.0), System.Linq.Expressions.Expression.Constant(0.0));
			}
		}
		Type[] array = new Type[11]
		{
			typeof(decimal),
			typeof(double),
			typeof(float),
			typeof(ulong),
			typeof(long),
			typeof(uint),
			typeof(int),
			typeof(ushort),
			typeof(short),
			typeof(byte),
			typeof(sbyte)
		};
		int num = Array.IndexOf(array, left.Type);
		int num2 = Array.IndexOf(array, right.Type);
		if (num >= 0 && num2 >= 0)
		{
			Type type = array[Math.Min(num, num2)];
			if (left.Type != type)
			{
				left = System.Linq.Expressions.Expression.Convert(left, type);
			}
			if (right.Type != type)
			{
				right = System.Linq.Expressions.Expression.Convert(right, type);
			}
		}
		System.Linq.Expressions.Expression expression = null;
		if (IgnoreCaseString)
		{
			expression = ((!Ordinal) ? System.Linq.Expressions.Expression.Property(null, typeof(StringComparer), "CurrentCultureIgnoreCase") : System.Linq.Expressions.Expression.Property(null, typeof(StringComparer), "OrdinalIgnoreCase"));
		}
		else
		{
			expression = System.Linq.Expressions.Expression.Property(null, typeof(StringComparer), "Ordinal");
		}
		if (expression != null && (typeof(string).Equals(left.Type) || typeof(string).Equals(right.Type)))
		{
			switch (expressiontype)
			{
			case BinaryExpressionType.Equal:
				return System.Linq.Expressions.Expression.Call(expression, typeof(StringComparer).GetRuntimeMethod("Equals", new Type[2]
				{
					typeof(string),
					typeof(string)
				}), new System.Linq.Expressions.Expression[2] { left, right });
			case BinaryExpressionType.NotEqual:
				return System.Linq.Expressions.Expression.Not(System.Linq.Expressions.Expression.Call(expression, typeof(StringComparer).GetRuntimeMethod("Equals", new Type[2]
				{
					typeof(string),
					typeof(string)
				}), new System.Linq.Expressions.Expression[2] { left, right }));
			case BinaryExpressionType.GreaterOrEqual:
				return System.Linq.Expressions.Expression.GreaterThanOrEqual(System.Linq.Expressions.Expression.Call(expression, typeof(StringComparer).GetRuntimeMethod("Compare", new Type[2]
				{
					typeof(string),
					typeof(string)
				}), new System.Linq.Expressions.Expression[2] { left, right }), System.Linq.Expressions.Expression.Constant(0));
			case BinaryExpressionType.LesserOrEqual:
				return System.Linq.Expressions.Expression.LessThanOrEqual(System.Linq.Expressions.Expression.Call(expression, typeof(StringComparer).GetRuntimeMethod("Compare", new Type[2]
				{
					typeof(string),
					typeof(string)
				}), new System.Linq.Expressions.Expression[2] { left, right }), System.Linq.Expressions.Expression.Constant(0));
			case BinaryExpressionType.Greater:
				return System.Linq.Expressions.Expression.GreaterThan(System.Linq.Expressions.Expression.Call(expression, typeof(StringComparer).GetRuntimeMethod("Compare", new Type[2]
				{
					typeof(string),
					typeof(string)
				}), new System.Linq.Expressions.Expression[2] { left, right }), System.Linq.Expressions.Expression.Constant(0));
			case BinaryExpressionType.Lesser:
				return System.Linq.Expressions.Expression.LessThan(System.Linq.Expressions.Expression.Call(expression, typeof(StringComparer).GetRuntimeMethod("Compare", new Type[2]
				{
					typeof(string),
					typeof(string)
				}), new System.Linq.Expressions.Expression[2] { left, right }), System.Linq.Expressions.Expression.Constant(0));
			}
		}
		return action(left, right);
	}

	private System.Linq.Expressions.Expression UnwrapNullable(System.Linq.Expressions.Expression expression)
	{
		TypeInfo typeInfo = expression.Type.GetTypeInfo();
		if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			return System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Property(expression, "HasValue"), System.Linq.Expressions.Expression.Property(expression, "Value"), System.Linq.Expressions.Expression.Default(expression.Type.GetTypeInfo().GenericTypeArguments[0]));
		}
		return expression;
	}
}
