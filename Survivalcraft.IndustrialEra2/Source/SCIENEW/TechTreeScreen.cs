using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX.Implementation;
using RecipaediaEX.UI;
using SCIENEW.Dialogs;
using SCIENEW.Tech;
using SCIENEW.Utils;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW;

public class TechTreeScreen : Screen
{
	public TechTree m_techTree;

	public CanvasWidget m_treeView;

	public BevelledButtonWidget m_thanksButton;

	public BevelledButtonWidget m_selectTreeButton;

	public BevelledButtonWidget m_staticButton;

	public TechTreeNodeInfoWidget m_nodeInfoWidget;

	public TechTreeBackgroundWidget m_background;

	public Dictionary<TechNode, TechTreeNodeWidget> m_nodeWidgets = new Dictionary<TechNode, TechTreeNodeWidget>();

	public TechNode m_selectedNode;

	public Screen m_previousScreen;

	public TechTreeScreen()
	{
		XElement node = ContentManager.Get<XElement>("Screens/TechTreeScreen");
		LoadContents(this, node);
		m_treeView = Children.Find<CanvasWidget>("TreeView");
		m_nodeInfoWidget = Children.Find<TechTreeNodeInfoWidget>("NodeInfo");
		m_staticButton = Children.Find<BevelledButtonWidget>("StaticBackgroundButton");
		m_selectTreeButton = Children.Find<BevelledButtonWidget>("SelectTechTreeButton");
		m_thanksButton = Children.Find<BevelledButtonWidget>("ThanksButton");
		m_background = Children.Find<TechTreeBackgroundWidget>("BG");
	}

	public override void Enter(object[] parameters)
	{
		if (ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("RecipaediaRecipes") && ScreensManager.PreviousScreen != ScreensManager.FindScreen<Screen>("RecipaediaDescription"))
		{
			m_previousScreen = ScreensManager.PreviousScreen;
			m_selectedNode = null;
			if (GameManager.Project != null)
			{
				RebuildTreeFromSubsystem();
			}
			else if (parameters != null && parameters.Length != 0 && parameters[0] is TechTree techTree)
			{
				m_techTree = techTree;
				m_thanksButton.IsVisible = true;
				m_treeView.Children.Clear();
				m_nodeWidgets.Clear();
				UpdateTreeView();
			}
			bool flag = IESettingsManager.Get<bool>("TechTreeStaticBackground");
			m_staticButton.IsChecked = flag;
			m_background.IsStatic = flag;
		}
	}

	private void RebuildTreeFromSubsystem()
	{
		if (GameManager.Project != null)
		{
			SubsystemTechTree subsystemTechTree = GameManager.Project.FindSubsystem<SubsystemTechTree>(throwOnError: true);
			m_techTree = subsystemTechTree.TechTree;
			m_thanksButton.IsVisible = subsystemTechTree.IsIndustrialDefaultTechTreeActive;
		}
		else
		{
			m_thanksButton.IsVisible = true;
		}
		m_treeView.Children.Clear();
		m_nodeWidgets.Clear();
		m_selectedNode = null;
		UpdateTreeView();
	}

	private void UpdateTreeView()
	{
		int num = 200;
		int num2 = 90;
		m_treeView.Size = new Vector2(12000f, 400f);
		m_treeView.VerticalAlignment = WidgetAlignment.Center;
		int num3 = 0;
		TechNode[] nodeByStage;
		do
		{
			nodeByStage = m_techTree.GetNodeByStage(num3);
			for (int i = 0; i < nodeByStage.Length; i++)
			{
				TechTreeNodeWidget techTreeNodeWidget = new TechTreeNodeWidget(nodeByStage[i]);
				techTreeNodeWidget.OnClicked = (Action<TechNode>)Delegate.Combine(techTreeNodeWidget.OnClicked, new Action<TechNode>(OnNodeClicked));
				float x = num + num3 * num;
				float num4 = m_treeView.Size.Y / 2f - techTreeNodeWidget.Size.Y / 2f;
				float y = ((nodeByStage.Length % 2 == 1) ? (((float)i - MathF.Floor((float)nodeByStage.Length / 2f)) * (float)num2) : (((float)i - ((float)nodeByStage.Length / 2f - 0.5f)) * (float)num2)) + num4;
				m_treeView.Children.Add(techTreeNodeWidget);
				m_nodeWidgets[nodeByStage[i]] = techTreeNodeWidget;
				m_treeView.SetWidgetPosition(techTreeNodeWidget, new Vector2(x, y) + nodeByStage[i].LayoutOffset);
			}
			num3++;
		}
		while (nodeByStage.Length != 0);
		foreach (KeyValuePair<TechNode, TechTreeNodeWidget> nodeWidget in m_nodeWidgets)
		{
			foreach (TechNode dependency in nodeWidget.Key.Dependencies)
			{
				TechTreeNodeWidget techTreeNodeWidget2 = m_nodeWidgets[dependency];
				TechTreeNodeWidget value = nodeWidget.Value;
				Vector2 vector = m_treeView.GetWidgetPosition(techTreeNodeWidget2).Value + new Vector2(techTreeNodeWidget2.Size.X + 6f, techTreeNodeWidget2.Size.Y / 2f);
				Vector2 vector2 = m_treeView.GetWidgetPosition(value).Value + new Vector2(-6f, value.Size.Y / 2f);
				Vector2 value2 = new Vector2(MathF.Min(vector.X, vector2.X), MathF.Min(vector.Y, vector2.Y));
				new Vector2(MathF.Max(vector.X, vector2.X), MathF.Max(vector.Y, vector2.Y));
				ArrowLineWidget widget = new ArrowLineWidget
				{
					Color = new Color(64, 150, 200),
					ArrowWidth = 4f,
					Width = 4f,
					PointsString = $"{vector.X},{vector.Y};{vector2.X},{vector2.Y}",
					ClampToBounds = true
				};
				m_treeView.Children.Add(widget);
				m_treeView.SetWidgetPosition(widget, value2);
			}
		}
	}

