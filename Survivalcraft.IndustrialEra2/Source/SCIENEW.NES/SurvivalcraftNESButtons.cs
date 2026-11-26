using System;

namespace SCIENEW.NES;

[Flags]
public enum SurvivalcraftNESButtons
{
	A = 1,
	B = 2,
	Select = 4,
	Start = 8,
	Up = 0x10,
	Down = 0x40,
	Left = 0x80,
	Right = 0x20
}
