using System;
using System.Collections;
using System.Collections.Generic;

namespace SCIENEW;

public struct FilteredItemsEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable where T : class
{
	private int m_index = 0;

	private T m_current = null;

	public T Current => m_current;

	object IEnumerator.Current => m_current;

	public FilteredItemsEnumerator()
	{
	}

	public void Dispose()
	{
	}

	public bool MoveNext()
	{
		while (m_index < ItemsBlockManager.Items.Length)
		{
			if (ItemsBlockManager.Items[m_index++] is T current)
			{
				m_current = current;
				return true;
			}
		}
		m_current = null;
		return false;
	}

	public void Reset()
	{
		m_index = 0;
		m_current = null;
	}
}
