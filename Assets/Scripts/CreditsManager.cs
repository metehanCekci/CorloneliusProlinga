using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI TeamText;

    [Header("Credits Content")]
    public string gameTitle = "Game Title";
    public List<string> Team;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;
    public float displayDuration = 2.0f;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        StartCoroutine(PlayCredits());
    }

    private IEnumerator PlayCredits()
    {
        // Show Game Title
        TitleText.text = gameTitle;
        yield return StartCoroutine(FadeInAndOut(TitleText));

        // Show Team Members
        foreach (string member in Team)
        {
            TeamText.text = member;
            yield return StartCoroutine(FadeInAndOut(TeamText));
        }
    }

    private IEnumerator FadeInAndOut(TextMeshProUGUI textElement)
    {
        textElement.gameObject.SetActive(true);

        // Fade In
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        for (float t = fadeDuration; t > 0; t -= Time.deltaTime)
        {
            canvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 0;

        textElement.gameObject.SetActive(false);
    }
}