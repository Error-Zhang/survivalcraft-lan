using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr.Runtime;
using NCalc.Domain;

namespace NCalc;

public class Expression
{
	protected string OriginalExpression;

	private static bool _cacheEnabled = true;

	private static ConcurrentDictionary<string, WeakReference> _compiledExpressions = new ConcurrentDictionary<string, WeakReference>();

	protected Dictionary<string, IEnumerator> ParameterEnumerators;

	protected Dictionary<string, object> ParametersBackup;

	private Dictionary<string, object> _parameters;

	public EvaluateOptions Options { get; set; }

	public static bool CacheEnabled
	{
		get
		{
			return _cacheEnabled;
		}
		set
		{
			_cacheEnabled = value;
			if (!CacheEnabled)
			{
				_compiledExpressions = new ConcurrentDictionary<string, WeakReference>();
			}
		}
	}

	public string Error { get; private set; }

	public Exception ErrorException { get; private set; }

	public LogicalExpression ParsedExpression { get; private set; }

	public Dictionary<string, object> Parameters
	{
		get
		{
			return _parameters ?? (_parameters = new Dictionary<string, object>());
		}
		set
		{
			_parameters = value;
		}
	}

	public event EvaluateFunctionHandler EvaluateFunction;

	public event EvaluateParameterHandler EvaluateParameter;

	public Expression(string expression)
		: this(expression, EvaluateOptions.None)
	{
	}

	public Expression(string expression, EvaluateOptions options)
	{
		if (string.IsNullOrEmpty(expression))
		{
			throw new ArgumentException("Expression can't be empty", "expression");
		}
		OriginalExpression = expression;
		Options = options;
	}

	public Expression(LogicalExpression expression)
		: this(expression, EvaluateOptions.None)
	{
	}

	public Expression(LogicalExpression expression, EvaluateOptions options)
	{
		if (expression == null)
		{
			throw new ArgumentException("Expression can't be null", "expression");
		}
		ParsedExpression = expression;
		Options = options;
	}

