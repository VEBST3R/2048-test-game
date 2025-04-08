using UnityEngine;

/// <summary>
/// Фабрика для створення кубів
/// </summary>
public class CubeFactory : MonoBehaviour, ICubeFactory
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private PhysicsMaterial lowBounceMaterial;

    [Header("Physics Settings")]
    [SerializeField] private float cubeDrag = 0.5f;
    [SerializeField] private float cubeAngularDrag = 5.0f;
    [SerializeField] private float cubeMass = 2.0f;
    [SerializeField] private float cubeBounciness = 0.05f;
    [SerializeField] private float cubeFriction = 0.8f;

    private const string CUBE_POOL_TAG = "Cube";

    public GameObject GetCube(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject cube = null;

        if (ObjectPooler.Instance != null)
        {
            cube = ObjectPooler.Instance.SpawnFromPool(CUBE_POOL_TAG, position, rotation, parent);
        }

        if (cube == null && cubePrefab != null)
        {
            cube = Instantiate(cubePrefab, position, rotation, parent);
        }

        if (cube != null)
        {
            ConfigureCubePhysics(cube);
        }

        return cube;
    }

    private void ConfigureCubePhysics(GameObject cube)
    {
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearDamping = cubeDrag;
            rb.angularDamping = cubeAngularDrag;
            rb.mass = cubeMass;

            rb.constraints = RigidbodyConstraints.None;

        }

        Collider[] colliders = cube.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                if (lowBounceMaterial != null)
                {
                    col.material = lowBounceMaterial;
                }
                else
                {
                    PhysicsMaterial stableMaterial = new PhysicsMaterial("StableCube");
                    stableMaterial.bounciness = cubeBounciness;
                    stableMaterial.dynamicFriction = cubeFriction;
                    stableMaterial.staticFriction = cubeFriction;
                    stableMaterial.frictionCombine = PhysicsMaterialCombine.Maximum;
                    stableMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
                    col.material = stableMaterial;
                }
            }
        }
    }


    public void SetRandomValue(GameObject cube, float probability2 = 0.75f)
    {
        CubeFacade cubeFacade = cube.GetComponent<CubeFacade>();
        if (cubeFacade != null)
        {
            int value = UnityEngine.Random.value < probability2 ? 2 : 4;
            cubeFacade.SetValue(value);
        }
    }
}