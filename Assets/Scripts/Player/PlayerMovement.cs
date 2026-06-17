using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Karakterin temel hareket ve fizik baglantilari
    public Joystick joystick;
    public float moveSpeed = 7f;
    public Rigidbody rb;

    // Silah yonetimi ve saldiri kontrolu
    public WeaponManager weaponManager;

    // Auto-aim hassasiyeti
    public float aimRotationSpeed = 8f;
    public float shootAngleThreshold = 5f;

    // Animasyon
    private Animator anim;
    private PlayerHealth playerHealth;

    // Dusman listesi 0.5sn'de bir guncellenir
    private GameObject[] cachedEnemies = new GameObject[0];
    private float enemyCacheTimer = 0f;
    private const float ENEMY_CACHE_INTERVAL = 0.5f;

    // Inspector'dan zorunlu referanslar atanmissa true 
    private bool isReady = false;

    void Start()
    {
        if (joystick == null)
        {
            Debug.LogError("PlayerMovement: 'joystick' atanmamis", this);
            return;
        }
        if (weaponManager == null)
        {
            Debug.LogError("PlayerMovement: 'weaponManager' atanmamis", this);
            return;
        }

        anim = GetComponentInChildren<Animator>();
        playerHealth = GetComponent<PlayerHealth>();

        isReady = true;
    }

    void FixedUpdate()
    {
        if (!isReady) return;

        // Canımız yoksa fiziksel hareketi de durdur
        if (PlayerHealth.currentHealth <= 0)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            MovePlayer(direction);
            if (anim != null) anim.SetFloat("Speed", rb.linearVelocity.magnitude); // Kosma animasyonunu tetikle
        }
        else
        {
            rb.linearVelocity = Vector3.zero; // Durduğunda kaymayı engelle
            if (anim != null) anim.SetFloat("Speed", 0f); // Durma animasyonuna gec
            AutoAimAndShoot();
        }
    }

    void MovePlayer(Vector3 direction)
    {
        // velocity ile duvardan gecme engellenir
        rb.linearVelocity = direction * moveSpeed;

        // Hareket yonune dogru donusu uygula
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime));
    }

    void AutoAimAndShoot()
    {
        // Listeyi belirli araliklarla tazele
        if (Time.time >= enemyCacheTimer)
        {
            cachedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            enemyCacheTimer = Time.time + ENEMY_CACHE_INTERVAL;
        }

        GameObject nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in cachedEnemies)
        {
            // Liste guncellenene kadar yok olan dusmanlari atla
            if (enemy == null) continue;

            // Düşman ölüyse hedef alma 
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null && eh.isDead) continue;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            // Dusmana dogru donus rotasyonunu hesapla
            Vector3 lookDirection = nearestEnemy.transform.position - transform.position;
            lookDirection.y = 0;

            // Dusman karakterin tam icine girdiyse yon hesaplamasinin sifir olup oyunu bozmasin
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

                // Hesaplananan rotasyona gecis yap
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, aimRotationSpeed * Time.fixedDeltaTime));

                // Karakterin bakis yonu ile hedef arasindaki aciyi hesapla
                float angleToEnemy = Vector3.Angle(transform.forward, lookDirection);

                // Aci uygunsa VEYA dusman 2 birimden yakinsa aciya bakmadan direkt vur
                // Aci uygunsa silah yoneticisine ates etme emri ver
                if (angleToEnemy <= shootAngleThreshold || shortestDistance <= 2f)
                {
                    if (weaponManager.TryAttack())
                    {
                        if (anim != null) anim.SetTrigger("Attack");
                    }
                }
            }
        }
    }
}