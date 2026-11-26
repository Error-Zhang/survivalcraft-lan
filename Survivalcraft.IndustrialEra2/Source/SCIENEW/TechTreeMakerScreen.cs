using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Dialogs;
using SCIENEW.Tech;

namespace SCIENEW;

public class TechTreeMakerScreen : Screen
{
	private IndustryBackgroundWidget m_background;

	private CanvasWidget m_treeView;

	private ListPanelWidget m_depList;

	private LabelWidget m_fileStatusLabel;

	private TextBoxWidget m_txtName;

	private TextBoxWidget m_txtStage;

	private TextBoxWidget m_txtDisplayName;

	private TextBoxWidget m_txtDescription;

	private TextBoxWidget m_txtLayoutX;

	private TextBoxWidget m_txtLayoutY;

	private TechTreeBlockIconWidget m_iconPreview;

	private BevelledButtonWidget m_btnNew;

	private BevelledButtonWidget m_btnImport;

	private BevelledButtonWidget m_btnSave;

	private BevelledButtonWidget m_btnSaveAs;

	private BevelledButtonWidget m_btnAddNode;

	private BevelledButtonWidget m_btnDeleteNode;

	private BevelledButtonWidget m_btnPickIcon;

	private BevelledButtonWidget m_btnAddDep;

	private BevelledButtonWidget m_btnRemoveDep;

	private BevelledButtonWidget m_btnStageOrderUp;

	private BevelledButtonWidget m_btnStageOrderDown;

	private TechTree m_tree = new TechTree("Default");

	private string? m_currentExternalFileName;

	private string? m_fileStatusHint;

	private TechNode? m_selected;

	private StackPanelWidget? m_inspectorColumn;

	private readonly Dictionary<TechNode, TechTreeNodeWidget> m_nodeWidgets = new Dictionary<TechNode, TechTreeNodeWidget>();

	private bool m_previewDirty = true;

	private bool m_depListDirty;

	private Screen? m_previousScreen;

	private bool m_inspectorTextHadFocus;

	public TechTreeMakerScreen()
	{
		XElement node = ContentManager.Get<XElement>("Screens/TechTreeMakerScreen");
		LoadContents(this, node);
		m_background = Children.Find<IndustryBackgroundWidget>("BG");
		m_treeView = Children.Find<CanvasWidget>("TreeView");
		m_depList = Children.Find<ListPanelWidget>("DepList");
		m_fileStatusLabel = Children.Find<LabelWidget>("FileStatusLabel");
		m_txtName = Children.Find<TextBoxWidget>("TxtName");
		m_txtStage = Children.Find<TextBoxWidget>("TxtStage");
		m_txtDisplayName = Children.Find<TextBoxWidget>("TxtDisplayName");
		m_txtDescription = Children.Find<TextBoxWidget>("TxtDescription");
		m_txtLayoutX = Children.Find<TextBoxWidget>("TxtLayoutX");
		m_txtLayoutY = Children.Find<TextBoxWidget>("TxtLayoutY");
		m_iconPreview = Children.Find<TechTreeBlockIconWidget>("IconPreview");
		m_btnNew = Children.Find<BevelledButtonWidget>("BtnNew");
		m_btnImport = Children.Find<BevelledButtonWidget>("BtnImport");
		m_btnSave = Children.Find<BevelledButtonWidget>("BtnSave");
		m_btnSaveAs = Children.Find<BevelledButtonWidget>("BtnSaveAs");
		m_btnAddNode = Children.Find<BevelledButtonWidget>("BtnAddNode");
		m_btnDeleteNode = Children.Find<BevelledButtonWidget>("BtnDeleteNode");
		m_btnPickIcon = Children.Find<BevelledButtonWidget>("BtnPickIcon");
		m_btnAddDep = Children.Find<BevelledButtonWidget>("BtnAddDep");
		m_btnRemoveDep = Children.Find<BevelledButtonWidget>("BtnRemoveDep");
		m_btnStageOrderUp = Children.Find<BevelledButtonWidget>("BtnStageOrderUp");
		m_btnStageOrderDown = Children.Find<BevelledButtonWidget>("BtnStageOrderDown");
		m_inspectorColumn = Children.Find<StackPanelWidget>("InspectorColumn");
		m_depList.ItemWidgetFactory = (object item) =>
		{
			TechNode techNode = (TechNode)item;
			CanvasWidget canvasWidget = new CanvasWidget
			{
				Size = new Vector2(0f, 48f),
				HorizontalAlignment = WidgetAlignment.Stretch
			};
			TechTreeBlockIconWidget widget = new TechTreeBlockIconWidget
			{
				Value = techNode.IconBlock,
				Size = new Vector2(36f),
				HorizontalAlignment = WidgetAlignment.Near,
				VerticalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(4f, 0f)
			};
			canvasWidget.Children.Add(widget);
			canvasWidget.Children.Add(new LabelWidget
			{
				Text = techNode.Name,
				HorizontalAlignment = WidgetAlignment.Stretch,
				VerticalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(44f, 0f),
				TextAnchor = TextAnchor.VerticalCenter
			});
			return canvasWidget;
		};
	}

