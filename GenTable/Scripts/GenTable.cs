using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace GenTools
{
    [System.Serializable]
    public class GenTableRow
    {
        public string Name = "";
        public List<string> Items = new();
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "GT", menuName = "GenTools/GenTable")]
    public class GenTable : ScriptableObject
    {
        [Multiline]
        public string Result = "";
        public List<GenTableRow> Rows = new();

        public string Generate()
        {
            System.Random random = new(Random.Range(int.MinValue, int.MaxValue));
            Result = "";
            foreach (var row in Rows)
            {
                string item = row.Items[random.Next(0, row.Items.Count)];
                Result += $"{row.Name}: {item}\n";
            }
            return Result;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GenTable))]
    public class GenTable_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenTable table = (GenTable) target;
            if (GUILayout.Button("Generate")) table.Generate();
        }
    }
#endif
}