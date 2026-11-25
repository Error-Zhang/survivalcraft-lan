using Engine;
using Game;

namespace Survivalcraft.Multiplayer;

internal sealed class MapPlayerColors {
    private static readonly Color[] s_playerColors = [
        new(77, 176, 255), new(255, 160, 54), new(115, 223, 101), new(216, 135, 252),
        new(255, 108, 133), new(89, 230, 225), new(249, 219, 92), new(235, 230, 244),
        new(171, 153, 113), new(104, 127, 248), new(241, 143, 189), new(129, 226, 166)
    ];

    public int NextColor { get; private set; }

    public MapPlayerColors(int nextColor) {
        NextColor = Math.Max(0, nextColor);
    }

    public void EnsurePlayer(PlayerData player) {
        if (GameManager.IsNetworkProject) {
            return;
        }
        if (player.MapColorIndex < 0) {
            player.MapColorIndex = NextColor++;
        }
        else {
            NextColor = Math.Max(NextColor, player.MapColorIndex + 1);
        }
    }

    public static Color PlayerColor(PlayerData player) => ColorForIndex(player.MapColorIndex >= 0 ? player.MapColorIndex : player.PlayerIndex);

    private static Color ColorForIndex(int index) {
        index = Math.Max(index, 0);
        if (index < s_playerColors.Length) {
            return s_playerColors[index];
        }
        Vector3 rgb = Color.HsvToRgb(new Vector3((float)((index * 137.507764) % 360), 0.76f, index % 2 == 0 ? 0.96f : 0.79f));
        return new Color(rgb.X, rgb.Y, rgb.Z);
    }
}
