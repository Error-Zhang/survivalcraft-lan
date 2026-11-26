namespace SCIENEW;

public interface IAirBlowerComponent
{
	AirBlowerPressureClass PressureClass { get; }

	bool IsWorking();
}
