using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    public class GenTileRoomPlacer : MonoBehaviour
    {
        public GenTile GenTile;

        public int Seed = 0;
        public bool RandomizeSeed = false;

        public List<GenTileRoomType> RoomType = new();
        public List<GenTileRoom> PlacedRooms = new();

        System.Random random = new();

        public void Clear()
        {
            PlacedRooms.Clear();
        }

        public void Generate()
        {
            Clear();
            if (RandomizeSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
            random = new(Seed);
            PlacedRooms = CreateRooms();
            PopulateRooms(PlacedRooms);
        }

        void PopulateRooms(List<GenTileRoom> rooms)
        {
            foreach (var room in rooms)
            {
                List<Vector3Int> availablePositions = new();
                for (int x = 0; x < room.Size.x; x++)
                {
                    for (int y = 0; y < room.Size.y; y++)
                    {
                        availablePositions.Add(new Vector3Int(x + room.Position.x, y + room.Position.y, 0));
                    }
                }
                availablePositions = room.ReplaceFloor(GenTile, availablePositions, random);
                availablePositions = room.PlaceWallsAndDoors(GenTile, availablePositions, random);
                // availablePositions = room.PlaceStairs();
                availablePositions = room.PlaceObjects(GenTile, availablePositions, random);
            }

            foreach (var tilemap in GenTile.Tilemap) tilemap.RefreshAllTiles();
        }

        GenTileRoom TryCreateRoom(Tilemap tilemap, Vector3Int pos, GenTileRoomType roomType)
        {
            foreach (var roomTile in roomType.RoomTile)
            {
                if (tilemap.GetTile(pos) == roomTile)
                {
                    Vector2Int min = new Vector2Int(pos.x, pos.y);
                    Vector2Int max = new Vector2Int(pos.x, pos.y);
                    List<Vector3Int> roomPositions = new();
                    roomPositions.Add(pos);

                    for (int x = pos.x; x < GenTile.Width; x++)
                    {
                        if (tilemap.GetTile(new Vector3Int(x, pos.y, 0)) == roomTile)
                        {
                            roomPositions.Add(pos);
                            if (x > max.x) max.x = x;
                        }
                        else break;
                    }
                    for (int x = pos.x; x >= 0; x--)
                    {
                        if (tilemap.GetTile(new Vector3Int(x, pos.y, 0)) == roomTile)
                        {
                            roomPositions.Add(pos);
                            if (x < min.x) min.x = x;
                        }
                        else break;
                    }
                    for (int y = pos.y; y < GenTile.Height; y++)
                    {
                        if (tilemap.GetTile(new Vector3Int(pos.x, y, 0)) == roomTile)
                        {
                            roomPositions.Add(pos);
                            if (y > max.y) max.y = y;
                        }
                        else break;
                    }
                    for (int y = pos.y; y >= 0; y--)
                    {
                        if (tilemap.GetTile(new Vector3Int(pos.x, y, 0)) == roomTile)
                        {
                            roomPositions.Add(pos);
                            if (y < min.y) min.y = y;
                        }
                        else break;
                    }

                    if (min.x != max.x && min.y != max.y)
                    {
                        Vector2Int rposition = min;
                        Vector2Int rsize = new Vector2Int(max.x - min.x + 1, max.y - min.y + 1);
                        if (rsize.x >= roomType.MinSize.x && rsize.x <= roomType.MaxSize.x && rsize.y >= roomType.MinSize.y && rsize.y <= roomType.MaxSize.y)
                        {
                            GenTileRoom room = new GenTileRoom(roomType, rsize, rposition);
                            return room;
                        }
                    }
                }
            }
            return null;
        }

        List<GenTileRoom> CreateRooms()
        {
            PlacedRooms.Clear();
            for (int x = 0; x < GenTile.Width; x++)
            {
                for (int y = 0; y < GenTile.Height; y++)
                {
                    bool positionAvailable = true;
                    foreach (var room in PlacedRooms)
                    {
                        if (x >= room.Position.x && x <= (room.Position.x + room.Size.x) && y >= room.Position.y && y <= (room.Position.y + room.Size.y))
                        {
                            positionAvailable = false;
                            break;
                        }
                    }
                    if (positionAvailable)
                    {
                        bool breakLoop = false;
                        for (int i = 0; i < GenTile.Tilemap.Count; i++)
                        {
                            foreach (var roomType in RoomType)
                            {
                                Vector3Int pos = new Vector3Int(x, y, 0);
                                TryCreateRoom(GenTile.Tilemap[i], pos, roomType);
                                GenTileRoom room = TryCreateRoom(GenTile.Tilemap[i], pos, roomType);
                                if (room != null)
                                {
                                    PlacedRooms.Add(room);
                                    breakLoop = true;
                                    break;
                                }
                            }
                            if (breakLoop) break;
                        }
                    }
                }
            }
            return PlacedRooms;
        }
    }
//
// #if UNITY_EDITOR
//     [CustomEditor(typeof(TileRoomPlacer))]
//     public class Tileroom_Editor : Editor
//     {
//         public override void OnInspectorGUI()
//         {
//             base.OnInspectorGUI();
//             TileRoomPlacer tileRoomPlacer = (TileRoomPlacer) target;
//             if (GUILayout.Button("Generate")) tileRoomPlacer.Generate();
//             if (GUILayout.Button("Clear")) tileRoomPlacer.Clear();
//         }
//     }
// #endif
//     
}