using System;
using System.IO;

namespace SCIENEW.AssetProtection;

public sealed class AmpkLazyMemoryStream : MemoryStream
{
	private readonly AmpkContentStore m_store;

	private readonly AmpkAssetEntry m_entry;

	private bool m_materialized;

	private bool m_materializing;

	public override bool CanRead
	{
		get
		{
			if (m_materializing)
			{
				return base.CanRead;
			}
			Materialize();
			return base.CanRead;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (m_materializing)
			{
				return base.CanSeek;
			}
			Materialize();
			return base.CanSeek;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (m_materializing)
			{
				return base.CanWrite;
			}
			Materialize();
			return base.CanWrite;
		}
	}

	public override long Length
	{
		get
		{
			if (m_materializing)
			{
				return base.Length;
			}
			Materialize();
			return base.Length;
		}
	}

	public override long Position
	{
		get
		{
			if (m_materializing)
			{
				return base.Position;
			}
			Materialize();
			return base.Position;
		}
		set
		{
			if (m_materializing)
			{
				base.Position = value;
				return;
			}
			Materialize();
			base.Position = value;
		}
	}

	public AmpkLazyMemoryStream(AmpkContentStore store, AmpkAssetEntry entry)
	{
		m_store = store;
		m_entry = entry;
	}

	private void Materialize()
	{
		if (m_materialized || m_materializing)
		{
			return;
		}
		m_materializing = true;
		try
		{
			using MemoryStream memoryStream = m_store.OpenPlaintextStream(m_entry);
			byte[] array = memoryStream.ToArray();
			base.Write(array, 0, array.Length);
			base.Position = 0L;
			m_materialized = true;
		}
		finally
		{
			m_materializing = false;
		}
	}

	public override void SetLength(long value)
	{
		if (m_materializing)
		{
			base.SetLength(value);
			return;
		}
		Materialize();
		base.SetLength(value);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (m_materializing)
		{
			return base.Read(buffer, offset, count);
		}
		Materialize();
		return base.Read(buffer, offset, count);
	}

	public override int Read(Span<byte> buffer)
	{
		if (m_materializing)
		{
			return base.Read(buffer);
		}
		Materialize();
		return base.Read(buffer);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (m_materializing)
		{
			base.Write(buffer, offset, count);
			return;
		}
		Materialize();
		base.Write(buffer, offset, count);
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		if (m_materializing)
		{
			base.Write(buffer);
			return;
		}
		Materialize();
		base.Write(buffer);
	}

	public override void CopyTo(Stream destination, int bufferSize)
	{
		if (m_materializing)
		{
			base.CopyTo(destination, bufferSize);
			return;
		}
		Materialize();
		base.CopyTo(destination, bufferSize);
	}

	public override long Seek(long offset, SeekOrigin loc)
	{
		if (m_materializing)
		{
			return base.Seek(offset, loc);
		}
		Materialize();
		return base.Seek(offset, loc);
	}
}
