using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public struct TilegenLayer
    {
        public List<GenTileIteration> Iterations;
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "TP", menuName = "GenTools/GenTile/Preset")]
    public class GenTilePreset : ScriptableObject
    {
        public List<TilegenLayer> Layer = new();
        public List<GenTileRoomType> RoomTypes = new();
    }
}