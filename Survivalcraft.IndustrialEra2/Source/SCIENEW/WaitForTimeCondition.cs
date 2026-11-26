using Engine;

namespace SCIENEW;

public struct WaitForTimeCondition : ICoroutineWaitCondition
{
	private float waitTime;

	bool ICoroutineWaitCondition.keepWaiting
	{
		get
		{
			waitTime -= Time.FrameDuration;
			return waitTime > 0f;
		}
	}

	public WaitForTimeCondition(float time)
	{
		waitTime = time;
	}

	void ICoroutineWaitCondition.Pause()
	{
	}

	void ICoroutineWaitCondition.Resume()
	{
	}
}
