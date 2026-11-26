using System;
using System.Xml.Linq;
using Game;

namespace SCIENEW;

public class IECreditsScreen : Screen
{
	public IndustryBackgroundWidget m_background;

	public CreditsSandboxWidget m_sandbox;

	public LabelWidget m_hintLabel;

	public IECreditsScreen()
	{
		XElement node = ContentManager.Get<XElement>("Screens/IECreditsScreen");
		LoadContents(this, node);
		m_background = Children.Find<IndustryBackgroundWidget>("BG");
		m_sandbox = Children.Find<CreditsSandboxWidget>("Sandbox");
		m_hintLabel = Children.Find<LabelWidget>("HintLabel");
	}

	public override void Enter(object[] parameters)
	{
		bool isStatic = IESettingsManager.Get<bool>("IndustrialHubStaticBackground");
		m_background.IsStatic = isStatic;
		m_sandbox.ResetSimulation(CreditsCsvLoader.LoadDefault());
		if (m_hintLabel != null)
		{
			m_hintLabel.Text = LanguageControl.Get("ContentWidgets", "IECreditsScreen", "2");
		}
	}

	public override void Leave()
	{
		m_sandbox.ResetSimulation(Array.Empty<CreditsEntry>());
	}

	public override void Update()
	{
		m_background.IsStatic = IESettingsManager.Get<bool>("IndustrialHubStaticBackground");
		if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
		{
			if (ScreensManager.TopOfHistoryScreen != null)
			{
				ScreensManager.GoBack();
			}
			else if (ScreensManager.PreviousScreen != null)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}
	}
}
