using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GreatWallGameController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text instructionText;
    public TMP_Text countdownText;
    public TMP_Text timerText;
    public TMP_Text resultText;
    public GameObject resultPanel;

    [Header("Systems")]
    public BrickSpawner brickSpawner;
    public BrickDragManager brickDragManager;
    public WallEvaluator wallEvaluator;
    public SoldierWaveController soldierWaveController;
    public GreatWallAudioManager audioManager;
    public WallGridManager wallGridManager;

    [Header("Parents")]
    public Transform bricksParent;

    [Header("Game Settings")]
    public float buildTime = 60f;
    public bool autoStartForTesting = false;

    [Header("Collapse Settings")]
    public float collapseForce = 0.25f;
    public float upwardForce = 0.08f;

    private bool gameStarted;
    private bool wallIsCompleteAfterBuild;
    private Coroutine gameRoutine;

    private void Start()
    {
        PrepareInitialUI();

        if (brickDragManager != null)
        {
            brickDragManager.canDrag = false;
        }

        if (autoStartForTesting)
        {
            StartMission();
        }
    }

    private void PrepareInitialUI()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        if (timerText != null)
        {
            timerText.text = "";
        }
    }

    public void StartMission()
    {
        if (gameStarted)
        {
            return;
        }

        gameStarted = true;
        gameRoutine = StartCoroutine(GameRoutine());
    }

    private IEnumerator GameRoutine()
    {
        if (instructionText != null)
        {
            instructionText.text = "Baue die Chinesische Mauer in 60 Sekunden!";
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "";
        }

        if (brickDragManager != null)
        {
            brickDragManager.canDrag = false;
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }

            if (audioManager != null)
            {
                audioManager.PlayCountdownTick();
            }

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.text = "Los!";
        }

        if (audioManager != null)
        {
            audioManager.PlayStart();
        }

        yield return new WaitForSeconds(0.7f);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        if (brickDragManager != null)
        {
            brickDragManager.canDrag = true;
        }

        if (brickSpawner != null)
        {
            brickSpawner.StartSpawning();
        }

        float remainingTime = buildTime;

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = "Zeit: " + Mathf.CeilToInt(remainingTime);
            }

            yield return null;
        }

        EndBuildPhase();
    }

    private void EndBuildPhase()
    {
        if (brickSpawner != null)
        {
            brickSpawner.StopSpawning();
        }

        if (brickDragManager != null)
        {
            brickDragManager.canDrag = false;
        }

        if (timerText != null)
        {
            timerText.text = "Zeit: 0";
        }

        if (audioManager != null)
        {
            audioManager.PlayTimeUp();
        }

        if (instructionText != null)
        {
            instructionText.text = "Die Soldaten greifen an!";
        }

        if (audioManager != null)
        {
            audioManager.PlayWarHorn();
        }

        wallIsCompleteAfterBuild = false;

        if (wallEvaluator != null)
        {
            wallIsCompleteAfterBuild = wallEvaluator.EvaluateWall();
        }
        else
        {
            Debug.LogWarning("GreatWallGameController: WallEvaluator eksik!");
        }

        StartSoldierAttackPhase();
    }

    private void StartSoldierAttackPhase()
    {
        if (audioManager != null)
        {
            audioManager.StartSoldierRunLoop();
        }

        if (soldierWaveController == null)
        {
            Debug.LogWarning("GreatWallGameController: SoldierWaveController eksik! Direkt sonuç veriliyor.");
            ResolveAfterSoldierImpact();
            return;
        }

        soldierWaveController.StartAttack(ResolveAfterSoldierImpact);
    }

    private void ResolveAfterSoldierImpact()
    {
        if (wallIsCompleteAfterBuild)
        {
            WinGame();
        }
        else
        {
            CollapseWall();
            LoseGame();
        }
    }

    private void WinGame()
    {
        if (instructionText != null)
        {
            instructionText.text = "";
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultText != null)
        {
            resultText.text = "Gewonnen!\nDeine Mauer hält stand!";
        }

        if (audioManager != null)
        {
            audioManager.PlayWin();
        }

        Debug.Log("Great Wall Game: WIN");
    }

    private void LoseGame()
    {
        if (instructionText != null)
        {
            instructionText.text = "";
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultText != null)
        {
            resultText.text = "Verloren!\nDie Mauer ist eingestürzt!";
        }

        if (audioManager != null)
        {
            audioManager.PlayLose();
        }

        Debug.Log("Great Wall Game: LOSE");
    }

    private void CollapseWall()
    {
        if (audioManager != null)
        {
            audioManager.PlayWallCollapse();
        }

        GameObject[] bricks = GameObject.FindGameObjectsWithTag("GreatWallBrick");

        foreach (GameObject brick in bricks)
        {
            Rigidbody rb = brick.GetComponent<Rigidbody>();

            if (rb == null)
            {
                continue;
            }

            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity = true;

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif

            rb.angularVelocity = Vector3.zero;

            Vector3 forceDirection = new Vector3(
                Random.Range(-0.25f, 0.25f),
                upwardForce,
                -1f
            ).normalized;

            rb.AddForce(forceDirection * collapseForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 0.05f, ForceMode.Impulse);
        }

        Debug.Log("Wall collapsed.");
    }

    public void PlayAgain()
    {
        StopCurrentGame();
        ClearGreatWallScene();
        PrepareInitialUI();

        gameStarted = false;
        wallIsCompleteAfterBuild = false;

        StartMission();
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }

    private void StopCurrentGame()
    {
        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        if (brickSpawner != null)
        {
            brickSpawner.StopSpawning();
        }

        if (brickDragManager != null)
        {
            brickDragManager.canDrag = false;
        }

        if (audioManager != null)
        {
            audioManager.StopSoldierRunLoop();
        }

        if (soldierWaveController != null)
        {
            soldierWaveController.ClearSoldiers();
        }
    }

    private void ClearGreatWallScene()
    {
        if (wallGridManager != null)
        {
            wallGridManager.ClearGrid();
        }

        ClearBricks();
        ClearLooseSoldiers();
    }

    private void ClearBricks()
    {
        if (bricksParent != null)
        {
            for (int i = bricksParent.childCount - 1; i >= 0; i--)
            {
                Destroy(bricksParent.GetChild(i).gameObject);
            }

            return;
        }

        GameObject[] bricks = GameObject.FindGameObjectsWithTag("GreatWallBrick");

        foreach (GameObject brick in bricks)
        {
            Destroy(brick);
        }
    }

    private void ClearLooseSoldiers()
    {
        SoldierRunner[] soldiers = FindObjectsByType<SoldierRunner>(FindObjectsSortMode.None);

        foreach (SoldierRunner soldier in soldiers)
        {
            Destroy(soldier.gameObject);
        }
    }
}