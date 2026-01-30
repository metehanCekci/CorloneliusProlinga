using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathScript : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private Transform initialRespawnPoint;

    [Header("Ayarlar")]
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float waitAtBlack = 0.5f;

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

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
    }

    // Update içindeki CheckDeathCollision'ı tamamen sildik, çünkü tetiklenme üzerinden gideceğiz.

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // İster Tag kullan, ister Layer; ikisi de burada çalışır.
        // "Death" tag'ine sahip bir şeye değerse ölür.
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
            Debug.Log("Checkpoint Alindi!");
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        // Hareketleri dondur
        controller.enabled = false;
        gravity.SetVelocity(Vector3.zero);

        // Ekranı karart
        while (fadeImage.color.a < 1)
        {
            Color c = fadeImage.color;
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(waitAtBlack);

        // Pozisyonu güncelle ve hızları sıfırla
        transform.position = currentRespawnPosition;
        gravity.SetVelocity(Vector3.zero);
        controller.ResetSpeed();
        controller.ResetDash();

        // Ekranı aç
        while (fadeImage.color.a > 0)
        {
            Color c = fadeImage.color;
            c.a -= Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        controller.enabled = true;
        isDead = false;
    }
}