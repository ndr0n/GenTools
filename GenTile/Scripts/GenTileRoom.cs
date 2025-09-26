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
            List<Vector2Int> positions = new();
            foreach (var pos in availablePositions.Distinct()) positions.Add(pos - Position);

            foreach (var floorAlgo in Type.Floor)
            {
                List<Vector2Int> placedFloor = PlaceAlgorithm(GenTileRoomObjectType.Floor, floorAlgo, genTile, positions, random);
                positions = placedFloor;
            }

            foreach (var wallAlgo in Type.Walls)
            {
                List<Vector2Int> placedWalls = PlaceAlgorithm(GenTileRoomObjectType.Wall, wallAlgo, genTile, positions, random);
                foreach (var wall in placedWalls) positions.Remove(wall);
            }

            List<Vector2Int> placedDoors = PlaceDoors(genTile, availablePositions, random);
            foreach (var door in placedDoors) positions.Remove(door);

            foreach (var wallAlgo in Type.Balcony)
            {
                List<Vector2Int> placedBalcony = PlaceAlgorithm(GenTileRoomObjectType.Balcony, wallAlgo, genTile, positions, random);
            }

            foreach (var objectAlgo in Type.Objects)
            {
                List<Vector2Int> placedObjects = PlaceAlgorithm(GenTileRoomObjectType.Object, objectAlgo, genTile, positions, random);
                // foreach (var obj in placedObjects) positions.Remove(obj);
            }

            for (int i = 0; i < positions.Count; i++) positions[i] = positions[i] + Position;

            return positions.Distinct().ToList();
        }

        public List<Vector2Int> PlaceAlgorithm(GenTileRoomObjectType type, GenTileAlgorithm algorithm, GenTile genTile, List<Vector2Int> availablePositions, System.Random random)
        {
            availablePositions = availablePositions.Where(v => v.x >= algorithm.Offset.x && v.x < Size.x - algorithm.Offset.x && v.y >= algorithm.Offset.y && v.y < Size.y - algorithm.Offset.y).ToList();
            List<Vector2Int> placed = algorithm.Execute(availablePositions, random.Next(int.MinValue, int.MaxValue));
            foreach (var p in placed)
            {
                Vector2Int worldPosition = p + Position;
                Vector3Int worldPos = new Vector3Int(worldPosition.x, worldPosition.y, 0);
                genTile.Tilemap[(int) algorithm.Type].SetTile(worldPos, algorithm.Tile);
                TileData data = new();
                algorithm.Tile.GetTileData(worldPos, genTile.Tilemap[(int) algorithm.Type], ref data);
                GameObject spawn = data.gameObject;

                switch (type)
                {
                    case GenTileRoomObjectType.Floor:
                        PlacedFloor.Add(new GenTileObject(algorithm.Tile, worldPosition, spawn));
                        break;
                    case GenTileRoomObjectType.Door:
                        PlacedDoors.Add(new GenTileObject(algorithm.Tile, worldPosition, spawn));
                        break;
                    case GenTileRoomObjectType.Wall:
                        PlacedWalls.Add(new GenTileObject(algorithm.Tile, worldPosition, spawn));
                        break;
                    case GenTileRoomObjectType.Balcony:
                        PlacedBalcony.Add(new GenTileObject(algorithm.Tile, worldPosition, spawn));
                        break;
                    case GenTileRoomObjectType.Object:
                        PlacedObjects.Add(new GenTileObject(algorithm.Tile, worldPosition, spawn));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            if (!roomTiles.Contains(algorithm.Tile)) roomTiles.Add(algorithm.Tile);
            return placed;
        }

        public List<Vector2Int> PlaceDoors(GenTile genTile, List<Vector2Int> availablePositions, System.Random random)
        {
            if (Type.Doors.Count > 0)
            {
                int doorCount = random.Next(Type.DoorCount.x, Type.DoorCount.y + 1);
                List<CardinalDirection> directions = new() {CardinalDirection.South, CardinalDirection.West, CardinalDirection.North, CardinalDirection.East};
                directions = directions.OrderBy(x => random.Next(int.MinValue, int.MaxValue)).ToList();
                Tilemap floorTilemap = genTile.Tilemap[(int) GenTileType.Terrain];
                Tilemap doorTilemap = genTile.Tilemap[(int) GenTileType.Objects];
                TileBase door = Type.Doors[random.Next(Type.Doors.Count)];

                for (int i = 0; i < doorCount; i++)
                {
                    if (directions.Count == 0) break;
                    CardinalDirection direction = directions[0];
                    directions.RemoveAt(0);
                    switch (direction)
                    {
                        case CardinalDirection.South:
                            foreach (int x1 in GT.GetRandomIterArray(Position.x + 1, Position.x + Size.x - 1, random))
                            {
                                int y1 = Position.y;
                                Vector3Int doorPosition1 = new Vector3Int(x1, y1, 0);

                                Vector3Int checkPosition = doorPosition1 + Vector3Int.down;
                                if (floorTilemap.HasTile(checkPosition))
                                {
                                    // TileData checkData = new();
                                    // door.GetTileData(checkPosition, floorTilemap, ref checkData);
                                    if (genTile.CollisionMap[checkPosition.x, checkPosition.y] == false)
                                    {
                                        doorTilemap.SetTile(doorPosition1, door);
                                        TileData doorData = new();
                                        door.GetTileData(doorPosition1, doorTilemap, ref doorData);
                                        GameObject spawn = doorData.gameObject;
                                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(x1, y1), spawn));
                                        break;
                                    }
                                }
                            }
                            break;
                        case CardinalDirection.West:
                            foreach (int y2 in GT.GetRandomIterArray(Position.y + 1, Position.y + Size.y - 1, random))
                            {
                                int x2 = Position.x;
                                Vector3Int doorPosition2 = new Vector3Int(x2, y2, 0);

                                Vector3Int checkPosition = doorPosition2 + Vector3Int.left;
                                if (floorTilemap.HasTile(checkPosition))
                                {
                                    // TileData checkData = new();
                                    // door.GetTileData(checkPosition, floorTilemap, ref checkData);
                                    if (genTile.CollisionMap[checkPosition.x, checkPosition.y] == false)
                                    {
                                        doorTilemap.SetTile(doorPosition2, door);
                                        TileData doorData = new();
                                        door.GetTileData(doorPosition2, doorTilemap, ref doorData);
                                        GameObject spawn = doorData.gameObject;
                                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(x2, y2), spawn));
                                        break;
                                    }
                                }
                            }
                            break;
                        case CardinalDirection.North:
                            foreach (int x3 in GT.GetRandomIterArray(Position.x + 1, Position.x + Size.x - 1, random))
                            {
                                int y3 = Position.y + Size.y - 1;
                                Vector3Int doorPosition3 = new Vector3Int(x3, y3, 0);

                                Vector3Int checkPosition = doorPosition3 + Vector3Int.up;
                                if (floorTilemap.HasTile(checkPosition))
                                {
                                    // TileData checkData = new();
                                    // door.GetTileData(checkPosition, floorTilemap, ref checkData);
                                    // Debug.Log($"Check North! {checkData.colliderType}");
                                    if (genTile.CollisionMap[checkPosition.x, checkPosition.y] == false)
                                    {
                                        doorTilemap.SetTile(doorPosition3, door);
                                        TileData doorData = new();
                                        door.GetTileData(doorPosition3, doorTilemap, ref doorData);
                                        GameObject spawn = doorData.gameObject;
                                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(x3, y3), spawn));
                                        break;
                                    }
                                }
                            }
                            break;
                        case CardinalDirection.East:
                            foreach (int y4 in GT.GetRandomIterArray(Position.y + 1, Position.y + Size.y - 1, random))
                            {
                                int x4 = Position.x + Size.x - 1;
                                Vector3Int doorPosition4 = new Vector3Int(x4, y4, 0);

                                Vector3Int checkPosition = doorPosition4 + Vector3Int.right;
                                if (floorTilemap.HasTile(checkPosition))
                                {
                                    // TileData checkData = new();
                                    // door.GetTileData(checkPosition, floorTilemap, ref checkData);
                                    if (genTile.CollisionMap[checkPosition.x, checkPosition.y] == false)
                                    {
                                        doorTilemap.SetTile(doorPosition4, door);
                                        TileData doorData = new();
                                        door.GetTileData(doorPosition4, doorTilemap, ref doorData);
                                        GameObject spawn = doorData.gameObject;
                                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(x4, y4), spawn));
                                        break;
                                    }
                                }
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
            // List<GenTileObjectData> toPlace = new();
            // List<Vector2Int> possiblePosition = new();
            //
            // if (Type.Objects.Count > 0)
            // {
            //     Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Objects];
            //     foreach (var pos in availablePositions) possiblePosition.Add(pos);
            //     foreach (var obj in Type.Objects)
            //     {
            //         for (int i = 0; i < obj.Amount; i++) toPlace.Add(obj);
            //     }
            //
            //     List<GenTileObjectData> priority = new();
            //     foreach (var placer in toPlace)
            //     {
            //         GenTileObjectData obj = placer;
            //         if (priority.Count > 0)
            //         {
            //             obj = priority[0];
            //             priority.RemoveAt(0);
            //         }
            //         List<Vector2Int> iterPositions = possiblePosition.OrderBy(x => random.Next()).ToList();
            //         switch (obj.PlacementRules)
            //         {
            //             case GenTilePlacementRules.Any:
            //                 break;
            //             case GenTilePlacementRules.Inner:
            //                 iterPositions = iterPositions.Where(x => x.x != Position.x && x.x != (Position.x + Size.x - 1) && x.y != Position.y && x.y != (Position.y + Size.y - 1)).ToList();
            //                 break;
            //             case GenTilePlacementRules.Outer:
            //                 iterPositions = iterPositions.Where(x => x.x == Position.x || x.x == (Position.x + Size.x - 1) || x.y == Position.y || x.y == (Position.y + Size.y - 1)).ToList();
            //                 break;
            //         }
            //         if (iterPositions.Count == 0) continue;
            //
            //         Vector3Int pos = new Vector3Int(iterPositions[0].x, iterPositions[0].y, 0);
            //         if (tilemap.GetTile(pos) != null) continue;
            //         if (random.Next(0, 100) < obj.Chance)
            //         {
            //             possiblePosition.Remove(iterPositions[0]);
            //             availablePositions.Remove(iterPositions[0]);
            //             TileBase tile = obj.Tile[random.Next(obj.Tile.Count)];
            //             tilemap.SetTile(pos, tile);
            //
            //             TileData data = new();
            //             tile.GetTileData(pos, tilemap, ref data);
            //             GameObject spawn = data.gameObject;
            //
            //             PlacedObjects.Add(new GenTileObject(tile, iterPositions[0], spawn));
            //             priority.AddRange(obj.Recursion);
            //             // Tilegen.CollisionMap[pos.x, pos.y] = true;
            //         }
            //     }
            // }
            // return availablePositions;
            return null;
        }
    }
}