using System.Collections;
using UnityEngine;

public class RomanMissionController : MonoBehaviour
{
    [Header("Characters")]
    public Transform romanSoldier;
    public Transform enemyDummy;

    [Header("Hit Effects")]
    public ParticleSystem playerHitEffect;
    public ParticleSystem enemyHitEffect;

    [Header("Player Animation")]
    public CharacterAnimationController playerAnimation;

    [Header("Enemy Animation")]
    public CharacterAnimationController enemyAnimation;

    [Header("Input")]
    public VirtualJoystick joystick;

    [Header("Movement")]
    public float playerMoveSpeed = 0.12f;
    public float enemyMoveSpeed = 0.06f;
    public float arenaRadius = 0.15f;

    [Header("Enemy Combat")]
    public float enemyAttackRange = 0.055f;
    public float enemyAttackCooldown = 1.5f;

    [Header("Spawn / Countdown")]
    public float countdownDuration = 3f;
    public float playerSpawnYOffset = -0.12f;
    public float enemyEntranceDistance = 0.18f;
    public AudioSource audioSource;
    public AudioClip spawnSound;

    [Header("Visual Feedback")]
    public Color playerHitColor = Color.red;
    public Color enemyHitColor = Color.red;
    public float hitFlashDuration = 0.15f;
    public float attackBumpDistance = 0.015f;
    public float attackBumpDuration = 0.12f;

    private MissionGameController gameController;

    private Vector3 enemyTargetLocalPosition;
    private float nextEnemyTargetTime;
    private float nextEnemyAttackTime;

    private bool entranceFinished = false;
    private Vector3 playerOriginalLocalPosition;
    private Vector3 enemyOriginalLocalPosition;

    private Renderer[] playerRenderers;
    private Renderer[] enemyRenderers;

    private Material[][] playerMaterials;
    private Material[][] enemyMaterials;

    private Color[][] playerOriginalColors;
    private Color[][] enemyOriginalColors;

    private void Awake()
    {
        if (romanSoldier == null)
        {
            romanSoldier = transform.Find("RomanSoldier");
        }

        if (enemyDummy == null)
        {
            enemyDummy = transform.Find("EnemyDummy");
        }

        if (playerAnimation == null && romanSoldier != null)
        {
            playerAnimation = romanSoldier.GetComponent<CharacterAnimationController>();
        }

        if (enemyAnimation == null && enemyDummy != null)
        {
            enemyAnimation = enemyDummy.GetComponent<CharacterAnimationController>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (playerHitEffect == null && romanSoldier != null)
        {
            playerHitEffect = FindParticleSystemInChildren(romanSoldier, "PlayerHitEffect");
        }

        if (enemyHitEffect == null && enemyDummy != null)
        {
            enemyHitEffect = FindParticleSystemInChildren(enemyDummy, "EnemyHitEffect");
        }

        CacheRenderersAndMaterials();
    }

    private void Start()
    {
        gameController = MissionGameController.Instance;

        if (gameController != null)
        {
            gameController.RegisterMission(this);
            joystick = gameController.joystick;
        }

        SelectNewEnemyTarget();

        StartCoroutine(PlayEntranceCountdownSequence());
    }

    private void Update()
    {
        if (gameController == null)
        {
            gameController = MissionGameController.Instance;
        }

        if (gameController == null || !gameController.IsCardScanned)
        {
            StopMovementAnimations();
            return;
        }

        if (gameController.IsMissionFinished)
        {
            StopMovementAnimations();
            return;
        }

        if (!entranceFinished)
        {
            StopMovementAnimations();
            return;
        }

        MovePlayer();
        MoveEnemyRandomly();
        EnemyAttackIfInRange();
    }

    private IEnumerator PlayEntranceCountdownSequence()
    {
        entranceFinished = false;

        if (romanSoldier == null || enemyDummy == null)
        {
            yield break;
        }

        playerOriginalLocalPosition = romanSoldier.localPosition;
        enemyOriginalLocalPosition = enemyDummy.localPosition;

        Vector3 playerHiddenPosition = playerOriginalLocalPosition;
        playerHiddenPosition.y += playerSpawnYOffset;

        Vector3 enemyStartPosition = enemyOriginalLocalPosition;
        enemyStartPosition.z += enemyEntranceDistance;

        romanSoldier.localPosition = playerHiddenPosition;
        enemyDummy.localPosition = enemyStartPosition;

        StopMovementAnimations();
        PlaySpawnSound();

        float timer = 0f;
        int lastShownNumber = -1;

        while (timer < countdownDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / countdownDuration);

            romanSoldier.localPosition = Vector3.Lerp(playerHiddenPosition, playerOriginalLocalPosition, progress);
            enemyDummy.localPosition = Vector3.Lerp(enemyStartPosition, enemyOriginalLocalPosition, progress);

            Vector3 enemyDirection = enemyOriginalLocalPosition - enemyDummy.localPosition;
            enemyDirection.y = 0f;

            if (enemyDirection.sqrMagnitude > 0.001f)
            {
                enemyDummy.localRotation = Quaternion.LookRotation(enemyDirection.normalized, Vector3.up);
                PlayEnemyWalkAnimation();
            }

            int remaining = Mathf.CeilToInt(countdownDuration - timer);

            if (remaining != lastShownNumber)
            {
                lastShownNumber = remaining;

                if (gameController != null)
                {
                    if (remaining > 0)
                    {
                        gameController.ShowCountdown(remaining.ToString());
                    }
                    else
                    {
                        gameController.ShowCountdown("Kampf!");
                    }
                }
            }

            yield return null;
        }

        romanSoldier.localPosition = playerOriginalLocalPosition;
        enemyDummy.localPosition = enemyOriginalLocalPosition;

        StopMovementAnimations();

        if (gameController != null)
        {
            gameController.ShowCountdown("Kampf!");
        }

        yield return new WaitForSeconds(0.5f);

        entranceFinished = true;

        if (gameController != null)
        {
            gameController.StartCombat();
        }
    }

