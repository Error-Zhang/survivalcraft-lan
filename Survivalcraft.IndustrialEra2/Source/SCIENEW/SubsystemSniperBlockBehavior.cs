using Engine;
using Game;
using SCIENEW.Extensions;
using SCIENEW.Utils;

namespace SCIENEW;

public class SubsystemSniperBlockBehavior : SubsystemMusketBlockBehavior
{
	public SubsystemGameInfo m_subsystemGameInfo;

	public override bool OnAim(Ray3 aim, ComponentMiner componentMiner, AimState state)
	{
		Vector3 position = aim.Position;
		Vector3 vector = aim.Direction;
		IInventory inventory = componentMiner.Inventory;
		if (inventory != null)
		{
			int activeSlotIndex = inventory.ActiveSlotIndex;
			if (activeSlotIndex >= 0)
			{
				int slotValue = inventory.GetSlotValue(activeSlotIndex);
				int slotCount = inventory.GetSlotCount(activeSlotIndex);
				int num = Terrain.ExtractContents(slotValue);
				int data = Terrain.ExtractData(slotValue);
				int value = slotValue;
				Vector3 vector2 = vector;
				if (num == SniperBlock.Index && slotCount > 0)
				{
					if (m_subsystemTerrain.Raycast(position, position + vector * 1f, useInteractionBoxes: true, skipAirBlocks: true, null).HasValue && IESettingsManager.Get<bool>("SniperMode"))
					{
						GameWidget gameWidget = componentMiner.ComponentPlayer.GameWidget;
						gameWidget.ActiveCamera = gameWidget.FindCamera<FppCamera>();
						componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage(LanguageUtils.GetText(this, "Blocked"), Color.White, blinking: true, playNotificationSound: true);
						return false;
					}
					if (componentMiner.ComponentPlayer.GameWidget.ActiveCamera is IScalableCamera scalableCamera)
					{
						vector = scalableCamera.GetDirection().Normalize();
					}
					if (!m_aimStartTimes.TryGetValue(componentMiner, out var value2))
					{
						value2 = m_subsystemTime.GameTime;
						m_aimStartTimes[componentMiner] = value2;
					}
					float num2 = (float)(m_subsystemTime.GameTime - value2);
					float num3 = (float)MathUtils.Remainder(m_subsystemTime.GameTime, 1000.0);
					_ = (float)((componentMiner.ComponentCreature.ComponentBody.IsSneaking ? 0.00999999977648258 : 0.0299999993294477) + 0.200000002980232 * (double)MathUtils.Saturate((num2 - 6.5f) / 40f)) * new Vector3
					{
						X = SimplexNoise.OctavedNoise(num3, 2f, 3, 2f, 0.5f),
						Y = SimplexNoise.OctavedNoise(num3 + 100f, 2f, 3, 2f, 0.5f),
						Z = SimplexNoise.OctavedNoise(num3 + 200f, 2f, 3, 2f, 0.5f)
					};
					vector = vector.Normalize();
					switch (state)
					{
					case AimState.InProgress:
					{
						if (num2 >= 50f)
						{
							componentMiner.ComponentCreature.ComponentCreatureSounds.PlayMoanSound();
							return true;
						}
						GameWidget gameWidget4 = componentMiner.ComponentPlayer.GameWidget;
						if (num2 > 0.2f && !(gameWidget4.ActiveCamera is IScalableCamera))
						{
							GameWidget gameWidget5 = gameWidget4;
							Camera activeCamera;
							if (gameWidget4.ActiveCamera is IScalableCamera)
							{
								activeCamera = gameWidget4.FindCamera<FppCamera>();
							}
							else
							{
								activeCamera = (IESettingsManager.Get<bool>("SniperMode") ? ((Camera)new TelescopeCamera_ScaleView(gameWidget4)) : ((Camera)new TelescopeCamera_MoveView(gameWidget4)));
							}
							gameWidget5.ActiveCamera = activeCamera;
						}
						ComponentFirstPersonModel componentFirstPersonModel = componentMiner.Entity.FindComponent<ComponentFirstPersonModel>();
						if (componentFirstPersonModel != null)
						{
							ComponentPlayer componentPlayer = componentMiner.ComponentPlayer;
							if (componentPlayer != null)
							{
								componentPlayer?.ComponentAimingSights.ShowAimingSights(aim.Position, vector);
							}
							componentFirstPersonModel.ItemOffsetOrder = new Vector3(-0.21f, 0.15f, 0.08f);
							componentFirstPersonModel.ItemRotationOrder = new Vector3(-0.7f, 0f, 0f);
						}
						componentMiner.ComponentCreature.ComponentCreatureModel.AimHandAngleOrder = 1.4f;
						componentMiner.ComponentCreature.ComponentCreatureModel.InHandItemOffsetOrder = new Vector3(-0.08f, -0.08f, 0.07f);
						componentMiner.ComponentCreature.ComponentCreatureModel.InHandItemRotationOrder = new Vector3(-1.7f, 0f, 0f);
						break;
					}
					case AimState.Cancelled:
					{
						GameWidget gameWidget6 = componentMiner.ComponentPlayer.GameWidget;
						GameWidget gameWidget7 = gameWidget6;
						Camera activeCamera2;
						if (gameWidget6.ActiveCamera is IScalableCamera)
						{
							activeCamera2 = gameWidget6.FindCamera<FppCamera>();
						}
						else
						{
							activeCamera2 = (IESettingsManager.Get<bool>("SniperMode") ? ((Camera)new TelescopeCamera_ScaleView(gameWidget6)) : ((Camera)new TelescopeCamera_MoveView(gameWidget6)));
						}
						gameWidget7.ActiveCamera = activeCamera2;
						if (RifleBlock.GetHammerState(Terrain.ExtractData(value)))
						{
							value = Terrain.MakeBlockValue(num, 0, RifleBlock.SetHammerState(Terrain.ExtractData(value), state: false));
							m_subsystemAudio.PlaySound("Audio/HammerUncock", 1f, m_random.Float(-0.1f, 0.1f), 0f, 0f);
						}
						m_aimStartTimes.Remove(componentMiner);
						break;
					}
					case AimState.Completed:
					{
						Vector3 vector3 = new Vector3(0f, 0f, 0f);
						GameWidget gameWidget2 = componentMiner.ComponentPlayer.GameWidget;
						if (gameWidget2.ActiveCamera is IScalableCamera scalableCamera2)
						{
							vector3 = scalableCamera2.GetDirection();
							gameWidget2.ActiveCamera = gameWidget2.FindCamera<FppCamera>();
						}
						bool flag = false;
						int value3 = 0;
						int num4 = 0;
						float num5 = 0f;
						vector2 = vector3.Normalize();
						Vector3 vector4 = Vector3.Zero;
						RifleBlock.GetLoadState(data);
						if (SniperBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(activeSlotIndex))) > 0)
						{
							flag = true;
							value3 = ItemsBlockManager.GetItemID("LeadBullet_Sniper");
							num5 = 320f;
							num4 = 1;
							vector4 = new Vector3(0.0001f, 0.0001f, 0.0001f);
						}
						if (flag)
						{
							Vector3 eyePosition = componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition;
							Matrix matrix = componentMiner.ComponentCreature.ComponentBody.Matrix;
							Vector3 vector5 = eyePosition + matrix.Right * 0.3f - matrix.Up * 0.2f;
							Vector3 vector6 = Vector3.Normalize(vector5 + vector * 10f - vector5);
							Vector3 vector7 = Vector3.Normalize(vector);
							Vector3 vector8 = Vector3.Normalize(vector6.Cross(Vector3.UnitY));
							Vector3 vector9 = Vector3.Normalize(vector6.Cross(vector8));
							for (int i = 0; i < num4; i++)
							{
								Vector3 vector10 = m_random.Float(0f - vector4.X, vector4.X) * vector8 + m_random.Float(0f - vector4.Y, vector4.Y) * vector9 + m_random.Float(0f - vector4.Z, vector4.Z) * vector6;
								Projectile projectile = m_subsystemProjectiles.FireProjectile(value3, vector5 + 1.3f * vector2, num5 * (vector7 + vector10), Vector3.Zero, componentMiner.ComponentCreature);
								if (projectile != null)
								{
									projectile.ProjectileStoppedAction = ProjectileStoppedAction.Disappear;
								}
							}
							m_subsystemAudio.PlaySound("Audio/MusketFire", 1f, m_random.Float(-0.1f, 0.1f), componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition, 10f, autoDelay: true);
							m_subsystemParticles.AddParticleSystem(new GunSmokeParticleSystem2(SubsystemTerrain, vector5 + 1.3f * vector2, vector2));
							m_subsystemNoise.MakeNoise(vector5, 1f, 60f);
							int bulletNum = SniperBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(activeSlotIndex)));
							componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage((bulletNum - 1).ToString(), Color.White, blinking: true, playNotificationSound: false);
							inventory.RemoveSlotItems(activeSlotIndex, 1);
							inventory.AddSlotItems(activeSlotIndex, Terrain.MakeBlockValue(SniperBlock.Index, 0, SniperBlock.SetBulletNum(bulletNum - 1)), 1);
						}
						GameWidget gameWidget3 = componentMiner.ComponentPlayer.GameWidget;
						gameWidget3.ActiveCamera = gameWidget3.FindCamera<FppCamera>();
						m_aimStartTimes.Remove(componentMiner);
						break;
					}
					}
				}
			}
		}
		return false;
	}

	public override int GetProcessInventoryItemCapacity(IInventory inventory, int slotIndex, int value)
	{
		if (value == ItemsBlockManager.GetItemID("StandardBullet") && SniperBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(slotIndex))) <= 9)
		{
			return 1;
		}
		return 0;
	}

	public override void ProcessInventoryItem(IInventory inventory, int slotIndex, int value, int count, int processCount, out int processedValue, out int processedCount)
	{
		processedValue = value;
		processedCount = count;
		int bulletNum = SniperBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(slotIndex)));
		if (processCount + bulletNum < 11 && processCount == 1)
		{
			processedValue = 0;
			processedCount = 0;
			inventory.RemoveSlotItems(slotIndex, 1);
			inventory.AddSlotItems(slotIndex, Terrain.MakeBlockValue(SniperBlock.Index, 0, SniperBlock.SetBulletNum(MathUtils.Min(bulletNum + processCount, 10))), 1);
		}
	}
}
