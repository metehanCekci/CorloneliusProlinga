using UnityEngine;

public class SkateSoundManager : MonoBehaviour
{
    [Header("Audio Sources (Sürükle Býrak)")]
    public AudioSource loopSource;
    public AudioSource sfxSource;

    [Header("Ses Dosyalarý (Sürükle Býrak)")]
    public AudioClip skateLoopClip;
    public AudioClip railLoopClip;
    public AudioClip brakeClip; // YENÝ: Buraya fren sesini at
    public AudioClip ollieClip;
    public AudioClip landClip;

    [Header("Ayarlar")]
    public float minSpeedForSound = 0.5f;

    // Durumlarý artýk dýþarýdan (Controller'dan) yönetiyoruz, 
    // burada sadece "þu an ne çalýyor" kontrolü yapacaðýz.

    public void StartSkating(float currentSpeed)
    {
        if (currentSpeed < minSpeedForSound)
        {
            StopLoop();
            return;
        }

        // Eðer zaten Skate sesi çalýyorsa elleme (kesinti olmasýn)
        if (loopSource.isPlaying && loopSource.clip == skateLoopClip) return;

        loopSource.clip = skateLoopClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StartRailGrind()
    {
        // Zaten Rail çalýyorsa elleme
        if (loopSource.isPlaying && loopSource.clip == railLoopClip) return;

        loopSource.clip = railLoopClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StartBraking()
    {
        // YENÝ: Zaten Fren çalýyorsa elleme
        if (loopSource.isPlaying && loopSource.clip == brakeClip) return;

        loopSource.clip = brakeClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StopLoop()
    {
        // Loop kaynaðýný tamamen durdurur
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