using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GenTools
{
    [System.Serializable]
    public class GenArea : MonoBehaviour
    {
        public Vector3Int Size = new Vector3Int(20, 1, 20);
        public Vector2Int Border = new Vector2Int(1, 1);

        public int Seed = 0;
        public bool RandomSeed = false;

        public GenRoomType Type;
        public GenRoom GenRoomPrefab;
        public GenRoomType InnerRoomType;

        [Header("Source")]
        public GenTile GenTile;

        [Header("Runtime")]
        public GenRoom MainRoom;
        public List<GenRoom> InnerRoom = new();

        GenRoomPreset preset;
        System.Random random;

        public void Clear()
        {
            foreach (var room in InnerRoom)
            {
                if (room != null) DestroyImmediate(room.gameObject);
            }
            InnerRoom.Clear();
            if (MainRoom != null) DestroyImmediate(MainRoom.gameObject);
        }

        public async Awaitable Generate()
        {
            try
            {
                Clear();
                if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
                random = new(Seed);
                if (GenTile != null)
                {
                    GenTile.Width = (Size.x - Border.x);
                    GenTile.Height = (Size.z - Border.y);
                    await GenerateFromGenTile();
                }
            }
            catch (Exception ex)
            {
                    Debug.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Awaitable GenerateFromGenTile()
        {
            // Generate Main Room
            MainRoom = Instantiate(GenRoomPrefab.gameObject, transform).GetComponent<GenRoom>();
            MainRoom.name = "MainRoom";
            MainRoom.Type = Type;
            MainRoom.RandomSeed = false;
            MainRoom.Seed = random.Next(int.MinValue, int.MaxValue);
            MainRoom.GridSize = Size;
            MainRoom.OuterDoorAmount = Vector2Int.zero;
            await MainRoom.Generate();

            for (int y = 0; y < Size.y; y++)
            {
                // Generate Tile Data
                GenTile.RandomSeed = false;
                GenTile.Seed = random.Next(int.MinValue, int.MaxValue);
                GenTile.Generate();
                // Generate Inner Rooms
                foreach (var tileRoom in GenTile.GenTileRoomPlacer.PlacedRooms)
                {
                    await BuildRoomFromTileRoom(tileRoom, y);
                }
            }
        }

        public async Awaitable BuildRoomFromTileRoom(GenTileRoom tileRoom, int y)
        {
            GenRoom room = Instantiate(GenRoomPrefab.gameObject, transform).GetComponent<GenRoom>();
            room.name = $"InnerRoom-{room.transform.parent.childCount}";
            room.Type = InnerRoomType;
            room.RandomSeed = false;
            room.Seed = random.Next(int.MinValue, int.MaxValue);
            room.GridSize = new Vector3Int(tileRoom.Size.x, 1, tileRoom.Size.y);
            room.transform.localPosition = new Vector3(tileRoom.Position.x * room.TileSize.x, y * room.TileSize.y, tileRoom.Position.y * room.TileSize.z);
            room.transform.localRotation = Quaternion.identity;

            room.transform.localPosition += new Vector3(Border.x * MainRoom.TileSize.x, 0, Border.y * MainRoom.TileSize.z);

            InnerRoom.Add(room);
            await room.Generate();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GenArea))]
    public class GenArea_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenArea genTileAreaBuilder = (GenArea) target;
            if (GUILayout.Button("Generate")) genTileAreaBuilder.Generate();
            if (GUILayout.Button("Clear")) genTileAreaBuilder.Clear();
        }
    }
#endif
}