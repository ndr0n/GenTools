using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Modules.GenTools.GenArea.Scripts
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
        Floor,
        Wall,
        Inner,
        Roof,
    }

    [System.Serializable]
    public enum GenObjectRotation
    {
        Any,
        Inside,
        Outside,
    }

    [System.Serializable]
    public struct GenObjectIteration
    {
        public Vector2Int Amount;
        public Vector2Int Percentage;
        public List<GenObject> Objects;
    }

    [System.Serializable]
    public struct GenObjectData
    {
        public List<GameObject> Prefabs;
        public GenObjectPosition Position;
        public Vector3 RandomPositionOffset;
        public Vector3Int Size;
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "GO", menuName = "GenTools/GenObject")]
    public class GenObject : ScriptableObject
    {
        public GenObjectData Object;
        public List<GenObjectData> Nearby = new();

        public Vector3 GetRandomPositionOffset(System.Random random)
        {
            Unity.Mathematics.Random rand = new((uint) random.Next(0, int.MaxValue));
            return new Vector3(rand.NextFloat(-Object.RandomPositionOffset.x, Object.RandomPositionOffset.x), rand.NextFloat(-Object.RandomPositionOffset.y, Object.RandomPositionOffset.y), rand.NextFloat(-Object.RandomPositionOffset.z, Object.RandomPositionOffset.z));
        }
    }
}