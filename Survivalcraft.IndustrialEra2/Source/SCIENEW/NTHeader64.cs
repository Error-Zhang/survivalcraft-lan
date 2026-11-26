using System.Runtime.InteropServices;

namespace SCIENEW;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NTHeader64
{
	public uint Signature;

	public PEHeader PEHeader;

	public OptionalPEHeader OptionalHeader;
}
