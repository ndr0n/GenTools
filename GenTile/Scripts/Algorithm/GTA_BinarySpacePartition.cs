using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public class GTA_BinarySpacePartition : GenTileAlgo
    {
        public Vector2Int Width = new Vector2Int(8, 12);
        public Vector2Int Height = new Vector2Int(8, 12);
        public Vector2Int Chance = new Vector2Int(100, 100);
        public Vector2Int Offset = new Vector2Int(0, 0);

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            List<Vector2Int> placed = new();
            System.Random random = new(seed);
            List<Vector2Int> positions = available.OrderBy(x => random.Next()).ToList();
            if (positions.Count == 0) return placed;
            placed = GenTileLibrary.BinarySpacePartition(random, positions, Width, Height, Chance, Offset);
            return placed;
        }
    }
}