	public override void Enter(object[] parameters)
	{
		m_previousScreen = ScreensManager.PreviousScreen;
		bool isStatic = IESettingsManager.Get<bool>("IndustrialHubStaticBackground");
		m_background.IsStatic = isStatic;
		if (parameters != null && parameters.Length != 0 && parameters[0] is TechTree tree)
		{
			m_tree = tree;
			m_currentExternalFileName = ((parameters.Length > 1) ? (parameters[1] as string) : null);
			m_fileStatusHint = null;
		}
		else
		{
			m_tree = new TechTree("Default");
			m_currentExternalFileName = null;
			m_fileStatusHint = null;
		}
		m_selected = null;
		m_previewDirty = true;
		m_depListDirty = true;
		RefreshFileStatus();
		PushFormFromSelection();
	}

	private bool InspectorTextboxIsFocused()
	{
		Widget focusedTextBox = TextBoxWidget.FocusedTextBox;
		if (focusedTextBox != m_txtName && focusedTextBox != m_txtStage && focusedTextBox != m_txtDisplayName && focusedTextBox != m_txtDescription && focusedTextBox != m_txtLayoutX)
		{
			return focusedTextBox == m_txtLayoutY;
		}
		return true;
	}

	private void TryCommitInspectorAfterTextFocusLost()
	{
		bool flag = InspectorTextboxIsFocused();
		if (m_inspectorTextHadFocus && !flag && m_selected != null && !TryApplyInspectorToSelected())
		{
			PushFormFromSelection();
		}
		m_inspectorTextHadFocus = flag;
	}

	private bool TryCommitInspectorForToolbarAction()
	{
		if (m_selected != null)
		{
			return TryApplyInspectorToSelected();
		}
		return true;
	}

	private void UnlockAllForPreview()
	{
		foreach (string item in m_tree.TechNodes.Keys.ToList())
		{
			m_tree.UnlockNode(item, out TechNode _);
		}
	}

