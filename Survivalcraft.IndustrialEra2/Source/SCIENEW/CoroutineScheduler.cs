using System;
using System.Collections;
using System.Collections.Generic;

namespace SCIENEW;

public class CoroutineScheduler : ICoroutineScheduler
{
	private List<ICoroutineNode> coroutineList = new List<ICoroutineNode>();

	private HashSet<ICoroutineNode> frozenCoroutineHashSet = new HashSet<ICoroutineNode>();

	private List<ICoroutineNode> finishedCoroutines = new List<ICoroutineNode>();

	public int CoroutineCount => coroutineList.Count;

	public ICoroutineNode AddCoroutine(IEnumerator fiber)
	{
		if (fiber == null)
		{
			return null;
		}
		ICoroutineNode coroutineNode = new CoroutineNode(fiber);
		coroutineList.Add(coroutineNode);
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

	public ICoroutineNode RemoveCoroutine(ICoroutineNode coroutine)
	{
		coroutine.IsFinished = true;
		if (frozenCoroutineHashSet.Contains(coroutine))
		{
			frozenCoroutineHashSet.Remove(coroutine);
		}
		return coroutine;
	}

	public void RemoveAllCoroutines()
	{
		foreach (ICoroutineNode coroutine in coroutineList)
		{
			coroutine.IsFinished = true;
		}
		frozenCoroutineHashSet.Clear();
	}

	public void UpdateCoroutines()
	{
		if (coroutineList.Count == 0)
		{
			return;
		}
		foreach (ICoroutineNode coroutine in coroutineList)
		{
			if (coroutine.IsFinished)
			{
				finishedCoroutines.Add(coroutine);
				continue;
			}
			if (coroutine.IsPaused)
			{
				if (!frozenCoroutineHashSet.Contains(coroutine) && coroutine.WaitCondition != null)
				{
					coroutine.Pause();
					frozenCoroutineHashSet.Add(coroutine);
				}
				continue;
			}
			if (frozenCoroutineHashSet.Contains(coroutine))
			{
				coroutine.Resume();
				frozenCoroutineHashSet.Remove(coroutine);
			}
			if (coroutine.WaitCondition == null || !coroutine.NeedWait())
			{
				MoveNextCoroutine(coroutine);
			}
		}
		foreach (ICoroutineNode finishedCoroutine in finishedCoroutines)
		{
			coroutineList.Remove(finishedCoroutine);
		}
		finishedCoroutines.Clear();
	}

	private void MoveNextCoroutine(ICoroutineNode coroutine)
	{
		if (coroutine.Fiber.MoveNext())
		{
			if (!(coroutine.Fiber.Current is ICoroutineWaitCondition condition))
			{
				throw new Exception("yield return type error");
			}
			coroutine.AddWaitCondition(condition);
		}
		else
		{
			coroutine.IsFinished = true;
			finishedCoroutines.Add(coroutine);
		}
	}
}
