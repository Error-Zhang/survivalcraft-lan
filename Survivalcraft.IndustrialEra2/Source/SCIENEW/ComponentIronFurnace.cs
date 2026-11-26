using Engine;

namespace SCIENEW;

public class ComponentIronFurnace : ComponentIEFurnace
{
	public bool m_fireActive;

	public override float SmeltSpeed => 0.2f;

	protected override void UpdateFurnaceFireParticles(int cellValue)
	{
		m_fireParticleSystem.m_position = m_componentBlockEntity.Position + new Vector3(0.5f, 0.2f, 0.5f);
		float heatLevel = m_heatLevel;
		if (!(heatLevel > 0f))
		{
			if (heatLevel <= 0f && m_fireActive)
			{
				m_fireActive = false;
				m_subsystemParticles.RemoveParticleSystem(m_fireParticleSystem);
			}
		}
		else if (!m_fireActive)
		{
			m_fireActive = true;
			m_subsystemParticles.AddParticleSystem(m_fireParticleSystem);
		}
	}

	protected override void ReplaceFurnaceBlock(int cellValue)
	{
		int direction_Static = FourDirectionModelDevice.GetDirection_Static(cellValue);
		int deviceValueByType = DevicesBlockManager.GetDeviceValueByType<IronFurnaceDevice>((m_heatLevel > 0f) ? (4 | direction_Static) : direction_Static);
		m_componentBlockEntity.BlockValue = deviceValueByType;
	}
}
