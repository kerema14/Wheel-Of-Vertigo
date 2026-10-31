using System.Collections.Generic;
using UnityEngine;

namespace WheelOfVertigo.UI.Common
{
    public static class TransformExtensions
    {
        /// <summary>Breadth-first search of the whole hierarchy under <paramref name="root"/> (Transform.Find only looks one level down).</summary>
        public static Transform FindDeep(this Transform root, string name)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();
                if (current.name == name) return current;

                foreach (Transform child in current)
                    queue.Enqueue(child);
            }

            return null;
        }

        /// <summary>
        /// Used from OnValidate so references set themselves from the hierarchy names.
        /// Leaves the field alone when nothing matches, so a hand-assigned reference is never lost.
        /// </summary>
        public static void AssignIfFound<T>(this Transform root, ref T field, string name) where T : Component
        {
            Transform found = root.FindDeep(name);
            if (found != null && found.TryGetComponent(out T component))
                field = component;
        }
    }
}
