using System;
using System.Globalization;
using Game;
using NCalc;

namespace SCIENEW;

public class EXTruthTableData : IEditableItemData
{
	public string Data = string.Empty;

	public IEditableItemData Copy()
	{
		return new EXTruthTableData
		{
			Data = Data
		};
	}

	public void LoadString(string data)
	{
		Data = data;
	}

	public string SaveString()
	{
		return Data;
	}

	public bool Calc(float i1, float i2, float i3, float i4, out float result, out string calcState)
	{
		result = 0f;
		calcState = string.Empty;
		if (string.IsNullOrEmpty(Data))
		{
			return true;
		}
		try
		{
			Func<float> func = new Expression(Data.Replace("i1", i1.ToString(CultureInfo.InvariantCulture)).Replace("i2", i2.ToString(CultureInfo.InvariantCulture)).Replace("i3", i3.ToString(CultureInfo.InvariantCulture))
				.Replace("i4", i4.ToString(CultureInfo.InvariantCulture))).ToLambda<float>();
			result = func();
			calcState = LanguageControl.Get("EXTruthTableData", "Succeed");
			return true;
		}
		catch (Exception ex)
		{
			calcState = ex.Message;
			result = 0f;
			return false;
		}
	}
}
