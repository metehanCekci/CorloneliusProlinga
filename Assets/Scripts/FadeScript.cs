using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadeScript : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeSuresi = 1.0f;
    public bool mainMenu = true;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        // Başlangıçta obje açık olsun
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        StartCoroutine(FadeIslemi(1, 0));
    }

    // --- ÖZEL FONKSİYONLAR (DEATH SCRIPT BUNLARI ÇAĞIRIYOR) ---

    // KARARTMA
    public IEnumerator FadeOutEkraniKarart(float sure)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0;

        float counter = 0f;
        while (counter < sure)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / sure);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    // AYDINLATMA (Burada SetActive(false) YOK!)
    public IEnumerator FadeInEkraniAc(float sure)
    {
        float counter = 0f;
        while (counter < sure)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, counter / sure);
            yield return null;
        }
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;

        // ÖNEMLİ: gameObject.SetActive(false) SİLİNDİ.
        // Obje sahnede hep açık kalacak, sadece görünmez olacak.
    }

    // --- NORMAL SAHNE GEÇİŞLERİ ---

    public void SiradakiSahne()
    {
        int mevcutIndex = SceneManager.GetActiveScene().buildIndex;
        if (mevcutIndex + 1 < SceneManager.sceneCountInBuildSettings)
            BaslatFadeOut(mevcutIndex + 1);
        else
        { Debug.Log("başka sahne yok amınığlu"); }
    }

    public void BaslatFadeOut(int hedefIndex)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        StartCoroutine(FadeIslemi(0, 1, true, hedefIndex));
    }

    IEnumerator FadeIslemi(float start, float end, bool sahneYukle = false, int hedefIndex = -1)
    {
        float counter = 0f;
        while (counter < fadeSuresi)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / fadeSuresi);
            yield return null;
        }
        canvasGroup.alpha = end;

        if (!sahneYukle)
        {
            canvasGroup.blocksRaycasts = false;
            // Buradaki SetActive(false) da kapalı kalsın garanti olsun.
        }
        else
        {
            SceneManager.LoadScene(hedefIndex);
        }
    }
}