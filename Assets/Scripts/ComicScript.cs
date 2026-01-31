using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI elementlerine erişmek için gerekli

public class ComicScript : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Her bir karenin görünür olma süresi (saniye)")]
    public float fadeDuration = 1.0f;

    [Tooltip("Bir sonraki kareye geçmeden önceki bekleme süresi")]
    public float delayBetweenPanels = 0.5f;
    FadeScript transition;


    private void Start()
    {
        // Oyuna başlar başlamaz hikayeyi oynat
        transition = FindObjectOfType<FadeScript>();
        StartCoroutine(PlayComicSequence());
        Debug.Log("sa");
    }

    IEnumerator PlayComicSequence()
    {
        // 1. ADIM: Panel içindeki tüm resimleri sırasıyla al
        // transform.childCount kullanarak hiyerarşideki sıraya göre gideriz.
        List<Image> comicPanels = new List<Image>();

        foreach (Transform child in transform)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                // Başlangıçta hepsinin alpha'sını 0 yap (Görünmez)
                Color c = img.color;
                c.a = 0f;
                img.color = c;

                comicPanels.Add(img);
            }
        }

        // 2. ADIM: Sırayla her birini görünür yap
        foreach (Image panelImg in comicPanels)
        {
            yield return StartCoroutine(FadeInImage(panelImg));

            // Bir sonraki kareye geçmeden önce biraz bekle (opsiyonel)
            yield return new WaitForSeconds(delayBetweenPanels);
        }

        Debug.Log("Hikaye anlatımı bitti!");
        transition.SiradakiSahne();
        // Buraya "Devam Et" butonu açma kodu ekleyebilirsin.
    }

    // Alpha değerini 0'dan 1'e yumuşakça çeken fonksiyon
    IEnumerator FadeInImage(Image targetImage)
    {
        float elapsedTime = 0f;
        Color c = targetImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp (Linear Interpolation) 0 ile 1 arasında yumuşak geçiş sağlar
            float newAlpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

            c.a = newAlpha;
            targetImage.color = c;

            yield return null; // Bir sonraki frame'i bekle
        }

        // Garanti olsun diye döngü bitince alpha'yı tam 1 yap
        c.a = 1f;
        targetImage.color = c;
    }

}
