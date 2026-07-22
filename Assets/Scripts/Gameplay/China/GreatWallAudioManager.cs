using UnityEngine;

public class GreatWallAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource loopSource;

    [Header("Countdown")]
    public AudioClip countdownTickClip;
    public AudioClip startClip;
    public AudioClip timeUpClip;

    [Header("Brick Sounds")]
    public AudioClip brickSpawnClip;
    public AudioClip brickPickClip;
    public AudioClip brickPlaceClip;

    [Header("Battle Sounds")]
    public AudioClip warHornClip;
    public AudioClip soldierRunLoopClip;
    public AudioClip wallCollapseClip;

    [Header("Result Sounds")]
    public AudioClip winClip;
    public AudioClip loseClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float loopVolume = 0.45f;

    private void Awake()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (loopSource == null)
        {
            GameObject loopObject = new GameObject("LoopAudioSource");
            loopObject.transform.SetParent(transform);
            loopObject.transform.localPosition = Vector3.zero;

            loopSource = loopObject.AddComponent<AudioSource>();
        }

        sfxSource.playOnAwake = false;
        loopSource.playOnAwake = false;
        loopSource.loop = true;
    }

    public void PlayCountdownTick()
    {
        PlaySfx(countdownTickClip);
    }

    public void PlayStart()
    {
        PlaySfx(startClip);
    }

    public void PlayTimeUp()
    {
        PlaySfx(timeUpClip);
    }

    public void PlayBrickSpawn()
    {
        PlaySfx(brickSpawnClip, 0.45f);
    }

    public void PlayBrickPick()
    {
        PlaySfx(brickPickClip, 0.8f);
    }

    public void PlayBrickPlace()
    {
        PlaySfx(brickPlaceClip, 0.8f);
    }

    public void PlayWarHorn()
    {
        PlaySfx(warHornClip);
    }

    public void PlayWallCollapse()
    {
        PlaySfx(wallCollapseClip);
    }

    public void PlayWin()
    {
        StopSoldierRunLoop();
        PlaySfx(winClip);
    }

    public void PlayLose()
    {
        StopSoldierRunLoop();
        PlaySfx(loseClip);
    }

    public void StartSoldierRunLoop()
    {
        if (soldierRunLoopClip == null || loopSource == null)
        {
            return;
        }

        loopSource.clip = soldierRunLoopClip;
        loopSource.volume = loopVolume;
        loopSource.loop = true;

        if (!loopSource.isPlaying)
        {
            loopSource.Play();
        }
    }

    public void StopSoldierRunLoop()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        PlaySfx(clip, sfxVolume);
    }

    private void PlaySfx(AudioClip clip, float volumeMultiplier)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }
}