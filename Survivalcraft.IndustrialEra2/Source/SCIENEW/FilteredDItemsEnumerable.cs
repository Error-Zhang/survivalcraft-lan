using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SCIENEW;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FilteredDItemsEnumerable<T> : IEnumerable<T>, IEnumerable where T : class
{
	public FilteredDItemsEnumerable()
	{
	}

	public FilteredDItemsEnumerator<T> GetEnumerator()
	{
		return new FilteredDItemsEnumerator<T>();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new FilteredDItemsEnumerator<T>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new FilteredDItemsEnumerator<T>();
	}
}
