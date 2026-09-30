using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Survivalcraft.Test;

public sealed class GunClientInputGameFixture : GameFixture {
    protected override void ConfigureContext(GameContext context) {
        context.InstallMod(Path.Combine(AppContext.BaseDirectory, "Mods", "01-gun-mod-2.8.original.scmod"));
        context.InstallMod(Path.Combine(AppContext.BaseDirectory, "Mods", "Survivalcraft.CompatNet.scmod"));
    }
}

public sealed class GunClientInputRegressionTest : IClassFixture<GunClientInputGameFixture> {
    private readonly GunClientInputGameFixture fixture;
    private GameContext server => fixture.Server;
    private GameContext client => fixture.Client;

    public GunClientInputRegressionTest(GunClientInputGameFixture fixture) {
        this.fixture = fixture;
    }

    [Fact]
    public async Task GunModClientStillReceivesMovementAndLookInput() {
        await GameTools.CreateTestWorld(server, "gun-client-input");
        await GameTools.ConnectTestServer(client, server.GamePort);

        string inputType = (string?)await client.PushCommandAsync(
            "FUNC var p = Game.GameManager.Project.FindSubsystem<Game.SubsystemPlayers>().PlayersData.FirstOrDefault(x => x.IsMainPlayer)?.ComponentPlayer; "
            + "return p?.ComponentInput?.GetType().FullName ?? \"null\";") ?? string.Empty;
        Assert.Contains("Game.ComponentNewInput", inputType);

        string beforePosition = (string?)await client.PushCommandAsync(
            "FUNC return Game.GameManager.Project.FindSubsystem<Game.SubsystemPlayers>().PlayersData.FirstOrDefault(x => x.IsMainPlayer)?.ComponentPlayer?.ComponentBody?.Position.ToString() ?? \"null\";") ?? string.Empty;
        await client.PushCommandAsync("EXEC " + GameTools.ReadScript("Neorxna/ForceWindowActiveCode.cs"));
        await client.PushCommandAsync("MOVE_INPUT 0 0 1 100000");
        await Task.Delay(100);
        string liveInput = (string?)await client.PushCommandAsync(
            "FUNC var p = Game.GameManager.Project.FindSubsystem<Game.SubsystemPlayers>().PlayersData.FirstOrDefault(x => x.IsMainPlayer)?.ComponentPlayer; var c = p?.ComponentMiner?.ComponentCreature; var m = p?.ComponentRider?.Mount; var parent = c?.ComponentBody?.ParentBody; "
            + "var u = Game.GameManager.Project.FindSubsystem<Game.SubsystemUpdate>(); var adapterSkips = string.Join(\",\", Game.CompatNetAdapterRegistry.Adapters.Select(a => a.AdapterName + \":input=\" + a.ShouldSkipClientUpdate(p?.ComponentInput) + \":locomotion=\" + a.ShouldSkipClientUpdate(c?.ComponentLocomotion))); return string.Join(\";\", p?.ComponentInput?.PlayerInput.Move.ToString() ?? \"null\", p?.ComponentInput?.PlayerInput.Look.ToString() ?? \"null\", p?.ComponentLocomotion?.GetType().FullName ?? \"null\", c?.ComponentLocomotion?.GetType().FullName ?? \"null\", c?.ComponentLocomotion?.WalkOrder?.ToString() ?? \"null\", c?.ComponentLocomotion?.LastWalkOrder?.ToString() ?? \"null\", c?.ComponentBody?.Velocity.ToString() ?? \"null\", c?.ComponentLocomotion?.DisableInputMovement, Game.NetworkManager.IsServerAuthoritativeCreatureBody(c?.ComponentBody), u?.m_updateables.ContainsKey(p), u?.m_sortedUpdateables.Contains(p), u?.m_updateables.ContainsKey(c?.ComponentLocomotion), u?.m_sortedUpdateables.Contains(c?.ComponentLocomotion), m != null, m?.GetType().FullName ?? \"null\", parent?.Entity?.Id, p?.ComponentRider?.GetType().FullName ?? \"null\", p?.PlayerData?.InputDevice, Engine.Window.IsActive, Engine.Input.Keyboard.IsKeyDown(Engine.Input.Key.W), p?.GameWidget?.Input?.Devices, p?.GameWidget?.Input?.IsKeyOrMouseDown(\"MoveFront\"), p?.PlayerData?.IsMainPlayer, p?.PlayerData?.IsReadyForPlaying, p?.ComponentInput?.GameplayInputBlocked, p?.GameWidget?.ActiveCamera?.IsEntityControlEnabled, p?.GameWidget?.ActiveCamera?.UsesMovementControls, Game.NetworkManager.PlayerIndex, adapterSkips);") ?? string.Empty;
        await Task.Delay(600);

        string afterMove = (string?)await client.PushCommandAsync(
            "FUNC var p = Game.GameManager.Project.FindSubsystem<Game.SubsystemPlayers>().PlayersData.FirstOrDefault(x => x.IsMainPlayer)?.ComponentPlayer; "
            + "return p?.ComponentBody?.Position.ToString() ?? \"null\";") ?? string.Empty;
        string afterMoveState = (string?)await client.PushCommandAsync(
            "FUNC var p = Game.GameManager.Project.FindSubsystem<Game.SubsystemPlayers>().PlayersData.FirstOrDefault(x => x.IsMainPlayer)?.ComponentPlayer; var c = p?.ComponentMiner?.ComponentCreature; var u = Game.GameManager.Project.FindSubsystem<Game.SubsystemUpdate>(); return string.Join(\";\", p?.ComponentBody?.Position.ToString() ?? \"null\", c?.ComponentBody?.Velocity.ToString() ?? \"null\", c?.ComponentLocomotion?.WalkOrder?.ToString() ?? \"null\", c?.ComponentLocomotion?.LastWalkOrder?.ToString() ?? \"null\", u?.m_sortedUpdateables.Contains(c?.ComponentLocomotion), Engine.Time.FrameIndex);") ?? string.Empty;
        Assert.True(beforePosition != afterMove, $"初始位置: {beforePosition}; 客户端输入状态: {liveInput}; 移动后: {afterMoveState}; 日志: {client.GetRecentLogText(120)}");

        string afterLook = (string?)await client.PushCommandAsync(
            "FUNC var p = Game.GameManager.Project.FindSubsystem<Game.SubsystemPlayers>().PlayersData.FirstOrDefault(x => x.IsMainPlayer)?.ComponentPlayer; var before = p.ComponentLocomotion.LookAngles; p.ComponentInput.m_playerInput.Look = new Vector2(0, 1); p.Update(0.016f); p.ComponentLocomotion.Update(0.016f); return string.Join(\";\", before.ToString(), p.ComponentLocomotion.LookAngles.ToString());") ?? string.Empty;
        Assert.Contains(";", afterLook);
        Assert.True(!afterLook.EndsWith(";0,0", StringComparison.Ordinal), $"视角状态: {afterLook}; 日志: {client.GetRecentLogText(80)}");
    }
}
