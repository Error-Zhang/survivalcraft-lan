namespace SCIENEW;

public struct WaitForCoroutineCondition : ICoroutineWaitCondition
{
	private ICoroutineNode coroutine;

	bool ICoroutineWaitCondition.keepWaiting => !coroutine.IsFinished;

	public WaitForCoroutineCondition(ICoroutineNode coroutine)
	{
		this.coroutine = coroutine;
	}

	void ICoroutineWaitCondition.Pause()
	{
		coroutine.Pause();
	}

	void ICoroutineWaitCondition.Resume()
	{
		coroutine.Resume();
	}
}
