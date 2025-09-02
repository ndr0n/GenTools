using System.Collections.Generic;
using System.Linq;
using Scaerth;
using UnityEngine;

namespace GenTools
{
    public static class GenRoomLibrary
    {
        public static async Awaitable<List<GenRoomNode>> BuildFloor(GenRoom room, System.Random random, GenRoomPreset preset)
        {
            List<GenRoomNode> built = new();
            if (preset.Floor.Count > 0)
            {
                GameObject floorPreset = preset.Floor[random.Next(0, preset.Floor.Count)];
                for (int x = 0; x < room.GridSize.x; x++)
                {
                    for (int z = 0; z < room.GridSize.z; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, 0, z);
                        GameObject floor = Object.Instantiate(floorPreset, room.Content);
                        floor.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                        floor.transform.localRotation = Quaternion.identity;
                        GenRoomNode node = new GenRoomNode(floor, pos);
                        built.Add(node);
                        await room.Await();
                    }
                }
            }
            return built;
        }

        public static async Awaitable<List<GenRoomNode>> BuildOuterWalls(GenRoom room, System.Random random, GenRoomPreset preset)
        {
            List<GenRoomNode> built = new();
            if (preset.OuterWall.Count > 0)
            {
                Vector3Int pos = Vector3Int.zero;
                GameObject outerWallPreset = preset.OuterWall[random.Next(0, preset.OuterWall.Count)];
                for (int y = 0; y < room.GridSize.y; y++)
                {
                    for (int x = 0; x < room.GridSize.x; x++)
                    {
                        // South
                        pos = new Vector3Int(x, y, 0);
                        if (!room.OuterDoor.Exists(node => node.Position == pos))
                        {
                            GameObject outerWall = Object.Instantiate(outerWallPreset, room.Content);
                            outerWall.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                            outerWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.South * 90, 0);
                            outerWall.transform.localPosition += new Vector3(0, 0, -room.TileSize.z / 2f);
                            GenRoomNode node = new GenRoomNode(outerWall, pos);
                            built.Add(node);
                        }
                        // North
                        pos = new Vector3Int(x, y, room.GridSize.z - 1);
                        if (!room.OuterDoor.Exists(node => node.Position == pos))
                        {
                            GameObject outerWall = Object.Instantiate(outerWallPreset, room.Content);
                            outerWall.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                            outerWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.North * 90, 0);
                            outerWall.transform.localPosition += new Vector3(0, 0, room.TileSize.z / 2f);
                            GenRoomNode node = new GenRoomNode(outerWall, pos);
                            built.Add(node);
                        }
                        await room.Await();
                    }

                    for (int z = 0; z < room.GridSize.z; z++)
                    {
                        // West
                        pos = new Vector3Int(0, y, z);
                        if (!room.OuterDoor.Exists(node => node.Position == pos))
                        {
                            GameObject outerWall = Object.Instantiate(outerWallPreset, room.Content);
                            outerWall.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                            outerWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.West * 90, 0);
                            outerWall.transform.localPosition += new Vector3(-room.TileSize.x / 2f, 0, 0);
                            GenRoomNode node = new GenRoomNode(outerWall, pos);
                            built.Add(node);
                        }
                        // East
                        pos = new Vector3Int(room.GridSize.x - 1, y, z);
                        if (!room.OuterDoor.Exists(node => node.Position == pos))
                        {
                            GameObject outerWall = Object.Instantiate(outerWallPreset, room.Content);
                            outerWall.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                            outerWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.East * 90, 0);
                            outerWall.transform.localPosition += new Vector3(room.TileSize.x / 2f, 0, 0);
                            GenRoomNode node = new GenRoomNode(outerWall, pos);
                            built.Add(node);
                        }
                        await room.Await();
                    }
                }
            }
            return built;
        }

        public static async Awaitable<List<GenRoomNode>> BuildOuterDoors(GenRoom room, System.Random random, GenRoomPreset preset, int count)
        {
            List<GenRoomNode> built = new();
            if (preset.OuterDoor.Count > 0)
            {
                GameObject outerDoorPreset = preset.OuterDoor[random.Next(0, preset.OuterDoor.Count)];

                List<List<GenRoomNode>> possibleWalls = new();
                for (int i = 0; i < 4; i++) possibleWalls.Add(new List<GenRoomNode>());
                foreach (var outerWall in room.OuterWall)
                {
                    if (outerWall.Position.y == 0)
                    {
                        if (outerWall.Position.x == 0) possibleWalls[0].Add(outerWall);
                        if (outerWall.Position.z == 0) possibleWalls[1].Add(outerWall);
                        if (outerWall.Position.x == (room.GridSize.x - 1)) possibleWalls[2].Add(outerWall);
                        if (outerWall.Position.z == (room.GridSize.z - 1)) possibleWalls[3].Add(outerWall);
                    }
                }
                possibleWalls = possibleWalls.OrderBy(x => random.Next(int.MinValue, int.MaxValue)).ToList();

                for (int i = 0; i < count; i++)
                {
                    int index = i % 4;
                    if (possibleWalls[index].Count > 0)
                    {
                        GenRoomNode outerWall = possibleWalls[index][random.Next(0, possibleWalls[index].Count)];

                        for (int iter = 0; iter < 4; iter++) possibleWalls[iter].Remove(outerWall);
                        // possibleWalls[index].Remove(outerWall);

                        room.OuterWall.Remove(outerWall);
                        GameObject outerDoor = Object.Instantiate(outerDoorPreset, room.Content);
                        outerDoor.transform.position = outerWall.Spawn.transform.position;
                        outerDoor.transform.rotation = outerWall.Spawn.transform.rotation;
                        GenRoomNode node = new GenRoomNode(outerDoor, outerWall.Position);
                        built.Add(node);
                        Object.DestroyImmediate(outerWall.Spawn);
                        await room.Await();
                    }
                }
            }
            return built;
        }
    }
}