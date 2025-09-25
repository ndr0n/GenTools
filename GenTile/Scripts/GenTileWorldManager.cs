using System.Collections.Generic;
using System.Linq;
using GenTools;
using MindTheatre;
using UnityEditor;
using UnityEngine;

namespace GenTools
{
    [System.Serializable]
    public class GenTileWorldManager : MonoBehaviour
    {
        public static GenTileWorldManager Instance;

        [Header("GenTile")]
        public Grid Grid;
        public GenTile GenTile;
        public GenTile GenTileLoader;
        public bool RenderAdjacentTiles = false;
        public readonly List<GenTile> Adjacent = new List<GenTile>();

        [Header("World")]
        public GenTileWorld World;
        public Vector3Int WorldPosition = new Vector3Int(0, 0, 0);
        public bool WorldEditor;
        public GenTileArea Area;
        public bool AreaEditor;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ClearWorld()
        {
            GenTile.Clear();
            GenTileLoader.Clear();
            GenTileLoader.gameObject.SetActive(false);
            foreach (var edge in Adjacent) DestroyImmediate(edge.gameObject);
            Adjacent.Clear();
        }

        public virtual void InitWorld(GenTileWorld worldMap)
        {
            ClearWorld();
            if (World.Map.Count == 0) World.Generate();

            GenTile.RandomSeed = false;
            GenTile.Width = worldMap.WorldAreaSize.x;
            GenTile.Height = worldMap.WorldAreaSize.z;
            GenTile.transform.position = Vector3.zero;
            GenTileLoader.RandomSeed = false;
            GenTileLoader.Width = worldMap.WorldAreaSize.x;
            GenTileLoader.Height = worldMap.WorldAreaSize.z;
            GenTileLoader.transform.position = Vector3.zero;

            Area = LoadWorldPosition(World, WorldPosition);
        }

        public GenTileArea LoadWorldPosition(GenTileWorld world, Vector3Int worldPosition)
        {
            GenTileArea worldArea = world.Map.FirstOrDefault(x => x.WorldPosition == worldPosition);
            if (worldArea == null)
            {
                Debug.LogError($"World Position {worldPosition} has no Area!");
                return null;
            }
            Area = worldArea;
            ClearWorld();
            Area.Load(GenTile);

            List<Vector3> adjacentPositions = new()
            {
                new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0),
                new Vector3(-1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, -1),
                new Vector3(0, 1, 0), new Vector3(0, -1, 0)
            };

            GenTileLoader.gameObject.SetActive(true);
            foreach (var adj in adjacentPositions)
            {
                GenTileArea adjacentArea = world.Map.FirstOrDefault(x => x.WorldPosition == worldPosition + adj);
                if (adjacentArea != null)
                {
                    GenTile genTile = GenTileLoader;
                    if (RenderAdjacentTiles)
                    {
                        genTile = Instantiate(GenTileLoader, GenTile.transform.parent);
                        genTile.transform.position = GenTile.transform.position + (new Vector3(GenTile.Width * adj.x, GenTile.Height * adj.z, adj.y));
                        genTile.transform.rotation = GenTile.transform.rotation;
                        genTile.name = $"Adjacent-{adj}";
                        Adjacent.Add(genTile);
                    }
                    adjacentArea.Load(genTile);
                    Debug.Log($"Loaded AdjacentPosition: {worldPosition}");
                }
            }
            GenTileLoader.gameObject.SetActive(false);
            Debug.Log($"Loaded WorldPosition: {worldPosition}");
            return Area;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(GenTileWorldManager))]
    public class GenTileWorldManager_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenTileWorldManager genTileWorldManager = (GenTileWorldManager) target;
            if (GUILayout.Button("Build")) genTileWorldManager.InitWorld(genTileWorldManager.World);
            if (GUILayout.Button("Clear")) genTileWorldManager.ClearWorld();
            if (genTileWorldManager.WorldEditor)
            {
                if (genTileWorldManager.World != null) CreateEditor(genTileWorldManager.World).OnInspectorGUI();
            }
            if (genTileWorldManager.AreaEditor)
            {
                if (genTileWorldManager.Area != null) CreateEditor(genTileWorldManager.Area).OnInspectorGUI();
            }
        }
    }
#endif
}