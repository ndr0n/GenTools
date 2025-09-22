using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modules.GenTools.GenArea.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GenTools
{
    [System.Serializable]
    public class GenRoom : MonoBehaviour
    {
        public Transform Content;
        public GenRoomType Type;
        public int GenerationDelayMilliseconds = 0;

        public bool RandomSeed = false;
        public int Seed = 0;
        public Vector3Int Size = new Vector3Int(5, 1, 5);
        public Vector3Int Position = new Vector3Int(0, 0, 0);

        public Vector3 TileSize = new Vector3(4, 4, 4);
        public Vector3 TileScale = new Vector3(1, 1, 1);
        public Vector2Int OuterDoorAmount = new Vector2Int(0, 0);

        public readonly List<GenRoomNode> Node = new();
        public readonly List<GameObject> OuterDoor = new();
        public readonly List<GameObject> InnerDoor = new();
        public readonly List<GameObject> Objects = new();

        GenRoomPreset preset = null;
        public GenRoomPreset Preset => preset;

        System.Random random = null;
        Color roomColor = Color.white;

        public void Clear()
        {
            if (Content != null)
            {
                for (int i = Content.childCount; i-- > 0;)
                {
                    DestroyImmediate(Content.GetChild(i).gameObject);
                }
            }

            if (Content == null)
            {
                Content = GenTools.CreateGameObject("Content", transform).transform;
                Content.transform.localPosition += new Vector3(TileSize.x / 2f, 0, TileSize.z / 2f);
            }
            OuterDoor.Clear();
            InnerDoor.Clear();

            foreach (var obj in Objects)
            {
                if (obj != null) DestroyImmediate(obj);
            }
            Objects.Clear();

            Node.Clear();
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    for (int z = 0; z < Size.z; z++)
                    {
                        Node.Add(new GenRoomNode(new Vector3Int(x, y, z)));
                    }
                }
            }
        }

        public async Awaitable Generate(GenTileRoom tileRoom, List<GameObject> existingWalls)
        {
            try
            {
                Clear();
                if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
                random = new System.Random(Seed);
                preset = Type.Presets[random.Next(Type.Presets.Count)];
                roomColor.r = random.Next(0, 100) / 100f;
                roomColor.g = random.Next(0, 100) / 100f;
                roomColor.b = random.Next(25, 100) / 100f;
                await BuildFloor(tileRoom);
                await BuildRoof(tileRoom);
                await BuildOuterWalls(existingWalls);
                await BuildDoors(tileRoom, 0);
                await BuildTunnels(tileRoom, 0);
                await BuildInnerWalls(tileRoom);
                await BuildBalconyFloor(tileRoom);
                await BuildBalconyWalls(existingWalls);
                await BuildBalconyStairs();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Awaitable BuildFloor(GenTileRoom tileRoom)
        {
            if (preset.Floor.Count > 0)
            {
                GameObject floorPreset = preset.Floor[random.Next(0, preset.Floor.Count)];
                foreach (var tileFloor in tileRoom.PlacedFloor)
                {
                    Vector3Int pos = new Vector3Int(tileFloor.Position.x, 0, tileFloor.Position.y);
                    GameObject floor = Instantiate(floorPreset, Content);
                    floor.transform.localPosition = new Vector3(pos.x * TileSize.x, pos.y * TileSize.y, pos.z * TileSize.z);
                    floor.transform.localRotation = Quaternion.identity;
                    GenRoomNode node = Node.FirstOrDefault(x => x.Position == pos);
                    node.Floor = floor;
                    await Await();
                }
            }
        }

        async Awaitable BuildRoof(GenTileRoom tileRoom)
        {
            if (preset.Roof.Count > 0)
            {
                GameObject roofPreset = preset.Roof[random.Next(0, preset.Roof.Count)];
                foreach (var tileFloor in tileRoom.PlacedFloor)
                {
                    Vector3Int pos = new Vector3Int(tileFloor.Position.x, Size.y, tileFloor.Position.y);
                    GameObject roof = Instantiate(roofPreset, Content);
                    roof.transform.localPosition = new Vector3(pos.x * TileSize.x, pos.y * TileSize.y, pos.z * TileSize.z);
                    roof.transform.localRotation = Quaternion.identity;
                    GenRoomNode roofNode = Node.FirstOrDefault(n => n.Position == new Vector3Int(pos.x, Size.y - 1, pos.z));
                    roofNode.Roof = roof;
                    await Await();
                }
            }
        }

        async Awaitable BuildOuterWalls(List<GameObject> existingWalls)
        {
            GameObject wallPreset = Preset.OuterWall[random.Next(Preset.OuterWall.Count)];
            List<GenRoomNode> nodes = Node.OrderBy(y => random.Next()).ToList();
            List<GenRoomNode> floorNodes = nodes.Where(x => x.Position.y == 0 && x.Floor != null).OrderBy(y => random.Next()).ToList();
            foreach (var node in floorNodes)
            {
                List<GenRoomNode> adjacentFloors = new() {null, null, null, null};
                foreach (var adj in floorNodes)
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

                List<Vector3> directions = new() {Vector3.back * Preset.TileSize.z, Vector3.left * Preset.TileSize.x, Vector3.forward * Preset.TileSize.z, Vector3.right * Preset.TileSize.x};
                for (int direction = 0; direction < 4; direction++)
                {
                    if (adjacentFloors[direction] == null)
                    {
                        for (int y = 0; y < Size.y; y++)
                        {
                            Vector3Int testPosition = new Vector3Int(node.Position.x, y, node.Position.z);
                            GenRoomNode iterNode = nodes.FirstOrDefault(n => n.Position == testPosition && n.Object == null);
                            if (iterNode == null) continue;
                            Vector3 position = new Vector3(iterNode.Position.x * Preset.TileSize.x, iterNode.Position.y * Preset.TileSize.y, iterNode.Position.z * Preset.TileSize.z);
                            Vector3 pos = position + (directions[direction] / 2f) + Content.transform.position;
                            if (iterNode.Wall[direction] != null) continue;
                            if (existingWalls.Exists(ew => ew.transform.position == pos)) continue;
                            GameObject wall = Instantiate(wallPreset, Content.transform);
                            wall.transform.position = pos;
                            wall.transform.localRotation = Quaternion.Euler(0, direction * 90, 0);
                            iterNode.Wall[direction] = wall;
                        }
                    }
                }
                await Await();
            }
        }

        // async Awaitable BuildOuterWalls()
        // {
        //     if (preset.OuterWall.Count > 0)
        //     {
        //         GameObject outerWallPreset = preset.OuterWall[random.Next(0, preset.OuterWall.Count)];
        //         for (int y = 0; y < GridSize.y; y++)
        //         {
        //             Vector3Int pos;
        //             for (int x = 0; x < GridSize.x; x++)
        //             {
        //                 // South
        //                 pos = new Vector3Int(x, y, 0);
        //                 GameObject southWall = Instantiate(outerWallPreset, Content);
        //                 southWall.transform.localPosition = new Vector3(pos.x * TileSize.x, pos.y * TileSize.y, pos.z * TileSize.z);
        //                 southWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.South * 90, 0);
        //                 southWall.transform.localPosition += new Vector3(0, 0, -TileSize.z / 2f);
        //                 GenRoomNode southNode = Node.FirstOrDefault(x => x.Position == pos);
        //                 southNode.Wall[(int) CardinalDirection.South] = southWall;
        //
        //                 // North
        //                 pos = new Vector3Int(x, y, GridSize.z - 1);
        //                 GameObject northWall = Instantiate(outerWallPreset, Content);
        //                 northWall.transform.localPosition = new Vector3(pos.x * TileSize.x, pos.y * TileSize.y, pos.z * TileSize.z);
        //                 northWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.North * 90, 0);
        //                 northWall.transform.localPosition += new Vector3(0, 0, TileSize.z / 2f);
        //                 GenRoomNode northNode = Node.FirstOrDefault(x => x.Position == pos);
        //                 northNode.Wall[(int) CardinalDirection.North] = northWall;
        //
        //                 await Await();
        //             }
        //
        //             for (int z = 0; z < GridSize.z; z++)
        //             {
        //                 // West
        //                 pos = new Vector3Int(0, y, z);
        //                 GameObject westWall = Instantiate(outerWallPreset, Content);
        //                 westWall.transform.localPosition = new Vector3(pos.x * TileSize.x, pos.y * TileSize.y, pos.z * TileSize.z);
        //                 westWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.West * 90, 0);
        //                 westWall.transform.localPosition += new Vector3(-TileSize.x / 2f, 0, 0);
        //                 GenRoomNode westNode = Node.FirstOrDefault(x => x.Position == pos);
        //                 westNode.Wall[(int) CardinalDirection.West] = westWall;
        //
        //                 // East
        //                 pos = new Vector3Int(GridSize.x - 1, y, z);
        //                 GameObject eastWall = Instantiate(outerWallPreset, Content);
        //                 eastWall.transform.localPosition = new Vector3(pos.x * TileSize.x, pos.y * TileSize.y, pos.z * TileSize.z);
        //                 eastWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.East * 90, 0);
        //                 eastWall.transform.localPosition += new Vector3(TileSize.x / 2f, 0, 0);
        //                 GenRoomNode eastNode = Node.FirstOrDefault(x => x.Position == pos);
        //                 eastNode.Wall[(int) CardinalDirection.East] = eastWall;
        //
        //                 await Await();
        //             }
        //         }
        //     }
        // }

        async Awaitable BuildDoors(GenTileRoom tileRoom, int y)
        {
            GameObject outerDoorPreset = Preset.OuterDoor[random.Next(0, Preset.OuterDoor.Count)];
            foreach (var door in tileRoom.PlacedDoors)
            {
                Vector3 doorPosition = new Vector3(door.Position.x * TileSize.x, y * TileSize.y, door.Position.y * TileSize.z);
                doorPosition += Content.localPosition;
                GenRoomNode node = Node.Where(x => x.Floor != null).OrderBy(x => random.Next()).FirstOrDefault(x => x.Floor.transform.position == doorPosition);
                if (node != null)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        GameObject wall = node.Wall[direction];
                        if (wall != null)
                        {
                            GameObject outerDoor = Instantiate(outerDoorPreset, Content);
                            outerDoor.transform.position = wall.transform.position;
                            outerDoor.transform.rotation = wall.transform.rotation;
                            OuterDoor.Add(outerDoor);
                            node.Object = outerDoor;
                            node.Wall[direction] = outerDoor;
                            DestroyImmediate(wall);
                            await Await();
                            break;
                        }
                    }
                }
            }
        }

        async Awaitable BuildInnerWalls(GenTileRoom tileRoom)
        {
            int h = 0;
            GameObject wallPreset = Preset.InnerWall[random.Next(0, Preset.InnerWall.Count)];
            List<Vector2Int> directions = new() {Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right};
            List<int> dirIter = new List<int> {0, 1, 2, 3};
            foreach (var wall in tileRoom.PlacedWalls)
            {
                Vector3Int pos = new Vector3Int(wall.Position.x, 0, wall.Position.y);
                GenRoomNode node = Node.FirstOrDefault(x => x.Floor != null && x.Position == pos && x.Object == null && x.Wall.Where(w => w != null).ToList().Count < 2);
                if (node != null)
                {
                    dirIter = dirIter.OrderBy(x => random.Next()).ToList();
                    foreach (int i in dirIter)
                    {
                        if (node.Wall[i] == null)
                        {
                            // for (int h = 0; h < height; h++)
                            // {
                            GameObject spawn = Instantiate(wallPreset, Content);
                            spawn.transform.position = node.Floor.transform.position + new Vector3(directions[i].x * (Preset.TileSize.x / 2f), h * Preset.TileSize.y, directions[i].y * (Preset.TileSize.z / 2f));
                            spawn.transform.rotation = Quaternion.Euler(0, 90 * i, 0);
                            node.Wall[i] = spawn;
                            // }
                            await Await();
                            break;
                        }
                    }
                }
            }
        }

        async Awaitable BuildBalconyFloor(GenTileRoom tileRoom)
        {
            GameObject floorPreset = Preset.Floor[random.Next(0, Preset.Floor.Count)];
            foreach (var balcony in tileRoom.PlacedBalcony)
            {
                Vector3Int pos = new Vector3Int(balcony.Position.x, 1, balcony.Position.y);
                GenRoomNode node = Node.FirstOrDefault(x => x.Floor == null && x.Position == pos);
                if (node != null)
                {
                    Vector3 worldPosition = new Vector3(pos.x * Preset.TileSize.x, pos.y * Preset.TileSize.y, pos.z * Preset.TileSize.z);
                    GameObject spawn = Instantiate(floorPreset, Content);
                    spawn.transform.localPosition = worldPosition;
                    spawn.transform.rotation = Quaternion.identity;
                    node.Floor = spawn;
                    await Await();
                }
            }
        }

        public async Awaitable BuildBalconyWalls(List<GameObject> existingWalls)
        {
            GameObject wallPreset = Preset.InnerWall[random.Next(Preset.InnerWall.Count)];
            GameObject railPreset = Preset.Rail[random.Next(0, Preset.Rail.Count)];
            List<GenRoomNode> nodes = Node.OrderBy(y => random.Next()).ToList();
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

                List<Vector3> directions = new() {Vector3.back * Preset.TileSize.z, Vector3.left * Preset.TileSize.x, Vector3.forward * Preset.TileSize.z, Vector3.right * Preset.TileSize.x};
                for (int direction = 0; direction < 4; direction++)
                {
                    if (adjacentFloors[direction] == null)
                    {
                        for (int y = 0; y < Size.y; y++)
                        {
                            Vector3Int testPosition = new Vector3Int(node.Position.x, y, node.Position.z);
                            GenRoomNode iterNode = nodes.FirstOrDefault(n => n.Position == testPosition && n.Object == null);
                            if (iterNode == null) continue;
                            Vector3 position = new Vector3(iterNode.Position.x * Preset.TileSize.x, iterNode.Position.y * Preset.TileSize.y, iterNode.Position.z * Preset.TileSize.z);
                            Vector3 pos = position + (directions[direction] / 2f) + Content.transform.position;
                            GameObject preset = wallPreset;
                            if (node.Wall[direction] != null) continue;
                            if (existingWalls.Exists(ew => ew.transform.position == pos)) continue;
                            if (y == 0)
                            {
                                if (CanBuildWall(iterNode) == false) continue;
                            }
                            else
                            {
                                if (CanBuildWall(iterNode) == false) preset = railPreset;
                            }
                            GameObject wall = Instantiate(preset, Content.transform);
                            wall.transform.position = pos;
                            wall.transform.localRotation = Quaternion.Euler(0, direction * 90, 0);
                            if (preset == wallPreset) iterNode.Wall[direction] = wall;
                        }
                    }
                }
                await Await();
            }
        }

        public async Awaitable BuildBalconyStairs()
        {
            int maxStairCount = random.Next(Type.StairCount.x, Type.StairCount.y + 1);
            GenRoomNode lastStairNode = null;
            GameObject stairPreset = Preset.Stair[random.Next(Preset.Stair.Count)];
            for (int stairCount = 0; stairCount < maxStairCount; stairCount++)
            {
                List<GenRoomNode> balconyNodes = Node.Where(x => x.Position.y > 0 && x.Floor != null && x.Object == null).ToList();
                if (stairCount % 2 == 0) balconyNodes = balconyNodes.OrderBy(x => random.Next()).ToList();
                else balconyNodes = balconyNodes.OrderByDescending(x => Vector3Int.Distance(x.Position, lastStairNode.Position)).ToList();

                foreach (var node in balconyNodes)
                {
                    GenRoomNode floorNode = Node.FirstOrDefault(x => x.Position == new Vector3Int(node.Position.x, 0, node.Position.z) && x.Object == null);
                    if (floorNode != null)
                    {
                        Vector3 pos = new Vector3(floorNode.Position.x * Preset.TileSize.x, floorNode.Position.y * Preset.TileSize.y, floorNode.Position.z * Preset.TileSize.z);
                        GameObject stair = Instantiate(stairPreset, Content);
                        stair.transform.localPosition = pos;
                        stair.transform.localRotation = Quaternion.Euler(0, (random.Next(0, 4)) * 90, 0);
                        floorNode.Object = stair;
                        node.Object = stair;
                        lastStairNode = node;
                        DestroyImmediate(node.Floor);
                        await Await();
                        break;
                    }
                }
            }
        }

        async Awaitable BuildTunnels(GenTileRoom tileRoom, int y)
        {
            foreach (var tunnel in tileRoom.PlacedTunnels)
            {
                GameObject roofPreset = null;
                if (Preset.Roof.Count > 0) roofPreset = Preset.Roof[random.Next(0, Preset.Roof.Count)];
                GameObject floorPreset = Preset.Floor[random.Next(0, Preset.Floor.Count)];
                foreach (var position in tunnel.Positions)
                {
                    Vector3Int nodePosition = new Vector3Int(position.x, 0, position.y);
                    GenRoomNode tunnelNode = Node.FirstOrDefault(x => x.Position == nodePosition);
                    if (tunnelNode == null)
                    {
                        tunnelNode = new GenRoomNode(nodePosition);
                        Node.Add(tunnelNode);
                        Vector3 pos = new Vector3(position.x * Preset.TileSize.x, 0, position.y * Preset.TileSize.z);
                        pos += new Vector3(Preset.TileSize.x / 2, 0, Preset.TileSize.z / 2);
                        // BuildTunnelFloorAndRoof(tunnelNode, floorPreset, roofPreset, pos, Preset.TileSize);
                        // Debug.Log($"TUNNEL FLOOR INIT COUNT: {Node.Count} | pos: {pos}");
                    }
                }

                // Build Tunnel Door
                GameObject outerDoorPreset = Preset.OuterDoor[random.Next(0, Preset.OuterDoor.Count)];
                CardinalDirection doorDirection = GenTools.GetDirection(tunnel.OriginPoint, tunnel.Positions[0]);
                Vector3 doorPosition = new Vector3(tunnel.OriginPoint.x * TileSize.x, y * TileSize.y, tunnel.OriginPoint.y * TileSize.z);
                doorPosition += Content.localPosition;
                GenRoomNode roomNode = Node.FirstOrDefault(x => x.Floor.transform.position == doorPosition);
                if (roomNode != null)
                {
                    GameObject wall = roomNode.Wall[(int) doorDirection];
                    if (wall != null)
                    {
                        GameObject outerDoor = Instantiate(outerDoorPreset, Content);
                        outerDoor.transform.position = wall.transform.position;
                        outerDoor.transform.rotation = wall.transform.rotation;
                        OuterDoor.Add(outerDoor);
                        // TunnelDoors.Add(outerDoor);
                        roomNode.Object = outerDoor;
                        DestroyImmediate(wall);
                        await Await();
                    }
                }
            }
        }

        public bool CanBuildWall(GenRoomNode node)
        {
            int count = 0;
            for (int d = 0; d < node.Wall.Count; d++)
            {
                if (node.Wall[d] != null) count++;
            }

            if (node.Position.x > 0)
            {
                GenRoomNode westNode = Node.FirstOrDefault(x => x.Position == new Vector3Int(node.Position.x - 1, node.Position.y, node.Position.z));
                if (westNode.Wall[(int) CardinalDirection.East] != null) count++;
            }

            if (node.Position.x < Size.x - 1)
            {
                GenRoomNode eastNode = Node.FirstOrDefault(x => x.Position == new Vector3Int(node.Position.x + 1, node.Position.y, node.Position.z));
                if (eastNode.Wall[(int) CardinalDirection.West] != null) count++;
            }

            if (node.Position.z > 0)
            {
                GenRoomNode southNode = Node.FirstOrDefault(x => x.Position == new Vector3Int(node.Position.x, node.Position.y, node.Position.z - 1));
                if (southNode.Wall[(int) CardinalDirection.North] != null) count++;
            }

            if (node.Position.z < Size.y - 1)
            {
                GenRoomNode northNode = Node.FirstOrDefault(x => x.Position == new Vector3Int(node.Position.x, node.Position.y, node.Position.z + 1));
                if (northNode.Wall[(int) CardinalDirection.South] != null) count++;
            }

            if (count >= 1) return false;

            return true;
        }

        public async Awaitable Await()
        {
            if (GenerationDelayMilliseconds > 0)
            {
                await Task.Delay(GenerationDelayMilliseconds);
            }
        }

        public async Awaitable PlaceRoomObjects(System.Random random)
        {
            List<GameObject> spawned = new();
            foreach (var lamp in Preset.Lamps)
            {
                List<GameObject> lamps = GenObjectLibrary.PlaceObject(Node, Content, Objects, lamp, random);
                spawned.AddRange(lamps);
            }

            foreach (var lamp in spawned)
            {
                foreach (var lght in lamp.GetComponentsInChildren<Light>(true))
                {
                    lght.color = roomColor;
                }
                if (Application.isPlaying)
                {
                    foreach (var rend in lamp.GetComponentsInChildren<Renderer>(true))
                    {
                        foreach (var mat in rend.materials)
                        {
                            mat.SetColor(GT.EmissionColorHash, roomColor);
                        }
                    }
                }
            }
            if (Application.isPlaying)
            {
                foreach (var wallNode in Node.Where(x => x.Wall.Exists(w => w != null)))
                {
                    foreach (var wall in wallNode.Wall)
                    {
                        if (wall != null)
                        {
                            foreach (var rend in wall.GetComponentsInChildren<Renderer>(true))
                            {
                                foreach (var mat in rend.materials)
                                {
                                    mat.SetColor(GT.EmissionColorHash, roomColor);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var obj in Preset.Object)
            {
                List<GameObject> objs = GenObjectLibrary.PlaceObject(Node, Content, Objects, obj, random);
                spawned.AddRange(objs);
                // if (spawn == null) return;
            }
        }
    }
}