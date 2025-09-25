using System.Collections.Generic;
using System.Linq;
using GenTools;
using MindTheatre;
using Sentience;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenTileAreaTemplate
    {
        public Vector3Int MinPosition;
        public Vector3Int MaxPosition;
        public List<WorldArea> WorldAreaTemplate;
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "World", menuName = "GenTools/World")]
    public class GenTileWorld : ScriptableObject
    {
        public int WorldSeed = 0;
        public List<WorldArea> Map = new();
        public Vector3Int WorldSize = new Vector3Int(1, 1, 1);
        public Vector3Int WorldAreaSize = new Vector3Int(50, 1, 50);
        public List<GenTileAreaTemplate> WorldAreaTemplates = new();

        public void Generate()
        {
            WorldSeed = Random.Range(int.MinValue, int.MaxValue);
            System.Random random = new System.Random(WorldSeed);
            Map = new();
            for (int x = 0; x < WorldSize.x; x++)
            {
                for (int z = 0; z < WorldSize.z; z++)
                {
                    for (int y = 0; y < WorldSize.y; y++)
                    {
                        GenTileAreaTemplate areaTemplate = null;
                        foreach (var template in WorldAreaTemplates.OrderBy(x => random.Next()))
                        {
                            if (x >= template.MinPosition.x && x <= template.MaxPosition.x && y >= template.MinPosition.y && y <= template.MaxPosition.y && z >= template.MinPosition.z && z <= template.MaxPosition.z)
                            {
                                areaTemplate = template;
                                break;
                            }
                        }
                        if (areaTemplate == null) areaTemplate = WorldAreaTemplates[random.Next(WorldAreaTemplates.Count)];
                        int seed = random.Next(int.MinValue, int.MaxValue);
                        WorldArea worldArea = WorldArea.CreateWorldTileFromTemplate(seed, areaTemplate.WorldAreaTemplate[random.Next(areaTemplate.WorldAreaTemplate.Count)], name, new Vector3Int(x, y, z));
                        Map.Add(worldArea);
                    }
                }
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(GenTileWorld))]
    public class GenTileWorld_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenTileWorld world = (GenTileWorld) target;
            if (GUILayout.Button("Generate"))
            {
                world.Generate();
            }
        }
    }
#endif
}