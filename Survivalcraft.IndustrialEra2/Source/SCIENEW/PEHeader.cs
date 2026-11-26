using System.Runtime.InteropServices;

namespace SCIENEW;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PEHeader
{
	public ushort Machine;

	public ushort NumberOfSections;

	public uint TimeDateStamp;

	public uint PointerToSymbolTable;

	public uint NumberOfSymbols;

	public ushort SizeOfOptionalHeader;

	public ushort Characteristics;
}
