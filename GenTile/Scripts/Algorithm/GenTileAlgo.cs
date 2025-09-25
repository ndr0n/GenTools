using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GenTools
{
    [System.Serializable]
    public abstract class GenTileAlgo
    {
        public abstract List<Vector2Int> Execute(List<Vector2Int> available, int seed);
    }

    [System.Serializable]
    public class GenTileAlgoData
    {
        public GenTileAlgorithmType Algorithm;

#if UNITY_EDITOR
        [DrawIf("Algorithm", GenTileAlgorithmType.Fill)]
#endif
        public GTA_Fill Fill = new();
    }
}