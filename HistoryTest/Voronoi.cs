namespace HistoryTest;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public class Voronoi<T> : IEnumerable // represents a 2D voronoi map
{
	public int sqrtNodes, scale, sqrtSize;
	public VoronoiPoint[][] grid;
	private readonly float _recScale;
	private readonly bool _wrap;

	public VoronoiPoint this[int i, int j] // for getting VoronoiPoint from voronoi-scale coords
	{
		get
		{
			try
			{
				if (_wrap)
				{
					if (i < 0)
					{
						i = sqrtNodes - 1;
					}

					if (i >= sqrtNodes)
					{
						i = 0;
					}

					if (j < 0)
					{
						j = sqrtNodes - 1;
					}

					if (j >= sqrtNodes)
					{
						j = 0;
					}
				}

				return grid[i][j];
			}
			catch
			{
				Console.WriteLine($"wrap: {_wrap}, i: {i}, j: {j}, x: {grid?.Length}, y: {grid?[0]?.Length}, sqrtNodes: {sqrtNodes}");
				throw;
			}
		}
	}

	public T this[Vector2Int v] => FindClosest(v).value; // for getting T from global-scale coords

	public Voronoi(int sqrtSize, int sqrtNodes, bool wrap = false)
	{
		this.sqrtSize = sqrtSize;
		this.sqrtNodes = sqrtNodes;
		this._wrap = wrap;

		if (sqrtSize % sqrtNodes != 0)
		{
			throw new ArgumentException("sqrtSize must be divisible by sqrtNodes!");
		}

		scale = sqrtSize / sqrtNodes;
		_recScale = 1f / scale;

		grid = new VoronoiPoint[sqrtNodes][];
		for (var i = 0; i < sqrtNodes; i++)
		{
			grid[i] = new VoronoiPoint[sqrtNodes];
		}
	}

	public IEnumerator GetEnumerator()
	{
		for (var i = 0; i < sqrtNodes; i++)
		{
			for (var j = 0; j < sqrtNodes; j++)
			{
				yield return grid[i][j];
			}
		}
	}

	public void PopulateRandom(Random random, float centeringFactor)
	{
		for (var i = 0; i < sqrtNodes; i++)
		{
			for (var j = 0; j < sqrtNodes; j++)
			{
				grid[i][j] = new(RandomInCell(new Vector2Int(i, j), random, centeringFactor));
			}
		}
	}

	public void PopulateNeighbors() // $$$ note: this algorithm doesn't give the right answer but it's good enough for an EXPERIMENTAL BUILD
	{
		foreach (VoronoiPoint point in this)
		{
			foreach (VoronoiPoint otherPoint in this)
			{
				if (point == otherPoint
					|| point.neighbors.Contains(otherPoint))
				{
					continue;
				}

				var midpointX = (point.pos.x + otherPoint.pos.x) / 2;
				var midpointY = (point.pos.y + otherPoint.pos.y) / 2;

				var distances = new List<(VoronoiPoint point, double distance)>();
				foreach (VoronoiPoint distancePoint in this)
				{
					distances.Add((distancePoint, Math.Pow(distancePoint.pos.x - midpointX, 2) + Math.Pow(distancePoint.pos.y - midpointY, 2)));
				}

				var sorted = distances.OrderBy(t => t.distance)
					.Select(t => t.point)
					.ToList();

				//Console.WriteLine($"p {point.pos.x}, {point.pos.y} => {{ {string.Join(", ", sorted.Select(s => "(" + s.pos.x + ", " + s.pos.y + ")"))} }}");

				if ((sorted[0] == point
						|| sorted[0] == otherPoint)
					&& (sorted[1] == point
						|| sorted[1] == otherPoint))
				{
					point.neighbors.Add(otherPoint);
					otherPoint.neighbors.Add(point);
				}
			}
		}
	}

	private Vector2Int RandomInCell(Vector2Int v, Random random, float factor = 0f)
	{
		var pos = new Vector2Int(random.Next(v.x * scale, (v.x + 1) * scale), random.Next(v.y * scale, (v.y + 1) * scale));
		if (factor != 0f)
		{
			//if (v.x == 0 || v.y == 0 || v.x == sqrtNodes - 1 || v.y == sqrtNodes - 1) factor = 1.0f;
			v = new(v.x * scale, v.y * scale); // change to global scale
			v = new(v.x + scale / 2, v.y + scale / 2); // move to center of grid square
			v = new(v.x - pos.x, v.y - pos.y); // get difference vector
			pos = new(pos.x + (int)Math.Floor(v.x * factor), pos.y + (int)Math.Floor(v.y * factor)); //MathHelper.FloorToInt((Vector2)v * factor); // add difference vector * centering factor
		}

		return pos;
	}

	private Vector2Int GetGridPosition(Vector2Int pos) // $$$ optimize
	{
		return new((int)(pos.x * _recScale), (int)(pos.y * _recScale));
	}

	private VoronoiPoint FindClosest(Vector2Int pos)
	{
		VoronoiPoint closest = null;
		var closestDistance = float.MaxValue;
		var gridPos = GetGridPosition(pos);

		for (var i = -1; i <= 1; i++)
		{
			for (var j = -1; j <= 1; j++)
			{
				var testGridPos = new Vector2Int(gridPos.x + i, gridPos.y + j);

				var testDistance = GridDistance(pos, testGridPos.x, testGridPos.y);

				if (testDistance != -1f && testDistance < closestDistance)
				{
					closestDistance = testDistance;
					closest = this[testGridPos.x, testGridPos.y];
				}
			}
		}

		return closest;
	}

	private (float, float, float, float, float, float, float, float, float) VoronoiLerpFactors(Vector2Int pos) // make wrap work
	{
		Vector2Int gridPos = GetGridPosition(pos);
		//Vector2Int thisPointPos = this[gridPos.x, gridPos.y].pos;

		float sum = 0;

		float nn, nz, np, zn, zz, zp, pn, pz, pp;

		nn = GridDistance(pos, gridPos.x - 1, gridPos.y - 1);
		if (nn != -1)
		{
			nn = nn > scale ? 0 : scale - nn;
			sum += nn;
		}

		nz = GridDistance(pos, gridPos.x - 1, gridPos.y);
		if (nz != -1)
		{
			nz = nz > scale ? 0 : scale - nz;
			sum += nz;
		}

		np = GridDistance(pos, gridPos.x - 1, gridPos.y + 1);
		if (np != -1)
		{
			np = np > scale ? 0 : scale - np;
			sum += np;
		}

		zn = GridDistance(pos, gridPos.x, gridPos.y - 1);
		if (zn != -1)
		{
			zn = zn > scale ? 0 : scale - zn;
			sum += zn;
		}

		zz = GridDistance(pos, gridPos.x, gridPos.y);
		if (zz != -1)
		{
			zz = zz > scale ? 0 : scale - zz;
			sum += zz;
		}

		zp = GridDistance(pos, gridPos.x, gridPos.y + 1);
		if (zp != -1)
		{
			zp = zp > scale ? 0 : scale - zp;
			sum += zp;
		}

		pn = GridDistance(pos, gridPos.x + 1, gridPos.y - 1);
		if (pn != -1)
		{
			pn = pn > scale ? 0 : scale - pn;
			sum += pn;
		}

		pz = GridDistance(pos, gridPos.x + 1, gridPos.y);
		if (pz != -1)
		{
			pz = pz > scale ? 0 : scale - pz;
			sum += pz;
		}

		pp = GridDistance(pos, gridPos.x + 1, gridPos.y + 1);
		if (pp != -1)
		{
			pp = pp > scale ? 0 : scale - pp;
			sum += pp;
		}

		if (sum == 0f)
		{
			zz = 1;
		}
		else
		{
			var rec = 1f / sum;

			nn *= rec;
			nz *= rec;
			np *= rec;
			zn *= rec;
			zz *= rec;
			zp *= rec;
			pn *= rec;
			pz *= rec;
			pp *= rec;
		}

		return (nn, nz, np, zn, zz, zp, pn, pz, pp);
	}

	public float VoronoiLerp(Vector2Int pos, Func<T, float> function)
	{
		(float nn, float nz, float np, float zn, float zz, float zp, float pn, float pz, float pp) = VoronoiLerpFactors(pos);
		float sum = 0f, output = 0f;
		Vector2Int gridPos = GetGridPosition(pos);

		if (nn != -1f)
		{
			sum += nn;
			output += function(this[gridPos.x - 1, gridPos.y - 1].value) * nn;
		}

		if (nz != -1f)
		{
			sum += nz;
			output += function(this[gridPos.x - 1, gridPos.y].value) * nz;
		}

		if (np != -1f)
		{
			sum += np;
			output += function(this[gridPos.x - 1, gridPos.y + 1].value) * np;
		}

		if (zn != -1f)
		{
			sum += zn;
			output += function(this[gridPos.x, gridPos.y - 1].value) * zn;
		}

		if (zz != -1f)
		{
			sum += zz;
			output += function(this[gridPos.x, gridPos.y].value) * zz;
		}

		if (zp != -1f)
		{
			sum += zp;
			output += function(this[gridPos.x, gridPos.y + 1].value) * zp;
		}

		if (pn != -1f)
		{
			sum += pn;
			output += function(this[gridPos.x + 1, gridPos.y - 1].value) * pn;
		}

		if (pz != -1f)
		{
			sum += pz;
			output += function(this[gridPos.x + 1, gridPos.y].value) * pz;
		}

		if (pp != -1f)
		{
			sum += pp;
			output += function(this[gridPos.x + 1, gridPos.y + 1].value) * pp;
		}

		return output / sum;
	}

	private float GridDistance(Vector2Int pos, int gridX, int gridY)
	{
		try
		{
			var v = this[gridX, gridY].pos;
			if (_wrap)
			{
				var div2 = sqrtSize / 2;
				if (Math.Abs(v.x - pos.x) > div2)
				{
					if (v.x > pos.x)
					{
						v = new(v.x - sqrtSize, v.y);
					}
					else
					{
						v = new(v.x + sqrtSize, v.y);
					}
				}

				if (Math.Abs(v.y - pos.y) > div2)
				{
					if (v.y > pos.y)
					{
						v = new(v.x, v.y - sqrtSize);
					}
					else
					{
						v = new(v.x, v.y + sqrtSize);
					}
				}
			}

			return (float)Math.Sqrt(Math.Pow(pos.x - v.x, 2) + Math.Pow(pos.y - v.y, 2));
		}
		catch
		{
			if (_wrap)
			{
				Console.WriteLine("hey what");
			}

			return -1f;
		}
	}

	public class VoronoiPoint
	{
		public Vector2Int pos; // actual position of point, in global coords
		public T value;
		public HashSet<VoronoiPoint> neighbors = new();

		public VoronoiPoint(int x, int y, T value)
		{
			pos = new(x, y);
			this.value = value;
		}

		public VoronoiPoint(Vector2Int pos, T value)
		{
			this.pos = pos;
			this.value = value;
		}

		public VoronoiPoint(int x, int y)
		{
			pos = new (x, y);
		}

		public VoronoiPoint(Vector2Int pos)
		{
			this.pos = pos;
		}

		private VoronoiPoint() { }
	}

	public record Vector2Int(int x, int y);
	public record Vector2(float x, float y);
}