using System.Collections.Generic;
using GenTools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "RT", menuName = "GenTools/Room/GenRoomType")]
    public class GenRoomType : ScriptableObject
    {
        public Vector2Int MinSize = new Vector2Int(1, 1);
        public Vector2Int MaxSize = new Vector2Int(100, 100);
        public Vector2Int StairCount = new Vector2Int(2, 2);
        public List<GenRoomPreset> Presets = new();
    }
}