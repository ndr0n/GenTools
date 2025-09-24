using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    public struct GenTileRoomTunnel
    {
        public GenTileRoom Origin;
        public Vector2Int OriginPoint;
        public GenTileRoom Connection;
        public Vector2Int ConnectionPoint;
        public List<Vector2Int> Positions;

        public GenTileRoomTunnel(GenTileRoom origin, Vector2Int originPoint, GenTileRoom connection, Vector2Int connectionPoint, List<Vector2Int> positions)
        {
            Origin = origin;
            OriginPoint = originPoint;
            Connection = connection;
            ConnectionPoint = connectionPoint;
            Positions = positions;
        }
    }

    public enum GenTileRoomObjectType
    {
        Door,
        Floor,
        Wall,
        Balcony,
        Object,
    }

    [System.Serializable]
    public class GenTileRoom
    {
        public GenTileRoomType Type;
        public Vector2Int Size;
        public Vector2Int Position;
        readonly List<TileBase> roomTiles = new();

        [Header("Runtime")]
        public List<GenTileObject> PlacedFloor = new();
        public List<GenTileObject> PlacedWalls = new();
        public List<GenTileObject> PlacedDoors = new();
        public List<GenTileObject> PlacedStairs = new();
        public List<GenTileObject> PlacedBalcony = new();
        public List<GenTileObject> PlacedObjects = new();
        public List<GenTileRoomTunnel> PlacedTunnels = new();

        public GenTileRoom(GenTileRoomType type, Vector2Int size, Vector2Int position)
        {
            Type = type;
            Size = size;
            Position = position;
        }

        public Vector3 GetCenter() => new Vector3(Position.x + (Size.x / 2f), Position.y + (Size.y / 2f), 0);
        public Bounds GetBounds() => new Bounds(GetCenter(), new Vector3(Size.x, Size.y, 0));
        public BoundsInt GetBoundsInt() => new BoundsInt(Vector3Int.RoundToInt(GetCenter()), new Vector3Int(Size.x, Size.y, 0));
        public bool Contains(Vector2Int point) => point.x >= Position.x && point.x < (Position.x + Size.x) && point.y >= Position.y && point.y < (Position.y + Size.y);

        public List<Vector2Int> PlaceTileRoom(GenTile genTile, List<Vector2Int> availablePositions, System.Random random)
        {
            roomTiles.Clear();
            foreach (var floorAlgo in Type.Floor) availablePositions = PlaceAlgorithm(GenTileRoomObjectType.Floor, floorAlgo, genTile, availablePositions, random);
            foreach (var wallAlgo in Type.Walls) availablePositions = PlaceAlgorithm(GenTileRoomObjectType.Wall, wallAlgo, genTile, availablePositions, random);
            availablePositions = PlaceDoors(genTile, availablePositions, random);
            foreach (var wallAlgo in Type.Balcony) availablePositions = PlaceAlgorithm(GenTileRoomObjectType.Balcony, wallAlgo, genTile, availablePositions, random);
            availablePositions = PlaceObjects(genTile, availablePositions, random);
            return availablePositions;
        }

        public List<Vector2Int> PlaceAlgorithm(GenTileRoomObjectType type, GenTileAlgorithm algorithm, GenTile genTile, List<Vector2Int> availablePositions, System.Random random)
        {
            byte value = 1;
            byte[,] map = new byte[Size.x, Size.y];
            map = algorithm.Execute(map, value, random.Next(int.MinValue, int.MaxValue));
            List<Vector2Int> worldPositions = availablePositions.ToList();
            foreach (var worldPosition in worldPositions)
            {
                Vector2Int roomPosition = worldPosition - new Vector2Int(Position.x, Position.y);
                if (map[roomPosition.x, roomPosition.y] == value)
                {
                    availablePositions.Remove(roomPosition);
                    Vector3Int worldPos = new Vector3Int(worldPosition.x, worldPosition.y, 0);
                    genTile.Tilemap[(int) algorithm.Type].SetTile(worldPos, algorithm.Tile);

                    TileData data = new();
                    algorithm.Tile.GetTileData(worldPos, genTile.Tilemap[(int) algorithm.Type], ref data);
                    GameObject obj = data.gameObject;

                    switch (type)
                    {
                        case GenTileRoomObjectType.Floor:
                            PlacedFloor.Add(new GenTileObject(algorithm.Tile, roomPosition, obj));
                            break;
                        case GenTileRoomObjectType.Door:
                            PlacedDoors.Add(new GenTileObject(algorithm.Tile, roomPosition, obj));
                            break;
                        case GenTileRoomObjectType.Wall:
                            PlacedWalls.Add(new GenTileObject(algorithm.Tile, roomPosition, obj));
                            break;
                        case GenTileRoomObjectType.Balcony:
                            PlacedBalcony.Add(new GenTileObject(algorithm.Tile, roomPosition, obj));
                            break;
                        case GenTileRoomObjectType.Object:
                            PlacedObjects.Add(new GenTileObject(algorithm.Tile, roomPosition, obj));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
            }
            if (!roomTiles.Contains(algorithm.Tile)) roomTiles.Add(algorithm.Tile);
            return availablePositions;
        }

        public List<Vector2Int> PlaceDoors(GenTile genTile, List<Vector2Int> availablePositions, System.Random random)
        {
            if (Type.Doors.Count > 0)
            {
                int doorCount = random.Next(Type.DoorCount.x, Type.DoorCount.y + 1);
                List<CardinalDirection> directions = new() {CardinalDirection.South, CardinalDirection.West, CardinalDirection.North, CardinalDirection.East};
                directions = directions.OrderBy(x => random.Next(int.MinValue, int.MaxValue)).ToList();
                Tilemap tunnelTilemap = genTile.Tilemap[(int) GenTileType.Terrain];
                Tilemap doorTilemap = genTile.Tilemap[(int) GenTileType.Terrain];
                TileBase door = Type.Doors[random.Next(Type.Doors.Count)];

                for (int i = 0; i < doorCount; i++)
                {
                    if (directions.Count == 0) break;
                    CardinalDirection direction = directions[0];
                    directions.RemoveAt(0);
                    switch (direction)
                    {
                        case CardinalDirection.South:
                            bool breakLoop1 = false;
                            for (int x1 = Position.x + 1; x1 < Position.x + Size.x - 1; x1++)
                            {
                                int y1 = Position.y;
                                Vector3Int doorPosition1 = new Vector3Int(x1, y1, 0);
                                foreach (var floorTile in Type.OuterFloorTile)
                                {
                                    if (tunnelTilemap.GetTile(doorPosition1 + Vector3Int.down) == floorTile)
                                    {
                                        doorTilemap.SetTile(doorPosition1, door);

                                        TileData data = new();
                                        door.GetTileData(doorPosition1, doorTilemap, ref data);
                                        GameObject spawn = data.gameObject;

                                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(x1, y1), spawn));
                                        breakLoop1 = true;
                                        break;
                                    }
                                }
                                if (breakLoop1) break;
                            }
                            break;
                        case CardinalDirection.West:
                            bool breakLoop2 = false;
                            for (int y2 = Position.y + 1; y2 < Position.y + Size.y - 1; y2++)
                            {
                                int x2 = Position.x;
                                Vector3Int doorPosition2 = new Vector3Int(x2, y2, 0);
                                foreach (var floorTile in Type.OuterFloorTile)
                                {
                                    if (tunnelTilemap.GetTile(doorPosition2 + Vector3Int.left) == floorTile)
                                    {
                                        doorTilemap.SetTile(doorPosition2, door);

                                        TileData data = new();
                                        door.GetTileData(doorPosition2, doorTilemap, ref data);
                                        GameObject spawn = data.gameObject;

                                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(x2, y2), spawn));
                                        breakLoop2 = true;
                                        break;
                                    }
                                }
                                if (breakLoop2) break;
                            }
                            break;
                        case CardinalDirection.North:
                            bool breakLoop3 = false;
                            for (int x3 = Position.x + 1; x3 < Position.x + Size.x - 1; x3++)
                            {
                                int y3 = Position.y + Size.y - 1;
                                Vector3Int doorPosition3 = new Vector3Int(x3, y3, 0);
                                foreach (var floorTile in Type.OuterFloorTile)
                                {
                                    if (tunnelTilemap.GetTile(doorPosition3 + Vector3Int.up) == floorTile)
                                    {
                                        doorTilemap.SetTile(doorPosition3, door);

                                        TileData data = new();
                                        door.GetTileData(doorPosition3, doorTilemap, ref data);
                                        GameObject spawn = data.gameObject;

                                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(x3, y3), spawn));
                                        breakLoop3 = true;
                                        break;
                                    }
                                }
                                if (breakLoop3) break;
                            }
                            break;
                        case CardinalDirection.East:
                            bool breakLoop4 = false;
                            for (int y4 = Position.y + 1; y4 < Position.y + Size.y - 1; y4++)
                            {
                                int x4 = Position.x + Size.x - 1;
                                Vector3Int doorPosition4 = new Vector3Int(x4, y4, 0);
                                foreach (var floorTile in Type.OuterFloorTile)
                                {
                                    if (tunnelTilemap.GetTile(doorPosition4 + Vector3Int.right) == floorTile)
                                    {
                                        doorTilemap.SetTile(doorPosition4, door);

                                        TileData data = new();
                                        door.GetTileData(doorPosition4, doorTilemap, ref data);
                                        GameObject spawn = data.gameObject;

                                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(x4, y4), spawn));
                                        breakLoop4 = true;
                                        break;
                                    }
                                }
                                if (breakLoop4) break;
                            }
                            break;
                    }
                }
            }
            return availablePositions;
        }

        public void PlaceStairs()
        {
            List<GenTileObjectData> toPlace = new();
        }

        public List<Vector2Int> PlaceObjects(GenTile genTile, List<Vector2Int> availablePositions, System.Random random)
        {
            List<GenTileObjectData> toPlace = new();
            List<Vector2Int> possiblePosition = new();

            if (Type.Objects.Count > 0)
            {
                Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Objects];
                foreach (var pos in availablePositions) possiblePosition.Add(pos);
                foreach (var obj in Type.Objects)
                {
                    for (int i = 0; i < obj.Amount; i++) toPlace.Add(obj);
                }

                List<GenTileObjectData> priority = new();
                foreach (var placer in toPlace)
                {
                    GenTileObjectData obj = placer;
                    if (priority.Count > 0)
                    {
                        obj = priority[0];
                        priority.RemoveAt(0);
                    }
                    List<Vector2Int> iterPositions = possiblePosition.OrderBy(x => random.Next()).ToList();
                    switch (obj.PlacementRules)
                    {
                        case GenTilePlacementRules.Any:
                            break;
                        case GenTilePlacementRules.Inner:
                            iterPositions = iterPositions.Where(x => x.x != Position.x && x.x != (Position.x + Size.x - 1) && x.y != Position.y && x.y != (Position.y + Size.y - 1)).ToList();
                            break;
                        case GenTilePlacementRules.Outer:
                            iterPositions = iterPositions.Where(x => x.x == Position.x || x.x == (Position.x + Size.x - 1) || x.y == Position.y || x.y == (Position.y + Size.y - 1)).ToList();
                            break;
                    }
                    if (iterPositions.Count == 0) continue;

                    Vector3Int pos = new Vector3Int(iterPositions[0].x, iterPositions[0].y, 0);
                    if (tilemap.GetTile(pos) != null) continue;
                    if (random.Next(0, 100) < obj.Chance)
                    {
                        possiblePosition.Remove(iterPositions[0]);
                        availablePositions.Remove(iterPositions[0]);
                        TileBase tile = obj.Tile[random.Next(obj.Tile.Count)];
                        tilemap.SetTile(pos, tile);

                        TileData data = new();
                        tile.GetTileData(pos, tilemap, ref data);
                        GameObject spawn = data.gameObject;

                        PlacedObjects.Add(new GenTileObject(tile, iterPositions[0], spawn));
                        priority.AddRange(obj.Recursion);
                        // Tilegen.CollisionMap[pos.x, pos.y] = true;
                    }
                }
            }
            return availablePositions;
        }
    }
}