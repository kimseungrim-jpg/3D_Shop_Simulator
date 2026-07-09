using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering;

public class FadeMessageUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI messageText;

    public float fadeDuration = 1f;
    public float showDuration = 3f;

    PlayerMovement playerMove;
    PlayerInteract playerInteract;

    void Start()
    {
        playerMove = FindAnyObjectByType<PlayerMovement>();
        playerInteract = FindAnyObjectByType<PlayerInteract>();

        canvasGroup.alpha = 0f;
    }
    
    public void ShowMessage(string message, System.Action onFullyVisible = null)
    {
        StopAllCoroutines();
        StartCoroutine(Sequence(message, onFullyVisible));
    }

    IEnumerator Sequence(string message, System.Action onFullyVisible)
    {
        messageText.text = message;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        onFullyVisible?.Invoke();

        yield return new WaitForSeconds(showDuration);

        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
