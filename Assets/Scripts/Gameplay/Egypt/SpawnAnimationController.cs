using System.Collections;
using UnityEngine;

public class SpawnAnimationController : MonoBehaviour
{
    [Header("Target")]
    public Transform targetRoot;

    [Header("Spawn Settings")]
    public float startScaleMultiplier = 0.05f;
    public float animationDuration = 0.6f;
    public float overshootMultiplier = 1.12f;
    public bool playOnStart = true;

    private Vector3 originalScale;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        if (targetRoot == null)
        {
            targetRoot = transform;
        }

        originalScale = targetRoot.localScale;
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlaySpawnAnimation();
        }
    }

    public void PlaySpawnAnimation()
    {
        if (targetRoot == null)
        {
            return;
        }

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        Vector3 startScale = originalScale * startScaleMultiplier;
        Vector3 overshootScale = originalScale * overshootMultiplier;

        targetRoot.localScale = startScale;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / animationDuration);

            // Smooth büyüme efekti
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (t < 0.75f)
            {
                float firstPhaseT = smoothT / 0.75f;
                targetRoot.localScale = Vector3.Lerp(startScale, overshootScale, firstPhaseT);
            }
            else
            {
                float secondPhaseT = (smoothT - 0.75f) / 0.25f;
                targetRoot.localScale = Vector3.Lerp(overshootScale, originalScale, secondPhaseT);
            }

            yield return null;
        }

        targetRoot.localScale = originalScale;
        spawnCoroutine = null;
    }

    public void ResetToOriginalScale()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (targetRoot != null)
        {
            targetRoot.localScale = originalScale;
        }
    }
}