using System.Linq;
using Game;

namespace SCIENEW.AssetProtection;

public static class AmpkContentRegistrar
{
	internal const string IndustrialPanoramaPath = "Textures/Gui/IndustrialEra2/Panorama";

	public static void RegisterAll(AmpkIndex index, AmpkContentStore store, ModEntity modEntity)
	{
		foreach (uint item in index.Entries.Select((AmpkAssetEntry e) => e.ChunkId).Distinct())
		{
			store.EnsureChunkLoaded(modEntity, item);
		}
		foreach (AmpkAssetEntry entry in index.Entries)
		{
			string contentPath = entry.VirtualPath == "Textures/Gui/Panorama.webp"
				? IndustrialPanoramaPath + ".webp"
				: entry.VirtualPath;
			ContentInfo contentInfo = new ContentInfo(contentPath);
			contentInfo.SetContentStream(new AmpkLazyMemoryStream(store, entry));
			ContentManager.Add(contentInfo);
		}
	}
}
