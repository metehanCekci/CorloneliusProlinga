using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlamEffect : MonoBehaviour
{
    [Header("Ayarlar")]
    public float startScale = 5f; // Resim ne kadar büyük başlayacak?
    public float slamSpeed = 0.15f; // Çarpma hızı (düşük = daha hızlı)
    public float shakeAmount = 0.5f; // Ekran ne kadar sallanacak?
    public float shakeDuration = 0.2f; // Sarsıntı ne kadar sürecek?

    [Header("Ses Efekti")]
    public AudioSource audioSource;
    public AudioClip slamSound;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Eğer CanvasGroup yoksa otomatik ekle (Görünmezden gelmesi için)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // Obje aktif olduğunda efekti başlat
        StartCoroutine(PlaySlam());
    }

    IEnumerator PlaySlam()
    {
        // 1. Başlangıç durumu: Çok büyük ve şeffaf
        rectTransform.localScale = Vector3.one * startScale;
        canvasGroup.alpha = 0f;

        float timer = 0f;

        // 2. Hızla küçülerek ekrana gel (Lerp)
        while (timer < 1f)
        {
            timer += Time.deltaTime / slamSpeed;
            
            // Vector3.Lerp ile boyutu 1'e indiriyoruz
            rectTransform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one, timer);
            
            // Görünürlüğü hızla aç
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer * 2); 

            yield return null;
        }

        // 3. TAM ÇARPMA ANI (Döngü bittiği an)
        rectTransform.localScale = Vector3.one; // Boyutu sabitle
        
        // Ekranı salla
        if(CameraShake.Instance != null)
            CameraShake.Instance.TriggerShake(shakeDuration, shakeAmount);

        // Ses çal (varsa)
        if (audioSource != null && slamSound != null)
            audioSource.PlayOneShot(slamSound);
    }
}
