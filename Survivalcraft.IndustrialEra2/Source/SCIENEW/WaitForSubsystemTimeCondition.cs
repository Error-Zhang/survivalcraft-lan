using System;
using Game;

namespace SCIENEW;

public struct WaitForSubsystemTimeCondition : ICoroutineWaitCondition
{
	private readonly SubsystemTime subsystemTime;

	private float waitTime;

	bool ICoroutineWaitCondition.keepWaiting
	{
		get
		{
			waitTime -= Math.Clamp(subsystemTime.GameTimeDelta, 0f, 0.1f);
			return waitTime > 0f;
		}
	}

	public WaitForSubsystemTimeCondition(float time, SubsystemTime subsystemTime)
	{
		waitTime = time;
		this.subsystemTime = subsystemTime;
	}

	void ICoroutineWaitCondition.Pause()
	{
	}

	void ICoroutineWaitCondition.Resume()
	{
	}
}
