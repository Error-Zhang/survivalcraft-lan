using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public static class IEIconManager
{
	private readonly record struct BlockIconKey(int BlockValue, bool UsesSubsystemTerrain);

	private sealed class BlockIconEntry
	{
		public RenderTarget2D RenderTarget;

		public LinkedListNode<BlockIconKey>? LruNode;
	}

	public const int BlockIconRasterLogicalSize = 128;

	public const int BlockIconRasterMultiplier = 4;

	private const int MaxCachedBlockIcons = 256;

	private static readonly Dictionary<BlockIconKey, BlockIconEntry> s_blockIcons = new Dictionary<BlockIconKey, BlockIconEntry>();

	private static readonly LinkedList<BlockIconKey> s_lru = new LinkedList<BlockIconKey>();

	private static readonly PrimitivesRenderer3D s_primitives = new PrimitivesRenderer3D();

	public static int BlockIconRasterDimension => 512;

	public static bool CanUseSharedBlockIconRaster(DrawBlockEnvironmentData env)
	{
		return env.SubsystemTerrain == null;
	}

	public static RenderTarget2D GetSharedBlockIconRaster(int blockValue, DrawBlockEnvironmentData envSource)
	{
		BlockIconKey blockIconKey = new BlockIconKey(blockValue, envSource.SubsystemTerrain != null);
		if (!s_blockIcons.TryGetValue(blockIconKey, out BlockIconEntry value))
		{
			EvictIfNeeded();
			value = new BlockIconEntry
			{
				RenderTarget = new RenderTarget2D(BlockIconRasterDimension, BlockIconRasterDimension, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8)
			};
			DrawBlockEnvironmentData env = ScratchEnvironment(envSource);
			RasterizeBlockIcon(blockValue, env, value.RenderTarget);
			RasterizeBlockIcon(blockValue, env, value.RenderTarget);
			value.LruNode = s_lru.AddFirst(blockIconKey);
			s_blockIcons[blockIconKey] = value;
			return value.RenderTarget;
		}
		Touch(blockIconKey);
		return value.RenderTarget;
	}

	public static void ClearSharedBlockIconCache()
	{
		foreach (BlockIconEntry value in s_blockIcons.Values)
		{
			value.RenderTarget.Dispose();
		}
		s_blockIcons.Clear();
		s_lru.Clear();
	}

	private static DrawBlockEnvironmentData ScratchEnvironment(DrawBlockEnvironmentData envSource)
	{
		return new DrawBlockEnvironmentData
		{
			SubsystemTerrain = envSource.SubsystemTerrain
		};
	}

	private static void Touch(BlockIconKey key)
	{
		if (s_blockIcons.TryGetValue(key, out BlockIconEntry value) && value.LruNode != null)
		{
			s_lru.Remove(value.LruNode);
			value.LruNode = s_lru.AddFirst(key);
		}
	}

	private static void EvictIfNeeded()
	{
		while (s_blockIcons.Count >= 256 && s_lru.Last != null)
		{
			BlockIconKey value = s_lru.Last.Value;
			s_lru.RemoveLast();
			if (s_blockIcons.Remove(value, out BlockIconEntry value2))
			{
				value2.RenderTarget.Dispose();
			}
		}
	}

	private static void RasterizeBlockIcon(int value, DrawBlockEnvironmentData env, RenderTarget2D rt)
	{
		Block obj = BlocksManager.Blocks[Terrain.ExtractContents(value)];
		Matrix matrix = Matrix.CreateLookAt(obj.GetIconViewOffset(value, env), new Vector3(0f, 0f, 0f), Vector3.UnitY);
		float num = 1f;
		Matrix matrix2 = Matrix.CreateOrthographic(3.6f, 3.6f, -10f - 1f * num, 10f - 1f * num);
		if (env.SubsystemTerrain == null)
		{
			_ = BlocksTexturesManager.DefaultBlocksTexture;
		}
		else
		{
			_ = env.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture;
		}
		env.DrawBlockMode = DrawBlockMode.UI;
		env.ViewProjectionMatrix = matrix * matrix2;
		float size = obj.GetIconViewScale(value, env) * 2.2f;
		Matrix matrix3 = Matrix.CreateTranslation(obj.GetIconBlockOffset(value, env));
		obj.DrawBlock(s_primitives, value, Color.White, size, ref matrix3, env);
		RenderTarget2D renderTarget = Display.RenderTarget;
		Display.RenderTarget = rt;
		Display.Clear(Color.Transparent, 1f, 0);
		try
		{
			s_primitives.Flush(Matrix.Identity);
		}
		finally
		{
			Display.RenderTarget = renderTarget;
		}
	}
}
