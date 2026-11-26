using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX.Events;
using SCIENEW.Dialogs;
using SCIENEW.Events;
using SCIENEW.Tech;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemTechTree : Subsystem, IUpdateable
{
	public SubsystemPlayers m_subsystemPlayers;

	private const string KeyActiveSelection = "TechTreeActiveSelection";

	private const string KeyPerTreeUnlocks = "TechTreePerIdUnlocks";

	private const string KeyLegacyUnlockedNodes = "UnlockedNodes";

	private readonly Dictionary<string, string> m_unlocksByTreeId = new Dictionary<string, string>(StringComparer.Ordinal);

	private IDisposable? m_crafterOutputSubscription;

	private IDisposable? m_openRecipaediaSubscription;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public TechTree TechTree { get; private set; }

	public string ActiveSelectionKey { get; private set; } = TechTreeManager.IndustrialDefaultSelectionKey;

	public bool IsIndustrialDefaultTechTreeActive => TechTreeManager.IsIndustrialDefaultSelection(ActiveSelectionKey);

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		TechTreeManager.RefreshModTechTreeCatalog();
		m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
		string value = valuesDictionary.GetValue("TechTreePerIdUnlocks", string.Empty);
		foreach (KeyValuePair<string, string> item in TechTreeManager.DeserializePerTreeUnlocks(value))
		{
			m_unlocksByTreeId[item.Key] = item.Value;
		}
		ActiveSelectionKey = valuesDictionary.GetValue("TechTreeActiveSelection", TechTreeManager.IndustrialDefaultSelectionKey);
		string error = null;
		TechTree techTree = TechTreeManager.TryLoadTechTreeBySelectionKey(ActiveSelectionKey, out error);
		if (techTree == null)
		{
			Log.Warning($"[SubsystemTechTree] Failed load \"{ActiveSelectionKey}\": {error}; fallback default.");
			ActiveSelectionKey = TechTreeManager.IndustrialDefaultSelectionKey;
			techTree = TechTreeManager.TryLoadTechTreeBySelectionKey(ActiveSelectionKey, out error);
		}
		if (techTree == null)
		{
			ActiveSelectionKey = TechTreeManager.IndustrialDefaultSelectionKey;
			techTree = TechTreeManager.DecodeTechTree(TechTreeManager.GetDefaultWorldTechTreeXml());
		}
		TechTree = techTree;
		string value2 = valuesDictionary.GetValue("UnlockedNodes", string.Empty);
		if (!string.IsNullOrEmpty(value2) && string.IsNullOrEmpty(value))
		{
			m_unlocksByTreeId[TechTree.Id] = value2;
		}
		ApplySavedUnlocksToTree();
		SubscribeToCrafterEvents();
	}

	private void SubscribeToCrafterEvents()
	{
		m_crafterOutputSubscription?.Dispose();
		m_crafterOutputSubscription = RecipaediaEventBus.CrafterOutputRemoved.Subscribe(OnCrafterOutputRemoved);
		m_openRecipaediaSubscription?.Dispose();
		m_openRecipaediaSubscription = RecipaediaEventBus.GetSubscriber<OpenFullRecipaediaRequestedEvent>().Subscribe((OpenFullRecipaediaRequestedEvent _) =>
		{
			OnOpenFullRecipaediaRequested();
		});
	}

	private void OnOpenFullRecipaediaRequested()
	{
		if (m_subsystemPlayers.MainPlayerData?.ComponentPlayer is ComponentPlayer player && !DialogsManager.HasDialogs(player.GuiWidget))
		{
			ScreensManager.SwitchScreen("Recipaedia");
		}
	}

	public override void Dispose()
	{
		m_crafterOutputSubscription?.Dispose();
		m_crafterOutputSubscription = null;
		m_openRecipaediaSubscription?.Dispose();
		m_openRecipaediaSubscription = null;
		base.Dispose();
	}

	private void OnCrafterOutputRemoved(CrafterOutputRemovedEvent e)
	{
		if (e.Project == Project)
		{
			Unlock(e.OutputBlockValue, e.InteractingPlayer);
		}
	}

	public override void Save(ValuesDictionary valuesDictionary)
	{
		base.Save(valuesDictionary);
		FlushCurrentTreeUnlocksToMap();
		valuesDictionary.SetValue("TechTreeActiveSelection", ActiveSelectionKey);
		valuesDictionary.SetValue("TechTreePerIdUnlocks", TechTreeManager.SerializePerTreeUnlocks(m_unlocksByTreeId));
		StringBuilder stringBuilder = new StringBuilder();
		TechNode[] unlockedNodes = TechTree.GetUnlockedNodes();
		foreach (TechNode techNode in unlockedNodes)
		{
			stringBuilder.Append(techNode.Name);
			stringBuilder.Append(';');
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		valuesDictionary.SetValue("UnlockedNodes", stringBuilder.ToString());
	}

	private void FlushCurrentTreeUnlocksToMap()
	{
		if (TechTree != null && !string.IsNullOrEmpty(TechTree.Id))
		{
			TechNode[] unlockedNodes = TechTree.GetUnlockedNodes();
			m_unlocksByTreeId[TechTree.Id] = ((unlockedNodes.Length == 0) ? string.Empty : string.Join(";", unlockedNodes.Select((TechNode n) => n.Name)));
		}
	}

	private void ApplySavedUnlocksToTree()
	{
		TechTree.LockAllNodes();
		if (m_unlocksByTreeId.TryGetValue(TechTree.Id, out string value) && !string.IsNullOrEmpty(value))
		{
			string[] nodes = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
			TechTree.UnlockNodes(nodes);
		}
	}

	public bool TryApplyWorldTemplateSelection(TechTreeTemplateItem item, out string? error)
	{
		error = null;
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			return false;
		}
		TechTree techTree = TechTree;
		string activeSelectionKey = ActiveSelectionKey;
		FlushCurrentTreeUnlocksToMap();
		try
		{
			if (item.Kind == TechTreeTemplateKind.ModEmbedded && item.ModEntry != null)
			{
				ActiveSelectionKey = TechTreeManager.EncodeModSelection(item.ModEntry.PackageName, item.ModEntry.FileName);
				TechTree = TechTreeManager.DecodeTechTree(item.ModEntry.Root);
			}
			else
			{
				if (item.Kind != TechTreeTemplateKind.ExternalFolder || string.IsNullOrEmpty(item.ExternalFileName))
				{
					error = "unsupported";
					TechTree = techTree;
					ActiveSelectionKey = activeSelectionKey;
					return false;
				}
				ActiveSelectionKey = TechTreeManager.EncodeExternalSelection(item.ExternalFileName);
				TechTree = TechTreeManager.LoadTechTreeFromExternalFile(item.ExternalFileName);
			}
		}
		catch (Exception ex)
		{
			error = ex.Message;
			TechTree = techTree;
			ActiveSelectionKey = activeSelectionKey;
			return false;
		}
		ApplySavedUnlocksToTree();
		return true;
	}

	public bool TryApplyDiskTechTree(string absolutePath, out string? error)
	{
		error = null;
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			return false;
		}
		TechTree techTree = TechTree;
		string activeSelectionKey = ActiveSelectionKey;
		FlushCurrentTreeUnlocksToMap();
		try
		{
			ActiveSelectionKey = TechTreeManager.EncodeDiskSelection(absolutePath);
			using FileStream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			TechTree = TechTreeManager.LoadTechTreeFromStream(stream);
		}
		catch (Exception ex)
		{
			error = ex.Message;
			TechTree = techTree;
			ActiveSelectionKey = activeSelectionKey;
			return false;
		}
		ApplySavedUnlocksToTree();
		return true;
	}

	public void Update(float dt)
	{
		if (m_subsystemPlayers.MainPlayerData?.ComponentPlayer is ComponentPlayer player
			&& TechTreeManager.IsOpenTechTree() && !DialogsManager.HasDialogs(player.GuiWidget))
		{
			Show();
		}
	}

	public void Show()
	{
		ScreensManager.SwitchScreen("TechTree", TechTree);
	}

	internal void ApplyLanProgress(TechTree tree, string unlocked)
	{
		TechTree = tree ?? TechTree;
		m_unlocksByTreeId[TechTree.Id] = unlocked;
		ApplySavedUnlocksToTree();
	}

	public void Unlock(int blockValue, ComponentPlayer? player)
	{
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			return;
		}
		TechNode techNode = FindLockedNodeByIcon(blockValue);
		if ((techNode == null || RecipaediaInterceptBus.TryProceed(new TechNodeUnlockingContext(Project, TechTree, techNode, blockValue, player))) && !TechTree.UnlockNode(blockValue, out TechNode unlockedNode) && unlockedNode != null)
		{
			RecipaediaEventBus.GetPublisher<TechNodeUnlockedEvent>().Publish(new TechNodeUnlockedEvent(Project, TechTree, unlockedNode, blockValue, player));
			player?.ComponentGui.DisplayLargeMessage(LanguageUtils.GetText(this, "UnlockText"), unlockedNode.DisplayName, 3f, 0f);
		}
	}

	private TechNode? FindLockedNodeByIcon(int blockValue)
	{
		foreach (KeyValuePair<string, TechNode> techNode in TechTree.TechNodes)
		{
			if (techNode.Value.IconBlock == blockValue && techNode.Value.IsLocked)
			{
				return techNode.Value;
			}
		}
		return null;
	}
}
