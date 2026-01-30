using UnityEngine;
using System.Collections;

public class DeathScript : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Transform initialRespawnPoint;

    [Header("Ayarlar")]
    [SerializeField] private float fadeDuration = 1f; // Kararma süresi
    [SerializeField] private float waitAtBlack = 0.5f; // Siyah ekranda bekleme

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

    private IEnumerator DeathSequence()
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

        // 3. EKRANI KARART (Özel fonksiyonu çağırıyoruz)
        yield return StartCoroutine(fadeSystem.FadeOutEkraniKarart(fadeDuration));

        // 4. Karanlıkta bekle
        yield return new WaitForSeconds(waitAtBlack);

        // 5. Oyuncuyu Işınla ve Fizikleri Sıfırla
        RespawnLogic();

        // 6. EKRANI AÇ (Özel fonksiyonu çağırıyoruz)
        yield return StartCoroutine(fadeSystem.FadeInEkraniAc(fadeDuration));

        // 7. Kontrolleri geri ver
        controller.enabled = true;
        isDead = false;
    }

    private void RespawnLogic()
    {
        transform.position = currentRespawnPosition;
        gravity.SetVelocity(Vector3.zero);
        controller.ResetSpeed();
        controller.ResetDash();
    }
}