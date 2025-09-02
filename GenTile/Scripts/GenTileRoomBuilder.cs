using System.Collections.Generic;
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
        public GenRoom GenRoomPrefab;
        public GenTileRoomPlacer GenTileRoomPlacer;
        public readonly List<GenRoom> BuiltRooms = new();

        public void Clear()
        {
            foreach (var room in BuiltRooms)
            {
                if (room != null) DestroyImmediate(room.gameObject);
            }
            BuiltRooms.Clear();
            for (int i = Parent.childCount; i-- > 0;)
            {
                DestroyImmediate(Parent.GetChild(i).gameObject);
            }
        }

        public async Awaitable BuildRooms()
        {
            Clear();
            System.Random random = new System.Random(GenTileRoomPlacer.Seed);
            foreach (var room in GenTileRoomPlacer.PlacedRooms)
            {
                await BuildRoomFromTileRoom(Parent, room, random);
            }
        }

        public async Awaitable BuildRoomFromTileRoom(Transform parent, GenTileRoom tileRoom, System.Random random)
        {
            GenRoom room = Instantiate(GenRoomPrefab.gameObject, parent).GetComponent<GenRoom>();
            room.transform.localPosition = new Vector3(tileRoom.Position.x * room.TileSize.x, 0, tileRoom.Position.y * room.TileSize.z);
            room.transform.localRotation = Quaternion.identity;
            room.GridSize = new Vector3Int(tileRoom.Size.x, 1, tileRoom.Size.y);
            await room.Generate();
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