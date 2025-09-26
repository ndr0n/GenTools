using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace GenTools
{
    public enum GenTileType : int
    {
        Terrain = 0,
        Scatter = 1,
        Objects = 2,
    }

    [System.Serializable]
    public class GenTileAlgorithm
    {
        public TileBase Tile;
        public GenTileType Type;
        public Vector2Int Offset = new Vector2Int(0, 0);
        public List<GenTileAlgorithmData> Algorithm = new List<GenTileAlgorithmData>();

        public List<Vector2Int> Execute(List<Vector2Int> available, int seed)
        {
            System.Random random = new System.Random(seed);
            List<Vector2Int> placed = new List<Vector2Int>();
            if (available.Count == 0) return placed;

            List<Vector2Int> positions = new();
            foreach (var pos in available)
            {
                Vector2Int p = new Vector2Int(pos.x, pos.y);
                positions.Add(p);
            }

            foreach (var algorithm in Algorithm)
            {
                switch (algorithm.Algorithm)
                {
                    case GenTileAlgoType.Fill:
                        algorithm.Fill.Count = algorithm.FillCount;
                        algorithm.Fill.Percentage = algorithm.FillPercentage;
                        List<Vector2Int> placedFill = algorithm.Fill.Execute(positions, seed);
                        foreach (var pos in placedFill)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgoType.RandomWalk:
                        algorithm.RandomWalk.Percentage = algorithm.RandomWalkPercentage;
                        List<Vector2Int> placedRandomWalk = algorithm.RandomWalk.Execute(positions, seed);
                        foreach (var pos in placedRandomWalk)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgoType.PerlinNoise:
                        algorithm.PerlinNoise.Modifier = algorithm.PerlinNoiseModifier;
                        List<Vector2Int> placedPerlinNoise = algorithm.PerlinNoise.Execute(positions, seed);
                        foreach (var pos in placedPerlinNoise)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgoType.Tunnel:
                        algorithm.Tunnel.PathWidth = algorithm.TunnelPathWidth;
                        algorithm.Tunnel.XBeginPercentage = algorithm.TunnelXBeginPercent;
                        algorithm.Tunnel.XFinishPercentage = algorithm.TunnelXFinishPercent;
                        algorithm.Tunnel.YBeginPercentage = algorithm.TunnelYBeginPercent;
                        algorithm.Tunnel.YFinishPercentage = algorithm.TunnelYFinishPercent;
                        List<Vector2Int> placedTunnel = algorithm.Tunnel.Execute(positions, seed);
                        foreach (var pos in placedTunnel)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgoType.Corridors:
                        algorithm.Corridors.Lifetime = algorithm.CorridorsLifetime;
                        algorithm.Corridors.TunnelWidth = algorithm.CorridorsWidth;
                        algorithm.Corridors.ChangePercentage = algorithm.CorridorsChangePercentage;
                        List<Vector2Int> placedTunneler = algorithm.Corridors.Execute(positions, seed);
                        foreach (var pos in placedTunneler)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgoType.RoomPlacer:
                        algorithm.RoomPlacer.Chance = algorithm.RoomPlacerChance;
                        algorithm.RoomPlacer.Width = algorithm.RoomPlacerWidth;
                        algorithm.RoomPlacer.Height = algorithm.RoomPlacerHeight;
                        List<Vector2Int> placedRooms = algorithm.RoomPlacer.Execute(positions, seed);
                        foreach (var pos in placedRooms)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgoType.BinarySpacePartition:
                        algorithm.BinarySpacePartition.Width = algorithm.BSPWidth;
                        algorithm.BinarySpacePartition.Height = algorithm.BSPHeight;
                        algorithm.BinarySpacePartition.Chance = algorithm.BSPChance;
                        algorithm.BinarySpacePartition.Offset = algorithm.BSPOffset;
                        List<Vector2Int> placedBsp = algorithm.BinarySpacePartition.Execute(positions, seed);
                        foreach (var pos in placedBsp)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgoType.WallPlacer:
                        algorithm.WallPlacer.Percentage = algorithm.WallPlacerPercentage;
                        List<Vector2Int> placedWallPlacer = algorithm.WallPlacer.Execute(positions, seed);
                        foreach (var pos in placedWallPlacer)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                    case GenTileAlgoType.WaveFunctionCollapse:
                        algorithm.WaveFunctionCollapse.InputTexture = algorithm.WFCInputTexture;
                        algorithm.WaveFunctionCollapse.Invert = algorithm.WFCInvert;
                        algorithm.WaveFunctionCollapse.N = algorithm.WFCN;
                        algorithm.WaveFunctionCollapse.Iterations = algorithm.WFCIterations;
                        algorithm.WaveFunctionCollapse.Symmetry = algorithm.WFCSymmetry;
                        List<Vector2Int> placedWfc = algorithm.WaveFunctionCollapse.Execute(positions, seed);
                        foreach (var pos in placedWfc)
                        {
                            positions.Remove(pos);
                            placed.Add(pos);
                        }
                        break;
                }
            }

            return placed;
        }
    }
}