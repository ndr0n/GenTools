using System.Collections.Generic;
using GenTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    public class WFC : MonoBehaviour
    {
        [Header("Seed")]
        public bool RandomSeed = false;
        public int Seed = 0;
        public bool Invert = false;

        [Header("Parameters")]
        public Texture2D InputTexture;
        public int N = 3;
        public int Width = 50;
        public int Height = 50;
        public bool PeriodicInput = false;
        public bool PeriodicOutput = false;
        public int Symmetry = 2;
        public int Ground = 0;
        public int Iterations = 0;

        [Header("Output")]
        public Tilemap Tilemap;
        public TileBase Tilebase;
        byte[,] map = new byte[0, 0];

        public void GenerateOverlappingModel()
        {
            if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
            Tilemap.ClearAllTiles();

            byte[,] sample = new byte[InputTexture.width, InputTexture.height];
            for (int x = 0; x < sample.GetUpperBound(0); x++)
            {
                for (int y = 0; y < sample.GetUpperBound(1); y++)
                {
                    Color32 color = InputTexture.GetPixel(x, y);
                    byte value = 0;
                    if (color.r > 50 || color.g > 50 || color.b > 50) value = 1;
                    sample[x, y] = value;
                }
            }

            if (TryRunModel(sample, N)) return;
            if (N > 2)
            {
                if (TryRunModel(sample, N - 1)) return;
                if (N > 3)
                {
                    if (TryRunModel(sample, N - 2)) return;
                }
            }
            Debug.Log($"Failed to run model.");
        }

        bool TryRunModel(byte[,] sample, int n)
        {
            OverlappingModel model = new(sample, n, Width, Height, PeriodicInput, PeriodicOutput, Symmetry, Ground);
            if (model.Run(Seed, Iterations))
            {
                map = new byte[Width, Height];
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        byte v = model.Sample(x, y);
                        if (v != 99) map[x, y] = v;
                        else map[x, y] = 0;
                    }
                }
                for (int x = 0; x < map.GetUpperBound(0); x++)
                {
                    for (int y = 0; y < map.GetUpperBound(1); y++)
                    {
                        if (map[x, y] == 0)
                        {
                            if (Invert == true) Tilemap.SetTile(new Vector3Int(x, y, 0), Tilebase);
                        }
                        else
                        {
                            if (Invert == false) Tilemap.SetTile(new Vector3Int(x, y, 0), Tilebase);
                        }
                    }
                }
                Debug.Log($"Model Ran Successfully with N: {n}.");
                return true;
            }
            return false;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WFC))]
    public class WFC_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            WFC wfc = (WFC) target;
            if (GUILayout.Button("Clear")) wfc.Tilemap.ClearAllTiles();
            if (GUILayout.Button("Generate")) wfc.GenerateOverlappingModel();
        }
    }
#endif
}