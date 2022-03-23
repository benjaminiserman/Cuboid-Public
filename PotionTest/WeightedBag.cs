#pragma warning disable
public class WeightedBag<T>
{
	Dictionary<T, int> weightTable;
	int sum;

	public WeightedBag(int capacity)
	{
		weightTable = new Dictionary<T, int>(capacity);
		sum = 0;
	}

	public WeightedBag(Dictionary<T, int> weightTable)
	{
		this.weightTable = weightTable;
		sum = 0;
		foreach (KeyValuePair<T, int> kvp in weightTable)
		{
			sum += kvp.Value;
		}
	}

	public void Add(T item, int weight)
	{
		weightTable.Add(item, weight);
		sum += weight;
	}

	public void Add(Dictionary<T, int> weightTable)
	{
		foreach (KeyValuePair<T, int> kvp in weightTable)
		{
			Add(kvp.Key, kvp.Value);
		}
	}

	public void Add(T[] array, Func<int, int> weightFunction)
	{
		for (int i = 0; i < array.Length; i++)
		{
			Add(array[i], weightFunction(i));
		}
	}

	public void Add(List<T> list, Func<int, int> weightFunction)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Add(list[i], weightFunction(i));
		}
	}

	public bool Remove(T item)
	{
		if (weightTable.ContainsKey(item))
		{
			sum -= weightTable[item];
			weightTable.Remove(item);

			return true;
		}

		return false;
	}

	public void Clear()
	{
		weightTable.Clear();
		sum = 0;
	}

	public T Get(Random random) // O(n)
	{
		int r = random.Next(sum);
		int total = 0;

		foreach (KeyValuePair<T, int> kvp in weightTable)
		{
			total += kvp.Value;
			if (r < total) return kvp.Key;
		}

		throw new Exception($"R value outside of range. Is the bag empty? r: {r}, total: {total}");
	}

	public T Get(int random)
	{
		int r = random % sum;
		int total = 0;

		foreach (KeyValuePair<T, int> kvp in weightTable)
		{
			total += kvp.Value;
			if (r < total) return kvp.Key;
		}

		throw new Exception($"R value outside of range. r: {r}, total: {total}");
	}

	public string Dump()
	{
		string s = string.Empty;

		foreach (var item in weightTable)
		{
			s += $"{item.Key}: {item.Value}, ";
		}

		return s;
	}
}