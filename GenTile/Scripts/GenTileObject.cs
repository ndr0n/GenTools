using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    public enum GenTilePlacementRules
    {
        Any = 0,
        Inner = 1,
        Outer = 2,
    }

    [System.Serializable]
    public struct GenTileObject
    {
        public TileBase Tile;
        public Vector2Int Position;

        public GenTileObject(TileBase tile, Vector2Int position)
        {
            Tile = tile;
            Position = position;
        }
    }

    [System.Serializable]
    public class GenTileWallData
    {
        // public int Chance = 100;
        public List<GenTileAlgorithm> Algorithm = new();
    }

    [System.Serializable]
    public class GenTileObjectData
    {
        public int Amount = 1;
        public int Chance = 100;
        public GenTilePlacementRules PlacementRules;
        public List<TileBase> Tile = new();
        public List<GenTileObjectData> Recursion = new();
    }
}