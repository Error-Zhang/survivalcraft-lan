using System.Linq;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentEntityOpener : Component, IUpdateable
{
	public ComponentRider m_componentRider;

	public ComponentGui m_componentGui;

	public bool m_isOpening;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentRider = Entity.FindComponent<ComponentRider>(throwOnError: true);
		m_componentGui = Entity.FindComponent<ComponentGui>(throwOnError: true);
	}

	public Widget OpenEntity(IInventory inventory, Entity entity)
	{
		string text = entity.ValuesDictionary.Keys.ToArray()[0].ToString();
		if (m_isOpening)
		{
			m_isOpening = false;
			return null;
		}
		m_isOpening = true;
		switch (text)
		{
		case "SteamBoat":
			if (entity.FindComponent<ComponentSteamEngineV>() != null)
			{
				return new SteamBoatWidget(inventory, entity.FindComponent<ComponentSteamEngineV>());
			}
			break;
		case "Train":
			if (entity.FindComponent<ComponentSteamEngineV>() != null)
			{
				return new SteamTrainWidget(inventory, entity.FindComponent<ComponentSteamEngineV>());
			}
			break;
		case "Carriage":
			if (entity.FindComponent<ComponentChest>() != null)
			{
				return new ChestNameWidget(inventory, entity.FindComponent<ComponentChest>(), "CarriageWidget");
			}
			break;
		case "MGun":
			if (entity.FindComponent<ComponentChest>() != null)
			{
				return new ChestNameWidget(inventory, entity.FindComponent<ComponentChest>(), "MachineGunWidget");
			}
			break;
		case "GasolineBoat":
			if (entity.FindComponent<ComponentCombustionEngine>() != null)
			{
				return new GasolineVehicleWidget(inventory, entity.FindComponent<ComponentCombustionEngine>(), LanguageControl.GetContentWidgets("GasolineVehicleWidget", 4));
			}
			break;
		case "Car":
			if (entity.FindComponent<ComponentCombustionEngine>() != null)
			{
				return new GasolineVehicleWidget(inventory, entity.FindComponent<ComponentCombustionEngine>(), LanguageControl.GetContentWidgets("GasolineVehicleWidget", 5));
			}
			break;
		case "Airship":
			if (entity.FindComponent<ComponentCombustionEngine>() != null)
			{
				return new GasolineVehicleWidget(inventory, entity.FindComponent<ComponentCombustionEngine>(), LanguageControl.GetContentWidgets("GasolineVehicleWidget", 6));
			}
			break;
		case "Tractor":
			if (entity.FindComponent<ComponentCombustionEngine>() != null)
			{
				return new GasolineVehicleWidget(inventory, entity.FindComponent<ComponentCombustionEngine>(), LanguageControl.GetContentWidgets("GasolineVehicleWidget", 6));
			}
			break;
		case "SubMarine":
			if (entity.FindComponent<ComponentSubMarine>() != null)
			{
				return new SteamBoatWidget(inventory, entity.FindComponent<ComponentSteamEngineV>());
			}
			break;
		case "PortableRadar":
			if (entity.FindComponent<ComponentRadar>() != null && entity.FindComponent<IInventory>() != null)
			{
				return new PortableRadarWidget(inventory, entity.FindComponent<ComponentRadar>(), entity.FindComponent<IInventory>());
			}
			break;
		}
		m_isOpening = false;
		return null;
	}

	public void Update(float dt)
	{
		m_isOpening &= m_componentRider.Mount != null;
		m_isOpening &= m_componentGui.ModalPanelWidget != null;
	}
}
