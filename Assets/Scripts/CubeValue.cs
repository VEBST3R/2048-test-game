using UnityEngine;
using TMPro;
using System.Collections;

public class CubeValue : MonoBehaviour, IPoolable
{
    [Header("Value Settings")]
    [SerializeField] private int value = 2;

    [Header("Visual Settings")]
    [SerializeField] private TextMeshPro[] valueTexts;
    [SerializeField] private MeshRenderer meshRenderer;

    [Header("Merging Settings")]
    [SerializeField] private float minCollisionForce = 2f;
    [SerializeField] private GameObject mergeEffectPrefab;

    [Header("Animation Settings")]
    [SerializeField] private float spawnAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve spawnScaleCurve;
    [SerializeField] private float mergeAnimationDuration = 0.2f; // Тривалість анімації злиття
    [SerializeField] private AnimationCurve mergeScaleCurve; // Крива злиття

    private bool hasBeenMerged = false;

    [HideInInspector] public bool IsInPool = false;

    private Vector3 originalScale;
    private Coroutine scaleAnimationCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        if (valueTexts.Length == 0)
        {
            valueTexts = GetComponentsInChildren<TextMeshPro>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        UpdateVisuals();
        StartScaleAnimation();
    }

    public void SetValue(int newValue)
    {
        value = newValue;
        UpdateVisuals();
        hasBeenMerged = false;
    }

    public int GetValue()
    {
        return value;
    }

    private void UpdateVisuals()
    {
        foreach (TextMeshPro text in valueTexts)
        {
            if (text != null)
            {
                text.text = value.ToString();
            }
        }

        if (meshRenderer != null)
        {
            Color cubeColor = GetColorForValue(value);
            meshRenderer.material.color = cubeColor;
        }
    }

    private Color GetColorForValue(int value)
    {
        switch (value)
        {
            case 2:
                return new Color(0.93f, 0.89f, 0.85f);
            case 4:
                return new Color(0.93f, 0.87f, 0.78f);
            case 8:
                return new Color(0.95f, 0.69f, 0.47f);
            case 16:
                return new Color(0.96f, 0.58f, 0.39f);
            case 32:
                return new Color(0.96f, 0.49f, 0.37f);
            case 64:
                return new Color(0.96f, 0.35f, 0.23f);
            case 128:
                return new Color(0.93f, 0.81f, 0.45f);
            case 256:
                return new Color(0.93f, 0.80f, 0.38f);
            case 512:
                return new Color(0.93f, 0.78f, 0.31f);
            case 1024:
                return new Color(0.93f, 0.77f, 0.25f);
            case 2048:
                return new Color(0.93f, 0.76f, 0.18f);
            case 4096:
                return new Color(0.64f, 0.83f, 0.45f);
            case 8192:
                return new Color(0.40f, 0.72f, 0.40f);
            default:
                return new Color(0.61f, 0.35f, 0.71f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CubeValue otherCube = collision.gameObject.GetComponent<CubeValue>();
        if (otherCube != null)
        {
            Debug.Log($"Collision: This={value}, Other={otherCube.GetValue()}, Force={collision.relativeVelocity.magnitude}, ThisMerged={hasBeenMerged}, OtherMerged={otherCube.hasBeenMerged}");
        }

        float requiredForce = value <= 4 ? minCollisionForce * 0.7f : minCollisionForce;
        if (collision.relativeVelocity.magnitude < requiredForce)
        {
            return;
        }

        if (otherCube != null && !hasBeenMerged && !otherCube.hasBeenMerged)
        {
            if (otherCube.GetValue() == value)
            {
                MergeWithCube(otherCube);
            }
        }
    }

    private void MergeWithCube(CubeValue otherCube)
    {
        hasBeenMerged = true;
        otherCube.hasBeenMerged = true;

        int newValue = value * 2;
        SetValue(newValue);

        StartMergeAnimation();

        if (mergeEffectPrefab != null)
        {
            Instantiate(mergeEffectPrefab, transform.position, Quaternion.identity);
        }

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore((value / 2) / 2);

            if (newValue >= 2048)
            {
                gameManager.WinGame();
            }
        }

        DisableCubeComponents(otherCube.gameObject);

        if (ObjectPooler.Instance != null)
        {
            StartCoroutine(ReturnToPoolDelayed(otherCube.gameObject));
        }
        else
        {
            Destroy(otherCube.gameObject);
        }

        Invoke("ResetMergeFlag", 0.1f);
    }

    private System.Collections.IEnumerator ReturnToPoolDelayed(GameObject objectToReturn)
    {
        yield return new WaitForFixedUpdate();

        if (objectToReturn != null && ObjectPooler.Instance != null)
        {
            CubeValue cubeValue = objectToReturn.GetComponent<CubeValue>();
            if (cubeValue != null)
            {
                cubeValue.IsInPool = true;
            }

            objectToReturn.SetActive(false);
            objectToReturn.transform.position = new Vector3(1000, 1000, 1000);
            ObjectPooler.Instance.ReturnToPool("Cube", objectToReturn);

            Debug.Log($"Куб зі значенням {value} повернуто у пул");
        }
    }

    private void DisableCubeComponents(GameObject cube)
    {
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.Sleep();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider col = cube.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    public void StartScaleAnimation()
    {
        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(scaleAnimationCoroutine);
        }

        scaleAnimationCoroutine = StartCoroutine(ScaleAnimation());
    }

    private IEnumerator ScaleAnimation()
    {
        transform.localScale = originalScale * 0.01f;

        float elapsed = 0f;

        while (elapsed < spawnAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / spawnAnimationDuration;

            float scaleMultiplier = spawnScaleCurve != null ?
                spawnScaleCurve.Evaluate(normalizedTime) :
                Mathf.Lerp(0.01f, 1f, normalizedTime);

            transform.localScale = originalScale * scaleMultiplier;

            yield return null;
        }

        transform.localScale = originalScale;
        scaleAnimationCoroutine = null;
    }

    public void StartMergeAnimation()
    {
        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(scaleAnimationCoroutine);
        }

        scaleAnimationCoroutine = StartCoroutine(MergeAnimation());
    }

    private IEnumerator MergeAnimation()
    {
        transform.localScale = originalScale * 0.6f;

        float elapsed = 0f;

        while (elapsed < mergeAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / mergeAnimationDuration;

            float scaleMultiplier = mergeScaleCurve != null ?
                0.6f + 0.4f * mergeScaleCurve.Evaluate(normalizedTime) :
                Mathf.Lerp(0.6f, 1.0f, normalizedTime);

            transform.localScale = originalScale * scaleMultiplier;

            yield return null;
        }

        transform.localScale = originalScale;
        scaleAnimationCoroutine = null;
    }

    public void OnObjectSpawn()
    {
        hasBeenMerged = false;
        IsInPool = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }

        value = 2;
        UpdateVisuals();
        StartScaleAnimation();
    }

    public void ResetMergeState()
    {
        hasBeenMerged = false;
    }
}