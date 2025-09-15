using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace GenTools
{
    public enum TunnelingAlgorithm
    {
        Door = 0,
        Directional = 1,
        BresenhamLine = 2,
    }

    [System.Serializable]
    public class GenTileRoomPlacer : MonoBehaviour
    {
        public GenTile GenTile;
        public List<TileBase> TunnelTile = new();
        public readonly List<GenTileRoom> PlacedRooms = new();
        public readonly List<Vector3Int> PlacedTunnels = new();
        System.Random random = new();

        public void Clear()
        {
            PlacedRooms.Clear();
            PlacedTunnels.Clear();
        }

        public void Generate()
        {
            Clear();
            random = new(GenTile.Seed);
            CheckRooms();
            PlaceTunnels();
            PopulateRooms(PlacedRooms);
            foreach (var tilemap in GenTile.Tilemap) tilemap.RefreshAllTiles();
        }

        #region Rooms

        public void CheckRooms()
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
                                GenTileRoom room = CheckForRoom(GenTile.Tilemap[i], pos, roomType);
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

        GenTileRoom CheckForRoom(Tilemap tilemap, Vector3Int pos, GenTileRoomType roomType)
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
                List<Vector3Int> availablePositions = new();
                for (int x = 0; x < room.Size.x; x++)
                {
                    for (int y = 0; y < room.Size.y; y++)
                    {
                        availablePositions.Add(new Vector3Int(x + room.Position.x, y + room.Position.y, 0));
                    }
                }
                availablePositions = room.ReplaceFloor(GenTile, availablePositions, random);
                availablePositions = room.PlaceDoors(GenTile, availablePositions, random, room.Type.PlaceDoorsInsideRoom);
                availablePositions = room.PlaceWalls(GenTile, availablePositions, random);
                // availablePositions = room.PlaceStairs();
                availablePositions = room.PlaceObjects(GenTile, availablePositions, room, random);
            }
        }

        #endregion

        #region Tunnels

        public void PlaceTunnels()
        {
            PlacedTunnels.Clear();
            for (int x = 0; x < GenTile.Width; x++)
            {
                for (int y = 0; y < GenTile.Height; y++)
                {
                    foreach (var tm in GenTile.Tilemap)
                    {
                        foreach (var tunnelTile in TunnelTile)
                        {
                            Vector3Int pos = new Vector3Int(x, y, 0);
                            if (tm.GetTile(pos) == tunnelTile)
                            {
                                if (!PlacedTunnels.Contains(pos)) PlacedTunnels.Add(pos);
                            }
                        }
                    }
                }
            }

            foreach (var origin in PlacedRooms.OrderBy(x => random.Next(int.MinValue, int.MaxValue)))
            {
                switch (origin.Type.TunnelingAlgorithm)
                {
                    case TunnelingAlgorithm.Door:
                        PlaceTunnelsDoor(origin);
                        break;
                    case TunnelingAlgorithm.Directional:
                        PlaceTunnelsDirectional(origin);
                        break;
                    case TunnelingAlgorithm.BresenhamLine:
                        PlaceTunnelsBresenhamLine(origin);
                        break;
                }
            }
        }

        public void PlaceTunnelsDoor(GenTileRoom origin)
        {
            List<CardinalDirection> directions = new() {CardinalDirection.South, CardinalDirection.West, CardinalDirection.North, CardinalDirection.East};
            directions = directions.OrderBy(x => random.Next(int.MinValue, int.MaxValue)).ToList();
            int doorCount = random.Next(origin.Type.TunnelAmount.x, origin.Type.TunnelAmount.y + 1);
            Tilemap tunnelTilemap = GenTile.Tilemap[(int) GenTileType.Terrain];
            Tilemap doorTilemap = GenTile.Tilemap[(int) GenTileType.Objects];
            TileBase door = origin.Type.Doors[random.Next(origin.Type.Doors.Count)];
            for (int i = 0; i < doorCount; i++)
            {
                if (directions.Count == 0) break;
                CardinalDirection direction = directions[0];
                directions.RemoveAt(0);
                switch (direction)
                {
                    case CardinalDirection.South:
                        bool breakLoop1 = false;
                        for (int x1 = origin.Position.x + 1; x1 < origin.Position.x + origin.Size.x; x1++)
                        {
                            int y1 = origin.Position.y;
                            Vector3Int doorPosition1 = new Vector3Int(x1, y1, 0);
                            foreach (var tunnelTile in TunnelTile)
                            {
                                if (tunnelTilemap.GetTile(doorPosition1 + Vector3Int.down) == tunnelTile)
                                {
                                    doorTilemap.SetTile(doorPosition1, door);
                                    origin.PlacedDoors.Add(new GenTileObject(door, doorPosition1));
                                    breakLoop1 = true;
                                    break;
                                }
                            }
                            if (breakLoop1) break;
                        }
                        break;
                    case CardinalDirection.West:
                        bool breakLoop2 = false;
                        for (int y2 = origin.Position.y + 1; y2 < origin.Position.y + origin.Size.y; y2++)
                        {
                            int x2 = origin.Position.x;
                            Vector3Int doorPosition2 = new Vector3Int(x2, y2, 0);
                            foreach (var tunnelTile in TunnelTile)
                            {
                                if (tunnelTilemap.GetTile(doorPosition2 - Vector3Int.left) == tunnelTile)
                                {
                                    doorTilemap.SetTile(doorPosition2, door);
                                    origin.PlacedDoors.Add(new GenTileObject(door, doorPosition2));
                                    breakLoop2 = true;
                                    break;
                                }
                            }
                            if (breakLoop2) break;
                        }
                        break;
                    case CardinalDirection.North:
                        bool breakLoop3 = false;
                        for (int x3 = origin.Position.x + 1; x3 < origin.Position.x + origin.Size.x; x3++)
                        {
                            int y3 = origin.Position.y + origin.Size.y - 1;
                            Vector3Int doorPosition3 = new Vector3Int(x3, y3, 0);
                            foreach (var tunnelTile in TunnelTile)
                            {
                                if (tunnelTilemap.GetTile(doorPosition3 + Vector3Int.up) == tunnelTile)
                                {
                                    doorTilemap.SetTile(doorPosition3, door);
                                    origin.PlacedDoors.Add(new GenTileObject(door, doorPosition3));
                                    breakLoop3 = true;
                                    break;
                                }
                            }
                            if (breakLoop3) break;
                        }
                        break;
                    case CardinalDirection.East:
                        bool breakLoop4 = false;
                        for (int y4 = origin.Position.y + 1; y4 < origin.Position.y + origin.Size.y; y4++)
                        {
                            int x4 = origin.Position.x + origin.Size.x - 1;
                            Vector3Int doorPosition4 = new Vector3Int(x4, y4, 0);
                            foreach (var tunnelTile in TunnelTile)
                            {
                                if (tunnelTilemap.GetTile(doorPosition4 + Vector3Int.right) == tunnelTile)
                                {
                                    doorTilemap.SetTile(doorPosition4, door);
                                    origin.PlacedDoors.Add(new GenTileObject(door, doorPosition4));
                                    breakLoop4 = true;
                                    break;
                                }
                            }
                            if (breakLoop4) break;
                        }
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