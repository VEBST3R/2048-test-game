using UnityEngine;

[CreateAssetMenu(fileName = "CubeMovementSettings", menuName = "2048/Cube Movement Settings")]
public class CubeMovementSettings : ScriptableObject
{
    [Header("Movement Settings")]
    [Tooltip("Швидкість горизонтального руху куба")]
    [SerializeField] private float horizontalMoveSpeed = 10f;

    [Tooltip("Сила, з якою куб штовхається вперед")]
    [SerializeField] private float forwardForce = 10f;

    [Tooltip("Діапазон руху куба по осі X")]
    [SerializeField] private float movementRange = 2.5f;

    [Header("Spawn Settings")]
    [Tooltip("Затримка перед створенням нового куба після запуску попереднього")]
    [SerializeField] private float spawnDelay = 1.5f;

    [Tooltip("Ймовірність створення куба зі значенням 2 (від 0 до 1)")]
    [Range(0f, 1f)]
    [SerializeField] private float probability2 = 0.75f;

    // Getter властивості
    public float HorizontalMoveSpeed => horizontalMoveSpeed;
    public float ForwardForce => forwardForce;
    public float MovementRange => movementRange;
    public float SpawnDelay => spawnDelay;
    public float Probability2 => probability2;
}