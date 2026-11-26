namespace SCIENEW;

public interface IChargeableDItem
{
	int Charge(int value, int count);

	int Discharge(int value, int count);

	bool IsFull(int value);

	bool IsEmpty(int value);

	int GetDurability(int value);

	int GetDamage(int value);

	int SetDamage(int value, int damage);
}
