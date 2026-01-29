using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathScript : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private Transform initialRespawnPoint;

    [Header("Ayarlar")]
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float waitAtBlack = 0.5f;

    [Header("Death Detection")]
    [SerializeField] private LayerMask deathLayer;
    [SerializeField] private Vector2 deathCheckSize = new Vector2(0.4f, 0.8f);

    private Vector3 currentRespawnPosition;
    private ControllerScript controller;
    private Gravity gravity;
    private bool isDead = false;

    void Start()
    {
        controller = GetComponent<ControllerScript>();
        gravity = GetComponent<Gravity>();

        if (initialRespawnPoint != null)
            currentRespawnPosition = initialRespawnPoint.position;
        else
            currentRespawnPosition = transform.position;

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
    }

    void Update()
    {
        CheckDeathCollision();
    }

    private void CheckDeathCollision()
    {
        if (isDead) return;

        Collider2D deathHit = Physics2D.OverlapBox(
            transform.position,
            deathCheckSize,
            0f,
            deathLayer
        );

        if (deathHit != null)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Death") && !isDead)
        {
            StartCoroutine(DeathSequence());
        }

        if (other.CompareTag("Checkpoint"))
        {
            UpdateCheckpoint(other.transform.position);
        }
    }

    private void UpdateCheckpoint(Vector3 newPos)
    {
        if (currentRespawnPosition != newPos)
        {
            currentRespawnPosition = newPos;
            Debug.Log("Checkpoint Alindi! Konum: " + newPos);
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        controller.enabled = false;
        gravity.SetVelocity(Vector2.zero);

        while (fadeImage.color.a < 1)
        {
            Color c = fadeImage.color;
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(waitAtBlack);
        transform.position = currentRespawnPosition;
        gravity.SetVelocity(Vector2.zero);

        while (fadeImage.color.a > 0)
        {
            Color c = fadeImage.color;
            c.a -= Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        controller.enabled = true;
        isDead = false;
    }
}
