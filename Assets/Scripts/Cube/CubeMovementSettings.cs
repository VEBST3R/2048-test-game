using UnityEngine;

[CreateAssetMenu(fileName = "CubeMovementSettings", menuName = "2048/Cube Movement Settings")]
public class CubeMovementSettings : ScriptableObject
{
    [Header("Movement Settings")]
    [SerializeField] private float horizontalMoveSpeed = 10f;
    [SerializeField] private float forwardForce = 10f;
    [SerializeField] private float movementRange = 2.5f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDelay = 1.5f;
    [Range(0f, 1f)]
    [SerializeField] private float probability2 = 0.75f;

    public float HorizontalMoveSpeed => horizontalMoveSpeed;
    public float ForwardForce => forwardForce;
    public float MovementRange => movementRange;
    public float SpawnDelay => spawnDelay;
    public float Probability2 => probability2;
}