using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SCIENEW;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FilteredItemsEnumerable<T> : IEnumerable<T>, IEnumerable where T : class
{
	public FilteredItemsEnumerable()
	{
	}

	public FilteredItemsEnumerator<T> GetEnumerator()
	{
		return new FilteredItemsEnumerator<T>();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new FilteredItemsEnumerator<T>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new FilteredItemsEnumerator<T>();
	}
}
