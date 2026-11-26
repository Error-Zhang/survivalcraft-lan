using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SCIENEW;

public class CoroutineSchedulerStage : ICoroutineScheduler
{
	private Queue<ICoroutineNode> coroutineQueue = new Queue<ICoroutineNode>();

	private ICoroutineNode frozenCoroutineNodeOrder;

	private bool isSchedulerPaused;

	private List<int> stageStartIndices = new List<int>();

	private List<ICoroutineNode> coroutineList = new List<ICoroutineNode>();

	public int CoroutineCount => coroutineQueue.Count;

	public int StageCount => stageStartIndices.Count;

	public ICoroutineNode AddCoroutine(IEnumerator fiber)
	{
		if (fiber == null)
		{
			return null;
		}
		ICoroutineNode coroutineNode = new CoroutineNode(fiber);
		coroutineQueue.Enqueue(coroutineNode);
		coroutineList.Add(coroutineNode);
		return coroutineNode;
	}

	public void DefineStages(List<ICoroutineNode> stageStartCoroutines)
	{
		stageStartIndices.Clear();
		for (int i = 0; i < stageStartCoroutines.Count; i++)
		{
			int num = coroutineList.IndexOf(stageStartCoroutines[i]);
			if (num >= 0)
			{
				stageStartIndices.Add(num);
			}
		}
	}

	public void JumpToStage(int stageIndex)
	{
		if (stageIndex < 0 || stageIndex >= stageStartIndices.Count)
		{
			return;
		}
		int count = stageStartIndices[stageIndex];
		List<ICoroutineNode> collection = coroutineList.Skip(count).ToList();
		foreach (ICoroutineNode item in coroutineList.Take(count))
		{
			item.IsFinished = true;
		}
		coroutineQueue = new Queue<ICoroutineNode>(collection);
		frozenCoroutineNodeOrder = null;
	}

	public void JumpToStageInstantly(int stageIndex)
	{
		if (stageIndex < 0 || stageIndex >= stageStartIndices.Count)
		{
			return;
		}
		int count = stageStartIndices[stageIndex];
		foreach (ICoroutineNode item in coroutineList.Take(count))
		{
			while (item.Fiber.MoveNext())
			{
				if (item.Fiber.Current is ICoroutineWaitCondition condition)
				{
					item.AddWaitCondition(condition);
				}
			}
			item.IsFinished = true;
		}
		List<ICoroutineNode> collection = coroutineList.Skip(count).ToList();
		coroutineQueue = new Queue<ICoroutineNode>(collection);
		frozenCoroutineNodeOrder = null;
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
		int num = coroutineList.IndexOf(target);
		if (num >= 0)
		{
			for (int i = 0; i < num; i++)
			{
				coroutineList[i].IsFinished = true;
			}
			List<ICoroutineNode> collection = coroutineList.Skip(num).ToList();
			coroutineQueue = new Queue<ICoroutineNode>(collection);
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
		coroutineList.Remove(coroutine);
		coroutineQueue = new Queue<ICoroutineNode>(coroutineQueue.Where((ICoroutineNode c) => c != coroutine));
		return coroutine;
	}

	public void RemoveAllCoroutines()
	{
		foreach (ICoroutineNode item in coroutineQueue)
		{
			item.IsFinished = true;
		}
		coroutineQueue.Clear();
		coroutineList.Clear();
		frozenCoroutineNodeOrder = null;
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
			if (frozenCoroutineNodeOrder != coroutineNode && coroutineNode.WaitCondition != null)
			{
				coroutineNode.Pause();
				frozenCoroutineNodeOrder = coroutineNode;
			}
			return;
		}
		if (frozenCoroutineNodeOrder == coroutineNode)
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
			if (!(coroutine.Fiber.Current is ICoroutineWaitCondition condition))
			{
				throw new Exception("yield return type error");
			}
			coroutine.AddWaitCondition(condition);
		}
		else
		{
			coroutine.IsFinished = true;
			coroutineQueue.Dequeue();
		}
	}
}
