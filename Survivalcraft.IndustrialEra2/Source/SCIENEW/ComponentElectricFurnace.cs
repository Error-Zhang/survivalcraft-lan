using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentElectricFurnace : ComponentIEFurnace, IUpdateable
{
	private WorkState m_previousWorkState;

	public override float SmeltSpeed => 0.2f;

	public int CircSlot1Index => SlotsCount - 3;

	public int CircSlot2Index => SlotsCount - 4;

	protected override void OnBeginUpdate(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		VoltElement voltElement = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true).GetVoltElement(coordinates) as ElectricFurnaceVoltElement;
		bool flag = voltElement != null && voltElement.CurrentWorkState == WorkState.Active;
		m_heatLevel = (flag ? 3 : 0);
		m_fireTimeRemaining = (flag ? 999 : 0);
		if (!flag)
		{
			m_smeltingProgress = 0f;
			m_smeltingRecipe = null;
		}
		if (m_previousWorkState != voltElement?.CurrentWorkState && voltElement != null)
		{
			m_previousWorkState = voltElement.CurrentWorkState;
			m_updateSmeltingRecipe = true;
		}
	}

	public override bool UseFuel()
	{
		return false;
	}

	protected override void ReplaceFurnaceBlock(int cellValue)
	{
		int direction_Static = FourDirectionModelDevice.GetDirection_Static(cellValue);
		int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<ElectricFurnaceDevice>((m_heatLevel > 0f) ? (4 | direction_Static) : direction_Static);
		m_componentBlockEntity.BlockValue = deviceValueByType;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_furnaceSize = SlotsCount - 4;
	}
}
