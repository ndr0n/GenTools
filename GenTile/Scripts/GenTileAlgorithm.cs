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

            List<Vector2Int> available = new();
            List<Vector2Int> placed = new();

            byte[,] m = new byte[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x + Offset.x, y + Offset.y);
                    m[x, y] = map[pos.x, pos.y];
                    available.Add(pos);
                }
            }

            foreach (var algorithm in Algorithm)
            {
                switch (algorithm.Algorithm)
                {
                    case GenTileAlgorithmType.Fill:
                        algorithm.Fill.Count = algorithm.FillCount;
                        algorithm.Fill.Percentage = algorithm.FillPercentage;
                        List<Vector2Int> placedFill = algorithm.Fill.Execute(available, seed);
                        foreach (var pos in placedFill)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgorithmType.RandomWalk:
                        algorithm.RandomWalk.Percentage = algorithm.RandomWalkPercentage;
                        List<Vector2Int> placedRandomWalk = algorithm.RandomWalk.Execute(available, seed);
                        foreach (var pos in placedRandomWalk)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgorithmType.PerlinNoise:
                        algorithm.PerlinNoise.Modifier = algorithm.PerlinNoiseModifier;
                        List<Vector2Int> placedPerlinNoise = algorithm.PerlinNoise.Execute(available, seed);
                        foreach (var pos in placedPerlinNoise)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgorithmType.Tunnel:
                        algorithm.Tunnel.PathWidth = algorithm.TunnelPathWidth;
                        algorithm.Tunnel.XBeginPercentage = algorithm.TunnelXBeginPercent;
                        algorithm.Tunnel.XFinishPercentage = algorithm.TunnelXFinishPercent;
                        algorithm.Tunnel.YBeginPercentage = algorithm.TunnelYBeginPercent;
                        algorithm.Tunnel.YFinishPercentage = algorithm.TunnelYFinishPercent;
                        List<Vector2Int> placedTunnel = algorithm.Tunnel.Execute(available, seed);
                        foreach (var pos in placedTunnel)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgorithmType.Corridors:
                        algorithm.Corridors.Lifetime = algorithm.CorridorsLifetime;
                        algorithm.Corridors.TunnelWidth = algorithm.CorridorsWidth;
                        algorithm.Corridors.ChangePercentage = algorithm.CorridorsChangePercentage;
                        List<Vector2Int> placedTunneler = algorithm.Corridors.Execute(available, seed);
                        foreach (var pos in placedTunneler)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgorithmType.RoomPlacer:
                        algorithm.RoomPlacer.Chance = algorithm.RoomPlacerChance;
                        algorithm.RoomPlacer.Width = algorithm.RoomPlacerWidth;
                        algorithm.RoomPlacer.Height = algorithm.RoomPlacerHeight;
                        List<Vector2Int> placedRooms = algorithm.RoomPlacer.Execute(available, seed);
                        foreach (var pos in placedRooms)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgorithmType.BinarySpacePartition:
                        algorithm.BinarySpacePartition.Width = algorithm.BSPWidth;
                        algorithm.BinarySpacePartition.Height = algorithm.BSPHeight;
                        algorithm.BinarySpacePartition.Chance = algorithm.BSPChance;
                        algorithm.BinarySpacePartition.Offset = algorithm.BSPOffset;
                        List<Vector2Int> placedBsp = algorithm.BinarySpacePartition.Execute(available, seed);
                        foreach (var pos in placedBsp)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgorithmType.WallPlacer:
                        algorithm.WallPlacer.Percentage = algorithm.WallPlacerPercentage;
                        List<Vector2Int> placedWallPlacer = algorithm.WallPlacer.Execute(available, seed);
                        foreach (var pos in placedWallPlacer)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgorithmType.WaveFunctionCollapse:
                        algorithm.WaveFunctionCollapse.InputTexture = algorithm.WFCInputTexture;
                        algorithm.WaveFunctionCollapse.Invert = algorithm.WFCInvert;
                        algorithm.WaveFunctionCollapse.N = algorithm.WFCN;
                        algorithm.WaveFunctionCollapse.Iterations = algorithm.WFCIterations;
                        algorithm.WaveFunctionCollapse.Symmetry = algorithm.WFCSymmetry;
                        List<Vector2Int> placedWfc = algorithm.WaveFunctionCollapse.Execute(available, seed);
                        foreach (var pos in placedWfc)
                        {
                            available.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                }
            }

            foreach (var pos in placed)
            {
                m[pos.x - Offset.x, pos.y - Offset.y] = value;
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

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Fill)]
#endif
        public GTA_Fill Fill = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Fill)]
