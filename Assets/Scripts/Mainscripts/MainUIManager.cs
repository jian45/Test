using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MainUIManager : MonoBehaviour
{
    public CanvasGroup adoptCatGroup;
    public CanvasGroup mainGroup;
    public float fadeDuration = 0.5f;

    void Start()
    {
        adoptCatGroup.alpha = 1f;
        adoptCatGroup.interactable = true;
        adoptCatGroup.blocksRaycasts = true;

        mainGroup.alpha = 0f;
        mainGroup.interactable = false;
        mainGroup.blocksRaycasts = false;
    }

    public void GoToMain()
    {
        StartCoroutine(FadeToMain());
    }

    IEnumerator FadeToMain()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            adoptCatGroup.alpha = 1f - t;
            mainGroup.alpha = t;
            yield return null;
        }

        adoptCatGroup.alpha = 0f;
        adoptCatGroup.interactable = false;
        adoptCatGroup.blocksRaycasts = false;

        mainGroup.alpha = 1f;
        mainGroup.interactable = true;
        mainGroup.blocksRaycasts = true;
    }
}