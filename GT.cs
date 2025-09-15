using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenTools
{
    public static class GT
    {
        public static string TrimCloneName(string name)
        {
            return name.Replace("(Clone)", "");
        }

        public static T GetRandom<T>(List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static int GetRandomIndex<T>(List<T> list)
        {
            return UnityEngine.Random.Range(0, list.Count);
        }

        [System.Serializable]
        public struct WeightedProbability<T>
        {
            public T Value;
            public float Probability;

            public bool TestProbability()
            {
                if (UnityEngine.Random.Range(0f, 100f) <= Probability) return true;
                return false;
            }
        }

        public static Vector3 RandomPointInBounds(Bounds bounds)
        {
            Vector3 randomPoint = new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );
            return randomPoint;
        }

        public static T GetWeightedRandom<T>(List<WeightedProbability<T>> list, System.Random random)
        {
            float t = 0;
            for (int i = 0; i < list.Count; i++) t += list[i].Probability;
            double r = random.NextDouble();
            float s = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                s += list[i].Probability / t;
                if (s >= r) return list[i].Value;
            }
            return default;
        }

        public static double GetDifferenceInSecondsFromFileTimeUtc(long fileTimeUtc)
        {
            return (DateTime.UtcNow - DateTime.FromFileTimeUtc(fileTimeUtc)).TotalSeconds;
        }

        public static float ScaleFloat(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            float OldRange = oldMax - oldMin;
            float NewRange = newMax - newMin;
            float NewValue = (((value - oldMin) * NewRange) / OldRange) + newMin;
            return NewValue;
        }

        public static int[] GetRandomIterArray(int min, int max, System.Random rand)
        {
            List<int> iterList = new();
            for (int i = min; i < max; i++) iterList.Add(i);
            return iterList.OrderBy(x => rand.Next()).ToArray();
        }

        public static GameObject CreateEmptyGameObject(Transform parent, string title)
        {
            GameObject x = new GameObject(title);
            x.transform.SetParent(parent);
            x.transform.localPosition = Vector3.zero;
            x.transform.localRotation = Quaternion.identity;
            return x;
        }

        public static Collider[] BoxColliderOverlaps(BoxCollider target, int layerMask, bool hitTriggers)
        {
            Vector3 scaledSize = new Vector3(target.size.x * target.transform.lossyScale.x, target.size.y * target.transform.lossyScale.y, target.size.z * target.transform.lossyScale.z) * 0.5f;
            return Physics.OverlapBox(target.transform.position + target.center, scaledSize, target.transform.rotation, layerMask, hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);
        }

        public static Collider[] BoundsOverlaps(GameObject obj, Bounds bounds, int layerMask, bool hitTriggers)
        {
            return Physics.OverlapBox(obj.transform.position + bounds.center, bounds.extents, obj.transform.rotation, layerMask, hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);
        }
    }
}