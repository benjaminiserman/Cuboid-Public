namespace HistoryTest;

using System;
using System.Drawing;

internal class Region
{
	public string Name { get; set; }
	


	public Biome Biome { get; set; }
	public double Temperature { get; set; }
	public double Precipitation { get; set; }
	public Color Color { get; set; }

	public void RandomizeBiomeData(bool isOcean)
	{
		if (isOcean)
		{
			Biome = Biome.Ocean;
		}
		else
		{
			var biomes = Enum.GetValues<Biome>();
			Biome = biomes[Random.Shared.Next(1, biomes.Length)];
		}

		Temperature = Biome switch
		{
			Biome.Ocean => 20,
			Biome.Jungle => 30,
			Biome.Plains => 20,
			Biome.Desert => 30,
			Biome.Forest => 15,
			Biome.Taiga => 10,
			Biome.Tundra => 0,
			Biome.Swamp => 20,
			_ => throw new Exception($"Case {Biome} not handled.")
		};

		Precipitation = Biome switch
		{
			Biome.Ocean => 200,
			Biome.Jungle => 350,
			Biome.Plains => 100,
			Biome.Desert => 10,
			Biome.Forest => 150,
			Biome.Taiga => 100,
			Biome.Tundra => 10,
			Biome.Swamp => 300,
			_ => throw new Exception($"Case {Biome} not handled.")
		};
	}
}
