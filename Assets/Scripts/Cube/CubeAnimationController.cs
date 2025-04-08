using UnityEngine;
using System.Collections;

/// <summary>
/// Відповідає за анімації кубика
/// </summary>
public class CubeAnimationController : MonoBehaviour
{
    [SerializeField] private float spawnAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve spawnScaleCurve;
    [SerializeField] private float mergeAnimationDuration = 0.2f;
    [SerializeField] private AnimationCurve mergeScaleCurve;

    private Vector3 originalScale;
    private Coroutine scaleAnimationCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void PlaySpawnAnimation()
    {
        StopCurrentAnimation();
        scaleAnimationCoroutine = StartCoroutine(ScaleAnimation());
    }

    public void PlayMergeAnimation()
    {
        StopCurrentAnimation();
        scaleAnimationCoroutine = StartCoroutine(MergeAnimation());
    }

    private void StopCurrentAnimation()
    {
        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(scaleAnimationCoroutine);
            scaleAnimationCoroutine = null;
        }
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
}
