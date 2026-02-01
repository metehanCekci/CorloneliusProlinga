using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FakeLoadingScreen : MonoBehaviour
{
    [Header("Bilesenler")]
    public Image targetImage;       // Animasyonun oynayacağı UI Image
    public Sprite[] animationFrames; // 9 karelik görsellerin buraya gelecek

    [Header("Ayarlar")]
    public float fakeWaitTime = 3.0f; // Kullanıcı ne kadar beklesin (saniye)
    public float frameRate = 12.0f;   // Animasyon hızı (saniyedeki kare sayısı)
    public string sceneToLoad;        // Yüklenecek sahnenin tam adı

    private void Start()
    {
        // Eğer görsel atanmadıysa hata vermemesi için kontrol
        if (targetImage == null || animationFrames.Length == 0)
        {
            Debug.LogError("Lütfen Inspector'dan Image ve Sprite'ları atadığından emin ol!");
            return;
        }

        StartCoroutine(PlayFakeLoading());
    }

    private IEnumerator PlayFakeLoading()
    {
        float timer = 0f;
        int frameIndex = 0;
        float frameTimer = 0f;
        float timePerFrame = 1f / frameRate; // Her karenin ne kadar ekranda kalacağı

        // Belirlenen süre (fakeWaitTime) dolana kadar döngü devam eder
        while (timer < fakeWaitTime)
        {
            // Zamanlayıcıları güncelle
            timer += Time.deltaTime;
            frameTimer += Time.deltaTime;

            // Animasyon karesini değiştirme mantığı
            if (frameTimer >= timePerFrame)
            {
                frameTimer -= timePerFrame; // Sayacı sıfırlama (taşmayı koruyarak)
                
                // Sıradaki kareye geç, dizi biterse başa dön (%)
                frameIndex = (frameIndex + 1) % animationFrames.Length;
                
                // Görseli güncelle
                targetImage.sprite = animationFrames[frameIndex];
            }

            yield return null; // Bir sonraki frame'i bekle
        }

        // Süre doldu, sahneyi yükle
        SceneManager.LoadScene(sceneToLoad);
    }
}