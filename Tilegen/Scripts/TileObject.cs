using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GenTools
{
    [System.Serializable]
    public class TileObjectType
    {
        public int Amount = 1;
        public int Chance = 100;
        public List<TileBase> Tile;
        public List<TileObjectType> Recursion;
    }

    [System.Serializable]
    public struct TileObject
    {
        public TileObjectType Type;
        public TileBase Tile;
        public Vector3Int Position;

        public TileObject(TileObjectType type, TileBase tile, Vector3Int position)
        {
            Type = type;
            Tile = tile;
            Position = position;
        }
    }
}