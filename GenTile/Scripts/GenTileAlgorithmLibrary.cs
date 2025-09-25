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
        Tunneler,
        Roomer,
        BinarySpacePartition,
        Walls,
        WaveFunctionCollapse,
        Random
    }

    public static class GenTileAlgorithmLibrary
    {
        #region Tunneler

        public static byte[,] Tunneler(byte[,] map, byte value, int seed, Vector2Int fill, Vector2Int changePercentage, Vector2Int tunnelWidth, bool overlap)
        {
            System.Random random = new(seed);
            BoundsInt bounds = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(map.GetLength(0), map.GetLength(1), 0));
            float _fill = random.Next(fill.x, fill.y) / 100f;
            int _lifetime = Mathf.RoundToInt(_fill * (bounds.size.x * bounds.size.y));
            int _changePercentage = random.Next(changePercentage.x, changePercentage.y);

            List<Vector3Int> overlapPositions = new();
            if (overlap == false)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        if (map[x, y] > 0) overlapPositions.Add(new Vector3Int(x, y, 0));
                    }
                }
            }

            GenTileTunneler tunneler = new();
            List<Vector3Int> tunnelPositions = tunneler.Init(random.Next(int.MinValue, int.MaxValue), bounds, _lifetime, _changePercentage, tunnelWidth, overlapPositions);
            foreach (var overlapPosition in overlapPositions) tunnelPositions.Remove(overlapPosition);
            foreach (var pos in tunnelPositions) map[pos.x, pos.y] = value;
            // foreach (var b in tunneler.roomPlacements)
            // {
            // for (int x = b.min.x; x < b.max.x; x++)
            // {
            // for (int y = b.min.y; y < b.max.y; y++)
            // {
            // map[x, y] = value;
            // }
            // }
            // }
            return map;
        }

        #endregion

        #region Roomer

        public static byte[,] Roomer(byte[,] map, byte value, int seed, Vector2Int chance, Vector2Int width, Vector2Int height)
        {
            System.Random random = new(seed);
            BoundsInt bounds = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(map.GetLength(0), map.GetLength(1), 0));
            List<Vector3Int> existingPositions = new();
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y] > 0) existingPositions.Add(new Vector3Int(x, y, 0));
                }
            }

            GenTileRoomer roomer = new();
            List<BoundsInt> rooms = roomer.Execute(random.Next(int.MinValue, int.MaxValue), bounds, chance, width, height, existingPositions, new());
            foreach (var room in rooms)
            {
                for (int x = room.min.x; x < room.max.x; x++)
                {
                    for (int y = room.min.y; y < room.max.y; y++)
                    {
                        map[x, y] = value;
                    }
                }
            }

            return map;
        }

        #endregion

        #region BinarySpacePartition

        public static byte[,] BinarySpacePartition(byte[,] map, byte value, int seed, Vector2Int chance, Vector2Int offset, Vector2Int minWidth, Vector2Int minHeight, List<BoundsInt> rooms = null)
        {
            System.Random random = new(seed);
            int of7 = random.Next(offset.x, offset.y + 1);
            BoundsInt bounds = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(map.GetLength(0), map.GetLength(1), 0));
            rooms = GenTileLibrary.BinarySpacePartition(random, bounds, minWidth, minHeight);
            foreach (var room in rooms.OrderBy(x => random.Next()))
            {
                if (random.Next(0, 100) < random.Next(chance.x, chance.y))
                {
                    for (int x = (room.position.x + of7); x < ((room.position.x + room.size.x) - of7 - 1); x++)
                    {
                        for (int y = (room.position.y + of7); y < ((room.position.y + room.size.y) - of7 - 1); y++)
                        {
                            map[x, y] = value;
                        }
                    }
                }
            }
            return map;
        }

        #endregion

        #region Walls

        public static byte[,] Walls(byte[,] map, byte value, int seed, Vector2Int percentage, bool outerWall)
        {
            System.Random random = new(seed);
            int _percentage = random.Next(percentage.x, percentage.y);

            byte[,] fill = new byte[map.GetLength(0), map.GetLength(1)];
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    fill[x, y] = map[x, y];
                    bool placeWall = false;
                    if (outerWall)
                    {
                        if (map[x, y] == 0)
                        {
                            if (x == 0 || y == 0 || x == (map.GetLength(0) - 1) || y == (map.GetLength(1) - 1))
                            {
                                placeWall = true;
                            }
                            else
                            {
                                if (map[x - 1, y] > 0) placeWall = true;
                                else if (map[x + 1, y] > 0) placeWall = true;
                                else if (map[x, y + 1] > 0) placeWall = true;
                                else if (map[x, y - 1] > 0) placeWall = true;
                                else if (map[x + 1, y + 1] > 0) placeWall = true;
                                else if (map[x + 1, y - 1] > 0) placeWall = true;
                                else if (map[x - 1, y - 1] > 0) placeWall = true;
                                else if (map[x - 1, y + 1] > 0) placeWall = true;
                            }
                        }
                    }
                    else
                    {
                        if (map[x, y] > 0)
                        {
                            if (x == 0 || y == 0 || x == (map.GetLength(0) - 1) || y == (map.GetLength(1) - 1))
                            {
                                placeWall = true;
                            }
                            else
                            {
                                if (map[x - 1, y] == 0) placeWall = true;
                                else if (map[x + 1, y] == 0) placeWall = true;
                                else if (map[x, y + 1] == 0) placeWall = true;
                                else if (map[x, y - 1] == 0) placeWall = true;
                                else if (map[x + 1, y + 1] == 0) placeWall = true;
                                else if (map[x + 1, y - 1] == 0) placeWall = true;
                                else if (map[x - 1, y - 1] == 0) placeWall = true;
                                else if (map[x - 1, y + 1] == 0) placeWall = true;
                            }
                        }
                    }
                    if (placeWall)
                    {
                        if (random.Next(0, 100) < _percentage) fill[x, y] = value;
                    }
                }
            }

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = fill[x, y];
                }
            }

            return map;
        }

        #endregion

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