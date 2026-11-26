using System.Collections;

namespace SCIENEW;

public interface ICoroutineNode
{
	bool IsFinished { get; set; }

	bool IsPaused { get; set; }

	IEnumerator Fiber { get; }

	ICoroutineWaitCondition WaitCondition { get; }

	bool NeedWait();

	void AddWaitCondition(ICoroutineWaitCondition condition);

	void Pause();

	void Resume();
}
