using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenTunnelNode
    {
        public Vector3 Position = Vector3.zero;
        public GameObject Floor = null;
        public GameObject Roof = null;
        public GameObject Object = null;
        public List<GameObject> Wall = new() {null, null, null, null};

        public GenTunnelNode(Vector3 position)
        {
            Position = position;
            Floor = null;
            Roof = null;
            Object = null;
            Wall = new() {null, null, null, null};
        }

        public void Clear()
        {
            Position = Vector3.zero;
            if (Floor != null) UnityEngine.Object.DestroyImmediate(Floor);
            if (Roof != null) UnityEngine.Object.DestroyImmediate(Floor);
            if (Object != null) UnityEngine.Object.DestroyImmediate(Floor);
            foreach (var w in Wall)
            {
                if (w != null) UnityEngine.Object.DestroyImmediate(w);
            }
        }
    }

    [System.Serializable]
    public class GenTunnel
    {
        public Transform Parent;
        public int Height = 2;
        public GenRoomPreset Preset;
        public List<GenTunnelNode> Node = new();
        public List<GameObject> TunnelDoors = new();

        public void Clear()
        {
            foreach (var node in Node)
            {
                if (node != null) node.Clear();
            }
            foreach (var door in TunnelDoors)
            {
                if (door != null) Object.DestroyImmediate(door.gameObject);
            }
            if (Parent != null) Object.DestroyImmediate(Parent.gameObject);
            Node.Clear();
            TunnelDoors.Clear();
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
            GameObject pillarPreset = null;
            if (Preset.Pillar.Count > 0) pillarPreset = Preset.Pillar[random.Next(0, Preset.Pillar.Count)];
            GameObject floorPreset = Preset.Floor[random.Next(0, Preset.Floor.Count)];
            foreach (var position in genTile.GenTileRoomPlacer.PlacedTunnels)
            {
                Vector3 pos = new Vector3(position.x * Preset.TileSize.x, 0, position.y * Preset.TileSize.z);
                pos += new Vector3(Preset.TileSize.x / 2, 0, Preset.TileSize.z / 2);
                GenTunnelNode node = Node.FirstOrDefault(x => x.Position == pos);
                if (node == null)
                {
                    node = new GenTunnelNode(pos);
                    Node.Add(node);
                    BuildTunnelFloorAndRoof(random, node, floorPreset, roofPreset, pillarPreset, pos, Preset.TileSize);
                    Debug.Log($"TUNNEL FLOOR INIT COUNT: {Node.Count} | pos: {pos}");
                }
            }
        }

        public int PillarChance = 10;

        void BuildTunnelFloorAndRoof(System.Random random, GenTunnelNode node, GameObject floorPreset, GameObject roofPreset, GameObject pillarPreset, Vector3 position, Vector3 size)
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

            if (pillarPreset != null)
            {
                if (node.Object == null)
                {
                    // Try Build Pillar
                    if (random.Next(0, 100) < PillarChance)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject pillar = Object.Instantiate(pillarPreset, Parent.transform);
                            pillar.transform.position = floor.transform.position + new Vector3(size.x / 2f, 0, size.z / 2f) + new Vector3(0, y * size.y, 0);
                            pillar.transform.rotation = Quaternion.identity;
                            node.Object = pillar;
                        }
                    }
                }
            }
        }

        public async Awaitable BuildTunnelsFromTileRoom(System.Random random, GenRoom room, GenTileRoom tileRoom, int y)
        {
            foreach (var tunnel in tileRoom.PlacedTunnels)
            {
                GameObject roofPreset = null;
                if (room.Preset.Roof.Count > 0) roofPreset = room.Preset.Roof[random.Next(0, room.Preset.Roof.Count)];
                GameObject pillarPreset = null;
                if (room.Preset.Pillar.Count > 0) pillarPreset = room.Preset.Pillar[random.Next(0, room.Preset.Pillar.Count)];
                GameObject floorPreset = room.Preset.Floor[random.Next(0, room.Preset.Floor.Count)];
                foreach (var position in tunnel.Positions)
                {
                    Vector3 pos = new Vector3(position.x * room.Preset.TileSize.x, 0, position.y * room.Preset.TileSize.z);
                    pos += new Vector3(room.Preset.TileSize.x / 2, 0, room.Preset.TileSize.z / 2);
                    GenTunnelNode tunnelNode = Node.FirstOrDefault(x => x.Position == pos);
                    if (tunnelNode == null)
                    {
                        tunnelNode = new GenTunnelNode(pos);
                        Node.Add(tunnelNode);
                        BuildTunnelFloorAndRoof(random, tunnelNode, floorPreset, roofPreset, pillarPreset, pos, room.Preset.TileSize);
                        Debug.Log($"TUNNEL FLOOR INIT COUNT: {Node.Count} | pos: {pos}");
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
                Vector3 pos = node.Position;
                List<GenTunnelNode> adjacentFloors = new() {null, null, null, null};
                foreach (var adj in Node)
                {
                    if (adj.Position == new Vector3(pos.x, pos.y, pos.z + Preset.TileSize.z))
                    {
                        adjacentFloors[(int) CardinalDirection.North] = adj;
                    }
                    else if (adj.Position == new Vector3(pos.x, pos.y, pos.z - Preset.TileSize.z))
                    {
                        adjacentFloors[(int) CardinalDirection.South] = adj;
                    }
                    else if (adj.Position == new Vector3(pos.x - Preset.TileSize.x, pos.y, pos.z))
                    {
                        adjacentFloors[(int) CardinalDirection.West] = adj;
                    }
                    else if (adj.Position == new Vector3(pos.x + Preset.TileSize.x, pos.y, pos.z))
                    {
                        adjacentFloors[(int) CardinalDirection.East] = adj;
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.North] == null)
                {
                    Vector3 position = node.Position + new Vector3(0, 0, Preset.TileSize.z / 2f);
                    if (!TunnelDoors.Exists(x => x.transform.position == position))
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject wall = Object.Instantiate(wallPreset, Parent.transform);
                            wall.transform.position = position + (new Vector3(0, Preset.TileSize.y, 0) * y);
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.North * 90, 0);
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.South] == null)
                {
                    Vector3 position = node.Position + new Vector3(0, 0, -Preset.TileSize.z / 2f);
                    if (!TunnelDoors.Exists(x => x.transform.position == position))
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject wall = Object.Instantiate(wallPreset, Parent.transform);
                            wall.transform.position = position + (new Vector3(0, Preset.TileSize.y, 0) * y);
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.South * 90, 0);
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.East] == null)
                {
                    Vector3 position = node.Position + new Vector3(Preset.TileSize.x / 2f, 0, 0);
                    if (!TunnelDoors.Exists(x => x.transform.position == position))
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject wall = Object.Instantiate(wallPreset, Parent.transform);
                            wall.transform.position = position + (new Vector3(0, Preset.TileSize.y, 0) * y);
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.East * 90, 0);
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.West] == null)
                {
                    Vector3 position = node.Position + new Vector3(-Preset.TileSize.x / 2f, 0, 0);
                    if (!TunnelDoors.Exists(x => x.transform.position == position))
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            GameObject wall = Object.Instantiate(wallPreset, Parent.transform);
                            wall.transform.position = position + (new Vector3(0, Preset.TileSize.y, 0) * y);
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.West * 90, 0);
                        }
                    }
                }
            }
        }
    }
}