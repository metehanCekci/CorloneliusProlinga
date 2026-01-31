using UnityEngine;

public class SlopeStabilizer : MonoBehaviour
{
    [Header("Ayarlar")]
    public float donusHizi = 20f;
    public float rayUzunlugu = 1.5f;
    public float rayYOffset = 0.5f;
    public LayerMask zeminLayer;

    [Header("4 Yönlü Offset Ayarı")]
    // Senin istediğin "Her durum için ayrı ayar" burada
    public float sagBakisYokusYukari = 0f;   // /
    public float sagBakisYokusAsagi = 0f;    // \
    public float solBakisYokusYukari = 0f;   // \ (Görsel olarak)
    public float solBakisYokusAsagi = 0f;    // / (Görsel olarak)

    [Header("Referanslar")]
    [Tooltip("Dönecek olan görsel obje (Sprite). Eğer boşsa otomatik bulur.")]
    public Transform gorselObje; 

    private ControllerScript controller;
    private Gravity gravity;

    void Start()
    {
        controller = GetComponent<ControllerScript>();
        gravity = GetComponent<Gravity>();
        
        if (zeminLayer == 0) zeminLayer = LayerMask.GetMask("Ground");

        // Eğer görsel objeyi elle atamadıysan, ilk çocuğu görsel kabul et
        if (gorselObje == null)
        {
            gorselObje = transform.GetChild(0); 
        }
    }

    void LateUpdate()
    {
        bool yerdeyis = false;
        if (controller != null) yerdeyis = controller.IsGrounded();
        else if (gravity != null) yerdeyis = gravity.IsGrounded();

        // Yokuş aşağı hızla inerken temas kopmasın
        if (!yerdeyis && gravity != null && gravity.GetVelocity().y <= 0.1f)
        {
            yerdeyis = Physics2D.Raycast(transform.position + Vector3.up * rayYOffset, Vector2.down, rayUzunlugu, zeminLayer);
        }

        if (yerdeyis)
        {
            ZemineGoreDon();
        }
        else
        {
            // Havada görseli düzelt
            gorselObje.localRotation = Quaternion.Slerp(gorselObje.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }

    void ZemineGoreDon()
    {
        Vector3 origin = transform.position + Vector3.up * rayYOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayUzunlugu, zeminLayer);

        if (hit.collider != null)
        {
            // 1. ZEMİNİN DİREKT AÇISINI AL (Senin istediğin mantık)
            float zeminAcisi = hit.transform.eulerAngles.z;
            
            // Açıyı -180 ile 180 arasına çek (Unity bazen 350 verir, onu -10 yapmak lazım)
            if (zeminAcisi > 180) zeminAcisi -= 360;

            float finalAngle = zeminAcisi;
            bool sagaBakiyor = transform.localScale.x > 0;

            // 2. 4 YÖNLÜ OFFSET MANTIĞI
            if (sagaBakiyor)
            {
                // Sağa bakıyoruz
                if (zeminAcisi > 0.1f)      finalAngle += sagBakisYokusYukari; // Rampa /
                else if (zeminAcisi < -0.1f) finalAngle += sagBakisYokusAsagi;  // Rampa \
            }
            else
            {
                // Sola bakıyoruz (Scale -1)
                // DİKKAT: Unity Scale -1 olunca görsel rotasyon terslenir.
                // Bu yüzden zemin açısını tersine çeviriyoruz ki görsel doğru dursun.
                finalAngle = -zeminAcisi;

                // Sol için offsetler
                // Zemin açısı pozitifse (/) sola bakan için yokuş aşağıdır
                if (zeminAcisi > 0.1f)       finalAngle += solBakisYokusAsagi;
                // Zemin açısı negatifse (\) sola bakan için yokuş yukarıdır
                else if (zeminAcisi < -0.1f) finalAngle += solBakisYokusYukari;
            }
Debug.Log("aeav");
            // 3. Sadece GÖRSELİ döndür (Fiziği değil)
            Quaternion hedefRotasyon = Quaternion.Euler(0, 0, finalAngle);
            gorselObje.localRotation = Quaternion.Slerp(gorselObje.localRotation, hedefRotasyon, Time.deltaTime * donusHizi);
        }
    }
}
