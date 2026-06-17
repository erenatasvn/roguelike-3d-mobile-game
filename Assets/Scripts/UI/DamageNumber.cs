using UnityEngine;
using TMPro;
using System.Collections;


// DamageNumberSpawner.Spawn() tarafından tetiklenir 
public class DamageNumber : MonoBehaviour
{
    public float floatDistance  = 1.5f;  // Kaç birim yukarı yükselsin
    public float totalDuration  = 0.9f;  // Toplam animasyon süresi (sn)
    public float scaleInTime    = 0.15f; // Pop-in süresi (sn)
    public float sizeMultiplier = 0.10f; 

    private TextMeshPro tmp;

    public void Setup(float amount, bool isPlayerDamage = false, Color? customColor = null)
    {
        tmp = GetComponent<TextMeshPro>();
        if (tmp == null) { ObjectPooler.ReturnToPool(gameObject); return; }

        tmp.text = Mathf.RoundToInt(amount).ToString();
        
        // Alta uzamayı engelle (wrapping) ve ortala
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.alignment = TextAlignmentOptions.Center;

        // özel renk varsa onu kullan yoksa default kırmızı/beyaz
        if (customColor.HasValue)
        {
            tmp.color = customColor.Value;
        }
        else
        {
            tmp.color = isPlayerDamage
                ? new Color(1f, 0.25f, 0.25f, 1f)
                : new Color(1f, 1f,    1f,    1f);
        }

        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float elapsed  = 0f;
        Vector3 origin = transform.position;
        transform.localScale = Vector3.zero;

        while (elapsed < totalDuration)
        {
            // unscaledDeltaTime: Level-up ekranı gibi timeScale=0 anlarında da çalışsın
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / totalDuration;

            // Kameraya bak (billboard)
            if (Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;

            // Pop-in kısa sürede tam boyuta ulaş
            float st = Mathf.Clamp01(elapsed / scaleInTime);
            transform.localScale = (Vector3.one * sizeMultiplier) * Mathf.SmoothStep(0f, 1f, st);

            // Yukarı float
            transform.position = origin + Vector3.up * (floatDistance * t);

            // Son %35'te saydamlaş
            const float fadeStart = 0.65f;
            float alpha = t < fadeStart ? 1f : 1f - ((t - fadeStart) / (1f - fadeStart));
            tmp.alpha = Mathf.Clamp01(alpha);

            yield return null;
        }

        ObjectPooler.ReturnToPool(gameObject);
    }
}
