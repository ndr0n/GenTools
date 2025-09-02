using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenRoomNode
    {
        public Vector3Int Position;
        public GameObject Floor;
        public GameObject Roof;
        public GameObject Object;
        public List<GameObject> Wall = new() {null, null, null, null};

        public GenRoomNode(Vector3Int position)
        {
            Position = position;
            Wall = new() {null, null, null, null};
        }
    }
}