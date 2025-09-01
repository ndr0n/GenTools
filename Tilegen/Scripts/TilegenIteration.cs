using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    public enum TilegenType : int
    {
        Terrain = 0,
        Scatter = 1,
        Objects = 2,
    }

    [System.Serializable]
    public class TilegenAlgorithmData
    {
        public TilegenAlgorithmType Algorithm;

        [DrawIf("Algorithm", TilegenAlgorithmType.Fill)]
        public Vector2Int FillPercentage = new Vector2Int(100, 100);

        [DrawIf("Algorithm", TilegenAlgorithmType.Degrade)]
        public Vector2Int DegradePercentage = new Vector2Int(25, 75);

        [DrawIf("Algorithm", TilegenAlgorithmType.RandomWalk)]
        public Vector2Int Size = new Vector2Int(50, 200);

        [DrawIf("Algorithm", TilegenAlgorithmType.PerlinNoise)]
        public Vector2 PerlinNoiseModifier = new Vector2(0f, 0.25f);

        [DrawIf("Algorithm", TilegenAlgorithmType.Tunnel)]
        public Vector2Int PathWidth = new Vector2Int(2, 6);
        [DrawIf("Algorithm", TilegenAlgorithmType.Tunnel)]
        public Vector2 XBeginPercent = new Vector2(0, 100);
        [DrawIf("Algorithm", TilegenAlgorithmType.Tunnel)]
        public Vector2 XFinishPercent = new Vector2(0, 100);
        [DrawIf("Algorithm", TilegenAlgorithmType.Tunnel)]
        public Vector2 YBeginPercent = new Vector2(0, 100);
        [DrawIf("Algorithm", TilegenAlgorithmType.Tunnel)]
        public Vector2 YFinishPercent = new Vector2(0, 100);

        [FormerlySerializedAs("Amount")]
        [DrawIf("Algorithm", TilegenAlgorithmType.Rooms)]
        public Vector2Int RoomAmount = new Vector2Int(5, 20);
        [FormerlySerializedAs("Width")]
        [DrawIf("Algorithm", TilegenAlgorithmType.Rooms)]
        public Vector2Int RoomWidth = new Vector2Int(5, 10);
        [FormerlySerializedAs("Height")]
        [DrawIf("Algorithm", TilegenAlgorithmType.Rooms)]
        public Vector2Int RoomHeight = new Vector2Int(5, 10);

        [DrawIf("Algorithm", TilegenAlgorithmType.Walls)]
        public bool OuterWall = false;
        [DrawIf("Algorithm", TilegenAlgorithmType.Walls)]
        public Vector2Int WallPercentage = new Vector2Int(0, 100);

        [DrawIf("Algorithm", TilegenAlgorithmType.WaveFunctionCollapse)]
        public Texture2D InputTexture;
        [DrawIf("Algorithm", TilegenAlgorithmType.WaveFunctionCollapse)]
        public Vector2Int N = new Vector2Int(4, 4);
        [DrawIf("Algorithm", TilegenAlgorithmType.WaveFunctionCollapse)]
        public Vector2Int Symmetry = new Vector2Int(1, 3);
        [DrawIf("Algorithm", TilegenAlgorithmType.WaveFunctionCollapse)]
        public Vector2Int Iterations = new Vector2Int(0, 0);
        [DrawIf("Algorithm", TilegenAlgorithmType.WaveFunctionCollapse)]
        public bool Invert = false;
    }

    [System.Serializable]
    public class TilegenIteration
    {
        public TileBase Tile;
        public TilegenType Type;
        public List<TilegenAlgorithmData> Algorithm = new List<TilegenAlgorithmData>();
    }
}