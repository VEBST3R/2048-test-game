using UnityEngine;

public class MergeEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private ParticleSystem mergeParticles;
    [SerializeField] private AudioClip mergeSound;
    [SerializeField] private float volume = 0.7f;

    private float timer = 0f;
    private Vector3 startScale;

    private void Start()
    {
        // Зберігаємо початковий розмір
        startScale = transform.localScale;

        // Запускаємо частинки, якщо вони є
        if (mergeParticles != null)
        {
            mergeParticles.Play();
        }

        // Відтворюємо звук, якщо він є
        if (mergeSound != null)
        {
            AudioSource.PlayClipAtPoint(mergeSound, transform.position, volume);
        }
    }

    private void Update()
    {
        // Збільшуємо таймер
        timer += Time.deltaTime;

        // Анімуємо розмір ефекту
        if (scaleCurve != null)
        {
            float t = timer / duration;
            float scaleMultiplier = scaleCurve.Evaluate(t);
            transform.localScale = startScale * scaleMultiplier;
        }

        // Знищуємо об'єкт після закінчення тривалості
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}