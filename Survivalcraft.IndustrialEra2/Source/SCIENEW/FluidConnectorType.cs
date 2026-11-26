using System;

namespace SCIENEW;

[Flags]
public enum FluidConnectorType
{
	Input = 1,
	Output = 2,
	InputOutput = Input | Output
}
