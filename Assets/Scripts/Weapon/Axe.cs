using UnityEngine;

public class Axe : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public float maxDistance;
    [HideInInspector] public Transform player;
    [HideInInspector] public float halfDistance;
    [HideInInspector] public WeaponManager weaponManager;
    [HideInInspector] public ElementType elementType = ElementType.None;
    [HideInInspector] public bool isSecondAxe = false; // Multishot'ta ikinci balta

    [Header("Mesafe Bazli Hasar")]
    // Yakin dusmanlara (halfDistance) verilecek hasar orani %60
    public float closeRangeDamageMultiplier = 0.6f;

    // Balta gidip donerken donecek
    public Transform visualModel;
    public float spinSpeed = 1500f; // Havada donme hizi

    private Vector3 startPosition;
    private bool isReturning = false; // donup donmedigini kontrol

    void OnEnable()
    {
        startPosition = transform.position; // Firlatildigi ilk noktayi kaydet
        isReturning = false;
    }

    void OnDisable()
    {
        if (isSecondAxe)
            weaponManager?.ClearActiveAxe2();
        else
            weaponManager?.ClearActiveAxe();
    }

    void Update()
    {
        // Baltanin sadece gorselini kendi etrafinda dondur (Bumerang efekti)
        if (visualModel != null)
        {
            visualModel.Rotate(Vector3.right * spinSpeed * Time.deltaTime);
        }

        if (!isReturning)
        {
            // Ileri dogru git
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // Gidecegi maksimum mesafeye ulastiysa geri don
            if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
            {
                isReturning = true;
            }
        }
        else
        {
            // Oyuncuya dogru geri don
            if (player != null)
            {
                Vector3 returnDirection = (player.position - transform.position).normalized;
                // Balta Y ekseni sabit
                returnDirection.y = 0;

                transform.position += returnDirection * speed * Time.deltaTime;

                // Balta karakterimize yeterince yaklastiysa yok et
                if (Vector3.Distance(transform.position, player.position) < 1.5f)
                {
                    ObjectPooler.ReturnToPool(gameObject);
                }
            }
            else
            {
                ObjectPooler.ReturnToPool(gameObject); // Eger oyuncu sahnede yoksa baltayi yok et
            }
        }
    }

    // Solid (isTrigger=false) duvarlara çarparsa da geri dön
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            isReturning = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            isReturning = true;
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            float distanceFromStart = Vector3.Distance(startPosition, transform.position);
            float rangeMod = (distanceFromStart < halfDistance) ? closeRangeDamageMultiplier : 1f;

            float finalDamage = damage * PlayerStats.damageMultiplier * rangeMod;

            // Kritik vuruş
            if (Random.value < PlayerStats.critChance)
                finalDamage *= PlayerStats.critMultiplier;

            other.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage, transform.position);

            // Element efekti
            if (elementType != ElementType.None)
            {
                ElementEffect effect = other.GetComponent<ElementEffect>();
                if (effect == null) effect = other.gameObject.AddComponent<ElementEffect>();
                effect.Apply(elementType, finalDamage);
            }
        }
    }
}