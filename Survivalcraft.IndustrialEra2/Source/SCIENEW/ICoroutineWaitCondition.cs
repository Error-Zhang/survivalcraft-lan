namespace SCIENEW;

public interface ICoroutineWaitCondition
{
	bool keepWaiting { get; }

	void Pause();

	void Resume();
}
