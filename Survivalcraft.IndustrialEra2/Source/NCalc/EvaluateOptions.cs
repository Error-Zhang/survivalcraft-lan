using System;

namespace NCalc;

[Flags]
public enum EvaluateOptions
{
	None = 1,
	IgnoreCase = 2,
	NoCache = 4,
	IterateParameters = 8,
	RoundAwayFromZero = 0x10,
	MatchStringsWithIgnoreCase = 0x20,
	MatchStringsOrdinal = 0x40,
	OverflowProtection = 0x80,
	BooleanCalculation = 0x100,
	UseDoubleForAbsFunction = 0x200,
	AllowNullParameter = 0x400
}
