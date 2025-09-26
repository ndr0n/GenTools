using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector2Int = UnityEngine.Vector2Int;

namespace GenTools
{
    public enum TunnelingAlgorithm
    {
        None = 0,
        Directional = 1,
        BresenhamLine = 2,
    }

    [System.Serializable]
    public class GenTileArea : MonoBehaviour
    {
        public GenTile GenTile;
        public readonly List<GenTileRoom> PlacedRooms = new();
        public readonly List<Vector3Int> PlacedTunnels = new();
        GenTileRoomType areaRoomType = null;
        System.Random random = new();

        public void Clear()
        {
            PlacedRooms.Clear();
            PlacedTunnels.Clear();
        }

        public void PlaceArea()
        {
            Clear();

            random = new(GenTile.Seed);
            if (GenTile.Preset.AreaRoomTypes.Count > 0) areaRoomType = GenTile.Preset.AreaRoomTypes[random.Next(GenTile.Preset.AreaRoomTypes.Count)];
            else areaRoomType = null;

            PlaceRooms();
            PlaceTunnels();
            PopulateRooms(PlacedRooms);

            if (areaRoomType != null)
            {
                GenTileRoom areaRoom = new GenTileRoom(areaRoomType, new Vector2Int(GenTile.Width, GenTile.Height), new Vector2Int(0, 0));

                List<Vector2Int> availablePositions = new();
                for (int x = 0; x < areaRoom.Size.x; x++)
                {
                    for (int y = 0; y < areaRoom.Size.y; y++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        foreach (var tile in areaRoomType.RoomTile)
                        {
                            if (GenTile.Tilemap[(int) GenTileType.Terrain].GetTile(new Vector3Int(pos.x, pos.y, 0)) == tile)
                            {
                                availablePositions.Add(new Vector2Int(pos.x + areaRoom.Position.x, pos.y + areaRoom.Position.y));
                                break;
                            }
                        }
                    }
                }
                availablePositions = areaRoom.PlaceTileRoom(GenTile, availablePositions, random);
                PlacedRooms.Insert(0, areaRoom);
            }

            foreach (var tilemap in GenTile.Tilemap) tilemap.RefreshAllTiles();
        }

        #region Rooms

        public void PlaceRooms()
        {
            PlacedRooms.Clear();
            for (int x = 0; x < GenTile.Width; x++)
            {
                for (int y = 0; y < GenTile.Height; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);

                    bool positionAvailable = true;
                    foreach (var room in PlacedRooms)
                    {
                        if (room.GetBounds().Contains(pos))
                        {
                            positionAvailable = false;
                            break;
                        }
                    }

                    if (positionAvailable)
                    {
                        bool roomPlaced = false;
                        for (int i = 0; i < GenTile.Tilemap.Count; i++)
                        {
                            foreach (var roomType in GenTile.Preset.RoomTypes)
                            {
                                GenTileRoom room = TryPlaceRoom(GenTile.Tilemap[i], pos, roomType);
                                if (room != null)
                                {
                                    PlacedRooms.Add(room);
                                    roomPlaced = true;
                                    break;
                                }
                            }
                            if (roomPlaced) break;
                        }
                    }
                }
            }
        }

