using System;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Utils;

namespace SCIENEW.Extensions.Graphics;

public static class DrawingExtensions
{
	extension(ModelShader modelShader)
	{
		public void PrepareModelShaderState(Camera camera, SubsystemSky subsystemSky, SubsystemModelsRenderer.ModelData modelData, Texture2D texture, float? alphaThreshold)
		{
			modelShader.LightDirection1 = -LightingManager.DirectionToLight1.TransformNormal(camera.ViewMatrix);
			modelShader.LightDirection2 = -LightingManager.DirectionToLight2.TransformNormal(camera.ViewMatrix);
			modelShader.FogColor = new Vector3(subsystemSky.ViewFogColor);
			modelShader.FogBottomTopDensity = new Vector3(subsystemSky.ViewFogBottom - camera.ViewPosition.Y, subsystemSky.ViewFogTop - camera.ViewPosition.Y, subsystemSky.ViewFogDensity);
			modelShader.HazeStartDensity = new Vector2(subsystemSky.ViewHazeStart, subsystemSky.ViewHazeDensity);
			modelShader.FogYMultiplier = subsystemSky.VisibilityRangeYMultiplier;
			modelShader.WorldUp = Vector3.UnitY.TransformNormal(camera.ViewMatrix);
			modelShader.Transforms.View = Matrix.Identity;
			modelShader.Transforms.Projection = camera.ProjectionMatrix;
			modelShader.SamplerState = SamplerState.PointClamp;
			if (alphaThreshold.HasValue)
			{
				modelShader.AlphaThreshold = alphaThreshold.Value;
			}
			ComponentModel componentModel = modelData.ComponentModel;
			Vector3 vector = componentModel.DiffuseColor ?? Vector3.One;
			float num = componentModel.Opacity ?? 1f;
			modelShader.InstancesCount = componentModel.AbsoluteBoneTransformsForCamera.Length;
			modelShader.MaterialColor = new Vector4(vector * num, num);
			modelShader.EmissionColor = componentModel.EmissionColor ?? Vector4.Zero;
			modelShader.AmbientLightColor = new Vector3(LightingManager.LightAmbient * modelData.Light);
			modelShader.DiffuseLightColor1 = new Vector3(modelData.Light);
			modelShader.DiffuseLightColor2 = new Vector3(modelData.Light);
			modelShader.Texture = texture;
			Array.Copy(componentModel.AbsoluteBoneTransformsForCamera, modelShader.Transforms.World, componentModel.AbsoluteBoneTransformsForCamera.Length);
		}

		public void PrepareModelShaderFog(SubsystemSky subsystemSky, Camera camera)
		{
			modelShader.FogBottomTopDensity = new Vector3(subsystemSky.ViewFogBottom - camera.ViewPosition.Y, subsystemSky.ViewFogTop - camera.ViewPosition.Y, subsystemSky.ViewFogDensity);
			modelShader.HazeStartDensity = new Vector2(subsystemSky.ViewHazeStart, subsystemSky.ViewHazeDensity);
		}
	}

	public static bool IsVisible(this SubsystemTerrain subsystemTerrain, Camera camera, Vector3 pos)
	{
		float visibilityRange = subsystemTerrain.m_subsystemsky.VisibilityRange;
		if (camera.ViewPosition.Distance(pos) < visibilityRange - 16f)
		{
			return true;
		}
		return false;
	}

	public static void QueueBoundingBoxFace(this FlatBatch3D batch, BoundingBox boundingBox, Color color)
	{
		DrawingUtils.DrawBoundingBoxFace(batch, boundingBox.Min, boundingBox.Max, color);
	}
}