	private static void CleanCache()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, WeakReference> compiledExpression in _compiledExpressions)
		{
			if (!compiledExpression.Value.IsAlive)
			{
				list.Add(compiledExpression.Key);
			}
		}
		foreach (string item in list)
		{
			_compiledExpressions.TryRemove(item, out WeakReference _);
		}
	}

	public static LogicalExpression Compile(string expression, bool nocache)
	{
		LogicalExpression logicalExpression = null;
		if (_cacheEnabled && !nocache && _compiledExpressions.ContainsKey(expression) && _compiledExpressions.TryGetValue(expression, out WeakReference value))
		{
			logicalExpression = value.Target as LogicalExpression;
			if (value.IsAlive && logicalExpression != null)
			{
				return logicalExpression;
			}
		}
		if (logicalExpression == null)
		{
			NCalcParser nCalcParser = new NCalcParser(new CommonTokenStream(new NCalcLexer(new ANTLRStringStream(expression))));
			logicalExpression = nCalcParser.ncalcExpression().value;
			List<string> errors = nCalcParser.Errors;
			if (errors != null && errors.Count > 0)
			{
				throw new EvaluationException(string.Join(Environment.NewLine, nCalcParser.Errors.ToArray()));
			}
			if (_cacheEnabled && !nocache)
			{
				try
				{
					if (_compiledExpressions.TryGetValue(expression, out WeakReference value2))
					{
						_compiledExpressions.TryUpdate(expression, new WeakReference(logicalExpression), value2);
					}
					else
					{
						_compiledExpressions.TryAdd(expression, new WeakReference(logicalExpression));
					}
				}
				finally
				{
					CleanCache();
				}
			}
		}
		return logicalExpression;
	}

	public bool HasErrors()
	{
		try
		{
			if (ParsedExpression == null)
			{
				ParsedExpression = Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
			}
			return ParsedExpression != null && Error != null;
		}
		catch (Exception ex)
		{
			Error = ex.Message;
			ErrorException = ex;
			return true;
		}
	}

	public Func<TResult> ToLambda<TResult>()
	{
		if (HasErrors())
		{
			throw new EvaluationException(Error, ErrorException);
		}
		if (ParsedExpression == null)
		{
			ParsedExpression = Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
		}
		LambdaExpressionVistor lambdaExpressionVistor = new LambdaExpressionVistor(Parameters, Options);
		ParsedExpression.Accept(lambdaExpressionVistor);
		System.Linq.Expressions.Expression expression = lambdaExpressionVistor.Result;
		if (expression.Type != typeof(TResult))
		{
			expression = System.Linq.Expressions.Expression.Convert(expression, typeof(TResult));
		}
		return System.Linq.Expressions.Expression.Lambda<Func<TResult>>(expression, Array.Empty<ParameterExpression>()).Compile();
	}

	public Func<TContext, TResult> ToLambda<TContext, TResult>()
	{
		if (HasErrors())
		{
			throw new EvaluationException(Error, ErrorException);
		}
		if (ParsedExpression == null)
		{
			ParsedExpression = Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
		}
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(TContext), "ctx");
		LambdaExpressionVistor lambdaExpressionVistor = new LambdaExpressionVistor(parameterExpression, Options);
		ParsedExpression.Accept(lambdaExpressionVistor);
		System.Linq.Expressions.Expression expression = lambdaExpressionVistor.Result;
		if (expression.Type != typeof(TResult))
		{
			expression = System.Linq.Expressions.Expression.Convert(expression, typeof(TResult));
		}
		return System.Linq.Expressions.Expression.Lambda<Func<TContext, TResult>>(expression, new ParameterExpression[1] { parameterExpression }).Compile();
	}

	public object Evaluate()
	{
		if (HasErrors())
		{
			throw new EvaluationException(Error, ErrorException);
		}
		if (ParsedExpression == null)
		{
			ParsedExpression = Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
		}
		EvaluationVisitor evaluationVisitor = new EvaluationVisitor(Options);
		evaluationVisitor.EvaluateFunction += EvaluateFunction;
		evaluationVisitor.EvaluateParameter += EvaluateParameter;
		evaluationVisitor.Parameters = Parameters;
		if ((Options & EvaluateOptions.AllowNullParameter) == EvaluateOptions.AllowNullParameter && !evaluationVisitor.Parameters.ContainsKey("null"))
		{
			evaluationVisitor.Parameters["null"] = null;
		}
		if ((Options & EvaluateOptions.IterateParameters) == EvaluateOptions.IterateParameters)
		{
			int num = -1;
			ParametersBackup = new Dictionary<string, object>();
			foreach (string key in Parameters.Keys)
			{
				ParametersBackup.Add(key, Parameters[key]);
			}
			ParameterEnumerators = new Dictionary<string, IEnumerator>();
			foreach (object value in Parameters.Values)
			{
				if (!(value is IEnumerable))
				{
					continue;
				}
				int num2 = 0;
				foreach (object item in (IEnumerable)value)
				{
					_ = item;
					num2++;
				}
				if (num == -1)
				{
					num = num2;
				}
				else if (num2 != num)
				{
					throw new EvaluationException("When IterateParameters option is used, IEnumerable parameters must have the same number of items");
				}
			}
			foreach (string key2 in Parameters.Keys)
			{
				if (Parameters[key2] is IEnumerable enumerable)
				{
					ParameterEnumerators.Add(key2, enumerable.GetEnumerator());
				}
			}
			List<object> list = new List<object>();
			for (int i = 0; i < num; i++)
			{
				foreach (string key3 in ParameterEnumerators.Keys)
				{
					IEnumerator enumerator5 = ParameterEnumerators[key3];
					enumerator5.MoveNext();
					Parameters[key3] = enumerator5.Current;
				}
				ParsedExpression.Accept(evaluationVisitor);
				list.Add(evaluationVisitor.Result);
			}
			return list;
		}
		ParsedExpression.Accept(evaluationVisitor);
		return evaluationVisitor.Result;
	}
}
