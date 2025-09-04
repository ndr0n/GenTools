using System.Collections.Generic;
using System.Linq;
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

    [System.Serializable]
    public class GenTileRoom
    {
        public GenTileRoomType Type;
        public Vector2Int Size;
        public Vector2Int Position;

        [Header("Runtime")]
        public List<GenTileObject> PlacedWalls = new();
        public List<GenTileObject> PlacedDoors = new();
        public List<GenTileObject> PlacedStairs = new();
        public List<GenTileObject> PlacedObjects = new();
        public List<GenTileRoomTunnel> PlacedTunnels = new();

        public GenTileRoom(GenTileRoomType type, Vector2Int size, Vector2Int position)
        {
            Type = type;
            Size = size;
            Position = position;
            PlacedDoors = new();
            PlacedStairs = new();
            PlacedObjects = new();
        }

        public Vector3 GetCenter() => new Vector3(Position.x + (Size.x / 2f), Position.y + (Size.y / 2f), 0);
        public Bounds GetBounds() => new Bounds(GetCenter(), new Vector3(Size.x, Size.y, 0));
        public BoundsInt GetBoundsInt() => new BoundsInt(Vector3Int.RoundToInt(GetCenter()), new Vector3Int(Size.x, Size.y, 0));
        public bool Contains(Vector2Int point) => point.x >= Position.x && point.x < (Position.x + Size.x) && point.y >= Position.y && point.y < (Position.y + Size.y);

        public List<Vector3Int> ReplaceFloor(GenTile genTile, List<Vector3Int> availablePositions, System.Random random)
        {
            if (Type.Floor.Count > 0)
            {
                Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Terrain];
                TileBase floor = Type.Floor[random.Next(Type.Floor.Count)];
                foreach (var pos in availablePositions)
                {
                    tilemap.SetTile(pos, floor);
                }
            }

            return availablePositions;
        }

        public List<Vector3Int> PlaceDoors(GenTile genTile, List<Vector3Int> availablePositions, System.Random random, bool placeInsideRoom)
        {
            if (Type.Doors.Count > 0)
            {
                Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Objects];
                TileBase door = Type.Doors[random.Next(Type.Doors.Count)];
                foreach (var tunnel in PlacedTunnels)
                {
                    if (placeInsideRoom)
                    {
                        Vector3Int doorPosition = new Vector3Int(tunnel.OriginPoint.x, tunnel.OriginPoint.y, 0);
                        if (availablePositions.Contains(doorPosition))
                        {
                            availablePositions.Remove(doorPosition);
                            tilemap.SetTile(doorPosition, door);
                            PlacedDoors.Add(new GenTileObject(door, doorPosition));
                        }
                    }
                    else
                    {
                        Vector3Int doorPosition = new Vector3Int(tunnel.Positions[0].x, tunnel.Positions[0].y, 0);
                        tilemap.SetTile(doorPosition, door);
                        PlacedDoors.Add(new GenTileObject(door, doorPosition));
                    }
                }
            }
            return availablePositions;
        }

        public List<Vector3Int> PlaceWalls(GenTile genTile, List<Vector3Int> availablePositions, System.Random random)
        {
            List<Vector3Int> possiblePosition = new();
            for (int x = 0; x < (Size.x); x++)
            {
                int posx = x + Position.x;
                Vector3Int pos1 = new Vector3Int(posx, Position.y, 0);
                if (availablePositions.Contains(pos1)) possiblePosition.Add(pos1);
                int posy = Size.y + Position.y - 1;
                Vector3Int pos2 = new Vector3Int(posx, posy, 0);
                if (availablePositions.Contains(pos2)) possiblePosition.Add(pos2);
            }
            for (int y = 0; y < (Size.y); y++)
            {
                int posy = y + Position.y;
                Vector3Int pos1 = new Vector3Int(Position.x, posy, 0);
                if (availablePositions.Contains(pos1)) possiblePosition.Add(pos1);
                int posx = Size.x + Position.x - 1;
                Vector3Int pos2 = new Vector3Int(posx, posy, 0);
                if (availablePositions.Contains(pos2)) possiblePosition.Add(pos2);
            }

            // List<GenTileObjectData> doorsToPlace = new();
            // foreach (var door in Type.Doors)
            // {
            //     for (int i = 0; i < door.Amount; i++) doorsToPlace.Add(door);
            // }
            // List<Vector3Int> iterPositions = possiblePosition.OrderBy(x => random.Next()).ToList();
            //
            // if (doorsToPlace.Count > 0)
            // {
            //     Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Objects];
            //     List<GenTileObjectData> priority = new();
            //     foreach (var pos in iterPositions)
            //     {
            //         // if (floorTilemap.GetTile(pos) != null) continue;
            //
            //         GenTileObjectData door = null;
            //         if (priority.Count > 0)
            //         {
            //             door = priority[0];
            //             priority.RemoveAt(0);
            //         }
            //         else if (doorsToPlace.Count > 0)
            //         {
            //             int index = random.Next(0, doorsToPlace.Count);
            //             door = doorsToPlace[index];
            //             doorsToPlace.RemoveAt(index);
            //         }
            //         if (door != null)
            //         {
            //             if (random.Next(0, 100) < door.Chance)
            //             {
            //                 possiblePosition.Remove(pos);
            //                 availablePositions.Remove(pos);
            //                 TileBase tile = door.Tile[random.Next(door.Tile.Count)];
            //                 tilemap.SetTile(pos, tile);
            //                 PlacedDoors.Add(new GenTileObject(tile, pos));
            //                 priority.AddRange(door.Recursion);
            //                 // Tilegen.CollisionMap[pos.x, pos.y] = true;
            //             }
            //         }
            //     }
            // }

            List<GenTileObjectData> wallsToPlace = new();
            foreach (var wall in Type.Walls)
            {
                for (int i = 0; i < (possiblePosition.Count * (wall.Amount / 100)); i++) wallsToPlace.Add(wall);
            }
            List<Vector3Int> iterPositions = possiblePosition.OrderBy(x => random.Next()).ToList();

            if (wallsToPlace.Count > 0)
            {
                Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Terrain];
                List<GenTileObjectData> priority = new();
                foreach (var pos in iterPositions)
                {
                    // if (floorTilemap.GetTile(pos) != null) continue;

                    GenTileObjectData wall = null;
                    if (priority.Count > 0)
                    {
                        wall = priority[0];
                        priority.RemoveAt(0);
                    }
                    else if (wallsToPlace.Count > 0)
                    {
                        int index = random.Next(0, wallsToPlace.Count);
                        wall = wallsToPlace[index];
                        wallsToPlace.RemoveAt(index);
                    }
                    if (wall != null)
                    {
                        if (random.Next(0, 100) < wall.Chance)
                        {
                            possiblePosition.Remove(pos);
                            availablePositions.Remove(pos);
                            TileBase tile = wall.Tile[random.Next(wall.Tile.Count)];
                            tilemap.SetTile(pos, tile);
                            PlacedWalls.Add(new GenTileObject(tile, pos));
                            // PlacedObjects.Add(new TileObject(door, tile, pos));
                            priority.AddRange(wall.Recursion);
                            // Tilegen.CollisionMap[pos.x, pos.y] = true;
                        }
                    }
                }
            }

            return availablePositions;
        }

        public void PlaceStairs()
        {
            List<GenTileObjectData> toPlace = new();
        }

        public List<Vector3Int> PlaceObjects(GenTile genTile, List<Vector3Int> availablePositions, System.Random random)
        {
            List<GenTileObjectData> toPlace = new();
            List<Vector3Int> possiblePosition = new();

            if (Type.Objects.Count > 0)
            {
                Tilemap tilemap = genTile.Tilemap[(int) GenTileType.Objects];
                foreach (var pos in availablePositions)
                {
                    possiblePosition.Add(pos);
                }

                List<Vector3Int> iterPositions = possiblePosition.OrderBy(x => random.Next()).ToList();
                foreach (var obj in Type.Objects)
                {
                    for (int i = 0; i < obj.Amount; i++) toPlace.Add(obj);
                }

                List<GenTileObjectData> priority = new();
                foreach (var pos in iterPositions)
                {
                    if (tilemap.GetTile(pos) != null) continue;

                    GenTileObjectData obj = null;
                    if (priority.Count > 0)
                    {
                        obj = priority[0];
                        priority.RemoveAt(0);
                    }
                    else if (toPlace.Count > 0)
                    {
                        int index = random.Next(0, toPlace.Count);
                        obj = toPlace[index];
                        toPlace.RemoveAt(index);
                    }
                    if (obj != null)
                    {
                        if (random.Next(0, 100) < obj.Chance)
                        {
                            possiblePosition.Remove(pos);
                            availablePositions.Remove(pos);

                            TileBase tile = obj.Tile[random.Next(obj.Tile.Count)];
                            tilemap.SetTile(pos, tile);
                            PlacedObjects.Add(new GenTileObject(tile, pos));
                            priority.AddRange(obj.Recursion);
                            // Tilegen.CollisionMap[pos.x, pos.y] = true;
                        }
                    }
                }
            }
            return availablePositions;
        }
    }
}