    private void PlaySpawnSound()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    private void CacheRenderersAndMaterials()
    {
        if (romanSoldier != null)
        {
            playerRenderers = romanSoldier.GetComponentsInChildren<Renderer>();
            playerMaterials = StoreMaterials(playerRenderers);
            playerOriginalColors = StoreOriginalColors(playerMaterials);
        }

        if (enemyDummy != null)
        {
            enemyRenderers = enemyDummy.GetComponentsInChildren<Renderer>();
            enemyMaterials = StoreMaterials(enemyRenderers);
            enemyOriginalColors = StoreOriginalColors(enemyMaterials);
        }
    }

    private void MovePlayer()
    {
        if (joystick == null || romanSoldier == null)
        {
            StopPlayerAnimation();
            return;
        }

        Vector2 input = joystick.Direction;

        if (input.sqrMagnitude < 0.001f)
        {
            StopPlayerAnimation();
            return;
        }

        Vector3 movement = new Vector3(input.x, 0f, input.y);
        Vector3 nextPosition = romanSoldier.localPosition + movement * playerMoveSpeed * Time.deltaTime;

        nextPosition = ClampToArena(nextPosition);
        nextPosition.y = romanSoldier.localPosition.y;

        romanSoldier.localPosition = nextPosition;

        if (movement.sqrMagnitude > 0.001f)
        {
            romanSoldier.localRotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
        }

        PlayPlayerWalkAnimation(input.magnitude);
    }

    private void MoveEnemyRandomly()
    {
        if (enemyDummy == null)
        {
            StopEnemyAnimation();
            return;
        }

        if (Time.time >= nextEnemyTargetTime)
        {
            SelectNewEnemyTarget();
        }

        Vector3 currentPosition = enemyDummy.localPosition;
        Vector3 direction = enemyTargetLocalPosition - currentPosition;
        direction.y = 0f;

        if (direction.magnitude < 0.01f)
        {
            StopEnemyAnimation();
            SelectNewEnemyTarget();
            return;
        }

        Vector3 movement = direction.normalized * enemyMoveSpeed * Time.deltaTime;
        Vector3 nextPosition = currentPosition + movement;

        nextPosition = ClampToArena(nextPosition);
        nextPosition.y = enemyDummy.localPosition.y;

        enemyDummy.localPosition = nextPosition;

        if (movement.sqrMagnitude > 0.001f)
        {
            enemyDummy.localRotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
            PlayEnemyWalkAnimation();
        }
        else
        {
            StopEnemyAnimation();
        }
    }

