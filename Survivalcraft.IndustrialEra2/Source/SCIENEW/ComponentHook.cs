using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.Extensions.GameLogic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentHook : Component, IUpdateable, IDrawable
{
	public struct HookInfo
	{
		public Vector3 StartPosition;

		public Vector3 HookPosition;

		public int HookSlotIndex;
	}

	public ComponentPlayer m_componentPlayer;

	public ComponentBody m_componentBody;

	public HookInfo? m_hookInfo;

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public HookDItem m_hookItem;

	public DrawBlockEnvironmentData m_environmentData = new DrawBlockEnvironmentData();

	private Game.Random m_random = new Game.Random();

	public UpdateOrder UpdateOrder => UpdateOrder.Locomotion;

	public int[] DrawOrders => new int[1] { 1500 };

	public void Hook(TerrainRaycastResult? raycastResult, out float hookDistance)
	{
		hookDistance = 0f;
		if (raycastResult.HasValue)
		{
			if (m_hookInfo.HasValue)
			{
				m_hookInfo = null;
				return;
			}
			m_hookInfo = new HookInfo
			{
				HookPosition = raycastResult.Value.HitPoint(),
				StartPosition = GetPlayerPosition(m_componentBody),
				HookSlotIndex = m_componentPlayer.ComponentMiner.Inventory.ActiveSlotIndex
			};
			AudioManager.PlaySound("Audio/Throw", m_random.Float(0.2f, 0.3f), m_random.Float(-0.2f, 0.2f), 0f);
			hookDistance = (GetPlayerPosition(m_componentBody) - raycastResult.Value.HitPoint()).Length();
		}
		else
		{
			m_hookInfo = null;
		}
	}

	public void Update(float dt)
	{
		if (m_hookInfo.HasValue)
		{
			if (m_componentPlayer.ComponentMiner.Inventory.ActiveSlotIndex == m_hookInfo.Value.HookSlotIndex)
			{
				Vector3 vector = m_hookInfo.Value.HookPosition - GetPlayerPosition(m_componentBody);
				Vector3 vector2 = m_hookInfo.Value.HookPosition - m_hookInfo.Value.StartPosition;
				Vector3 vector3 = (m_hookInfo.Value.HookPosition - GetPlayerPosition(m_componentBody)).Normalize();
				float num = MathF.Sqrt(1f - MathF.Pow(vector.Length() / vector2.Length() - 1f, 2f));
				m_componentBody.ApplyImpulse(num * vector3 * 240f * dt);
			}
			else
			{
				m_hookInfo = null;
			}
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
		m_componentBody = m_componentPlayer.ComponentBody;
		m_hookItem = DamageableItemManager.GetItem<HookDItem>();
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (m_hookInfo.HasValue)
		{
			Vector3 vector = (m_hookInfo.Value.HookPosition - m_hookInfo.Value.StartPosition).Normalize();
			Matrix matrix = Matrix.Invert(Matrix.CreateLookAt(Vector3.Zero, vector, Vector3.UnitY)) * Matrix.CreateTranslation(m_hookInfo.Value.HookPosition - vector * 0.75f);
			m_environmentData.ModifyEnvironmentData(m_componentPlayer.m_subsystemTerrain, m_hookInfo.Value.HookPosition - vector * 0.75f);
			m_hookItem.DrawHook(m_primitivesRenderer3D, Color.White, 0.5f, ref matrix, m_environmentData);
			int num = Math.Max((int)MathF.Floor((m_hookInfo.Value.HookPosition - GetPlayerPosition(m_componentBody)).Length()), 1);
			Vector3 vector2 = new Plane(m_hookInfo.Value.StartPosition, m_hookInfo.Value.HookPosition, m_componentPlayer.ComponentCreatureModel.EyePosition).Normal * (1f / 32f);
			for (int i = 0; i < num; i++)
			{
				Vector3 vector3 = GetPlayerPosition(m_componentBody).Lerp(m_hookInfo.Value.HookPosition, (float)i / (float)num);
				Vector3 vector4 = GetPlayerPosition(m_componentBody).Lerp(m_hookInfo.Value.HookPosition, (float)(i + 1) / (float)num);
				Vector3 p = vector3 - vector2;
				Vector3 p2 = vector3 + vector2;
				Vector3 p3 = vector4 + vector2;
				Vector3 p4 = vector4 - vector2;
				m_environmentData.ModifyEnvironmentData(m_componentPlayer.m_subsystemTerrain, (vector3 + vector4) / 2f);
				float s = LightingManager.LightIntensityByLightValue[m_environmentData.Light];
				Color c = new Color(194, 190, 170);
				m_primitivesRenderer3D.FlatBatch().QueueQuad(p, p2, p3, p4, Color.MultiplyColorOnly(c, s));
			}
			m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
		}
	}

	public Vector3 GetPlayerPosition(ComponentBody componentBody)
	{
		return componentBody.Position + new Vector3(0f, 1f, 0f);
	}
}
