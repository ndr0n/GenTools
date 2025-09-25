using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    [System.Serializable]
    public class GTA_Fill : GenTileAlgo
    {
        public Vector2Int Count = Vector2Int.zero;
        public Vector2Int Percentage = Vector2Int.zero;

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            List<Vector2Int> placed = new();
            System.Random random = new(seed);
            int _percentage = random.Next(Percentage.x, Percentage.y + 1);
            int _count = random.Next(Count.x, Count.y + 1);
            _count += Mathf.FloorToInt(available.Count * (_percentage / 100f));
            if (_count <= 0) return placed;

            int c = 0;
            foreach (var pos in available.OrderBy(x => random.Next()))
            {
                if (c >= _count) return placed;
                placed.Add(pos);
                c++;
            }
            return placed;
        }
    }
}