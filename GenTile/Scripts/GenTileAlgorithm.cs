using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    public enum GenTileType : int
    {
        Terrain = 0,
        Scatter = 1,
        Objects = 2,
    }

    [System.Serializable]
    public class GenTileAlgorithm
    {
        public TileBase Tile;
        public GenTileType Type;
        public Vector2Int Offset = new Vector2Int(0, 0);
        public List<GenTileAlgorithmData> Algorithm = new List<GenTileAlgorithmData>();

        public byte[,] Execute(byte[,] map, byte value, int seed)
        {
            Vector2Int size = new Vector2Int(map.GetLength(0) - (Offset.x * 2), map.GetLength(1) - (Offset.y * 2));
            if (size.x < 0 || size.y < 0) return map;
            byte[,] m = new byte[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x + Offset.x, y + Offset.y);
                    m[x, y] = map[pos.x, pos.y];
                }
            }

            foreach (var algorithm in Algorithm)
            {
                switch (algorithm.Algorithm)
                {
                    case GenTileAlgorithmType.Fill:
                        m = GenTileAlgorithmLibrary.Fill(m, value, seed, algorithm.FillPercentage);
                        break;
                    case GenTileAlgorithmType.Degrade:
                        m = GenTileAlgorithmLibrary.Degrade(m, value, seed, algorithm.DegradePercentage);
                        break;
                    case GenTileAlgorithmType.RandomWalk:
                        m = GenTileAlgorithmLibrary.RandomWalk(m, value, seed, algorithm.Size);
                        break;
                    case GenTileAlgorithmType.PerlinNoise:
                        m = GenTileAlgorithmLibrary.PerlinNoise(m, value, seed, algorithm.PerlinNoiseModifier);
                        break;
                    case GenTileAlgorithmType.Tunnel:
                        m = GenTileAlgorithmLibrary.Tunnel(m, value, seed, algorithm.PathWidth, algorithm.XBeginPercent, algorithm.XFinishPercent, algorithm.YBeginPercent, algorithm.YFinishPercent);
                        break;
                    case GenTileAlgorithmType.Tunneler:
                        m = GenTileAlgorithmLibrary.Tunneler(m, value, seed, algorithm.TunnelerLifetime, algorithm.TunnelerChangePercentage, algorithm.TunnelerWidth, algorithm.TunnelerOverlap);
                        break;
                    case GenTileAlgorithmType.Roomer:
                        m = GenTileAlgorithmLibrary.Roomer(m, value, seed, algorithm.RoomerChance, algorithm.RoomerWidth, algorithm.RoomerHeight);
                        break;
                    case GenTileAlgorithmType.BinarySpacePartition:
                        m = GenTileAlgorithmLibrary.BinarySpacePartition(m, value, seed, algorithm.Percentage, algorithm.Offset, algorithm.MinRoomWidth, algorithm.MinRoomHeight);
                        break;
                    case GenTileAlgorithmType.Walls:
                        m = GenTileAlgorithmLibrary.Walls(m, value, seed, algorithm.WallPercentage, algorithm.OuterWall);
                        break;
                    case GenTileAlgorithmType.WaveFunctionCollapse:
                        m = GenTileAlgorithmLibrary.WFC_Overlapping(m, value, seed, algorithm.Invert, algorithm.InputTexture, algorithm.N, algorithm.Symmetry, algorithm.Iterations);
                        break;
                }
            }

            for (int x = 0; x < m.GetLength(0); x++)
            {
                for (int y = 0; y < m.GetLength(1); y++)
                {
                    Vector2Int pos = new Vector2Int(x + Offset.x, y + Offset.y);
                    map[pos.x, pos.y] = m[x, y];
                }
            }

            return map;
        }
    }

    [System.Serializable]
    public class GenTileAlgorithmData
    {
        public GenTileAlgorithmType Algorithm;

        // FILL
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Fill)]
#endif
        public Vector2Int FillPercentage = new Vector2Int(100, 100);

        // DEGRADE
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Degrade)]
#endif
        public Vector2Int DegradePercentage = new Vector2Int(25, 75);

        // RANDOM WALK
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.RandomWalk)]
#endif
        public Vector2Int Size = new Vector2Int(50, 200);

        // PERLIN NOISE
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.PerlinNoise)]
#endif
        public Vector2 PerlinNoiseModifier = new Vector2(0f, 0.25f);

        // TUNNEL
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2Int PathWidth = new Vector2Int(2, 6);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2 XBeginPercent = new Vector2(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2 XFinishPercent = new Vector2(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2 YBeginPercent = new Vector2(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2 YFinishPercent = new Vector2(0, 100);

        // TUNNELER
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunneler)]
#endif
        public Vector2Int TunnelerLifetime = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunneler)]
#endif
        public Vector2Int TunnelerChangePercentage = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunneler)]
#endif
        public Vector2Int TunnelerWidth = new Vector2Int(3, 3);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunneler)]
#endif
        public bool TunnelerOverlap = false;

        // ROOMER
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Roomer)]
#endif
        public Vector2Int RoomerChance = new Vector2Int(100, 100);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Roomer)]
#endif
        public Vector2Int RoomerWidth = new Vector2Int(5, 10);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Roomer)]
#endif
        public Vector2Int RoomerHeight = new Vector2Int(5, 10);

        // BINARY SPACE PARTITION
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public Vector2Int Percentage = new Vector2Int(100, 100);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public Vector2Int Offset = new Vector2Int(1, 1);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public Vector2Int MinRoomWidth = new Vector2Int(5, 10);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public Vector2Int MinRoomHeight = new Vector2Int(5, 10);

        // WALLS
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Walls)]
#endif
        public bool OuterWall = false;

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Walls)]
#endif
        public Vector2Int WallPercentage = new Vector2Int(0, 100);

        // WAVE FUNCTION COLLAPSE
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public Texture2D InputTexture;

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public Vector2Int N = new Vector2Int(4, 4);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public Vector2Int Symmetry = new Vector2Int(1, 3);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public Vector2Int Iterations = new Vector2Int(0, 0);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public bool Invert = false;
    }
}