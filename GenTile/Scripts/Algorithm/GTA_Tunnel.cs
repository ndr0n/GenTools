using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public class GTA_Tunnel : GenTileAlgo
    {
        public Vector2Int PathWidth = new Vector2Int(1, 3);
        public Vector2Int XBeginPercentage = new Vector2Int(0, 100);
        public Vector2Int XFinishPercentage = new Vector2Int(0, 100);
        public Vector2Int YBeginPercentage = new Vector2Int(0, 100);
        public Vector2Int YFinishPercentage = new Vector2Int(0, 100);

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            List<Vector2Int> placed = new();
            if (available.Count == 0) return placed;

            System.Random random = new System.Random(seed);
            Vector2Int min = available[0];
            Vector2Int max = available[0];
            foreach (var val in available)
            {
                if (val.sqrMagnitude < min.sqrMagnitude) min = val;
                if (val.sqrMagnitude > max.sqrMagnitude) max = val;
            }
            max = max - min;

            int beginx = available.OrderBy(v => Vector2Int.Distance(v, max * random.Next(XBeginPercentage.x, XBeginPercentage.y + 1))).First().x;
            int endx = available.OrderBy(v => Vector2Int.Distance(v, max * random.Next(XFinishPercentage.x, XFinishPercentage.y + 1))).First().x;
            int beginy = available.OrderBy(v => Vector2Int.Distance(v, max * random.Next(YBeginPercentage.x, YBeginPercentage.y + 1))).First().y;
            int endy = available.OrderBy(v => Vector2Int.Distance(v, max * random.Next(YFinishPercentage.x, YFinishPercentage.y + 1))).First().y;

            Vector2Int begin = new Vector2Int(beginx, beginy);
            Vector2Int end = new Vector2Int(endx, endy);

            List<Vector2Int> positions = available.OrderBy(x => random.Next()).ToList();

            Vector2 pos = new Vector2Int(begin.x, begin.y);
            Vector2 destination = new Vector2Int(end.x, end.y);

            for (int i = 0; i < (positions.Count); i++)
            {
                if (Vector2.Distance(pos, destination) > 1)
                {
                    placed.AddRange(Tunnel_Dig(positions, random, PathWidth, pos));
                    pos = Vector2.MoveTowards(pos, destination, 1);
                }
                else break;
            }

            return placed;
        }

        static List<Vector2Int> Tunnel_Dig(List<Vector2Int> positions, System.Random random, Vector2Int pathWidth, Vector2 pos)
        {
            List<Vector2Int> placed = new();
            int tunnelWidth = random.Next(pathWidth.x, pathWidth.y + 1);
            for (int widthIterator = -tunnelWidth; widthIterator <= tunnelWidth; widthIterator++)
            {
                Vector2Int p = new Vector2Int(Mathf.RoundToInt(pos.x + widthIterator), Mathf.RoundToInt(pos.y));
                if (positions.Contains(p))
                {
                    placed.Add(p);
                    positions.Remove(p);
                }
            }
            tunnelWidth = random.Next(pathWidth.x, pathWidth.y + 1);
            for (int heightIterator = -tunnelWidth; heightIterator <= tunnelWidth; heightIterator++)
            {
                Vector2Int p = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y + heightIterator));
                if (positions.Contains(p))
                {
                    placed.Add(p);
                    positions.Remove(p);
                }
            }
            return placed;
        }
    }
}