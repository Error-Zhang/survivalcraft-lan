using System;
using System.Xml.Linq;
using Engine;
using Game;
using SCIENEW.Tech;
using SCIENEW.Utils;

namespace SCIENEW;

public class TechTreeNodeInfoWidget : CanvasWidget
{
	public TechNode m_node;

	public TechTreeBlockIconWidget m_blockIcon;

	public LabelWidget m_titleLabel;

	public LabelWidget m_descLabel;

	public BevelledButtonWidget m_recipeButton;

	public BevelledButtonWidget m_descButton;

	public LinkWidget m_helpLink;

	public Action OnRecipeButtonClicked;

	public Action OnDescButtonClicked;

	public TechNode Node
	{
		get
		{
			return m_node;
		}
		set
		{
			m_node = value;
		}
	}

	public TechTreeNodeInfoWidget()
	{
		XElement node = ContentManager.Get<XElement>("Widgets/TechTreeNodeInfoWidget");
		LoadContents(this, node);
		m_blockIcon = Children.Find<TechTreeBlockIconWidget>("Icon");
		m_titleLabel = Children.Find<LabelWidget>("Title");
		m_descLabel = Children.Find<LabelWidget>("Description");
		m_recipeButton = Children.Find<BevelledButtonWidget>("RecipeButton");
		m_descButton = Children.Find<BevelledButtonWidget>("DescriptionButton");
		m_helpLink = Children.Find<LinkWidget>("HelpLink");
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		base.MeasureOverride(parentAvailableSize);
		if (m_node != null)
		{
			m_blockIcon.Value = m_node.IconBlock;
			m_titleLabel.Text = m_node.DisplayName;
			m_descLabel.Text = m_node.Description;
			if (HelpsManager.GetLink(m_node.IconBlock, out string link))
			{
				m_helpLink.Text = LanguageUtils.GetText(this, "3") + link;
				m_helpLink.Url = link;
				m_helpLink.IsVisible = true;
			}
			else
			{
				m_helpLink.IsVisible = false;
			}
			m_recipeButton.IsVisible = true;
			m_descButton.IsVisible = true;
		}
		else
		{
			m_blockIcon.Value = 0;
			LabelWidget titleLabel = m_titleLabel;
			string text = (m_descLabel.Text = string.Empty);
			titleLabel.Text = text;
			m_recipeButton.IsVisible = false;
			m_descButton.IsVisible = false;
			LinkWidget helpLink = m_helpLink;
			text = (m_helpLink.Url = string.Empty);
			helpLink.Text = text;
			m_helpLink.IsVisible = false;
		}
	}
}
