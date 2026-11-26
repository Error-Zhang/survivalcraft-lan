using System.Xml.Linq;

namespace Game;

public class TechTreeSavedDialog : Dialog
{
	private readonly string m_pathToCopy;

	private LabelWidget m_title;

	private LabelWidget m_body;

	private BevelledButtonWidget m_btnCopy;

	private BevelledButtonWidget m_btnOk;

	public TechTreeSavedDialog(string title, string body, string pathToCopy)
	{
		m_pathToCopy = pathToCopy;
		XElement node = ContentManager.Get<XElement>("Dialogs/TechTreeSavedDialog");
		LoadContents(this, node);
		m_title = Children.Find<LabelWidget>("TechTreeSavedDialog.Title");
		m_body = Children.Find<LabelWidget>("TechTreeSavedDialog.Body");
		m_btnCopy = Children.Find<BevelledButtonWidget>("TechTreeSavedDialog.CopyPath");
		m_btnOk = Children.Find<BevelledButtonWidget>("TechTreeSavedDialog.Ok");
		m_title.Text = title;
		m_body.Text = body;
		m_btnCopy.Text = LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "BtnCopyPath");
		m_btnOk.Text = LanguageControl.Ok;
	}

	public override void Update()
	{
		if (Input.Cancel || Input.Back)
		{
			DialogsManager.HideDialog(this);
		}
		else if (m_btnOk.IsClicked)
		{
			DialogsManager.HideDialog(this);
		}
		else if (m_btnCopy.IsClicked)
		{
			ClipboardManager.ClipboardString = m_pathToCopy;
			DialogsManager.ShowDialog(ParentWidget ?? this, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "PathCopiedTitle"), LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "PathCopiedBody"), LanguageControl.Ok, null, (MessageDialogButton _) =>
			{
			}));
		}
	}
}
