using System.Collections;

namespace SCIENEW;

public class CoroutineNode : ICoroutineNode
{
	private ICoroutineWaitCondition waitCondition;

	public IEnumerator Fiber { get; private set; }

	public bool IsFinished { get; set; }

	public bool IsPaused { get; set; }

	public ICoroutineWaitCondition WaitCondition => waitCondition;

	public CoroutineNode(IEnumerator fiber)
	{
		Fiber = fiber;
		IsFinished = false;
		IsPaused = false;
	}

	public void AddWaitCondition(ICoroutineWaitCondition condition)
	{
		waitCondition = condition;
	}

	public bool NeedWait()
	{
		return waitCondition.keepWaiting;
	}

	public void Pause()
	{
		waitCondition.Pause();
	}

	public void Resume()
	{
		waitCondition.Resume();
	}
}
