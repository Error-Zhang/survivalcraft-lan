using System.Xml.Linq;
using Game;

namespace SCIENEW;

public class VoltmeterWidget : CanvasWidget
{
	public ComponentVoltmeter m_voltmeter;

	public LabelWidget m_titleLabel;

	public LabelWidget m_voltageLabel;

	public LabelWidget m_rangeLabel;

	public SliderWidget m_rangeSlider;

	public VoltmeterWidget(IInventory inventory, ComponentVoltmeter voltmeter)
	{
		m_voltmeter = voltmeter;
		XElement node = ContentManager.Get<XElement>("Widgets/VoltmeterWidget");
		LoadContents(this, node);
		m_titleLabel = Children.Find<LabelWidget>("TitleLabel");
		m_voltageLabel = Children.Find<LabelWidget>("VoltageLabel");
		m_rangeLabel = Children.Find<LabelWidget>("RangeLabel");
		m_rangeSlider = Children.Find<SliderWidget>("RangeSlider");
		m_rangeSlider.MinValue = 1f;
		m_rangeSlider.MaxValue = 4f;
		m_rangeSlider.Granularity = 1f;
		m_rangeSlider.Value = m_voltmeter.Range;
	}

	public override void Update()
	{
		m_voltageLabel.Text = string.Format(LanguageControl.GetContentWidgets("VoltmeterWidget", 2), m_voltmeter.Voltage);
		m_rangeLabel.Text = string.Format(LanguageControl.GetContentWidgets("VoltmeterWidget", 3), m_voltmeter.Range.ToString("F0"));
		if (m_rangeSlider.IsSliding)
		{
			m_voltmeter.Range = m_rangeSlider.Value;
		}
		else
		{
			m_rangeSlider.Value = m_voltmeter.Range;
		}
		if (!m_voltmeter.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
