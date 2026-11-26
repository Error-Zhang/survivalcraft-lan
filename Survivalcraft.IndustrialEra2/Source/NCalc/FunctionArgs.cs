using System;

namespace NCalc;

public class FunctionArgs : EventArgs
{
	private object _result;

	public object Result
	{
		get
		{
			return _result;
		}
		set
		{
			_result = value;
			HasResult = true;
		}
	}

	public bool HasResult { get; set; }

	public Expression[] Parameters { get; set; } = Array.Empty<Expression>();

	public object[] EvaluateParameters()
	{
		object[] array = new object[Parameters.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parameters[i].Evaluate();
		}
		return array;
	}
}
