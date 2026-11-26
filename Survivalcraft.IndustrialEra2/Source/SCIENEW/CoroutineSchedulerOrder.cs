using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SCIENEW;

public class CoroutineSchedulerOrder : ICoroutineScheduler
{
	private Queue<ICoroutineNode> coroutineQueue = new Queue<ICoroutineNode>();

	private ICoroutineNode frozenCoroutineNodeOrder;

	private bool isSchedulerPaused;

	public int CoroutineCount => coroutineQueue.Count;

	public void Reset()
	{
		foreach (ICoroutineNode item in coroutineQueue)
		{
			item.IsFinished = true;
		}
		coroutineQueue.Clear();
		frozenCoroutineNodeOrder = null;
		isSchedulerPaused = false;
	}

	public ICoroutineNode AddCoroutine(IEnumerator fiber)
	{
		if (fiber == null)
		{
			return null;
		}
		ICoroutineNode coroutineNode = new CoroutineNode(fiber);
		coroutineQueue.Enqueue(coroutineNode);
		return coroutineNode;
	}

	public void PauseCoroutine(ICoroutineNode coroutine)
	{
		coroutine.IsPaused = true;
	}

	public void ResumeCoroutine(ICoroutineNode coroutine)
	{
		coroutine.IsPaused = false;
	}

	public void PauseAll()
	{
		isSchedulerPaused = true;
	}

	public void ResumeAll()
	{
		isSchedulerPaused = false;
	}

	public void JumpTo(ICoroutineNode target)
	{
		if (target == null)
		{
			return;
		}
		List<ICoroutineNode> list = coroutineQueue.ToList();
		int num = list.IndexOf(target);
		if (num >= 0)
		{
			for (int i = 0; i < num; i++)
			{
				list[i].IsFinished = true;
			}
			list = list.Skip(num).ToList();
			coroutineQueue = new Queue<ICoroutineNode>(list);
			frozenCoroutineNodeOrder = null;
		}
	}

	public ICoroutineNode RemoveCoroutine(ICoroutineNode coroutine)
	{
		coroutine.IsFinished = true;
		if (frozenCoroutineNodeOrder == coroutine)
		{
			frozenCoroutineNodeOrder = null;
		}
		List<ICoroutineNode> list = coroutineQueue.ToList();
		list.Remove(coroutine);
		coroutineQueue = new Queue<ICoroutineNode>(list);
		return coroutine;
	}

	public void RemoveAllCoroutines()
	{
		foreach (ICoroutineNode item in coroutineQueue)
		{
			item.IsFinished = true;
		}
		frozenCoroutineNodeOrder = null;
		coroutineQueue.Clear();
	}

	public void UpdateCoroutines()
	{
		if (isSchedulerPaused || coroutineQueue.Count == 0)
		{
			return;
		}
		ICoroutineNode coroutineNode = coroutineQueue.Peek();
		if (coroutineNode.IsFinished)
		{
			coroutineQueue.Dequeue();
			return;
		}
		if (coroutineNode.IsPaused)
		{
			if ((frozenCoroutineNodeOrder == null || frozenCoroutineNodeOrder != coroutineNode) && coroutineNode.WaitCondition != null)
			{
				coroutineNode.Pause();
				frozenCoroutineNodeOrder = coroutineNode;
			}
			return;
		}
		if (frozenCoroutineNodeOrder != null && frozenCoroutineNodeOrder == coroutineNode)
		{
			coroutineNode.Resume();
			frozenCoroutineNodeOrder = null;
		}
		if (coroutineNode.WaitCondition == null || !coroutineNode.NeedWait())
		{
			MoveNextCoroutine(coroutineNode);
		}
	}

	private void MoveNextCoroutine(ICoroutineNode coroutine)
	{
		if (coroutine.Fiber.MoveNext())
		{
			object current = coroutine.Fiber.Current;
			ICoroutineWaitCondition coroutineWaitCondition = current as ICoroutineWaitCondition;
			if (current is IEnumerator fiber)
			{
				AddCoroutine(fiber);
				return;
			}
			if (coroutineWaitCondition == null)
			{
				throw new Exception("yield return type error");
			}
			coroutine.AddWaitCondition(coroutineWaitCondition);
		}
		else
		{
			coroutine.IsFinished = true;
			coroutineQueue.Dequeue();
		}
	}
}
