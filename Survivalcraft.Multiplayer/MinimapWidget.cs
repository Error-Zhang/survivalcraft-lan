using Engine;
using Engine.Graphics;
using Engine.Input;
using Game;
using Game.Network;

namespace Survivalcraft.Multiplayer;

internal sealed class MinimapWidget : Widget {
    private const int Samples = 58;
    private const int Cell = 2;
    private const int CacheRange = Samples * Cell;
    private const int VisibleRange = 96;
    private const int CircleSegments = 48;
    private const float HudSize = 142f;
    private const float FrameWidth = 3f;
    private const float CompassMargin = 9f;
    private const float PlayerDotRadius = 3.4f;
    private static readonly Color s_unloaded = new(32, 40, 36);
    private static readonly Color s_border = new(124, 132, 91);
    private static readonly Color s_dark = new(17, 23, 19);
    private static readonly Vector2 s_worldNorth = new(0f, -1f);
    private static readonly (string Letter, Vector2 Direction)[] s_cardinals = [
        ("N", s_worldNorth), ("E", Vector2.UnitX), ("S", Vector2.UnitY), ("W", -Vector2.UnitX)
    ];
    private static readonly Vector2[] s_circle = Enumerable.Range(0, CircleSegments + 1)
        .Select(i => new Vector2(MathF.Sin(i * 2f * MathF.PI / CircleSegments), -MathF.Cos(i * 2f * MathF.PI / CircleSegments)))
        .ToArray();
    private static bool s_visible = true;

    private readonly GameWidget m_gameWidget;
    private readonly Terrain m_terrain;
    private readonly SubsystemPlayers m_players;
    private readonly MapPlayerColors m_playerColors;
    private Color[] m_pixels = new Color[Samples * Samples];
    private Color[] m_nextPixels = new Color[Samples * Samples];
    private Texture2D m_texture;
    private Point2 m_origin;
    private double m_nextSample;
    private Vector2 m_forward = s_worldNorth;

    public MinimapWidget(GameWidget gameWidget) {
        m_gameWidget = gameWidget;
        m_terrain = gameWidget.SubsystemGameWidgets.Project.FindSubsystem<SubsystemTerrain>()?.Terrain;
        m_players = gameWidget.SubsystemGameWidgets.Project.FindSubsystem<SubsystemPlayers>();
        m_playerColors = gameWidget.SubsystemGameWidgets.Project.FindSubsystem<SubsystemNetwork>()?.PlayerColors;
        Name = "Minimap";
        IsHitTestVisible = false;
        ClampToBounds = true;
        HorizontalAlignment = WidgetAlignment.Near;
        VerticalAlignment = WidgetAlignment.Near;
        Margin = new Vector2(10f);
    }

    public override void Update() {
        PlayerData player = m_gameWidget.PlayerData;
        if (m_terrain == null || player?.ComponentPlayer == null || !player.IsMainPlayer) {
            return;
        }
        ComponentPlayer component = player.ComponentPlayer;
        m_playerColors?.EnsurePlayer(player);
        if (component.ComponentInput.GameplayInputBlocked || component.ComponentGui.ModalPanelWidget != null
            || DialogsManager.Dialogs.Any(dialog => dialog.IsChildWidgetOf(ScreensManager.CurrentScreen))) {
            return;
        }
        if (Input.IsKeyDownOnce(Key.M)) {
            s_visible = !s_visible;
        }
    }

    public override void MeasureOverride(Vector2 parentAvailableSize) {
        IsDrawRequired = s_visible && m_gameWidget.PlayerData?.ComponentPlayer != null && m_terrain != null;
        DesiredSize = IsDrawRequired ? new Vector2(HudSize + 2f * CompassMargin) : Vector2.Zero;
    }

    public override void Arrange(Vector2 position, Vector2 parentActualSize) {
        if (ParentWidget is ContainerWidget parent
            && parent.Children.Find<ContainerWidget>("LeftControlsContainer", false) is { IsVisible: true } controls) {
            float inset = Math.Max(0f, controls.ActualSize.X + Math.Min(0f, controls.RenderTransform.M41));
            if (inset > 0f && position.X + inset + 4f + DesiredSize.X <= parent.ActualSize.X) {
                position.X += inset + 4f;
            }
        }
        base.Arrange(position, parentActualSize);
    }

