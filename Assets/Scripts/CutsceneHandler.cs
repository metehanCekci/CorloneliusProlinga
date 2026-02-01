using UnityEngine;

public class CutsceneHandler : MonoBehaviour
{
    public GameObject playerSprite;
    public CameraFollow myCam;

    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerSprite.SetActive(true);
            collision.gameObject.SetActive(false);
            myCam.target = playerSprite.transform;
            animator.enabled = true;
        }
    }
}