        GenTileRoom TryPlaceRoom(Tilemap tilemap, Vector3Int pos, GenTileRoomType roomType)
        {
            foreach (var roomTile in roomType.RoomTile)
            {
                if (tilemap.GetTile(pos) == roomTile)
                {
                    Vector2Int min = new Vector2Int(pos.x, pos.y);
                    Vector2Int max = new Vector2Int(pos.x, pos.y);

                    for (int x = pos.x; x >= 0; x--)
                    {
                        if (tilemap.GetTile(new Vector3Int(x, pos.y, 0)) == roomTile) min.x = x;
                        else break;
                    }
                    for (int y = pos.y; y >= 0; y--)
                    {
                        if (tilemap.GetTile(new Vector3Int(min.x, y, 0)) == roomTile) min.y = y;
                        else break;
                    }

                    for (int x = min.x; x < GenTile.Width; x++)
                    {
                        if (tilemap.GetTile(new Vector3Int(x, min.y, 0)) == roomTile) max.x = x;
                        else break;
                    }
                    for (int y = min.y; y < GenTile.Height; y++)
                    {
                        if (tilemap.GetTile(new Vector3Int(min.x, y, 0)) == roomTile) max.y = y;
                        else break;
                    }

                    // if (min.x != max.x && min.y != max.y)
                    // {
                    Vector2Int rpos = min;
                    Vector2Int rsize = new Vector2Int((max.x - min.x) + 1, (max.y - min.y) + 1);
                    if (rsize.x >= roomType.MinSize.x && rsize.x <= roomType.MaxSize.x && rsize.y >= roomType.MinSize.y && rsize.y <= roomType.MaxSize.y)
                    {
                        bool canCreateRoom = true;
                        for (int x = min.x; x < max.x; x++)
                        {
                            for (int y = min.y; y < max.y; y++)
                            {
                                if (tilemap.GetTile(new Vector3Int(x, y, 0)) != roomTile)
                                {
                                    canCreateRoom = false;
                                    break;
                                }
                            }
                            if (canCreateRoom == false) break;
                        }
                        if (canCreateRoom)
                        {
                            Bounds roomBounds = new Bounds(new Vector3(rpos.x + (rsize.x / 2f), rpos.y + (rsize.y / 2f), 0), new Vector3(rsize.x, rsize.y, 0));
                            foreach (var placedRoom in PlacedRooms)
                            {
                                if (roomBounds.Intersects(placedRoom.GetBounds()))
                                {
                                    canCreateRoom = false;
                                    break;
                                }
                            }
                            if (canCreateRoom)
                            {
                                GenTileRoom room = new GenTileRoom(roomType, rsize, rpos);
                                return room;
                            }
                        }
                    }
                    // }
                    break;
                }
            }
            return null;
        }

        void PopulateRooms(List<GenTileRoom> rooms)
        {
            foreach (var room in rooms)
            {
                List<Vector2Int> availablePositions = new();
                for (int x = 0; x < room.Size.x; x++)
                {
                    for (int y = 0; y < room.Size.y; y++)
                    {
                        availablePositions.Add(new Vector2Int(x + room.Position.x, y + room.Position.y));
                    }
                }
                availablePositions = room.PlaceTileRoom(GenTile, availablePositions, random);
            }
        }

        #endregion

        #region Tunnels

        public void PlaceTunnels()
        {
            PlacedTunnels.Clear();
            // for (int x = 0; x < GenTile.Width; x++)
            // {
            // for (int y = 0; y < GenTile.Height; y++)
            // {
            // foreach (var tm in GenTile.Tilemap)
            // {
            // foreach (var room in PlacedRooms)
            // {
            // Vector3Int pos = new Vector3Int(x, y, 0);
            // if (tm.GetTile(pos) == tunnelTile)
            // {
            // if (!PlacedTunnels.Contains(pos)) PlacedTunnels.Add(pos);
            // }
            // }
            // }
            // }
            // }

            foreach (var origin in PlacedRooms.OrderBy(x => random.Next(int.MinValue, int.MaxValue)))
            {
                switch (origin.Type.TunnelingAlgorithm)
                {
                    case TunnelingAlgorithm.Directional:
                        PlaceTunnelsDirectional(origin);
                        break;
                    case TunnelingAlgorithm.BresenhamLine:
                        PlaceTunnelsBresenhamLine(origin);
                        break;
                }
            }
        }

