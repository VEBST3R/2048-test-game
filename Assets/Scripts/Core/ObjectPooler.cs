using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Керує пулом об'єктів для оптимізації створення та знищення
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    // Синглтон
    public static ObjectPooler Instance { get; private set; }

    // Події
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
        // Перевіряємо синглтон
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ініціалізуємо словники
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolConfigs = new Dictionary<string, Pool>();

        // Ініціалізуємо пули
        foreach (Pool pool in pools)
        {
            InitializePool(pool);
        }
    }

    private void InitializePool(Pool pool)
    {
        // Створюємо чергу для пулу
        Queue<GameObject> objectPool = new Queue<GameObject>();

        // Заповнюємо пул об'єктами
        for (int i = 0; i < pool.size; i++)
        {
            GameObject obj = CreateNewObject(pool.tag, pool.prefab, pool.parent);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        // Зберігаємо пул і його конфігурацію
        poolDictionary[pool.tag] = objectPool;
        poolConfigs[pool.tag] = pool;
    }

    private GameObject CreateNewObject(string tag, GameObject prefab, Transform parent)
    {
        if (prefab == null) return null;

        GameObject obj = Instantiate(prefab, parent);
        obj.name = $"{tag}_pooled";

        // Переконуємося, що об'єкт має правильний тег
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

        // Отримуємо конфігурацію пулу
        Pool poolConfig = poolConfigs.ContainsKey(tag) ? poolConfigs[tag] : null;

        Queue<GameObject> objectPool = poolDictionary[tag];

        // Якщо пул порожній - створюємо новий об'єкт
        if (objectPool.Count == 0 && poolConfig != null)
        {
            GameObject newObj = CreateNewObject(tag, poolConfig.prefab, poolConfig.parent);
            objectPool.Enqueue(newObj);
        }

        // Дістаємо об'єкт з черги
        GameObject objectToSpawn = objectPool.Count > 0 ? objectPool.Dequeue() : null;

        if (objectToSpawn == null) return null;

        // Встановлюємо батьківський об'єкт
        Transform parent = customParent != null ? customParent :
                         (poolConfig != null ? poolConfig.parent : null);
        objectToSpawn.transform.SetParent(parent);

        // Встановлюємо позицію та поворот
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Активуємо об'єкт
        objectToSpawn.SetActive(true);

        // Скидаємо стан об'єкта, якщо він підтримує IPoolable
        IPoolable poolableObj = objectToSpawn.GetComponent<IPoolable>();
        if (poolableObj != null)
        {
            poolableObj.OnObjectSpawn();
        }

        // Викликаємо подію появи об'єкта
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

        // Перевіряємо, щоб об'єкт не був null
        if (objectToReturn == null) return;

        // Отримуємо пул і його конфігурацію
        Queue<GameObject> pool = poolDictionary[tag];
        Pool poolConfig = poolConfigs.ContainsKey(tag) ? poolConfigs[tag] : null;

        // Позначаємо об'єкт як "в пулі" - тільки для нової архітектури
        CubeFacade cubeFacade = objectToReturn.GetComponent<CubeFacade>();
        if (cubeFacade != null)
        {
            cubeFacade.IsInPool = true;
        }

        // Вимикаємо об'єкт
        objectToReturn.SetActive(false);

        // Переміщуємо об'єкт за межі видимості
        objectToReturn.transform.position = new Vector3(1000, -1000, 1000);

        // Повертаємо об'єкт до батьківського елемента пулу
        if (poolConfig != null && poolConfig.parent != null)
        {
            objectToReturn.transform.SetParent(poolConfig.parent);
        }

        // Перевіряємо, щоб об'єкт не був уже в пулі
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

