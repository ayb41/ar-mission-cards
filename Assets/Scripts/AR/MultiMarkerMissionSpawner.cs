using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultiMarkerMissionSpawner : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Marker Names")]
    public string romanMarkerName = "roman_soldier_card";
    public string egyptMarkerName = "Anubis_Marker";
    public string greatWallMarkerName = "GreatWallMarker";

    [Header("Mission Prefabs")]
    public GameObject romanMissionPrefab;
    public GameObject egyptMissionPrefab;

    [Header("Roman Scene Objects")]
    public GameObject romanHUDCanvas;
    public GameObject romanMissionControllerObject;

    [Header("Great Wall Scene Object")]
    [Tooltip("ARGameScene_Egypt sahnesindeki inactive GreatWallSystem objesini buraya bağla.")]
    public GameObject greatWallSystem;

    [Tooltip("Opsiyonel. Eğer GreatWallSystem sahnede yoksa prefabını buraya bağlayabilirsin.")]
    public GameObject greatWallSystemPrefab;

    [Header("Stability Settings")]
    public int stableFrameCount = 8;
    public float maxPositionDifference = 0.04f;
    public bool freezeAfterSpawn = true;

    [Header("Game Flow")]
    public bool allowOnlyOneMission = true;

    [Header("Debug")]
    public bool logDetectedMarkers = true;

    private readonly Dictionary<string, GameObject> spawnedMissions = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, Coroutine> pendingSpawns = new Dictionary<string, Coroutine>();

    private bool missionAlreadyStarted = false;

    private void Awake()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = GetComponent<ARTrackedImageManager>();
        }

        if (trackedImageManager == null)
        {
            trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
        }

        ResetSceneUI();
    }

    private void Start()
    {
        ResetSceneUI();
    }

    private void OnEnable()
    {
        ResetSceneUI();

        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
            Debug.Log("MultiMarkerMissionSpawner enabled. Listening for markers.");
        }
        else
        {
            Debug.LogError("ARTrackedImageManager bulunamadı! MultiMarkerMissionSpawner ile aynı objede veya sahnede ARTrackedImageManager olmalı.");
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }
    }

    private void ResetSceneUI()
    {
        SetRomanSceneObjects(false);
        SetGreatWallSceneObjects(false);
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            HandleTrackedImage(trackedImage);
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage == null)
        {
            return;
        }

        string imageName = trackedImage.referenceImage.name;

        if (logDetectedMarkers)
        {
            Debug.Log("Detected marker name: [" + imageName + "] State: " + trackedImage.trackingState);
        }

        if (allowOnlyOneMission && missionAlreadyStarted)
        {
            Debug.Log("Mission already started. Ignoring marker: " + imageName);
            return;
        }

        if (!IsKnownMarker(imageName))
        {
            Debug.LogWarning("Unknown marker detected: [" + imageName + "]");
            return;
        }

        if (spawnedMissions.ContainsKey(imageName))
        {
            UpdateExistingMissionIfNeeded(imageName, trackedImage);
            return;
        }

        if (pendingSpawns.ContainsKey(imageName))
        {
            return;
        }

        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            return;
        }

        Coroutine spawnRoutine = StartCoroutine(SpawnAfterStableTracking(trackedImage, imageName));
        pendingSpawns.Add(imageName, spawnRoutine);
    }

    private bool IsKnownMarker(string imageName)
    {
        return imageName == romanMarkerName ||
               imageName == egyptMarkerName ||
               imageName == greatWallMarkerName;
    }

    private void UpdateExistingMissionIfNeeded(string imageName, ARTrackedImage trackedImage)
    {
        if (freezeAfterSpawn)
        {
            return;
        }

        if (!spawnedMissions.TryGetValue(imageName, out GameObject existingMission))
        {
            return;
        }

        if (existingMission == null)
        {
            spawnedMissions.Remove(imageName);
            return;
        }

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            existingMission.transform.position = Vector3.Lerp(
                existingMission.transform.position,
                trackedImage.transform.position,
                Time.deltaTime * 2f
            );

            existingMission.transform.rotation = Quaternion.Slerp(
                existingMission.transform.rotation,
                trackedImage.transform.rotation,
                Time.deltaTime * 2f
            );
        }
    }

    private IEnumerator SpawnAfterStableTracking(ARTrackedImage trackedImage, string imageName)
    {
        Vector3 startPosition = trackedImage.transform.position;
        Vector3 positionSum = Vector3.zero;
        Quaternion smoothedRotation = trackedImage.transform.rotation;

        int validFrames = 0;
        int safetyCounter = 0;

        while (validFrames < stableFrameCount && safetyCounter < 90)
        {
            safetyCounter++;

            if (trackedImage == null)
            {
                pendingSpawns.Remove(imageName);
                yield break;
            }

            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                Vector3 currentPosition = trackedImage.transform.position;
                float distanceFromStart = Vector3.Distance(startPosition, currentPosition);

                if (distanceFromStart <= maxPositionDifference)
                {
                    positionSum += currentPosition;
                    smoothedRotation = Quaternion.Slerp(
                        smoothedRotation,
                        trackedImage.transform.rotation,
                        0.35f
                    );

                    validFrames++;
                }
                else
                {
                    startPosition = currentPosition;
                    positionSum = Vector3.zero;
                    smoothedRotation = trackedImage.transform.rotation;
                    validFrames = 0;
                }
            }
            else
            {
                validFrames = 0;
                positionSum = Vector3.zero;
                startPosition = trackedImage.transform.position;
            }

            yield return null;
        }

        pendingSpawns.Remove(imageName);

        if (spawnedMissions.ContainsKey(imageName))
        {
            yield break;
        }

        if (validFrames <= 0)
        {
            Debug.LogWarning("Marker stable olmadı, mission başlatılmadı: " + imageName);
            yield break;
        }

        Vector3 stablePosition = positionSum / validFrames;

        ResetSceneUI();

        if (imageName == romanMarkerName)
        {
            StartRomanMission(stablePosition, smoothedRotation);
        }
        else if (imageName == egyptMarkerName)
        {
            StartEgyptMission(stablePosition, smoothedRotation);
        }
        else if (imageName == greatWallMarkerName)
        {
            StartGreatWallMission(stablePosition, smoothedRotation);
        }

        missionAlreadyStarted = true;

        Debug.Log("Stable mission handled for marker: " + imageName);
    }

    private void StartRomanMission(Vector3 position, Quaternion rotation)
    {
        SetRomanSceneObjects(true);
        SetGreatWallSceneObjects(false);

        if (romanMissionPrefab == null)
        {
            Debug.LogError("Roman Mission Prefab boş!");
            return;
        }

        GameObject mission = Instantiate(romanMissionPrefab, position, rotation);
        spawnedMissions.Add(romanMarkerName, mission);

        Debug.Log("Roman mission started.");
    }

    private void StartEgyptMission(Vector3 position, Quaternion rotation)
    {
        SetRomanSceneObjects(false);
        SetGreatWallSceneObjects(false);

        if (egyptMissionPrefab == null)
        {
            Debug.LogError("Egypt Mission Prefab boş!");
            return;
        }

        GameObject mission = Instantiate(egyptMissionPrefab, position, rotation);
        spawnedMissions.Add(egyptMarkerName, mission);

        Debug.Log("Egypt mission started.");
    }

    private void StartGreatWallMission(Vector3 position, Quaternion rotation)
    {
        SetRomanSceneObjects(false);

        GameObject systemToUse = GetOrCreateGreatWallSystem(position, rotation);

        if (systemToUse == null)
        {
            Debug.LogError("GreatWallSystem bulunamadı!");
            return;
        }

        // Marker'ın child'ı olmasın, world space'te bağımsız kalsın.
        systemToUse.transform.SetParent(null, true);

        // Pozisyonu sadece 1 kere marker'a göre ayarla.
        systemToUse.transform.SetPositionAndRotation(position, rotation);

        systemToUse.SetActive(true);

        AutoFixGreatWallReferences(systemToUse);

        GreatWallGameController greatWallController =
            systemToUse.GetComponentInChildren<GreatWallGameController>(true);

        if (greatWallController == null)
        {
            Debug.LogError("GreatWallSystem içinde GreatWallGameController bulunamadı!");
            return;
        }

        greatWallController.StartMission();

        if (!spawnedMissions.ContainsKey(greatWallMarkerName))
        {
            spawnedMissions.Add(greatWallMarkerName, systemToUse);
        }

        // Oyun başladıktan sonra marker tracking platformu etkilemesin.
        if (trackedImageManager != null)
        {
            trackedImageManager.enabled = false;
        }

        Debug.Log("Great Wall mission started and locked in world position.");
    }

    private GameObject GetOrCreateGreatWallSystem(Vector3 position, Quaternion rotation)
    {
        if (greatWallSystem != null)
        {
            if (IsSceneObject(greatWallSystem))
            {
                return greatWallSystem;
            }

            GameObject createdFromAssignedPrefab = Instantiate(greatWallSystem, position, rotation);
            greatWallSystem = createdFromAssignedPrefab;
            return createdFromAssignedPrefab;
        }

        if (greatWallSystemPrefab != null)
        {
            GameObject createdFromPrefab = Instantiate(greatWallSystemPrefab, position, rotation);
            greatWallSystem = createdFromPrefab;
            return createdFromPrefab;
        }

        return null;
    }

    private bool IsSceneObject(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        Scene scene = obj.scene;
        return scene.IsValid() && scene.isLoaded;
    }

    private void AutoFixGreatWallReferences(GameObject systemToUse)
    {
        if (systemToUse == null)
        {
            return;
        }

        GreatWallAudioManager audioManager =
            systemToUse.GetComponentInChildren<GreatWallAudioManager>(true);

        BrickSpawner brickSpawner =
            systemToUse.GetComponentInChildren<BrickSpawner>(true);

        BrickDragManager brickDragManager =
            systemToUse.GetComponentInChildren<BrickDragManager>(true);

        WallEvaluator wallEvaluator =
            systemToUse.GetComponentInChildren<WallEvaluator>(true);

        WallGridManager wallGridManager =
            systemToUse.GetComponentInChildren<WallGridManager>(true);

        SoldierWaveController soldierWaveController =
            systemToUse.GetComponentInChildren<SoldierWaveController>(true);

        GreatWallGameController gameController =
            systemToUse.GetComponentInChildren<GreatWallGameController>(true);

        if (brickDragManager != null)
        {
            if (brickDragManager.arCamera == null)
            {
                brickDragManager.arCamera = Camera.main;
            }

            if (brickDragManager.audioManager == null)
            {
                brickDragManager.audioManager = audioManager;
            }

            if (brickDragManager.wallGridManager == null)
            {
                brickDragManager.wallGridManager = wallGridManager;
            }
        }

        if (brickSpawner != null)
        {
            if (brickSpawner.audioManager == null)
            {
                brickSpawner.audioManager = audioManager;
            }
        }

        if (wallEvaluator != null)
        {
            if (wallEvaluator.wallGridManager == null)
            {
                wallEvaluator.wallGridManager = wallGridManager;
            }
        }

        if (gameController != null)
        {
            if (gameController.brickSpawner == null)
            {
                gameController.brickSpawner = brickSpawner;
            }

            if (gameController.brickDragManager == null)
            {
                gameController.brickDragManager = brickDragManager;
            }

            if (gameController.wallEvaluator == null)
            {
                gameController.wallEvaluator = wallEvaluator;
            }

            if (gameController.soldierWaveController == null)
            {
                gameController.soldierWaveController = soldierWaveController;
            }

            if (gameController.audioManager == null)
            {
                gameController.audioManager = audioManager;
            }
        }
    }

    private void SetRomanSceneObjects(bool active)
    {
        if (romanHUDCanvas != null)
        {
            romanHUDCanvas.SetActive(active);
        }

        if (romanMissionControllerObject != null)
        {
            romanMissionControllerObject.SetActive(active);
        }
    }

    private void SetGreatWallSceneObjects(bool active)
    {
        if (greatWallSystem != null && IsSceneObject(greatWallSystem))
        {
            greatWallSystem.SetActive(active);
        }
    }

    public void ResetMissionStateForTesting()
    {
        missionAlreadyStarted = false;
        spawnedMissions.Clear();
        pendingSpawns.Clear();

        ResetSceneUI();

        Debug.Log("Mission state reset.");
    }
}