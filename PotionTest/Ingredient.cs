namespace PotionTest;

internal class Ingredient
{
	protected List<Symptom> _symptoms = new();

	public string Name { get; protected set; }

	public double Amount = 1;

	public override string ToString() => $"{Name}: {string.Join(", ", _symptoms)}";

	public Ingredient(Ingredient x)
	{
		Name = x.Name;
		Amount = x.Amount;
		_symptoms = new(x._symptoms);
	}

	public Ingredient() { }

	public void PrettyPrint()
	{
		Console.WriteLine($"{Name} (Toxicity: {(int)(_symptoms.Sum(x => x.Toxicity) / Amount * 100)})");
		Console.WriteLine($"Symptoms:");
		foreach (dynamic x in EnumerateSymptomPairs())
		{
			Console.Write(" - ");
			switch (x)
			{
				case Symptom s:
				{
					s.PrettyPrint();
					break;
				}
				case (Symptom a, Symptom b):
				{
					Console.Write("(");
					a.PrettyPrint();
					Console.Write(", ");
					b.PrettyPrint();
					Console.Write(")");
					break;
				}
			}

			Console.WriteLine();
		}
	}

	public IEnumerable<dynamic> EnumerateSymptomPairs()
	{
		var positive = _symptoms.Where(x => x.Polar && x.Potency > 0).OrderBy(x => -x.Potency).ToArray();
		var negative = _symptoms.Where(x => x.Polar && x.Potency < 0).OrderBy(x => +x.Potency).ToArray();
		var neutral = _symptoms.Where(x => x.Neutral).ToArray();

		int posCount = positive.Length;
		int negCount = negative.Length;

		int pairs = Math.Min(posCount, negCount);
		int polar = Math.Max(posCount, negCount);

		for (int i = 0; i < polar; i++)
		{
			if (i < pairs) yield return (positive[i], negative[i]);
			else if (i < posCount) yield return positive[i];
			else if (i < negCount) yield return negative[i];
			else throw new Exception("weird");
		}

		foreach (var n in neutral) yield return n;
	}

	public IEnumerable<Symptom> GetSymptoms() => _symptoms.Select(x => x);

	public static Ingredient Generate()
	{
		Ingredient ingredient = new();

		do
		{
			ingredient.Name = GenerateName();
		}
		while (Program.NameExists(ingredient.Name));
		
		ingredient._symptoms = GenerateSymptoms();

		return ingredient;
	}

	private static string GenerateName()
	{
		string[] starts =
		{
			"Horn", "Spike", "Spot", "Dot", "Stripe", "Leopard", "Tiger", "Red", "Orange", "Yellow", "Flash", "Green", "Cold", "Teal", "Blue", "Azure", "Scarlet", "Indigo", "Violet", "Forest", "Volcano", "Cave", "Plains", "Desert", "Canyon", "Ravine", "Rock", "Stone", "Grey", "Black", "White"
		};

		string[] ends =
		{
			"Flower", "Mushroom", "Shroom", "Toadstool", "Lily", "Bone", "Guts", "Feather", "Leaf", "Fruit", "Apple", "Mold", "Moss", "Fungus", "Skull", "Heart", "Wool", "Dust", "Extract", "Syrup", "Diamond", "Gem", "Agate", "Eye", "Powder"
		};

		return $"{starts[Random.Shared.Next(starts.Length)]} {ends[Random.Shared.Next(ends.Length)]}";
	}

	private static List<Symptom> GenerateSymptoms()
	{
		List<Symptom> symptoms = new();
		(int pos, int neg, int neu) = _distributions.Get(Random.Shared.Next());

		foreach (var _ in Enumerable.Range(0, pos))
		{
			while (true)
			{
				var x = Symptom.RandomPositive();

				if (!symptoms.Any(y => y.PotionEffect == x.PotionEffect))
				{
					symptoms.Add(x);
					break;
				}
			}
		}

		foreach (var _ in Enumerable.Range(0, neg))
		{
			while (true)
			{
				var x = Symptom.RandomNegative();

				if (!symptoms.Any(y => y.PotionEffect == x.PotionEffect))
				{
					symptoms.Add(x);
					break;
				}
			}
		}

		foreach (var _ in Enumerable.Range(0, neu))
		{
			while (true)
			{
				var x = Symptom.RandomNeutral();

				if (!symptoms.Any(y => y.PotionEffect == x.PotionEffect))
				{
					symptoms.Add(x);
					break;
				}
			}
		}

		return symptoms;
	}

	private static readonly WeightedBag<(int, int, int)> _distributions = new(new Dictionary<(int, int, int), int>()
	{
		/*[(1, 1, 1)] = 50,
		[(2, 2, 1)] = 150,
		[(3, 3, 1)] = 100,
		[(1, 0, 1)] = 5,
		[(0, 1, 1)] = 5,
		[(2, 1, 1)] = 10,
		[(1, 2, 1)] = 10,
		[(3, 2, 1)] = 5,
		[(2, 3, 1)] = 5,*/

		[(1, 1, 1)] = 30,
		[(2, 2, 1)] = 10,
		[(3, 3, 1)] = 1,
		[(1, 0, 1)] = 2,
		[(0, 1, 1)] = 2,
		[(2, 1, 1)] = 2,
		[(1, 2, 1)] = 2,
		[(3, 2, 1)] = 1,
		[(2, 3, 1)] = 1,

		/*[(1, 0, 1)] = 1,
		[(0, 1, 1)] = 1,
		[(1, 1, 1)] = 1,*/
	});

}
