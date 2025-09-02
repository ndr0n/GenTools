using System.Collections.Generic;
using Scaerth;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    public class GenTileAreaBuilder : MonoBehaviour
    {
        public GenTileRoomPlacer GenTileRoomPlacer;
        public GenArea GenArea;

        public void Clear()
        {
            GenArea.Clear();
        }

        public async Awaitable BuildRooms()
        {
            Clear();
            System.Random random = new System.Random(GenTileRoomPlacer.Seed);

            GenArea.MainRoom.RandomSeed = false;
            GenArea.MainRoom.Seed = random.Next(int.MinValue, int.MaxValue);
            GenArea.MainRoom.GridSize = new Vector3Int(GenTileRoomPlacer.GenTile.Width, 1, GenTileRoomPlacer.GenTile.Height);
            
            await GenArea.MainRoom.Generate();

            foreach (var room in GenTileRoomPlacer.PlacedRooms)
            {
                await BuildRoomFromTileRoom(GenArea, room, random);
            }
        }

        public async Awaitable BuildRoomFromTileRoom(GenArea area, GenTileRoom tileRoom, System.Random random)
        {
            GenRoom room = Instantiate(area.GenRoomPrefab.gameObject, area.MainRoom.Content).GetComponent<GenRoom>();

            room.RandomSeed = false;
            room.Seed = random.Next(int.MinValue, int.MaxValue);
            room.GridSize = new Vector3Int(tileRoom.Size.x, 1, tileRoom.Size.y);
            room.transform.localPosition = new Vector3(tileRoom.Position.x * room.TileSize.x, 0, tileRoom.Position.y * room.TileSize.z);
            room.transform.localRotation = Quaternion.identity;

            area.Rooms.Add(room);
            await room.Generate();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GenTileAreaBuilder))]
    public class GenTileRoomBuilder_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenTileAreaBuilder genTileAreaBuilder = (GenTileAreaBuilder) target;
            if (GUILayout.Button("Build")) genTileAreaBuilder.BuildRooms();
            if (GUILayout.Button("Clear")) genTileAreaBuilder.Clear();
        }
    }
#endif
}