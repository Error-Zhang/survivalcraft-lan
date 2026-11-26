using System;
using System.Collections;
using System.Collections.Generic;

namespace SCIENEW;

public class BiDictionary<TFirst, TSecond> : IEnumerable<KeyValuePair<TFirst, TSecond>>, IEnumerable
{
	private IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();

	private IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

	public int Count => firstToSecond.Count;

	public TSecond this[TFirst first]
	{
		get
		{
			if (!firstToSecond.TryGetValue(first, out var value))
			{
				throw new ArgumentException("first");
			}
			return value;
		}
		set
		{
			if (firstToSecond.TryGetValue(first, out var value2))
			{
				secondToFirst.Remove(value2);
			}
			if (secondToFirst.TryGetValue(value, out var value3))
			{
				firstToSecond.Remove(value3);
			}
			firstToSecond[first] = value;
			secondToFirst[value] = first;
		}
	}

	public TFirst this[TSecond second]
	{
		get
		{
			if (!secondToFirst.TryGetValue(second, out var value))
			{
				throw new ArgumentException("second");
			}
			return value;
		}
		set
		{
			if (secondToFirst.TryGetValue(second, out var value2))
			{
				firstToSecond.Remove(value2);
			}
			if (firstToSecond.TryGetValue(value, out var value3))
			{
				secondToFirst.Remove(value3);
			}
			secondToFirst[second] = value;
			firstToSecond[value] = second;
		}
	}

	public void Add(TFirst first, TSecond second)
	{
		if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
		{
			throw new ArgumentException("Duplicate first or second");
		}
		firstToSecond.Add(first, second);
		secondToFirst.Add(second, first);
	}

	public TSecond GetByFirst(TFirst first)
	{
		if (!firstToSecond.TryGetValue(first, out var value))
		{
			throw new ArgumentException("first");
		}
		return value;
	}

	public TFirst GetBySecond(TSecond second)
	{
		if (!secondToFirst.TryGetValue(second, out var value))
		{
			throw new ArgumentException("second");
		}
		return value;
	}

	public void RemoveByFirst(TFirst first)
	{
		if (!firstToSecond.TryGetValue(first, out var value))
		{
			throw new ArgumentException("first");
		}
		firstToSecond.Remove(first);
		secondToFirst.Remove(value);
	}

	public void RemoveBySecond(TSecond second)
	{
		if (!secondToFirst.TryGetValue(second, out var value))
		{
			throw new ArgumentException("second");
		}
		secondToFirst.Remove(second);
		firstToSecond.Remove(value);
	}

	public bool TryAdd(TFirst first, TSecond second)
	{
		if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
		{
			return false;
		}
		firstToSecond.Add(first, second);
		secondToFirst.Add(second, first);
		return true;
	}

	public bool TryGetByFirst(TFirst first, out TSecond second)
	{
		return firstToSecond.TryGetValue(first, out second);
	}

	public bool TryGetBySecond(TSecond second, out TFirst first)
	{
		return secondToFirst.TryGetValue(second, out first);
	}

	public bool TryRemoveByFirst(TFirst first)
	{
		if (!firstToSecond.TryGetValue(first, out var value))
		{
			return false;
		}
		firstToSecond.Remove(first);
		secondToFirst.Remove(value);
		return true;
	}

	public bool TryRemoveBySecond(TSecond second)
	{
		if (!secondToFirst.TryGetValue(second, out var value))
		{
			return false;
		}
		secondToFirst.Remove(second);
		firstToSecond.Remove(value);
		return true;
	}

	public void Clear()
	{
		firstToSecond.Clear();
		secondToFirst.Clear();
	}

	public bool ContainsKey(TFirst first)
	{
		return firstToSecond.ContainsKey(first);
	}

	public bool ContainsValue(TSecond second)
	{
		return secondToFirst.ContainsKey(second);
	}

	public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator()
	{
		return firstToSecond.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
