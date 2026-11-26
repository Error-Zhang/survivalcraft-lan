using System;

namespace SCIENEW;

public struct WaitForFrameCondition : ICoroutineWaitCondition
{
	private int waitFrame;

	bool ICoroutineWaitCondition.keepWaiting
	{
		get
		{
			waitFrame--;
			return waitFrame > 0;
		}
	}

	public WaitForFrameCondition(int frame)
	{
		if (frame <= 0)
		{
			throw new ArgumentException("Frame must be greater than 0.", "frame");
		}
		waitFrame = frame;
	}

	void ICoroutineWaitCondition.Pause()
	{
	}

	void ICoroutineWaitCondition.Resume()
	{
	}
}
