using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public class GTA_WaveFunctionCollapse : GenTileAlgo
    {
        public Texture2D InputTexture;
        public bool Invert;
        public Vector2Int N;
        public Vector2Int Symmetry;
        public Vector2Int Iterations;

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            List<Vector2Int> placed = new();
            System.Random random = new(seed);
            if (available.Count == 0) return placed;
            List<Vector2Int> positions = available.ToList();
            Vector2Int min = positions[0];
            Vector2Int max = positions[0];
            foreach (var val in positions)
            {
                if (val.x < min.x) min.x = val.x;
                if (val.y < min.y) min.y = val.y;
                if (val.x > max.x) max.x = val.x;
                if (val.y > max.y) max.y = val.y;
            }

            int width = max.x + 1;
            int height = max.y + 1;

            byte[,] map = new byte[width, height];
            map = WFC_Overlapping(map, 1, seed, Invert, InputTexture, N, Symmetry, Iterations);

            foreach (var pos in positions)
            {
                if (map[pos.x, pos.y] == 1) placed.Add(pos);
            }

            return placed;
        }

        public static byte[,] WFC_Overlapping(byte[,] map, byte value, int seed, bool invert, Texture2D inputTexture, Vector2Int n, Vector2Int symmetry, Vector2Int iterations)
        {
            System.Random random = new(seed);
            // Texture2D _inputTexture = inputTexture[random.Next(inputTexture.Count)];
            int _n = random.Next(n.x, n.y);
            int _symmetry = random.Next(symmetry.x, symmetry.y);
            int _iterations = random.Next(iterations.x, iterations.y);

            byte[,] sample = new byte[inputTexture.width, inputTexture.height];
            for (int x = 0; x < sample.GetLength(0); x++)
            {
                for (int y = 0; y < sample.GetLength(1); y++)
                {
                    Color32 color = inputTexture.GetPixel(x, y);
                    byte v = 0;
                    if (color.r > 50 || color.g > 50 || color.b > 50) v = 1;
                    sample[x, y] = v;
                }
            }

            int _seed = random.Next(int.MinValue, int.MaxValue);

            for (int i = 0; i < 32; i++)
            {
                if (TryRunWfcOverlappingModel(map, value, _seed, invert, sample, _n, _symmetry, _iterations))
                {
                    return map;
                }
                else _seed = random.Next(int.MinValue, int.MaxValue);
            }
            return map;
        }

        static bool TryRunWfcOverlappingModel(byte[,] map, byte value, int seed, bool invert, byte[,] sample, int n, int symmetry, int iterations)
        {
            int width = map.GetLength(0) + 2;
            int height = map.GetLength(1) + 2;
            OverlappingModel model = new(sample, n, width, height, false, false, symmetry, 0);
            if (model.Run(seed, iterations))
            {
                byte[,] m = new byte[width, height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte v = model.Sample(x, y);
                        if (v != 99) m[x, y] = v;
                        else m[x, y] = 0;
                    }
                }
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        if (m[x, y] == 0)
                        {
                            if (invert == true) map[x, y] = value;
                        }
                        else
                        {
                            if (invert == false) map[x, y] = value;
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}