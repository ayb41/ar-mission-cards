using System;
using System.Collections;
using UnityEngine;

public class SoldierWaveController : MonoBehaviour
{
    public GameObject soldierPrefab;
    public Transform spawnCenter;
    public Transform attackTarget;
    public Transform soldierParent;

    [Header("Wave Settings")]
    public int soldierCount = 4;
    public float spacing = 0.08f;
    public float moveSpeed = 0.22f;
    public float spawnDelay = 0.2f;

    private int reachedSoldiers;
    private bool attackFinished;
    private Action onSoldiersReachedWall;

    public void StartAttack(Action onReachedWall)
    {
        if (soldierPrefab == null)
        {
            Debug.LogError("SoldierWaveController: Soldier Prefab eksik!");
            onReachedWall?.Invoke();
            return;
        }

        if (spawnCenter == null || attackTarget == null)
        {
            Debug.LogError("SoldierWaveController: Spawn Center veya Attack Target eksik!");
            onReachedWall?.Invoke();
            return;
        }

        StopAllCoroutines();

        reachedSoldiers = 0;
        attackFinished = false;
        onSoldiersReachedWall = onReachedWall;

        StartCoroutine(SpawnSoldiersRoutine());
    }

    private IEnumerator SpawnSoldiersRoutine()
    {
        for (int i = 0; i < soldierCount; i++)
        {
            SpawnSoldier(i);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnSoldier(int index)
    {
        float startOffset = -((soldierCount - 1) * spacing) * 0.5f;
        float xOffset = startOffset + index * spacing;

        Vector3 spawnPosition = spawnCenter.position + spawnCenter.right * xOffset;

        GameObject soldier = Instantiate(
            soldierPrefab,
            spawnPosition,
            spawnCenter.rotation,
            soldierParent
        );

        SoldierRunner runner = soldier.GetComponent<SoldierRunner>();

        if (runner == null)
        {
            runner = soldier.AddComponent<SoldierRunner>();
        }

        runner.Init(attackTarget, this, moveSpeed);
    }

    public void RegisterSoldierReachedWall(SoldierRunner soldier)
    {
        if (attackFinished)
        {
            return;
        }

        reachedSoldiers++;

        // İlk asker duvara ulaştığında saldırı etkisi başlasın.
        if (reachedSoldiers >= 1)
        {
            attackFinished = true;
            onSoldiersReachedWall?.Invoke();
        }
    }
    public void ClearSoldiers()
    {
        StopAllCoroutines();

        reachedSoldiers = 0;
        attackFinished = false;
        onSoldiersReachedWall = null;

        if (soldierParent != null)
        {
            for (int i = soldierParent.childCount - 1; i >= 0; i--)
            {
                Destroy(soldierParent.GetChild(i).gameObject);
            }
        }
    }

}