	private void RebuildPreviewTree()
	{
		UnlockAllForPreview();
		m_treeView.Children.Clear();
		m_nodeWidgets.Clear();
		m_treeView.Size = new Vector2(12000f, 400f);
		m_treeView.VerticalAlignment = WidgetAlignment.Center;
		int num = 0;
		TechNode[] nodeByStage;
		do
		{
			nodeByStage = m_tree.GetNodeByStage(num);
			for (int i = 0; i < nodeByStage.Length; i++)
			{
				TechTreeNodeWidget techTreeNodeWidget = new TechTreeNodeWidget(nodeByStage[i]);
				techTreeNodeWidget.OnClicked = (Action<TechNode>)Delegate.Combine(techTreeNodeWidget.OnClicked, new Action<TechNode>(OnPreviewNodeClicked));
				float x = 200 + num * 200;
				float num2 = m_treeView.Size.Y / 2f - techTreeNodeWidget.Size.Y / 2f;
				float y = ((nodeByStage.Length % 2 == 1) ? (((float)i - MathF.Floor((float)nodeByStage.Length / 2f)) * 90f) : (((float)i - ((float)nodeByStage.Length / 2f - 0.5f)) * 90f)) + num2;
				m_treeView.Children.Add(techTreeNodeWidget);
				m_nodeWidgets[nodeByStage[i]] = techTreeNodeWidget;
				m_treeView.SetWidgetPosition(techTreeNodeWidget, new Vector2(x, y) + nodeByStage[i].LayoutOffset);
			}
			num++;
		}
		while (nodeByStage.Length != 0);
		foreach (KeyValuePair<TechNode, TechTreeNodeWidget> nodeWidget in m_nodeWidgets)
		{
			foreach (TechNode dependency in nodeWidget.Key.Dependencies)
			{
				if (m_nodeWidgets.TryGetValue(dependency, out TechTreeNodeWidget value))
				{
					TechTreeNodeWidget value2 = nodeWidget.Value;
					Vector2 vector = m_treeView.GetWidgetPosition(value).Value + new Vector2(value.Size.X + 6f, value.Size.Y / 2f);
					Vector2 vector2 = m_treeView.GetWidgetPosition(value2).Value + new Vector2(-6f, value2.Size.Y / 2f);
					Vector2 value3 = new Vector2(MathF.Min(vector.X, vector2.X), MathF.Min(vector.Y, vector2.Y));
					ArrowLineWidget widget = new ArrowLineWidget
					{
						Color = new Color(64, 150, 200),
						ArrowWidth = 4f,
						Width = 4f,
						PointsString = $"{vector.X},{vector.Y};{vector2.X},{vector2.Y}",
						ClampToBounds = true
					};
					m_treeView.Children.Add(widget);
					m_treeView.SetWidgetPosition(widget, value3);
				}
			}
		}
		SyncPreviewSelectionHighlight();
	}

	private void OnPreviewNodeClicked(TechNode techNode)
	{
		if (m_selected != null && m_selected != techNode && !TryApplyInspectorToSelected())
		{
			SyncPreviewSelectionHighlight();
		}
		else if (m_selected != techNode)
		{
			m_selected = techNode;
			SyncPreviewSelectionHighlight();
			PushFormFromSelection();
			m_depListDirty = true;
		}
	}

	private void SyncPreviewSelectionHighlight()
	{
		foreach (KeyValuePair<TechNode, TechTreeNodeWidget> nodeWidget in m_nodeWidgets)
		{
			nodeWidget.Value.IsChecked = m_selected != null && nodeWidget.Key == m_selected;
		}
	}

