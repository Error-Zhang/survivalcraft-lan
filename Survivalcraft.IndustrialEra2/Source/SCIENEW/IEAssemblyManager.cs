using System.IO;
using System.Runtime.InteropServices;

namespace SCIENEW;

public static class IEAssemblyManager
{
	public static DOSHeader DOSHeader;

	public static NTHeader64 NTHeader64;

	private static T ReadStruct<T>(BinaryReader reader) where T : struct
	{
		int count = Marshal.SizeOf(typeof(T));
		GCHandle gCHandle = GCHandle.Alloc(reader.ReadBytes(count), GCHandleType.Pinned);
		T result = Marshal.PtrToStructure<T>(gCHandle.AddrOfPinnedObject());
		gCHandle.Free();
		return result;
	}

	public static void Init(Stream dllStream)
	{
		using BinaryReader reader = new BinaryReader(dllStream);
		DOSHeader = ReadStruct<DOSHeader>(reader);
		dllStream.Seek(DOSHeader.e_lfanew, SeekOrigin.Begin);
		NTHeader64 = ReadStruct<NTHeader64>(reader);
	}
}
