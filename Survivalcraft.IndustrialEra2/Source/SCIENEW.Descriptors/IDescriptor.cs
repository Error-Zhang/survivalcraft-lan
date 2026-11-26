using Game;

namespace SCIENEW.Descriptors;

public interface IDescriptor
{
	string PropertyName { get; set; }

	Widget GetWidget();
}
