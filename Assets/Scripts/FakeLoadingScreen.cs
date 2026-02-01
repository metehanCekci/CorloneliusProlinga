using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FakeLoadingScreen : MonoBehaviour
{
    [Header("Bilesenler")]
    public Image targetImage;        // Animasyonun oynayacağı UI Image
    public Sprite[] animationFrames; // Görseller

    [Header("Ayarlar")]
    public float fakeWaitTime = 3.0f; // Bekleme süresi
    public float frameRate = 12.0f;   // Animasyon hızı

    private void Start()
    {
        if (targetImage == null || animationFrames.Length == 0)
        {
            Debug.LogError("Lütfen Inspector'dan Image ve Sprite'ları ata!");
            return;
        }

        StartCoroutine(PlayFakeLoading());
    }

    private IEnumerator PlayFakeLoading()
    {
        float timer = 0f;
        int frameIndex = 0;
        float frameTimer = 0f;
        float timePerFrame = 1f / frameRate;

        // Bu değişkeni sahne yükleme emrini sadece 1 kere vermek için kullanacağız
        bool hasTriggeredNextScene = false;

        // KANKA BURASI ARTIK SONSUZ DÖNGÜ (while true)
        // Sahne değişip bu obje yok olana kadar animasyon hep döner.
        while (true)
        {
            // --- 1. KISIM: ANİMASYON (Hep çalışır) ---
            frameTimer += Time.deltaTime;

            if (frameTimer >= timePerFrame)
            {
                frameTimer -= timePerFrame;
                frameIndex = (frameIndex + 1) % animationFrames.Length;
                targetImage.sprite = animationFrames[frameIndex];
            }

            // --- 2. KISIM: SÜRE KONTROLÜ ---
            // Eğer henüz geçiş emri verilmediyse süreyi say
            if (!hasTriggeredNextScene)
            {
                timer += Time.deltaTime;

                // Süre dolduysa GEÇİŞİ BAŞLAT ama döngüden çıkma
                if (timer >= fakeWaitTime)
                {
                    hasTriggeredNextScene = true; // Bir daha buraya girmesin
                    LoadNextScene();
                }
            }

            yield return null; // Frame bekle ve devam et
        }
    }

    private void LoadNextScene()
    {
        FadeScript transition = FindObjectOfType<FadeScript>();

        if (transition != null)
        {
            transition.SiradakiSahne();
        }
        else
        {
            Debug.LogError("Sahnede FadeScript bulunamadı!");
        }
    }
}