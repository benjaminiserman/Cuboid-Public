namespace PotionTest;

internal class Potion : Ingredient
{
	public Potion() { }
	public Potion(Potion p) : base(p) { }

	public static Potion Combine(IEnumerable<Ingredient> ingredients)
	{
		Potion p = new();
		p.Amount = 0;

		p._symptoms = new();

		foreach (var ingredient in ingredients)
		{
			foreach (var s in ingredient.GetSymptoms())
			{
				Symptom? match = p._symptoms.Select(x => (Symptom?)x).FirstOrDefault(x => x.Value.PotionEffect == s.PotionEffect);

				if (match is null || s.Neutral)
				{
					p._symptoms.Add(s);
				}
				else
				{
					p._symptoms.Remove(match.Value);
					p._symptoms.Add(Symptom.Combine(match.Value, s, p.Amount, ingredient.Amount));
				}
			}

			if (p._symptoms.Count(x => x.Neutral) > 1)
			{
				foreach (var s in p._symptoms.Where(x => x.Neutral).OrderBy(x => -x.Potency).Skip(1))
				{
					p._symptoms.Remove(s);
				}
			}

			p.Amount += ingredient.Amount;
		}

		p.Name = p.GetName();

		return p;
	}

	public IEnumerable<Potion> Distill()
	{
		Potion p = new(this);
		Potion o = new();
		o._symptoms.AddRange(p._symptoms.Where(x => x.Neutral));

		var enumeration = EnumerateSymptomPairs();

		if (enumeration.Count(x => x is not Symptom s || s.Polar) > 1)
		{
			switch (enumeration.Last(x => x is not Symptom s || s.Polar))
			{
				case Symptom s:
				{
					p._symptoms.Remove(s);
					p.Name = GetName();
					yield return p;
					o._symptoms.Add(s);
					o.Name = GetName();
					yield return o;
					break;
				}
				case (Symptom a, Symptom b):
				{
					p._symptoms.Remove(a);
					p._symptoms.Remove(b);
					p.Name = GetName();
					yield return p;
					o._symptoms.Add(a);
					o._symptoms.Add(b);
					o.Name = GetName();
					yield return o;
					break;
				}
			}
		}
		else yield break;
	}

	public Potion Calcinate()
	{
		Potion p = new(this);
		foreach (var x in _symptoms.Where(y => y.Neutral))
		{
			p._symptoms.Remove(x);
		}

		p.Name = GetName();

		return p;
	}

	private string GetName() => $"Potion {Program.PotionCount}";

	public IEnumerable<Potion> Divide(int x)
	{
		foreach (var _ in Enumerable.Range(0, x))
		{
			yield return new Potion()
			{
				Name = GetName(),
				Amount = Amount / x,
				_symptoms = new(_symptoms)
			};

			break; // $$$ remove
		}
	}
}
