using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // Bu satır objeye otomatik AudioSource ekler
public class ComicScript : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    [Tooltip("Eğer 'Pop' diye gelsin istiyorsan burayı 0 yap.")]
    public float fadeDuration = 0.0f; 

    [Tooltip("Bir sonraki kareye geçmeden bekleme süresi")]
    public float delayBetweenPanels = 0.5f;

    [Tooltip("Sahne perdesinin açılmasını beklemek için burayı artır")]
    public float startDelay = 1.0f;

    [Header("Ses Ayarları")]
    public AudioClip impactSound; // BURAYA ÖRS SESİNİ SÜRÜKLE
    [Range(0f, 1f)] public float volume = 1.0f; // Ses seviyesi

    private AudioSource audioSource;
    private FadeScript transition;

    private void Start()
    {
        transition = FindObjectOfType<FadeScript>();
        audioSource = GetComponent<AudioSource>();
        
        // Temizlik
        PrepareImages();

        StartCoroutine(PlayComicSequence());
    }

    void PrepareImages()
    {
        foreach (Transform child in transform)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                if (img.GetComponent<Animator>()) img.GetComponent<Animator>().enabled = false;
                if (img.GetComponent<Button>()) img.GetComponent<Button>().enabled = false;

                Color c = img.color;
                c.a = 0f;
                img.color = c;
            }
        }
    }

    IEnumerator PlayComicSequence()
    {
        // Sahne açılışını bekle
        yield return new WaitForSeconds(startDelay);

        List<Image> comicPanels = new List<Image>();
        foreach (Transform child in transform)
        {
            Image img = child.GetComponent<Image>();
            if (img != null) comicPanels.Add(img);
        }

        foreach (Image panelImg in comicPanels)
        {
            // --- SESİ BURADA OYNATIYORUZ ---
            if (impactSound != null && audioSource != null)
            {
                // PlayOneShot, sesler üst üste binse bile kesilmemesini sağlar
                audioSource.PlayOneShot(impactSound, volume);
            }

            // GÖRSELİ AÇ
            if (fadeDuration <= 0.05f)
            {
                SetAlpha(panelImg, 1f);
            }
            else
            {
                yield return StartCoroutine(FadeInImage(panelImg));
            }

            yield return new WaitForSeconds(delayBetweenPanels);
        }

        Debug.Log("Hikaye bitti!");
        if(SceneManager.GetActiveScene().buildIndex == 12)
        {
             yield return new WaitForSeconds(5);
        }
        
        if (transition != null) transition.SiradakiSahne();
    }

    void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    IEnumerator FadeInImage(Image targetImage)
    {
        float elapsedTime = 0f;
        Color c = targetImage.color;
        c.a = 0f;
        targetImage.color = c;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            c.a = newAlpha;
            targetImage.color = c;
            yield return null;
        }

        c.a = 1f;
        targetImage.color = c;
    }
}
