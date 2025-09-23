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
        public List<TileBase> RoomTile = new();
        public Vector2Int MinSize = new Vector2Int(1, 1);
        public Vector2Int MaxSize = new Vector2Int(100, 100);

        [Header("Doors")]
        public List<TileBase> Doors = new();
        public List<TileBase> OuterFloorTile = new();
        public Vector2Int DoorCount = new Vector2Int(4, 4);

        [Header("Building")]
        public List<GenTileAlgorithm> Floor = new();
        public List<GenTileAlgorithm> Walls = new();
        public List<GenTileAlgorithm> Balcony = new();
        public List<GenTileObjectData> Objects = new();

        [Header("Tunneling")]
        public TileBase TunnelTile;
        public bool PlaceDoorsInsideRoom = false;
        public Vector2Int TunnelAmount = new Vector2Int(4, 4);
        public TunnelingAlgorithm TunnelingAlgorithm = TunnelingAlgorithm.Directional;
    }
}