using System.Xml.Linq;
using Game;

namespace SCIENEW.Descriptors;

[Descriptor(typeof(bool))]
public class BoolDescriptor : CanvasWidget, IDescriptor
{
	protected string m_propertyName = string.Empty;

	public bool m_value;

	public LabelWidget m_labelWidget;

	public BevelledButtonWidget m_bevelledButtonWidget;

	public string PropertyName
	{
		get
		{
			return m_propertyName;
		}
		set
		{
			m_propertyName = value;
			m_value = IESettingsManager.Get<bool>(value);
		}
	}

	public Widget GetWidget()
	{
		return this;
	}

	public BoolDescriptor()
	{
		XElement node = ContentManager.Get<XElement>("Widgets/Descriptors/BoolDescriptor");
		LoadContents(this, node);
		m_labelWidget = Children.Find<LabelWidget>("PropertyNameLabel");
		m_bevelledButtonWidget = Children.Find<BevelledButtonWidget>("Button");
	}

	public override void Update()
	{
		base.Update();
		m_value = IESettingsManager.Get<bool>(m_propertyName);
		if (m_bevelledButtonWidget.IsClicked)
		{
			IESettingsManager.Set(m_propertyName, !m_value);
		}
		m_value = IESettingsManager.Get<bool>(m_propertyName);
		m_labelWidget.Text = LanguageControl.Get("IESettings", m_propertyName, "DisplayName");
		m_bevelledButtonWidget.Text = LanguageControl.Get("IESettings", m_propertyName, m_value ? "TrueText" : "FalseText");
	}
}
