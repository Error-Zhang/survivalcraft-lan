using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Engine;
using RecipaediaEX.UI;
using SCIENEW;
using SCIENEW.RecipaediaEX.Items;

namespace Game;

public class FluidRecipaediaDescriptionScreen : Screen
{
	public FluidWidget m_fluidIconWidget;

	public LabelWidget m_nameWidget;

	public ButtonWidget m_leftButtonWidget;

	public ButtonWidget m_rightButtonWidget;

	public LabelWidget m_descriptionWidget;

	public LabelWidget m_propertyNames1Widget;

	public LabelWidget m_propertyValues1Widget;

	public LabelWidget m_propertyNames2Widget;

	public LabelWidget m_propertyValues2Widget;

	public int m_index;

	public IList<FluidItem> m_valuesList;

	public static string fName = "FluidRecipaediaDescriptionScreen";

	public FluidRecipaediaDescriptionScreen()
	{
		XElement node = ContentManager.Get<XElement>("Screens/FluidRecipaediaDescriptionScreen");
		LoadContents(this, node);
		m_fluidIconWidget = Children.Find<FluidWidget>("Icon");
		m_nameWidget = Children.Find<LabelWidget>("Name");
		m_leftButtonWidget = Children.Find<ButtonWidget>("Left");
		m_rightButtonWidget = Children.Find<ButtonWidget>("Right");
		m_descriptionWidget = Children.Find<LabelWidget>("Description");
		m_propertyNames1Widget = Children.Find<LabelWidget>("PropertyNames1");
		m_propertyValues1Widget = Children.Find<LabelWidget>("PropertyValues1");
		m_propertyNames2Widget = Children.Find<LabelWidget>("PropertyNames2");
		m_propertyValues2Widget = Children.Find<LabelWidget>("PropertyValues2");
	}

	public override void Enter(object[] parameters)
	{
		IRecipaediaItem item = (IRecipaediaItem)parameters[0];
		IList<IRecipaediaItem> list = (IList<IRecipaediaItem>)parameters[1];
		if (list.Any((IRecipaediaItem x) => !(x is FluidItem)))
		{
			throw new ArgumentException("All element in the list must be FluidItem");
		}
		m_index = list.IndexOf(item);
		m_valuesList = list.OfType<FluidItem>().ToList();
		m_index = list.Select((IRecipaediaItem value, int i) => new
		{
			Index = i,
			Value = value
		}).First(x => x.Value == item).Index;
		UpdateFluidProperties();
	}

	public override void Update()
	{
		m_leftButtonWidget.IsEnabled = m_index > 0;
		m_rightButtonWidget.IsEnabled = m_index < m_valuesList.Count - 1;
		if (m_leftButtonWidget.IsClicked || Input.Left)
		{
			m_index = MathUtils.Max(m_index - 1, 0);
			UpdateFluidProperties();
		}
		if (m_rightButtonWidget.IsClicked || Input.Right)
		{
			m_index = MathUtils.Min(m_index + 1, m_valuesList.Count - 1);
			UpdateFluidProperties();
		}
		if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
		{
			ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
		}
	}

	public Dictionary<string, string> GetFluidProperties(int value)
	{
		Fluid fluid = FluidsManager.GetFluid(value);
		return FluidItem.BuildRawProperties(value, fluid);
	}

	public void UpdateFluidProperties()
	{
		if (m_index < 0 || m_index >= m_valuesList.Count)
		{
			return;
		}
		FluidItem fluidItem = m_valuesList[m_index];
		int fluidValue = fluidItem.m_fluidValue;
		m_fluidIconWidget.m_fluidValue = fluidValue;
		m_fluidIconWidget.m_relativeVolume = 1f;
		m_nameWidget.Text = fluidItem.Name;
		m_descriptionWidget.Text = fluidItem.Description;
		m_propertyNames1Widget.Text = string.Empty;
		m_propertyValues1Widget.Text = string.Empty;
		m_propertyNames2Widget.Text = string.Empty;
		m_propertyValues2Widget.Text = string.Empty;
		Dictionary<string, string> properties = fluidItem.GetProperties();
		int num = 0;
		foreach (KeyValuePair<string, string> item in properties)
		{
			if (num < properties.Count - properties.Count / 2)
			{
				LabelWidget propertyNames1Widget = m_propertyNames1Widget;
				propertyNames1Widget.Text = propertyNames1Widget.Text + item.Key + ":\n";
				LabelWidget propertyValues1Widget = m_propertyValues1Widget;
				propertyValues1Widget.Text = propertyValues1Widget.Text + item.Value + "\n";
			}
			else
			{
				LabelWidget propertyNames2Widget = m_propertyNames2Widget;
				propertyNames2Widget.Text = propertyNames2Widget.Text + item.Key + ":\n";
				LabelWidget propertyValues2Widget = m_propertyValues2Widget;
				propertyValues2Widget.Text = propertyValues2Widget.Text + item.Value + "\n";
			}
			num++;
		}
	}
}
