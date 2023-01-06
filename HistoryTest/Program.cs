namespace HistoryTest;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

internal class Program
{
	static void Main(string[] args)
	{
		var sqrtSize = 500;
		var sqrtNodes = 10;

		var map = new Voronoi<Region>(sqrtSize, sqrtNodes, true);
		map.PopulateRandom(Random.Shared, 0.05f);
		map.PopulateNeighbors();

		// set colors
		foreach (Voronoi<Region>.VoronoiPoint point in map)
		{
			var oceanFactor = 0.2 + (Random.Shared.NextDouble() * 0.3 - 0.1);

			point.value.RandomizeBiomeData(point.pos.x <= sqrtSize * oceanFactor
				|| point.pos.y <= sqrtSize * oceanFactor
				|| point.pos.x >= sqrtSize - sqrtSize * oceanFactor
				|| point.pos.y >= sqrtSize - sqrtSize * oceanFactor);

			if (point.value.Biome == Biome.Ocean)
			{
				point.value.Color = Color.CornflowerBlue;
			}
			else
			{
				point.value.Color = Color.FromArgb(
					Random.Shared.Next(100),
					Random.Shared.Next(100, 200),
					Random.Shared.Next(100));
			}
		}

		DrawMap(map, x => x.Color);
	}

	private static void AllocateNames(Voronoi<Region> voronoi)
	{
		var allocatedNames = new HashSet<string>();
		var text = File.ReadAllLines("city_names.txt");

		foreach (Voronoi<Region>.VoronoiPoint point in voronoi)
		{
			var name = string.Empty;
			do
			{
				name = $"{text[Random.Shared.Next(text.Length)]} {point.value.Biome}";
			}
			while (allocatedNames.Contains(name));

			allocatedNames.Add(name);
			point.value.Name = name;
		}
	}

	private static void DrawMap<T>(Voronoi<T> map, Func<T, Color> func)
	{
		var testImage = new Bitmap(map.sqrtSize, map.sqrtSize);
		for (var x = 0; x < map.sqrtSize; x++)
		{
			for (var y = 0; y < map.sqrtSize; y++)
			{
				testImage.SetPixel(x, y, func(map[new(x, y)]));
			}
		}

		foreach (Voronoi<Color>.VoronoiPoint point in map)
		{
			foreach (var neighbor in point.neighbors)
			{
				if (Math.Abs(neighbor.pos.x - point.pos.x) < Math.Abs(neighbor.pos.y - point.pos.y))
				{
					var slope = (double)(neighbor.pos.x - point.pos.x) / (neighbor.pos.y - point.pos.y);
					var xOffset = neighbor.pos.x - slope * neighbor.pos.y;

					for (var y = Math.Min(point.pos.y, neighbor.pos.y); y <= Math.Max(point.pos.y, neighbor.pos.y); y++)
					{
						testImage.SetPixel((int)(y * slope + xOffset), y, Color.Red);
					}
				}
				else
				{
					var slope = (double)(neighbor.pos.y - point.pos.y) / (neighbor.pos.x - point.pos.x);
					var yOffset = neighbor.pos.y - slope * neighbor.pos.x;

					for (var x = Math.Min(point.pos.x, neighbor.pos.x); x <= Math.Max(point.pos.x, neighbor.pos.x); x++)
					{
						testImage.SetPixel(x, (int)(x * slope + yOffset), Color.Red);
					}
				}
			}
		}

		foreach (Voronoi<Color>.VoronoiPoint point in map)
		{
			for (var i = -2; i <= 2; i++)
			{
				for (var j = -2; j <= 2; j++)
				{
					try
					{
						testImage.SetPixel(point.pos.x + i, point.pos.y + j, Color.Black);
					}
					catch { }
				}
			}
		}

		testImage.Save("test.png", ImageFormat.Png);
		Console.WriteLine("image saved.");
		Console.ReadLine();
	}
}
