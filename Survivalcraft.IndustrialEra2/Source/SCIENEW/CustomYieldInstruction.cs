using System.Collections;

namespace SCIENEW;

public abstract class CustomYieldInstruction : IEnumerator
{
	public abstract bool keepWaiting { get; }

	public object Current => null;

	public bool MoveNext()
	{
		return keepWaiting;
	}

	public virtual void Reset()
	{
	}
}
