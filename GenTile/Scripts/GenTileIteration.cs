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

        // ROOMS 
#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Rooms)]
#endif
        public Vector2Int RoomAmount = new Vector2Int(5, 20);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Rooms)]
#endif
        public Vector2Int RoomWidth = new Vector2Int(5, 10);

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Rooms)]
#endif
        public Vector2Int RoomHeight = new Vector2Int(5, 10);

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

    [System.Serializable]
    public class GenTileIteration
    {
        public TileBase Tile;
        public GenTileType Type;
        public List<GenTileAlgorithmData> Algorithm = new List<GenTileAlgorithmData>();
    }
}