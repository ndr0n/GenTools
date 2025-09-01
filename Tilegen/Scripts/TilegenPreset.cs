using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public struct TilegenLayer
    {
        public List<TilegenIteration> Iterations;
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "TP", menuName = "Tilegen/Preset")]
    public class TilegenPreset : ScriptableObject
    {
        public List<TilegenLayer> Layer = new();
    }
}