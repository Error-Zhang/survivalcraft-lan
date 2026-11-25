using Engine;
using Game;

namespace Survivalcraft.Multiplayer {
    internal static class MapTerrain {
        public static bool TrySample(Terrain terrain, int x, int z, out Color result) {
            TerrainChunk chunk = terrain.GetChunkAtCell(x, z);
            if (chunk == null || chunk.State < TerrainChunkState.InvalidVertices1) {
                result = Color.Black;
                return false;
            }
            int localX = x & 15;
            int localZ = z & 15;
            int height = chunk.GetTopHeightFast(localX, localZ);
            Block block = BlocksManager.Blocks[chunk.GetCellContentsFast(localX, height, localZ)];
            if (block is CrossBlock && height > 0) {
                block = BlocksManager.Blocks[chunk.GetCellContentsFast(localX, height - 1, localZ)];
            }

            Color color = block switch {
                AirBlock => Color.Black,
                WaterBlock => new Color(39, 89, 116),
                MagmaBlock => new Color(200, 86, 32),
                IceBlock => new Color(133, 185, 201),
                SnowBlock => new Color(210, 222, 218),
                SandBlock => new Color(174, 156, 101),
                LeavesBlock => new Color(47, 88, 58),
                GrassBlock => new Color(106, 124, 72),
                WoodBlock => new Color(102, 76, 53),
                DirtBlock or SoilBlock => new Color(137, 108, 69),
                GravelBlock => new Color(133, 132, 121),
                _ => new Color(125, 129, 118)
            };
            int northHeight = localZ > 0 ? chunk.GetTopHeightFast(localX, localZ - 1) : terrain.GetTopHeight(x, z - 1);
            int westHeight = localX > 0 ? chunk.GetTopHeightFast(localX - 1, localZ) : terrain.GetTopHeight(x - 1, z);
            float slope = Math.Clamp(height - (northHeight + westHeight) * 0.5f, -6f, 6f);
            float elevation = Math.Clamp((height - 64f) * 0.003f, -0.08f, 0.14f);
            float shade = block is WaterBlock ? 1f : 1f + 0.035f * slope + elevation;
            result = new Color(
                Math.Clamp((int)(color.R * shade), 0, 255),
                Math.Clamp((int)(color.G * shade), 0, 255),
                Math.Clamp((int)(color.B * shade), 0, 255));
            return true;
        }

    }
}
