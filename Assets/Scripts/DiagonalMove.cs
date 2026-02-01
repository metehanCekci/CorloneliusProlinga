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

    [Header("Zaman Bazlı Döngü (Opsiyonel)")]
    public bool useTimeLoop = false; // true ise belirli süre sonra reset
    public float loopDuration = 3f; // Kaç saniyede bir başa dönsün

    [Header("Raycast Ayarları")]
    public LayerMask groundLayer; // Raycast için zemin katmanı
    public float raycastDistance = 1.0f; // Raycast mesafesi

    private Vector3 startPosition;
    private float elapsedTime;
    private Transform currentRail;
    private Vector3 railDirection;

    void Start()
    {
        startPosition = transform.position;

        // Ray başlangıç noktasını bul
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            currentRail = hit.transform;
            railDirection = (currentRail.GetComponent<RailSystem>().endPoint.position - currentRail.GetComponent<RailSystem>().startPoint.position).normalized;
        }
    }

    void Update()
    {
        if (currentRail != null)
        {
            // Ray boyunca hareket
            transform.position += railDirection * moveSpeedX * Time.deltaTime;

            // Raycast ile rayın yüzeyine sabitleme
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
            {
                Vector3 newPosition = transform.position;
                newPosition.y = hit.point.y; // Y pozisyonunu rayın yüzeyine sabitle
                transform.position = newPosition;
            }
        }

        // Döngü kontrolü
        if (useLoop && transform.position.y >= limitY)
        {
            Vector3 newPos = transform.position;
            newPos.y = startY;
            newPos.x = startX;
            transform.position = newPos;
        }

        // 1. Manuel Hareket: Hem X'i hem Y'yi artırıyoruz (Sağ-Üst Çapraz)
        // transform.position += new Vector3(moveSpeedX, moveSpeedY, 0) * Time.deltaTime;

        // 1.1 Zaman Bazlı Döngü
        // if (useLoop && useTimeLoop)
        // {
        //     elapsedTime += Time.deltaTime;
        //     if (elapsedTime >= loopDuration)
        //     {
        //         transform.position = startPosition;
        //         elapsedTime = 0f;
        //     }
        //     return;
        // }
    }
}