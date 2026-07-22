using UnityEngine;

public class PyramidDoorController : MonoBehaviour
{
    [Header("Door Objects")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Open Settings")]
    public float openDistance = 0.06f;
    public float openSpeed = 3f;

    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;

    private float targetProgress = 0f;

    private void Start()
    {
        leftClosedPosition = leftDoor.localPosition;
        rightClosedPosition = rightDoor.localPosition;

        leftOpenPosition = leftClosedPosition + new Vector3(-openDistance, 0f, 0f);
        rightOpenPosition = rightClosedPosition + new Vector3(openDistance, 0f, 0f);
    }

    private void Update()
    {
        leftDoor.localPosition = Vector3.Lerp(
            leftDoor.localPosition,
            Vector3.Lerp(leftClosedPosition, leftOpenPosition, targetProgress),
            Time.deltaTime * openSpeed
        );

        rightDoor.localPosition = Vector3.Lerp(
            rightDoor.localPosition,
            Vector3.Lerp(rightClosedPosition, rightOpenPosition, targetProgress),
            Time.deltaTime * openSpeed
        );
    }

    public void SetDoorProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }

    public void OpenCompletely()
    {
        targetProgress = 1f;
    }

    public void CloseDoor()
    {
        targetProgress = 0f;
    }
}