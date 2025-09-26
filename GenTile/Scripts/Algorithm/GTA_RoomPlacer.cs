using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public class GTA_RoomPlacer : GenTileAlgo
    {
        public Vector2Int Chance = new Vector2Int(100, 100);
        public Vector2Int Width = new Vector2Int(4, 20);
        public Vector2Int Height = new Vector2Int(4, 20);

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            System.Random random = new(seed);
            List<Vector2Int> placed = TryPlaceRooms(available.OrderBy(x => random.Next()).ToList(), random);
            return placed;
        }

        public List<Vector2Int> TryPlaceRooms(List<Vector2Int> positions, System.Random random)
        {
            List<Vector2Int> placed = new();
            List<Vector2Int> checkedPositions = new();
            List<Vector2Int> possibleDirections = new List<Vector2Int>() {Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right};
            foreach (var pos in positions.ToList())
            {
                if (random.Next(0, 100) < (random.Next(Chance.x, Chance.y)))
                {
                    foreach (var dir in possibleDirections.OrderBy(x => random.Next()))
                    {
                        bool breakLoop = false;
                        Vector2Int position = pos + dir;
                        if (checkedPositions.Contains(position)) continue;
                        checkedPositions.Add(position);
                        for (int sizex = Width.y; sizex >= Width.x; sizex--)
                        {
                            for (int sizey = Height.y; sizey >= Height.x; sizey--)
                            {
                                Vector2Int size = new Vector2Int(sizex, sizey);
                                if (CanPlaceRoom(positions, placed, size, position))
                                {
                                    for (int x = position.x; x < position.x + size.x; x++)
                                    {
                                        for (int y = position.y; y < position.y + size.y; y++)
                                        {
                                            Vector2Int roomPosition = new Vector2Int(x, y);
                                            placed.Add(roomPosition);
                                            positions.Remove(roomPosition);
                                        }
                                    }
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
            return placed;
        }

        bool CanPlaceRoom(List<Vector2Int> positions, List<Vector2Int> placed, Vector2Int size, Vector2Int position)
        {
            for (int x = position.x; x < position.x + size.x; x++)
            {
                for (int y = position.y; y < position.y + size.y; y++)
                {
                    if (CheckIfPositionAvailable(positions, placed, new Vector2Int(x, y)) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        int offset = 1;

        public bool CheckIfPositionAvailable(List<Vector2Int> positions, List<Vector2Int> placed, Vector2Int position)
        {
            if (!positions.Contains(position)) return false;
            foreach (var point in placed)
            {
                if (position.x >= (point.x - offset) && position.x <= (point.x + offset) && position.y >= (point.y - offset) && position.y <= (point.y + offset)) return false;
            }
            return true;
        }
    }
}