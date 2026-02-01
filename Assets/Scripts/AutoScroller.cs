using UnityEngine;

public class AutoScroller : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public Vector3 direction = Vector3.left; // Objelerin kayacağı yön (Sola)
    public float speed = 5f; // Kayma hızı

    [Header("Sonsuz Döngü (Railler İçin)")]
    public bool loop = false; // Railler için TRUE, yazılar için FALSE yap
    public float limitX = -30f; // Ekrandan çıkma noktası (Sınır)
    public float resetX = 30f;  // Başa dönme noktası (Başlangıç)

    void Update()
    {
        // Objeyi belirlenen yöne doğru kaydır
        transform.Translate(direction * speed * Time.deltaTime);

        // Eğer döngü açıksa ve sınır geçildiyse objeyi başa ışınla
        if (loop && transform.position.x <= limitX)
        {
            Vector3 newPos = transform.position;
            newPos.x = resetX;
            transform.position = newPos;
        }
    }
}