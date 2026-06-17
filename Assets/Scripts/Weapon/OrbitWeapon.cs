using UnityEngine;

// Oyuncunun etrafında dönen çember silahları için.
public class OrbitWeapon : MonoBehaviour
{
    [HideInInspector] public ElementType elementType;

    public float orbitRadius = 2f;
    public float baseDamage = 10f;
    public float baseOrbitSpeed = 90f;   // derece/sn
    public float damageCooldown = 0.5f;

    private Transform target;
    private float angle = 0f;
    private float cooldownTimer = 0f;

    // AbilityManager tarafından set edilir: eşit aralıklı spawn için başlangıç açısı
    [HideInInspector] public float startAngle = 0f;

    public void Initialize(Transform playerTransform)
    {
        target = playerTransform;
        angle  = startAngle; // eşit dağılım için başlangıç açısını ata
    }

    void Update()
    {
        if (target == null) return;

        // Zehir Çemberi dönüş hızı saldırı hızıyla orantılı büyüt
        float speedMult = (elementType == ElementType.Poison)
            ? PlayerStats.attackSpeedMultiplier
            : 1f;

        angle += baseOrbitSpeed * speedMult * Time.deltaTime;

        // Karakterin etrafında yörüngede dön
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
        transform.position = target.position + offset;

        // Hasar cooldown
        cooldownTimer -= Time.deltaTime;
    }

    // Düşmana değince hasar ver ve element efekti uygula
    void OnTriggerStay(Collider other)
    {
        if (cooldownTimer > 0f) return;
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth eh = other.GetComponent<EnemyHealth>();
        if (eh == null) return;

        float dmg = baseDamage * PlayerStats.damageMultiplier;
        eh.TakeDamage(dmg, transform.position);

    
        if (elementType != ElementType.None)
        {
            ElementEffect effect = other.GetComponent<ElementEffect>();
            if (effect == null) effect = other.gameObject.AddComponent<ElementEffect>();
            effect.Apply(elementType, dmg, 2f); // Circle temas edince 2 sn element hasarı
        }

        cooldownTimer = damageCooldown;
    }
}
