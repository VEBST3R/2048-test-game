using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

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

    private void Awake()
    {
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Initialize all pools
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Create all objects and add to pool
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = CreateNewObject(pool.tag, pool.prefab, pool.parent);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    private GameObject CreateNewObject(string tag, GameObject prefab, Transform parent)
    {
        GameObject obj = Instantiate(prefab, parent);
        obj.name = $"{tag}_pooled";

        // Ensure all cubes have the correct tag
        if (tag == "Cube" && obj.tag != "Cube")
        {
            obj.tag = "Cube";
        }

        return obj;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[tag];

        // If the pool is empty, add a new object
        if (objectPool.Count == 0)
        {
            // Find the pool config
            Pool poolConfig = pools.Find(p => p.tag == tag);
            if (poolConfig != null)
            {
                GameObject obj = CreateNewObject(tag, poolConfig.prefab, poolConfig.parent);
                objectPool.Enqueue(obj);
            }
        }

        // Get object from pool
        GameObject objectToSpawn = objectPool.Dequeue();

        // Розміщуємо об'єкт спочатку, а вже потім активуємо, щоб анімація починалась з правильної позиції
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Ensure the object is active before use
        if (!objectToSpawn.activeInHierarchy)
        {
            objectToSpawn.SetActive(true);
        }

        // Get components that need resetting
        Rigidbody rb = objectToSpawn.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Add an interface for poolable objects if needed
        IPoolable poolable = objectToSpawn.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnObjectSpawn();
        }

        // Fully enable object components
        EnableObjectComponents(objectToSpawn);

        // Позначаємо об'єкт як "не в пулі"
        CubeValue cubeValue = objectToSpawn.GetComponent<CubeValue>();
        if (cubeValue != null)
        {
            cubeValue.IsInPool = false;
        }

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return;
        }

        // Позначаємо об'єкт як "в пулі"
        CubeValue cubeValue = objectToReturn.GetComponent<CubeValue>();
        if (cubeValue != null)
        {
            cubeValue.IsInPool = true;
        }

        // Ensure the object is truly deactivated
        objectToReturn.SetActive(false);

        // Move the object far away from the game field for assurance
        objectToReturn.transform.position = new Vector3(999, -999, 999);

        // Deactivate components for additional assurance
        DisableObjectComponents(objectToReturn);

        // Ensure the object is not added multiple times
        if (!ContainsObject(poolDictionary[tag], objectToReturn))
        {
            poolDictionary[tag].Enqueue(objectToReturn);
            Debug.Log($"Object returned to pool {tag}. Pool size: {poolDictionary[tag].Count}");
        }
    }

    // Add a method to disable all important components
    private void DisableObjectComponents(GameObject obj)
    {
        // Disable collider
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Disable renderer
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        // Make Rigidbody kinematic
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Add a method to enable all important components
    private void EnableObjectComponents(GameObject obj)
    {
        // Enable collider
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Enable renderer
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }

        // Make Rigidbody non-kinematic
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    // Check if the queue contains a specific object
    private bool ContainsObject(Queue<GameObject> queue, GameObject obj)
    {
        foreach (GameObject item in queue)
        {
            if (item == obj)
                return true;
        }
        return false;
    }

    // Add a public method for diagnostics
    public void PrintPoolStatus()
    {
        foreach (var pool in poolDictionary)
        {
            Debug.Log($"Pool {pool.Key}: {pool.Value.Count} objects");
        }
    }
}

// Interface for objects that need initialization when spawned from pool
public interface IPoolable
{
    void OnObjectSpawn();
}
