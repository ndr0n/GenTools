using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace GenTools
{
    public class GTA_Corridors : GenTileAlgo
    {
        public Vector2Int Lifetime = new Vector2Int(50, 50);
        public Vector2Int ChangePercentage = new Vector2Int(10, 10);
        public Vector2Int TunnelWidth = new Vector2Int(1, 1);

        public override List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            List<Vector2Int> placed = new();
            System.Random random = new(seed);
            lifetime = Mathf.RoundToInt(available.Count * (random.Next(Lifetime.x, Lifetime.y + 1) / 100f));
            changePercentage = random.Next(ChangePercentage.x, ChangePercentage.y + 1);
            positions = available.OrderBy(x => random.Next()).ToList();
            if (positions.Count == 0) return placed;
            placed = Run(random);
            return placed;
        }

        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;
        Vector2Int pos = Vector2Int.zero;
        Vector2Int dir = Vector2Int.zero;
        List<Vector2Int> positions = new();
        int lifetime = 0;
        int changePercentage = 0;

        List<Vector2Int> Run(System.Random random)
        {
            min = positions[0];
            max = positions[0];
            foreach (var val in positions)
            {
                if (val.x < min.x) min.x = val.x;
                if (val.y < min.y) min.y = val.y;
                if (val.x > max.x) max.x = val.x;
                if (val.y > max.y) max.y = val.y;
            }

            List<Vector2Int> placed = new();
            List<Vector2Int> targetPositions = new();
            switch (random.Next(0, 4))
            {
                case 0:
                    pos = new Vector2Int(Mathf.RoundToInt(Mathf.Lerp(max.x, min.x, 0.5f)), min.y);
                    // finalPosition = new Vector3Int(bounds.max.x - initPosition.x, bounds.max.y - 1);
                    targetPositions.Add(new Vector2Int(max.x - pos.x, max.y - 1));
                    targetPositions.Add(new Vector2Int(min.x, Mathf.RoundToInt(Mathf.Lerp(max.y, min.y, 0.5f))));
                    targetPositions.Add(new Vector2Int(max.x - targetPositions[^1].x, min.y));
                    dir = Vector2Int.up;
                    break;
                case 1:
                    pos = new Vector2Int(Mathf.RoundToInt(Mathf.Lerp(max.x, min.x, 0.5f)), max.y - 1);
                    // finalPosition = new Vector3Int(bounds.max.x - initPosition.x, bounds.min.y);
                    targetPositions.Add(new Vector2Int(max.x - pos.x, min.y));
                    targetPositions.Add(new Vector2Int(min.x, Mathf.RoundToInt(Mathf.Lerp(max.y, min.y, 0.5f))));
                    targetPositions.Add(new Vector2Int(max.x - targetPositions[^1].x, min.y));
                    dir = Vector2Int.down;
                    break;
                case 2:
                    pos = new Vector2Int(min.x, Mathf.RoundToInt(Mathf.Lerp(max.y, min.y, 0.5f)));
                    // finalPosition = new Vector3Int(bounds.max.x - 1, bounds.max.y - initPosition.y);
                    targetPositions.Add(new Vector2Int(max.x - 1, max.y - pos.y));
                    targetPositions.Add(new Vector2Int(Mathf.RoundToInt(Mathf.Lerp(max.x, min.x, 0.5f)), min.y));
                    targetPositions.Add(new Vector2Int(max.x - targetPositions[^1].x, max.y - 1));
                    dir = Vector2Int.right;
                    break;
                case 3:
                    pos = new Vector2Int(max.x - 1, Mathf.RoundToInt(Mathf.Lerp(max.y, min.y, 0.5f)));
                    // finalPosition = new Vector3Int(bounds.min.x, bounds.max.y - initPosition.y);
                    targetPositions.Add(new Vector2Int(min.x, max.y - pos.y));
                    targetPositions.Add(new Vector2Int(Mathf.RoundToInt(Mathf.Lerp(max.x, min.x, 0.5f)), min.y));
                    targetPositions.Add(new Vector2Int(max.x - targetPositions[^1].x, max.y - 1));
                    dir = Vector2Int.left;
                    break;
            }
            if (!positions.Contains(pos)) pos = GetNextPosition(pos);
            placed.Add(pos);
            positions.Remove(pos);
            for (int i = 0; i < lifetime; i++) placed.AddRange(Iterate(random));
            return placed;
        }

        public List<Vector2Int> Iterate(System.Random random)
        {
            List<Vector2Int> placed = new();
            Vector2Int futurePosition = pos + dir;
            if (!positions.Contains(futurePosition) || random.Next(0, 100) < changePercentage)
            {
                List<Vector2Int> possibleDirections = new List<Vector2Int>() {Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right};
                possibleDirections = possibleDirections.OrderBy(x => random.Next()).ToList();
                possibleDirections.Remove(dir);
                possibleDirections.Remove(-dir);
                bool newPosition = false;
                foreach (var direction in possibleDirections)
                {
                    futurePosition = pos + direction;
                    if (positions.Contains(futurePosition))
                    {
                        dir = direction;
                        newPosition = true;
                        break;
                    }
                }
                if (newPosition == false)
                {
                    futurePosition = GetNextPosition(pos);
                    if (futurePosition == pos) return placed;
                    placed.Add(futurePosition);
                    positions.Remove(futurePosition);
                }
            }
            pos = futurePosition;
            if (positions.Contains(pos))
            {
                placed.Add(pos);
                positions.Remove(pos);
            }
            return placed;
        }

        Vector2Int GetNextPosition(Vector2Int currentPosition)
        {
            Vector2Int checkPosition = currentPosition;
            if (positions.Count > 0)
            {
                List<Vector2Int> toCheck = positions.OrderBy(x => Vector2Int.Distance(positions[0], x)).ToList();
                if (toCheck.Count > 0)
                {
                    checkPosition = toCheck[0];
                    if (checkPosition == positions[0])
                    {
                        if (positions.Count > 1) positions.RemoveAt(0);
                    }
                }
            }
            List<Vector2Int> possibleDirections = new List<Vector2Int>() {Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right};
            possibleDirections = possibleDirections.OrderBy(x => Vector2Int.Distance(checkPosition + x, positions[0])).ToList();
            for (int i = 0; i < Mathf.Max(max.x, max.y); i++)
            {
                foreach (var direction in possibleDirections)
                {
                    Vector2Int position = checkPosition + (i * direction);
                    if (positions.Contains(position))
                    {
                        dir = direction;
                        return position;
                    }
                }
            }

            return currentPosition;
        }
    }
}