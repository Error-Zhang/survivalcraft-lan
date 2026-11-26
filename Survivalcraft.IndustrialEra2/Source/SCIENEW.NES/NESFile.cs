namespace SCIENEW.NES;

public class NESFile
{
	public byte[] Data;

	public string FileName;

	public NESFile(byte[] data, string fileName)
	{
		Data = data;
		FileName = fileName;
	}
}
