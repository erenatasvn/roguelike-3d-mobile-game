using UnityEngine;
using System.Collections;

public class StaffProjectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public ElementType elementType = ElementType.None;

    // Piercing: kaç düşmandan geçebileceği 
    [HideInInspector] public int piercesLeft = 0;
    // Ricochet: kaç kez sekebileceği
    [HideInInspector] public int ricochetLeft = 0;
    // Her sekme/geçişte hasar katları
    [HideInInspector] public float damageMult = 1f;

    public float lifeTime = 3f;

    private bool isRicochetMoving = false; // ricochet animasyonu oynarken Update'i blokla
    private Collider lastHitCollider;      // ricochet'te kendine tekrar çarpmasın

    void OnEnable()
    {
        damageMult = 1f;
        isRicochetMoving = false;
        lastHitCollider = null;
        piercesLeft = 0;
        ricochetLeft = 0;

        if (AbilityManager.Instance != null)
        {
            if (AbilityManager.Instance.HasAbility(AbilityID.Piercing))
                piercesLeft = 2;
            if (AbilityManager.Instance.HasAbility(AbilityID.Ricochet))
                ricochetLeft = 2;
        }

        Invoke(nameof(Deactivate), lifeTime);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Deactivate()
    {
        ObjectPooler.ReturnToPool(gameObject);
    }

    void Update()
    {
        if (isRicochetMoving) return;
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // Solid (isTrigger=false) duvarlara çarparsa da dur
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Deactivate();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == lastHitCollider) return;

        if (other.CompareTag("Obstacle"))
        {
            Deactivate();
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            lastHitCollider = other;
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                float finalDamage = damage * damageMult * PlayerStats.damageMultiplier;

                // Kritik vuruş
                if (Random.value < PlayerStats.critChance)
                    finalDamage *= PlayerStats.critMultiplier;

                eh.TakeDamage(finalDamage, transform.position);

                // Element efekti uygula
                if (elementType != ElementType.None)
                {
                    ElementEffect effect = other.GetComponent<ElementEffect>();
                    if (effect == null) effect = other.gameObject.AddComponent<ElementEffect>();
                    effect.Apply(elementType, finalDamage);
                }
            }

            // Ricochet (Sekme) kontrolü
            if (ricochetLeft > 0)
            {
                ricochetLeft--;
                damageMult *= 0.70f; // her sekmede %30 hasar düşer
                EnemyHealth nextTarget = FindNearestOtherEnemy(other.gameObject);
                if (nextTarget != null)
                {
                    StopAllCoroutines(); // Önceki sekme hareketini durdur 
                    StartCoroutine(RicochetTo(nextTarget.transform));
                    return;
                }
            }

            // Ricochet yoksa Piercing (Delip Geçme) kontrolü
            if (piercesLeft > 0)
            {
                piercesLeft--;
                damageMult *= 0.67f; // her geçişte %33 hasar düşer
                return; // oku yok etme, dümdüz devam et
            }

            Deactivate();
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("Wall"))
        {
            Deactivate();
        }
    }

    // Ricochet için en yakın başka düşmanı bul
    EnemyHealth FindNearestOtherEnemy(GameObject exclude)
    {
        EnemyHealth[] all = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        EnemyHealth nearest = null;
        float minDist = float.MaxValue;
        foreach (EnemyHealth e in all)
        {
            if (e.gameObject == exclude) continue;
            if (e.isDead) continue; // Ölü cesetlere sekmesini engelle
            float d = Vector3.Distance(transform.position, e.transform.position);
            // Sadece 15 metre çapındaki yakındaki düşmanlara seksin
            if (d < minDist && d < 15f) { minDist = d; nearest = e; }
        }
        return nearest;
    }

    // Oku ricochet hedefine doğru yönlendir
    IEnumerator RicochetTo(Transform target)
    {
        isRicochetMoving = true;
        while (target != null && Vector3.Distance(transform.position, target.position) > 0.3f)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            yield return null;
        }
        isRicochetMoving = false;
    }
}