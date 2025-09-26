using System;
using System.Collections.Generic;
using System.Linq;
using GenTools;
using Sentience;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace MindTheatre
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "WorldArea", menuName = "GenTools/WorldArea")]
    public class GenTileWorldArea : ScriptableObject
    {
        public int Seed = 0;
        public List<int[,]> Map = null;
        public List<GenTilePreset> Presets = new();
        public Vector3Int WorldPosition = Vector3Int.zero;

        public void Load(GenTile genTile)
        {
            genTile.Seed = Seed;
            genTile.Presets = Presets;
            if (Map != null) genTile.RenderMap(Map);
            else Map = genTile.Generate();
        }

        #region Creation

        public static GenTileWorldArea CreateNewArea(int seed, string worldName, Vector3Int worldPosition)
        {
            GenTileWorldArea worldArea = CreateInstance<GenTileWorldArea>();
            worldArea.Seed = seed;
            worldArea.name = worldName;
            worldArea.WorldPosition = new Vector3Int(worldPosition.x, worldPosition.y, worldPosition.z);
            SerializeArea(worldArea, worldName, worldPosition);
            return worldArea;
        }

        public static GenTileWorldArea CreateNewAreaFromTemplate(int seed, GenTileWorldArea template, string worldName, Vector3Int worldPosition)
        {
            GenTileWorldArea worldArea = Instantiate(template);
            worldArea.Seed = seed;
            worldArea.name = worldName;
            worldArea.WorldPosition = new Vector3Int(worldPosition.x, worldPosition.y, worldPosition.z);
            SerializeArea(worldArea, worldName, worldPosition);
            return worldArea;
        }

        public static void SerializeArea(GenTileWorldArea worldArea, string worldName, Vector3Int worldPosition)
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GenerateUniqueAssetPath($"Assets/Modules/Core/World/Data/{worldName}/{worldName}_A_X{worldPosition.x}Y{worldPosition.y}Z{worldPosition.z}.asset");
            AssetDatabase.CreateAsset(worldArea, path);
            AssetDatabase.SaveAssets();
#endif
        }

        #endregion
    }
}