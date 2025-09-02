using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "TR", menuName = "GenTools/Tilegen/TileRoomType")]
    public class TileRoomType : ScriptableObject
    {
        public List<TileBase> RoomTile = new();
        public Vector2Int MinSize = new Vector2Int(1, 1);
        public Vector2Int MaxSize = new Vector2Int(100, 100);
        public List<TileBase> Floor = new();
        public List<TileObjectData> Walls = new();
        public List<TileObjectData> Doors = new();
        public List<TileObjectData> Stairs = new();
        public List<TileObjectData> Objects = new();
    }
}