        public void PlaceTunnelsDirectional(GenTileRoom origin)
        {
            int tunnelCount = random.Next(origin.Type.TunnelAmount.x, origin.Type.TunnelAmount.y + 1);
            var otherRooms = PlacedRooms.ToList();
            otherRooms.Remove(origin);
            List<CardinalDirection> directions = new List<CardinalDirection>() {CardinalDirection.South, CardinalDirection.West, CardinalDirection.North, CardinalDirection.East};
            directions = directions.OrderBy(x => random.Next(int.MinValue, int.MaxValue)).ToList();
            foreach (var direction in directions)
            {
                if (origin.PlacedTunnels.Count < tunnelCount)
                {
                    GenTileRoom connection = null;
                    List<Vector2Int> tunnelPositions = new();
                    switch (direction)
                    {
                        case CardinalDirection.North:
                            List<int> xitern = GenTools.GetRandomIterArray(origin.Position.x, (origin.Position.x + origin.Size.x), random);
                            foreach (var x in xitern)
                            {
                                tunnelPositions.Clear();
                                for (int y = (origin.Position.y + (origin.Size.y - 1)); y < GenTile.Height; y++)
                                {
                                    Vector2Int pos = new Vector2Int(x, y);
                                    tunnelPositions.Add(pos);
                                    foreach (var conn in otherRooms)
                                    {
                                        if (!origin.PlacedTunnels.Exists(originTunnel => originTunnel.Connection == conn))
                                        {
                                            if (conn.Contains(pos))
                                            {
                                                connection = conn;
                                                break;
                                            }
                                        }
                                    }
                                    if (connection != null) break;

                                    bool hasAdjacentRoom = false;
                                    foreach (var room in otherRooms)
                                    {
                                        if (room.Contains(pos + Vector2Int.left) || room.Contains(pos + Vector2Int.right))
                                        {
                                            hasAdjacentRoom = true;
                                            break;
                                        }
                                    }
                                    if (hasAdjacentRoom) break;
                                }
                                if (connection != null) break;
                            }
                            break;
                        case CardinalDirection.South:
                            List<int> xiters = GenTools.GetRandomIterArray(origin.Position.x, (origin.Position.x + origin.Size.x), random);
                            foreach (var x in xiters)
                            {
                                tunnelPositions.Clear();
                                for (int y = origin.Position.y; y >= 0; y--)
                                {
                                    Vector2Int pos = new Vector2Int(x, y);
                                    tunnelPositions.Add(pos);
                                    foreach (var conn in otherRooms)
                                    {
                                        if (!origin.PlacedTunnels.Exists(originTunnel => originTunnel.Connection == conn))
                                        {
                                            if (conn.Contains(pos))
                                            {
                                                connection = conn;
                                                break;
                                            }
                                        }
                                    }
                                    if (connection != null) break;

                                    bool hasAdjacentRoom = false;
                                    foreach (var room in otherRooms)
                                    {
                                        if (room.Contains(pos + Vector2Int.left) || room.Contains(pos + Vector2Int.right))
                                        {
                                            hasAdjacentRoom = true;
                                            break;
                                        }
                                    }
                                    if (hasAdjacentRoom) break;
                                }
                                if (connection != null) break;
                            }
                            break;
                        case CardinalDirection.East:
                            List<int> yitere = GenTools.GetRandomIterArray(origin.Position.y, (origin.Position.y + origin.Size.y), random);
                            foreach (var y in yitere)
                            {
                                tunnelPositions.Clear();
                                for (int x = (origin.Position.x + (origin.Size.x - 1)); x < GenTile.Width; x++)
                                {
                                    Vector2Int pos = new Vector2Int(x, y);
                                    tunnelPositions.Add(pos);
                                    foreach (var conn in otherRooms)
                                    {
                                        if (!origin.PlacedTunnels.Exists(originTunnel => originTunnel.Connection == conn))
                                        {
                                            if (conn.Contains(pos))
                                            {
                                                connection = conn;
                                                break;
                                            }
                                        }
                                    }
                                    if (connection != null) break;

                                    bool hasAdjacentRoom = false;
                                    foreach (var room in otherRooms)
                                    {
                                        if (room.Contains(pos + Vector2Int.up) || room.Contains(pos + Vector2Int.down))
                                        {
                                            hasAdjacentRoom = true;
                                            break;
                                        }
                                    }
                                    if (hasAdjacentRoom) break;
                                }
                                if (connection != null) break;
                            }
                            break;
                        case CardinalDirection.West:
                            List<int> yiterw = GenTools.GetRandomIterArray(origin.Position.y, (origin.Position.y + origin.Size.y), random);
                            foreach (var y in yiterw)
                            {
                                tunnelPositions.Clear();
                                for (int x = origin.Position.x; x >= 0; x--)
                                {
                                    Vector2Int pos = new Vector2Int(x, y);
                                    tunnelPositions.Add(pos);
                                    foreach (var conn in otherRooms)
                                    {
                                        if (!origin.PlacedTunnels.Exists(originTunnel => originTunnel.Connection == conn))
                                        {
                                            if (conn.Contains(pos))
                                            {
                                                connection = conn;
                                                break;
                                            }
                                        }
                                    }
                                    if (connection != null) break;

                                    bool hasAdjacentRoom = false;
                                    foreach (var room in otherRooms)
                                    {
                                        if (room.Contains(pos + Vector2Int.up) || room.Contains(pos + Vector2Int.down))
                                        {
                                            hasAdjacentRoom = true;
                                            break;
                                        }
                                    }
                                    if (hasAdjacentRoom) break;
                                }
                                if (connection != null) break;
                            }
                            break;
                    }
                    if (connection != null) FinishPlacingTunnel(origin, connection, tunnelPositions);
                }
            }
        }

