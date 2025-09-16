using System;
using System.Collections.Generic;
using System.Linq;
using GenerativeTools;
using GenTools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.GenTools.GenArea.Scripts
{
    public static class GenObjectLibrary
    {
        public static GameObject SpawnObject(GenObject genObject, Transform parent, Vector3 position, Quaternion rotation)
        {
            GameObject spawn = Object.Instantiate(genObject.Prefab, parent);
            spawn.transform.position = position;
            spawn.transform.rotation = rotation;
            return spawn;
        }

        public static List<GameObject> PlaceInTunnel(GenTunnel tunnel, GenObject genObject, System.Random random)
        {
            List<GameObject> placed = new();
            List<GenTunnelNode> available = null;
            switch (genObject.Position)
            {
                case GenObjectPosition.All:
                    available = tunnel.Node.Where(x => x.Object == null).OrderBy(x => random.Next()).ToList();
                    break;
                case GenObjectPosition.Outer:
                    available = tunnel.Node.Where(x => x.Object == null && x.Wall.Exists(wall => wall != null)).OrderBy(x => random.Next()).ToList();
                    break;
                case GenObjectPosition.Inner:
                    available = tunnel.Node.Where(x => x.Object == null && !x.Wall.Exists(wall => wall != null)).OrderBy(x => random.Next()).ToList();
                    break;
                case GenObjectPosition.Corner:
                    break;
                case GenObjectPosition.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (available == null || available.Count == 0) return null;
            int count = random.Next(genObject.Amount.x, genObject.Amount.y + 1);
            for (int i = 0; i < count; i++)
            {
                if (available.Count == 0) return placed;
                GameObject spawn = SpawnObject(genObject, tunnel.Parent, available[0].Floor.transform.position, Quaternion.identity);
                tunnel.TunnelObjects.Add(spawn);
                placed.Add(spawn);
                available.RemoveAt(0);
            }
            return placed;
        }
    }
}