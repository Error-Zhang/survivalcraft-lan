using System.Collections.Generic;

namespace Antlr3.Runtime.PCL.Output;

public static class OutputStreamHost
{
	private static double _previous_progress;

	private static IList<IOutputStream> _output_streams;

	public static IOutputStream Error => new ErrorOutputStream();

	public static IOutputStream Out => new OutOutputStream();

	private static void InitializeIfNeeded()
	{
		if (_output_streams == null)
		{
			_output_streams = new List<IOutputStream>();
		}
	}

	public static void WriteLine()
	{
		WriteLine(string.Empty);
	}

	public static void WriteLine(string text)
	{
		InitializeIfNeeded();
		if (_previous_progress > 0.0)
		{
			_previous_progress = 0.0;
			WriteLine();
		}
		foreach (IOutputStream output_stream in _output_streams)
		{
			output_stream.WriteLine(text);
		}
	}

	public static void WriteLine(string format, params object[] arg)
	{
		WriteLine(string.Format(format, arg));
	}

	public static void Write(string text)
	{
		InitializeIfNeeded();
		if (_previous_progress > 0.0)
		{
			_previous_progress = 0.0;
			WriteLine();
		}
		foreach (IOutputStream output_stream in _output_streams)
		{
			output_stream.Write(text);
		}
	}

	public static void Write(string format, params object[] arg)
	{
		Write(string.Format(format, arg));
	}

	public static void ReportProgress(double progress, string key, string message)
	{
		InitializeIfNeeded();
		if (progress > 1.0)
		{
			progress = 1.0;
		}
		if (progress < 0.0)
		{
			progress = 0.0;
		}
		_previous_progress = progress;
		foreach (IOutputStream output_stream in _output_streams)
		{
			output_stream.ReportProgress(progress, key, message);
		}
		if (_previous_progress != 1.0)
		{
			return;
		}
		foreach (IOutputStream output_stream2 in _output_streams)
		{
			output_stream2.WriteLine(string.Empty);
		}
		_previous_progress = 0.0;
	}

	public static void ReportProgress(long current, long total, string key, string message)
	{
		ReportProgress((double)current / (double)total, key, message);
	}

	public static void RegisterOutputStream(IOutputStream output_stream)
	{
		InitializeIfNeeded();
		_output_streams.Add(output_stream);
	}

	public static void UnRegisterOutputStream(IOutputStream output_stream)
	{
		InitializeIfNeeded();
		_output_streams.Remove(output_stream);
	}
}
