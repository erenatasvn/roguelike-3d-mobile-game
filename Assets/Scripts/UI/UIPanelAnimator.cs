using UnityEngine;
using System.Collections;

public class UIPanelAnimator : MonoBehaviour
{
    [Header("Panel Açılış Animasyonu")]
    public float animationDuration = 0.2f;
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f);
    public bool animateOnEnable = true;

    private Coroutine scaleCoroutine;

    void OnEnable()
    {
        if (animateOnEnable)
        {
            PlayOpenAnimation();
        }
    }

    public void PlayOpenAnimation()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleUp());
    }

    private IEnumerator ScaleUp()
    {
        transform.localScale = startScale;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Time.unscaledDeltaTime kullanıyoruz ki oyun Pause olsa bile UI çalışsın
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / animationDuration;
            
            // Basit bir ease-out efekti (Hızlı başlar yavaş biter)
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            
            transform.localScale = Vector3.Lerp(startScale, Vector3.one, t);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    public void ClosePanel(GameObject panelToClose)
    {
        StartCoroutine(ScaleDownAndClose(panelToClose));
    }

    private IEnumerator ScaleDownAndClose(GameObject panelToClose)
    {
        float elapsedTime = 0f;
        Vector3 currentScale = transform.localScale;

        while (elapsedTime < animationDuration / 2f) // Kapanış daha hızlı olsun
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / (animationDuration / 2f);
            
            transform.localScale = Vector3.Lerp(currentScale, startScale, t);
            yield return null;
        }

        panelToClose.SetActive(false);
    }
}
