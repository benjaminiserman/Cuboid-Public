namespace PotionTest;

internal struct Symptom
{
	public Effect PotionEffect { get; private set; }
	public double Potency { get; private set; }
	public double Toxicity { get; private set; }

	public bool Neutral => PotionEffect >= Effect.Awkward;
	public bool Polar => !Neutral;

	public Symptom(Effect PotionEffect, double Potency, double Toxicity)
	{
		this.PotionEffect = PotionEffect;
		this.Potency = Potency;
		this.Toxicity = Toxicity;
	}

	private string NeutralString => Neutral ? "N:" : string.Empty;

	public override string ToString() => $"{NeutralString}{PotionEffect} {(int)(Potency * 100)}/{(int)(Toxicity * 100)}";

	public void PrettyPrint()
	{
		Console.ForegroundColor = Neutral switch
		{
			true => ConsoleColor.Blue,
			false => Potency < 0
				? ConsoleColor.Red
				: ConsoleColor.Green,
		};

		Console.Write(ToString());

		Console.ForegroundColor = ConsoleColor.White;
	}

	public void Benedict()
	{
		if (Potency < 0) Potency *= -1;
	}

	public void Maledict()
	{
		if (Potency > 0) Potency *= -1;
	}

	public static Symptom Combine(Symptom a, Symptom b, double aAmount, double bAmount)
	{
		if (!CanCombine(a, b)) throw new ArgumentException($"Symptoms {a} and {b} are not combinable.");

		return new(a.PotionEffect, Combine(a.Potency, b.Potency, aAmount, bAmount), Combine(a.Toxicity, b.Toxicity, aAmount, bAmount));
	}

	private static double Combine(double a, double b, double x, double y) => (a * x + b * y) / (x + y);

	public static bool CanCombine(Symptom a, Symptom b)
	{
		if (a.PotionEffect != b.PotionEffect) return false;
		if (a.Neutral || b.Neutral) return false;

		return true;
	}

	public static double GetToxicity() => Random.Shared.NextDouble() * 0.05 + 0.01;

	public static Symptom RandomPositive() => new((Effect)Random.Shared.Next((int)Effect.Awkward), Random.Shared.NextDouble() * 0.9 + 0.1 , GetToxicity());

	public static Symptom RandomNegative() => new((Effect)Random.Shared.Next((int)Effect.Awkward), Random.Shared.NextDouble() * -0.9 - 0.1, GetToxicity());

	public static Symptom RandomNeutral() => new((Effect)Random.Shared.Next((int)Effect.Awkward, Enum.GetValues<Effect>().Length), Random.Shared.NextDouble(), GetToxicity());
}