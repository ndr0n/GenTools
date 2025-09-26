using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenTileAlgorithmData
    {
        public GenTileAlgoType Algorithm;

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Fill)]
#endif
        public GTA_Fill Fill = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Fill)]
#endif
        public Vector2Int FillCount = new Vector2Int(0, 0);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Fill)]
#endif
        public Vector2Int FillPercentage = new Vector2Int(100, 100);

        // RANDOM WALK
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.RandomWalk)]
#endif
        public GTA_RandomWalk RandomWalk = new();

#if UNITY_EDITOR
        [FormerlySerializedAs("Size")]
        [DrawIf("Algorithm", GenTileAlgoType.RandomWalk)]
#endif
        public Vector2Int RandomWalkPercentage = new Vector2Int(50, 200);

        // PERLIN NOISE
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.RandomWalk)]
#endif
        public GTA_PerlinNoise PerlinNoise = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.PerlinNoise)]
#endif
        public Vector2 PerlinNoiseModifier = new Vector2(0f, 0.25f);

        // TUNNEL
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Tunnel)]
#endif
        public GTA_Tunnel Tunnel = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Tunnel)]
#endif
        public Vector2Int TunnelPathWidth = new Vector2Int(2, 6);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Tunnel)]
#endif
        public Vector2Int TunnelXBeginPercent = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Tunnel)]
#endif
        public Vector2Int TunnelXFinishPercent = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Tunnel)]
#endif
        public Vector2Int TunnelYBeginPercent = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Tunnel)]
#endif
        public Vector2Int TunnelYFinishPercent = new Vector2Int(0, 100);

        // CORRIDORS
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Corridors)]
#endif
        public GTA_Corridors Corridors = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Corridors)]
#endif
        public Vector2Int CorridorsLifetime = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Corridors)]
#endif
        public Vector2Int CorridorsChangePercentage = new Vector2Int(0, 100);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.Corridors)]
#endif
        public Vector2Int CorridorsWidth = new Vector2Int(3, 3);

        // ROOM PLACER
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.RoomPlacer)]
#endif
        public GTA_RoomPlacer RoomPlacer = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.RoomPlacer)]
#endif
        public Vector2Int RoomPlacerChance = new Vector2Int(100, 100);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.RoomPlacer)]
#endif
        public Vector2Int RoomPlacerWidth = new Vector2Int(4, 20);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.RoomPlacer)]
#endif
        public Vector2Int RoomPlacerHeight = new Vector2Int(4, 20);

        // BINARY SPACE PARTITION
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.BinarySpacePartition)]
#endif
        public GTA_BinarySpacePartition BinarySpacePartition = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.BinarySpacePartition)]
#endif
        public Vector2Int BSPWidth = new Vector2Int(8, 12);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.BinarySpacePartition)]
#endif
        public Vector2Int BSPHeight = new Vector2Int(8, 12);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.BinarySpacePartition)]
#endif
        public Vector2Int BSPChance = new Vector2Int(100, 100);
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.BinarySpacePartition)]
#endif
        public Vector2Int BSPOffset = new Vector2Int(1, 1);

        // WALL PLACER
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.WallPlacer)]
#endif
        public GTA_WallPlacer WallPlacer = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.WallPlacer)]
#endif
        public Vector2Int WallPlacerPercentage = new Vector2Int(100, 100);

        // WAVE FUNCTION COLLAPSE
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.WallPlacer)]
#endif
        public GTA_WaveFunctionCollapse WaveFunctionCollapse = new();

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.WaveFunctionCollapse)]
#endif
        public Texture2D WFCInputTexture;

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.WaveFunctionCollapse)]
#endif
        public Vector2Int WFCN = new Vector2Int(4, 4);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.WaveFunctionCollapse)]
#endif
        public Vector2Int WFCSymmetry = new Vector2Int(1, 3);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.WaveFunctionCollapse)]
#endif
        public Vector2Int WFCIterations = new Vector2Int(0, 0);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgoType.WaveFunctionCollapse)]
#endif
        public bool WFCInvert = false;
    }
}