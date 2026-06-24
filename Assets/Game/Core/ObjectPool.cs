using System.Collections.Generic;
using UnityEngine;

namespace SteelRain.Core
{
    /// <summary>
    /// 通用对象池：预分配游戏对象实例，避免运行时频繁 Instantiate/Destroy 造成 GC 压力。
    /// 适用于子弹、爆炸特效等高频生成销毁的对象。
    /// </summary>
    public sealed class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public struct PoolConfig
        {
            public string id;
            public GameObject prefab;
            public int initialSize;
            public int maxSize;
        }

        [SerializeField] private PoolConfig[] pools;

        private readonly Dictionary<string, Queue<GameObject>> available = new();
        private readonly Dictionary<string, HashSet<GameObject>> inUse = new();
        private readonly Dictionary<string, PoolConfig> configs = new();

        private static ObjectPool instance;

        public static ObjectPool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<ObjectPool>();
                    if (instance == null)
                    {
                        var go = new GameObject("[ObjectPool]");
                        instance = go.AddComponent<ObjectPool>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }

        private void InitializePools()
        {
            if (pools == null) return;
            foreach (var config in pools)
            {
                if (config.prefab == null || string.IsNullOrEmpty(config.id)) continue;
                configs[config.id] = config;
                available[config.id] = new Queue<GameObject>();
                inUse[config.id] = new HashSet<GameObject>();

                for (int i = 0; i < config.initialSize; i++)
                {
                    var obj = CreateInstance(config);
                    available[config.id].Enqueue(obj);
                }
            }
        }

        private GameObject CreateInstance(PoolConfig config)
        {
            var obj = Instantiate(config.prefab, transform);
            obj.SetActive(false);
            var pooled = obj.GetComponent<PooledObject>();
            if (pooled == null) pooled = obj.AddComponent<PooledObject>();
            pooled.PoolId = config.id;
            return obj;
        }

        /// <summary>
        /// 从池中获取一个对象。若池空且未达上限则新建，否则返回 null。
        /// </summary>
        public GameObject Spawn(string id, Vector3 position, Quaternion rotation)
        {
            if (!available.TryGetValue(id, out var queue))
            {
                Debug.LogWarning($"[ObjectPool] Pool '{id}' not found");
                return null;
            }

            GameObject obj;
            if (queue.Count > 0)
            {
                obj = queue.Dequeue();
            }
            else
            {
                var config = configs[id];
                var max = config.maxSize > 0 ? config.maxSize : 200;
                if (inUse[id].Count >= max)
                {
                    Debug.LogWarning($"[ObjectPool] Pool '{id}' reached max size {max}");
                    return null;
                }
                obj = CreateInstance(config);
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            inUse[id].Add(obj);
            return obj;
        }

        /// <summary>
        /// 将对象归还到池中。对象需带有 PooledObject 组件（Spawn 时自动添加）。
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;
            var pooled = obj.GetComponent<PooledObject>();
            if (pooled == null)
            {
                Destroy(obj);
                return;
            }
            var id = pooled.PoolId;
            if (!available.ContainsKey(id))
            {
                Destroy(obj);
                return;
            }
            obj.SetActive(false);
            inUse[id].Remove(obj);
            available[id].Enqueue(obj);
        }

        /// <summary>
        /// 预热指定池（在关卡开始时调用，避免首次Spawn卡顿）。
        /// </summary>
        public void Prewarm(string id, int count)
        {
            if (!configs.TryGetValue(id, out var config)) return;
            if (!available.ContainsKey(id)) return;
            for (int i = 0; i < count; i++)
                available[id].Enqueue(CreateInstance(config));
        }

        /// <summary>
        /// 清空所有池（场景切换时调用）。
        /// </summary>
        public void ClearAll()
        {
            foreach (var kvp in inUse)
            {
                foreach (var obj in kvp.Value)
                {
                    if (obj != null) Destroy(obj);
                }
            }
            foreach (var kvp in available)
            {
                foreach (var obj in kvp.Value)
                {
                    if (obj != null) Destroy(obj);
                }
            }
            available.Clear();
            inUse.Clear();
        }
    }

    /// <summary>
    /// 标记对象所属的池ID，用于归还时识别。
    /// </summary>
    public sealed class PooledObject : MonoBehaviour
    {
        public string PoolId;

        /// <summary>
        /// 便捷方法：将自身归还到对象池。
        /// </summary>
        public void Despawn()
        {
            ObjectPool.Instance.Despawn(gameObject);
        }
    }
}
