namespace SCIENEW;

public interface IScoopableFluid
{
	bool IsScoopable(int container, out int bucketIndex);

	BiDictionary<int, int> AcquireContainer(int value);
}