    private void SelectNewEnemyTarget()
    {
        float yPosition = enemyDummy != null ? enemyDummy.localPosition.y : 0.05f;

        if (romanSoldier != null && Random.value < 0.45f)
        {
            enemyTargetLocalPosition = romanSoldier.localPosition;
            enemyTargetLocalPosition.y = yPosition;
        }
        else
        {
            Vector2 randomPoint = Random.insideUnitCircle * arenaRadius;
            enemyTargetLocalPosition = new Vector3(randomPoint.x, yPosition, randomPoint.y);
        }

        nextEnemyTargetTime = Time.time + Random.Range(1.2f, 2.5f);
    }

    private Vector3 ClampToArena(Vector3 position)
    {
        Vector2 xzPosition = new Vector2(position.x, position.z);

        if (xzPosition.magnitude > arenaRadius)
        {
            xzPosition = xzPosition.normalized * arenaRadius;
            position.x = xzPosition.x;
            position.z = xzPosition.y;
        }

        return position;
    }

    private void EnemyAttackIfInRange()
    {
        if (gameController == null || gameController.IsMissionFinished || !gameController.IsCombatStarted)
        {
            return;
        }

        if (romanSoldier == null || enemyDummy == null)
        {
            return;
        }

        float distance = GetDistanceToEnemy();

        if (distance <= enemyAttackRange && Time.time >= nextEnemyAttackTime)
        {
            StopEnemyAnimation();

            if (enemyAnimation != null)
            {
                enemyAnimation.PlayAttack();
            }

            StartCoroutine(EnemyAttackBump());
            StartCoroutine(PlayerHitFlash());
            PlayPlayerHitEffect();

            gameController.TakePlayerDamage();

            nextEnemyAttackTime = Time.time + enemyAttackCooldown;
        }
    }

    public float GetDistanceToEnemy()
    {
        if (romanSoldier == null || enemyDummy == null)
        {
            return 999f;
        }

        Vector3 playerPosition = romanSoldier.localPosition;
        Vector3 enemyPosition = enemyDummy.localPosition;

        playerPosition.y = 0f;
        enemyPosition.y = 0f;

        return Vector3.Distance(playerPosition, enemyPosition);
    }

    public void PlayAttackMotion()
    {
        if (romanSoldier == null || enemyDummy == null)
        {
            return;
        }

        StopPlayerAnimation();

        if (playerAnimation != null)
        {
            playerAnimation.PlayAttack();
        }

        StartCoroutine(PlayerAttackBump());
        StartCoroutine(EnemyHitFlash());
        PlayEnemyHitEffect();
    }

    public void PlayPlayerDefend()
    {
        StopPlayerAnimation();

        if (playerAnimation != null)
        {
            playerAnimation.PlayDefend();
        }
    }

    public void PlayPlayerDeath()
    {
        StopMovementAnimations();

        if (playerAnimation != null)
        {
            playerAnimation.PlayDeath();
        }
    }

    public void PlayPlayerVictory()
    {
        StopMovementAnimations();

        if (playerAnimation != null)
        {
            playerAnimation.PlayVictory();
        }
    }

    public void PlayEnemyDeath()
    {
        StopEnemyAnimation();

        if (enemyAnimation != null)
        {
            enemyAnimation.PlayDeath();
        }
    }

    public void PlayPlayerHitEffect()
    {
        PlayHitEffect(playerHitEffect);
    }

    public void PlayEnemyHitEffect()
    {
        PlayHitEffect(enemyHitEffect);
    }

    private void PlayHitEffect(ParticleSystem hitEffect)
    {
        if (hitEffect == null)
        {
            return;
        }

        hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        hitEffect.Play();
    }

