using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Modules.GenTools.GenArea.Scripts;
using Pathfinding.Graphs.Grid.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenTunnel
    {
        public Transform Parent;
        public int Height = 2;
        public GenRoomPreset Preset;
        public List<GenRoomNode> Node = new();
        public List<GameObject> TunnelDoors = new();
        public List<GameObject> TunnelObjects = new();
        public int PillarChance = 10;
        public bool BuildPillarBalcony = false;

        public void Clear()
        {
            foreach (var node in Node)
            {
                if (node != null) node.Clear();
            }
            Node.Clear();
            foreach (var obj in TunnelObjects)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            TunnelObjects.Clear();
            foreach (var door in TunnelDoors)
            {
                if (door != null) Object.DestroyImmediate(door.gameObject);
            }
            TunnelDoors.Clear();
            if (Parent != null) Object.DestroyImmediate(Parent.gameObject);
        }

        public void Init(Transform parent, GenRoomPreset preset)
        {
            Parent = GenTools.CreateGameObject("Tunnel", parent).transform;
            Preset = preset;
        }

        public void Build(System.Random random, GenTile genTile)
        {
            // Build Tunnels
            GameObject roofPreset = null;
            if (Preset.Roof.Count > 0) roofPreset = Preset.Roof[random.Next(0, Preset.Roof.Count)];
            GameObject floorPreset = Preset.Floor[random.Next(0, Preset.Floor.Count)];
            foreach (var position in genTile.GenTileRoomPlacer.PlacedTunnels)
            {
                Vector3Int nodePosition = new Vector3Int(position.x, 0, position.y);
                GenRoomNode node = Node.FirstOrDefault(x => x.Position == nodePosition);
                if (node == null)
                {
                    node = new GenRoomNode(nodePosition);
                    Node.Add(node);
                    Vector3 pos = new Vector3(position.x * Preset.TileSize.x, 0, position.y * Preset.TileSize.z);
                    pos += new Vector3(Preset.TileSize.x / 2, 0, Preset.TileSize.z / 2);
                    BuildTunnelFloorAndRoof(node, floorPreset, roofPreset, pos, Preset.TileSize);
                    // Debug.Log($"TUNNEL FLOOR INIT COUNT: {Node.Count} | pos: {pos}");
                }
            }
        }

        void BuildTunnelFloorAndRoof(GenRoomNode node, GameObject floorPreset, GameObject roofPreset, Vector3 position, Vector3 size)
        {
            // Build Floor
            GameObject floor = Object.Instantiate(floorPreset, Parent.transform);
            floor.transform.position = position;
            floor.transform.rotation = Quaternion.identity;
            node.Floor = floor;

            if (roofPreset != null)
            {
                // Build Roof
                GameObject roof = Object.Instantiate(roofPreset, Parent.transform);
                roof.transform.position = floor.transform.position + new Vector3(0, size.y * (Height), 0);
                roof.transform.rotation = Quaternion.identity;
                node.Roof = roof;
            }
        }

        public async Awaitable BuildTunnelsFromTileRoom(System.Random random, GenRoom room, GenTileRoom tileRoom, int y)
        {
            foreach (var tunnel in tileRoom.PlacedTunnels)
            {
                GameObject roofPreset = null;
                if (room.Preset.Roof.Count > 0) roofPreset = room.Preset.Roof[random.Next(0, room.Preset.Roof.Count)];
                GameObject floorPreset = room.Preset.Floor[random.Next(0, room.Preset.Floor.Count)];
                foreach (var position in tunnel.Positions)
                {
                    Vector3Int nodePosition = new Vector3Int(position.x, 0, position.y);
                    GenRoomNode tunnelNode = Node.FirstOrDefault(x => x.Position == nodePosition);
                    if (tunnelNode == null)
                    {
                        tunnelNode = new GenRoomNode(nodePosition);
                        Node.Add(tunnelNode);
                        Vector3 pos = new Vector3(position.x * room.Preset.TileSize.x, 0, position.y * room.Preset.TileSize.z);
                        pos += new Vector3(room.Preset.TileSize.x / 2, 0, room.Preset.TileSize.z / 2);
                        BuildTunnelFloorAndRoof(tunnelNode, floorPreset, roofPreset, pos, room.Preset.TileSize);
                        // Debug.Log($"TUNNEL FLOOR INIT COUNT: {Node.Count} | pos: {pos}");
                    }
                }

                // Build Tunnel Door
                GameObject outerDoorPreset = room.Preset.OuterDoor[random.Next(0, room.Preset.OuterDoor.Count)];
                CardinalDirection doorDirection = GenTools.GetDirection(tunnel.OriginPoint, tunnel.Positions[0]);
                Vector3 doorPosition = new Vector3(tunnel.OriginPoint.x * room.TileSize.x, y * room.TileSize.y, tunnel.OriginPoint.y * room.TileSize.z);
                doorPosition += room.Content.localPosition;
                List<GenRoomNode> roomNodes = room.GetAllNodes();
                GenRoomNode roomNode = roomNodes.FirstOrDefault(x => x.Floor.transform.position == doorPosition);
                if (roomNode != null)
                {
                    GameObject wall = roomNode.Wall[(int) doorDirection];
                    if (wall != null)
                    {
                        GameObject outerDoor = Object.Instantiate(outerDoorPreset, room.Content);
                        outerDoor.transform.position = wall.transform.position;
                        outerDoor.transform.rotation = wall.transform.rotation;
                        room.OuterDoor.Add(outerDoor);
                        TunnelDoors.Add(outerDoor);
                        roomNode.Object = outerDoor;
                        Object.DestroyImmediate(wall);
                        await room.Await();
                    }
                }
            }
        }

        public void BuildTunnelWalls(System.Random random)
        {
            GameObject wallPreset = Preset.OuterWall[random.Next(Preset.OuterWall.Count)];
            foreach (var node in Node)
            {
                List<GenRoomNode> adjacentFloors = new() {null, null, null, null};
                foreach (var adj in Node)
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

                Vector3 position = new Vector3(node.Position.x * Preset.TileSize.x, node.Position.y * Preset.TileSize.y, node.Position.z * Preset.TileSize.z);
                position += new Vector3(Preset.TileSize.x / 2f, 0, Preset.TileSize.z / 2f);
                if (adjacentFloors[(int) CardinalDirection.North] == null)
                {
                    Vector3 pos = position + new Vector3(0, 0, Preset.TileSize.z / 2f);
                    if (!TunnelDoors.Exists(x => x.transform.position == pos))
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject wall = Object.Instantiate(wallPreset, Parent.transform);
                            wall.transform.position = pos + (new Vector3(0, Preset.TileSize.y * y, 0));
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.North * 90, 0);
                            node.Wall[(int) CardinalDirection.North] = wall;
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.South] == null)
                {
                    Vector3 pos = position + new Vector3(0, 0, -Preset.TileSize.z / 2f);
                    if (!TunnelDoors.Exists(x => x.transform.position == pos))
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject wall = Object.Instantiate(wallPreset, Parent.transform);
                            wall.transform.position = pos + (new Vector3(0, Preset.TileSize.y * y, 0));
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.South * 90, 0);
                            node.Wall[(int) CardinalDirection.South] = wall;
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.East] == null)
                {
                    Vector3 pos = position + new Vector3(Preset.TileSize.x / 2f, 0, 0);
                    if (!TunnelDoors.Exists(x => x.transform.position == pos))
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject wall = Object.Instantiate(wallPreset, Parent.transform);
                            wall.transform.position = pos + (new Vector3(0, Preset.TileSize.y * y, 0));
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.East * 90, 0);
                            node.Wall[(int) CardinalDirection.East] = wall;
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.West] == null)
                {
                    Vector3 pos = position + new Vector3(-Preset.TileSize.x / 2f, 0, 0);
                    if (!TunnelDoors.Exists(x => x.transform.position == pos))
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject wall = Object.Instantiate(wallPreset, Parent.transform);
                            wall.transform.position = pos + (new Vector3(0, Preset.TileSize.y * y, 0));
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.West * 90, 0);
                            node.Wall[(int) CardinalDirection.West] = wall;
                        }
                    }
                }
            }
        }

        public void BuildTunnelPillars(System.Random random)
        {
            GameObject pillarPreset = null;
            if (Preset.Pillar.Count > 0) pillarPreset = Preset.Pillar[random.Next(0, Preset.Pillar.Count)];
            if (pillarPreset != null)
            {
                List<GenRoomNode> pillars = new();
                foreach (var node in Node)
                {
                    if (random.Next(0, 100) < PillarChance)
                    {
                        if (node.Object == null && !node.Wall.Exists(wall => wall != null))
                        {
                            // Quaternion rotation = Quaternion.Euler(0, random.Next(0, 4) * 90, 0);
                            for (int y = 0; y < Height; y++)
                            {
                                GameObject pillar = Object.Instantiate(pillarPreset, Parent.transform);
                                pillar.transform.position = node.Floor.transform.position + new Vector3(Preset.TileSize.x / 2f, 0, Preset.TileSize.z / 2f) + new Vector3(0, y * Preset.TileSize.y, 0);
                                // pillar.transform.rotation = rotation;
                                node.Object = pillar;
                                pillars.Add(node);
                            }
                        }
                    }
                }

                if (BuildPillarBalcony == false) return;

                // Build Balcony
                if (pillars.Count == 0) return;
                pillars = pillars.OrderBy(x => Vector3.Distance(x.Floor.transform.position, pillars[0].Floor.transform.position)).ToList();
                GameObject floorPreset = Preset.Floor[random.Next(0, Preset.Floor.Count)];
                for (int i = 0; i < pillars.Count; i++)
                {
                    Vector3Int nodePosition = pillars[i].Position + new Vector3Int(0, 1, 0);
                    GenRoomNode node = Node.FirstOrDefault(x => x.Position == nodePosition);
                    if (node == null)
                    {
                        node = new(nodePosition);
                        Node.Add(node);
                        GameObject floor = Object.Instantiate(floorPreset, Parent.transform);
                        floor.transform.position = new Vector3(node.Position.x * Preset.TileSize.x, node.Position.y * Preset.TileSize.y, node.Position.z * Preset.TileSize.z) + new Vector3(Preset.TileSize.x / 2f, 0, Preset.TileSize.z / 2f);
                        floor.transform.rotation = Quaternion.identity;
                        node.Floor = floor;
                    }
                    GenRoomNode target = pillars.Where(x => x != pillars[i]).OrderBy(x => Vector3.Distance(x.Floor.transform.position, pillars[i].Floor.transform.position)).FirstOrDefault();
                    if (target != null)
                    {
                        Vector2Int startingPosition = new Vector2Int(pillars[i].Position.x, pillars[i].Position.z);
                        Vector2Int finalPosition = new Vector2Int(target.Position.x, target.Position.z);
                        List<Vector2Int> targetPositions = BresenhamLine.ComputeNoDiagonal(startingPosition, finalPosition);
                        foreach (var pos in targetPositions)
                        {
                            GenRoomNode checkNode = Node.FirstOrDefault(x => x.Position == new Vector3Int(pos.x, 0, pos.y));
                            if (checkNode == null) break;
                            nodePosition = new Vector3Int(pos.x, node.Position.y, pos.y);
                            node = Node.FirstOrDefault(x => x.Position == nodePosition);
                            if (node == null)
                            {
                                node = new(nodePosition);
                                Node.Add(node);
                                GameObject floor = Object.Instantiate(floorPreset, Parent.transform);
                                floor.transform.position = new Vector3(node.Position.x * Preset.TileSize.x, node.Position.y * Preset.TileSize.y, node.Position.z * Preset.TileSize.z) + new Vector3(Preset.TileSize.x / 2f, 0, Preset.TileSize.z / 2f);
                                floor.transform.rotation = Quaternion.identity;
                                node.Floor = floor;
                            }
                        }
                    }
                }
            }
        }

        public async Awaitable PlaceTunnelObjects(System.Random random)
        {
            foreach (var obj in Preset.Object)
            {
                List<GameObject> spawn = GenObjectLibrary.PlaceObject(Node, Parent, TunnelObjects, obj, random);
                // if (spawn == null) return;
            }
        }
    }
}