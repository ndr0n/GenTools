using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenRoomNode
    {
        public GameObject Spawn;
        public Vector3Int Position;

        public GenRoomNode(GameObject spawn, Vector3Int position)
        {
            Spawn = spawn;
            Position = position;
        }
    }
}