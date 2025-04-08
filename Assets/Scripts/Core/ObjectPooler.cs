using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    public event Action<GameObject, string> OnObjectSpawned;
    public event Action<GameObject, string> OnObjectReturnedToPool;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public Transform parent;
    }

    [SerializeField] private List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Pool> poolConfigs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolConfigs = new Dictionary<string, Pool>();

        foreach (Pool pool in pools)
        {
            InitializePool(pool);
        }
    }

    private void InitializePool(Pool pool)
    {
        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < pool.size; i++)
        {
            GameObject obj = CreateNewObject(pool.tag, pool.prefab, pool.parent);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        poolDictionary[pool.tag] = objectPool;
        poolConfigs[pool.tag] = pool;
    }

    private GameObject CreateNewObject(string tag, GameObject prefab, Transform parent)
    {
        if (prefab == null) return null;

        GameObject obj = Instantiate(prefab, parent);
        obj.name = $"{tag}_pooled";

        if (!string.IsNullOrEmpty(tag))
        {
            obj.tag = tag;
        }

        return obj;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform customParent = null)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        Pool poolConfig = poolConfigs.ContainsKey(tag) ? poolConfigs[tag] : null;

        Queue<GameObject> objectPool = poolDictionary[tag];

        if (objectPool.Count == 0 && poolConfig != null)
        {
            GameObject newObj = CreateNewObject(tag, poolConfig.prefab, poolConfig.parent);
            objectPool.Enqueue(newObj);
        }

        GameObject objectToSpawn = objectPool.Count > 0 ? objectPool.Dequeue() : null;

        if (objectToSpawn == null) return null;

        Transform parent = customParent != null ? customParent :
                         (poolConfig != null ? poolConfig.parent : null);
        objectToSpawn.transform.SetParent(parent);

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        objectToSpawn.SetActive(true);

        IPoolable poolableObj = objectToSpawn.GetComponent<IPoolable>();
        if (poolableObj != null)
        {
            poolableObj.OnObjectSpawn();
        }

        OnObjectSpawned?.Invoke(objectToSpawn, tag);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return;
        }

        if (objectToReturn == null) return;

        Queue<GameObject> pool = poolDictionary[tag];
        Pool poolConfig = poolConfigs.ContainsKey(tag) ? poolConfigs[tag] : null;

        CubeFacade cubeFacade = objectToReturn.GetComponent<CubeFacade>();
        if (cubeFacade != null)
        {
            cubeFacade.IsInPool = true;
        }

        objectToReturn.SetActive(false);

        objectToReturn.transform.position = new Vector3(1000, -1000, 1000);

        if (poolConfig != null && poolConfig.parent != null)
        {
            objectToReturn.transform.SetParent(poolConfig.parent);
        }

        if (!ContainsObject(pool, objectToReturn))
        {
            pool.Enqueue(objectToReturn);
            OnObjectReturnedToPool?.Invoke(objectToReturn, tag);
        }
    }

    private bool ContainsObject(Queue<GameObject> queue, GameObject obj)
    {
        foreach (GameObject item in queue)
        {
            if (item == obj) return true;
        }
        return false;
    }

    public void PrintPoolStatus()
    {
        foreach (var pool in poolDictionary)
        {
            Debug.Log($"Pool {pool.Key}: {pool.Value.Count} objects");
        }
    }
}

