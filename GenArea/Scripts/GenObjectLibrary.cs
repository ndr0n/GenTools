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

        #region Tunnel

        public static List<GameObject> PlaceInTunnel(GenTunnel genTunnel, GenObjectIteration genObjectIteration, System.Random random)
        {
            List<GameObject> placed = new();
            int count = random.Next(genObjectIteration.Amount.x, genObjectIteration.Amount.y + 1);
            for (int i = 0; i < count; i++)
            {
                GenObject obj = genObjectIteration.Objects[random.Next(genObjectIteration.Objects.Count)];
                List<GenTunnelNode> available = GetAvailableNodesInTunnel(genTunnel, obj.Object, random);
                if (available.Count > 0)
                {
                    GenTunnelNode node = GetFirstValidNode(genTunnel, obj.Object, available);
                    if (node != null)
                    {
                        GameObject spawn = PlaceObjectInTunnel(genTunnel, obj.Object, random, node);
                        placed.Add(spawn);
                        available.Remove(node);
                        foreach (var nearbyObject in obj.Nearby)
                        {
                            List<GenTunnelNode> closest = GetAvailableNodesInTunnel(genTunnel, nearbyObject, random);
                            closest = closest.OrderBy(x => Vector3.Distance(x.Position, node.Position)).ToList();
                            int nearbyCount = random.Next(genObjectIteration.Amount.x, genObjectIteration.Amount.y + 1);
                            for (int j = 0; j < nearbyCount; j++)
                            {
                                if (closest.Count == 0) break;
                                if (available.Count == 0) return placed;
                                GenTunnelNode nearby = GetFirstValidNode(genTunnel, nearbyObject, closest);
                                if (nearby != null)
                                {
                                    GameObject nearbySpawn = PlaceObjectInTunnel(genTunnel, nearbyObject, random, nearby);
                                    placed.Add(nearbySpawn);
                                    closest.Remove(nearby);
                                    available.Remove(nearby);
                                }
                                else break;
                            }
                        }
                    }
                }
            }
            return placed;
        }

        static GenTunnelNode GetFirstValidNode(GenTunnel genTunnel, GenObjectData data, List<GenTunnelNode> available)
        {
            GenTunnelNode node = null;
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
                            if (!available.Exists(check => check.Position == (node.Position + new Vector3(x, y, z))))
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

        static List<GenTunnelNode> GetAvailableNodesInTunnel(GenTunnel genTunnel, GenObjectData data, System.Random random)
        {
            List<GenTunnelNode> available = genTunnel.Node.Where(x => x.Object == null).OrderBy(x => random.Next()).ToList();
            switch (data.Position)
            {
                case GenObjectPosition.Outer:
                    available = genTunnel.Node.Where(x => x.Object == null && x.Wall.Exists(wall => wall != null)).OrderBy(x => random.Next()).ToList();
                    break;
                case GenObjectPosition.Inner:
                    available = genTunnel.Node.Where(x => x.Object == null && !x.Wall.Exists(wall => wall != null)).OrderBy(x => random.Next()).ToList();
                    break;
            }

            return available;
        }

        static GameObject PlaceObjectInTunnel(GenTunnel tunnel, GenObjectData genObjectData, System.Random random, GenTunnelNode node)
        {
            List<Quaternion> possibleRotations = new() {Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 90, 0), Quaternion.Euler(0, 180, 0), Quaternion.Euler(0, 270, 0)};
            switch (genObjectData.Position)
            {
                case GenObjectPosition.Outer:
                    possibleRotations.Clear();
                    foreach (var wall in node.Wall)
                    {
                        if (wall != null) possibleRotations.Add(wall.transform.rotation);
                    }
                    break;
            }
            possibleRotations = possibleRotations.OrderBy(x => random.Next()).ToList();
            Quaternion rotation = possibleRotations[0];
            GameObject spawn = SpawnObject(genObjectData, tunnel.Parent, node.Floor.transform.position, rotation, random);
            node.Object = spawn;
            tunnel.TunnelObjects.Add(spawn);
            return spawn;
        }

        #endregion
    }
}