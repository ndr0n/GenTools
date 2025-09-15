using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace GenTools
{
    public class GenTileTunneler
    {
        Random random = new();
        int lifetime = 0;
        int change = 0;
        Vector2Int width;
        BoundsInt bounds;
        public List<Vector3Int> tunnelPositions = new();

        Vector3Int pos;
        Vector3Int initPosition;
        // Vector3Int finalPosition;

        Vector3Int direction;
        Vector3Int initDirection;
        List<Vector3Int> possibleDirections = new List<Vector3Int>() {Vector3Int.down, Vector3Int.left, Vector3Int.up, Vector3Int.right};
        List<Vector3Int> targetPositions = new();

        public List<Vector3Int> Init(int seed, BoundsInt _bounds, int _lifetime, int _change, Vector2Int _width, List<Vector3Int> _existingPositions)
        {
            Debug.Log($"TUNNELER BOUNDS: position: {_bounds.position} | size: {_bounds.size}");
            random = new Random(seed);
            lifetime = _lifetime;
            change = _change;
            width = _width;
            bounds = _bounds;
            tunnelPositions = _existingPositions.ToList();
            tunnelPositions = Initialize();
            tunnelPositions = Execute();
            return tunnelPositions;
        }

        public List<Vector3Int> Initialize()
        {
            targetPositions = new();
            tunnelPositions = new();
            switch (random.Next(0, 4))
            {
                case 0:
                    initPosition = new Vector3Int(random.Next(bounds.min.x, bounds.max.x), bounds.min.y);
                    // finalPosition = new Vector3Int(bounds.max.x - initPosition.x, bounds.max.y - 1);
                    targetPositions.Add(new Vector3Int(bounds.max.x - initPosition.x, bounds.max.y - 1));
                    targetPositions.Add(new Vector3Int(bounds.min.x, random.Next(bounds.min.y, bounds.max.y)));
                    targetPositions.Add(new Vector3Int(bounds.max.x - targetPositions[^1].x, bounds.min.y));
                    initDirection = Vector3Int.up;
                    break;
                case 1:
                    initPosition = new Vector3Int(random.Next(bounds.min.x, bounds.max.x), bounds.max.y - 1);
                    // finalPosition = new Vector3Int(bounds.max.x - initPosition.x, bounds.min.y);
                    targetPositions.Add(new Vector3Int(bounds.max.x - initPosition.x, bounds.min.y));
                    targetPositions.Add(new Vector3Int(bounds.min.x, random.Next(bounds.min.y, bounds.max.y)));
                    targetPositions.Add(new Vector3Int(bounds.max.x - targetPositions[^1].x, bounds.min.y));
                    initDirection = Vector3Int.down;
                    break;
                case 2:
                    initPosition = new Vector3Int(bounds.min.x, random.Next(bounds.min.y, bounds.max.y));
                    // finalPosition = new Vector3Int(bounds.max.x - 1, bounds.max.y - initPosition.y);
                    targetPositions.Add(new Vector3Int(bounds.max.x - 1, bounds.max.y - initPosition.y));
                    targetPositions.Add(new Vector3Int(random.Next(bounds.min.x, bounds.max.x), bounds.min.y));
                    targetPositions.Add(new Vector3Int(bounds.max.x - targetPositions[^1].x, bounds.max.y - 1));
                    initDirection = Vector3Int.right;
                    break;
                case 3:
                    initPosition = new Vector3Int(bounds.max.x - 1, random.Next(bounds.min.y, bounds.max.y));
                    // finalPosition = new Vector3Int(bounds.min.x, bounds.max.y - initPosition.y);
                    targetPositions.Add(new Vector3Int(bounds.min.x, bounds.max.y - initPosition.y));
                    targetPositions.Add(new Vector3Int(random.Next(bounds.min.x, bounds.max.x), bounds.min.y));
                    targetPositions.Add(new Vector3Int(bounds.max.x - targetPositions[^1].x, bounds.max.y - 1));
                    initDirection = Vector3Int.left;
                    break;
            }

            direction = initDirection;
            if (CheckIfPositionAvailable(initPosition))
            {
                initPosition = GetNextPosition(initPosition);
            }
            pos = initPosition;
            tunnelPositions.Add(pos);
            return tunnelPositions;
        }

        public List<Vector3Int> Execute()
        {
            Debug.Log($"TUNNEL INIT - pos: {initPosition} | direction: {direction}");
            for (int i = 0; i < lifetime; i++)
            {
                tunnelPositions = Iterate();
            }
            return tunnelPositions;
        }

        public List<Vector3Int> Iterate()
        {
            Vector3Int futurePosition = pos + direction;
            if (!CheckIfPositionAvailable(futurePosition) || random.Next(0, 100) < change)
            {
                List<Vector3Int> directions = possibleDirections.OrderBy(x => random.Next()).ToList();
                directions.Remove(direction);
                directions.Remove(-direction);
                bool newPosition = false;
                foreach (var dir in directions)
                {
                    Debug.Log($"Try Direction: {dir}.");
                    futurePosition = pos + dir;
                    if (CheckIfPositionAvailable(futurePosition))
                    {
                        direction = dir;
                        newPosition = true;
                        break;
                    }
                }
                if (newPosition == false)
                {
                    Debug.Log($"Tunneler Stuck! pos: {pos} | direction: {direction}");
                    futurePosition = GetNextPosition(pos);
                    if (futurePosition == pos)
                    {
                        Debug.Log($"Tunneler Died!");
                        return tunnelPositions;
                    }
                }
                Debug.Log($"TUNNEL SHIFT - pos: {pos} | direction: {direction}");
            }
            pos = futurePosition;
            if (!tunnelPositions.Contains(pos)) tunnelPositions.Add(pos);
            Debug.Log($"Tunneler Position: {pos}");
            return tunnelPositions;
        }

        bool CheckIfPositionAvailable(Vector3Int position)
        {
            if (position.x < bounds.min.x || position.x >= bounds.max.x || position.y < bounds.min.y || position.y >= bounds.max.y) return false;
            if (tunnelPositions.Contains(position)) return false;
            return true;
        }

        List<Vector3Int> checkedPosiitions = new();

        Vector3Int GetNextPosition(Vector3Int currentPosition)
        {
            Vector3Int checkPosition = currentPosition;
            if (tunnelPositions.Count > 0)
            {
                List<Vector3Int> toCheck = tunnelPositions.OrderBy(x => Vector3Int.Distance(targetPositions[0], x)).ToList();
                foreach (var chkd in checkedPosiitions) toCheck.Remove(chkd);
                if (toCheck.Count > 0)
                {
                    checkPosition = toCheck[0];
                    checkedPosiitions.Add(checkPosition);
                    if (checkPosition == targetPositions[0])
                    {
                        if (targetPositions.Count > 1) targetPositions.RemoveAt(0);
                    }
                }
            }
            List<Vector3Int> possDirections = possibleDirections.OrderBy(x => Vector3Int.Distance(checkPosition + x, targetPositions[0])).ToList();
            for (int i = 0; i < Mathf.Max(bounds.max.x, bounds.max.y); i++)
            {
                foreach (var dir in possDirections)
                {
                    Vector3Int position = checkPosition + (i * dir);
                    if (CheckIfPositionAvailable(position))
                    {
                        direction = dir;
                        return position;
                    }
                }
            }

            return currentPosition;
        }
    }
}