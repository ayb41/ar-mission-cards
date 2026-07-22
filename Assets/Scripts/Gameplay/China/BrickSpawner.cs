using System.Collections;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    public GreatWallAudioManager audioManager;
    public GameObject brickPrefab;
    public Transform spawnCenter;
    public Transform bricksParent;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;
    public float spawnHeight = 0.45f;
    public float spawnRangeX = 0.25f;
    public float spawnRangeZ = 0.08f;

    private Coroutine spawnRoutine;

    public void StartSpawning()
    {
        if (spawnRoutine != null) return;

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnBrick();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBrick()
    {
        if (brickPrefab == null)
        {
            Debug.LogWarning("BrickSpawner: Brick Prefab eksik!");
            return;
        }

        if (spawnCenter == null)
        {
            Debug.LogWarning("BrickSpawner: Spawn Center eksik!");
            return;
        }

        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);

        Vector3 spawnPosition =
            spawnCenter.position +
            spawnCenter.right * randomX +
            spawnCenter.forward * randomZ +
            spawnCenter.up * spawnHeight;

        GameObject brick = Instantiate(
            brickPrefab,
            spawnPosition,
            spawnCenter.rotation,
            bricksParent
        );

        if (audioManager != null)
        {
            audioManager.PlayBrickSpawn();
        }

        Rigidbody rb = brick.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}