	private void RefreshFileStatus()
	{
		string format = LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "FileStatus");
		string arg = m_currentExternalFileName ?? m_fileStatusHint ?? LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "Unsaved");
		m_fileStatusLabel.Text = string.Format(CultureInfo.CurrentCulture, format, m_tree.Name, arg);
	}

	private void ShowSavedExternalDialog(string savedFileName)
	{
		string externalTechTreeAbsoluteUserPath = TechTreeManager.GetExternalTechTreeAbsoluteUserPath(savedFileName);
		string text = (TechTreeManager.TryRevealExternalTechTreeFile(savedFileName) ? LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "SavedRevealOpened") : string.Format(CultureInfo.CurrentCulture, LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "SavedRevealFailed"), externalTechTreeAbsoluteUserPath));
		string body = string.Format(CultureInfo.CurrentCulture, LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "SavedBodyExternal"), externalTechTreeAbsoluteUserPath) + "\n\n" + text;
		DialogsManager.ShowDialog(this, new TechTreeSavedDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "SavedTitle"), body, externalTechTreeAbsoluteUserPath));
	}

	private void StartSystemImportPicker()
	{
		TechTreeManager.StartChooseTchFromSystem(this, LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "FilterTch"), LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "DlgSystemImportTitle"), (TechTree tree, string? _) =>
		{
			m_tree = tree;
			m_currentExternalFileName = null;
			m_fileStatusHint = null;
			m_selected = null;
			m_previewDirty = true;
			m_depListDirty = true;
			RefreshFileStatus();
			PushFormFromSelection();
		});
	}

	private void OnImportTemplatePicked(TechTreeTemplateItem item)
	{
		if (item.Kind == TechTreeTemplateKind.SystemPicker)
		{
			StartSystemImportPicker();
			return;
		}
		try
		{
			if (item.Kind == TechTreeTemplateKind.ModEmbedded && item.ModEntry != null)
			{
				m_tree = TechTreeManager.DecodeTechTree(item.ModEntry.Root);
				m_currentExternalFileName = null;
				m_fileStatusHint = "[" + item.ModEntry.ModTitle + "] " + item.ModEntry.FileName;
			}
			else if (item.Kind == TechTreeTemplateKind.ExternalFolder && !string.IsNullOrEmpty(item.ExternalFileName))
			{
				m_tree = TechTreeManager.LoadTechTreeFromExternalFile(item.ExternalFileName);
				m_currentExternalFileName = item.ExternalFileName;
				m_fileStatusHint = null;
			}
			m_selected = null;
			m_previewDirty = true;
			m_depListDirty = true;
			RefreshFileStatus();
			PushFormFromSelection();
		}
		catch (Exception ex)
		{
			DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrTitle"), ex.Message, LanguageControl.Ok, null, (MessageDialogButton _) =>
			{
			}));
		}
	}

	private void RefreshDepList()
	{
		m_depList.ClearItems();
		if (m_selected == null)
		{
			m_depListDirty = false;
			return;
		}
		foreach (TechNode item in m_selected.Dependencies.OrderBy((TechNode x) => x.Name, StringComparer.OrdinalIgnoreCase))
		{
			m_depList.AddItem(item);
		}
		m_depListDirty = false;
	}

	private void PushFormFromSelection()
	{
		if (m_selected == null)
		{
			m_txtName.Text = string.Empty;
			m_txtStage.Text = string.Empty;
			m_txtDisplayName.Text = string.Empty;
			m_txtDescription.Text = string.Empty;
			m_txtLayoutX.Text = "0";
			m_txtLayoutY.Text = "0";
			UpdateInspectorVisibility();
			SyncStageOrderButtons();
			m_inspectorTextHadFocus = InspectorTextboxIsFocused();
		}
		else
		{
			m_txtName.Text = m_selected.Name;
			m_txtStage.Text = m_selected.Stage.ToString(CultureInfo.InvariantCulture);
			m_txtDisplayName.Text = m_selected.DisplayName ?? string.Empty;
			m_txtDescription.Text = m_selected.Description ?? string.Empty;
			m_txtLayoutX.Text = m_selected.LayoutOffset.X.ToString(CultureInfo.InvariantCulture);
			m_txtLayoutY.Text = m_selected.LayoutOffset.Y.ToString(CultureInfo.InvariantCulture);
			m_iconPreview.Value = m_selected.IconBlock;
			m_iconPreview.m_needRefresh = true;
			m_depListDirty = true;
			UpdateInspectorVisibility();
			SyncStageOrderButtons();
			m_inspectorTextHadFocus = InspectorTextboxIsFocused();
		}
	}

	private void SyncStageOrderButtons()
	{
		if (m_selected == null)
		{
			m_btnStageOrderUp.IsEnabled = false;
			m_btnStageOrderDown.IsEnabled = false;
			return;
		}
		TechNode[] array = (from n in m_tree.TechNodes.Values
			where n.Stage == m_selected.Stage
			orderby n.StageOrder
			select n).ThenBy((TechNode n) => n.Name, StringComparer.OrdinalIgnoreCase).ToArray();
		int num = Array.IndexOf(array, m_selected);
		m_btnStageOrderUp.IsEnabled = num > 0;
		m_btnStageOrderDown.IsEnabled = num >= 0 && num < array.Length - 1;
	}

	private void UpdateInspectorVisibility()
	{
		if (m_inspectorColumn != null)
		{
			m_inspectorColumn.IsVisible = m_selected != null;
		}
	}

	private bool TryApplyInspectorToSelected()
	{
		if (m_selected == null)
		{
			return false;
		}
		string text = (m_txtName.Text ?? string.Empty).Trim();
		if (string.IsNullOrEmpty(text))
		{
			DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrTitle"), LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrEmptyName"), LanguageControl.Ok, null, (MessageDialogButton _) =>
			{
			}));
			return false;
		}
		if (text != m_selected.Name && !m_tree.RenameNode(m_selected.Name, text))
		{
			DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrTitle"), LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrDuplicateName"), LanguageControl.Ok, null, (MessageDialogButton _) =>
			{
			}));
			return false;
		}
		m_selected = m_tree.GetNode(text);
		if (m_selected == null)
		{
			return false;
		}
		if (!int.TryParse((m_txtStage.Text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			result = m_selected.Stage;
		}
		int stage = m_selected.Stage;
		m_selected.Stage = result;
		if (result != stage)
		{
			m_selected.StageOrder = m_tree.AllocateStageOrderForStage(result);
		}
		m_selected.DisplayName = m_txtDisplayName.Text ?? string.Empty;
		m_selected.Description = m_txtDescription.Text ?? string.Empty;
		float x = (float.TryParse((m_txtLayoutX.Text ?? string.Empty).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result2) ? result2 : 0f);
		float y = (float.TryParse((m_txtLayoutY.Text ?? string.Empty).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result3) ? result3 : 0f);
		m_selected.LayoutOffset = new Vector2(x, y);
		m_previewDirty = true;
		m_depListDirty = true;
		SyncStageOrderButtons();
		return true;
	}

	public override void Update()
	{
		if (m_previewDirty)
		{
			RebuildPreviewTree();
			m_previewDirty = false;
		}
		if (m_depListDirty)
		{
			RefreshDepList();
		}
		TryCommitInspectorAfterTextFocusLost();
		if (m_btnNew.IsClicked && TryCommitInspectorForToolbarAction())
		{
			DialogsManager.ShowDialog(this, new TextBoxDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "DlgNewTreeTitle"), m_tree.Name, 128, (string s) =>
			{
				if (!string.IsNullOrWhiteSpace(s))
				{
					string text = s.Trim();
					m_tree = new TechTree(text, text);
					m_currentExternalFileName = null;
					m_fileStatusHint = null;
					m_selected = null;
					m_previewDirty = true;
					m_depListDirty = true;
					RefreshFileStatus();
					PushFormFromSelection();
				}
			}));
		}
		if (m_btnImport.IsClicked && TryCommitInspectorForToolbarAction())
		{
			TechTreeFileDialog.Show(this, LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "DlgImportTitle"), OnImportTemplatePicked);
		}
		if (m_btnSave.IsClicked && TryCommitInspectorForToolbarAction())
		{
			if (string.IsNullOrEmpty(m_currentExternalFileName))
			{
				PromptSaveAs();
			}
			else
			{
				try
				{
					TechTreeManager.SaveTechTreeToExternalFile(m_tree, m_currentExternalFileName);
					ShowSavedExternalDialog(m_currentExternalFileName);
				}
				catch (Exception ex)
				{
					DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrTitle"), ex.Message, LanguageControl.Ok, null, (MessageDialogButton _) =>
					{
					}));
				}
			}
		}
		if (m_btnSaveAs.IsClicked && TryCommitInspectorForToolbarAction())
		{
			PromptSaveAs();
		}
		if (m_btnAddNode.IsClicked && TryCommitInspectorForToolbarAction())
		{
			DialogsManager.ShowDialog(this, new TextBoxDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "DlgNewNodeTitle"), string.Empty, 128, (string s) =>
			{
				if (!string.IsNullOrWhiteSpace(s))
				{
					string text = s.Trim();
					if (m_tree.TechNodes.ContainsKey(text))
					{
						DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrTitle"), LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrDuplicateName"), LanguageControl.Ok, null, (MessageDialogButton _) =>
						{
						}));
					}
					else
					{
						TechNode orCreateNode = m_tree.GetOrCreateNode(text);
						orCreateNode.StageOrder = m_tree.AllocateStageOrderForStage(orCreateNode.Stage);
						m_selected = orCreateNode;
						m_previewDirty = true;
						m_depListDirty = true;
						RefreshFileStatus();
						PushFormFromSelection();
					}
				}
			}));
		}
		if (m_btnDeleteNode.IsClicked && m_selected != null && TryCommitInspectorForToolbarAction())
		{
			string toRemove = m_selected.Name;
			DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ConfirmDeleteTitle"), string.Format(CultureInfo.CurrentCulture, LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ConfirmDeleteBody"), toRemove), LanguageControl.Ok, LanguageControl.Cancel, (MessageDialogButton btn) =>
			{
				if (btn == MessageDialogButton.Button1)
				{
					m_tree.RemoveNode(toRemove);
					m_selected = null;
					m_previewDirty = true;
					m_depListDirty = true;
					PushFormFromSelection();
				}
			}));
		}
		if (m_btnPickIcon.IsClicked && m_selected != null && TryCommitInspectorForToolbarAction())
		{
			DialogsManager.ShowDialog(this, new SelectBlockDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "DlgPickIconTitle"), (int v) =>
			{
				m_selected.IconBlock = v;
				m_iconPreview.Value = v;
				m_iconPreview.m_needRefresh = true;
				m_previewDirty = true;
			}));
		}
		if (m_btnAddDep.IsClicked && m_selected != null && TryCommitInspectorForToolbarAction())
		{
			SelectDependencyDialog.Show(this, m_tree, m_selected, LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "DlgAddDepTitle"), (string? depName) =>
			{
				if (!string.IsNullOrEmpty(depName))
				{
					m_tree.AddDependency(m_selected.Name, depName);
					m_depListDirty = true;
					m_previewDirty = true;
				}
			});
		}
		if (m_btnRemoveDep.IsClicked && m_selected != null && m_depList.SelectedItem is TechNode techNode && TryCommitInspectorForToolbarAction())
		{
			m_tree.RemoveDependency(m_selected.Name, techNode.Name);
			m_depListDirty = true;
			m_previewDirty = true;
		}
		if (m_btnStageOrderUp.IsClicked && m_selected != null && TryCommitInspectorForToolbarAction() && m_tree.MoveNodeStageOrderWithinStage(m_selected.Name, moveEarlier: true))
		{
			m_previewDirty = true;
			SyncStageOrderButtons();
		}
		if (m_btnStageOrderDown.IsClicked && m_selected != null && TryCommitInspectorForToolbarAction() && m_tree.MoveNodeStageOrderWithinStage(m_selected.Name, moveEarlier: false))
		{
			m_previewDirty = true;
			SyncStageOrderButtons();
		}
		if ((Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked) && TryCommitInspectorForToolbarAction())
		{
			ScreensManager.SwitchScreen(m_previousScreen);
		}
	}

	private void PromptSaveAs()
	{
		DialogsManager.ShowDialog(this, new TextBoxDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "DlgSaveAsTitle"), m_currentExternalFileName ?? m_tree.Name, 128, (string s) =>
		{
			if (string.IsNullOrWhiteSpace(s))
			{
				return;
			}
			try
			{
				string text = s.Trim();
				TechTreeManager.SaveTechTreeToExternalFile(m_tree, text);
				m_currentExternalFileName = (text.EndsWith(".tch", StringComparison.OrdinalIgnoreCase) ? text : (text + ".tch"));
				m_fileStatusHint = null;
				RefreshFileStatus();
				ShowSavedExternalDialog(m_currentExternalFileName);
			}
			catch (Exception ex)
			{
				DialogsManager.ShowDialog((ContainerWidget)this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ErrTitle"), ex.Message, LanguageControl.Ok, null, (MessageDialogButton _) =>
				{
				}));
			}
		}));
	}
}
