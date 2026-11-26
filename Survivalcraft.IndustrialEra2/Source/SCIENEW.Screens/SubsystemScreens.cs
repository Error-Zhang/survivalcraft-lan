using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Serialization;
using Game;
using Game.Network;
using GameEntitySystem;
using SCIENEW.Extensions.Graphics;
using TemplatesDatabase;

namespace SCIENEW.Screens;

public class SubsystemScreens : Subsystem, IDrawable
{
	private struct PendingScreenLink
	{
		public int ProviderEntityId;

		public int ContainerEntityId;

		public string ProviderTypeName;

		public string ContainerTypeName;
	}

	public PrimitivesRenderer3D m_p3d = new PrimitivesRenderer3D();

	private FlatBatch3D m_flatBath;

	public bool m_debugCanDraw;

	public SubsystemTerrain m_subsystemTerrain;

	protected BiDictionary<IScreenProviderComponent, IScreenContainerComponent> m_screenLinks = new BiDictionary<IScreenProviderComponent, IScreenContainerComponent>();

	public Dictionary<Entity, IScreenProviderComponent[]> m_providerEntity = new Dictionary<Entity, IScreenProviderComponent[]>();

	public Dictionary<Entity, IScreenContainerComponent[]> m_containerEntity = new Dictionary<Entity, IScreenContainerComponent[]>();

	private List<PendingScreenLink> m_pendingLinks = new List<PendingScreenLink>();

	public static Color ContainerColor => new Color(137, 255, 128);

	public static Color ProviderColor => new Color(255, 253, 107);

	public static Color LinkColor => new Color(0, 255, 255);

	public int[] DrawOrders => new int[1] { 1000 };

