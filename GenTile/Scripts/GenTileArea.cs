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
    [CreateAssetMenu(fileName = "Area", menuName = "GenTools/Area")]
    public class GenTileArea : ScriptableObject
    {
        public int Seed = 0;
        public List<byte[,]> Map = null;
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

        public static GenTileArea CreateNewArea(int seed, string worldName, Vector3Int worldPosition)
        {
            GenTileArea area = CreateInstance<GenTileArea>();
            area.Seed = seed;
            area.name = worldName;
            area.WorldPosition = new Vector3Int(worldPosition.x, worldPosition.y, 0);
            SerializeArea(area, worldName, worldPosition);
            return area;
        }

        public static GenTileArea CreateNewAreaFromTemplate(int seed, GenTileArea template, string worldName, Vector3Int worldPosition)
        {
            GenTileArea area = Instantiate(template);
            area.Seed = seed;
            area.name = worldName;
            area.WorldPosition = new Vector3Int(worldPosition.x, worldPosition.y, 0);
            GenTileArea.SerializeArea(area, worldName, worldPosition);
            return area;
        }

        public static void SerializeArea(GenTileArea area, string worldName, Vector3Int worldPosition)
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GenerateUniqueAssetPath($"Assets/Modules/GenTools/GenTile/Data/World/{worldName}/{worldName}_A_X{worldPosition.x}Y{worldPosition.y}Z{worldPosition.z}.asset");
            AssetDatabase.CreateAsset(area, path);
            AssetDatabase.SaveAssets();
#endif
        }

        #endregion
    }
}