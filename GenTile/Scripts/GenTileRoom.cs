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
            availablePositions = ReplaceFloor(genTile, availablePositions, random);
            availablePositions = PlaceDoors(genTile, availablePositions, random);
            foreach (var wallAlgo in Type.Walls) availablePositions = PlaceAlgorithm(GenTileRoomObjectType.Wall, wallAlgo, genTile, availablePositions, random);
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
                    genTile.Tilemap[(int) algorithm.Type].SetTile(new Vector3Int(worldPosition.x, worldPosition.y, 0), algorithm.Tile);
                    switch (type)
                    {
                        case GenTileRoomObjectType.Door:
                            PlacedDoors.Add(new GenTileObject(algorithm.Tile, roomPosition));
                            break;
                        case GenTileRoomObjectType.Wall:
                            PlacedWalls.Add(new GenTileObject(algorithm.Tile, roomPosition));
                            break;
                        case GenTileRoomObjectType.Balcony:
                            PlacedBalcony.Add(new GenTileObject(algorithm.Tile, roomPosition));
                            break;
                        case GenTileRoomObjectType.Object:
                            PlacedObjects.Add(new GenTileObject(algorithm.Tile, roomPosition));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
            }
            return availablePositions;
        }

        public List<Vector2Int> ReplaceFloor(GenTile genTile, List<Vector2Int> availablePositions, System.Random random)
        {
            if (Type.Floor.Count > 0)
            {
                Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Terrain];
                TileBase floor = Type.Floor[random.Next(Type.Floor.Count)];
                foreach (var pos in availablePositions)
                {
                    tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), floor);
                    PlacedFloor.Add(new GenTileObject(floor, new Vector2Int(pos.x - Position.x, pos.y - Position.y)));
                }
            }
            return availablePositions;
        }

        public List<Vector2Int> PlaceDoors(GenTile genTile, List<Vector2Int> availablePositions, System.Random random)
        {
            if (Type.Doors.Count > 0)
            {
                Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Objects];
                TileBase door = Type.Doors[random.Next(Type.Doors.Count)];
                foreach (var tunnel in PlacedTunnels)
                {
                    availablePositions.Remove(tunnel.OriginPoint);
                    if (Type.PlaceDoorsInsideRoom)
                    {
                        Vector3Int doorPosition = new Vector3Int(tunnel.OriginPoint.x, tunnel.OriginPoint.y, 0);
                        tilemap.SetTile(doorPosition, door);
                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(tunnel.OriginPoint.x - Position.x, tunnel.OriginPoint.y - Position.y)));
                    }
                    else
                    {
                        Vector3Int doorPosition = new Vector3Int(tunnel.Positions[0].x, tunnel.Positions[0].y, 0);
                        tilemap.SetTile(doorPosition, door);
                        PlacedDoors.Add(new GenTileObject(door, new Vector2Int(tunnel.Positions[0].x - Position.x, tunnel.Positions[0].y - Position.y)));
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
                        PlacedObjects.Add(new GenTileObject(tile, iterPositions[0]));
                        priority.AddRange(obj.Recursion);
                        // Tilegen.CollisionMap[pos.x, pos.y] = true;
                    }
                }
            }
            return availablePositions;
        }
    }
}