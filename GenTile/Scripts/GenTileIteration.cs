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

        [DrawIf("Algorithm", GenTileAlgorithmType.Fill)]
        public Vector2Int FillPercentage = new Vector2Int(100, 100);

        [DrawIf("Algorithm", GenTileAlgorithmType.Degrade)]
        public Vector2Int DegradePercentage = new Vector2Int(25, 75);

        [DrawIf("Algorithm", GenTileAlgorithmType.RandomWalk)]
        public Vector2Int Size = new Vector2Int(50, 200);

        [DrawIf("Algorithm", GenTileAlgorithmType.PerlinNoise)]
        public Vector2 PerlinNoiseModifier = new Vector2(0f, 0.25f);

        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
        public Vector2Int PathWidth = new Vector2Int(2, 6);
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
        public Vector2 XBeginPercent = new Vector2(0, 100);
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
        public Vector2 XFinishPercent = new Vector2(0, 100);
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
        public Vector2 YBeginPercent = new Vector2(0, 100);
        [DrawIf("Algorithm", GenTileAlgorithmType.Tunnel)]
        public Vector2 YFinishPercent = new Vector2(0, 100);

        [FormerlySerializedAs("Amount")]
        [DrawIf("Algorithm", GenTileAlgorithmType.Rooms)]
        public Vector2Int RoomAmount = new Vector2Int(5, 20);
        [FormerlySerializedAs("Width")]
        [DrawIf("Algorithm", GenTileAlgorithmType.Rooms)]
        public Vector2Int RoomWidth = new Vector2Int(5, 10);
        [FormerlySerializedAs("Height")]
        [DrawIf("Algorithm", GenTileAlgorithmType.Rooms)]
        public Vector2Int RoomHeight = new Vector2Int(5, 10);

        [DrawIf("Algorithm", GenTileAlgorithmType.Walls)]
        public bool OuterWall = false;
        [DrawIf("Algorithm", GenTileAlgorithmType.Walls)]
        public Vector2Int WallPercentage = new Vector2Int(0, 100);

        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
        public Texture2D InputTexture;
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
        public Vector2Int N = new Vector2Int(4, 4);
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
        public Vector2Int Symmetry = new Vector2Int(1, 3);
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
        public Vector2Int Iterations = new Vector2Int(0, 0);
        [DrawIf("Algorithm", GenTileAlgorithmType.WaveFunctionCollapse)]
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