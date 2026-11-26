using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemCameraGameWidgets : Subsystem
{
	public SubsystemGameWidgets m_subsystemGameWidgets;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemSky m_subsystemSky;

	public SubsystemDrawing m_subsystemDrawing;

	public readonly HashSet<GameWidget> m_gameWidgets = new HashSet<GameWidget>();

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemGameWidgets = Project.FindSubsystem<SubsystemGameWidgets>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemSky = Project.FindSubsystem<SubsystemSky>(throwOnError: true);
		m_subsystemDrawing = Project.FindSubsystem<SubsystemDrawing>(throwOnError: true);
		int num = Project.m_subsystems.IndexOf(m_subsystemGameWidgets);
		Project.m_subsystems.Remove(this);
		Project.m_subsystems.Insert(num - 1, this);
	}

	public GameWidget AddGameWidget(Matrix viewMatrix, Matrix projectionMatrix, out PerspectiveCamera camera)
	{
		int num = new Game.Random().Int(4, int.MaxValue);
		while (m_subsystemTerrain.TerrainUpdater.m_pendingLocations.ContainsKey(num))
		{
			num = new Game.Random().Int(4, int.MaxValue);
		}
		GameWidget gameWidget = new GameWidget(new PlayerData(Project)
		{
			PlayerIndex = num
		}, 0);
		m_subsystemGameWidgets.m_gameWidgets.Add(gameWidget);
		m_gameWidgets.Add(gameWidget);
		camera = new PerspectiveCamera(gameWidget, viewMatrix, projectionMatrix);
		gameWidget.m_activeCamera = camera;
		int num2 = 1;
		List<Camera> list = new List<Camera>(num2);
		CollectionsMarshal.SetCount(list, num2);
		Span<Camera> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = camera;
		gameWidget.m_cameras = list;
		m_subsystemTerrain.TerrainUpdater.SetUpdateLocation(gameWidget.PlayerData.PlayerIndex, viewMatrix.Translation.XZ, MathUtils.Min(m_subsystemTerrain.m_subsystemsky.VisibilityRange, 64f), 64f);
		return gameWidget;
	}

	public void RemoveGameWidget(GameWidget gameWidget, PerspectiveCamera camera)
	{
		m_subsystemTerrain.TerrainUpdater.RemoveUpdateLocation(gameWidget.PlayerData.PlayerIndex);
		m_subsystemGameWidgets.m_gameWidgets.Remove(gameWidget);
		m_gameWidgets.Remove(gameWidget);
		gameWidget.Dispose();
		Utilities.Dispose(ref camera.ViewTexture);
	}

	public void DrawView(PerspectiveCamera camera, bool setUpdateLocation)
	{
		camera.PrepareForDrawing();
		Vector3 viewPosition = camera.ViewPosition;
		if (setUpdateLocation)
		{
			m_subsystemTerrain.TerrainUpdater.SetUpdateLocation(camera.GameWidget.PlayerData.PlayerIndex, viewPosition.XZ, MathUtils.Min(m_subsystemTerrain.m_subsystemsky.VisibilityRange, 64f), 64f);
		}
		TerrainChunk[] allocatedChunks = m_subsystemTerrain.Terrain.AllocatedChunks;
		foreach (TerrainChunk terrainChunk in allocatedChunks)
		{
			if (camera.ViewPosition.XZ.DistanceSquared(terrainChunk.Center) <= MathUtils.Sqr(m_subsystemSky.VisibilityRange) && terrainChunk.State == TerrainChunkState.Valid)
			{
				terrainChunk.HazeEnds[0] = float.MaxValue;
			}
		}
		RenderTarget2D renderTarget = Display.RenderTarget;
		Display.RenderTarget = camera.ViewTexture;
		Display.Clear(Color.Black, 1f, 0);
		try
		{
			m_subsystemDrawing.Draw(camera);
		}
		finally
		{
			Display.RenderTarget = renderTarget;
		}
	}

	public override void Dispose()
	{
		foreach (GameWidget gameWidget in m_gameWidgets)
		{
			m_subsystemGameWidgets.m_gameWidgets.Remove(gameWidget);
		}
	}
}