#endif
        public Vector2Int FillCount = new Vector2Int(0, 0);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Fill)]
#endif
        public Vector2Int FillPercentage = new Vector2Int(100, 100);

        // RANDOM WALK
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.RandomWalk)]
#endif
        public GTA_RandomWalk RandomWalk = new();

#if UNITY_EDITOR
        [FormerlySerializedAs("Size")]
        [DrawIf("Algorithm", GenTileAlgorithmType.RandomWalk)]
#endif
        public Vector2Int RandomWalkPercentage = new Vector2Int(50, 200);

        // PERLIN NOISE
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.RandomWalk)]
#endif
        public GTA_PerlinNoise PerlinNoise = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.PerlinNoise)]
#endif
        public Vector2 PerlinNoiseModifier = new Vector2(0f, 0.25f);

        // TUNNEL
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public GTA_Tunnel Tunnel = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2Int TunnelPathWidth = new Vector2Int(2, 6);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2Int TunnelXBeginPercent = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2Int TunnelXFinishPercent = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2Int TunnelYBeginPercent = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
#endif
        public Vector2Int TunnelYFinishPercent = new Vector2Int(0, 100);

        // CORRIDORS
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Corridors)]
#endif
        public GTA_Corridors Corridors = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Corridors)]
#endif
        public Vector2Int CorridorsLifetime = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Corridors)]
#endif
        public Vector2Int CorridorsChangePercentage = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Corridors)]
#endif
        public Vector2Int CorridorsWidth = new Vector2Int(3, 3);

        // ROOM PLACER
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.RoomPlacer)]
#endif
        public GTA_RoomPlacer RoomPlacer = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.RoomPlacer)]
#endif
        public Vector2Int RoomPlacerChance = new Vector2Int(100, 100);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.RoomPlacer)]
#endif
        public Vector2Int RoomPlacerWidth = new Vector2Int(4, 20);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.RoomPlacer)]
#endif
        public Vector2Int RoomPlacerHeight = new Vector2Int(4, 20);

        // BINARY SPACE PARTITION
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public GTA_BinarySpacePartition BinarySpacePartition = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public Vector2Int BSPWidth = new Vector2Int(8, 12);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public Vector2Int BSPHeight = new Vector2Int(8, 12);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public Vector2Int BSPChance = new Vector2Int(100, 100);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.BinarySpacePartition)]
#endif
        public Vector2Int BSPOffset = new Vector2Int(1, 1);

        // WALL PLACER
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WallPlacer)]
#endif
        public GTA_WallPlacer WallPlacer = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WallPlacer)]
#endif
        public Vector2Int WallPlacerPercentage = new Vector2Int(100, 100);

        // WAVE FUNCTION COLLAPSE
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WallPlacer)]
#endif
        public GTA_WaveFunctionCollapse WaveFunctionCollapse = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public Texture2D WFCInputTexture;

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public Vector2Int WFCN = new Vector2Int(4, 4);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public Vector2Int WFCSymmetry = new Vector2Int(1, 3);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public Vector2Int WFCIterations = new Vector2Int(0, 0);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
#endif
        public bool WFCInvert = false;
    }
}