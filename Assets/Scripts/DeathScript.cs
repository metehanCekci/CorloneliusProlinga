using UnityEngine;
using System.Collections;

public class DeathScript : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Transform initialRespawnPoint;

    [Header("Ayarlar")]
    [SerializeField] private float fadeDuration = 1f; // Kararma/Açılma süresi
    [SerializeField] private float waitAtBlack = 0.8f; // Siyah ekranda bekleme süresi (Artırdım ki kamera yetişsin)

    private Vector3 currentRespawnPosition;
    private ControllerScript controller;
    private Gravity gravity;
    private bool isDead = false;

    void Start()
    {
        controller = GetComponent<ControllerScript>();
        gravity = GetComponent<Gravity>();

        // İlk spawn noktasını ayarla
        currentRespawnPosition = initialRespawnPoint != null ? initialRespawnPoint.position : transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Death"))
        {
            StartCoroutine(DeathSequence());
        }

        if (other.CompareTag("Checkpoint"))
        {
            UpdateCheckpoint(other.transform.position);
        }
    }

    private void UpdateCheckpoint(Vector3 newPos)
    {
        if (currentRespawnPosition != newPos)
        {
            currentRespawnPosition = newPos;
            Debug.Log("Checkpoint Alındı!");
        }
    }

    public IEnumerator DeathSequence()
    {
        isDead = true;

        // 1. FadeScript'i bul
        FadeScript fadeSystem = Object.FindAnyObjectByType<FadeScript>();

        if (fadeSystem == null)
        {
            Debug.LogError("FadeScript bulunamadı!");
            RespawnLogic();
            isDead = false;
            yield break;
        }

        // 2. Hareketleri dondur
        controller.enabled = false;
        gravity.SetVelocity(Vector3.zero);

        // OYUNU DURDUR
        Time.timeScale = 0f;

        // 3. EKRANI KARART
        yield return StartCoroutine(fadeSystem.FadeOutEkraniKarart(fadeDuration));

        // 4. OYUNCUYU IŞINLA
        // --- DÜZELTME BURADA BAŞLIYOR ---

        // Eğer karakter ölürken bir rayın üzerindeyse, önce ray sistemine "beni bırak" demeli.
        if (controller.activeRail != null)
        {
            // Yeni yazdığımız fonksiyonu çağır
            controller.activeRail.ForceDisconnect();
        }

        // Controller üzerindeki değerleri sıfırla
        controller.isGrinding = false;
        controller.activeRail = null;
        controller.railCooldown = 0.5f; // Hemen tekrar tutunmasın diye cooldown
        controller.momentumTime = 0f;

        // --- DÜZELTME BURADA BİTİYOR ---

        RespawnLogic();
        // 5. OYUNU DEVAM ETTİR (ÖNEMLİ NOKTA BURASI)
        // Zamanı başlattık ki Kamera (CameraFollow scripti) karakterin yeni yerine ışınlanabilsin/gidebilsin.
        Time.timeScale = 1f;

        // 6. EKRANI AÇ (GECİKMELİ)
        // Burada 2. parametreye 'waitAtBlack' veriyoruz.
        // FadeScript, bu süre kadar SİYAH EKRANDA bekleyecek.
        // Bu sırada arkada oyun çalıştığı için kamera yerine oturmuş olacak.
        yield return StartCoroutine(fadeSystem.FadeInEkraniAc(fadeDuration, waitAtBlack));

        // 7. Kontrolleri geri ver
        controller.enabled = true;
        isDead = false;
    }

    private void RespawnLogic()
    {
        // 1. ÖNCE Controller'daki ray bilgilerini temizle
        // Bunu yapınca RailSystem'deki Update bloğundaki "if (player.activeRail != this)" çalışacak
        // ve ray sistemi karakteri bırakacak.
        if (controller.activeRail != null)
        {
            // Eğer referans hala duruyorsa nezaketen bırak diyelim
            controller.activeRail.ForceDisconnect();
        }

        controller.isGrinding = false;
        controller.activeRail = null; // En kritik satır burası!
        controller.railCooldown = 0.5f;
        controller.momentumTime = 0f;
        controller.ResetSpeed();
        controller.ResetDash();

        // 2. SONRA pozisyonu değiştir
        // Artık ray sistemi karakteri tutmadığı için uçma olmayacak.
        transform.position = currentRespawnPosition;
        gravity.SetVelocity(Vector3.zero);

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.ResetMaskToDefault();
        }
    }
}
