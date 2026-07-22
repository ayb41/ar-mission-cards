using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Default State")]
    public string defaultStateName = "Idle";

    [Header("Parameter Names")]
    public string moveSpeedParameter = "MoveSpeed";
    public string attackTrigger = "Attack";
    public string defendTrigger = "Defend";
    public string dieTrigger = "Die";
    public string victoryTrigger = "Victory";
    public string spawnTrigger = "Spawn";

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        ResetAllTriggers();
        SetMoveSpeed(0f);

        if (animator != null && !string.IsNullOrEmpty(defaultStateName))
        {
            animator.Play(defaultStateName, 0, 0f);
        }
    }

    public void SetMoveSpeed(float speed)
    {
        if (animator == null)
            return;

        if (!HasParameter(moveSpeedParameter, AnimatorControllerParameterType.Float))
            return;

        animator.SetFloat(moveSpeedParameter, speed);
    }

    public void PlayAttack()
    {
        SetTriggerIfExists(attackTrigger);
    }

    public void PlayDefend()
    {
        SetTriggerIfExists(defendTrigger);
    }

    public void PlayDeath()
    {
        SetTriggerIfExists(dieTrigger);
    }

    public void PlayVictory()
    {
        SetTriggerIfExists(victoryTrigger);
    }

    public void PlaySpawn()
    {
        SetTriggerIfExists(spawnTrigger);
    }

    private void SetTriggerIfExists(string triggerName)
    {
        if (animator == null)
            return;

        if (!HasParameter(triggerName, AnimatorControllerParameterType.Trigger))
            return;

        animator.ResetTrigger(triggerName);
        animator.SetTrigger(triggerName);
    }

    private void ResetAllTriggers()
    {
        if (animator == null)
            return;

        ResetTriggerIfExists(attackTrigger);
        ResetTriggerIfExists(defendTrigger);
        ResetTriggerIfExists(dieTrigger);
        ResetTriggerIfExists(victoryTrigger);
        ResetTriggerIfExists(spawnTrigger);
    }

    private void ResetTriggerIfExists(string triggerName)
    {
        if (animator == null)
            return;

        if (!HasParameter(triggerName, AnimatorControllerParameterType.Trigger))
            return;

        animator.ResetTrigger(triggerName);
    }

    private bool HasParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }
}