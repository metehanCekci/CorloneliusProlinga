using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Ayarları")]
    [SerializeField] public Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Header("Zoom Ayarları")]
    [SerializeField] private float zoomLevel = 5f; // Orthographic size - düşük = yakın
    
    [Header("Sınırlar (Opsiyonel)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -10f, maxX = 10f;
    [SerializeField] private float minY = -5f, maxY = 5f;
    
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = zoomLevel;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 desiredPos = target.position + offset;
        
        if (useBounds)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
            desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        }
        
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        
        // Runtime'da zoom değişikliği için
        if (cam != null && cam.orthographic && cam.orthographicSize != zoomLevel)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomLevel, smoothSpeed * Time.deltaTime);
        }
    }
}
