using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animasyon Ayarları")]
    public float pressedScale = 0.9f; // Butona basıldığında küçüleceği oran
    public float animationSpeed = 15f; // Büyüme/küçülme hızı

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * pressedScale));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            // Time.unscaledDeltaTime kullanıyoruz ki oyun Pause olsa bile UI animasyonları çalışsın
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