    public override void Draw(DrawContext dc) {
        PlayerData local = m_gameWidget.PlayerData;
        float radius = Math.Min(ActualSize.X, ActualSize.Y) / 2f - CompassMargin;
        if (local?.ComponentPlayer == null || m_terrain == null || radius < 20f) {
            return;
        }

        ComponentBody body = local.ComponentPlayer.ComponentBody;
        Vector3 bodyPosition = body.Position;
        Vector3 bodyForward = body.Matrix.Forward;
        Vector2 forward = new(bodyForward.X, bodyForward.Z);
        if (forward.LengthSquared() >= 0.001f) {
            m_forward = Vector2.Normalize(forward);
        }
        Vector2 mapRight = new(-m_forward.Y, m_forward.X);
        Vector2 mapDown = -m_forward;
        Vector2 worldCenter = new(bodyPosition.X, bodyPosition.Z);
        Vector2 cacheCenter = new(m_origin.X + CacheRange / 2f, m_origin.Y + CacheRange / 2f);
        if (m_texture == null || Time.RealTime >= m_nextSample || (worldCenter - cacheCenter).LengthSquared() > 49f) {
            RefreshTerrain(worldCenter);
        }

        Vector2 center = new(radius + CompassMargin);
        float contentRadius = radius - FrameWidth;
        Color tint = GlobalColorTransform;
        FlatBatch2D frame = dc.PrimitivesRenderer2D.FlatBatch(0, DepthStencilState.None, RasterizerState.CullNoneScissor, BlendState.AlphaBlend);
        int frameStart = frame.TriangleVertices.Count;
        frame.QueueDisc(center, new Vector2(radius), 0f, s_dark * tint, CircleSegments);
        frame.QueueDisc(center, new Vector2(radius - 1.3f), 0f, s_border * tint, CircleSegments);
        frame.TransformTriangles(GlobalTransform, frameStart);

        TexturedBatch2D terrain = dc.PrimitivesRenderer2D.TexturedBatch(
            m_texture, false, 1, DepthStencilState.None, RasterizerState.CullNoneScissor, BlendState.AlphaBlend, SamplerState.PointClamp);
        int terrainStart = terrain.TriangleVertices.Count;
        Vector2 uvCenter = (worldCenter - new Vector2(m_origin.X, m_origin.Y)) / CacheRange;
        float uvRadius = VisibleRange / (2f * CacheRange);
        for (int i = 0; i < CircleSegments; i++) {
            Vector2 a = s_circle[i];
            Vector2 b = s_circle[i + 1];
            Vector2 worldA = a.X * mapRight + a.Y * mapDown;
            Vector2 worldB = b.X * mapRight + b.Y * mapDown;
            terrain.QueueTriangle(center, center + a * contentRadius, center + b * contentRadius, 0f,
                uvCenter, uvCenter + worldA * uvRadius, uvCenter + worldB * uvRadius, tint);
        }
        terrain.TransformTriangles(GlobalTransform, terrainStart);

        FlatBatch2D markers = dc.PrimitivesRenderer2D.FlatBatch(2, DepthStencilState.None, RasterizerState.CullNoneScissor, BlendState.AlphaBlend);
        int markerStart = markers.TriangleVertices.Count;
        float pixelsPerBlock = 2f * contentRadius / VisibleRange;
        if (m_players != null) {
            foreach (PlayerData player in m_players.PlayersData) {
                if (player == local || player.ComponentPlayer == null || player.IsDead) {
                    continue;
                }
                Vector3 teammate = player.ComponentPlayer.ComponentBody.Position;
                Vector2 delta = new Vector2(teammate.X, teammate.Z) - worldCenter;
                Vector2 offset = new Vector2(Vector2.Dot(delta, mapRight), Vector2.Dot(delta, mapDown)) * pixelsPerBlock;
                Vector2 point = center + ClampPlayerMarker(offset, contentRadius - PlayerDotRadius - 1f);
                markers.QueueDisc(point, new Vector2(PlayerDotRadius + 1f), 0f, s_dark * tint, 12);
                markers.QueueDisc(point, new Vector2(PlayerDotRadius), 0f, MapPlayerColors.PlayerColor(player) * tint, 12);
            }
        }

        markers.QueueTriangle(center + new Vector2(0f, -9f), center + new Vector2(5.5f, 5.5f),
            center + new Vector2(-5.5f, 5.5f), 0f, s_dark * tint);
        markers.QueueTriangle(center + new Vector2(0f, -7f), center + new Vector2(3.75f, 4f),
            center + new Vector2(-3.75f, 4f), 0f, Color.White * tint);
        markers.TransformTriangles(GlobalTransform, markerStart);
        DrawCompass(dc, center, radius + 1f, mapRight, mapDown, tint);
    }

    private void DrawCompass(DrawContext dc, Vector2 center, float radius, Vector2 mapRight, Vector2 mapDown, Color tint) {
        FontBatch2D batch = dc.PrimitivesRenderer2D.FontBatch(LabelWidget.BitmapFont, 3);
        int start = batch.TriangleVertices.Count;
        foreach ((string letter, Vector2 direction) in s_cardinals) {
            Vector2 offset = new(Vector2.Dot(direction, mapRight), Vector2.Dot(direction, mapDown));
            batch.QueueText(letter, center + offset * radius, 0f, Color.White * tint, TextAnchor.Center, new Vector2(0.62f));
        }
        batch.TransformTriangles(GlobalTransform, start);
    }

    private static Vector2 ClampPlayerMarker(Vector2 offset, float radius) {
        float distanceSquared = offset.LengthSquared();
        if (distanceSquared > radius * radius) {
            offset *= radius / MathF.Sqrt(distanceSquared);
        }
        return offset;
    }

    private void RefreshTerrain(Vector2 worldCenter) {
        m_nextSample = Time.RealTime + 0.28;
        Point2 oldOrigin = m_origin;
        Point2 origin = new(Terrain.ToCell(worldCenter.X / Cell) * Cell - CacheRange / 2,
            Terrain.ToCell(worldCenter.Y / Cell) * Cell - CacheRange / 2);
        for (int row = 0; row < Samples; row++) {
            for (int column = 0; column < Samples; column++) {
                int x = origin.X + column * Cell + Cell / 2;
                int z = origin.Y + row * Cell + Cell / 2;
                Color color;
                if (!MapTerrain.TrySample(m_terrain, x, z, out color)) {
                    int oldX = x - oldOrigin.X;
                    int oldZ = z - oldOrigin.Y;
                    if (m_texture != null && oldX >= 0 && oldZ >= 0 && oldX < CacheRange && oldZ < CacheRange) {
                        color = m_pixels[oldZ / Cell * Samples + oldX / Cell];
                    }
                    else {
                        color = s_unloaded;
                    }
                }
                m_nextPixels[row * Samples + column] = color;
            }
        }

        (m_pixels, m_nextPixels) = (m_nextPixels, m_pixels);
        m_origin = origin;
        m_texture ??= new Texture2D(Samples, Samples, 1, ColorFormat.Rgba8888);
        m_texture.SetData(0, m_pixels);
    }

    public override void Dispose() {
        Utilities.Dispose(ref m_texture);
        base.Dispose();
    }
}
