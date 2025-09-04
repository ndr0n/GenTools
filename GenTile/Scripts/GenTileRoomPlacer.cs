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
    [System.Serializable]
    public class GenTileRoomPlacer : MonoBehaviour
    {
        public GenTile GenTile;

        public int Seed = 0;
        public bool RandomizeSeed = false;

        public List<GenTileRoomType> RoomType = new();
        public List<GenTileRoom> PlacedRooms = new();

        public TileBase TunnelTile;

        System.Random random = new();

        public void Clear()
        {
            PlacedRooms.Clear();
        }

        public void Generate()
        {
            Clear();
            if (RandomizeSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
            random = new(Seed);

            PlaceRooms();
            PlaceTunnels();
            PopulateRooms(PlacedRooms);

            foreach (var tilemap in GenTile.Tilemap) tilemap.RefreshAllTiles();
        }

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
                            foreach (var roomType in RoomType)
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
            foreach (var roomSprite in roomType.RoomSprite)
            {
                if (tilemap.GetSprite(pos) == roomSprite)
                {
                    Vector2Int min = new Vector2Int(pos.x, pos.y);
                    Vector2Int max = new Vector2Int(pos.x, pos.y);

                    for (int x = pos.x; x >= 0; x--)
                    {
                        if (tilemap.GetSprite(new Vector3Int(x, pos.y, 0)) == roomSprite) min.x = x;
                        else break;
                    }
                    for (int y = pos.y; y >= 0; y--)
                    {
                        if (tilemap.GetSprite(new Vector3Int(min.x, y, 0)) == roomSprite) min.y = y;
                        else break;
                    }

                    for (int x = min.x; x < GenTile.Width; x++)
                    {
                        if (tilemap.GetSprite(new Vector3Int(x, min.y, 0)) == roomSprite) max.x = x;
                        else break;
                    }
                    for (int y = min.y; y < GenTile.Height; y++)
                    {
                        if (tilemap.GetSprite(new Vector3Int(min.x, y, 0)) == roomSprite) max.y = y;
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
                                if (tilemap.GetSprite(new Vector3Int(x, y, 0)) != roomSprite)
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

        public void PlaceTunnels()
        {
            foreach (var origin in PlacedRooms.OrderBy(x => random.Next(int.MinValue, int.MaxValue)))
            {
                int tunnelCount = random.Next(origin.Type.MaxTunnels.x, origin.Type.MaxTunnels.y + 1);
                var possibleConnections = PlacedRooms.OrderBy(connection => Vector3.Distance(origin.GetCenter(), connection.GetCenter())).ToList();
                possibleConnections.Remove(origin);
                possibleConnections = possibleConnections.Where(connection => !origin.PlacedTunnels.Exists(originTunnel => originTunnel.Connection == connection)).ToList();
                for (int i = 0; i < (tunnelCount - origin.PlacedTunnels.Count); i++)
                {
                    if (i >= possibleConnections.Count) break;
                    var connection = possibleConnections[i];

                    List<Vector2Int> tunnelPositions = new();
                    Vector3 oPoint = GenTools.ClosestPointBetween(origin, connection);
                    Vector2Int originPoint = new Vector2Int((int) oPoint.x, (int) oPoint.y);
                    Vector3 cPoint = GenTools.ClosestPointBetween(connection, origin);
                    Vector2Int connectionPoint = new Vector2Int((int) cPoint.x, (int) cPoint.y);
                    BresenhamLine.Compute(originPoint, connectionPoint, tunnelPositions);

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
                        }
                        if (canPlaceTunnel == false) break;
                    }

                    if (canPlaceTunnel)
                    {
                        GenTileRoomTunnel originTunnel = new(origin, originPoint, connection, connectionPoint, tunnelPositions);
                        origin.PlacedTunnels.Add(originTunnel);

                        List<Vector2Int> reversedTunnelPositions = new();
                        for (int iter = 0; iter < tunnelPositions.Count; iter++) reversedTunnelPositions.Add(tunnelPositions[tunnelPositions.Count - 1 - iter]);
                        GenTileRoomTunnel connectionTunnel = new(connection, connectionPoint, origin, originPoint, reversedTunnelPositions);
                        connection.PlacedTunnels.Add(connectionTunnel);

                        foreach (var pos in tunnelPositions) GenTile.Tilemap[0].SetTile(new Vector3Int(pos.x, pos.y, 0), TunnelTile);
                    }
                }
            }
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
                availablePositions = room.PlaceDoors(GenTile, availablePositions, random, true);
                availablePositions = room.PlaceWalls(GenTile, availablePositions, random);
                // availablePositions = room.PlaceStairs();
                availablePositions = room.PlaceObjects(GenTile, availablePositions, random);
            }
        }
    }
}