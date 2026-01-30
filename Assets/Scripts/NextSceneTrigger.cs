using UnityEngine;

public class NextSceneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Temas eden objenin "Player" tag'ine sahip olup olmadýðýný kontrol et
        if (collision.CompareTag("Player"))
        {
            // Sahnede FadeScript tipindeki objeyi bul (Düzeltildi: <T> kullanýmý)
            FadeScript transition = FindObjectOfType<FadeScript>();
            Debug.Log("sa");

            if (transition != null)
            {
                transition.SiradakiSahne();
            }
            else
            {
                Debug.LogError("Sahnede FadeScript bileþeni bulunamadý! Lütfen Canvas'ý ve scripti kontrol et.");
            }
        }
    }
}