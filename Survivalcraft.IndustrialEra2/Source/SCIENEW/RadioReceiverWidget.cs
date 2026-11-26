using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class RadioReceiverWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public SliderWidget m_frequencySlider;

	public ButtonWidget m_plusFrequencyButton;

	public ButtonWidget m_minusFrequencyButton;

	public LabelWidget m_frequencyLabel;

	public RadioReceiverElectricElement m_receiver;

	private bool m_wasSliding;

	public RadioReceiverWidget(IInventory inventory, RadioReceiverElectricElement receiver)
	{
		m_receiver = receiver;
		XElement node = ContentManager.Get<XElement>("Widgets/RadioReceiverWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_frequencySlider = Children.Find<SliderWidget>("FrequencySlider");
		m_plusFrequencyButton = Children.Find<ButtonWidget>("PlusFrequencyButton");
		m_minusFrequencyButton = Children.Find<ButtonWidget>("MinusFrequencyButton");
		m_frequencyLabel = Children.Find<LabelWidget>("FrequencyLabel");
		int num = 10;
		for (int i = 0; i < m_inventoryGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_inventoryGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
			}
		}
		m_frequencySlider.Value = m_receiver.Frequency;
		m_frequencySlider.MinValue = 1000f;
		m_frequencySlider.MaxValue = 50000f;
		m_frequencyLabel.TextAnchor = TextAnchor.Center;
	}

	public override void Update()
	{
		int frequency = (int)m_receiver.Frequency;
		m_plusFrequencyButton.IsEnabled = frequency < SubsystemRadioElectricElements.MaxFrequency;
		m_minusFrequencyButton.IsEnabled = frequency > SubsystemRadioElectricElements.MinFrequency;
		if (m_frequencySlider.IsSliding)
		{
			frequency = (int)m_frequencySlider.Value;
		}
		else if (m_wasSliding)
		{
			IndustrialEraLanAdapter.SetRadioChannel(m_receiver, m_receiver.m_cellFace.Point, (int)m_frequencySlider.Value);
		}
		else
		{
			if (m_plusFrequencyButton.IsClicked)
			{
				IndustrialEraLanAdapter.SetRadioChannel(m_receiver, m_receiver.m_cellFace.Point, frequency + 1000);
			}
			else if (m_minusFrequencyButton.IsClicked)
			{
				IndustrialEraLanAdapter.SetRadioChannel(m_receiver, m_receiver.m_cellFace.Point, frequency - 1000);
			}
			m_frequencySlider.Value = frequency;
		}
		m_wasSliding = m_frequencySlider.IsSliding;
		m_frequencyLabel.Text = string.Format(LanguageControl.GetContentWidgets("RadioTransmitterWidget", 3), frequency);
	}
}
