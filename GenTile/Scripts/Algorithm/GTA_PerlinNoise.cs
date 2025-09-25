using System.Collections.Generic;
using UnityEngine;

namespace GenTools
{
    [System.Serializable]
    public class GTA_PerlinNoise : GenTileAlgo
    {
        public Vector2 Modifier = new Vector2(0, 0.25f);

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            System.Random random = new(seed);
            float _modifier = random.Next(Mathf.RoundToInt(Modifier.x * int.MaxValue), Mathf.RoundToInt(Modifier.y * int.MaxValue)) / (float) int.MaxValue;
            List<Vector2Int> placed = new();
            foreach (var pos in available)
            {
                int newPoint = Mathf.RoundToInt(Mathf.PerlinNoise(pos.x * _modifier, pos.y * _modifier));
                if (newPoint > 0) placed.Add(pos);
            }
            return placed;
        }
    }
}