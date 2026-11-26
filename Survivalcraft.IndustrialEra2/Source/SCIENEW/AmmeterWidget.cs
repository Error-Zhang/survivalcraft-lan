using System.Xml.Linq;
using Game;

namespace SCIENEW;

public class AmmeterWidget : CanvasWidget
{
	public ComponentAmmeter m_ammeter;

	public LabelWidget m_titleLabel;

	public LabelWidget m_currentLabel;

	public LabelWidget m_rangeLabel;

	public SliderWidget m_rangeSlider;

	public AmmeterWidget(IInventory inventory, ComponentAmmeter ammeter)
	{
		m_ammeter = ammeter;
		XElement node = ContentManager.Get<XElement>("Widgets/AmmeterWidget");
		LoadContents(this, node);
		m_titleLabel = Children.Find<LabelWidget>("TitleLabel");
		m_currentLabel = Children.Find<LabelWidget>("CurrentLabel");
		m_rangeLabel = Children.Find<LabelWidget>("RangeLabel");
		m_rangeSlider = Children.Find<SliderWidget>("RangeSlider");
		m_rangeSlider.MinValue = 4f;
		m_rangeSlider.MaxValue = 64f;
		m_rangeSlider.Granularity = 4f;
		m_rangeSlider.Value = m_ammeter.Range;
	}

	public override void Update()
	{
		m_currentLabel.Text = string.Format(LanguageControl.GetContentWidgets("AmmeterWidget", 2), m_ammeter.Current);
		m_rangeLabel.Text = string.Format(LanguageControl.GetContentWidgets("AmmeterWidget", 3), m_ammeter.Range.ToString("F0"));
		if (m_rangeSlider.IsSliding)
		{
			m_ammeter.Range = m_rangeSlider.Value;
		}
		else
		{
			m_rangeSlider.Value = m_ammeter.Range;
		}
		if (!m_ammeter.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
