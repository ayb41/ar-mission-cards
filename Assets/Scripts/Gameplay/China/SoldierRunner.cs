using UnityEngine;

public class SoldierRunner : MonoBehaviour
{
    private Transform target;
    private SoldierWaveController waveController;
    private float moveSpeed;
    private bool hasReached;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void Init(Transform targetPoint, SoldierWaveController controller, float speed)
    {
        target = targetPoint;
        waveController = controller;
        moveSpeed = speed;
        hasReached = false;

        if (animator != null)
        {
            animator.SetBool("IsRunning", true);
        }
    }

    private void Update()
    {
        if (target == null || hasReached)
        {
            return;
        }

        Vector3 targetPosition = target.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;

        if (direction.magnitude <= 0.025f)
        {
            hasReached = true;

            if (animator != null)
            {
                animator.SetBool("IsRunning", false);
                animator.SetTrigger("Attack");
            }

            if (waveController != null)
            {
                waveController.RegisterSoldierReachedWall(this);
            }

            return;
        }

        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        }
    }
}