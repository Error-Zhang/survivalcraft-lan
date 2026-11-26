using System;

namespace SCIENEW.Descriptors;

[AttributeUsage(AttributeTargets.Class)]
public class DescriptorAttribute : Attribute
{
	public Type TargetType;

	public DescriptorAttribute(Type targetType)
	{
		TargetType = targetType;
	}
}
