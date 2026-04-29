using System.Collections.Generic;
using UnityEngine;

namespace SteelRain.Core
{
    public sealed class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> inactive = new();

        public ObjectPool(T prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            var item = inactive.Count > 0 ? inactive.Pop() : Object.Instantiate(prefab, parent);
            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            item.gameObject.SetActive(false);
            inactive.Push(item);
        }
    }
}