	public BiDictionary<IScreenProviderComponent, IScreenContainerComponent> GetScreensMap()
	{
		return m_screenLinks;
	}

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_flatBath = m_p3d.FlatBatch(0, DepthStencilState.None);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_pendingLinks = ParsePendingLinks(valuesDictionary, null);
		if (m_pendingLinks.Count > 0)
		{
			Project.OnProjectLoad += OnProjectLoadedRestoreLinks;
		}
	}

	public override void Save(ValuesDictionary valuesDictionary)
	{
		base.Save(valuesDictionary);
		valuesDictionary.SetValue("LinksCount", m_screenLinks.Count);
		int num = 0;
		foreach (KeyValuePair<IScreenProviderComponent, IScreenContainerComponent> screenLink in m_screenLinks)
		{
			ValuesDictionary valuesDictionary2 = new ValuesDictionary();
			valuesDictionary2.SetValue("ProviderEntityId", screenLink.Key.GetEntity().Id);
			valuesDictionary2.SetValue("ContainerEntityId", screenLink.Value.GetEntity().Id);
			valuesDictionary2.SetValue("Provider", screenLink.Key.GetType().ToString());
			valuesDictionary2.SetValue("Container", screenLink.Value.GetType().ToString());
			valuesDictionary.SetValue("Entity" + num.ToString(CultureInfo.InvariantCulture), valuesDictionary2);
			num++;
		}
	}

	public void ImportLegacyLinks(ValuesDictionary valuesDictionary, Entity loaderEntity)
	{
		foreach (PendingScreenLink item in ParsePendingLinks(valuesDictionary, loaderEntity))
		{
			TryRestoreLink(item);
		}
	}

	private void OnProjectLoadedRestoreLinks(Project project)
	{
		Project.OnProjectLoad -= OnProjectLoadedRestoreLinks;
		foreach (PendingScreenLink pendingLink in m_pendingLinks)
		{
			TryRestoreLink(pendingLink);
		}
		m_pendingLinks = new List<PendingScreenLink>();
	}

	private List<PendingScreenLink> ParsePendingLinks(ValuesDictionary valuesDictionary, Entity? loaderEntity)
	{
		List<PendingScreenLink> list = new List<PendingScreenLink>();
		int value = valuesDictionary.GetValue("LinksCount", 0);
		for (int i = 0; i < value; i++)
		{
			ValuesDictionary value2 = valuesDictionary.GetValue<ValuesDictionary>("Entity" + i.ToString(CultureInfo.InvariantCulture));
			int num = value2.GetValue("ProviderEntityId", 0);
			int num2 = value2.GetValue("ContainerEntityId", 0);
			if (num == 0 && value2.ContainsKey("ProviderEntity"))
			{
				num = value2.GetValue<EntityReference>("ProviderEntity").GetEntity(loaderEntity, null, throwIfNotFound: false)?.Id ?? 0;
			}
			if (num2 == 0 && value2.ContainsKey("ContainerEntity"))
			{
				num2 = value2.GetValue<EntityReference>("ContainerEntity").GetEntity(loaderEntity, null, throwIfNotFound: false)?.Id ?? 0;
			}
			list.Add(new PendingScreenLink
			{
				ProviderEntityId = num,
				ContainerEntityId = num2,
				ProviderTypeName = value2.GetValue<string>("Provider"),
				ContainerTypeName = value2.GetValue<string>("Container")
			});
		}
		return list;
	}

	private bool TryRestoreLink(PendingScreenLink pending)
	{
		if (pending.ProviderEntityId == 0 || pending.ContainerEntityId == 0)
		{
			return false;
		}
		Entity entity = Project.FindEntity(pending.ProviderEntityId);
		Entity entity2 = Project.FindEntity(pending.ContainerEntityId);
		Type type = TypeCache.FindType(pending.ProviderTypeName, skipSystemAssemblies: false, throwIfNotFound: false);
		Type type2 = TypeCache.FindType(pending.ContainerTypeName, skipSystemAssemblies: false, throwIfNotFound: false);
		if (entity == null || entity2 == null || type == null || type2 == null)
		{
			return false;
		}
		if (!(entity.FindComponent(type, null, throwOnError: false) is IScreenProviderComponent first))
		{
			return false;
		}
		if (!(entity2.FindComponent(type2, null, throwOnError: false) is IScreenContainerComponent value))
		{
			return false;
		}
		m_screenLinks[first] = value;
		return true;
	}

	public override void OnEntityAdded(Entity entity)
	{
		base.OnEntityAdded(entity);
		IScreenProviderComponent[] array = entity.FindComponents<IScreenProviderComponent>().ToArray();
		IScreenContainerComponent[] array2 = entity.FindComponents<IScreenContainerComponent>().ToArray();
		if (array.Length != 0)
		{
			m_providerEntity[entity] = array;
		}
		if (array2.Length != 0)
		{
			m_containerEntity[entity] = array2;
		}
	}

	public override void OnEntityRemoved(Entity entity)
	{
		base.OnEntityRemoved(entity);
		if (m_providerEntity.ContainsKey(entity))
		{
			foreach (IScreenProviderComponent item in entity.FindComponents<IScreenProviderComponent>())
			{
				RemoveLink(item);
			}
			m_providerEntity.Remove(entity);
		}
		if (!m_containerEntity.ContainsKey(entity))
		{
			return;
		}
		foreach (IScreenContainerComponent item2 in entity.FindComponents<IScreenContainerComponent>())
		{
			RemoveLink(item2);
		}
		m_containerEntity.Remove(entity);
	}

	public bool CheckVisible(Camera camera, Screen screen)
	{
		if (!m_subsystemTerrain.IsVisible(camera, screen.CenterPos))
		{
			return false;
		}
		if (!camera.ViewFrustum.Intersection(screen.WorldPos1) && !camera.ViewFrustum.Intersection(screen.WorldPos2) && !camera.ViewFrustum.Intersection(screen.WorldPos3) && !camera.ViewFrustum.Intersection(screen.WorldPos4))
		{
			return camera.ViewFrustum.Intersection(screen.CenterPos);
		}
		return true;
	}

	public void Draw(Camera camera, int drawOrder)
	{
		foreach (KeyValuePair<IScreenProviderComponent, IScreenContainerComponent> screenLink in m_screenLinks)
		{
			Screen[] screens = screenLink.Value.GetScreens(camera);
			foreach (Screen screen in screens)
			{
				if (CheckVisible(camera, screen))
				{
					screenLink.Key.Draw(screen, Project, camera, drawOrder);
				}
			}
		}
		if (!m_debugCanDraw)
		{
			return;
		}
		foreach (KeyValuePair<IScreenProviderComponent, IScreenContainerComponent> screenLink2 in m_screenLinks)
		{
			m_flatBath.QueueBoundingBoxFace(screenLink2.Key.GetBoundingBox(), ProviderColor);
			m_flatBath.QueueBoundingBoxFace(screenLink2.Value.GetBoundingBox(), ContainerColor);
			m_flatBath.QueueLine(screenLink2.Key.GetPosition(), screenLink2.Value.GetPosition(), LinkColor);
		}
		m_p3d.Flush(camera.ViewProjectionMatrix);
	}

	internal void SerializeLanLinks(PacketSerializer archive)
	{
		const int limit = 2048;
		ushort count = archive.IsWriting ? (ushort)Math.Min(limit, m_screenLinks.Count) : (ushort)0;
		archive.Value(ref count);
		if (count > limit)
		{
			throw new System.IO.InvalidDataException("Invalid screen link snapshot.");
		}
		if (archive.IsWriting)
		{
			foreach (KeyValuePair<IScreenProviderComponent, IScreenContainerComponent> link in m_screenLinks.Take(count))
			{
				Component provider = (Component)link.Key;
				Component container = (Component)link.Value;
				int providerId = provider.Entity.Id;
				int providerIndex = provider.Entity.Components.IndexOf(provider);
				int containerId = container.Entity.Id;
				int containerIndex = container.Entity.Components.IndexOf(container);
				archive.Value(ref providerId);
				archive.Value(ref providerIndex);
				archive.Value(ref containerId);
				archive.Value(ref containerIndex);
			}
			return;
		}

		m_screenLinks.Clear();
		for (int i = 0; i < count; i++)
		{
			int providerId = 0;
			int providerIndex = 0;
			int containerId = 0;
			int containerIndex = 0;
			archive.Value(ref providerId);
			archive.Value(ref providerIndex);
			archive.Value(ref containerId);
			archive.Value(ref containerIndex);
			Entity provider = Project.FindEntity(providerId);
			Entity container = Project.FindEntity(containerId);
			if (provider != null && (uint)providerIndex < provider.Components.Count
				&& container != null && (uint)containerIndex < container.Components.Count
				&& provider.Components[providerIndex] is IScreenProviderComponent source
				&& container.Components[containerIndex] is IScreenContainerComponent display)
			{
				AddLink(source, display);
			}
		}
	}

	public void AddLink(IScreenProviderComponent providerComponent, IScreenContainerComponent containerComponent)
	{
		m_screenLinks[providerComponent] = containerComponent;
	}

	public void RemoveLink(IScreenContainerComponent containerComponent)
	{
		m_screenLinks.TryRemoveBySecond(containerComponent);
	}

	public void RemoveLink(IScreenProviderComponent providerComponent)
	{
		m_screenLinks.TryRemoveByFirst(providerComponent);
	}

	public bool TryGetProvider(IScreenContainerComponent containerComponent, out IScreenProviderComponent providerComponent)
	{
		return m_screenLinks.TryGetBySecond(containerComponent, out providerComponent);
	}

	public bool TryGetContainer(IScreenProviderComponent providerComponent, out IScreenContainerComponent containerComponent)
	{
		return m_screenLinks.TryGetByFirst(providerComponent, out containerComponent);
	}

	public bool HasProvider(Entity entity, out IScreenProviderComponent[]? providerComponents)
	{
		return m_providerEntity.TryGetValue(entity, out providerComponents);
	}

	public bool HasContainer(Entity entity, out IScreenContainerComponent[]? containerComponents)
	{
		return m_containerEntity.TryGetValue(entity, out containerComponents);
	}
}
