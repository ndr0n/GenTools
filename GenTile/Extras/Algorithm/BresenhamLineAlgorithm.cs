using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenTools
{
    public static class BresenhamLine
    {
        public static List<Vector2Int> Compute(Vector2Int start, Vector2Int end)
        {
            List<Vector2Int> positions = new();
            int x = start.x, y = start.y;
            int dx = Mathf.Abs(end.x - start.x), dy = Mathf.Abs(end.y - start.y);
            int sx = start.x < end.x ? 1 : -1, sy = start.y < end.y ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                positions.Add(new Vector2Int(x, y));
                if (x == end.x && y == end.y)
                {
                    break;
                }
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
            return positions;
        }
    }
}