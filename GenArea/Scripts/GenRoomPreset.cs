using System.Collections.Generic;
using UnityEngine;

namespace GenTools
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "RP", menuName = "GenTools/Room/GenRoomPreset")]
    public class GenRoomPreset : ScriptableObject
    {
        public Vector3 TileSize = new Vector3(4, 4, 4);

        public List<GameObject> Floor = new();
        public List<GameObject> OuterWall = new();
        public List<GameObject> OuterDoor = new();
        public List<GameObject> Roof = new();

        public List<GameObject> InnerWall = new();
        public List<GameObject> InnerDoor = new();

        public List<GameObject> Pillar = new();
        public List<GameObject> Elevator = new();

        public List<GenRoomStair> Stair = new();
        public List<GenRoomRail> Rail = new();

        public List<GameObject> Lamps = new();
        public List<GameObject> Object = new();
    }

    [System.Serializable]
    public class GenRoomStair
    {
        public enum RoomStairType
        {
            Square,
            // Straight,
            // Floor,
        }

        public GameObject Stair;
        public RoomStairType Type;
    }

    [System.Serializable]
    public class GenRoomRail
    {
        public enum RoomRailType
        {
            Rail,
            Wall,
        }

        public GameObject Rail;
        public RoomRailType Type;
    }
}