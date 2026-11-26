using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game;

namespace SCIENEW.AssetProtection;

public sealed class AmpkContentStore
{
	private readonly byte[] m_masterKey;

	private readonly Dictionary<uint, byte[]> m_chunkPayloads = new Dictionary<uint, byte[]>();

	private readonly HashSet<uint> m_verifiedChunks = new HashSet<uint>();

	private readonly Dictionary<string, AmpkAssetEntry> m_byPath;

	public AmpkIndex Index { get; }

	private AmpkContentStore(AmpkIndex index, byte[] masterKey)
	{
		Index = index;
		m_masterKey = masterKey;
		m_byPath = index.Entries.ToDictionary((AmpkAssetEntry e) => e.VirtualPath, (AmpkAssetEntry e) => e, StringComparer.Ordinal);
	}

	public static bool TryLoadFromMod(ModEntity modEntity, byte[] masterKey, out AmpkContentStore? store)
	{
		AmpkContentStore loaded = null;
		bool file = modEntity.GetFile(AmpkBinaryIO.IndexFileName, (Stream stream) =>
		{
			AmpkIndex index = AmpkBinaryIO.ReadIndexBytes(ModsManager.StreamToBytes(stream), masterKey);
			loaded = new AmpkContentStore(index, masterKey);
		});
		store = loaded;
		if (!file)
		{
			return store != null;
		}
		return false;
	}

	public bool TryGetEntry(string virtualPath, out AmpkAssetEntry? entry)
	{
		if (m_byPath.TryGetValue(virtualPath, out AmpkAssetEntry value))
		{
			entry = value;
			return true;
		}
		entry = null;
		return false;
	}

	public MemoryStream OpenPlaintextStream(AmpkAssetEntry entry)
	{
		byte[] chunkPayload = GetChunkPayload(entry.ChunkId);
		return new MemoryStream(AmpkAssetDecryptor.DecryptAsset(m_masterKey, chunkPayload, entry, AmpkRuntimeConfig.VerifyPlainHashOnRuntime), writable: false);
	}

	public bool TryOpenPlaintext(string virtualPath, out MemoryStream? stream)
	{
		stream = null;
		if (!TryGetEntry(virtualPath, out AmpkAssetEntry entry) || entry == null)
		{
			return false;
		}
		stream = OpenPlaintextStream(entry);
		return true;
	}

	private byte[] GetChunkPayload(uint chunkId)
	{
		if (m_chunkPayloads.TryGetValue(chunkId, out byte[] value))
		{
			return value;
		}
		throw new InvalidOperationException($"Chunk {chunkId} was not preloaded. Call EnsureChunkLoaded on ModEntity first.");
	}

	public void EnsureChunkLoaded(ModEntity modEntity, uint chunkId)
	{
		if (!m_chunkPayloads.ContainsKey(chunkId))
		{
			string chunkFileName = AmpkBinaryIO.GetChunkFileName(chunkId);
			if (modEntity.GetFile(chunkFileName, (Stream stream) =>
			{
				byte[] array = ModsManager.StreamToBytes(stream);
				using MemoryStream stream2 = new MemoryStream(array, writable: false);
				if (!m_verifiedChunks.Contains(chunkId))
				{
					AmpkBinaryIO.VerifyChunkHmac(stream2, m_masterKey);
					m_verifiedChunks.Add(chunkId);
				}
				int length = array.Length - AmpkBinaryIO.HashSize;
				byte[] value = array.AsSpan(0, length).ToArray();
				m_chunkPayloads[chunkId] = value;
			}) || !m_chunkPayloads.ContainsKey(chunkId))
			{
				throw new FileNotFoundException("Chunk file not found: " + chunkFileName);
			}
		}
	}

	public void PreloadAllChunks(ModEntity modEntity)
	{
		foreach (uint item in Index.Entries.Select((AmpkAssetEntry e) => e.ChunkId).Distinct())
		{
			EnsureChunkLoaded(modEntity, item);
		}
	}
}
