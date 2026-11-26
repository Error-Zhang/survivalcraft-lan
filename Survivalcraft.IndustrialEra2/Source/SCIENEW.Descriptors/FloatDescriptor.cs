using System.Xml.Linq;
using Game;

namespace SCIENEW.Descriptors;

[Descriptor(typeof(float))]
public class FloatDescriptor : CanvasWidget, IDescriptor
{
	protected string m_propertyName = string.Empty;

	public float m_value;

	public LabelWidget m_labelWidget;

	public SliderWidget m_sliderWidget;

	public string PropertyName
	{
		get
		{
			return m_propertyName;
		}
		set
		{
			m_propertyName = value;
			m_value = IESettingsManager.Get<float>(value);
		}
	}

	public Widget GetWidget()
	{
		return this;
	}

	public FloatDescriptor()
	{
		XElement node = ContentManager.Get<XElement>("Widgets/Descriptors/FloatDescriptor");
		LoadContents(this, node);
		m_labelWidget = Children.Find<LabelWidget>("PropertyNameLabel");
		m_sliderWidget = Children.Find<SliderWidget>("Slider");
	}

	public override void Update()
	{
		base.Update();
		if (m_sliderWidget.IsSliding)
		{
			m_value = m_sliderWidget.Value;
			IESettingsManager.Set(PropertyName, m_value);
		}
		m_labelWidget.Text = LanguageControl.Get("IESettings", m_propertyName, "DisplayName");
		m_sliderWidget.Value = m_value;
	}
}
