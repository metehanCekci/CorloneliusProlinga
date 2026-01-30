using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadeScript : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeSuresi = 1.0f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        canvasGroup.alpha = 1;
        canvasGroup.gameObject.SetActive(true);
        // Sahne açılış efekti (Fade In)
        StartCoroutine(FadeIslemi(1, 0));
    }

    // ---------------------------------------------------------
    // SEÇENEK 1: OTOMATİK OLARAK BİR SONRAKİ SAHNEYE GEÇER
    // Butona bunu bağlarsan, Build Settings'deki sıradaki sahneyi açar.
    // ---------------------------------------------------------
    public void SiradakiSahne()
    {
        int mevcutIndex = SceneManager.GetActiveScene().buildIndex;
        int sonrakiIndex = mevcutIndex + 1;

        // Eğer son sahnede değilsek geçiş yap
        if (sonrakiIndex < SceneManager.sceneCountInBuildSettings)
        {
            BaslatFadeOut(sonrakiIndex);
        }
        else
        {
            Debug.LogWarning("Zaten son sahnedesin, daha ileri gidemem!");
            // İstersen burada Ana Menüye (Index 0) döndürebilirsin:
            // BaslatFadeOut(0); 
        }
    }

    // SEÇENEK 2: İNDEX İLE ÇAĞIRMA (Örn: 0. sahneye git)
    public void SahneGitIndex(int sahneNo)
    {
        BaslatFadeOut(sahneNo);
    }

    // SEÇENEK 3: İSİM İLE ÇAĞIRMA (Eski yöntem)
    public void SahneGitIsim(string sahneAdi)
    {
        // İsmi bulup indexe çevirip öyle yolluyoruz, kod tekrarı olmasın diye.
        int sahneNo = SceneUtility.GetBuildIndexByScenePath(sahneAdi);
        if (sahneNo != -1) // Sahne bulunduysa
        {
            BaslatFadeOut(sahneNo);
        }
        else
        {
            Debug.LogError("Bu isimde bir sahne bulunamadı: " + sahneAdi);
        }
    }

    // --- ARKA PLAN İŞLEMLERİ ---

    public void BaslatFadeOut(int hedefIndex)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        StartCoroutine(FadeIslemi(0, 1, true, hedefIndex));
    }

    // Coroutine artık sadece index ile çalışıyor (daha performanslı)
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
            canvasGroup.gameObject.SetActive(false);
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            SceneManager.LoadScene(hedefIndex);
        }
    }
    public void OyundanCik()
    {
        Debug.Log("Oyundan çıkış komutu verildi.");

        // Eğer Unity Editöründeysek (Test ediyorsak)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

        // Eğer gerçek oyundaysak (Build aldıysak)
#else
            Application.Quit();
#endif
    }
}
