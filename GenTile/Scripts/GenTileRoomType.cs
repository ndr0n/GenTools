using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "TR", menuName = "GenTools/GenTile/GenTileRoomType")]
    public class GenTileRoomType : ScriptableObject
    {
        public List<TileBase> RoomTile = new();
        public Vector2Int MinSize = new Vector2Int(1, 1);
        public Vector2Int MaxSize = new Vector2Int(100, 100);
        public List<TileBase> Floor = new();
        public List<GenTileObjectData> Walls = new();
        public List<GenTileObjectData> Doors = new();
        public List<GenTileObjectData> Stairs = new();
        public List<GenTileObjectData> Objects = new();
    }
}