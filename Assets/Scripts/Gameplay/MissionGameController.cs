using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionGameController : MonoBehaviour
{
    public static MissionGameController Instance { get; private set; }

    [Header("UI Texts")]
    public TMP_Text missionText;
    public TMP_Text playerHPText;
    public TMP_Text enemyHPText;
    public TMP_Text scoreText;
    public TMP_Text countdownText;

    [Header("HP Bars")]
    public Image playerHPFill;
    public Image enemyHPFill;

    [Header("Countdown Animation")]
    public float countdownAnimationDuration = 0.75f;
    public float countdownStartScale = 0.5f;
    public float countdownPeakScale = 1.35f;
    public float countdownEndScale = 1.0f;

    [Header("Joystick")]
    public VirtualJoystick joystick;

    [Header("Game Values")]
    public int playerHP = 100;
    public int enemyHP = 100;
    public int score = 0;

    [Header("Combat Range")]
    public float attackRange = 0.07f;
    public float specialRange = 0.09f;

    [Header("Player Random Damage")]
    public int attackMinDamage = 10;
    public int attackMaxDamage = 25;
    public int specialMinDamage = 25;
    public int specialMaxDamage = 45;

    [Header("Enemy Random Damage")]
    public int enemyMinDamage = 8;
    public int enemyMaxDamage = 18;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip specialSound;
    public AudioClip hitSound;
    public AudioClip blockSound;
    public AudioClip winSound;
    public AudioClip gameOverSound;

    private bool defending = false;
    private bool missionFinished = false;
    private bool cardScanned = false;
    private bool combatStarted = false;

    private int maxPlayerHP;
    private int maxEnemyHP;

    private RomanMissionController activeMission;
    private Coroutine countdownAnimationCoroutine;

    public bool IsMissionFinished => missionFinished;
    public bool IsCardScanned => cardScanned;
    public bool IsCombatStarted => combatStarted;

    private void Awake()
    {
        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        maxPlayerHP = playerHP;
        maxEnemyHP = enemyHP;

        HideCountdown();
        UpdateUI();

        if (missionText != null)
        {
            missionText.text = "Mission: Scanne zuerst die AR-Karte!";
        }
    }

    public void RegisterMission(RomanMissionController mission)
    {
        activeMission = mission;

        cardScanned = true;
        combatStarted = false;
        missionFinished = false;
        defending = false;

        if (activeMission != null)
        {
            activeMission.joystick = joystick;
        }

        if (missionText != null)
        {
            missionText.text = "Karte erkannt! Die Kämpfer bereiten sich vor...";
        }

        UpdateUI();
    }

    public void StartCombat()
    {
        if (!cardScanned || missionFinished)
        {
            return;
        }

        combatStarted = true;
        HideCountdown();

        if (missionText != null)
        {
            missionText.text = "Kampf beginnt! Geh näher zum Gegner und greife an!";
        }
    }

    public void ShowCountdown(string text)
    {
        if (countdownText == null)
        {
            return;
        }

        if (countdownAnimationCoroutine != null)
        {
            StopCoroutine(countdownAnimationCoroutine);
        }

        countdownAnimationCoroutine = StartCoroutine(AnimateCountdown(text));
    }

    public void HideCountdown()
    {
        if (countdownAnimationCoroutine != null)
        {
            StopCoroutine(countdownAnimationCoroutine);
            countdownAnimationCoroutine = null;
        }

        if (countdownText == null)
        {
            return;
        }

        countdownText.text = "";
        countdownText.transform.localScale = Vector3.one;
        countdownText.gameObject.SetActive(false);
    }

    private IEnumerator AnimateCountdown(string text)
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = text;
        countdownText.color = GetCountdownColor(text);

        float timer = 0f;

        Vector3 startScale = Vector3.one * countdownStartScale;
        Vector3 peakScale = Vector3.one * countdownPeakScale;
        Vector3 endScale = Vector3.one * countdownEndScale;

        countdownText.transform.localScale = startScale;

        Color startColor = countdownText.color;
        startColor.a = 1f;
        countdownText.color = startColor;

        while (timer < countdownAnimationDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / countdownAnimationDuration);

            if (progress < 0.45f)
            {
                float scaleProgress = progress / 0.45f;
                countdownText.transform.localScale = Vector3.Lerp(startScale, peakScale, scaleProgress);
            }
            else
            {
                float scaleProgress = (progress - 0.45f) / 0.55f;
                countdownText.transform.localScale = Vector3.Lerp(peakScale, endScale, scaleProgress);
            }

            if (progress > 0.75f)
            {
                float fadeProgress = (progress - 0.75f) / 0.25f;

                Color color = countdownText.color;
                color.a = Mathf.Lerp(1f, 0.25f, fadeProgress);
                countdownText.color = color;
            }

            yield return null;
        }

        countdownText.transform.localScale = endScale;
    }

    private Color GetCountdownColor(string text)
    {
        if (text == "3")
        {
            return new Color(1f, 0.35f, 0f, 1f);
        }

        if (text == "2")
        {
            return new Color(1f, 0.65f, 0f, 1f);
        }

        if (text == "1")
        {
            return new Color(1f, 0.9f, 0f, 1f);
        }

        return new Color(1f, 0.95f, 0.25f, 1f);
    }

    public void Attack()
    {
        if (!CanFight())
        {
            return;
        }

        if (!IsEnemyInRange(attackRange))
        {
            if (missionText != null)
            {
                missionText.text = "Zu weit entfernt! Geh näher zum Gegner.";
            }

            return;
        }

        int damage = Random.Range(attackMinDamage, attackMaxDamage + 1);

        enemyHP -= damage;

        if (enemyHP < 0)
        {
            enemyHP = 0;
        }

        if (activeMission != null)
        {
            activeMission.PlayAttackMotion();
        }

        PlaySound(attackSound);
        PlaySound(hitSound);

        if (missionText != null)
        {
            missionText.text = "Angriff! Gegner verliert " + damage + " HP.";
        }

        CheckGameState();
        UpdateUI();
    }

    public void Special()
    {
        if (!CanFight())
        {
            return;
        }

        if (!IsEnemyInRange(specialRange))
        {
            if (missionText != null)
            {
                missionText.text = "Spezialangriff fehlgeschlagen! Gegner ist zu weit weg.";
            }

            return;
        }

        int damage = Random.Range(specialMinDamage, specialMaxDamage + 1);

        enemyHP -= damage;

        if (enemyHP < 0)
        {
            enemyHP = 0;
        }

        if (activeMission != null)
        {
            activeMission.PlayAttackMotion();
        }

        PlaySound(specialSound);
        PlaySound(hitSound);

        if (missionText != null)
        {
            missionText.text = "Spezialangriff! Gegner verliert " + damage + " HP.";
        }

        CheckGameState();
        UpdateUI();
    }

    public void Defend()
    {
        if (!CanFight())
        {
            return;
        }

        defending = true;

        if (activeMission != null)
        {
            activeMission.PlayPlayerDefend();
        }

        if (missionText != null)
        {
            missionText.text = "Verteidigung bereit! Der nächste gegnerische Angriff wird blockiert.";
        }
    }

    public void TakePlayerDamage()
    {
        if (!CanFight())
        {
            return;
        }

        if (defending)
        {
            defending = false;

            PlaySound(blockSound);

            if (missionText != null)
            {
                missionText.text = "Block erfolgreich! Du hast keinen Schaden erhalten.";
            }

            UpdateUI();
            return;
        }

        int damage = Random.Range(enemyMinDamage, enemyMaxDamage + 1);

        playerHP -= damage;

        if (playerHP < 0)
        {
            playerHP = 0;
        }

        PlaySound(hitSound);

        if (missionText != null)
        {
            missionText.text = "Gegner greift an! Du verlierst " + damage + " HP.";
        }

        CheckGameState();
        UpdateUI();
    }

    private bool CanFight()
    {
        if (missionFinished)
        {
            return false;
        }

        if (!cardScanned || activeMission == null)
        {
            if (missionText != null)
            {
                missionText.text = "Scanne zuerst die AR-Karte!";
            }

            return false;
        }

        if (!combatStarted)
        {
            if (missionText != null)
            {
                missionText.text = "Warte, bis der Kampf beginnt!";
            }

            return false;
        }

        return true;
    }

    private bool IsEnemyInRange(float range)
    {
        if (activeMission == null)
        {
            if (missionText != null)
            {
                missionText.text = "Keine AR-Karte erkannt!";
            }

            return false;
        }

        float distance = activeMission.GetDistanceToEnemy();

        return distance <= range;
    }

    private void CheckGameState()
    {
        if (enemyHP <= 0)
        {
            missionFinished = true;
            combatStarted = false;
            score += 100;

            PlaySound(winSound);

            if (activeMission != null)
            {
                activeMission.PlayEnemyDeath();
                activeMission.PlayPlayerVictory();
            }

            if (missionText != null)
            {
                missionText.text = "Mission abgeschlossen! +100 Punkte";
            }

            UpdateUI();

            Invoke(nameof(LoadWinScene), 3f);
        }
        else if (playerHP <= 0)
        {
            missionFinished = true;
            combatStarted = false;

            PlaySound(gameOverSound);

            if (activeMission != null)
            {
                activeMission.PlayPlayerDeath();
            }

            if (missionText != null)
            {
                missionText.text = "Mission fehlgeschlagen!";
            }

            UpdateUI();

            Invoke(nameof(LoadGameOverScene), 3f);
        }
    }

    private void UpdateUI()
    {
        if (playerHPText != null)
        {
            playerHPText.text = "Player HP: " + playerHP;
        }

        if (enemyHPText != null)
        {
            enemyHPText.text = "Enemy HP: " + enemyHP;
        }

        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }

        if (playerHPFill != null && maxPlayerHP > 0)
        {
            playerHPFill.fillAmount = Mathf.Clamp01((float)playerHP / maxPlayerHP);
        }

        if (enemyHPFill != null && maxEnemyHP > 0)
        {
            enemyHPFill.fillAmount = Mathf.Clamp01((float)enemyHP / maxEnemyHP);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void LoadWinScene()
    {
        SceneManager.LoadScene("WinScene");
    }

    private void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}