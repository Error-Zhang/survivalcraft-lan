namespace SCIENEW;

public interface ICondenserComponent
{
	float OutputPower { get; }

	float OutputVoltage { get; }

	bool HasEnergy { get; }

	void InputEnergy(float energy);

	float OutputEnergy(float energy);
}
