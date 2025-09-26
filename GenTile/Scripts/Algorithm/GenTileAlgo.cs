using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GenTools
{
    public enum GenTileAlgoType
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

    [System.Serializable]
    public abstract class GenTileAlgo
    {
        public abstract List<Vector2Int> Execute(List<Vector2Int> available, int seed);
    }
}