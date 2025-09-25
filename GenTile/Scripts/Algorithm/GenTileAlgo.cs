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
}