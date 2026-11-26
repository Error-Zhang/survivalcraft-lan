using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Screens;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentGReactorControllerScreenProvider : Component, IScreenProviderComponent
{
	public ComponentBlockEntity m_componentBlockEntity;

	public ComponentGReactorController m_componentGReactorController;

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public FontBatch3D m_fontBatch3D;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_componentGReactorController = Entity.FindComponent<ComponentGReactorController>(throwOnError: true);
		m_fontBatch3D = m_primitivesRenderer3D.FontBatch(IndustrialModLoader.PixelFont, 0, null, SCIENEW.Screens.Screen.FontRasterizerState);
	}

	public void Draw(SCIENEW.Screens.Screen screen, Project project, Camera camera, int drawOrder)
	{
		screen.GetTextAxes(0.003f, out var right, out var down);
		m_fontBatch3D.QueueText(m_componentGReactorController.GetStateText(), screen.CenterPos, right, down, Color.Green, TextAnchor.Center);
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}

	public Entity GetEntity()
	{
		return Entity;
	}

	public Vector3 GetPosition()
	{
		return new Vector3(m_componentBlockEntity.Coordinates) + Vector3.One * 0.5f;
	}

	public BoundingBox GetBoundingBox()
	{
		return new BoundingBox(new Vector3(m_componentBlockEntity.Coordinates), new Vector3(m_componentBlockEntity.Coordinates) + Vector3.One);
	}

	public string GetDescription()
	{
		return LanguageUtils.GetText(this, "ProviderDescription");
	}
}
