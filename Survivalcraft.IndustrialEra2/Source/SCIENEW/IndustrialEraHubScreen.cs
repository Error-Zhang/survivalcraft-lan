using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;
using RecipaediaEX.Events;
using SCIENEW.Events;

namespace SCIENEW;

public class IndustrialEraHubScreen : Screen
{
	public IndustryBackgroundWidget m_background;

	public BevelledButtonWidget m_settingsButton;

	public BevelledButtonWidget m_staticBackgroundButton;

	public BevelledButtonWidget m_techTreeMakerButton;

	public BevelledButtonWidget m_iconMakerButton;

	public BevelledButtonWidget m_releasesButton;

	public BevelledButtonWidget m_creditsButton;

	public LabelWidget m_versionLabel;

	private StackPanelWidget m_entriesPanel;

	private readonly List<(BevelledButtonWidget Button, Action OnClick)> m_extraEntries = new List<(BevelledButtonWidget, Action)>();

	public IndustrialEraHubScreen()
	{
		XElement node = ContentManager.Get<XElement>("Screens/IndustrialEraHubScreen");
		LoadContents(this, node);
		m_background = Children.Find<IndustryBackgroundWidget>("BG");
		m_settingsButton = Children.Find<BevelledButtonWidget>("SettingsButton");
		m_staticBackgroundButton = Children.Find<BevelledButtonWidget>("StaticBackgroundButton");
		m_techTreeMakerButton = Children.Find<BevelledButtonWidget>("TechTreeMakerButton");
		m_iconMakerButton = Children.Find<BevelledButtonWidget>("IconMakerButton");
		m_releasesButton = Children.Find<BevelledButtonWidget>("ReleasesButton");
		m_releasesButton.ParentWidget.IsVisible = false;
		m_creditsButton = Children.Find<BevelledButtonWidget>("CreditsButton");
		m_versionLabel = Children.Find<LabelWidget>("VersionLabel");
		m_entriesPanel = Children.Find<StackPanelWidget>("EntriesPanel");
		PublishHubCreatingAndBuildExtras();
	}

	private void PublishHubCreatingAndBuildExtras()
	{
		IndustrialHubCreatingEvent industrialHubCreatingEvent = new IndustrialHubCreatingEvent(this);
		RecipaediaEventBus.GetPublisher<IndustrialHubCreatingEvent>().Publish(industrialHubCreatingEvent);
		foreach (IndustrialHubEntry extraEntry in industrialHubCreatingEvent.ExtraEntries)
		{
			AppendHubEntry(extraEntry);
		}
	}

	private void AppendHubEntry(IndustrialHubEntry entry)
	{
		StackPanelWidget stackPanelWidget = new StackPanelWidget
		{
			Direction = LayoutDirection.Horizontal
		};
		CanvasWidget canvasWidget = new CanvasWidget
		{
			Size = new Vector2(320f, 96f)
		};
		canvasWidget.Children.Add(new LabelWidget
		{
			Text = entry.Description,
			FontScale = 0.7f,
			Color = new Color(128, 128, 128),
			WordWrap = true,
			HorizontalAlignment = WidgetAlignment.Far,
			VerticalAlignment = WidgetAlignment.Center,
			TextAnchor = TextAnchor.Center
		});
		CanvasWidget widget = new CanvasWidget
		{
			Size = new Vector2(12f, 0f)
		};
		BevelledButtonWidget bevelledButtonWidget = new BevelledButtonWidget
		{
			Text = entry.ButtonText,
			Size = new Vector2(310f, 60f),
			VerticalAlignment = WidgetAlignment.Center
		};
		stackPanelWidget.Children.Add(canvasWidget);
		stackPanelWidget.Children.Add(widget);
		stackPanelWidget.Children.Add(bevelledButtonWidget);
		int num = m_entriesPanel.Children.IndexOf(m_versionLabel);
		if (num < 0)
		{
			num = m_entriesPanel.Children.Count;
		}
		m_entriesPanel.Children.Insert(num, new CanvasWidget
		{
			Size = new Vector2(0f, 16f)
		});
		m_entriesPanel.Children.Insert(num + 1, stackPanelWidget);
		m_extraEntries.Add((bevelledButtonWidget, entry.OnClick));
	}

	public override void Enter(object[] parameters)
	{
		bool flag = IESettingsManager.Get<bool>("IndustrialHubStaticBackground");
		m_staticBackgroundButton.IsChecked = flag;
		m_background.IsStatic = flag;
		m_versionLabel.Text = string.Format(LanguageControl.Get("ContentWidgets", "IndustrialEraHubScreen", "9"), IndustrialModLoader.GetCurrentModInfo().Version);
	}

	public override void Update()
	{
		Color color = CreditsColors.RainbowMixWhite((float)Time.RealTime);
		m_creditsButton.Color = Color.White;
		m_creditsButton.CenterColor = color;
		m_creditsButton.BevelColor = color;
		m_staticBackgroundButton.Text = LanguageControl.Get("ContentWidgets", "IndustrialEraHubScreen", "3") + (m_staticBackgroundButton.IsChecked ? LanguageControl.Get("ContentWidgets", "TechTreeScreen", "On") : LanguageControl.Get("ContentWidgets", "TechTreeScreen", "Off"));
		m_background.IsStatic = m_staticBackgroundButton.IsChecked;
		bool flag = IESettingsManager.Get<bool>("IndustrialHubStaticBackground");
		if (m_staticBackgroundButton.IsChecked != flag)
		{
			IESettingsManager.Set("IndustrialHubStaticBackground", m_staticBackgroundButton.IsChecked);
			IESettingsManager.Save();
		}
		if (m_creditsButton.IsClicked)
		{
			ScreensManager.SwitchScreen("IECredits");
		}
		if (m_settingsButton.IsClicked)
		{
			ScreensManager.SwitchScreen("IESettings", ScreensManager.CurrentScreen);
		}
		if (m_techTreeMakerButton.IsClicked)
		{
			ScreensManager.SwitchScreen("IndustrialTechTreeMaker");
		}
		if (m_iconMakerButton.IsClicked)
		{
			ScreensManager.SwitchScreen("IndustrialIconMaker");
		}

		foreach (var (bevelledButtonWidget, action) in m_extraEntries)
		{
			if (bevelledButtonWidget.IsClicked)
			{
				action();
			}
		}
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
