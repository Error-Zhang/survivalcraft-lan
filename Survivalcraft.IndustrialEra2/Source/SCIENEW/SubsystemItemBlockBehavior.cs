using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemItemBlockBehavior : SubsystemBlockBehavior, IUpdateable, IDrawable
{
	public Dictionary<string, ItemBehavior[]> behaviorsMap = new Dictionary<string, ItemBehavior[]>();

	public List<IUpdateable> m_updateables = new List<IUpdateable>();

	public List<IDrawable> m_drawables = new List<IDrawable>();

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<BaseItemBlock>() };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1] { 1000 };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		foreach (KeyValuePair<string, int> item in ItemsBlockManager.IdMap)
		{
			ItemBehavior[] behaviors = ItemsBlockManager.GetItem(item.Key).GetBehaviors(Project);
			behaviorsMap.Add(item.Key, behaviors);
			m_updateables.AddRange(behaviors.OfType<IUpdateable>());
			m_drawables.AddRange(behaviors.OfType<IDrawable>());
		}
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		bool flag = false;
		string itemCraftingID = ItemsBlockManager.GetItemCraftingID(componentMiner.ActiveBlockValue);
		ItemBehavior[] array = behaviorsMap[itemCraftingID];
		if (array.Length != 0)
		{
			ItemBehavior[] array2 = array;
			foreach (ItemBehavior itemBehavior in array2)
			{
				flag |= itemBehavior.OnUse(ray, componentMiner);
			}
		}
		return flag;
	}

	public override bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem)
	{
		bool flag = false;
		string itemCraftingID = ItemsBlockManager.GetItemCraftingID(worldItem.Value);
		ItemBehavior[] array = behaviorsMap[itemCraftingID];
		if (array.Length != 0)
		{
			ItemBehavior[] array2 = array;
			foreach (ItemBehavior itemBehavior in array2)
			{
				flag |= itemBehavior.OnHitAsProjectile(cellFace, componentBody, worldItem);
			}
		}
		return flag;
	}

	public void Update(float dt)
	{
		m_updateables.ForEach((IUpdateable x) =>
		{
			x.Update(dt);
		});
	}

	public void Draw(Camera camera, int drawOrder)
	{
		m_drawables.ForEach((IDrawable x) =>
		{
			x.Draw(camera, drawOrder);
		});
	}
}
