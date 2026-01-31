using UnityEngine;

public class DeathLineScript : MonoBehaviour
{
    DeathScript death;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        death= FindObjectOfType<DeathScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(death.DeathSequence());
    }
}
