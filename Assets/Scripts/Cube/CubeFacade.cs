using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CubeValue))]
[RequireComponent(typeof(CubeVisualController))]
[RequireComponent(typeof(CubeMergeHandler))]
[RequireComponent(typeof(CubeAnimationController))]
public class CubeFacade : MonoBehaviour, IPoolable
{
    public event Action<int> OnValueChanged;
    public event Action<CubeFacade, CubeFacade> OnCubeMerged;

    private CubeValue valueComponent;
    private CubeVisualController visualController;
    private CubeMergeHandler mergeHandler;
    private CubeAnimationController animationController;

    [HideInInspector] public bool IsInPool = false;

#if UNITY_EDITOR
    private void Reset()
    {
        EnsureRequiredComponents();
    }

    private void OnValidate()
    {
        EnsureRequiredComponents();
    }

    private void EnsureRequiredComponents()
    {
        valueComponent = GetComponent<CubeValue>() ?? gameObject.AddComponent<CubeValue>();
        visualController = GetComponent<CubeVisualController>() ?? gameObject.AddComponent<CubeVisualController>();
        mergeHandler = GetComponent<CubeMergeHandler>() ?? gameObject.AddComponent<CubeMergeHandler>();
        animationController = GetComponent<CubeAnimationController>() ?? gameObject.AddComponent<CubeAnimationController>();

        Debug.Log("Required components for CubeFacade have been added/verified");
    }
#endif

    private void Awake()
    {
        valueComponent = GetComponent<CubeValue>();
        visualController = GetComponent<CubeVisualController>();
        mergeHandler = GetComponent<CubeMergeHandler>();
        animationController = GetComponent<CubeAnimationController>();

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
        if (animationController != null && !IsInPool)
        {
            animationController.PlaySpawnAnimation();
        }
    }

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

    public void OnObjectSpawn()
    {
        IsInPool = false;

        if (valueComponent != null)
        {
            valueComponent.ResetValue();
        }

        if (mergeHandler != null)
        {
            mergeHandler.ResetMergeState();
            mergeHandler.EnableComponents();
        }

        if (animationController != null)
        {
            animationController.PlaySpawnAnimation();
        }
    }
}
