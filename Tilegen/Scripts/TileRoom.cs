using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    public class TileRoom
    {
        public TileRoomType Type;
        public Vector2Int Size;
        public Vector2Int Position;

        [Header("Runtime")]
        public List<TileObject> PlacedWalls = new();
        public List<TileObject> PlacedDoors = new();
        public List<TileObject> PlacedStairs = new();
        public List<TileObject> PlacedObjects = new();

        public TileRoom(TileRoomType type, Vector2Int size, Vector2Int position)
        {
            Type = type;
            Size = size;
            Position = position;
            PlacedDoors = new();
            PlacedStairs = new();
            PlacedObjects = new();
        }

        public List<Vector3Int> ReplaceFloor(Tilegen tilegen, List<Vector3Int> availablePositions, System.Random random)
        {
            if (Type.Floor.Count > 0)
            {
                Tilemap tilemap = tilegen.Tilemap[(int) TilegenType.Terrain];
                TileBase floor = Type.Floor[random.Next(Type.Floor.Count)];
                foreach (var pos in availablePositions)
                {
                    tilemap.SetTile(pos, floor);
                }
            }

            return availablePositions;
        }

        public List<Vector3Int> PlaceWallsAndDoors(Tilegen tilegen, List<Vector3Int> availablePositions, System.Random random)
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

            List<TileObjectData> doorsToPlace = new();
            foreach (var door in Type.Doors)
            {
                for (int i = 0; i < door.Amount; i++) doorsToPlace.Add(door);
            }
            List<Vector3Int> iterPositions = possiblePosition.OrderBy(x => random.Next()).ToList();

            if (doorsToPlace.Count > 0)
            {
                Tilemap tilemap = tilegen.Tilemap[(int) TilegenType.Objects];
                List<TileObjectData> priority = new();
                foreach (var pos in iterPositions)
                {
                    // if (floorTilemap.GetTile(pos) != null) continue;

                    TileObjectData door = null;
                    if (priority.Count > 0)
                    {
                        door = priority[0];
                        priority.RemoveAt(0);
                    }
                    else if (doorsToPlace.Count > 0)
                    {
                        int index = random.Next(0, doorsToPlace.Count);
                        door = doorsToPlace[index];
                        doorsToPlace.RemoveAt(index);
                    }
                    if (door != null)
                    {
                        if (random.Next(0, 100) < door.Chance)
                        {
                            possiblePosition.Remove(pos);
                            availablePositions.Remove(pos);
                            TileBase tile = door.Tile[random.Next(door.Tile.Count)];
                            tilemap.SetTile(pos, tile);
                            PlacedDoors.Add(new TileObject(tile, pos));
                            priority.AddRange(door.Recursion);
                            // Tilegen.CollisionMap[pos.x, pos.y] = true;
                        }
                    }
                }
            }

            List<TileObjectData> wallsToPlace = new();
            foreach (var wall in Type.Walls)
            {
                for (int i = 0; i < (possiblePosition.Count * (wall.Amount / 100)); i++) wallsToPlace.Add(wall);
            }
            iterPositions = possiblePosition.OrderBy(x => random.Next()).ToList();

            if (wallsToPlace.Count > 0)
            {
                Tilemap tilemap = tilegen.Tilemap[(int) TilegenType.Terrain];
                List<TileObjectData> priority = new();
                foreach (var pos in iterPositions)
                {
                    // if (floorTilemap.GetTile(pos) != null) continue;

                    TileObjectData wall = null;
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
                            PlacedWalls.Add(new TileObject(tile, pos));
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
            List<TileObjectData> toPlace = new();
        }

        public List<Vector3Int> PlaceObjects(Tilegen tilegen, List<Vector3Int> availablePositions, System.Random random)
        {
            List<TileObjectData> toPlace = new();
            List<Vector3Int> possiblePosition = new();

            if (Type.Objects.Count > 0)
            {
                Tilemap tilemap = tilegen.Tilemap[(int) TilegenType.Objects];
                foreach (var pos in availablePositions)
                {
                    possiblePosition.Add(pos);
                }

                List<Vector3Int> iterPositions = possiblePosition.OrderBy(x => random.Next()).ToList();
                foreach (var obj in Type.Objects)
                {
                    for (int i = 0; i < obj.Amount; i++) toPlace.Add(obj);
                }

                List<TileObjectData> priority = new();
                foreach (var pos in iterPositions)
                {
                    if (tilemap.GetTile(pos) != null) continue;

                    TileObjectData obj = null;
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
                            PlacedObjects.Add(new TileObject(tile, pos));
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