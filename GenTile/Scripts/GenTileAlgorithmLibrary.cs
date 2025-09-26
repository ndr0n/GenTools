using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public enum GenTileAlgorithmType
    {
        Fill,
        RandomWalk,
        PerlinNoise,
        Tunnel,
        Corridors,
        RoomPlacer,
        BinarySpacePartition,
        WallPlacer,
        WaveFunctionCollapse,
    }

    public static class GenTileAlgorithmLibrary
    {
        #region WaveFunctionCollapse

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

            // if (TryRunWfcOverlappingModel(map, value, _seed, invert, sample, _n, _symmetry, _iterations))
            // {
            //     return map;
            // }
            // else if (_n > 2)
            // {
            //     if (TryRunWfcOverlappingModel(map, value, _seed, invert, sample, _n - 1, _symmetry, _iterations))
            //     {
            //         return map;
            //     }
            //     else if (_n > 3)
            //     {
            //         if (TryRunWfcOverlappingModel(map, value, _seed, invert, sample, _n - 2, _symmetry, _iterations))
            //         {
            //             return map;
            //         }
            //     }
            // }

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

        #endregion
    }
}