	public override void Update()
	{
		if (GameManager.Project is Project current && IndustrialEraLanAdapter.IsRemote(current))
		{
			m_selectTreeButton.IsVisible = false;
			if (current.FindSubsystem<SubsystemTechTree>() is SubsystemTechTree subsystem && !ReferenceEquals(m_techTree, subsystem.TechTree))
			{
				RebuildTreeFromSubsystem();
			}
		}
		m_nodeInfoWidget.m_node = m_selectedNode;
		m_staticButton.Text = LanguageUtils.GetText(this, "2") + (m_staticButton.IsChecked ? LanguageUtils.GetText(this, "On") : LanguageUtils.GetText(this, "Off"));
		m_background.IsStatic = m_staticButton.IsChecked;
		bool flag = IESettingsManager.Get<bool>("TechTreeStaticBackground");
		if (m_staticButton.IsChecked != flag)
		{
			IESettingsManager.Set("TechTreeStaticBackground", m_staticButton.IsChecked);
			IESettingsManager.Save();
		}
		if (m_thanksButton.IsVisible && m_thanksButton.IsClicked)
		{
			DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageUtils.GetText(this, "4"), "悟诗守禅哒(bilibili430597594)\r\nmmmmmooo(QQ2606235417)\r\n欧耶(QQ669468050)\r\n玖＜月＞(QQ2390735361)", LanguageControl.Ok, null, null));
		}
		if (GameManager.Project != null && m_selectTreeButton.IsVisible && m_selectTreeButton.IsClicked)
		{
			SubsystemTechTree sub = GameManager.Project.FindSubsystem<SubsystemTechTree>(throwOnError: true);
			TechTreeFileDialog.Show(this, LanguageUtils.GetText(this, "DlgPickTitle"), (TechTreeTemplateItem item) =>
			{
				if (!sub.TryApplyWorldTemplateSelection(item, out string error))
				{
					DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Error, error ?? string.Empty, LanguageControl.Ok, null, (MessageDialogButton _) =>
					{
					}));
				}
				else
				{
					RebuildTreeFromSubsystem();
				}
			}, includeSystemPicker: false);
		}
		if (m_nodeInfoWidget.m_node != null)
		{
			int blockID = m_nodeInfoWidget.m_node.IconBlock;
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(blockID)];
			if (m_nodeInfoWidget.m_recipeButton.IsClicked)
			{
				ScreensManager.SwitchScreen("RecipaediaRecipes", new BlockItem(block, 0, blockID), false);
			}
			if (m_nodeInfoWidget.m_descButton.IsClicked)
			{
				List<IRecipaediaItem> list = (from x in m_nodeWidgets.Keys.AsValueEnumerable()
					select x.IconBlock).Select<Select<FromEnumerable<TechNode>, TechNode, int>, int, IRecipaediaItem>((Func<int, IRecipaediaItem>)((int x) => new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(x)], 0, x))).ToList<Select<FromEnumerable<TechNode>, TechNode, int>, int, IRecipaediaItem>();
				IRecipaediaItem recipaediaItem = list.FirstOrDefault((IRecipaediaItem x) => x is BlockItem blockItem && blockItem.m_blockValue == blockID) ?? list.FirstOrDefault((IRecipaediaItem x) => object.Equals(x.Value, blockID)) ?? new BlockItem(block, 0, blockID);
				ScreensManager.SwitchScreen("RecipaediaDescription", recipaediaItem, list);
			}
		}
		if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
		{
			ScreensManager.SwitchScreen(m_previousScreen);
		}
	}

	public void OnNodeClicked(TechNode techNode)
	{
		m_selectedNode = techNode;
		foreach (KeyValuePair<TechNode, TechTreeNodeWidget> nodeWidget in m_nodeWidgets)
		{
			nodeWidget.Value.IsChecked = nodeWidget.Key == techNode;
		}
	}
}
