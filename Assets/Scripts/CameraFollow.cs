using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Ayarları")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);
    
    [Header("Sınırlar (Opsiyonel)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -10f, maxX = 10f;
    [SerializeField] private float minY = -5f, maxY = 5f;

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
    }
}
