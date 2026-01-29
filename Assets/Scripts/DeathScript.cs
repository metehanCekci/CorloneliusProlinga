using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathScript : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Image fadeImage;      
    [SerializeField] private Transform initialRespawnPoint; // İlk doğuş yeri

    [Header("Ayarlar")]
    [SerializeField] private float fadeSpeed = 2f;    
    [SerializeField] private float waitAtBlack = 0.5f; 

    private Vector3 currentRespawnPosition; // Aktif checkpoint konumu
    private ControllerScript controller;
    private Gravity gravity;
    private bool isDead = false;

    void Start()
    {
        controller = GetComponent<ControllerScript>();
        gravity = GetComponent<Gravity>();

        // Başlangıç checkpoint'ini belirle
        if (initialRespawnPoint != null)
            currentRespawnPosition = initialRespawnPoint.position;
        else
            currentRespawnPosition = transform.position; // Atanmamışsa başladığı yer

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ÖLÜM KONTROLÜ
        if (other.CompareTag("Death") && !isDead)
        {
            StartCoroutine(DeathSequence());
        }
        
        // CHECKPOINT KONTROLÜ
        if (other.CompareTag("Checkpoint"))
        {
            UpdateCheckpoint(other.transform.position);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Death") && !isDead)
        {
            StartCoroutine(DeathSequence());
        }

        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            UpdateCheckpoint(collision.transform.position);
        }
    }

    private void UpdateCheckpoint(Vector3 newPos)
    {
        // Eğer yeni checkpoint konumu eskisinden farklıysa güncelle
        if (currentRespawnPosition != newPos)
        {
            currentRespawnPosition = newPos;
            Debug.Log("Checkpoint Alındı! Konum: " + newPos);
            // İstersen buraya küçük bir "Checkpoint!" yazısı veya partikül ekleyebilirsin
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;
        
        // 1. Kontrolü Kapat
        controller.enabled = false;
        gravity.SetVelocity(Vector2.zero);

        // 2. Ekranı Karart
        while (fadeImage.color.a < 1)
        {
            Color c = fadeImage.color;
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        // 3. Siyah Ekranda Bekle ve Kayıtlı Checkpoint'e Işınlan
        yield return new WaitForSeconds(waitAtBlack);
        transform.position = currentRespawnPosition;
        
        gravity.SetVelocity(Vector2.zero);

        // 4. Ekranı Aç
        while (fadeImage.color.a > 0)
        {
            Color c = fadeImage.color;
            c.a -= Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        // 5. Kontrolü Geri Ver
        controller.enabled = true;
        isDead = false;
    }
}