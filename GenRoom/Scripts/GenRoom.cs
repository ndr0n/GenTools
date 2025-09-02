using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenRoom : MonoBehaviour
    {
        public Transform Content;
        public GenRoomType Type;
        public int GenerationDelayMilliseconds = 0;

        public bool RandomSeed = false;
        public int Seed = 0;

        public Vector3 TileSize = new Vector3(4, 4, 4);
        public Vector3Int GridSize = new Vector3Int(5, 1, 5);

        public Vector2Int OuterDoorAmount = new Vector2Int(2, 2);

        public readonly List<GenRoomNode> Floor = new();
        public readonly List<GenRoomNode> OuterWall = new();
        public readonly List<GenRoomNode> OuterDoor = new();
        public readonly List<GenRoomNode> Roof = new();
        public readonly List<GenRoomNode> InnerDoor = new();
        public readonly List<GenRoomNode> InnerWall = new();

        GenRoomPreset preset = null;
        System.Random random = null;

        public void Clear()
        {
            if (Content != null)
            {
                for (int i = Content.childCount; i-- > 0;)
                {
                    DestroyImmediate(Content.GetChild(i).gameObject);
                }
            }

            Floor.Clear();
            OuterDoor.Clear();
            OuterWall.Clear();
            Roof.Clear();
            InnerDoor.Clear();
            InnerWall.Clear();

            if (Content == null)
            {
                Content = GenTools.CreateGameObject("Content", transform).transform;
                Content.transform.localPosition += new Vector3(TileSize.x / 2f, 0, TileSize.z / 2f);
            }
        }

        public async Awaitable Generate()
        {
            Clear();
            if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
            random = new System.Random(Seed);
            preset = Type.Presets[random.Next(Type.Presets.Count)];

            Floor.AddRange(await GenRoomLibrary.BuildFloor(this, random, preset));
            OuterWall.AddRange(await GenRoomLibrary.BuildOuterWalls(this, random, preset));
            OuterDoor.AddRange(await GenRoomLibrary.BuildOuterDoors(this, random, preset, random.Next(OuterDoorAmount.x, OuterDoorAmount.y + 1)));
        }

        public async Awaitable Await()
        {
            if (GenerationDelayMilliseconds > 0)
            {
                await Task.Delay(GenerationDelayMilliseconds);
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(GenRoom))]
    public class GenRoom_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenRoom genRoom = (GenRoom) target;
            if (GUILayout.Button("Clear")) genRoom.Clear();
            if (GUILayout.Button("Generate")) genRoom.Generate();
        }
    }
#endif
}