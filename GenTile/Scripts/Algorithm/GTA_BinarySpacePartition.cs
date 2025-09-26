using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public class GTA_BinarySpacePartition : GenTileAlgo
    {
        public Vector2Int Width = new Vector2Int(8, 12);
        public Vector2Int Height = new Vector2Int(8, 12);
        public Vector2Int Chance = new Vector2Int(100, 100);
        public Vector2Int Offset = new Vector2Int(0, 0);

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            List<Vector2Int> placed = new();
            System.Random random = new(seed);
            List<Vector2Int> positions = available.OrderBy(x => random.Next()).ToList();
            if (positions.Count == 0) return placed;
            placed = BinarySpacePartition(random, positions, Width, Height, Chance, Offset);
            return placed;
        }

        public static List<Vector2Int> BinarySpacePartition(System.Random random, List<Vector2Int> positions, Vector2Int width, Vector2Int height, Vector2Int chance, Vector2Int Offset)
        {
            int of7 = random.Next(Offset.x, Offset.y + 1);

            Vector2Int min = positions[0];
            Vector2Int max = positions[0];

            foreach (var val in positions)
            {
                if (val.x < min.x) min.x = val.x;
                if (val.y < min.y) min.y = val.y;
                if (val.x > max.x) max.x = val.x;
                if (val.y > max.y) max.y = val.y;
            }

            BoundsInt spaceToSplit = new(new Vector3Int(min.x, min.y, 0), new Vector3Int(max.x, max.y, 0));
            Queue<BoundsInt> roomsQueue = new();
            List<BoundsInt> roomsList = new();
            roomsQueue.Enqueue(spaceToSplit);
            while (roomsQueue.Count > 0)
            {
                var room = roomsQueue.Dequeue();
                int minWidth = random.Next(width.x, width.y);
                int minHeight = random.Next(height.x, height.y);
                if (room.size.x >= minWidth && room.size.y >= minHeight)
                {
                    bool splitHorizontal = (random.Next(0, 2) == 0);
                    if (splitHorizontal)
                    {
                        if (room.size.y >= (minHeight * 2)) SplitHorizontally(random, minHeight, roomsQueue, room);
                        else if (room.size.x >= (minWidth * 2)) SplitVertically(random, minWidth, roomsQueue, room);
                        else if (room.size.x >= minWidth && room.size.y >= minHeight) roomsList.Add(room);
                    }
                    else
                    {
                        if (room.size.x >= (minWidth * 2)) SplitVertically(random, minWidth, roomsQueue, room);
                        else if (room.size.y >= (minHeight * 2)) SplitHorizontally(random, minHeight, roomsQueue, room);
                        else if (room.size.x >= minWidth && room.size.y >= minHeight) roomsList.Add(room);
                    }
                }
            }
            List<Vector2Int> placed = new();
            foreach (var bounds in roomsList)
            {
                if (random.Next(0, 100) < random.Next(chance.x, chance.y))
                {
                    for (int x = bounds.position.x + of7; x < bounds.position.x + bounds.size.x - of7 - 1; x++)
                    {
                        for (int y = bounds.position.y + of7; y < bounds.position.y + bounds.size.y - of7 - 1; y++)
                        {
                            Vector2Int pos = new Vector2Int(x, y);
                            if (positions.Contains(pos))
                            {
                                placed.Add(pos);
                                positions.Remove(pos);
                            }
                        }
                    }
                }
            }
            return placed;
        }

        static void SplitHorizontally(System.Random random, int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
        {
            var ySplit = random.Next(minHeight, room.size.y - 1);
            BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
            BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z), new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
            roomsQueue.Enqueue(room1);
            roomsQueue.Enqueue(room2);
        }

        static void SplitVertically(System.Random random, int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
        {
            var xSplit = random.Next(minWidth, room.size.x - 1);
            BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
            BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z), new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
            roomsQueue.Enqueue(room1);
            roomsQueue.Enqueue(room2);
        }
    }
}