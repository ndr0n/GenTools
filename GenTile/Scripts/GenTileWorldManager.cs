using System.Collections.Generic;
using System.Linq;
using GenTools;
using MindTheatre;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenTileWorldManager : MonoBehaviour
    {
        public static GenTileWorldManager Instance;

        [Header("GenTile")]
        public Grid Grid;
        public GenTile GenTile;
        public bool RenderAdjacentTiles = false;
        public readonly List<GenTile> Adjacent = new List<GenTile>();

        [Header("World")]
        public GenTileWorld World;
        public Vector3Int WorldPosition = new Vector3Int(0, 0, 0);
        public bool WorldEditor;
        public GenTileWorldArea WorldArea;
        public bool AreaEditor;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ClearWorld()
        {
            GenTile.Clear();
            foreach (var adj in Adjacent)
            {
                if (adj != null) DestroyImmediate(adj.gameObject);
            }
            Adjacent.Clear();
        }

        public void Rebuild(GenTileWorld world, Vector3Int worldPosition)
        {
            GenTileWorldArea worldArea = world.Map.FirstOrDefault(x => x.WorldPosition == worldPosition);
            worldArea.Map = null;
            Build(world, worldPosition);
        }

        public virtual void Build(GenTileWorld worldMap, Vector3Int worldPosition)
        {
            WorldPosition = worldPosition;
            ClearWorld();
            if (World.Map.Count == 0) World.Generate();

            GenTile.RandomSeed = false;
            GenTile.Width = worldMap.WorldAreaSize.x;
            GenTile.Height = worldMap.WorldAreaSize.z;
            GenTile.transform.position = Vector3.zero;

            WorldArea = LoadWorldPosition(World, WorldPosition);
        }

        public GenTileWorldArea LoadWorldPosition(GenTileWorld world, Vector3Int worldPosition)
        {
            GenTileWorldArea worldWorldArea = world.Map.FirstOrDefault(x => x.WorldPosition == worldPosition);
            if (worldWorldArea == null)
            {
                Debug.LogError($"World Position {worldPosition} has no Area!");
                return null;
            }
            WorldArea = worldWorldArea;
            ClearWorld();

            GenTile loader = Instantiate(GenTile, GenTile.transform.parent);
            loader.transform.position = GenTile.transform.position;
            loader.transform.rotation = GenTile.transform.rotation;

            WorldArea.Load(GenTile);

            List<Vector3> adjacentPositions = new()
            {
                new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0),
                new Vector3(-1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, -1),
                new Vector3(0, 1, 0), new Vector3(0, -1, 0)
            };

            foreach (var adj in adjacentPositions)
            {
                GenTileWorldArea adjacentWorldArea = world.Map.FirstOrDefault(x => x.WorldPosition == worldPosition + adj);
                if (adjacentWorldArea != null)
                {
                    GenTile genTile = loader;
                    if (RenderAdjacentTiles)
                    {
                        genTile = Instantiate(loader, GenTile.transform.parent);
                        genTile.transform.position = GenTile.transform.position + (new Vector3(GenTile.Width * adj.x, GenTile.Height * adj.z, adj.y));
                        genTile.transform.rotation = GenTile.transform.rotation;
                        genTile.name = $"Adjacent-{adj}";
                        Adjacent.Add(genTile);
                    }
                    adjacentWorldArea.Load(genTile);
                    Debug.Log($"Loaded AdjacentPosition: {worldPosition}");
                }
            }

            DestroyImmediate(loader.gameObject);

            Debug.Log($"Loaded WorldPosition: {worldPosition}");
            return WorldArea;
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
            if (GUILayout.Button("Build")) genTileWorldManager.Build(genTileWorldManager.World, genTileWorldManager.WorldPosition);
            if (GUILayout.Button("Rebuild")) genTileWorldManager.Rebuild(genTileWorldManager.World, genTileWorldManager.WorldPosition);
            if (GUILayout.Button("Clear")) genTileWorldManager.ClearWorld();
            if (genTileWorldManager.WorldEditor)
            {
                if (genTileWorldManager.World != null) CreateEditor(genTileWorldManager.World).OnInspectorGUI();
            }
            if (genTileWorldManager.AreaEditor)
            {
                if (genTileWorldManager.WorldArea != null) CreateEditor(genTileWorldManager.WorldArea).OnInspectorGUI();
            }
        }
    }
#endif
}