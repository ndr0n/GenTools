using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace GenTools
{
    public class GenTileRoomer
    {
        System.Random random = new();
        BoundsInt bounds;
        Vector2Int chance;
        Vector2Int width;
        Vector2Int height;
        List<BoundsInt> rooms = new();
        List<Vector3Int> existingPositions = new();
        List<Vector3Int> possibleDirections = new List<Vector3Int>() {Vector3Int.down, Vector3Int.left, Vector3Int.up, Vector3Int.right};

        public List<BoundsInt> Execute(int seed, BoundsInt _bounds, Vector2Int _chance, Vector2Int _width, Vector2Int _height, List<Vector3Int> _existingPositions, List<BoundsInt> _rooms)
        {
            random = new Random(seed);
            bounds = _bounds;
            chance = _chance;
            width = _width;
            height = _height;
            existingPositions = _existingPositions.ToList();
            rooms = _rooms.ToList();
            rooms = TryPlaceRooms();
            return rooms;
        }

        public List<BoundsInt> TryPlaceRooms()
        {
            // List<Vector3Int> sizeiter = new();
            // for (int x = width.x; x <= width.y; x++)
            // {
            // for (int y = height.x; y <= height.y; y++)
            // {
            // sizeiter.Add(new Vector3Int(x, y, 0));
            // }
            // }
            // sizeiter = sizeiter.OrderBy(x => random.Next()).ToList();

            List<Vector3Int> checkedPositions = new();
            foreach (var pos in existingPositions)
            {
                if (random.Next(0, 100) < (random.Next(chance.x, chance.y)))
                {
                    foreach (var dir in possibleDirections.OrderBy(x => random.Next()))
                    {
                        bool breakLoop = false;
                        Vector3Int position = pos + dir;
                        if (checkedPositions.Contains(position)) continue;
                        checkedPositions.Add(position);
                        for (int sizex = width.y; sizex >= width.x; sizex--)
                        {
                            for (int sizey = height.y; sizey >= height.x; sizey--)
                            {
                                Vector3Int size = new Vector3Int(sizex, sizey, 0);
                                if (CanPlaceRoom(size, position))
                                {
                                    rooms = PlaceRoom(size, position);
                                    breakLoop = true;
                                    break;
                                }
                            }
                            if (breakLoop) break;
                        }
                        if (breakLoop) break;
                    }
                }
            }

            return rooms;
        }

        bool CanPlaceRoom(Vector3Int size, Vector3Int position)
        {
            for (int x = position.x; x < position.x + size.x; x++)
            {
                for (int y = position.y; y < position.y + size.y; y++)
                {
                    if (CheckIfPositionAvailable(new Vector3Int(x, y, 0)) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        List<BoundsInt> PlaceRoom(Vector3Int size, Vector3Int position)
        {
            BoundsInt roomBounds = new BoundsInt(position, size);
            rooms.Add(roomBounds);
            return rooms;
        }

        int offset = 1;

        public bool CheckIfPositionAvailable(Vector3Int position)
        {
            if (position.x < bounds.min.x || position.x >= bounds.max.x || position.y < bounds.min.y || position.y >= bounds.max.y) return false;
            if (existingPositions.Contains(position)) return false;
            foreach (var room in rooms)
            {
                if (position.x >= (room.min.x - offset) && position.x <= (room.max.x + offset) && position.y >= (room.min.y - offset) && position.y <= (room.max.y + offset))
                {
                    // Debug.Log($"EXISTING POSITIONS: {existingPositions.Count}");
                    return false;
                }
            }

            return true;
        }
    }
}