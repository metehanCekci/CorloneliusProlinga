using UnityEngine;

public class AutoScroller : MonoBehaviour
{
    [Header("Hareket")]
    public float speed = 5f;
    // 1 = Ok Yönü (Sağ/Yukarı), -1 = Tersi (Sol/Aşağı)
    public float direction = 1f; 

    [Header("Döngü Ayarı (Local Space)")]
    public bool loop = true;
    // Inspector'da objeyi hareket ettirip X değerine bakarak bunları gir:
    public float limitX = 20f;  // Bu noktaya gelince başa dönsün
    public float startX = -20f; // Buraya ışınlansın

    void Update()
    {
        // Translate varsayılan olarak "Space.Self" çalışır.
        // Yani objen yamuksa, yamuk şekilde ilerler.
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        if (loop)
        {
            // Eğer ok yönünde gidiyorsan (Pozitif X) ve limiti geçtiysen
            if (direction > 0 && transform.localPosition.x >= limitX)
            {
                Vector3 newPos = transform.localPosition;
                newPos.x = startX;
                transform.localPosition = newPos;
            }
            // Eğer ters yöne gidiyorsan (Negatif X) ve limiti geçtiysen
            else if (direction < 0 && transform.localPosition.x <= limitX)
            {
                Vector3 newPos = transform.localPosition;
                newPos.x = startX;
                transform.localPosition = newPos;
            }
        }
    }
}