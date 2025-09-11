using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "RT", menuName = "GenTools/GenTile/GenTileRoomType")]
    public class GenTileRoomType : ScriptableObject
    {
        [Header("Tagging")]
        public List<Sprite> RoomSprite = new();
        public Vector2Int MinSize = new Vector2Int(1, 1);
        public Vector2Int MaxSize = new Vector2Int(100, 100);

        [Header("Building")]
        public List<TileBase> Floor = new();
        public List<TileBase> Doors = new();
        public List<GenTileWallData> Walls = new();
        // public List<GenTileObjectData> Stairs = new();
        public List<GenTileObjectData> Objects = new();

        [Header("Tunneling")]
        public TileBase TunnelTile;
        public bool PlaceDoorsInsideRoom = false;
        public Vector2Int TunnelAmount = new Vector2Int(4, 4);
        public TunnelingAlgorithm TunnelingAlgorithm = TunnelingAlgorithm.Directional;
    }
}