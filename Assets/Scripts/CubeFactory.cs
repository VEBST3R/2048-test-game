using UnityEngine;

public class CubeFactory : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private PhysicsMaterial lowBounceMaterial;
    [SerializeField] private GameObject cubeParent;
    [SerializeField] private float cubeDrag = 0.5f;
    [SerializeField] private float cubeAngularDrag = 0.2f;
    [SerializeField] private float cubeMass = 1.0f;

    private const string CUBE_POOL_TAG = "Cube";

    private void Start()
    {
        // Переконуємося, що ObjectPooler існує
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("Відсутній ObjectPooler на сцені!");
        }
    }

    public GameObject GetCube(Vector3 position, Quaternion rotation)
    {
        if (ObjectPooler.Instance != null)
        {
            GameObject cube = ObjectPooler.Instance.SpawnFromPool(CUBE_POOL_TAG, position, rotation);
            if (cube != null)
            {
                ConfigureCubePhysics(cube);
                return cube;
            }
        }

        // Резервний варіант: створюємо новий куб, якщо пул не працює
        GameObject newCube = Instantiate(cubePrefab, position, rotation, cubeParent.transform);
        ConfigureCubePhysics(newCube);
        return newCube;
    }

    public void ReturnCube(GameObject cube)
    {
        if (cube != null && ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(CUBE_POOL_TAG, cube);
        }
    }

    private void ConfigureCubePhysics(GameObject cube)
    {
        Rigidbody rb = cube.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearDamping = cubeDrag;
            rb.angularDamping = cubeAngularDrag;
            rb.mass = cubeMass;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        Collider[] colliders = cube.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != null && lowBounceMaterial != null)
            {
                col.material = lowBounceMaterial;
            }
        }
    }

    public void SetRandomValue(GameObject cube, float probability2 = 0.75f)
    {
        CubeValue cubeValue = cube.GetComponent<CubeValue>();
        if (cubeValue != null)
        {
            int startValue = Random.Range(0f, 1f) < probability2 ? 2 : 4;
            cubeValue.SetValue(startValue);

            // Додатково переконуємося, що флаг злиття скинуто
            cubeValue.ResetMergeState();

            Debug.Log($"Created cube with value: {startValue}");
        }
    }
}
