using Engine;
using Engine.Input;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentIEGui : Component, IUpdateable
{
	public BitmapButtonWidget m_techTreeButton;

	public ComponentPlayer m_componentPlayer;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
		m_techTreeButton = m_componentPlayer.GuiWidget.Children.Find<BitmapButtonWidget>("TechTreeButton", throwIfNotFound: false);
		if (m_techTreeButton == null)
		{
			m_techTreeButton = new BitmapButtonWidget
			{
				Name = "TechTreeButton",
				NormalSubtexture = ContentManager.Get<Subtexture>("Textures/Gui/TechTreeButton1"),
				ClickedSubtexture = ContentManager.Get<Subtexture>("Textures/Gui/TechTreeButton2"),
				Size = new Vector2(68f, 64f),
				Margin = new Vector2(4f, 0f)
			};
			m_componentPlayer.GuiWidget.Children.Find<StackPanelWidget>("MoreContents").Children.Add(m_techTreeButton);
		}
	}

	public void Update(float dt)
	{
		if (m_techTreeButton.IsClicked)
		{
			Project.FindSubsystem<SubsystemTechTree>(throwOnError: true).Show();
		}
		if (Keyboard.IsKeyDownOnce(Key.F1))
		{
			DialogsManager.ShowDialog(m_componentPlayer.GuiWidget, new BlockPonderDialog(Project));
		}
	}
}
