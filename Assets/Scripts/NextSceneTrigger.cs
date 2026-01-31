using UnityEngine;

public class NextSceneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Temas eden objenin "Player" tag'ine sahip olup olmad���n� kontrol et
        if (collision.CompareTag("Player"))
        {
            // Sahnede FadeScript tipindeki objeyi bul (D�zeltildi: <T> kullan�m�)
            FadeScript transition = FindObjectOfType<FadeScript>();

            if (transition != null)
            {
                transition.SiradakiSahne();
            }
            else
            {
                Debug.LogError("Sahnede FadeScript bile�eni bulunamad�! L�tfen Canvas'� ve scripti kontrol et.");
            }
        }
    }
}
