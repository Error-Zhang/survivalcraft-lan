using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;
using Game.Network;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentTurretTargeting : Component
{
	public SubsystemBodies m_subsystemBodies;

	public SubsystemMovingBlocks m_subsystemMovingBlocks;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemPlayers m_subsystemPlayers;

	public ComponentBlockEntity m_componentBlockEntity;

	private List<string> m_trustedPlayers = new List<string>();

	public string OwnerName { get; set; } = "";

	public bool AttackCreatures { get; set; } = true;

	public bool AttackPlayers { get; set; }

	public bool PreventFriendlyFire { get; set; } = true;

	public IReadOnlyList<string> TrustedPlayers => m_trustedPlayers;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
		m_subsystemMovingBlocks = Project.FindSubsystem<SubsystemMovingBlocks>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		OwnerName = valuesDictionary.GetValue("OwnerName", "");
		AttackCreatures = valuesDictionary.GetValue("AttackCreatures", defaultValue: true);
		AttackPlayers = valuesDictionary.GetValue("AttackPlayers", defaultValue: false);
		PreventFriendlyFire = valuesDictionary.GetValue("PreventFriendlyFire", defaultValue: true);
		m_trustedPlayers = ParseTrustedPlayers(valuesDictionary.GetValue("TrustedPlayers", ""));
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("OwnerName", OwnerName ?? "");
		valuesDictionary.SetValue("AttackCreatures", AttackCreatures);
		valuesDictionary.SetValue("AttackPlayers", AttackPlayers);
		valuesDictionary.SetValue("PreventFriendlyFire", PreventFriendlyFire);
		valuesDictionary.SetValue("TrustedPlayers", SerializeTrustedPlayers(m_trustedPlayers));
	}

	internal void SerializeSettings(PacketSerializer archive)
	{
		string owner = OwnerName ?? string.Empty;
		string trusted = archive.IsWriting ? SerializeTrustedPlayers(m_trustedPlayers) : string.Empty;
		bool creatures = AttackCreatures;
		bool players = AttackPlayers;
		bool protection = PreventFriendlyFire;
		archive.Value(ref owner);
		archive.Value(ref trusted);
		archive.Value(ref creatures);
		archive.Value(ref players);
		archive.Value(ref protection);
		if (archive.IsReading)
		{
			OwnerName = owner;
			m_trustedPlayers = ParseTrustedPlayers(trusted);
			AttackCreatures = creatures;
			AttackPlayers = players;
			PreventFriendlyFire = protection;
		}
	}

	internal bool ChangeSetting(byte setting, string value)
	{
		switch (setting)
		{
			case 0: AttackCreatures = value == "1"; return true;
			case 1: AttackPlayers = value == "1"; return true;
			case 2: PreventFriendlyFire = value == "1"; return true;
			case 3: return TryAddTrustedPlayer(value);
			case 4: return TryRemoveTrustedPlayer(value);
			default: return false;
		}
	}

	public bool TryAddTrustedPlayer(string name)
	{
		return TryAddTrustedPlayer(m_trustedPlayers, name);
	}

	public bool TryRemoveTrustedPlayer(string name)
	{
		return TryRemoveTrustedPlayer(m_trustedPlayers, name);
	}

	public void TryAssignOwnerFromNearestPlayer()
	{
		if (string.IsNullOrEmpty(OwnerName))
		{
			Vector3 position = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f);
			ComponentPlayer componentPlayer = m_subsystemPlayers.FindNearestPlayer(position);
			if (componentPlayer != null && componentPlayer.PlayerData?.Name != null)
			{
				OwnerName = componentPlayer.PlayerData.Name;
			}
		}
	}

	public bool IsProtected(ComponentBody body)
	{
		if (body == null)
		{
			return false;
		}
		ComponentPlayer componentPlayer = body.Entity.FindComponent<ComponentPlayer>();
		if (componentPlayer == null)
		{
			return false;
		}
		string playerName = componentPlayer.PlayerData?.Name ?? "";
		if (IsPlayerNameProtected(OwnerName, m_trustedPlayers, playerName))
		{
			return true;
		}
		return !AttackPlayers;
	}

	public bool TryAcquireTarget(Vector3 origin, float maxRange, out ComponentBody? targetBody, out Vector3 aimDirection)
	{
		targetBody = null;
		aimDirection = Vector3.UnitZ;
		float num = maxRange * maxRange;
		foreach (ComponentBody body in m_subsystemBodies.Bodies)
		{
			Entity entity = body.Entity;
			if (entity.FindComponent<ComponentCreature>() == null)
			{
				continue;
			}
			ComponentHealth componentHealth = entity.FindComponent<ComponentHealth>();
			if (componentHealth == null || componentHealth.Health <= 0f)
			{
				continue;
			}
			bool flag = entity.FindComponent<ComponentPlayer>() != null;
			bool nameProtected = false;
			if (flag)
			{
				nameProtected = IsPlayerNameProtected(playerName: entity.FindComponent<ComponentPlayer>().PlayerData?.Name ?? "", ownerName: OwnerName, trusted: m_trustedPlayers);
			}
			if (!ShouldAcquireTarget(flag, AttackCreatures, AttackPlayers, nameProtected))
			{
				continue;
			}
			Vector3 vector = body.BoundingBox.Center() - origin;
			if (!(vector.LengthSquared() > num) && !(vector.LengthSquared() < 1E-06f))
			{
				Vector3 vector2 = Vector3.Normalize(vector);
				Vector3 position = origin + vector2 * 1f;
				BodyRaycastResult? bodyRaycastResult = PickBody(position, vector2, 50f);
				TerrainRaycastResult? terrainRaycastResult = PickTerrain(position, vector2, 50f);
				if (bodyRaycastResult.HasValue && (!terrainRaycastResult.HasValue || !(bodyRaycastResult.Value.Distance >= terrainRaycastResult.Value.Distance)) && bodyRaycastResult.Value.ComponentBody == body)
				{
					targetBody = body;
					aimDirection = vector;
					return true;
				}
			}
		}
		return false;
	}

	public bool CanFireSafely(Vector3 muzzle, Vector3 aimDirection, ComponentBody intendedTarget, float maxRange)
	{
		if (!PreventFriendlyFire)
		{
			return true;
		}
		if (intendedTarget == null)
		{
			return false;
		}
		aimDirection = Vector3.Normalize(aimDirection);
		BodyRaycastResult? bodyRaycastResult = PickBody(muzzle, aimDirection, maxRange);
		if (!bodyRaycastResult.HasValue)
		{
			return true;
		}
		ComponentBody componentBody = bodyRaycastResult.Value.ComponentBody;
		if (componentBody == intendedTarget)
		{
			return true;
		}
		if (IsProtected(componentBody))
		{
			return false;
		}
		return true;
	}

	private BodyRaycastResult? PickBody(Vector3 position, Vector3 direction, float distance)
	{
		direction = Vector3.Normalize(direction);
		Vector3 end = position + direction * distance;
		return m_subsystemBodies.Raycast(position, end, 0.35f, (ComponentBody body, float dist) => true);
	}

	private TerrainRaycastResult? PickTerrain(Vector3 position, Vector3 direction, float distance)
	{
		direction = Vector3.Normalize(direction);
		Vector3 end = position + direction * distance;
		MovingBlocksRaycastResult? movingBlocksResult = m_subsystemMovingBlocks.Raycast(position, end, extendToFillCells: true);
		return m_subsystemTerrain.Raycast(position, end, useInteractionBoxes: true, skipAirBlocks: true, (int value, float dist) => (!movingBlocksResult.HasValue || !(dist > movingBlocksResult.Value.Distance)) && Terrain.ExtractContents(value) != GlassBlock.Index);
	}

	internal static List<string> ParseTrustedPlayers(string serialized)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(serialized))
		{
			return list;
		}
		string[] array = serialized.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (text.Length > 0)
			{
				list.Add(text);
			}
		}
		return list;
	}

	internal static string SerializeTrustedPlayers(IEnumerable<string> names)
	{
		return string.Join(";", from n in names
			select n.Trim() into n
			where n.Length > 0
			select n);
	}

	internal static bool TryAddTrustedPlayer(List<string> list, string name)
	{
		name = name?.Trim() ?? "";
		if (name.Length == 0)
		{
			return false;
		}
		if (list.Any((string x) => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)))
		{
			return false;
		}
		list.Add(name);
		return true;
	}

	internal static bool TryRemoveTrustedPlayer(List<string> list, string name)
	{
		name = name?.Trim() ?? "";
		int num = list.FindIndex((string x) => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
		if (num < 0)
		{
			return false;
		}
		list.RemoveAt(num);
		return true;
	}

	internal static bool IsPlayerNameProtected(string ownerName, IReadOnlyList<string> trusted, string playerName)
	{
		if (string.IsNullOrWhiteSpace(playerName))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(ownerName) && string.Equals(ownerName, playerName, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return trusted.Any((string t) => string.Equals(t, playerName, StringComparison.OrdinalIgnoreCase));
	}

	internal static bool ShouldAcquireTarget(bool isPlayer, bool attackCreatures, bool attackPlayers, bool nameProtected)
	{
		if (nameProtected)
		{
			return false;
		}
		if (!isPlayer)
		{
			return attackCreatures;
		}
		return attackPlayers;
	}
}
