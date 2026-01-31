using UnityEngine;

public class MusicLoopManager : MonoBehaviour
{
    [Header("Audio Sources (2 Tane Ekle)")]
    public AudioSource introSource; // Sadece Intro çalacak kaynak
    public AudioSource loopSource;  // Sonsuza kadar dönecek kaynak

    [Header("Müzik Dosyalarý")]
    public AudioClip introClip;
    public AudioClip loopClip;

    void Start()
    {
        // 1. Kaynaklarý hazýrla
        introSource.clip = introClip;
        loopSource.clip = loopClip;

        // Loop kaynaðýnýn döngü ayarýný aç
        loopSource.loop = true;

        // 2. Intro'nun tam süresini hesapla (Saniye cinsinden double precision kullanýyoruz)
        // clip.length kullanma, o float olduðu için bazen ritim kaçýrýr.
        double introDuration = (double)introClip.samples / introClip.frequency;

        // 3. Þu anki ses motoru zamanýný al
        double startTime = AudioSettings.dspTime + 0.1; // 0.1sn gecikmeli baþlat ki motor hazýr olsun

        // 4. Intro'yu hemen (0.1sn sonra) baþlat
        introSource.PlayScheduled(startTime);

        // 5. Loop parçasýný TAM intro'nun bittiði saniyeye rezerve et
        loopSource.PlayScheduled(startTime + introDuration);
    }
}