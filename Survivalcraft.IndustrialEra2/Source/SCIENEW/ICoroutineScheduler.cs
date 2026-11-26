using System.Collections;

namespace SCIENEW;

public interface ICoroutineScheduler
{
	int CoroutineCount { get; }

	ICoroutineNode AddCoroutine(IEnumerator fiber);

	void PauseCoroutine(ICoroutineNode coroutine);

	void ResumeCoroutine(ICoroutineNode coroutine);

	ICoroutineNode RemoveCoroutine(ICoroutineNode coroutine);

	void RemoveAllCoroutines();

	void UpdateCoroutines();
}
