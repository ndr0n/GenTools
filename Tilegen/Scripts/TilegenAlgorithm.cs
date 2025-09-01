using System.Collections.Generic;
using UnityEngine;

namespace GenTools
{
    public enum TilegenAlgorithmType
    {
        Fill,
        Degrade,
        RandomWalk,
        PerlinNoise,
        Tunnel,
        Rooms,
        Walls,
        WaveFunctionCollapse
    }

    public static class TilegenAlgorithm
    {
        #region Fill

        public static byte[,] Fill(byte[,] map, byte value, int seed, Vector2Int percentage)
        {
            System.Random random = new(seed);
            int _percentage = random.Next(percentage.x, percentage.y);
            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
                {
                    if (random.Next(0, 100) < _percentage) map[x, y] = value;
                }
            }
            return map;
        }

        #endregion

        #region Degrade

        public static byte[,] Degrade(byte[,] map, byte value, int seed, Vector2Int chance)
        {
            System.Random random = new(seed);
            int _chance = random.Next(chance.x, chance.y);
            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
                {
                    if (map[x, y] == value)
                    {
                        if (random.Next(0, 100) < _chance) map[x, y] = 0;
                    }
                }
            }
            return map;
        }

        #endregion

        #region RandomWalk

        public static byte[,] RandomWalk(byte[,] map, byte value, int seed, Vector2Int size)
        {
            System.Random random = new(seed);
            int x = random.Next(0, map.GetUpperBound(0));
            int y = random.Next(0, map.GetUpperBound(1));
            int _size = random.Next(size.x, size.y);

            int count = ((map.GetUpperBound(1) * map.GetUpperBound(0)) * _size) / 100;

            map[x, y] = value;

            for (int i = 0; i < count; i++)
            {
                int randomDirection = random.Next(4);
                switch (randomDirection)
                {
                    case 0: // Up
                        if (y < (map.GetUpperBound(1) - 1))
                        {
                            y++;
                            map[x, y] = value;
                        }
                        break;
                    case 1: // Down
                        if (y >= 1)
                        {
                            y--;
                            map[x, y] = value;
                        }
                        break;
                    case 2: //Right
                        if (x < (map.GetUpperBound(0) - 1))
                        {
                            x++;
                            map[x, y] = value;
                        }
                        break;
                    case 3: //Left
                        if (x >= 1)
                        {
                            x--;
                            map[x, y] = value;
                        }
                        break;
                }
            }
            return map;
        }

        #endregion

        #region PerlinNoise

        public static byte[,] PerlinNoise(byte[,] map, byte value, int seed, Vector2 modifier)
        {
            System.Random random = new(seed);
            float _modifier = random.Next(Mathf.RoundToInt(modifier.x * int.MaxValue), Mathf.RoundToInt(modifier.y * int.MaxValue)) / (float) int.MaxValue;
            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
                {
                    int newPoint = Mathf.RoundToInt(Mathf.PerlinNoise(x * _modifier, y * _modifier));
                    if (newPoint > 0) map[x, y] = value;
                }
            }
            return map;
        }

        #endregion

        #region Tunnel

        public static byte[,] Tunnel(byte[,] map, byte value, int seed, Vector2Int pathWidth, Vector2 xBeginPercent, Vector2 xFinishPercent, Vector2 yBeginPercent, Vector2 yFinishPercent)
        {
            System.Random random = new System.Random(seed);

            int _xBeginPercent = random.Next((int) xBeginPercent.x, (int) xBeginPercent.y + 1);
            int _xFinishPercent = random.Next((int) xFinishPercent.x, (int) xFinishPercent.y + 1);
            int _yBeginPercent = random.Next((int) yBeginPercent.x, (int) yBeginPercent.y + 1);
            int _yFinishPercent = random.Next((int) yFinishPercent.x, (int) yFinishPercent.y + 1);

            int xBegin = Mathf.RoundToInt(map.GetUpperBound(0) * (_xBeginPercent / 100f));
            int xEnd = Mathf.RoundToInt(map.GetUpperBound(0) * (_xFinishPercent / 100f));
            int yBegin = Mathf.RoundToInt(map.GetUpperBound(1) * (_yBeginPercent / 100f));
            int yEnd = Mathf.RoundToInt(map.GetUpperBound(1) * (_yFinishPercent / 100f));

            Vector2 pos = new Vector2Int(xBegin, yBegin);
            Vector2 destination = new Vector2Int(xEnd, yEnd);

            map = Tunnel_Dig(map, value, random, pathWidth, pos);

            for (int i = 0; i < (map.GetUpperBound(0) + map.GetUpperBound(1)); i++)
            {
                if (Vector2.Distance(pos, destination) > 1)
                {
                    pos = Vector2.MoveTowards(pos, destination, 1);
                    map = Tunnel_Dig(map, value, random, pathWidth, pos);
                }
                else break;
            }

            return map;
        }

        static byte[,] Tunnel_Dig(byte[,] map, byte value, System.Random random, Vector2Int pathWidth, Vector2 pos)
        {
            int tunnelWidth = random.Next(pathWidth.x, pathWidth.y + 1);
            for (int widthIterator = -tunnelWidth; widthIterator <= tunnelWidth; widthIterator++)
            {
                Vector2Int p = new Vector2Int(Mathf.RoundToInt(pos.x + widthIterator), Mathf.RoundToInt(pos.y));
                if (p.x < 0 || p.y < 0 || p.x >= map.GetUpperBound(0) || p.y >= map.GetUpperBound(1)) continue;
                map[p.x, p.y] = value;
            }
            tunnelWidth = random.Next(pathWidth.x, pathWidth.y + 1);
            for (int heightIterator = -tunnelWidth; heightIterator <= tunnelWidth; heightIterator++)
            {
                Vector2Int p = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y + heightIterator));
                if (p.x < 0 || p.y < 0 || p.x >= map.GetUpperBound(0) || p.y >= map.GetUpperBound(1)) continue;
                map[p.x, p.y] = value;
            }
            return map;
        }

        #endregion

        #region Rooms

        public static byte[,] Rooms(byte[,] map, byte value, int seed, Vector2Int roomAmount, Vector2Int roomWidth, Vector2Int roomHeight)
        {
            System.Random random = new System.Random(seed);
            int count = random.Next(roomAmount.x, roomAmount.y + 1);
            for (int i = 0; i < count; i++)
            {
                int w = random.Next(roomWidth.x, roomWidth.y + 1);
                int h = random.Next(roomHeight.x, roomHeight.y + 1);
                Vector2Int pos = new Vector2Int(random.Next(0, map.GetUpperBound(0) - w + 1), random.Next(0, map.GetUpperBound(1) - h + 1));
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        map[pos.x + x, pos.y + y] = value;
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

            byte[,] fill = new byte[map.GetUpperBound(0), map.GetUpperBound(1)];
            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
                {
                    fill[x, y] = map[x, y];
                    bool placeWall = false;
                    if (outerWall)
                    {
                        if (map[x, y] == 0)
                        {
                            if (x == 0 || y == 0 || x == (map.GetUpperBound(0) - 1) || y == (map.GetUpperBound(1) - 1))
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
                            if (x == 0 || y == 0 || x == (map.GetUpperBound(0) - 1) || y == (map.GetUpperBound(1) - 1))
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

            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
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
            for (int x = 0; x < sample.GetUpperBound(0); x++)
            {
                for (int y = 0; y < sample.GetUpperBound(1); y++)
                {
                    Color32 color = inputTexture.GetPixel(x, y);
                    byte v = 0;
                    if (color.r > 50 || color.g > 50 || color.b > 50) v = 1;
                    sample[x, y] = v;
                }
            }

            int _seed = random.Next(int.MinValue, int.MaxValue);
            if (TryRunWfcOverlappingModel(map, value, _seed, invert, sample, _n, _symmetry, _iterations)) return map;
            if (_n > 2)
            {
                if (TryRunWfcOverlappingModel(map, value, _seed, invert, sample, _n - 1, _symmetry, _iterations)) return map;
                if (_n > 3)
                {
                    if (TryRunWfcOverlappingModel(map, value, _seed, invert, sample, _n - 2, _symmetry, _iterations)) return map;
                }
            }

            return map;
        }

        static bool TryRunWfcOverlappingModel(byte[,] map, byte value, int seed, bool invert, byte[,] sample, int n, int symmetry, int iterations)
        {
            int width = map.GetUpperBound(0) + 2;
            int height = map.GetUpperBound(1) + 2;
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
                for (int x = 0; x < map.GetUpperBound(0); x++)
                {
                    for (int y = 0; y < map.GetUpperBound(1); y++)
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