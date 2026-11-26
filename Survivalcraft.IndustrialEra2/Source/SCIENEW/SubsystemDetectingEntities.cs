using System.Collections.Generic;
using Engine;
using GameEntitySystem;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW;

public class SubsystemDetectingEntities : Subsystem
{
	public List<IDetectableComponent> m_detectableComponents = new List<IDetectableComponent>();

	public override void OnEntityAdded(Entity entity)
	{
		base.OnEntityAdded(entity);
		IDetectableComponent detectableComponent = entity.FindComponent<IDetectableComponent>();
		if (detectableComponent != null)
		{
			m_detectableComponents.Add(detectableComponent);
		}
	}

	public override void OnEntityRemoved(Entity entity)
	{
		base.OnEntityRemoved(entity);
		IDetectableComponent detectableComponent = entity.FindComponent<IDetectableComponent>();
		if (detectableComponent != null)
		{
			m_detectableComponents.Remove(detectableComponent);
		}
	}

	public Vector2[] GetRelativePositions(Vector2 center, float radius, int countLimit)
	{
		return (from v in (from x in m_detectableComponents.AsValueEnumerable()
				where (x.GetPosition() - center).Length() <= radius
				select (x.GetPosition() - center) / radius).ToList<ListWhereSelect<IDetectableComponent, Vector2>, Vector2>().AsValueEnumerable()
			orderby v.Length()
			select v).Take<FromList<Vector2>, Vector2, float>(countLimit).ToArray<OrderBySkipTake<FromList<Vector2>, Vector2, float>, Vector2>();
	}
}
