using UnityEngine;

namespace GenerativeTools
{
    [System.Serializable]
    public enum GenObjectType
    {
        Floor,
        Wall,
        Door,
    }

    [System.Serializable]
    public enum GenObjectPosition
    {
        All,
        Outer,
        Inner,
        Corner,
        Center,
    }

    [System.Serializable]
    public enum GenObjectRotation
    {
        Any,
        Inside,
        Outside,
    }

    [System.Serializable]
    public struct GenObject
    {
        public GameObject Prefab;
        public Vector2Int Amount;
        public GenObjectType Type;
        public GenObjectPosition Position;
        public GenObjectRotation Rotation;
        public Vector3 RandomPositionOffset;

        public GenObject(GameObject prefab, Vector2Int amount, GenObjectType type, GenObjectPosition position, GenObjectRotation rotation, Vector3 randomPositionOffset)
        {
            Prefab = prefab;
            Amount = amount;
            Type = type;
            Position = position;
            Rotation = rotation;
            RandomPositionOffset = randomPositionOffset;
        }

        public Vector3 GetRandomPositionOffset(System.Random random)
        {
            Unity.Mathematics.Random rand = new((uint) random.Next(0, int.MaxValue));
            return new Vector3(rand.NextFloat(-RandomPositionOffset.x, RandomPositionOffset.x), rand.NextFloat(-RandomPositionOffset.y, RandomPositionOffset.y), rand.NextFloat(-RandomPositionOffset.z, RandomPositionOffset.z));
        }
    }
}