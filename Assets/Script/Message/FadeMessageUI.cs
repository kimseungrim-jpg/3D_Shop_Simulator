using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 전달받은 안내 문구를 화면에 표시
/// 페이드 인 -> 메세지 유지 -> 페이드 아웃 순서로 재생
/// </summary>
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
    
    /// <summary>
    /// 새로운 메시지 표시 시퀀스를 시작
    /// </summary>
    public void ShowMessage(string message, System.Action onFullyVisible = null)
    {
        // 이전 메시지 연출이 남아 있으면 투명도 제어가 겹치니 중단하고 최신 요청만 재생 
        StopAllCoroutines();
        StartCoroutine(Sequence(message, onFullyVisible));
    }

    /// <summary>
    /// 메시지의 페이드 인, 완전 표시 콜백, 표시 유지, 페이드 아웃을 순서대로 처리
    /// ShowMessage가 새롱누 메시지 표시를 요청할 때 코루틴으로 실행
    /// </summary>
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
