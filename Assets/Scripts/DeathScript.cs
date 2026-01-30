using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathScript : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private CanvasGroup canvasgroup; // Inspector'dan atamayı unutma!
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

        // Başlangıçta ekranın açık olduğundan emin olalım
        if (canvasgroup != null)
        {
            canvasgroup.alpha = 0f;
            canvasgroup.blocksRaycasts = false; // Tıklamaları engellememesi için
        }
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

        // Hareketleri dondur
        controller.enabled = false;
        gravity.SetVelocity(Vector3.zero);

        // Ekranı karart (CanvasGroup Alpha Artışı)
        if (canvasgroup != null)
        {
            canvasgroup.blocksRaycasts = true; // Kararma sırasında yanlışlıkla bir şeye basılmasın
            while (canvasgroup.alpha < 1f)
            {
                canvasgroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        yield return new WaitForSeconds(waitAtBlack);

        // Pozisyonu güncelle ve hızları sıfırla
        transform.position = currentRespawnPosition;
        gravity.SetVelocity(Vector3.zero);
        controller.ResetSpeed();
        controller.ResetDash();

        // Ekranı aç (CanvasGroup Alpha Azalışı)
        if (canvasgroup != null)
        {
            while (canvasgroup.alpha > 0f)
            {
                canvasgroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            canvasgroup.blocksRaycasts = false;
        }

        controller.enabled = true;
        isDead = false;
    }
}