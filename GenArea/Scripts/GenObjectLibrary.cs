using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GenTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.GenTools.GenArea.Scripts
{
    public static class GenObjectLibrary
    {
        public static GameObject SpawnObject(GenObjectData genObjectData, Transform parent, Vector3 position, Quaternion rotation, System.Random random)
        {
            GameObject toSpawn = genObjectData.Prefabs[UnityEngine.Random.Range(0, genObjectData.Prefabs.Count)];
            GameObject spawn = Object.Instantiate(toSpawn, parent);
            spawn.transform.position = position;
            spawn.transform.rotation = rotation;
            return spawn;
        }

        public static List<GameObject> PlaceObject(List<GenRoomNode> nodes, Transform parent, List<GameObject> objects, GenObjectIteration genObjectIteration, System.Random random)
        {
            List<GameObject> placed = new();
            List<GenRoomNode> available = nodes.Where(x => x.Floor != null && x.Object == null).ToList();
            int percent = random.Next(genObjectIteration.Percentage.x, genObjectIteration.Percentage.y + 1);
            int count = Mathf.CeilToInt((available.Count / 100f) * percent);
            count += random.Next(genObjectIteration.Amount.x, genObjectIteration.Amount.y + 1);

            for (int i = 0; i < count; i++)
            {
                foreach (var obj in genObjectIteration.Objects.OrderBy(x => random.Next()))
                {
                    available = GetAvailableNodes(nodes, obj.Object.Position, random);
                    if (available.Count > 0)
                    {
                        GenRoomNode node = GetFirstValidNode(obj.Object, available);
                        if (node != null)
                        {
                            GameObject spawn = ExecutePlaceObject(parent, objects, obj.Object, random, node, available);
                            placed.Add(spawn);
                            available.Remove(node);
                            foreach (var nearbyObject in obj.Nearby)
                            {
                                List<GenRoomNode> closest = GetAvailableNodes(nodes, nearbyObject.Position, random);
                                closest = closest.OrderBy(x => Vector3.Distance(x.Position, node.Position)).ToList();
                                int nearbyCount = random.Next(genObjectIteration.Percentage.x, genObjectIteration.Percentage.y + 1);
                                for (int j = 0; j < nearbyCount; j++)
                                {
                                    if (closest.Count == 0) break;

                                    GenRoomNode nearby = GetFirstValidNode(nearbyObject, closest);
                                    if (nearby != null)
                                    {
                                        GameObject nearbySpawn = ExecutePlaceObject(parent, objects, nearbyObject, random, nearby, closest);
                                        placed.Add(nearbySpawn);
                                        closest.Remove(nearby);
                                        available.Remove(nearby);
                                    }
                                    else break;
                                }
                            }
                            if (spawn != null) break;
                        }
                    }
                }
            }
            return placed;
        }

        static GenRoomNode GetFirstValidNode(GenObjectData data, List<GenRoomNode> available)
        {
            GenRoomNode node = null;
            foreach (var n in available)
            {
                node = n;
                bool breakLoop = false;
                for (int x = 0; x < data.Size.x; x++)
                {
                    for (int y = 0; y < data.Size.y; y++)
                    {
                        for (int z = 0; z < data.Size.z; z++)
                        {
                            if (!available.Exists(check => check.Position == (node.Position + new Vector3Int(x, y, z))))
                            {
                                node = null;
                                breakLoop = true;
                                break;
                            }
                        }
                        if (breakLoop) break;
                    }
                    if (breakLoop) break;
                }
                if (node != null) break;
            }
            return node;
        }

        static List<GenRoomNode> GetAvailableNodes(List<GenRoomNode> nodes, GenObjectPosition position, System.Random random)
        {
            List<GenRoomNode> available = nodes.Where(x => x.Object == null).OrderBy(x => random.Next()).ToList();
            switch (position)
            {
                case GenObjectPosition.Floor:
                    available = available.Where(x => x.Floor != null).ToList();
                    break;
                case GenObjectPosition.Wall:
                    available = available.Where(x => x.Floor != null && x.Wall.Exists(wall => wall != null)).ToList();
                    break;
                case GenObjectPosition.Inner:
                    available = available.Where(x => x.Floor != null && !x.Wall.Exists(wall => wall != null)).ToList();
                    break;
                case GenObjectPosition.Roof:
                    available = available.Where(x => x.Floor != null).ToList();
                    available = available.Where(x => x.Roof != null || nodes.FirstOrDefault(n => n.Floor != null && n.Position == new Vector3Int(x.Position.x, x.Position.y + 1, x.Position.z)) != null).ToList();
                    break;
            }

            return available;
        }

        static GameObject ExecutePlaceObject(Transform parent, List<GameObject> objects, GenObjectData data, System.Random random, GenRoomNode node, List<GenRoomNode> available)
        {
            List<Quaternion> possibleRotations = new() {Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 90, 0), Quaternion.Euler(0, 180, 0), Quaternion.Euler(0, 270, 0)};
            switch (data.Position)
            {
                case GenObjectPosition.Wall:
                    possibleRotations.Clear();
                    for (int dir = 0; dir < 4; dir++)
                    {
                        if (node.Wall[dir] != null) possibleRotations.Add(Quaternion.Euler(0, 90 * dir, 0));
                    }
                    break;
            }
            possibleRotations = possibleRotations.OrderBy(x => random.Next()).ToList();
            Quaternion rotation = possibleRotations[0];

            Vector3 position = node.Floor.transform.position;
            if (data.Size.x > 1) position += new Vector3((data.Size.x - 1) * 2f, 0, 0);
            if (data.Size.z > 1) position += new Vector3(0, 0, (data.Size.z - 1) * 2f);

            GameObject spawn = SpawnObject(data, parent, position, rotation, random);
            objects.Add(spawn);
            node.Object = spawn;

            for (int x = 0; x < data.Size.x; x++)
            {
                for (int y = 0; y < data.Size.y; y++)
                {
                    for (int z = 0; z < data.Size.z; z++)
                    {
                        Vector3Int pos = new Vector3Int(node.Position.x + x, node.Position.y + y, node.Position.z + z);
                        var n = available.FirstOrDefault(n => n.Position == pos);
                        n.Object = spawn;
                    }
                }
            }

            return spawn;
        }
    }
}