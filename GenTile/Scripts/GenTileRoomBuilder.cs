using Scaerth;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    public class GenTileRoomBuilder : MonoBehaviour
    {
        public Transform Parent;
        public GenTileRoomPlacer GenTileRoomPlacer;
        public GenTileRoomType GenTileRoomType;

        public void Clear()
        {
            for (int i = Parent.childCount; i-- > 0;)
            {
                DestroyImmediate(Parent.GetChild(i).gameObject);
            }
        }

        public void BuildRooms()
        {
            System.Random random = new System.Random(GenTileRoomPlacer.Seed);
            foreach (var room in GenTileRoomPlacer.PlacedRooms)
            {
                BuildRoomFromTileRoom(random.Next(int.MinValue, int.MaxValue), Parent, room, GenTileRoomType);
            }
        }

        public void BuildRoomFromTileRoom(int seed, Transform parent, GenTileRoom tileRoom, GenTileRoomType roomType)
        {
            System.Random random = new(seed);
            // if (roomType.OuterWall.Count > 0)
            // {
            //     GameObject wallPreset = roomType.OuterWall[random.Next(roomType.OuterWall.Count)];
            //     foreach (var tileWall in tileRoom.PlacedWalls)
            //     {
            //         Vector3Int worldPosition = new Vector3Int(tileWall.Position.x, 0, tileWall.Position.y);
            //         worldPosition *= roomType.TileSize;
            //         GameObject wall = Instantiate(wallPreset, parent);
            //         wall.transform.localPosition = worldPosition;
            //         Vector3 rotation = GenTileRoomPlacer.Tilegen.Tilemap[0].GetTransformMatrix(tileWall.Position).rotation.eulerAngles;
            //         wall.transform.localRotation = Quaternion.Euler(rotation.x, rotation.z, rotation.y);
            //     }
            // }
            //
            // if (roomType.Door.Count > 0)
            // {
            //     GameObject doorPreset = roomType.Door[random.Next(roomType.Door.Count)];
            //     foreach (var tileDoor in tileRoom.PlacedDoors)
            //     {
            //         Vector3Int worldPosition = new Vector3Int(tileDoor.Position.x, 0, tileDoor.Position.y);
            //         worldPosition *= roomType.TileSize;
            //         GameObject door = Instantiate(doorPreset, parent);
            //         door.transform.localPosition = worldPosition;
            //         Vector3 rotation = GenTileRoomPlacer.Tilegen.Tilemap[0].GetTransformMatrix(tileDoor.Position).rotation.eulerAngles;
            //         door.transform.localRotation = Quaternion.Euler(rotation.x, rotation.z, rotation.y);
            //     }
            // }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GenTileRoomBuilder))]
    public class GenTileRoomBuilder_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenTileRoomBuilder genTileRoomBuilder = (GenTileRoomBuilder) target;
            if (GUILayout.Button("Build")) genTileRoomBuilder.BuildRooms();
            if (GUILayout.Button("Clear")) genTileRoomBuilder.Clear();
        }
    }
#endif
}