        public void PlaceTunnelsBresenhamLine(GenTileRoom origin)
        {
            int tunnelCount = random.Next(origin.Type.TunnelAmount.x, origin.Type.TunnelAmount.y + 1);
            var possibleConnections = PlacedRooms.OrderBy(connection => Vector3.Distance(origin.GetCenter(), connection.GetCenter())).ToList();
            possibleConnections.Remove(origin);
            possibleConnections = possibleConnections.Where(connection => !origin.PlacedTunnels.Exists(originTunnel => originTunnel.Connection == connection)).ToList();
            for (int i = 0; i < (tunnelCount - origin.PlacedTunnels.Count); i++)
            {
                if (i >= possibleConnections.Count) break;
                var connection = possibleConnections[i];
                Vector3 oPoint = GenTools.ClosestPointBetween(origin, connection);
                Vector2Int originPoint = new Vector2Int((int) oPoint.x, (int) oPoint.y);
                Vector3 cPoint = GenTools.ClosestPointBetween(connection, origin);
                Vector2Int connectionPoint = new Vector2Int((int) cPoint.x, (int) cPoint.y);
                List<Vector2Int> tunnelPositions = BresenhamLine.Compute(originPoint, connectionPoint);
                FinishPlacingTunnel(origin, connection, tunnelPositions);
            }
        }

        void FinishPlacingTunnel(GenTileRoom origin, GenTileRoom connection, List<Vector2Int> tunnelPositions)
        {
            Vector2Int originPoint = tunnelPositions[0];
            Vector2Int connectionPoint = tunnelPositions[^1];

            List<Vector2Int> toRemove = new();
            for (int ipos = 1; ipos < tunnelPositions.Count; ipos++)
            {
                if (origin.Contains(tunnelPositions[ipos]))
                {
                    toRemove.Add(tunnelPositions[ipos - 1]);
                    originPoint = tunnelPositions[ipos];
                }
                if (connection.Contains(tunnelPositions[ipos]))
                {
                    for (int index = (ipos + 1); index < tunnelPositions.Count; index++) toRemove.Add(tunnelPositions[index]);
                    connectionPoint = tunnelPositions[ipos];
                    break;
                }
            }
            foreach (var r in toRemove) tunnelPositions.Remove(r);

            tunnelPositions.Remove(originPoint);
            tunnelPositions.Remove(connectionPoint);

            bool canPlaceTunnel = true;
            foreach (var pos in tunnelPositions)
            {
                foreach (var room in PlacedRooms)
                {
                    if (room.Contains(pos))
                    {
                        canPlaceTunnel = false;
                        break;
                    }
                    foreach (var tunnel in room.PlacedTunnels)
                    {
                        if (tunnel.OriginPoint == originPoint || tunnel.OriginPoint == connectionPoint || tunnel.ConnectionPoint == originPoint || tunnel.ConnectionPoint == connectionPoint)
                        {
                            canPlaceTunnel = false;
                            break;
                        }
                    }
                    if (canPlaceTunnel == false) break;
                }
                if (canPlaceTunnel == false) break;
            }

            if (canPlaceTunnel)
            {
                GenTileRoomTunnel originTunnel = new(origin, originPoint, connection, connectionPoint, tunnelPositions);
                origin.PlacedTunnels.Add(originTunnel);

                if (connection != null)
                {
                    List<Vector2Int> reversedTunnelPositions = new();
                    for (int iter = 0; iter < tunnelPositions.Count; iter++) reversedTunnelPositions.Add(tunnelPositions[tunnelPositions.Count - 1 - iter]);
                    GenTileRoomTunnel connectionTunnel = new(connection, connectionPoint, origin, originPoint, reversedTunnelPositions);
                    connection.PlacedTunnels.Add(connectionTunnel);
                }

                GenTile.Tilemap[0].SetTile(new Vector3Int(originPoint.x, originPoint.y, 0), origin.Type.TunnelTile);
                GenTile.Tilemap[0].SetTile(new Vector3Int(connectionPoint.x, connectionPoint.y, 0), origin.Type.TunnelTile);
                foreach (var pos in tunnelPositions)
                {
                    Vector3Int position = new Vector3Int(pos.x, pos.y, 0);
                    GenTile.Tilemap[0].SetTile(position, origin.Type.TunnelTile);
                    if (!PlacedTunnels.Contains(position)) PlacedTunnels.Add(position);
                }
            }
        }

        #endregion
    }
}