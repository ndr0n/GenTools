using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GenTools
{
    [System.Serializable]
    public struct TilegenLayerData
    {
        public int Layer;
        public List<byte[,]> Map;

        public TilegenLayerData(int layer, int width, int height)
        {
            Layer = layer;
            Map = new();
            for (int i = 0; i < Enum.GetNames(typeof(TilegenType)).Length; i++) Map.Add(new byte[width, height]);
        }
    }

    public class Tilegen : MonoBehaviour
    {
        public List<Tilemap> Tilemap = new();

        public int Width = 100;
        public int Height = 100;

        public bool RandomSeed = false;
        public int Seed = 0;

        public TileRoomPlacer TileRoomPlacer;

        public List<TilegenPreset> Presets = new();

        public bool ShowPresetEditor = false;

        public List<byte[,]> Map = new();
        public bool[,] CollisionMap = new bool[0, 0];
        readonly List<TileBase> tiles = new();
        readonly List<TilegenLayerData> layerData = new();

        TilegenPreset preset;
        System.Random random;

        public void Clear()
        {
            foreach (var tilemap in Tilemap) tilemap.ClearAllTiles();
            tiles.Clear();

            if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
            random = new(Seed);
            preset = Presets[random.Next(Presets.Count)];

            tiles.Add(null);
            for (int i = 0; i < preset.Layer.Count; i++)
            {
                foreach (var iter in preset.Layer[i].Iterations)
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
            Map.Clear();
            layerData.Clear();

            for (int i = 0; i < Enum.GetNames(typeof(TilegenType)).Length; i++)
            {
                Map.Add(new byte[Width, Height]);
            }

            for (int layer = 0; layer < preset.Layer.Count; layer++)
            {
                layerData.Add(new TilegenLayerData((layer + 1), Width, Height));
            }
        }

        void Iterate()
        {
            for (int layer = 0; layer < preset.Layer.Count; layer++)
            {
                foreach (var iter in preset.Layer[layer].Iterations)
                {
                    byte value = (byte) tiles.IndexOf(iter.Tile);
                    int seed = random.Next(int.MinValue, int.MaxValue);
                    int type = (int) iter.Type;

                    foreach (var algorithm in iter.Algorithm)
                    {
                        switch (algorithm.Algorithm)
                        {
                            case TilegenAlgorithmType.Fill:
                                layerData[layer].Map[type] = TilegenAlgorithm.Fill(layerData[layer].Map[type], value, seed, algorithm.FillPercentage);
                                break;
                            case TilegenAlgorithmType.Degrade:
                                layerData[layer].Map[type] = TilegenAlgorithm.Degrade(layerData[layer].Map[type], value, seed, algorithm.DegradePercentage);
                                break;
                            case TilegenAlgorithmType.RandomWalk:
                                layerData[layer].Map[type] = TilegenAlgorithm.RandomWalk(layerData[layer].Map[type], value, seed, algorithm.Size);
                                break;
                            case TilegenAlgorithmType.PerlinNoise:
                                layerData[layer].Map[type] = TilegenAlgorithm.PerlinNoise(layerData[layer].Map[type], value, seed, algorithm.PerlinNoiseModifier);
                                break;
                            case TilegenAlgorithmType.Tunnel:
                                layerData[layer].Map[type] = TilegenAlgorithm.Tunnel(layerData[layer].Map[type], value, seed, algorithm.PathWidth, algorithm.XBeginPercent, algorithm.XFinishPercent, algorithm.YBeginPercent, algorithm.YFinishPercent);
                                break;
                            case TilegenAlgorithmType.Rooms:
                                layerData[layer].Map[type] = TilegenAlgorithm.Rooms(layerData[layer].Map[type], value, seed, algorithm.RoomAmount, algorithm.RoomWidth, algorithm.RoomHeight);
                                break;
                            case TilegenAlgorithmType.Walls:
                                layerData[layer].Map[type] = TilegenAlgorithm.Walls(layerData[layer].Map[type], value, seed, algorithm.WallPercentage, algorithm.OuterWall);
                                break;
                            case TilegenAlgorithmType.WaveFunctionCollapse:
                                layerData[layer].Map[type] = TilegenAlgorithm.WFC_Overlapping(layerData[layer].Map[type], value, seed, algorithm.Invert, algorithm.InputTexture, algorithm.N, algorithm.Symmetry, algorithm.Iterations);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        void Draw()
        {
            CollisionMap = new bool[Width, Height];
            for (int type = 0; type < Enum.GetNames(typeof(TilegenType)).Length; type++)
            {
                for (int x = 0; x < Map[type].GetUpperBound(0); x++)
                {
                    for (int y = 0; y < Map[type].GetUpperBound(1); y++)
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
            Map = map;
            for (int type = 0; type < Enum.GetNames(typeof(TilegenType)).Length; type++)
            {
                for (int x = 0; x < map[type].GetUpperBound(0); x++)
                {
                    for (int y = 0; y < map[type].GetUpperBound(1); y++)
                    {
                        Tilemap[type].SetTile(new Vector3Int(x, y, 0), tiles[Map[type][x, y]]);
                    }
                }
            }
        }

        public List<byte[,]> Generate()
        {
            Clear();
            Initialize();
            Iterate();
            Draw();
            Render(Map);
            if (TileRoomPlacer != null) TileRoomPlacer.Generate();
            return Map;
        }

        public List<byte[,]> RenderMap(List<byte[,]> map)
        {
            Map = map;
            Clear();
            // Initialize();
            Render(map);
            return Map;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Tilegen))]
    public class Tilegen_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Tilegen tilegen = (Tilegen) target;
            if (GUILayout.Button("Generate")) tilegen.Generate();
            if (GUILayout.Button("Clear")) tilegen.Clear();

            if (tilegen.ShowPresetEditor)
            {
                List<TilegenPreset> editorPresets = new();

                for (int i = 0; i < tilegen.Presets.Count; i++)
                {
                    if (tilegen.Presets[i] != null && !editorPresets.Contains(tilegen.Presets[i]))
                    {
                        EditorGUILayout.LabelField($"{tilegen.Presets[i].name}", GUI.skin.horizontalSlider);
                        EditorGUILayout.LabelField($"{tilegen.Presets[i].name}", GUI.skin.label);
                        editorPresets.Add(tilegen.Presets[i]);
                        CreateEditor(tilegen.Presets[i]).DrawDefaultInspector();
                    }
                }
            }
        }
    }
#endif
}