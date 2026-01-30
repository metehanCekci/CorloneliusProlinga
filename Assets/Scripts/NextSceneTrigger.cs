using UnityEngine;

public class NextSceneTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FadeScript fadeScript = FindObjectOfType<FadeScript>();
            if (fadeScript != null)
            {
                fadeScript.SiradakiSahne();
            }
            else
            {
                Debug.LogError("FadeScript bulunamadý!");
            }
        }
    }
}
