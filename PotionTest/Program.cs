namespace PotionTest;
using InputHandler;

public static class Program
{
	private static readonly List<Ingredient> _ingredients = new();
	private static readonly List<Potion> _potions = new();
	private static readonly List<Ingredient> _cauldron = new();
	private static int _cauldronWater = 1;

	public static void Main()
	{
		while (true)
		{
			Console.WriteLine("Enter Instruction:");

			string[] split = Console.ReadLine().Split();

			if (split[0].ToLower() == "clear")
			{
				Console.Clear();
			}
			else if (split[0].ToLower() == "a")
			{
				int num = int.Parse(split[1]);
				foreach (var _ in Enumerable.Range(0, num))
				{
					Console.Write($"[i{_ingredients.Count}]: ");
					var x = Ingredient.Generate();
					x.PrettyPrint();
					_ingredients.Add(x);
				}
			}
			else if (split[0].ToLower() == "g")
			{
				PrintOptions();
			}
			else if (split[0].ToLower() == "p")
			{
				int count = 0;
				for (int i = 0; i < _ingredients.Count; i++)
				{
					foreach (var x in _ingredients[i].GetSymptoms().Where(x => x.Polar))
					{
						for (int j = i + 1; j < _ingredients.Count; j++)
						{
							foreach (var y in _ingredients[j].GetSymptoms().Where(x => x.Polar))
							{
								if (x.PotionEffect == y.PotionEffect)
								{
									Console.WriteLine($"{x.PotionEffect}: [i{i}] & [i{j}]");
									count++;
									goto IngredientDone;
								}
							}
						}

IngredientDone:;
					}
				}

				Console.WriteLine($"{count} matching pairs found.");
			}
			else if (split[0].ToLower() == "c")
			{
				foreach (string s in split[1..])
				{
					if (split[1].ToLower() == "f")
					{
						Finish();
					}
					else if (split[1].ToLower() == "g")
					{
						PrintCauldron();
					}
					else if (s == "f")
					{
						Finish();
					}
					else
					{
						int x = s.IndexOf('x');
						if (x == -1) x = s.Length;

						if (s[..x].ToLower() == "w")
						{
							int amount = 1;
							if (x != s.Length) amount = int.Parse(s[(x + 1)..]);

							_cauldronWater = amount;
						}
						else
						{
							Ingredient ingredient = GetFromString(s[..x]);

							double amount = 1;
							if (x != s.Length) amount = double.Parse(s[(x + 1)..]);

							Ingredient match = _cauldron.FirstOrDefault(x => x.Name == ingredient.Name);
							if (match is not null)
							{
								match.Amount += ingredient.Amount * amount;
							}
							else
							{
								ingredient = new(ingredient);
								ingredient.Amount *= amount;

								_cauldron.Add(ingredient);
							}
						}
					}
				}
			}
			else if (split[0].ToLower() == "d")
			{
				Ingredient ingredient = GetFromString(split[1]);

				if (ingredient is Potion p)
				{
					var distilled = p.Distill();
					if (!distilled.Any()) Console.WriteLine("Distillation failed.");
					else
					{
						foreach (var x in distilled)
						{
							_potions.Add(x);
							Console.WriteLine("Created new potion: ");
							_potions[^1].PrettyPrint();
						}
					}
				}
				else
				{
					Console.WriteLine("Cannot distill raw ingredients.");
				}
			}
			else if (split[0].ToLower() == "l")
			{
				Ingredient ingredient = GetFromString(split[1]);

				if (ingredient is Potion p)
				{
					_potions.Add(p.Calcinate());
					Console.WriteLine("Created new potion: ");
					_potions[^1].PrettyPrint();
				}
				else
				{
					Console.WriteLine("Cannot calcinate raw ingredients.");
				}
			}
		}
	}

	private static void Finish()
	{
		_potions.Add(Potion.Combine(_cauldron).Divide(_cauldronWater).First());
		_cauldron.Clear();
		_cauldronWater = 1;

		Console.WriteLine("Created new potion: ");
		_potions[^1].PrettyPrint();
	}

	private static Ingredient GetFromString(string s) => s[0] switch
	{
		'i' => _ingredients[int.Parse(s[1..])],
		'p' => _potions[int.Parse(s[1..])],
		_ => _ingredients[int.Parse(s)]
	};

	private static void PrintCauldron()
	{
		foreach (var x in _cauldron)
		{
			Console.WriteLine($" - {x.Name} x {x.Amount}");
		}
	}

	private static void PrintOptions()
	{
		Console.WriteLine("Ingredients:");
		foreach (var (x, i) in _ingredients.Select((x, i) => (x, i)))
		{
			Console.Write($"[i{i}]: ");
			x.PrettyPrint();
		}

		Console.WriteLine("Potions:");
		foreach (var (x, i) in _potions.Select((x, i) => (x, i)))
		{
			Console.Write($"[p{i}]: ");
			x.PrettyPrint();
		}
	}

	public static bool NameExists(string s) => _ingredients.Any(x => x.Name == s);
	public static int PotionCount => _potions.Count;
}