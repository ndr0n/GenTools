using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenRoomNode
    {
        public Vector3Int Position = Vector3Int.zero;
        public GameObject Floor = null;
        public GameObject Roof = null;
        public GameObject Object = null;
        public List<GameObject> Wall = new() {null, null, null, null};

        public GenRoomNode(Vector3Int position)
        {
            Position = position;
            Floor = null;
            Roof = null;
            Object = null;
            Wall = new() {null, null, null, null};
        }

        public void Clear()
        {
            Position = Vector3Int.zero;
            if (Floor != null) UnityEngine.Object.DestroyImmediate(Floor);
            if (Roof != null) UnityEngine.Object.DestroyImmediate(Floor);
            if (Object != null) UnityEngine.Object.DestroyImmediate(Floor);
            foreach (var w in Wall)
            {
                if (w != null) UnityEngine.Object.DestroyImmediate(w);
            }
        }
    }
}