using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GenTools
{
    [System.Serializable]
    public struct GenTileLayerData
    {
        public int Layer;
        public List<byte[,]> Map;

        public GenTileLayerData(int layer, int width, int height)
        {
            Layer = layer;
            Map = new();
            for (int i = 0; i < Enum.GetNames(typeof(GenTileType)).Length; i++) Map.Add(new byte[width, height]);
        }
    }

    public class GenTile : MonoBehaviour
    {
        public List<Tilemap> Tilemap = new();
        public List<TilemapRenderer> TilemapRenderer = new();

        public int Width = 100;
        public int Height = 100;

        public bool RandomSeed = false;
        public int Seed = 0;

        public GenTileArea GenTileArea;
        public List<GenTilePreset> Presets = new();

        public List<byte[,]> Map = new();
        public bool[,] CollisionMap = new bool[0, 0];
        public readonly List<TileBase> tiles = new();
        readonly List<GenTileLayerData> layerData = new();

        [HideInInspector] public GenTilePreset Preset;
        System.Random random;

        public void Clear()
        {
            Map.Clear();
            layerData.Clear();
            foreach (var tilemap in Tilemap) tilemap.ClearAllTiles();
            if (GenTileArea != null) GenTileArea.Clear();
        }

        void Init()
        {
            if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
            random = new(Seed);
            Preset = Presets[random.Next(Presets.Count)];

            tiles.Clear();
            tiles.Add(null);
            for (int i = 0; i < Preset.Layer.Count; i++)
            {
                foreach (var iter in Preset.Layer[i].Iterations)
                {
                    if (!tiles.Contains(iter.Tile))
                    {
                        tiles.Add(iter.Tile);
                    }
                }
            }
        }

        void Initialize()
        {
            for (int i = 0; i < Enum.GetNames(typeof(GenTileType)).Length; i++)
            {
                Map.Add(new byte[Width, Height]);
            }

            for (int layer = 0; layer < Preset.Layer.Count; layer++)
            {
                layerData.Add(new GenTileLayerData((layer + 1), Width, Height));
            }
        }

        void Iterate()
        {
            for (int layer = 0; layer < Preset.Layer.Count; layer++)
            {
                List<Vector2Int> availablePositions = new();
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        availablePositions.Add(new Vector2Int(x, y));
                    }
                }
                foreach (var iter in Preset.Layer[layer].Iterations)
                {
                    byte value = (byte) tiles.IndexOf(iter.Tile);
                    int seed = random.Next(int.MinValue, int.MaxValue);
                    int type = (int) iter.Type;
                    List<Vector2Int> placed = iter.Execute(availablePositions, layerData[layer].Map[type], value, seed);
                    foreach (var p in placed) availablePositions.Remove(p);
                }
            }
        }

        void Draw()
        {
            CollisionMap = new bool[Width, Height];
            for (int type = 0; type < Enum.GetNames(typeof(GenTileType)).Length; type++)
            {
                for (int x = 0; x < Map[type].GetLength(0); x++)
                {
                    for (int y = 0; y < Map[type].GetLength(1); y++)
                    {
                        if (CollisionMap[x, y] == false)
                        {
                            int terrainLayer = 0;
                            for (int layer = 0; layer < layerData.Count; layer++)
                            {
                                if (layerData[layer].Map[0][x, y] > 0)
                                {
                                    if (layerData[layer].Layer >= layerData[terrainLayer].Layer) terrainLayer = layer;
                                }
                            }
                            Map[type][x, y] = layerData[terrainLayer].Map[type][x, y];
                            Tilemap[type].SetTile(new Vector3Int(x, y, 0), tiles[Map[type][x, y]]);
                            if (Tilemap[type].GetColliderType(new Vector3Int(x, y, 0)) > 0) CollisionMap[x, y] = true;
                        }
                    }
                }
            }
        }

        void Render(List<byte[,]> map)
        {
            Map = map.ToList();
            for (int type = 0; type < Enum.GetNames(typeof(GenTileType)).Length; type++)
            {
                for (int x = 0; x < map[type].GetLength(0); x++)
                {
                    for (int y = 0; y < map[type].GetLength(1); y++)
                    {
                        Tilemap[type].SetTile(new Vector3Int(x, y, 0), tiles[map[type][x, y]]);
                    }
                }
            }
        }

        public List<byte[,]> Generate()
        {
            Clear();
            Init();
            Initialize();
            Iterate();
            Draw();
            Render(Map);
            if (GenTileArea != null) GenTileArea.PlaceArea();
            return Map.ToList();
        }

        public void RenderMap(List<byte[,]> map)
        {
            Clear();
            Init();
            Render(map);
            if (GenTileArea != null) GenTileArea.PlaceArea();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GenTile))]
    public class GenTile_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenTile genTile = (GenTile) target;
            if (GUILayout.Button("Generate")) genTile.Generate();
            if (GUILayout.Button("Clear")) genTile.Clear();

            // if (genTile.ShowPresetEditor)
            // {
            //     List<GenTilePreset> editorPresets = new();
            //
            //     for (int i = 0; i < genTile.Presets.Count; i++)
            //     {
            //         if (genTile.Presets[i] != null && !editorPresets.Contains(genTile.Presets[i]))
            //         {
            //             EditorGUILayout.LabelField($"{genTile.Presets[i].name}", GUI.skin.horizontalSlider);
            //             EditorGUILayout.LabelField($"{genTile.Presets[i].name}", GUI.skin.label);
            //             editorPresets.Add(genTile.Presets[i]);
            //             CreateEditor(genTile.Presets[i]).DrawDefaultInspector();
            //         }
            //     }
            // }
        }
    }
#endif
}