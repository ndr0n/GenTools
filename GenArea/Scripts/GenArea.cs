using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GenTools
{
    [System.Serializable]
    public class GenArea : MonoBehaviour
    {
        public int Seed = 0;
        public bool RandomSeed = false;

        public Vector3Int Size = new Vector3Int(20, 1, 20);
        public Vector2Int Border = new Vector2Int(1, 1);

        public GenTile GenTile;
        public GenRoom GenRoomPrefab;
        public GenTileRoomType TileRoom;
        public GenRoomType MainRoomType;
        public GenRoomType InnerRoomType;
        public List<GenRoomPreset> TunnelPreset;

        [Header("Runtime")]
        public GenRoom MainRoom;
        public GenTunnel GenTunnel = new();
        public List<GenRoom> InnerRoom = new();

        System.Random random;

        public bool GenerateNewTile = true;

        public void Clear()
        {
            foreach (var room in InnerRoom)
            {
                if (room != null) DestroyImmediate(room.gameObject);
            }
            InnerRoom.Clear();
            if (GenTunnel != null) GenTunnel.Clear();
        }

        public async Awaitable Generate()
        {
            try
            {
                Clear();
                if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
                random = new(Seed);
                if (GenTile != null)
                {
                    GenTile.Width = (Size.x - Border.x);
                    GenTile.Height = (Size.z - Border.y);
                    await GenerateFromGenTile();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Awaitable GenerateFromGenTile()
        {
            if (GenerateNewTile)
            {
                // Generate Tile Data
                GenTile.RandomSeed = false;
                GenTile.Seed = random.Next(int.MinValue, int.MaxValue);
                GenTile.Generate();
            }

            if (MainRoomType != null)
            {
                GenTileRoom mainTileRoom = new(TileRoom, new Vector2Int(GenTile.Width, GenTile.Height), new Vector2Int(0, 0));
                for (int x = 0; x < GenTile.Width; x++)
                {
                    for (int y = 0; y < GenTile.Height; y++)
                    {
                        foreach (var tile in TileRoom.RoomTile)
                        {
                            if (GenTile.Tilemap[0].GetTile(new Vector3Int(x, y, 0)) == tile)
                            {
                                mainTileRoom.PlacedFloor.Add(new GenTileObject(tile, new Vector2Int(x, y)));
                                break;
                            }
                        }
                    }
                }
                MainRoom = await BuildRoomFromTileRoom(mainTileRoom, 0);
            }

            // Init Tunnel
            if (TunnelPreset.Count > 0)
            {
                GenRoomPreset tunnelPreset = TunnelPreset[random.Next(TunnelPreset.Count)];
                GenTunnel.Init(transform, tunnelPreset);
            }

            // Build Inner Rooms
            foreach (var tileRoom in GenTile.GenTileRoomPlacer.PlacedRooms)
            {
                GenRoom room = await BuildRoomFromTileRoom(tileRoom, 0);
                await GenTunnel.BuildTunnelsFromTileRoom(random, room, tileRoom, 0);
                await room.PlaceRoomObjects(random);
            }

            // Build Tunnel
            if (TunnelPreset.Count > 0)
            {
                GenTunnel.Build(random, GenTile);
                List<GameObject> roomWalls = new();
                foreach (var room in InnerRoom)
                {
                    foreach (var node in room.Node)
                    {
                        foreach (var wall in node.Wall)
                        {
                            if (wall != null) roomWalls.Add(wall);
                        }
                    }
                }
                GenTunnel.BuildTunnelWalls(roomWalls, random);
                await GenTunnel.PlaceTunnelObjects(random);
                GenTunnel.BuildTunnelPillars(random);
            }
        }

        public async Awaitable<GenRoom> BuildRoomFromTileRoom(GenTileRoom tileRoom, int y)
        {
            GenRoom room = Instantiate(GenRoomPrefab.gameObject, transform).GetComponent<GenRoom>();
            room.name = $"InnerRoom-{room.transform.parent.childCount}";
            room.Type = InnerRoomType;
            room.RandomSeed = false;
            room.Seed = random.Next(int.MinValue, int.MaxValue);
            room.GridSize = new Vector3Int(tileRoom.Size.x, Size.y, tileRoom.Size.y);
            room.transform.localPosition = new Vector3(tileRoom.Position.x * room.TileSize.x, y * room.TileSize.y, tileRoom.Position.y * room.TileSize.z);
            room.transform.localRotation = Quaternion.identity;
            room.transform.localPosition += new Vector3(Border.x * room.TileSize.x, 0, Border.y * room.TileSize.z);
            InnerRoom.Add(room);
            await room.Generate(tileRoom);
            return room;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GenArea))]
    public class GenArea_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenArea genTileAreaBuilder = (GenArea) target;
            if (GUILayout.Button("Generate")) genTileAreaBuilder.Generate();
            if (GUILayout.Button("Clear")) genTileAreaBuilder.Clear();
        }
    }
#endif
}