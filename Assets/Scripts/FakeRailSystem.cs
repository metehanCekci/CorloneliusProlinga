using UnityEngine;

/// <summary>
/// Bu script, bir trigger alanına giren karakterin eğimini yüzeyin normaline göre yumuşak bir şekilde ayarlar.
/// Karakterin hareketini kontrol etmez, sadece görsel olarak rotasyonunu düzenler.
/// Zıplama ve dash gibi yetenekler çalışmaya devam eder.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FakeRailSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Karakterin eğime ne kadar hızlı döneceği.")]
    public float rotationSpeed = 8f;

    [Tooltip("Rotasyon sıfırlanırken ne kadar hızlı döneceği.")]
    public float resetRotationSpeed = 5f;

    [Tooltip("Yüzeyi algılamak için kullanılacak layer.")]
    [SerializeField] private LayerMask groundLayer;

    // Alan içindeki aktif oyuncu referansları
    private ControllerScript playerController;
    private Transform playerTransform;
    private Gravity playerGravity;

    // Hedef rotasyon
    private Quaternion targetRotation;
    private bool isPlayerInZone = false;

    private void Awake()
    {
        // Collider'ın trigger olduğundan emin ol
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"'{gameObject.name}' üzerindeki Collider2D bir trigger olmalı. Otomatik olarak ayarlandı.", this);
            col.isTrigger = true;
        }

        // LayerMask ayarlanmadıysa varsayılan 'Ground' layer'ını ata
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Alana bir oyuncu mu girdi?
        if (other.TryGetComponent<ControllerScript>(out var controller))
        {
            // Collider kontrolü yap
            if (!GetComponent<Collider2D>().enabled)
            {
                Debug.Log($"{gameObject.name}: Player attempted to interact, but collider is disabled.");
                return;
            }

            playerController = controller;
            playerTransform = playerController.transform;
            playerGravity = playerController.GetComponent<Gravity>(); // Gravity component'ini de alalım
            isPlayerInZone = true;

            Debug.Log($"{gameObject.name}: Player entered rail zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Oyuncu alandan çıktı mı?
        if (other.GetComponent<ControllerScript>() == playerController)
        {
            isPlayerInZone = false;
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        if (isPlayerInZone)
        {
            HandleSlopeRotation();
        }
        else
        {
            // Oyuncu alandan çıktıysa ve hala referans duruyorsa, rotasyonu sıfırla
            ResetRotation();
            // Tamamen sıfırlandıysa referansı temizle
            if (Quaternion.Angle(playerTransform.rotation, targetRotation) < 0.1f)
            {
                playerController = null;
                playerTransform = null;
                playerGravity = null;
            }
        }
    }

    private void HandleSlopeRotation()
    {
        // Oyuncu referansı veya yerçekimi bileşeni yoksa çık
        if (playerController == null || playerGravity == null) return;

        // Raycast için başlangıç noktaları (karakterin alt kısmı boyunca)
        Vector2[] raycastOrigins = new Vector2[]
        {
            playerTransform.position + Vector3.up * 0.2f + Vector3.left * 0.2f,
            playerTransform.position + Vector3.up * 0.2f,
            playerTransform.position + Vector3.up * 0.2f + Vector3.right * 0.2f
        };

        foreach (var origin in raycastOrigins)
        {
            // Sadece 'groundLayer' üzerinde bir yüzey ara
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 1.5f, groundLayer);

            // Bir yüzeye temas ediyorsak ve karakter yerdeyse veya aşağı doğru hareket ediyorsa
            if (hit.collider != null && (playerController.IsGrounded() || playerGravity.GetVelocity().y <= 0))
            {
                // Yüzeyin normalini Vector2 olarak al
                Vector2 slopeNormal = hit.normal;

                // Yüzeyin açısını Vector2.up'a göre (-180, 180 aralığında) hesapla
                float targetZRotation = Vector2.SignedAngle(Vector2.up, slopeNormal);

                // Karakterin mevcut 'sağa/sola bakma' yönünü scale'den al
                float currentFacingDirection = Mathf.Sign(playerTransform.localScale.x);

                // Eğer karakter sola bakıyorsa (yani transform'u ters çevrilmişse),
                // görsel olarak doğru durması için açının da ters çevrilmesi gerekir.
                if (currentFacingDirection < 0)
                {
                    targetZRotation *= -1;
                }

                // Hedef rotasyonu oluştur
                targetRotation = Quaternion.Euler(0, 0, targetZRotation);

                // Yumuşak bir geçişle rotasyonu uygula
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                return; // İlk uygun raycast'te işlemi sonlandır
            }
        }

        // Eğer hiçbir raycast yüzey bulamazsa rotasyonu sıfırla
        ResetRotation();
    }

    private void ResetRotation()
    {
        if (playerTransform == null) return;

        // Sadece Z eksenindeki rotasyonu sıfırla, karakterin baktığı yönü koru
        targetRotation = Quaternion.Euler(0, playerTransform.eulerAngles.y, 0);
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime * resetRotationSpeed);
    }

    // Editörde trigger alanını ve raycast'i görselleştirmek için
    private void OnDrawGizmos()
    {
        if (isPlayerInZone && playerTransform != null)
        {
            Gizmos.color = Color.green;
            Vector2 raycastOrigin = playerTransform.position + Vector3.up * 0.2f;
            Gizmos.DrawLine(raycastOrigin, raycastOrigin + Vector2.down * 0.5f);
        }
    }
}
