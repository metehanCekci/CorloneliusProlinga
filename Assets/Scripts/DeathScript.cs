using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathScript : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Image fadeImage;      // UI'daki siyah resmi buraya sürükle
    [SerializeField] private Transform respawnPoint; // Karakterin doğacağı yer (boş bir obje)

    [Header("Ayarlar")]
    [SerializeField] private float fadeSpeed = 2f;    // Kararma hızı
    [SerializeField] private float waitAtBlack = 0.5f; // Siyah ekranda ne kadar beklesin?

    private ControllerScript controller;
    private Gravity gravity;
    private bool isDead = false;

    void Start()
    {
        controller = GetComponent<ControllerScript>();
        gravity = GetComponent<Gravity>();

        // Oyun başında ekranın açık olduğundan emin olalım
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Death") && !isDead)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Death") && !isDead)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;
        
        // 1. Kontrolü Kapat
        controller.enabled = false;
        gravity.SetVelocity(Vector2.zero);

        // 2. Ekranı Karart (Fade In)
        while (fadeImage.color.a < 1)
        {
            Color c = fadeImage.color;
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        // 3. Siyah Ekranda Bekle ve Işınlan
        yield return new WaitForSeconds(waitAtBlack);
        transform.position = respawnPoint.position;
        
        // Işınlanma sonrası hız sıfırlansın ki fırlamasın
        gravity.SetVelocity(Vector2.zero);

        // 4. Ekranı Aç (Fade Out)
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