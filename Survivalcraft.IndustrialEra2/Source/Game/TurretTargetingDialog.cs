using System.Xml.Linq;
using Engine;
using SCIENEW;

namespace Game;

public class TurretTargetingDialog : Dialog
{
	private readonly ComponentTurretTargeting m_targeting;

	public LabelWidget m_titleWidget;

	public LabelWidget m_ownerWidget;

	public CheckboxWidget m_attackCreatures;

	public CheckboxWidget m_attackPlayers;

	public CheckboxWidget m_preventFriendlyFire;

	public LabelWidget m_preventHelp;

	public LabelWidget m_trustedTitle;

	public TextBoxWidget m_trustedInput;

	public BevelledButtonWidget m_addButton;

	public BevelledButtonWidget m_removeButton;

	public BevelledButtonWidget m_closeButton;

	public ListPanelWidget m_trustedList;

	public TurretTargetingDialog(ComponentTurretTargeting targeting)
	{
		m_targeting = targeting;
		XElement node = ContentManager.Get<XElement>("Dialogs/TurretTargetingDialog");
		LoadContents(this, node);
		StackPanelWidget stackPanelWidget = Children.Find<StackPanelWidget>("TurretTargetingDialog.Body");
		stackPanelWidget.MarginLeft = 20f;
		stackPanelWidget.MarginRight = 20f;
		stackPanelWidget.MarginTop = 16f;
		stackPanelWidget.MarginBottom = 12f;
		string[] array = new string[3] { "TurretTargetingDialog.AttackCreatures", "TurretTargetingDialog.AttackPlayers", "TurretTargetingDialog.PreventFriendlyFire" };
		foreach (string name in array)
		{
			CheckboxWidget checkboxWidget = Children.Find<CheckboxWidget>(name);
			checkboxWidget.MarginLeft = 0f;
			checkboxWidget.MarginRight = 0f;
			checkboxWidget.MarginTop = 2f;
			checkboxWidget.MarginBottom = 2f;
			checkboxWidget.CheckboxSize = new Vector2(24f, 24f);
		}
		m_titleWidget = Children.Find<LabelWidget>("TurretTargetingDialog.Title");
		m_ownerWidget = Children.Find<LabelWidget>("TurretTargetingDialog.Owner");
		m_attackCreatures = Children.Find<CheckboxWidget>("TurretTargetingDialog.AttackCreatures");
		m_attackPlayers = Children.Find<CheckboxWidget>("TurretTargetingDialog.AttackPlayers");
		m_preventFriendlyFire = Children.Find<CheckboxWidget>("TurretTargetingDialog.PreventFriendlyFire");
		m_preventHelp = Children.Find<LabelWidget>("TurretTargetingDialog.PreventHelp");
		m_trustedTitle = Children.Find<LabelWidget>("TurretTargetingDialog.TrustedTitle");
		m_trustedInput = Children.Find<TextBoxWidget>("TurretTargetingDialog.TrustedInput");
		m_addButton = Children.Find<BevelledButtonWidget>("TurretTargetingDialog.AddButton");
		m_removeButton = Children.Find<BevelledButtonWidget>("TurretTargetingDialog.RemoveButton");
		m_closeButton = Children.Find<BevelledButtonWidget>("TurretTargetingDialog.CloseButton");
		m_trustedList = Children.Find<ListPanelWidget>("TurretTargetingDialog.TrustedList");
		m_titleWidget.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "Title");
		m_attackCreatures.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "AttackCreatures");
		m_attackPlayers.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "AttackPlayers");
		m_preventFriendlyFire.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "PreventFriendlyFire");
		m_preventHelp.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "PreventFriendlyFireHelp");
		m_trustedTitle.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "TrustedTitle");
		m_addButton.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "Add");
		m_removeButton.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "Remove");
		m_closeButton.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "Close");
		m_trustedList.ItemWidgetFactory = (object item) => new LabelWidget
		{
			Text = (string)item,
			Color = Color.White,
			HorizontalAlignment = WidgetAlignment.Stretch,
			VerticalAlignment = WidgetAlignment.Center,
			Margin = new Vector2(8f, 4f)
		};
		RefreshOwnerAndChecks();
		RefreshTrustedList();
	}

	private string m_lastTrustedList;

	public override void Update()
	{
		if (Input.Cancel || m_closeButton.IsClicked)
		{
			DialogsManager.HideDialog(this);
			return;
		}
		if (m_attackCreatures.IsClicked)
		{
			IndustrialEraLanAdapter.ChangeTargeting(m_targeting, 0, m_targeting.AttackCreatures ? "0" : "1");
		}
		if (m_attackPlayers.IsClicked)
		{
			IndustrialEraLanAdapter.ChangeTargeting(m_targeting, 1, m_targeting.AttackPlayers ? "0" : "1");
		}
		if (m_preventFriendlyFire.IsClicked)
		{
			IndustrialEraLanAdapter.ChangeTargeting(m_targeting, 2, m_targeting.PreventFriendlyFire ? "0" : "1");
		}
		if (m_addButton.IsClicked && IndustrialEraLanAdapter.ChangeTargeting(m_targeting, 3, m_trustedInput.Text))
		{
			m_trustedInput.Text = "";
			RefreshTrustedList();
		}
		if (m_removeButton.IsClicked && m_trustedList.SelectedItem is string name && IndustrialEraLanAdapter.ChangeTargeting(m_targeting, 4, name))
		{
			RefreshTrustedList();
		}
		RefreshOwnerAndChecks();
		RefreshTrustedList();
	}

	private void RefreshOwnerAndChecks()
	{
		if (string.IsNullOrEmpty(m_targeting.OwnerName))
		{
			m_ownerWidget.Text = LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "NoOwner");
		}
		else
		{
			m_ownerWidget.Text = string.Format(LanguageControl.Get("ContentWidgets", "TurretTargetingDialog", "Owner"), m_targeting.OwnerName);
		}
		m_attackCreatures.IsChecked = m_targeting.AttackCreatures;
		m_attackPlayers.IsChecked = m_targeting.AttackPlayers;
		m_preventFriendlyFire.IsChecked = m_targeting.PreventFriendlyFire;
	}

	private void RefreshTrustedList()
	{
		string current = string.Join("\n", m_targeting.TrustedPlayers);
		if (current == m_lastTrustedList)
		{
			return;
		}
		m_lastTrustedList = current;
		m_trustedList.ClearItems();
		foreach (string trustedPlayer in m_targeting.TrustedPlayers)
		{
			m_trustedList.AddItem(trustedPlayer);
		}
	}
}
