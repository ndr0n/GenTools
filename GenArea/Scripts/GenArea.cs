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
        public GenRoomType InnerRoomType;
        public List<GenRoomPreset> TunnelPreset;

        [Header("Runtime")]
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
                await BuildDoorsFromTileRoom(room, tileRoom, 0);
                await GenTunnel.BuildTunnelsFromTileRoom(random, room, tileRoom, 0);
                await BuildWallsFromTileRoom(room, tileRoom, Size.y);
            }

            // Build Tunnel
            if (TunnelPreset.Count > 0)
            {
                GenTunnel.Build(random, GenTile);
                GenTunnel.BuildTunnelWalls(random);
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

            await room.Generate();
            return room;
        }

        async Awaitable BuildWallsFromTileRoom(GenRoom room, GenTileRoom tileRoom, int height)
        {
            List<GenRoomNode> nodes = room.GetAllNodes();
            GameObject wallPreset = room.Preset.InnerWall[random.Next(0, room.Preset.InnerWall.Count)];
            List<Vector2Int> directions = new() {Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right};
            List<int> dirIter = new List<int> {0, 1, 2, 3};
            foreach (var wall in tileRoom.PlacedWalls)
            {
                Vector3Int pos = new Vector3Int(wall.Position.x, 0, wall.Position.y);
                GenRoomNode node = nodes.FirstOrDefault(x => x.Floor != null && x.Position == pos);
                if (node != null)
                {
                    dirIter = dirIter.OrderBy(x => random.Next()).ToList();
                    foreach (int i in dirIter)
                    {
                        if (node.Wall[i] == null)
                        {
                            for (int h = 0; h < height; h++)
                            {
                                GameObject spawn = Instantiate(wallPreset, room.Content);
                                spawn.transform.position = node.Floor.transform.position + new Vector3(directions[i].x * (room.Preset.TileSize.x / 2f), h * room.Preset.TileSize.y, directions[i].y * (room.Preset.TileSize.z / 2f));
                                spawn.transform.rotation = Quaternion.Euler(0, 90 * i, 0);
                                if (h == 0) node.Wall[i] = spawn;
                            }
                            await room.Await();
                            break;
                        }
                    }
                }
            }
        }

        async Awaitable BuildDoorsFromTileRoom(GenRoom room, GenTileRoom tileRoom, int y)
        {
            GameObject outerDoorPreset = room.Preset.OuterDoor[random.Next(0, room.Preset.OuterDoor.Count)];
            foreach (var door in tileRoom.PlacedDoors)
            {
                Vector3 doorPosition = new Vector3(door.Position.x * room.TileSize.x, y * room.TileSize.y, door.Position.y * room.TileSize.z);
                doorPosition += room.Content.localPosition;
                List<GenRoomNode> nodes = room.GetAllNodes();
                GenRoomNode node = nodes.FirstOrDefault(x => x.Floor.transform.position == doorPosition);
                if (node != null)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        GameObject wall = node.Wall[direction];
                        if (wall != null)
                        {
                            GameObject outerDoor = Instantiate(outerDoorPreset, room.Content);
                            outerDoor.transform.position = wall.transform.position;
                            outerDoor.transform.rotation = wall.transform.rotation;
                            room.OuterDoor.Add(outerDoor);
                            GenTunnel.TunnelDoors.Add(outerDoor);
                            node.Object = outerDoor;
                            DestroyImmediate(wall);
                            await room.Await();
                            break;
                        }
                    }
                }
            }
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