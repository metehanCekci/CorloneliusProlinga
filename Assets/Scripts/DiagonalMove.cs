using UnityEngine;

public class DiagonalMove : MonoBehaviour
{
    [Header("Hız Ayarları")]
    public float moveSpeedX = 2.0f; // Sağa gitme hızı
    public float moveSpeedY = 1.0f; // Yukarı gitme hızı

    [Header("Reset Ayarları (Sonsuz Döngü)")]
    public bool useLoop = true;
    public float limitY = 10f; // Bu yüksekliğe gelince başa dön
    public float startY = -10f; // Buradan başla
    public float startX = -10f; // X de buradan başlasın

    void Update()
    {
        // 1. Manuel Hareket: Hem X'i hem Y'yi artırıyoruz (Sağ-Üst Çapraz)
        transform.position += new Vector3(moveSpeedX, moveSpeedY, 0) * Time.deltaTime;

        // 2. Döngü Kontrolü
        if (useLoop && transform.position.y >= limitY)
        {
            Vector3 newPos = transform.position;
            newPos.y = startY;
            newPos.x = startX;
            transform.position = newPos;
        }
    }
}