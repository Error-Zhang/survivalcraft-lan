using System.Xml.Linq;
using Game;

namespace SCIENEW.Descriptors;

[Descriptor(typeof(WidgetAlignmentData))]
public class WidgetAlignmentDescriptor : CanvasWidget, IDescriptor
{
	protected string m_propertyName = string.Empty;

	public WidgetAlignmentData m_value;

	public LabelWidget m_labelWidget;

	public WidgetAlignmentSetterWidget m_widgetAlignmentSetterWidget;

	public string PropertyName
	{
		get
		{
			return m_propertyName;
		}
		set
		{
			m_propertyName = value;
			m_value = IESettingsManager.Get<WidgetAlignmentData>(value);
		}
	}

	public Widget GetWidget()
	{
		return this;
	}

	public WidgetAlignmentDescriptor()
	{
		XElement node = ContentManager.Get<XElement>("Widgets/Descriptors/WidgetAlignmentDescriptor");
		LoadContents(this, node);
		m_labelWidget = Children.Find<LabelWidget>("PropertyNameLabel");
		m_widgetAlignmentSetterWidget = Children.Find<WidgetAlignmentSetterWidget>("WidgetAlignmentSetter");
	}

	public override void Update()
	{
		base.Update();
		if (m_widgetAlignmentSetterWidget.IsClicked)
		{
			m_value = m_widgetAlignmentSetterWidget.Data;
			IESettingsManager.Set(PropertyName, m_value);
		}
		m_labelWidget.Text = LanguageControl.Get("IESettings", m_propertyName, "DisplayName");
		m_widgetAlignmentSetterWidget.Data = m_value;
	}
}
