using UnityEngine;

public class Sword : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float range;
    [HideInInspector] public ElementType elementType = ElementType.None;

    public LayerMask enemyLayer;

    // Kilica savurma efekti icin bilek kismi 
    public Transform swordPivot;

    // Savurma acilari 
    public Vector3 startAngle = new Vector3(-20f, 45f, 0f); // Dik, hafif yukari ve sagda
    public Vector3 endAngle = new Vector3(30f, -45f, 0f);  // Sol asagi capraza inmis

    public float swingDuration = 0.2f; // Savurma isleminin ne kadar surecegi
    private float swingTimer = 0f;

    void OnEnable()
    {
        swingTimer = 0f;
        PerformSlash();

        if (swordPivot != null)
        {
            // Kilici baslangic acisina (dik ve saga) al
            swordPivot.localRotation = Quaternion.Euler(startAngle);
        }

        Invoke(nameof(Deactivate), swingDuration);
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
        // Kilici belirlenen sure icinde baslangictan bitis acisina (sol asagi) yumusakca savur
        if (swordPivot != null)
        {
            swingTimer += Time.deltaTime;
            float fraction = swingTimer / swingDuration;

            // Slerp: Iki aci arasinda puruzsuz gecis yapar
            swordPivot.localRotation = Quaternion.Slerp(Quaternion.Euler(startAngle), Quaternion.Euler(endAngle), fraction);
        }
    }

    void PerformSlash()
    {
        // Cemberin icindeki tum dusmanlari bul
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, range, enemyLayer);

        bool hasSwordSweep = AbilityManager.Instance != null && AbilityManager.Instance.HasAbility(AbilityID.SwordSweep);

        if (hasSwordSweep)
        {
            // 100 derece koni içindeki tüm düşmanlara vur
            foreach (Collider enemy in hitEnemies)
            {
                Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToEnemy);

                if (angle <= 50f) // 50 derece sağ, 50 derece sol -> toplam 100
                {
                    ApplyDamage(enemy);
                }
            }
        }
        else
        {
            Collider closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            // Bulunanlar arasindan en yakin olani sec
            foreach (Collider enemy in hitEnemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            // Eger yakinda bir dusman varsa, sadece ona hasar ver
            if (closestEnemy != null)
            {
                ApplyDamage(closestEnemy);
            }
        }
    }

    void ApplyDamage(Collider enemy)
    {
        EnemyHealth eHealth = enemy.GetComponent<EnemyHealth>();
        if (eHealth != null)
        {
            // Hasar carpanini uygula
            float finalDamage = damage * PlayerStats.damageMultiplier;
            
            // Kritik vuruş kontrolü
            if (Random.value < PlayerStats.critChance)
            {
                finalDamage *= PlayerStats.critMultiplier;
            }

            eHealth.TakeDamage(finalDamage, transform.position);

            if (elementType != ElementType.None)
            {
                ElementEffect effect = enemy.GetComponent<ElementEffect>();
                if (effect == null) effect = enemy.gameObject.AddComponent<ElementEffect>();
                effect.Apply(elementType, finalDamage);
            }
        }
    }
}