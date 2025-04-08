using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Головний клас-фасад для кубика, що координує всі інші компоненти
/// </summary>
[RequireComponent(typeof(CubeValue))]
[RequireComponent(typeof(CubeVisualController))]
[RequireComponent(typeof(CubeMergeHandler))]
[RequireComponent(typeof(CubeAnimationController))]
public class CubeFacade : MonoBehaviour, IPoolable
{
    public event Action<int> OnValueChanged;
    public event Action<CubeFacade, CubeFacade> OnCubeMerged;

    // Залежні компоненти
    private CubeValue valueComponent;
    private CubeVisualController visualController;
    private CubeMergeHandler mergeHandler;
    private CubeAnimationController animationController;

    [HideInInspector] public bool IsInPool = false;

#if UNITY_EDITOR
    // Викликається при додаванні компонента в редакторі
    private void Reset()
    {
        // Додаємо залежні компоненти, якщо вони відсутні
        EnsureRequiredComponents();
    }

    // Викликається при зміні полів компонента в інспекторі
    private void OnValidate()
    {
        // Перевіряємо наявність залежних компонентів
        EnsureRequiredComponents();
    }

    // Метод для забезпечення наявності всіх залежних компонентів
    private void EnsureRequiredComponents()
    {
        // Unity автоматично додасть ці компоненти через RequireComponent,
        // але ми можемо зберегти посилання на них
        valueComponent = GetComponent<CubeValue>() ?? gameObject.AddComponent<CubeValue>();
        visualController = GetComponent<CubeVisualController>() ?? gameObject.AddComponent<CubeVisualController>();
        mergeHandler = GetComponent<CubeMergeHandler>() ?? gameObject.AddComponent<CubeMergeHandler>();
        animationController = GetComponent<CubeAnimationController>() ?? gameObject.AddComponent<CubeAnimationController>();

        // Додаткова логіка налаштування компонентів, якщо потрібно
        Debug.Log("Required components for CubeFacade have been added/verified");
    }
#endif

    private void Awake()
    {
        // Отримуємо компоненти
        valueComponent = GetComponent<CubeValue>();
        visualController = GetComponent<CubeVisualController>();
        mergeHandler = GetComponent<CubeMergeHandler>();
        animationController = GetComponent<CubeAnimationController>();

        // Підписуємося на події
        SetupEventListeners();
    }

    private void SetupEventListeners()
    {
        if (valueComponent != null)
        {
            valueComponent.OnValueChanged += (value) => OnValueChanged?.Invoke(value);
        }

        if (mergeHandler != null)
        {
            mergeHandler.OnCubeMerged += (thisCube, otherCube) =>
            {
                CubeFacade otherFacade = otherCube.GetComponent<CubeFacade>();
                if (otherFacade != null)
                {
                    OnCubeMerged?.Invoke(this, otherFacade);
                }
            };
        }
    }

    private void Start()
    {
        // Запускаємо анімацію на старті
        if (animationController != null && !IsInPool)
        {
            animationController.PlaySpawnAnimation();
        }
    }

    // Публічні методи для взаємодії

    public int GetValue()
    {
        return valueComponent != null ? valueComponent.GetValue() : 0;
    }

    public void SetValue(int value)
    {
        if (valueComponent != null)
        {
            valueComponent.SetValue(value);
        }
    }

    public void SetInPool(bool inPool)
    {
        IsInPool = inPool;
    }

    public CubeMergeHandler GetMergeHandler()
    {
        return mergeHandler;
    }

    public CubeAnimationController GetAnimationController()
    {
        return animationController;
    }

    // IPoolable implementation
    public void OnObjectSpawn()
    {
        IsInPool = false;

        // Скидаємо компоненти
        if (valueComponent != null)
        {
            valueComponent.ResetValue();
        }

        if (mergeHandler != null)
        {
            mergeHandler.ResetMergeState();
            mergeHandler.EnableComponents();
        }

        // Запускаємо анімацію появи
        if (animationController != null)
        {
            animationController.PlaySpawnAnimation();
        }
    }
}
