using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class WidgetAlignmentSetterWidget : CanvasWidget
{
	public GridPanelWidget m_grid;

	public Vector2 m_size;

	public WidgetAlignmentData m_data;

	public List<BevelledButtonWidget> m_buttons = new List<BevelledButtonWidget>();

	public new Vector2 Size
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
			m_buttons.ForEach((BevelledButtonWidget x) =>
			{
				x.Size = new Vector2(value.X / (float)m_grid.ColumnsCount, value.Y / (float)m_grid.RowsCount);
			});
		}
	}

	public WidgetAlignmentData Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public bool IsClicked => m_buttons.Any((BevelledButtonWidget x) => x.IsClicked);

	public WidgetAlignmentSetterWidget()
	{
		XElement node = ContentManager.Get<XElement>("Widgets/WidgetAlignmentSetterWidget");
		LoadContents(this, node);
		m_grid = Children.Find<GridPanelWidget>("Grid");
		for (int i = 0; i < m_grid.RowsCount; i++)
		{
			for (int j = 0; j < m_grid.ColumnsCount; j++)
			{
				BevelledButtonWidget bevelledButtonWidget = new BevelledButtonWidget
				{
					IsAutoCheckingEnabled = true,
					Size = new Vector2(310f / (float)m_grid.ColumnsCount, 60f / (float)m_grid.RowsCount)
				};
				m_grid.Children.Add(bevelledButtonWidget);
				m_grid.SetWidgetCell(bevelledButtonWidget, new Point2(i, j));
				m_buttons.Add(bevelledButtonWidget);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].IsClicked)
			{
				m_data.Horizontal = (WidgetAlignment)(i / 3);
				m_data.Vertical = (WidgetAlignment)(i % 3);
			}
		}
		UpdateState();
	}

	public void UpdateState()
	{
		int num = 0;
		foreach (BevelledButtonWidget button in m_buttons)
		{
			button.IsChecked = (int)m_data.Horizontal * 3 + m_data.Vertical == (WidgetAlignment)num;
			Color bevelColor = (button.CenterColor = (((int)m_data.Horizontal * 3 + m_data.Vertical == (WidgetAlignment)num) ? new Color(50, 150, 35) : new Color(181, 172, 154)));
			button.BevelColor = bevelColor;
			num++;
		}
	}
}
