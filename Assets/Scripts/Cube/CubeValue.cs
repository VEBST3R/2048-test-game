using UnityEngine;
using System;

/// <summary>
/// Відповідає тільки за зберігання значення кубика
/// </summary>
public class CubeValue : MonoBehaviour
{
    public event Action<int> OnValueChanged;

    [SerializeField] private int value = 2;

    public void SetValue(int newValue)
    {
        if (value != newValue)
        {
            int oldValue = value;
            value = newValue;
            OnValueChanged?.Invoke(value);
        }
    }

    public int GetValue()
    {
        return value;
    }

    public void ResetValue()
    {
        SetValue(2);
    }
}