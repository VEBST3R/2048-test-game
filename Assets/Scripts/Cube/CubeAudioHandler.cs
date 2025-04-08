using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CubeAudioHandler : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip cubeToCubeCollisionSound;
    [SerializeField] private AudioClip cubeToWallCollisionSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float cubeCollisionVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float wallCollisionVolume = 0.5f;
    [SerializeField] private float minCollisionVelocity = 1f;

    private AudioSource audioSource;
    private float lastPlayTime = 0f;
    private float cooldownTime = 0.2f;
    private CubeMergeHandler mergeHandler;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.volume = 0.7f;
        }

        mergeHandler = GetComponent<CubeMergeHandler>();
        if (mergeHandler != null)
        {
            mergeHandler.OnCubeMerged += OnCubesMerged;
        }
    }

    private void OnDestroy()
    {
        if (mergeHandler != null)
        {
            mergeHandler.OnCubeMerged -= OnCubesMerged;
        }
    }

    private void OnCubesMerged(GameObject cube1, GameObject cube2)
    {
        PlayCollisionSound(cubeToCubeCollisionSound, cubeCollisionVolume);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastPlayTime < cooldownTime || collision.relativeVelocity.magnitude < minCollisionVelocity)
            return;

        GameObject otherObject = collision.gameObject;

        if (otherObject.CompareTag("Wall"))
        {
            PlayCollisionSound(cubeToWallCollisionSound, wallCollisionVolume);
        }
    }

    private void PlayCollisionSound(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = volume;
        audioSource.PlayOneShot(clip);
        lastPlayTime = Time.time;
    }
}
