using UnityEngine;

namespace GenTools
{
    public enum CardinalDirection
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
        NorthEast = 4,
        SouthEast = 5,
        SouthWest = 6,
        NorthWest = 7,
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