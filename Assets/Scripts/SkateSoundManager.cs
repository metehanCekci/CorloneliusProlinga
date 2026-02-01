using UnityEngine;

public class SkateSoundManager : MonoBehaviour
{
    [Header("Audio Sources (Sürükle Býrak)")]
    public AudioSource loopSource;
    public AudioSource sfxSource;

    [Header("Ses Dosyalarý (Sürükle Býrak)")]
    public AudioClip skateLoopClip;
    public AudioClip railLoopClip;
    public AudioClip brakeClip;
    public AudioClip ollieClip;
    public AudioClip landClip;

    [Header("Yeni Efekt Sesleri")]
    public AudioClip dashClip;   // YENÝ: Dash sesi buraya
    public AudioClip pickupClip; // YENÝ: Pickup sesi buraya

    [Header("Ayarlar")]
    public float minSpeedForSound = 0.5f;

    // --- Loop Yönetimi ---

    public void StartSkating(float currentSpeed)
    {
        if (currentSpeed < minSpeedForSound)
        {
            StopLoop();
            return;
        }

        if (loopSource.isPlaying && loopSource.clip == skateLoopClip) return;

        loopSource.clip = skateLoopClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StartRailGrind()
    {
        if (loopSource.isPlaying && loopSource.clip == railLoopClip) return;

        loopSource.clip = railLoopClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StartBraking()
    {
        if (loopSource.isPlaying && loopSource.clip == brakeClip) return;

        loopSource.clip = brakeClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StopLoop()
    {
        if (loopSource.isPlaying) loopSource.Stop();
    }

    // --- One Shot (Tek Seferlik) Sesler ---

    public void PlayOllie()
    {
        // Ses atanmýþ mý diye basit bir kontrol eklemek her zaman iyidir
        if (ollieClip != null) sfxSource.PlayOneShot(ollieClip);
    }

    public void PlayLand()
    {
        if (landClip != null) sfxSource.PlayOneShot(landClip);
    }

    // YENÝ: Dash Sesi Fonksiyonu
    public void PlayDash()
    {
        if (dashClip != null) sfxSource.PlayOneShot(dashClip);
    }

    // YENÝ: Pickup (Toplama) Sesi Fonksiyonu
    public void PlayPickup()
    {
        if (pickupClip != null) sfxSource.PlayOneShot(pickupClip);
    }
}