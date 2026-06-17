using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 20f;
    private float baseDamage; // temel hasar

    public float attackRate = 1f;
    private float nextAttackTime = 0f;

    // dusmanin vuracagi maks mesafe
    public float attackRange = 1.5f;
    public float damageDelay = 0f; 

    private Transform player;
    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        baseDamage = damage;
        int room = GameManager.currentRoom;

        // hasar odaya gore ustel artiyor (%4)
        damage = baseDamage * Mathf.Pow(1.04f, room - 1);

        // eger boss/elit odasiysa dusmanin vuruslari %50 daha guclu olur
        if (room % 10 == 0)
        {
            damage *= 1.5f;
        }

        // Oyuncuyu bul ve kaydet
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Eger oyuncu yoksa bir sey yapma
        if (player == null) return;

        // Oyuncuyla aramizdaki mesafeyi olc
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Eger oyuncu saldiri menzilimize girdiyse ve saldiri suremiz geldiyse
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                if (anim != null) anim.SetTrigger("Attack");
                nextAttackTime = Time.time + attackRate;

                if (damageDelay > 0f)
                {
                    StartCoroutine(DealDamageWithDelay(playerHealth, damage, damageDelay));
                }
                else
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    // Unity ekraninda saldiri menzilini gormek icin
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }


    // Fiziksel olarak değdikleri an kesinlikle hasar vermeleri
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= nextAttackTime)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (anim != null) anim.SetTrigger("Attack");
                nextAttackTime = Time.time + attackRate;

                if (damageDelay > 0f)
                {
                    StartCoroutine(DealDamageWithDelay(playerHealth, damage, damageDelay));
                }
                else
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    private System.Collections.IEnumerator DealDamageWithDelay(PlayerHealth pHealth, float dmg, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Hasarı vermeden önce oyuncunun menzilde olup olmadığını tekrar ölç
        if (pHealth != null && player != null)
        {
            float currentDistance = Vector3.Distance(transform.position, player.position);
            
            if (currentDistance <= attackRange + 1.5f)
            {
                pHealth.TakeDamage(dmg);
            }
        }
    }
}