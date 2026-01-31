using UnityEngine;

public class SkateSoundManager : MonoBehaviour
{
    [Header("Audio Sources (Sürükle Bırak)")]
    public AudioSource loopSource;
    public AudioSource sfxSource;

    [Header("Ses Dosyaları (Sürükle Bırak)")]
    public AudioClip skateLoopClip;
    public AudioClip railLoopClip;
    public AudioClip brakeClip; // YENİ: Buraya fren sesini at
    public AudioClip ollieClip;
    public AudioClip landClip;

    [Header("Ayarlar")]
    public float minSpeedForSound = 0.5f;

    // Durumları artık dışarıdan (Controller'dan) yönetiyoruz, 
    // burada sadece "şu an ne çalıyor" kontrolü yapacağız.

    public void StartSkating(float currentSpeed)
    {
        if (currentSpeed < minSpeedForSound)
        {
            StopLoop();
            return;
        }

        // Eğer zaten Skate sesi çalıyorsa elleme (kesinti olmasın)
        if (loopSource.isPlaying && loopSource.clip == skateLoopClip) return;

        loopSource.clip = skateLoopClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StartRailGrind()
    {
        // Zaten Rail çalıyorsa elleme
        if (loopSource.isPlaying && loopSource.clip == railLoopClip) return;

        loopSource.clip = railLoopClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StartBraking()
    {
        // YENİ: Zaten Fren çalıyorsa elleme
        if (loopSource.isPlaying && loopSource.clip == brakeClip) return;

        loopSource.clip = brakeClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StopLoop()
    {
        // Loop kaynağını tamamen durdurur
        if (loopSource.isPlaying) loopSource.Stop();
    }

    public void PlayOllie()
    {
        sfxSource.PlayOneShot(ollieClip);
    }

    public void PlayLand()
    {
        sfxSource.PlayOneShot(landClip);
    }
}