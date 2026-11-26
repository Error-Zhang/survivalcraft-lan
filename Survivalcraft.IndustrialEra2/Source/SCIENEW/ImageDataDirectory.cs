using System.Runtime.InteropServices;

namespace SCIENEW;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ImageDataDirectory
{
	public uint VirtualAddress;

	public uint Size;
}
