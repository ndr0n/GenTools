using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    public struct GenTileObject
    {
        public TileBase Tile;
        public Vector3Int Position;

        public GenTileObject(TileBase tile, Vector3Int position)
        {
            Tile = tile;
            Position = position;
        }
    }

    [System.Serializable]
    public class GenTileObjectData
    {
        public int Amount = 1;
        public int Chance = 100;
        public List<TileBase> Tile = new();
        public List<GenTileObjectData> Recursion = new();
    }
}