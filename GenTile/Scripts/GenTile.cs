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

        public List<GenTilePreset> Presets = new();

        public bool ShowPresetEditor = false;

        [Header("Extensions")]
        public GenTileRoomPlacer GenTileRoomPlacer;

        public List<byte[,]> Map = new();
        public bool[,] CollisionMap = new bool[0, 0];
        readonly List<TileBase> tiles = new();
        readonly List<GenTileLayerData> layerData = new();

        GenTilePreset preset;
        System.Random random;

        public void Clear()
        {
            Map.Clear();
            layerData.Clear();

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

            if (GenTileRoomPlacer != null) GenTileRoomPlacer.Clear();
        }

        void Initialize()
        {
            for (int i = 0; i < Enum.GetNames(typeof(GenTileType)).Length; i++)
            {
                Map.Add(new byte[Width, Height]);
            }

            for (int layer = 0; layer < preset.Layer.Count; layer++)
            {
                layerData.Add(new GenTileLayerData((layer + 1), Width, Height));
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
                            case GenTileAlgorithmType.Fill:
                                layerData[layer].Map[type] = GenTileAlgorithm.Fill(layerData[layer].Map[type], value, seed, algorithm.FillPercentage);
                                break;
                            case GenTileAlgorithmType.Degrade:
                                layerData[layer].Map[type] = GenTileAlgorithm.Degrade(layerData[layer].Map[type], value, seed, algorithm.DegradePercentage);
                                break;
                            case GenTileAlgorithmType.RandomWalk:
                                layerData[layer].Map[type] = GenTileAlgorithm.RandomWalk(layerData[layer].Map[type], value, seed, algorithm.Size);
                                break;
                            case GenTileAlgorithmType.PerlinNoise:
                                layerData[layer].Map[type] = GenTileAlgorithm.PerlinNoise(layerData[layer].Map[type], value, seed, algorithm.PerlinNoiseModifier);
                                break;
                            case GenTileAlgorithmType.Tunnel:
                                layerData[layer].Map[type] = GenTileAlgorithm.Tunnel(layerData[layer].Map[type], value, seed, algorithm.PathWidth, algorithm.XBeginPercent, algorithm.XFinishPercent, algorithm.YBeginPercent, algorithm.YFinishPercent);
                                break;
                            case GenTileAlgorithmType.Rooms:
                                layerData[layer].Map[type] = GenTileAlgorithm.Rooms(layerData[layer].Map[type], value, seed, algorithm.RoomAmount, algorithm.RoomWidth, algorithm.RoomHeight);
                                break;
                            case GenTileAlgorithmType.Walls:
                                layerData[layer].Map[type] = GenTileAlgorithm.Walls(layerData[layer].Map[type], value, seed, algorithm.WallPercentage, algorithm.OuterWall);
                                break;
                            case GenTileAlgorithmType.WaveFunctionCollapse:
                                layerData[layer].Map[type] = GenTileAlgorithm.WFC_Overlapping(layerData[layer].Map[type], value, seed, algorithm.Invert, algorithm.InputTexture, algorithm.N, algorithm.Symmetry, algorithm.Iterations);
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
            for (int type = 0; type < Enum.GetNames(typeof(GenTileType)).Length; type++)
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
            Map = map.ToList();
            for (int type = 0; type < Enum.GetNames(typeof(GenTileType)).Length; type++)
            {
                for (int x = 0; x < map[type].GetUpperBound(0); x++)
                {
                    for (int y = 0; y < map[type].GetUpperBound(1); y++)
                    {
                        Tilemap[type].SetTile(new Vector3Int(x, y, 0), tiles[map[type][x, y]]);
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
            if (GenTileRoomPlacer != null) GenTileRoomPlacer.Generate();
            return Map.ToList();
        }

        public void RenderMap(List<byte[,]> map)
        {
            Clear();
            Render(map);
            if (GenTileRoomPlacer != null) GenTileRoomPlacer.Generate();
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