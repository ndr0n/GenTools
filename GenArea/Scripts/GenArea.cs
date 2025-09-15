using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GenTools
{
    [System.Serializable]
    public class GenArea : MonoBehaviour
    {
        public Vector3Int Size = new Vector3Int(20, 1, 20);
        public Vector2Int Border = new Vector2Int(1, 1);

        public int Seed = 0;
        public bool RandomSeed = false;

        [FormerlySerializedAs("Type")]
        public bool BuildMainRoom = false;
        public GenRoomType MainRoomType;
        public GenRoom GenRoomPrefab;
        public GenRoomType InnerRoomType;

        [Header("Source")]
        public GenTile GenTile;

        [Header("Runtime")]
        public GenRoom MainRoom;
        public List<GenRoom> InnerRoom = new();

        System.Random random;

        GameObject tunnelParent;
        readonly List<GameObject> tunnelFloor = new();
        readonly List<GameObject> tunnelRoof = new();
        readonly List<GameObject> tunnelDoor = new();
        readonly List<GameObject> tunnelPillar = new();
        public readonly List<Vector3> tunnelPositions = new();

        public bool GenerateNewTile = true;

        public void Clear()
        {
            foreach (var room in InnerRoom)
            {
                if (room != null) DestroyImmediate(room.gameObject);
            }
            InnerRoom.Clear();
            if (MainRoom != null) DestroyImmediate(MainRoom.gameObject);

            if (tunnelParent != null)
            {
                DestroyImmediate(tunnelParent.gameObject);
            }
            tunnelFloor.Clear();
            tunnelRoof.Clear();
            tunnelDoor.Clear();
            tunnelPositions.Clear();
        }

        public async Awaitable Generate()
        {
            try
            {
                Clear();
                if (RandomSeed) Seed = Random.Range(int.MinValue, int.MaxValue);
                random = new(Seed);
                tunnelParent = GenTools.CreateGameObject("Tunnel", transform);
                if (GenTile != null)
                {
                    GenTile.Width = (Size.x - Border.x);
                    GenTile.Height = (Size.z - Border.y);
                    await GenerateFromGenTile();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Awaitable GenerateFromGenTile()
        {
            GenRoomPreset preset = MainRoomType.Presets[random.Next(MainRoomType.Presets.Count)];
            if (BuildMainRoom)
            {
                // Generate Main Room
                MainRoom = Instantiate(GenRoomPrefab.gameObject, transform).GetComponent<GenRoom>();
                MainRoom.name = "MainRoom";
                MainRoom.Type = MainRoomType;
                MainRoom.RandomSeed = false;
                MainRoom.Seed = random.Next(int.MinValue, int.MaxValue);
                MainRoom.GridSize = Size;
                MainRoom.OuterDoorAmount = Vector2Int.zero;
                await MainRoom.Generate();
                preset = MainRoom.Preset;
            }

            if (GenerateNewTile)
            {
                // Generate Tile Data
                GenTile.RandomSeed = false;
                GenTile.Seed = random.Next(int.MinValue, int.MaxValue);
                GenTile.Generate();
            }

            // Build Tunnels
            GameObject roofPreset = null;
            if (preset.Roof.Count > 0) roofPreset = preset.Roof[random.Next(0, preset.Roof.Count)];
            GameObject pillarPreset = null;
            if (preset.Pillar.Count > 0) pillarPreset = preset.Pillar[random.Next(0, preset.Pillar.Count)];
            GameObject floorPreset = preset.Floor[random.Next(0, preset.Floor.Count)];
            foreach (var position in GenTile.GenTileRoomPlacer.PlacedTunnels)
            {
                Vector3 pos = new Vector3(position.x * preset.TileSize.x, 0, position.y * preset.TileSize.z);
                pos += new Vector3(preset.TileSize.x / 2, 0, preset.TileSize.z / 2);
                if (!tunnelFloor.Exists(x => x.transform.position == pos))
                {
                    BuildTunnelFloorAndRoof(floorPreset, roofPreset, pillarPreset, Vector3Int.RoundToInt(pos), preset.TileSize);
                    Debug.Log($"TUNNEL FLOOR INIT COUNT: {tunnelFloor.Count} | pos: {pos}");
                    // await MainRoom.Await();
                }
            }

            // Build Inner Rooms
            foreach (var tileRoom in GenTile.GenTileRoomPlacer.PlacedRooms)
            {
                await BuildRoomFromTileRoom(tileRoom, 0);
            }
            BuildTunnelWalls(preset.TileSize);
        }

        public async Awaitable BuildRoomFromTileRoom(GenTileRoom tileRoom, int y)
        {
            GenRoom room = Instantiate(GenRoomPrefab.gameObject, transform).GetComponent<GenRoom>();
            room.name = $"InnerRoom-{room.transform.parent.childCount}";
            room.Type = InnerRoomType;
            room.RandomSeed = false;
            room.Seed = random.Next(int.MinValue, int.MaxValue);
            room.GridSize = new Vector3Int(tileRoom.Size.x, Size.y, tileRoom.Size.y);
            room.transform.localPosition = new Vector3(tileRoom.Position.x * room.TileSize.x, y * room.TileSize.y, tileRoom.Position.y * room.TileSize.z);
            room.transform.localRotation = Quaternion.identity;
            room.transform.localPosition += new Vector3(Border.x * room.TileSize.x, 0, Border.y * room.TileSize.z);
            InnerRoom.Add(room);

            await room.Generate();

            await BuildDoorsFromTileRoom(room, tileRoom, y);
            await BuildTunnelsFromTileRoom(room, tileRoom, y);
        }

        async Awaitable BuildDoorsFromTileRoom(GenRoom room, GenTileRoom tileRoom, int y)
        {
            foreach (var door in tileRoom.PlacedDoors)
            {
                // Build Doors
                GameObject outerDoorPreset = room.Preset.OuterDoor[random.Next(0, room.Preset.OuterDoor.Count)];
                Vector3 doorPosition = new Vector3(door.Position.x * room.TileSize.x, y * room.TileSize.y, door.Position.y * room.TileSize.z);
                doorPosition += room.Content.localPosition;
                List<GenRoomNode> nodes = room.GetAllNodes();
                GenRoomNode node = nodes.FirstOrDefault(x => x.Floor.transform.position == doorPosition);
                if (node != null)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        GameObject wall = node.Wall[direction];
                        if (wall != null)
                        {
                            GameObject outerDoor = Instantiate(outerDoorPreset, room.Content);
                            outerDoor.transform.position = wall.transform.position;
                            outerDoor.transform.rotation = wall.transform.rotation;
                            room.OuterDoor.Add(outerDoor);
                            tunnelDoor.Add(outerDoor);
                            DestroyImmediate(wall);
                            await room.Await();
                            break;
                        }
                    }
                }
            }
        }

        public int PillarChance = 10;

        void BuildTunnelFloorAndRoof(GameObject floorPreset, GameObject roofPreset, GameObject pillarPreset, Vector3Int position, Vector3 size)
        {
            tunnelPositions.Add(position);
            // Build Floor
            GameObject floor = Instantiate(floorPreset, tunnelParent.transform);
            floor.transform.position = position;
            floor.transform.rotation = Quaternion.identity;
            tunnelFloor.Add(floor);

            if (roofPreset != null)
            {
                // Build Roof
                GameObject roof = Instantiate(roofPreset, tunnelParent.transform);
                roof.transform.position = floor.transform.position + new Vector3(0, size.y * (Size.y), 0);
                roof.transform.rotation = Quaternion.identity;
                tunnelRoof.Add(roof);
            }

            if (pillarPreset != null)
            {
                // Try Build Pillar
                if (random.Next(0, 100) < PillarChance)
                {
                    for (int y = 0; y < Size.y; y++)
                    {
                        GameObject pillar = Instantiate(pillarPreset, tunnelParent.transform);
                        pillar.transform.position = floor.transform.position + new Vector3(size.x / 2f, 0, size.z / 2f) + new Vector3(0, y * size.y, 0);
                        pillar.transform.rotation = Quaternion.identity;
                        tunnelPillar.Add(pillar);
                    }
                }
            }
        }

        public async Awaitable BuildTunnelsFromTileRoom(GenRoom room, GenTileRoom tileRoom, int y)
        {
            foreach (var tunnel in tileRoom.PlacedTunnels)
            {
                GameObject roofPreset = null;
                if (room.Preset.Roof.Count > 0) roofPreset = room.Preset.Roof[random.Next(0, room.Preset.Roof.Count)];
                GameObject pillarPreset = null;
                if (room.Preset.Pillar.Count > 0) pillarPreset = room.Preset.Pillar[random.Next(0, room.Preset.Pillar.Count)];
                GameObject floorPreset = room.Preset.Floor[random.Next(0, room.Preset.Floor.Count)];
                foreach (var position in tunnel.Positions)
                {
                    Vector3 pos = new Vector3(position.x * room.TileSize.x, y * room.TileSize.y, position.y * room.TileSize.z);
                    pos += room.Content.localPosition;
                    if (!tunnelFloor.Exists(x => x.transform.position == pos))
                    {
                        BuildTunnelFloorAndRoof(floorPreset, roofPreset, pillarPreset, Vector3Int.RoundToInt(pos), room.TileSize);
                        await room.Await();
                    }
                }

                // Build Tunnel Door
                GameObject outerDoorPreset = room.Preset.OuterDoor[random.Next(0, room.Preset.OuterDoor.Count)];
                CardinalDirection doorDirection = GenTools.GetDirection(tunnel.OriginPoint, tunnel.Positions[0]);
                Vector3 doorPosition = new Vector3(tunnel.OriginPoint.x * room.TileSize.x, y * room.TileSize.y, tunnel.OriginPoint.y * room.TileSize.z);
                doorPosition += room.Content.localPosition;
                List<GenRoomNode> nodes = room.GetAllNodes();
                GenRoomNode node = nodes.FirstOrDefault(x => x.Floor.transform.position == doorPosition);
                if (node != null)
                {
                    GameObject wall = node.Wall[(int) doorDirection];
                    if (wall != null)
                    {
                        GameObject outerDoor = Instantiate(outerDoorPreset, room.Content);
                        outerDoor.transform.position = wall.transform.position;
                        outerDoor.transform.rotation = wall.transform.rotation;
                        room.OuterDoor.Add(outerDoor);
                        tunnelDoor.Add(outerDoor);
                        DestroyImmediate(wall);
                        await room.Await();
                    }
                }
            }
        }

        public void BuildTunnelWalls(Vector3 tileSize)
        {
            GenRoomPreset tunnelPreset = MainRoomType.Presets[random.Next(MainRoomType.Presets.Count)];
            GameObject wallPreset = tunnelPreset.OuterWall[random.Next(tunnelPreset.OuterWall.Count)];
            foreach (var floor in tunnelFloor)
            {
                Vector3 pos = floor.transform.position;
                List<GameObject> adjacentFloors = new() {null, null, null, null};
                foreach (var adj in tunnelFloor)
                {
                    if (adj.transform.position == new Vector3(pos.x, pos.y, pos.z + tunnelPreset.TileSize.z))
                    {
                        adjacentFloors[(int) CardinalDirection.North] = adj;
                    }
                    else if (adj.transform.position == new Vector3(pos.x, pos.y, pos.z - tunnelPreset.TileSize.z))
                    {
                        adjacentFloors[(int) CardinalDirection.South] = adj;
                    }
                    else if (adj.transform.position == new Vector3(pos.x - tunnelPreset.TileSize.x, pos.y, pos.z))
                    {
                        adjacentFloors[(int) CardinalDirection.West] = adj;
                    }
                    else if (adj.transform.position == new Vector3(pos.x + tunnelPreset.TileSize.x, pos.y, pos.z))
                    {
                        adjacentFloors[(int) CardinalDirection.East] = adj;
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.North] == null)
                {
                    Vector3 position = floor.transform.position + new Vector3(0, 0, tunnelPreset.TileSize.z / 2f);
                    if (!tunnelDoor.Exists(x => x.transform.position == position))
                    {
                        for (int y = 0; y < Size.y; y++)
                        {
                            GameObject wall = Instantiate(wallPreset, tunnelParent.transform);
                            wall.transform.position = position + (new Vector3(0, tileSize.y, 0) * y);
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.North * 90, 0);
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.South] == null)
                {
                    Vector3 position = floor.transform.position + new Vector3(0, 0, -tunnelPreset.TileSize.z / 2f);
                    if (!tunnelDoor.Exists(x => x.transform.position == position))
                    {
                        for (int y = 0; y < Size.y; y++)
                        {
                            GameObject wall = Instantiate(wallPreset, tunnelParent.transform);
                            wall.transform.position = position + (new Vector3(0, tileSize.y, 0) * y);
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.South * 90, 0);
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.East] == null)
                {
                    Vector3 position = floor.transform.position + new Vector3(tunnelPreset.TileSize.x / 2f, 0, 0);
                    if (!tunnelDoor.Exists(x => x.transform.position == position))
                    {
                        for (int y = 0; y < Size.y; y++)
                        {
                            GameObject wall = Instantiate(wallPreset, tunnelParent.transform);
                            wall.transform.position = position + (new Vector3(0, tileSize.y, 0) * y);
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.East * 90, 0);
                        }
                    }
                }
                if (adjacentFloors[(int) CardinalDirection.West] == null)
                {
                    Vector3 position = floor.transform.position + new Vector3(-tunnelPreset.TileSize.x / 2f, 0, 0);
                    if (!tunnelDoor.Exists(x => x.transform.position == position))
                    {
                        for (int y = 0; y < Size.y; y++)
                        {
                            GameObject wall = Instantiate(wallPreset, tunnelParent.transform);
                            wall.transform.position = position + (new Vector3(0, tileSize.y, 0) * y);
                            wall.transform.localRotation = Quaternion.Euler(0, (int) CardinalDirection.West * 90, 0);
                        }
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GenArea))]
    public class GenArea_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GenArea genTileAreaBuilder = (GenArea) target;
            if (GUILayout.Button("Generate")) genTileAreaBuilder.Generate();
            if (GUILayout.Button("Clear")) genTileAreaBuilder.Clear();
        }
    }
#endif
}