using System.Collections;
using UnityEngine;

public class AnubisReactionController : MonoBehaviour
{
    [Header("Target")]
    public Transform anubisModel;

    [Header("Correct Animation")]
    public float jumpHeight = 0.025f;
    public float jumpDuration = 0.35f;

    [Header("Wrong Animation")]
    public float shakeAmount = 0.015f;
    public float shakeDuration = 0.35f;

    [Header("Win Animation")]
    public float winJumpHeight = 0.04f;
    public float winDuration = 0.7f;

    [Header("Lose Animation")]
    public float loseScaleAmount = 0.85f;
    public float loseDuration = 0.4f;

    private Vector3 startLocalPosition;
    private Vector3 startLocalScale;
    private Quaternion startLocalRotation;

    private Coroutine currentAnimation;

    private void Awake()
    {
        if (anubisModel == null)
        {
            anubisModel = transform;
        }

        startLocalPosition = anubisModel.localPosition;
        startLocalScale = anubisModel.localScale;
        startLocalRotation = anubisModel.localRotation;
    }

    public void PlayCorrectReaction()
    {
        PlayAnimation(CorrectReactionRoutine());
    }

    public void PlayWrongReaction()
    {
        PlayAnimation(WrongReactionRoutine());
    }

    public void PlayWinReaction()
    {
        PlayAnimation(WinReactionRoutine());
    }

    public void PlayLoseReaction()
    {
        PlayAnimation(LoseReactionRoutine());
    }

    public void ResetAnubis()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        anubisModel.localPosition = startLocalPosition;
        anubisModel.localScale = startLocalScale;
        anubisModel.localRotation = startLocalRotation;
    }

    private void PlayAnimation(IEnumerator routine)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        ResetAnubis();
        currentAnimation = StartCoroutine(routine);
    }

    private IEnumerator CorrectReactionRoutine()
    {
        float timer = 0f;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;

            float t = timer / jumpDuration;
            float jump = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            anubisModel.localPosition = startLocalPosition + new Vector3(0f, jump, 0f);

            yield return null;
        }

        ResetAnubis();
    }

    private IEnumerator WrongReactionRoutine()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            float shake = Mathf.Sin(timer * 50f) * shakeAmount;

            anubisModel.localPosition = startLocalPosition + new Vector3(shake, 0f, 0f);

            yield return null;
        }

        ResetAnubis();
    }

    private IEnumerator WinReactionRoutine()
    {
        float timer = 0f;

        while (timer < winDuration)
        {
            timer += Time.deltaTime;

            float t = timer / winDuration;
            float jump = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * winJumpHeight;
            float rotation = Mathf.Sin(t * Mathf.PI * 4f) * 10f;

            anubisModel.localPosition = startLocalPosition + new Vector3(0f, jump, 0f);
            anubisModel.localRotation = startLocalRotation * Quaternion.Euler(0f, rotation, 0f);

            yield return null;
        }

        ResetAnubis();
    }

    private IEnumerator LoseReactionRoutine()
    {
        float timer = 0f;

        Vector3 targetScale = startLocalScale * loseScaleAmount;

        while (timer < loseDuration)
        {
            timer += Time.deltaTime;

            float t = timer / loseDuration;

            anubisModel.localScale = Vector3.Lerp(startLocalScale, targetScale, t);
            anubisModel.localRotation = startLocalRotation * Quaternion.Euler(10f * t, 0f, 0f);

            yield return null;
        }
    }
}