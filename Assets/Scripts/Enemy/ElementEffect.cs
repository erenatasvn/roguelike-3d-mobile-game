using UnityEngine;
using System.Collections;

// element varsa bu script çalışı
// Blaze/Poison sürekli hasar (DoT). Freeze yavaşlatma. Bolt zincir. Dark gecikmeli AoE.
public class ElementEffect : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private UnityEngine.AI.NavMeshAgent agent;

    // Freeze orijinal hız
    private float originalSpeed;

    // Aktif DoT efektleri
    private System.Collections.Generic.Dictionary<ElementType, GameObject> activeVFX = new System.Collections.Generic.Dictionary<ElementType, GameObject>();

    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) originalSpeed = agent.speed;
    } 

    // Dışarıdan çağrılır.DoT'un süresi (ok = 4s, circle = 2s)
    public void Apply(ElementType type, float baseDamage, float dotDuration = 4f)
    {
        int stack = AbilityManager.Instance != null ? AbilityManager.Instance.GetElementStack(type) : 1;

        switch (type)
        {
            case ElementType.Blaze:
                PlayOrRefreshVFX(type, dotDuration);
                StartCoroutine(DoBlazeDoT(baseDamage, dotDuration, stack)); 
                break;
            case ElementType.Poison:
                PlayOrRefreshVFX(type, dotDuration);
                StartCoroutine(DoPoisonDoT(baseDamage, dotDuration, stack)); 
                break;
            case ElementType.Freeze:
                float freezeDuration = 0.3f;
                if (stack == 2) freezeDuration = 0.5f;
                if (stack >= 3) freezeDuration = 0.7f;
                PlayOrRefreshVFX(type, freezeDuration);
                StartCoroutine(DoFreeze(freezeDuration)); 
                break;
            case ElementType.OBSOLETE_Bolt:
                break;
            case ElementType.Dark:    
                StartCoroutine(DoDarkTouch(baseDamage, stack)); 
                break;
        }
    }

    void PlayOrRefreshVFX(ElementType type, float duration)
    {
        // Eski efekt varsa sil
        if (activeVFX.ContainsKey(type) && activeVFX[type] != null)
        {
            ObjectPooler.ReturnToPool(activeVFX[type]);
        }

        // Yeni efekti başlat
        if (VFXManager.Instance != null)
        {
            GameObject vfx = VFXManager.Instance.PlayDoTVFX(type, transform);
            if (vfx != null)
            {
                activeVFX[type] = vfx;
                StartCoroutine(ReturnVFXAfter(vfx, duration));
            }
        }
    }

    IEnumerator ReturnVFXAfter(GameObject vfx, float duration)
    {
        yield return new WaitForSeconds(duration);
        ObjectPooler.ReturnToPool(vfx);
    }

    // Ateş: dotDuration  boyunca her 0.5sn'de bir hasar
    IEnumerator DoBlazeDoT(float baseDamage, float duration, int stack)
    {
        float interval = 0.5f;
        // Stack 1: %8, Stack 2: %12, Stack 3: %16
        float multiplier = 0.04f + (stack * 0.04f); 
        float tickDamage = baseDamage * multiplier;
        float elapsed = 0f;
        
        while (elapsed < duration && enemyHealth != null)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;
            enemyHealth?.TakeDamage(tickDamage, transform.position, false);
        }
    }

    // Zehir: dotDuration  boyunca her 1sn'de bir hasar
    IEnumerator DoPoisonDoT(float baseDamage, float duration, int stack)
    {
        float interval = 1f;
        // Stack 1: %15, Stack 2: %25, Stack 3: %35
        float multiplier = 0.05f + (stack * 0.10f); 
        float tickDamage = baseDamage * multiplier;
        float elapsed = 0f;
        
        while (elapsed < duration && enemyHealth != null)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;
            enemyHealth?.TakeDamage(tickDamage, transform.position, false);
        }
    }

    // Buz: Belirlenen süre kadar dondurur 
    IEnumerator DoFreeze(float duration)
    {
        if (agent != null) agent.speed = 0f;
        yield return new WaitForSeconds(duration);
        if (agent != null && agent.isActiveAndEnabled) agent.speed = originalSpeed;
    }

    // Karanlık Dokunuş: 1 saniye sonra AoE patlama
    IEnumerator DoDarkTouch(float baseDamage, int stack)
    {
        yield return new WaitForSeconds(1f);
        if (enemyHealth == null) yield break;

        // Patlama efekti
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayInstantVFX(ElementType.Dark, transform.position);
        }

        float aoeRange = 3.5f;
        
        // Stack 1: 1.0x, Stack 2: 1.5x, Stack 3: 2.0x
        float multiplier = 0.5f + (stack * 0.5f);
        float aoeDamage = baseDamage * multiplier;
        
        Collider[] nearby = Physics.OverlapSphere(transform.position, aoeRange);
        foreach (Collider col in nearby)
        {
            EnemyHealth eh = col.GetComponent<EnemyHealth>();
            eh?.TakeDamage(aoeDamage, transform.position, false);
        }
    }
}
