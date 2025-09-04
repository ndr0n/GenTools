using UnityEngine;

namespace GenTools
{
    public enum CardinalDirection
    {
        South = 0,
        West = 1,
        North = 2,
        East = 3,
        SouthWest = 4,
        NorthWest = 5,
        NorthEast = 6,
        SouthEast = 7,
    }

    public static class GenTools
    {
        public static GameObject CreateGameObject(string name, Transform parent)
        {
            Transform obj = new GameObject(name).transform;
            obj.parent = parent;
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;
            return obj.gameObject;
        }

        public static CardinalDirection GetDirection(Vector2Int point1, Vector2Int point2)
        {
            CardinalDirection dir = CardinalDirection.SouthWest;
            Vector2Int direction = (point2 - point1);
            if (direction == Vector2Int.up) dir = CardinalDirection.North;
            else if (direction == Vector2Int.right) dir = CardinalDirection.East;
            else if (direction == Vector2Int.down) dir = CardinalDirection.South;
            else if (direction == Vector2Int.left) dir = CardinalDirection.West;
            return dir;
        }

        public static Vector3 ClosestPointBetween(GenTileRoom origin, GenTileRoom connection)
        {
            float oDistance = float.MaxValue;
            Vector3 connectionCenter = connection.GetCenter();
            Vector3 closestPoint = Vector3.zero;
            for (int x = origin.Position.x; x < (origin.Position.x + origin.Size.x); x++)
            {
                for (int y = origin.Position.y; y < (origin.Position.y + origin.Size.y); y++)
                {
                    Vector3 oPoint = new Vector3(x, y, 0);
                    float d = Vector3.Distance(oPoint, connectionCenter);
                    if (d < oDistance)
                    {
                        oDistance = d;
                        closestPoint = oPoint;
                    }
                }
            }
            return closestPoint;
        }
    }
}