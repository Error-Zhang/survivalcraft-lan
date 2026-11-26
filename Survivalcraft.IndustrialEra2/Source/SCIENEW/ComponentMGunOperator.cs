using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentMGunOperator : Component, IUpdateable
{
	public ComponentPlayer m_componentPlayer;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
	}

	public void Update(float dt)
	{
		ComponentMount mount = m_componentPlayer.ComponentRider.Mount;
		PlayerInput playerInput = m_componentPlayer.ComponentInput.PlayerInput;
		if (mount == null)
		{
			return;
		}
		ComponentMGun componentMGun = mount.Entity.FindComponent<ComponentMGun>();
		if (!playerInput.Aim.HasValue || componentMGun == null)
		{
			return;
		}
		Ray3 value = playerInput.Aim.Value;
		Vector3 forward = mount.ComponentBody.Matrix.Forward;
		new Vector3(0f, 0f, 0f);
		if (!(value.Direction.Cross(forward).Length() > 0.3f))
		{
			Ray3 value2 = playerInput.Aim.Value;
			playerInput.Dig = null;
			playerInput.Interact = null;
			m_componentPlayer.ComponentAimingSights.ShowAimingSights(value2.Position, value2.Direction);
			componentMGun.vect = value2.Direction;
			if ((float)componentMGun.fire == 0f)
			{
				componentMGun.fire = 1;
			}
		}
	}
}
