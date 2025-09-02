using System.Collections.Generic;
using UnityEngine;

namespace GenTools
{
    [System.Serializable]
    public class GenArea : MonoBehaviour
    {
        public GenRoom MainRoom;
        public GenRoom GenRoomPrefab;
        public List<GenRoom> Rooms = new();

        GenRoomPreset preset;
        System.Random random;

        public void Clear()
        {
            foreach (var room in Rooms)
            {
                if (room != null) DestroyImmediate(room.gameObject);
            }
            Rooms.Clear();
        }

        // public async Awaitable Generate()
        // {
        // await MainRoom.Generate();
        // }
    }
}