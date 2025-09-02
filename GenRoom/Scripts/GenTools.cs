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
    }
}