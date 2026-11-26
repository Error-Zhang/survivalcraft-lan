namespace Antlr3.Runtime.PCL.Output;

public interface IOutputStream
{
	void WriteLine();

	void WriteLine(string text);

	void WriteLine(object someObject);

	void Write(string text);

	void ReportProgress(double progress, string key, string message);
}
