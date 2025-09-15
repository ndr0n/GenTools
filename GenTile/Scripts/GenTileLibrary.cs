using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace GenTools
{
    public static class GenTileLibrary
    {
        #region BinarySpacePartition

        public static List<BoundsInt> BinarySpacePartition(System.Random random, BoundsInt spaceToSplit, Vector2Int width, Vector2Int height)
        {
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
            return roomsList;
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

        #endregion

        #region Tunneler

        #endregion
    }
}