using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BrickDragManager : MonoBehaviour
{
    [Header("Game State")]
    public bool canDrag = true;

    [Header("References")]
    public Camera arCamera;
    public Transform buildRoot;
    public WallGridManager wallGridManager;
    public GreatWallAudioManager audioManager;

    [Header("Layers")]
    public LayerMask brickLayer;

    [Header("Drag Area Limits")]
    public float minX = -0.32f;
    public float maxX = 0.32f;
    public float minZ = -0.24f;
    public float maxZ = 0.22f;

    [Header("Free Drag")]
    public float freeDragY = 0.035f;

    [Header("Two Finger Layer Control")]
    public float pixelsPerLayer = 70f;
    public int currentLayer = 0;

    [Header("Optional UI")]
    public TMP_Text layerText;

    private Transform selectedBrick;
    private Rigidbody selectedRigidbody;
    private Vector3 dragOffsetLocal;

    private bool twoFingerActive;
    private float twoFingerStartY;
    private int startLayer;

    private void Start()
    {
        UpdateLayerText();
    }

    private void Update()
    {
        if (!canDrag)
        {
            if (selectedBrick != null)
            {
                ReleaseBrick(false);
            }

            return;
        }

        Vector2 screenPosition;
        bool isPressed = GetPointerPressed(out screenPosition);

        if (isPressed && selectedBrick == null)
        {
            TrySelectBrick(screenPosition);
        }
        else if (isPressed && selectedBrick != null)
        {
            UpdateLayerWithTwoFingers();
            DragBrick(screenPosition);
        }
        else if (!isPressed && selectedBrick != null)
        {
            ReleaseBrick(true);
        }

        UpdateLayerWithKeyboardForTesting();
    }

    private bool GetPointerPressed(out Vector2 screenPosition)
    {
        screenPosition = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            screenPosition = Mouse.current.position.ReadValue();
            return true;
        }

        return false;
    }

    private void TrySelectBrick(Vector2 screenPosition)
    {
        if (arCamera == null || buildRoot == null)
        {
            Debug.LogWarning("BrickDragManager: AR Camera veya Build Root eksik!");
            return;
        }

        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, brickLayer))
        {
            if (hit.rigidbody != null)
            {
                selectedRigidbody = hit.rigidbody;
                selectedBrick = selectedRigidbody.transform;
            }
            else
            {
                selectedBrick = hit.transform;
                selectedRigidbody = selectedBrick.GetComponent<Rigidbody>();
            }

            if (selectedBrick == null)
            {
                return;
            }

            if (wallGridManager != null)
            {
                int oldRow = wallGridManager.GetBrickRow(selectedBrick);
                currentLayer = oldRow >= 0 ? oldRow : 0;

                wallGridManager.ReleaseBrickSlot(selectedBrick);
            }
            else
            {
                currentLayer = 0;
            }

            Vector3 selectedLocalPosition = buildRoot.InverseTransformPoint(selectedBrick.position);
            Vector3 hitLocalPosition = buildRoot.InverseTransformPoint(hit.point);

            dragOffsetLocal = selectedLocalPosition - hitLocalPosition;
            dragOffsetLocal.y = 0f;

            if (selectedRigidbody != null)
            {
                selectedRigidbody.constraints = RigidbodyConstraints.None;
                selectedRigidbody.isKinematic = true;

#if UNITY_6000_0_OR_NEWER
                selectedRigidbody.linearVelocity = Vector3.zero;
#else
                selectedRigidbody.velocity = Vector3.zero;
#endif

                selectedRigidbody.angularVelocity = Vector3.zero;
            }

            twoFingerActive = false;
            UpdateLayerText();

            if (audioManager != null)
            {
                audioManager.PlayBrickPick();
            }
        }
    }

    private void DragBrick(Vector2 screenPosition)
    {
        if (selectedBrick == null || arCamera == null || buildRoot == null)
        {
            return;
        }

        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        Plane dragPlane = new Plane(buildRoot.up, buildRoot.position);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            Vector3 localPoint = buildRoot.InverseTransformPoint(worldPoint);

            localPoint += dragOffsetLocal;

            localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
            localPoint.z = Mathf.Clamp(localPoint.z, minZ, maxZ);

            bool isInWallArea = wallGridManager != null && wallGridManager.IsInWallArea(localPoint);

            if (isInWallArea)
            {
                localPoint.y = wallGridManager.GetSlotLocalY(currentLayer);
            }
            else
            {
                localPoint.y = freeDragY;
            }

            selectedBrick.position = buildRoot.TransformPoint(localPoint);
            selectedBrick.rotation = buildRoot.rotation;
        }
    }

    private void ReleaseBrick(bool playSound)
    {
        bool hadSelectedBrick = selectedBrick != null;
        bool placedInWall = false;

        if (selectedBrick != null && wallGridManager != null)
        {
            Vector3 localPoint = buildRoot.InverseTransformPoint(selectedBrick.position);

            if (wallGridManager.IsInWallArea(localPoint))
            {
                if (wallGridManager.TryPlaceBrick(selectedBrick, localPoint, currentLayer, out Vector3 slotWorldPosition))
                {
                    selectedBrick.position = slotWorldPosition;
                    selectedBrick.rotation = buildRoot.rotation;
                    placedInWall = true;
                }
            }
        }

        if (selectedRigidbody != null)
        {
            if (placedInWall)
            {
                selectedRigidbody.isKinematic = true;
                selectedRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                selectedRigidbody.constraints = RigidbodyConstraints.None;
                selectedRigidbody.isKinematic = false;
            }
        }

        if (playSound && hadSelectedBrick && audioManager != null)
        {
            audioManager.PlayBrickPlace();
        }

        selectedBrick = null;
        selectedRigidbody = null;
        dragOffsetLocal = Vector3.zero;
        twoFingerActive = false;
    }

    private void UpdateLayerWithTwoFingers()
    {
        if (wallGridManager == null)
        {
            return;
        }

        if (TryGetTwoFingerAverageY(out float averageY))
        {
            if (!twoFingerActive)
            {
                twoFingerActive = true;
                twoFingerStartY = averageY;
                startLayer = currentLayer;
            }

            float deltaY = averageY - twoFingerStartY;
            int layerChange = Mathf.RoundToInt(deltaY / pixelsPerLayer);

            currentLayer = Mathf.Clamp(startLayer + layerChange, 0, wallGridManager.rows - 1);
            UpdateLayerText();
        }
        else
        {
            twoFingerActive = false;
        }
    }

    private bool TryGetTwoFingerAverageY(out float averageY)
    {
        averageY = 0f;

        if (Touchscreen.current == null)
        {
            return false;
        }

        if (Touchscreen.current.touches.Count < 2)
        {
            return false;
        }

        bool firstPressed = Touchscreen.current.touches[0].press.isPressed;
        bool secondPressed = Touchscreen.current.touches[1].press.isPressed;

        if (!firstPressed || !secondPressed)
        {
            return false;
        }

        float y1 = Touchscreen.current.touches[0].position.ReadValue().y;
        float y2 = Touchscreen.current.touches[1].position.ReadValue().y;

        averageY = (y1 + y2) * 0.5f;
        return true;
    }

    private void UpdateLayerWithKeyboardForTesting()
    {
        if (selectedBrick == null || wallGridManager == null || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentLayer = Mathf.Clamp(currentLayer + 1, 0, wallGridManager.rows - 1);
            UpdateLayerText();
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            currentLayer = Mathf.Clamp(currentLayer - 1, 0, wallGridManager.rows - 1);
            UpdateLayerText();
        }
    }

    private void UpdateLayerText()
    {
        if (layerText != null && wallGridManager != null)
        {
            layerText.text = "Kat: " + (currentLayer + 1) + " / " + wallGridManager.rows;
        }
    }
}