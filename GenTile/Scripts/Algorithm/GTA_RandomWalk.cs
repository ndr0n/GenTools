using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    [System.Serializable]
    public class GTA_RandomWalk : GenTileAlgo
    {
        public Vector2Int Percentage = Vector2Int.zero;

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            List<Vector2Int> placed = new();
            System.Random random = new(seed);
            int percentage = random.Next(Percentage.x, Percentage.y + 1);
            int count = Mathf.FloorToInt(available.Count * (percentage / 100f));
            if (count == 0) return placed;

            Vector2Int pos = available[random.Next(available.Count)];
            List<Vector2Int> positions = available.OrderBy(x => random.Next()).ToList();
            List<Vector2Int> directions = new() {Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right};

            for (int i = 0; i < count; i++)
            {
                bool foundPosition = false;
                foreach (var direction in directions.OrderBy(x => random.Next()))
                {
                    Vector2Int adjacent = pos + direction;
                    if (positions.Contains(adjacent))
                    {
                        placed.Add(adjacent);
                        positions.Remove(adjacent);
                        pos = adjacent;
                        foundPosition = true;
                        break;
                    }
                }
                if (foundPosition == false)
                {
                    Vector2Int adjacent = positions.OrderBy(x => Vector2Int.Distance(x, pos)).FirstOrDefault();
                    placed.Add(adjacent);
                    positions.Remove(adjacent);
                    pos = adjacent;
                }
            }
            return placed;
        }
    }
}