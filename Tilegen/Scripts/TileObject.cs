using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    public struct TileObject
    {
        public TileBase Tile;
        public Vector3Int Position;

        public TileObject(TileBase tile, Vector3Int position)
        {
            Tile = tile;
            Position = position;
        }
    }

    [System.Serializable]
    public class TileObjectData
    {
        public int Amount = 1;
        public int Chance = 100;
        public List<TileBase> Tile = new();
        public List<TileObjectData> Recursion = new();
    }
}