using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Game;

namespace SCIENEW;

public class IESettingsScreen : Screen
{
	public StackPanelWidget m_contentPanel;

	public readonly List<Widget> m_descriptors = new List<Widget>();

	public IESettingsScreen()
	{
		XElement node = ContentManager.Get<XElement>("Screens/IESettingsScreen");
		LoadContents(this, node);
		m_contentPanel = Children.Find<StackPanelWidget>("Content");
		foreach (KeyValuePair<string, object> setting in IESettingsManager.Settings)
		{
			string key = setting.Key;
			Type type = setting.Value.GetType();
			Widget descriptor = IESettingsManager.GetDescriptor(key, type);
			if (descriptor != null)
			{
				m_descriptors.Add(descriptor);
				m_contentPanel.Children.Add(descriptor);
			}
		}
	}

	public override void Update()
	{
		if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
		{
			IESettingsManager.Save();
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
