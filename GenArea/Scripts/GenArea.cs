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
                // await BuildWallsFromTileRoom(room, tileRoom, 1);
                await BuildBalconyFromTileRoom(room, tileRoom);
                await BuildBalconyWallsFromTileRoom(room, tileRoom);
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
            int h = 0;
            List<GenRoomNode> nodes = room.GetAllNodes();
            GameObject wallPreset = room.Preset.InnerWall[random.Next(0, room.Preset.InnerWall.Count)];
            List<Vector2Int> directions = new() {Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right};
            List<int> dirIter = new List<int> {0, 1, 2, 3};
            foreach (var wall in tileRoom.PlacedWalls)
            {
                Vector3Int pos = new Vector3Int(wall.Position.x, 0, wall.Position.y);
                GenRoomNode node = nodes.FirstOrDefault(x => x.Floor != null && x.Position == pos && x.Object == null && x.Wall.Where(w => w != null).ToList().Count < 2);
                if (node != null)
                {
                    dirIter = dirIter.OrderBy(x => random.Next()).ToList();
                    foreach (int i in dirIter)
                    {
                        if (node.Wall[i] == null)
                        {
                            // for (int h = 0; h < height; h++)
                            // {
                            GameObject spawn = Instantiate(wallPreset, room.Content);
                            spawn.transform.position = node.Floor.transform.position + new Vector3(directions[i].x * (room.Preset.TileSize.x / 2f), h * room.Preset.TileSize.y, directions[i].y * (room.Preset.TileSize.z / 2f));
                            spawn.transform.rotation = Quaternion.Euler(0, 90 * i, 0);
                            node.Wall[i] = spawn;
                            // }
                            await room.Await();
                            break;
                        }
                    }
                }
            }
        }

        async Awaitable BuildBalconyFromTileRoom(GenRoom room, GenTileRoom tileRoom)
        {
            List<GenRoomNode> nodes = room.GetAllNodes();
            GameObject floorPreset = room.Preset.Floor[random.Next(0, room.Preset.Floor.Count)];
            foreach (var balcony in tileRoom.PlacedBalcony)
            {
                Vector3Int pos = new Vector3Int(balcony.Position.x, 1, balcony.Position.y);
                GenRoomNode node = nodes.FirstOrDefault(x => x.Floor == null && x.Position == pos);
                if (node != null)
                {
                    Vector3 worldPosition = new Vector3(pos.x * room.Preset.TileSize.x, pos.y * room.Preset.TileSize.y, pos.z * room.Preset.TileSize.z);
                    GameObject spawn = Instantiate(floorPreset, room.Content);
                    spawn.transform.localPosition = worldPosition;
                    spawn.transform.rotation = Quaternion.identity;
                    node.Floor = spawn;
                    await room.Await();
                }
            }
        }

        public async Awaitable BuildBalconyWallsFromTileRoom(GenRoom room, GenTileRoom tileRoom)
        {
            GameObject wallPreset = room.Preset.InnerWall[random.Next(room.Preset.InnerWall.Count)];
            GameObject railPreset = room.Preset.Rail[random.Next(0, room.Preset.Rail.Count)];
            List<GenRoomNode> nodes = room.GetAllNodes().OrderBy(y => random.Next()).ToList();
            List<GenRoomNode> balconyNodes = nodes.Where(x => x.Position.y > 0 && x.Floor != null).OrderBy(y => random.Next()).ToList();
            foreach (var node in balconyNodes)
            {
                List<GenRoomNode> adjacentFloors = new() {null, null, null, null};
                foreach (var adj in balconyNodes)
                {
                    if (adj.Position == new Vector3(node.Position.x, node.Position.y, node.Position.z + 1))
                    {
                        adjacentFloors[(int) CardinalDirection.North] = adj;
                    }
                    else if (adj.Position == new Vector3(node.Position.x, node.Position.y, node.Position.z - 1))
                    {
                        adjacentFloors[(int) CardinalDirection.South] = adj;
                    }
                    else if (adj.Position == new Vector3(node.Position.x - 1, node.Position.y, node.Position.z))
                    {
                        adjacentFloors[(int) CardinalDirection.West] = adj;
                    }
                    else if (adj.Position == new Vector3(node.Position.x + 1, node.Position.y, node.Position.z))
                    {
                        adjacentFloors[(int) CardinalDirection.East] = adj;
                    }
                }

                List<Vector3> directions = new() {Vector3.back * room.Preset.TileSize.z, Vector3.left * room.Preset.TileSize.x, Vector3.forward * room.Preset.TileSize.z, Vector3.right * room.Preset.TileSize.x};
                for (int direction = 0; direction < 4; direction++)
                {
                    if (adjacentFloors[direction] == null)
                    {
                        for (int y = 0; y < Size.y; y++)
                        {
                            Vector3Int testPosition = new Vector3Int(node.Position.x, y, node.Position.z);
                            GenRoomNode iterNode = nodes.FirstOrDefault(n => n.Position == testPosition && n.Object == null);
                            if (iterNode == null) continue;
                            Vector3 position = new Vector3(iterNode.Position.x * room.Preset.TileSize.x, iterNode.Position.y * room.Preset.TileSize.y, iterNode.Position.z * room.Preset.TileSize.z);
                            Vector3 pos = position + (directions[direction] / 2f);
                            GameObject preset = wallPreset;
                            if (y == 0)
                            {
                                if (CanBuildWall(room, iterNode, direction) == false) continue;
                            }
                            else
                            {
                                if (CanBuildWall(room, iterNode, direction) == false) preset = railPreset;
                            }
                            GameObject wall = Instantiate(preset, room.Content.transform);
                            wall.transform.localPosition = pos;
                            wall.transform.localRotation = Quaternion.Euler(0, direction * 90, 0);
                            iterNode.Wall[direction] = wall;
                        }
                    }
                }
                await room.Await();
            }
        }

        public bool CanBuildWall(GenRoom room, GenRoomNode node, int direction)
        {
            if (node.Wall[direction] != null) return false;
            int count = 0;
            for (int d = 0; d < node.Wall.Count; d++)
            {
                if (node.Wall[d] != null) count++;
            }

            if (node.Position.x > 0)
            {
                GenRoomNode westNode = room.Node[node.Position.y][node.Position.x - 1][node.Position.z];
                if (westNode.Wall[(int) CardinalDirection.East] != null) count++;
            }

            if (node.Position.x < room.Node[0].Count - 1)
            {
                GenRoomNode eastNode = room.Node[node.Position.y][node.Position.x + 1][node.Position.z];
                if (eastNode.Wall[(int) CardinalDirection.West] != null) count++;
            }

            if (node.Position.z > 0)
            {
                GenRoomNode southNode = room.Node[node.Position.y][node.Position.x][node.Position.z - 1];
                if (southNode.Wall[(int) CardinalDirection.North] != null) count++;
            }

            if (node.Position.z < room.Node[0][0].Count - 1)
            {
                GenRoomNode northNode = room.Node[node.Position.y][node.Position.x][node.Position.z + 1];
                if (northNode.Wall[(int) CardinalDirection.South] != null) count++;
            }

            if (count >= 1) return false;

            return true;
        }

        async Awaitable BuildDoorsFromTileRoom(GenRoom room, GenTileRoom tileRoom, int y)
        {
            List<GenRoomNode> nodes = room.GetAllNodes();
            GameObject outerDoorPreset = room.Preset.OuterDoor[random.Next(0, room.Preset.OuterDoor.Count)];
            foreach (var door in tileRoom.PlacedDoors)
            {
                Vector3 doorPosition = new Vector3(door.Position.x * room.TileSize.x, y * room.TileSize.y, door.Position.y * room.TileSize.z);
                doorPosition += room.Content.localPosition;
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
                            node.Wall[direction] = outerDoor;
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