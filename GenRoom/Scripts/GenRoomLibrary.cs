using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public static class GenRoomLibrary
    {
        public static List<List<GameObject>> GetOuterWalls(GenRoom room)
        {
            List<List<GameObject>> outerWalls = new List<List<GameObject>>();
            for (int i = 0; i < 4; i++) outerWalls.Add(new List<GameObject>());

            Vector3Int pos;
            for (int x = 0; x < room.GridSize.x; x++)
            {
                // South
                pos = new Vector3Int(x, 0, 0);
                GameObject southWall = room.Node[pos.y][pos.x][pos.z].Wall[(int) CardinalDirection.South];
                if (southWall != null) outerWalls[(int) CardinalDirection.South].Add(southWall);
                // North
                pos = new Vector3Int(x, 0, room.GridSize.z - 1);
                GameObject northWall = room.Node[pos.y][pos.x][pos.z].Wall[(int) CardinalDirection.North];
                if (northWall != null) outerWalls[(int) CardinalDirection.North].Add(northWall);
            }

            for (int z = 0; z < room.GridSize.z; z++)
            {
                // West
                pos = new Vector3Int(0, 0, z);
                GameObject southWall = room.Node[pos.y][pos.x][pos.z].Wall[(int) CardinalDirection.West];
                if (southWall != null) outerWalls[(int) CardinalDirection.West].Add(southWall);
                // East
                pos = new Vector3Int(room.GridSize.x - 1, 0, z);
                GameObject northWall = room.Node[pos.y][pos.x][pos.z].Wall[(int) CardinalDirection.East];
                if (northWall != null) outerWalls[(int) CardinalDirection.East].Add(northWall);
            }

            return outerWalls;
        }

        #region Build

        public static async Awaitable BuildFloor(GenRoom room, System.Random random, GenRoomPreset preset)
        {
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
                        room.Node[0][x][z].Floor = floor;
                        await room.Await();
                    }
                }
            }
        }

        public static async Awaitable BuildOuterWalls(GenRoom room, System.Random random, GenRoomPreset preset)
        {
            if (preset.OuterWall.Count > 0)
            {
                GameObject outerWallPreset = preset.OuterWall[random.Next(0, preset.OuterWall.Count)];
                for (int y = 0; y < room.GridSize.y; y++)
                {
                    Vector3Int pos;
                    for (int x = 0; x < room.GridSize.x; x++)
                    {
                        // South
                        pos = new Vector3Int(x, y, 0);
                        GameObject southWall = Object.Instantiate(outerWallPreset, room.Content);
                        southWall.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                        southWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.South * 90, 0);
                        southWall.transform.localPosition += new Vector3(0, 0, -room.TileSize.z / 2f);
                        room.Node[pos.y][pos.x][pos.z].Wall[(int) CardinalDirection.South] = southWall;

                        // North
                        pos = new Vector3Int(x, y, room.GridSize.z - 1);
                        GameObject northWall = Object.Instantiate(outerWallPreset, room.Content);
                        northWall.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                        northWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.North * 90, 0);
                        northWall.transform.localPosition += new Vector3(0, 0, room.TileSize.z / 2f);
                        room.Node[pos.y][pos.x][pos.z].Wall[(int) CardinalDirection.North] = northWall;

                        await room.Await();
                    }

                    for (int z = 0; z < room.GridSize.z; z++)
                    {
                        // West
                        pos = new Vector3Int(0, y, z);
                        GameObject westWall = Object.Instantiate(outerWallPreset, room.Content);
                        westWall.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                        westWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.West * 90, 0);
                        westWall.transform.localPosition += new Vector3(-room.TileSize.x / 2f, 0, 0);
                        room.Node[pos.y][pos.x][pos.z].Wall[(int) CardinalDirection.West] = westWall;

                        // East
                        pos = new Vector3Int(room.GridSize.x - 1, y, z);
                        GameObject eastWall = Object.Instantiate(outerWallPreset, room.Content);
                        eastWall.transform.localPosition = new Vector3(pos.x * room.TileSize.x, pos.y * room.TileSize.y, pos.z * room.TileSize.z);
                        eastWall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.East * 90, 0);
                        eastWall.transform.localPosition += new Vector3(room.TileSize.x / 2f, 0, 0);
                        room.Node[pos.y][pos.x][pos.z].Wall[(int) CardinalDirection.East] = eastWall;

                        await room.Await();
                    }
                }
            }
        }

        public static async Awaitable BuildOuterDoors(GenRoom room, System.Random random, GenRoomPreset preset, int count)
        {
            if (preset.OuterDoor.Count > 0)
            {
                GameObject outerDoorPreset = preset.OuterDoor[random.Next(0, preset.OuterDoor.Count)];

                List<List<GameObject>> possibleWalls = GetOuterWalls(room);
                possibleWalls = possibleWalls.OrderBy(x => random.Next(int.MinValue, int.MaxValue)).ToList();

                for (int i = 0; i < count; i++)
                {
                    int index = i % 4;
                    if (possibleWalls[index].Count > 0)
                    {
                        GameObject outerWall = possibleWalls[index][random.Next(0, possibleWalls[index].Count)];
                        possibleWalls[index].Remove(outerWall);
                        GameObject outerDoor = Object.Instantiate(outerDoorPreset, room.Content);
                        outerDoor.transform.position = outerWall.transform.position;
                        outerDoor.transform.rotation = outerWall.transform.rotation;
                        room.OuterDoor.Add(outerDoor);
                        Object.DestroyImmediate(outerWall);
                        await room.Await();
                    }
                }
            }
        }

        #endregion
    }
}