    private ParticleSystem FindParticleSystemInChildren(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem.gameObject.name == objectName)
            {
                return particleSystem;
            }
        }

        return null;
    }

    private void PlayPlayerWalkAnimation(float speed)
    {
        if (playerAnimation != null)
        {
            playerAnimation.SetMoveSpeed(speed);
        }
    }

    private void StopPlayerAnimation()
    {
        if (playerAnimation != null)
        {
            playerAnimation.SetMoveSpeed(0f);
        }
    }

    private void PlayEnemyWalkAnimation()
    {
        if (enemyAnimation != null)
        {
            enemyAnimation.SetMoveSpeed(1f);
        }
    }

    private void StopEnemyAnimation()
    {
        if (enemyAnimation != null)
        {
            enemyAnimation.SetMoveSpeed(0f);
        }
    }

    private void StopMovementAnimations()
    {
        StopPlayerAnimation();
        StopEnemyAnimation();
    }

    private IEnumerator PlayerAttackBump()
    {
        Vector3 startPosition = romanSoldier.localPosition;
        Vector3 direction = enemyDummy.localPosition - romanSoldier.localPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            yield break;
        }

        direction.Normalize();

        Vector3 attackPosition = startPosition + direction * attackBumpDistance;

        romanSoldier.localPosition = attackPosition;
        yield return new WaitForSeconds(attackBumpDuration);

        romanSoldier.localPosition = startPosition;
    }

    private IEnumerator EnemyAttackBump()
    {
        Vector3 startPosition = enemyDummy.localPosition;
        Vector3 direction = romanSoldier.localPosition - enemyDummy.localPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            yield break;
        }

        direction.Normalize();

        Vector3 attackPosition = startPosition + direction * attackBumpDistance;

        enemyDummy.localPosition = attackPosition;
        yield return new WaitForSeconds(attackBumpDuration);

        enemyDummy.localPosition = startPosition;
    }

    private IEnumerator EnemyHitFlash()
    {
        SetMaterialsColor(enemyMaterials, enemyHitColor);
        yield return new WaitForSeconds(hitFlashDuration);
        RestoreMaterialsColor(enemyMaterials, enemyOriginalColors);
    }

    private IEnumerator PlayerHitFlash()
    {
        SetMaterialsColor(playerMaterials, playerHitColor);
        yield return new WaitForSeconds(hitFlashDuration);
        RestoreMaterialsColor(playerMaterials, playerOriginalColors);
    }

    private Material[][] StoreMaterials(Renderer[] renderers)
    {
        if (renderers == null)
        {
            return new Material[0][];
        }

        Material[][] materials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                materials[i] = renderers[i].materials;
            }
            else
            {
                materials[i] = new Material[0];
            }
        }

        return materials;
    }

    private Color[][] StoreOriginalColors(Material[][] materials)
    {
        if (materials == null)
        {
            return new Color[0][];
        }

        Color[][] colors = new Color[materials.Length][];

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
            {
                colors[i] = new Color[0];
                continue;
            }

            colors[i] = new Color[materials[i].Length];

            for (int j = 0; j < materials[i].Length; j++)
            {
                if (materials[i][j] != null && materials[i][j].HasProperty("_Color"))
                {
                    colors[i][j] = materials[i][j].color;
                }
            }
        }

        return colors;
    }

    private void SetMaterialsColor(Material[][] materials, Color color)
    {
        if (materials == null)
        {
            return;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
            {
                continue;
            }

            for (int j = 0; j < materials[i].Length; j++)
            {
                if (materials[i][j] != null && materials[i][j].HasProperty("_Color"))
                {
                    materials[i][j].color = color;
                }
            }
        }
    }

    private void RestoreMaterialsColor(Material[][] materials, Color[][] originalColors)
    {
        if (materials == null || originalColors == null)
        {
            return;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null || i >= originalColors.Length || originalColors[i] == null)
            {
                continue;
            }

            for (int j = 0; j < materials[i].Length; j++)
            {
                if (materials[i][j] != null &&
                    materials[i][j].HasProperty("_Color") &&
                    j < originalColors[i].Length)
                {
                    materials[i][j].color = originalColors[i][j];
                }
            }
        }
    }
}