using UnityEngine;
using TMPro;

[RequireComponent(typeof(CubeValue))]
public class CubeVisualController : MonoBehaviour
{
    [SerializeField] private TextMeshPro[] valueTexts;
    [SerializeField] private MeshRenderer meshRenderer;

    private CubeValue cubeValue;

    private void Awake()
    {
        cubeValue = GetComponent<CubeValue>();
        InitializeComponents();
    }

    private void OnEnable()
    {
        if (cubeValue != null)
        {
            cubeValue.OnValueChanged += UpdateVisual;
        }
    }

    private void OnDisable()
    {
        if (cubeValue != null)
        {
            cubeValue.OnValueChanged -= UpdateVisual;
        }
    }

    private void Start()
    {
        UpdateVisual(cubeValue.GetValue());
    }

    private void InitializeComponents()
    {
        if (valueTexts == null || valueTexts.Length == 0)
        {
            valueTexts = GetComponentsInChildren<TextMeshPro>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }

    public void UpdateVisual(int value)
    {
        UpdateTexts(value);
        UpdateColor(value);
    }

    private void UpdateTexts(int value)
    {
        foreach (TextMeshPro text in valueTexts)
        {
            if (text != null)
            {
                text.text = value.ToString();
            }
        }
    }

    private void UpdateColor(int value)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = GetColorForValue(value);
        }
    }

    private Color GetColorForValue(int value)
    {
        switch (value)
        {
            case 2: return new Color(0.93f, 0.89f, 0.85f);
            case 4: return new Color(0.93f, 0.87f, 0.78f);
            case 8: return new Color(0.95f, 0.69f, 0.47f);
            case 16: return new Color(0.96f, 0.58f, 0.39f);
            case 32: return new Color(0.96f, 0.49f, 0.37f);
            case 64: return new Color(0.96f, 0.35f, 0.23f);
            case 128: return new Color(0.93f, 0.81f, 0.45f);
            case 256: return new Color(0.93f, 0.80f, 0.38f);
            case 512: return new Color(0.93f, 0.78f, 0.31f);
            case 1024: return new Color(0.93f, 0.77f, 0.25f);
            case 2048: return new Color(0.93f, 0.76f, 0.18f);
            case 4096: return new Color(0.64f, 0.83f, 0.45f);
            case 8192: return new Color(0.40f, 0.72f, 0.40f);
            default: return new Color(0.61f, 0.35f, 0.71f);
        }
    }
}