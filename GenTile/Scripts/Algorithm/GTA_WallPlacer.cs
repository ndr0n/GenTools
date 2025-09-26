using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public class GTA_WallPlacer : GenTileAlgo
    {
        public Vector2Int Percentage = new Vector2Int(100, 100);

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            List<Vector2Int> placed = new();
            System.Random random = new(seed);
            if (available.Count == 0) return placed;
            int percentage = random.Next(Percentage.x, Percentage.y);

            List<Vector2Int> positions = available.OrderBy(x => random.Next()).ToList();
            Vector2Int min = positions[0];
            Vector2Int max = positions[0];
            foreach (var val in positions)
            {
                if (val.x < min.x) min.x = val.x;
                if (val.y < min.y) min.y = val.y;
                if (val.x > max.x) max.x = val.x;
                if (val.y > max.y) max.y = val.y;
            }

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (positions.Contains(pos))
                    {
                        if (random.Next(0, 100) < percentage)
                        {
                            placed.Add(pos);
                            positions.Remove(pos);
                            break;
                        }
                    }
                }

                for (int y = max.y; y >= min.y; y--)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (positions.Contains(pos))
                    {
                        if (random.Next(0, 100) < percentage)
                        {
                            placed.Add(pos);
                            positions.Remove(pos);
                            break;
                        }
                    }
                }
            }

            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (positions.Contains(pos))
                    {
                        if (random.Next(0, 100) < percentage)
                        {
                            placed.Add(pos);
                            positions.Remove(pos);
                            break;
                        }
                    }
                }

                for (int x = max.x; x >= min.x; x--)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (positions.Contains(pos))
                    {
                        if (random.Next(0, 100) < percentage)
                        {
                            placed.Add(pos);
                            positions.Remove(pos);
                            break;
                        }
                    }
                }
            }

            return placed;
        }
    }
}