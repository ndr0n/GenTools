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

        public readonly List<List<List<GenRoomNode>>> Node = new();
        public readonly List<GameObject> OuterDoor = new();
        public readonly List<GameObject> InnerDoor = new();

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

            if (Content == null)
            {
                Content = GenTools.CreateGameObject("Content", transform).transform;
                Content.transform.localPosition += new Vector3(TileSize.x / 2f, 0, TileSize.z / 2f);
            }
            OuterDoor.Clear();
            InnerDoor.Clear();

            Node.Clear();
            for (int y = 0; y < GridSize.y; y++)
            {
                Node.Add(new());
                for (int x = 0; x < GridSize.x; x++)
                {
                    Node[y].Add(new());
                    for (int z = 0; z < GridSize.z; z++)
                    {
                        Node[y][x].Add(new GenRoomNode(new Vector3Int(x, y, z)));
                    }
                }
            }
        }

        public async Awaitable Generate()
        {
            Clear();
            if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
            random = new System.Random(Seed);
            preset = Type.Presets[random.Next(Type.Presets.Count)];

            await GenRoomLibrary.BuildFloor(this, random, preset);
            await GenRoomLibrary.BuildOuterWalls(this, random, preset);
            await GenRoomLibrary.BuildOuterDoors(this, random, preset, random.Next(OuterDoorAmount.x, OuterDoorAmount.y + 1));
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