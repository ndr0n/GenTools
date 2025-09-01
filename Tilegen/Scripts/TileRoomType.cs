using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "TR", menuName = "Tilegen/TileRoomType")]
    public class TileRoomType : ScriptableObject
    {
        public List<TileBase> RoomTile = new();
        public Vector2Int MinSize = new Vector2Int(1, 1);
        public Vector2Int MaxSize = new Vector2Int(100, 100);
        public List<TileBase> Floor = new();
        public List<TileObjectType> Walls = new();
        public List<TileObjectType> Doors = new();
        public List<TileObjectType> Stairs = new();
        public List<TileObjectType> Objects = new();
    }
}